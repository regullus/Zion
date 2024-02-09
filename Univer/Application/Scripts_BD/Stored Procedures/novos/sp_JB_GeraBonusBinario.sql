--EXEC sp_JB_GeraBonusBinario '20190701', '20190701'

IF OBJECT_ID('dbo.sp_JB_GeraBonusBinario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_GeraBonusBinario];
GO

CREATE PROCEDURE sp_JB_GeraBonusBinario (@baseDateIni varchar(8), @baseDateFim varchar(8))
AS

	BEGIN TRY

		DECLARE @RegraID_Binario INT = 3
		DECLARE @CategoriaBonificacaoID INT = 25
		DECLARE @valorBonus float;
		DECLARE @faixaPagamentoBonus float;
		DECLARE @percentualBonus FLOAT = 8; /*COLOCAR EM TABELA DE CONFIGURACAO*/
	
		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;

		SET @dataInicio = CAST(@baseDateIni + ' 00:00:00' as datetime2);
		SET @dataFim = CAST(@baseDateFim + ' 23:59:59' as datetime2);

		declare @idCiclo int
		set @idCiclo = (select ID from Rede.Ciclo where @dataFim between DataInicial and DataFinal)

		IF(EXISTS(SELECT 1 FROM Rede.BonificacaoExecucao where CategoriaBonusID = @CategoriaBonificacaoID and DataExecucao = cast(@dataFim as date)))
		BEGIN
			print 'este bonus ja foi executado para este dia. Altere a data do processamento, ou remova a trava em Rede.BonificacaoExecucao'
			RETURN
		END


		BEGIN TRANSACTION


		/*1 - pedidos do dia*/
		CREATE TABLE #tempPedidos
		(
			usuarioID int,
			pedidoID int,
			pontos float
		)
		
		INSERT INTO #tempPedidos
		SELECT U.ID as UsuarioID, P.ID as PedidoID,
		SUM(PI.BonificacaoUnitaria) AS pontos
		FROM Loja.Pedido P
			INNER JOIN Usuario.Usuario U ON P.UsuarioID = U.ID
			INNER JOIN Loja.PedidoItem PI ON P.ID = PI.PedidoID
			INNER JOIN Loja.Produto Prod ON PI.ProdutoID = Prod.ID
			INNER JOIN Loja.ProdutoValor PV ON PV.ProdutoID = Prod.ID
			INNER JOIN Loja.PedidoPagamento PPAG ON P.ID = PPAG.PedidoID
			INNER JOIN Loja.PedidoPagamentoStatus PPAGST ON PPAG.ID = PPAGST.PedidoPagamentoID
			WHERE PPAGST.Data BETWEEN @dataInicio AND @dataFim 
			AND U.GeraBonus = 1
			AND PPAGST.StatusID = 3 /*PAGO */
			AND Prod.TipoID IN (1,2) /*"PRODUTOS" NAO DEVEM IR PRO CALCULO DO BINARIO. APENAS Associacoa/Upgrade.*/
		GROUP BY U.ID, U.Assinatura, P.ID
		
		/*2  - constroi cadeia de rede acima*/

		CREATE TABLE #tempPedidosCadeia
		(
			usuarioIDPedido int,
			pedidoIDReferencia int,
			assinaturaReferencia varchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
			assinaturaCadeia varchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
			LenAssinaturaCadeia int,
			Lado varchar(1) null,
			usuarioIDCadeia int null,
			
		)

		CREATE TABLE #tempPontos
		(
			usuarioID int,
			PontosEsquerda float,
			PontosDireita float,
			receberBonus bit default 0,
			qualificado bit default 0,
			gap float,
			tetoBonus float
		)

		insert into #tempPedidosCadeia
		select usuarioID, pedidoID, assinatura, assinatura, LEN(assinatura) - 1, null as lado, null as usuarioIDcadeia from #tempPedidos T inner join Usuario.Usuario U on U.ID =  T.usuarioID

		/*com base em um pedido, monta toda a arvore acima, de quem terá o binario alterado, até o nivel 0*/
		WHILE (exists( select PedidoIDReferencia, count(pedidoIDReferencia), max(LenAssinaturaCadeia) from #tempPedidosCadeia group by PedidoIDReferencia having count(pedidoIDReferencia) < max(LenAssinaturaCadeia) ))
		BEGIN
			INSERT INTO #tempPedidosCadeia
			select usuarioIDPedido, pedidoIDReferencia, assinaturaReferencia, LEFT(assinaturaReferencia, min(LenAssinaturaCadeia)), min(LenAssinaturaCadeia)-1, null, null
			from #tempPedidosCadeia group by usuarioIDPedido, PedidoIDReferencia, assinaturaReferencia having count(pedidoIDReferencia) < max(LenAssinaturaCadeia)
		END

		UPDATE #tempPedidosCadeia
		set usuarioIDCadeia = U.ID
		from #tempPedidosCadeia P inner join Usuario.Usuario U on P.assinaturaCadeia = U.Assinatura

		update #tempPedidosCadeia set Lado = 'E' where SUBSTRING ( assinaturaReferencia, LenAssinaturaCadeia + 2 , 1) = '0'
		update #tempPedidosCadeia set Lado = 'D' where SUBSTRING ( assinaturaReferencia, LenAssinaturaCadeia + 2 , 1) = '1'

		
		/* resgata pontuacao acumulada*/
		insert into #tempPontos
		select TC.usuarioIDCadeia, sum(isnull(UB.PontosEsquerda,0)), sum(isnull(UB.PontosDireita,0)), 0, 0, 0, 0
		from #tempPedidosCadeia TC
		left join (
			select usuarioID, max(DataReferencia) as Data from Usuario.PontosBinario group by usuarioID
		) UltimaPosicao on TC.usuarioIDCadeia = UltimaPosicao.UsuarioID 
		left join Usuario.PontosBinario UB on UltimaPosicao.UsuarioID = UB.usuarioID and UltimaPosicao.Data = UB.DataReferencia
		group by TC.usuarioIDCadeia


		/*ativo mensal*/
		update #tempPontos
		set receberBonus = 1
		from #tempPontos TP inner join Usuario.AtivacaoMensal ATV on TP.usuarioID = ATV.usuarioID
		inner join Usuario.Usuario U on TP.usuarioID  = U.ID 
		where CicloID = @idCiclo
		and U.StatusID = 2 
		And U.NivelAssociacao > 0
		and U.RecebeBonus = 1
		and U.Bloqueado = 0
		and U.DataRenovacao >= getdate()
		

		/*qualificador*/
		update #tempPontos
		set qualificado = 1
		from #tempPontos TP inner join Usuario.Qualificacao Q on TP.usuarioID = Q.usuarioID
		where Q.QualificadorEsquerdaID is not null
		and Q.QualificadorDireitaID is not null


		/*calculo de pontuacao/perna menor / bonus*/

		update #tempPontos
		set PontosEsquerda = PontosEsquerda + PontosE.pontos
		from #tempPontos P
		inner join (
			select usuarioIDCadeia, sum(pontos) as pontos from #tempPedidosCadeia TC inner join #tempPedidos TP on TC.pedidoIDReferencia = TP.pedidoID
			where usuarioIDPedido <> usuarioIDCadeia 
			and Lado = 'E'
			group by usuarioIDCadeia
		) PontosE
		on P.usuarioID = PontosE.usuarioIDCadeia

		update #tempPontos
		set PontosDireita = PontosDireita + PontosD.pontos
		from #tempPontos P
		inner join (
			select usuarioIDCadeia, sum(pontos) as pontos from #tempPedidosCadeia TC inner join #tempPedidos TP on TC.pedidoIDReferencia = TP.pedidoID
			where usuarioIDPedido <> usuarioIDCadeia 
			and Lado = 'D'
			group by usuarioIDCadeia
		) PontosD
		on P.usuarioID = PontosD.usuarioIDCadeia


		update #tempPontos 
		set gap = case when PontosEsquerda >= PontosDireita then PontosDireita Else PontosEsquerda end

		/*teto*/
		update #tempPontos set tetoBonus = [dbo].[fn_ObtemBonusTetoGanhoBinario](T.usuarioID) from #tempPontos T

		select * from #tempPontos
		
		/*calcula bonus*/
		INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
		SELECT @CategoriaBonificacaoID as Categoria, usuarioID, 0, 0, @dataFim, 
		case 
			when ((gap * @percentualBonus) / 100) > tetoBonus 
			then tetoBonus 
			else ((gap * @percentualBonus) / 100)
		end as valor
		FROM #tempPontos
		WHERE receberBonus = 1
		and qualificado = 1
		AND gap > 0


		/* atualiza pontuação */
		insert into Usuario.PontosBinarioLinha (UsuarioID, PedidoIDOrigem, Lado, Pontos, DataReferencia, DataCriacao)
		select usuarioIDCadeia, pedidoID, 
		case when Lado = 'D' then 1 else 0 end as lado, 
		pontos, @dataInicio, getdate() 
		from #tempPedidosCadeia TC inner join #tempPedidos T on T.usuarioID = TC.usuarioIDPedido 
		where usuarioIDPedido <> usuarioIDCadeia 

		insert into Rede.Posicao (UsuarioID, DataInicio, DataFim, ValorDiaPernaEsquerda, ValorDiaPernaDireita, 
		AcumuladoEsquerda, AcumuladoDireita, 
		DataCriacao)
		select usuarioID, @dataInicio, @dataFim, (PontosEsquerda-Gap), (PontosDireita-Gap), 
		(PontosEsquerda-Gap), (PontosDireita-Gap), getdate()
		from #tempPontos

		insert into Usuario.PontosBinario (UsuarioID, PontosEsquerda, PontosDireita, DataReferencia, DataCriacao)
		select usuarioID, (PontosEsquerda-Gap), (PontosDireita-Gap), @dataFim, getdate()
		from #tempPontos


		drop table #tempPedidos
		drop table #tempPedidosCadeia
		drop table #tempPontos

		
		insert into Rede.BonificacaoExecucao values (@CategoriaBonificacaoID, cast(@dataFim as date), getdate())

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_BonusBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
