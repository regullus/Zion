--EXEC sp_JB_GeraBonusAlavancagemDiario '20190701'

IF OBJECT_ID('dbo.sp_JB_GeraBonusAlavancagemDiario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_GeraBonusAlavancagemDiario];
GO

CREATE PROCEDURE sp_JB_GeraBonusAlavancagemDiario (@dataRef varchar(8))
AS

	BEGIN TRY

		DECLARE @CategoriaBonificacaoID INT = 22
		DECLARE @percentualBonus FLOAT;
	
		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;

		SET @dataInicio = CAST(@dataRef + ' 00:00:00' as datetime2);
		SET @dataFim = CAST(@dataRef + ' 23:59:59' as datetime2);

		declare @idCiclo int
		set @idCiclo = (select ID from Rede.Ciclo where @dataFim between DataInicial and DataFinal)

		IF(EXISTS(SELECT 1 FROM Rede.BonificacaoExecucao where CategoriaBonusID = @CategoriaBonificacaoID and DataExecucao = cast(@dataFim as date)))
		BEGIN
			print 'este bonus ja foi executado para este dia. Altere a data do processamento, ou remova a trava em Rede.BonificacaoExecucao'
			RETURN
		END
		
		IF (DATEPART(WEEKDAY, @dataInicio) = 1 or DATEPART(WEEKDAY, @dataInicio) = 7)
		BEGIN
			print 'este bonus não pode ser executado em finais de semana. Só é gerado para dias da semana.'
			RETURN
		END


		BEGIN TRANSACTION
		
		create table #tempDiario (
			usuarioID int,
			valorTotalPacotes float,
			valorBonus float,
			valorTeto float,
			valorPago float
		)

		set @percentualBonus = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where DataReferencia <= getdate() order by DataReferencia desc)
		
		insert into #tempDiario (usuarioID, valorPago)
		select ID, 0 as ValorPago from Usuario.Usuario
		where StatusID = 2 And NivelAssociacao > 0
		and RecebeBonus = 1
		and Bloqueado = 0
		and DataRenovacao >= getdate()
		
		update #tempDiario set valorTeto = dbo.fn_ObtemBonusTetoGanhoAlavancagem (T.usuarioID) from #tempDiario T


		update #tempDiario set valorPago = isnull(F.Valor,0)
		from #tempDiario T
		inner join (
			select usuarioID, sum(Valor) as Valor from Financeiro.Lancamento 
			where CategoriaID = @CategoriaBonificacaoID
			group by usuarioID
		) F on T.usuarioID = F.UsuarioID

		
		update #tempDiario
		set valorBonus = (PC.ValorPacote * @percentualBonus) / 100,
		valorTotalPacotes = PC.ValorPacote
		from #tempDiario inner join (
			SELECT UA.UsuarioID, sum(POV.Bonificacao) as ValorPacote FROM Usuario.usuarioAssociacao UA 
			inner join Loja.Produto P on UA.NivelAssociacao = P.NivelAssociacao	
			inner join Loja.ProdutoValor POV ON P.ID = POV.ProdutoID
			where P.SKU like 'ADE%'
			group by UA.UsuarioID		
			) 
		PC on #tempDiario.usuarioID = PC.UsuarioID
		
		

		
		/*calcula bonus*/
		INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
		select @CategoriaBonificacaoID, usuarioID, 0, 0, @dataRef, valorBonus
		from #tempDiario
		where valorPago + valorBonus < valorTeto

		
		
		
		drop table #tempDiario
		
		insert into Rede.BonificacaoExecucao values (@CategoriaBonificacaoID, cast(@dataFim as date), getdate())

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_JB_GeraBonusAlavancagemDiario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
