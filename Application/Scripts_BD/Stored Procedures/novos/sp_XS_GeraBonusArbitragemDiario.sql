--EXEC sp_XS_GeraBonusArbitragemDiario '20190701'

IF OBJECT_ID('dbo.sp_XS_GeraBonusArbitragemDiario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_GeraBonusArbitragemDiario];
GO

CREATE PROCEDURE sp_XS_GeraBonusArbitragemDiario (@dataRef varchar(8))
AS

	BEGIN TRY

		DECLARE @CategoriaBonificacaoID INT = 21
		DECLARE @percentualBonusGeral FLOAT;
		DECLARE @percentualBonus1 FLOAT,@percentualBonus2 FLOAT, @percentualBonus3 FLOAT, @percentualBonus4 FLOAT, @percentualBonus5 FLOAT;
		DECLARE @percentualBonus6 FLOAT,@percentualBonus7 FLOAT, @percentualBonus8 FLOAT, @percentualBonus9 FLOAT, @percentualBonus10 FLOAT;
	
		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;

		SET @dataInicio = CAST(@dataRef + ' 00:00:00' as datetime2);
		SET @dataFim = CAST(@dataRef + ' 23:59:59' as datetime2);

		/*
		declare @idCiclo int
		set @idCiclo = (select ID from Rede.Ciclo where @dataFim between DataInicial and DataFinal)
		*/

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
			NivelAssociacao int,
			valorTotalPacotes float,
			valorBonus float,
			valorTeto float,
			valorPago float
		)
		CREATE TABLE #TBonus 
		( 
			BonificacaoId	INT
		); 

		set @percentualBonusGeral = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID is null and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)

		set @percentualBonus1 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 2 and DataReferencia between @dataInicio and @dataFim 	order by DataReferencia desc)
		set @percentualBonus2 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 3 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus3 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 4 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus4 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 5 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus5 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 6 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus6 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 7 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus7 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 8 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus8 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 9 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus9 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 10 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc)
		set @percentualBonus10 = (select top 1 Valor from Rede.ConfiguracaoBonusDiario where AssociacaoID = 11 and DataReferencia between @dataInicio and @dataFim 	 order by DataReferencia desc);

		if @percentualBonus1 is null begin	set @percentualBonus1 =  @percentualBonusGeral end
		if @percentualBonus2 is null begin	set @percentualBonus2 =  @percentualBonusGeral end
		if @percentualBonus3 is null begin	set @percentualBonus3 =  @percentualBonusGeral end
		if @percentualBonus4 is null begin	set @percentualBonus4 =  @percentualBonusGeral end
		if @percentualBonus5 is null begin	set @percentualBonus5 =  @percentualBonusGeral end
		if @percentualBonus6 is null begin	set @percentualBonus6 =  @percentualBonusGeral end
		if @percentualBonus7 is null begin	set @percentualBonus7 =  @percentualBonusGeral end
		if @percentualBonus8 is null begin	set @percentualBonus8 =  @percentualBonusGeral end
		if @percentualBonus9 is null begin	set @percentualBonus9 =  @percentualBonusGeral end
		if @percentualBonus10 is null begin	set @percentualBonus10 =  @percentualBonusGeral end
		
		
		insert into #tempDiario (usuarioID, NivelAssociacao, valorPago)
		select ID, NivelAssociacao, 0 as ValorPago from Usuario.Usuario
		where StatusID = 2 And NivelAssociacao > 0
		and RecebeBonus = 1
		and Bloqueado = 0;
		
		update #tempDiario set valorTotalPacotes = dbo.fn_ObtemReferenciaGanhoArbitragem (T.usuarioID) from #tempDiario T;
		
		update #tempDiario set valorTeto = dbo.fn_ObtemBonusTetoGanhoArbitragem (T.usuarioID) from #tempDiario T;

		
		update #tempDiario set valorPago = isnull(F.Valor,0)
		from #tempDiario T
		inner join (
			select usuarioID, sum(Valor) as Valor from Financeiro.Lancamento 
			where CategoriaID = @CategoriaBonificacaoID
			group by usuarioID
		) F on T.usuarioID = F.UsuarioID;
		
		
		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus1) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 1;
		
		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus2) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 2;

		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus3) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 3;
		
		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus4) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 4;

		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus5) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 5;		

		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus6) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 6;		

		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus7) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 7;
		
		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus8) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 8;
		
		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus9) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 9;

		update #tempDiario set valorBonus = (valorTotalPacotes * @percentualBonus10) / 100
		from #tempDiario where valorPago < valorTeto and NivelAssociacao = 10;
		
		
		select *, ValorBonus * 10 from #tempDiario
		
		/*calcula bonus*/
		INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor, ValorBTC)
		OUTPUT inserted.ID INTO #TBonus
		select @CategoriaBonificacaoID, usuarioID, 0, 0, @dataRef, valorBonus, valorBonus
		from #tempDiario
		where valorPago + valorBonus < valorTeto

		
		
			/*casas decimais de BTC*/		
	
	
		DECLARE C_BTC CURSOR FOR
			SELECT BonificacaoId, RB.ValorBTC FROM #TBonus T inner join Rede.Bonificacao RB on T.BonificacaoId = RB.ID

		DECLARE @BonificacaoID INT, @valorBrutoBTC float, @valorLiquidoPagamentoBTC float

		OPEN C_BTC

		FETCH NEXT FROM C_BTC INTO @BonificacaoID, @valorBrutoBTC
		
		WHILE(@@FETCH_STATUS = 0)
		BEGIN
			exec sp_XS_BitcoinSafeDecimais @valorBrutoBTC, null, @BonificacaoID, @valorLiquidoPagamentoBTC output
			select @valorBrutoBTC
			select @valorLiquidoPagamentoBTC
			update Rede.Bonificacao set ValorBTC = @valorLiquidoPagamentoBTC where ID = @BonificacaoID

			FETCH NEXT FROM C_BTC INTO @BonificacaoID, @valorBrutoBTC
		END

		CLOSE C_BTC
		DEALLOCATE C_BTC

		
		drop table #tempDiario
		drop table #TBonus 
		
		insert into Rede.BonificacaoExecucao values (@CategoriaBonificacaoID, cast(@dataFim as date), getdate())

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_XS_GeraBonusArbitragemDiario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
