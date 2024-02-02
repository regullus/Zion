--EXEC sp_XS_GeraBonusBinario '20190701', '20190701'

IF OBJECT_ID('dbo.sp_XS_GeraBonusBinario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_GeraBonusBinario];
GO

CREATE PROCEDURE sp_XS_GeraBonusBinario (@baseDateIni varchar(8), @baseDateFim varchar(8))
AS

	BEGIN TRY

		DECLARE @CategoriaBonificacaoID INT = 14
		DECLARE @valorBonus float;
		DECLARE @faixaPagamentoBonus float;
		DECLARE @percentualBonus FLOAT = 10; /*COLOCAR EM TABELA DE CONFIGURACAO*/
	
		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;
		
		/*TODO: ler de tabela de cotação, a cotação BTC do dia*/
		DECLARE @cotacaoBTCDia float 

		DECLARE @moedaBTC int = 6
		DECLARE @moedaUSD int = 3
		DECLARE @moedaTipoSaida int = 3

		exec sp_XS_Cotacao @moedaBTC, @moedaUSD, @moedaTipoSaida,  @cotacaoBTCDia OUT

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
			pontos float,
			valorBTC float,
			cotacaoBTC float
		)

		INSERT INTO #tempPedidos
		SELECT U.ID as UsuarioID, P.ID as PedidoID,
		SUM(PI.BonificacaoUnitaria) AS pontos,
		PPAG.ValorBTC,
		PPAG.CotacaoBTC
		FROM Loja.Pedido P
			INNER JOIN Usuario.Usuario U ON P.UsuarioID = U.ID
			INNER JOIN Loja.PedidoItem PI ON P.ID = PI.PedidoID
			INNER JOIN Loja.Produto Prod ON PI.ProdutoID = Prod.ID
			INNER JOIN Loja.ProdutoValor PV ON PV.ProdutoID = Prod.ID
			INNER JOIN Loja.PedidoPagamento PPAG ON P.ID = PPAG.PedidoID
			INNER JOIN Loja.PedidoPagamentoStatus PPAGST ON PPAG.ID = PPAGST.PedidoPagamentoID
			WHERE PPAGST.Data BETWEEN @dataInicio AND @dataFim 
			AND U.GeraBonus = 1
			AND PPAGST.StatusID = 3 /* PAGO */
			AND Prod.TipoID IN (1,2) /* "PRODUTOS" NAO DEVEM IR PRO CALCULO DO BINARIO. APENAS Associacoa/Upgrade. */
			AND PPAG.ValorBTC <= PPAGST.ValorPago
		GROUP BY U.ID, P.ID, PPAG.ValorBTC, PPAG.CotacaoBTC

		/*2  - constrói cadeia de rede acima */

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
			BTCEsquerda float,
			BTCDireita float,
			receberBonus bit default 0,
			qualificado bit default 0,
			gap float  default 0,
			tetoBonus float  default 0,
			tetoBTC float  default 0,
			BTCEsquerdaDia float  default 0,
			BTCDireitaDia float  default 0
		)

		insert into #tempPedidosCadeia
		select usuarioID, pedidoID, UA.Assinatura, UA.Assinatura, LEN(UA.Assinatura) - 1, null as lado, null as usuarioIDcadeia 
		from #tempPedidos T 
		inner join Usuario.Usuario U on U.ID =  T.usuarioID
		inner join UsuarioAssinatura UA on U.ID =  UA.ID

		/*com base em um pedido, monta toda a arvore acima, de quem terá o binario alterado, até o nivel 0*/
		WHILE (exists( select PedidoIDReferencia, count(pedidoIDReferencia), max(LenAssinaturaCadeia) from #tempPedidosCadeia group by PedidoIDReferencia having count(pedidoIDReferencia) < max(LenAssinaturaCadeia) ))
		BEGIN
			INSERT INTO #tempPedidosCadeia
			select usuarioIDPedido, pedidoIDReferencia, assinaturaReferencia, LEFT(assinaturaReferencia, min(LenAssinaturaCadeia)), 
			min(LenAssinaturaCadeia)-1, null, null
			from #tempPedidosCadeia group by usuarioIDPedido, PedidoIDReferencia, assinaturaReferencia 
			having count(pedidoIDReferencia) < max(LenAssinaturaCadeia)
		END

		UPDATE #tempPedidosCadeia
		set usuarioIDCadeia = UA.ID
		from #tempPedidosCadeia P inner join UsuarioAssinatura UA on P.assinaturaCadeia = UA.Assinatura

		update #tempPedidosCadeia set Lado = 'E' where SUBSTRING ( assinaturaReferencia, LenAssinaturaCadeia + 2 , 1) = '0'
		update #tempPedidosCadeia set Lado = 'D' where SUBSTRING ( assinaturaReferencia, LenAssinaturaCadeia + 2 , 1) = '1'

		select * from #tempPedidosCadeia
		
		/* resgata pontuação acumulada*/
		declare @dataReferenciaMax datetime
		set @dataReferenciaMax  = (select max(DataInicio) from Rede.Posicao)

		insert into #tempPontos
		select TC.usuarioIDCadeia, sum(isnull(UltimaPosicao.AcumuladoEsquerda,0)), sum(isnull(UltimaPosicao.AcumuladoDireita,0)), 0, 0, 0, 0, 0,0,0
		from #tempPedidosCadeia TC
		left join Rede.Posicao UltimaPosicao on TC.usuarioIDCadeia = UltimaPosicao.usuarioID and UltimaPosicao.DataInicio = @dataReferenciaMax
		group by TC.usuarioIDCadeia

		/*ativo mensal*/
		/*
		update #tempPontos
		set receberBonus = 1
		from #tempPontos TP 
		inner join Usuario.Usuario U on TP.usuarioID  = U.ID 
		where U.StatusID = 2 
		And U.NivelAssociacao > 0
		and U.RecebeBonus = 1
		and U.Bloqueado = 0
		and U.DataRenovacao >= getdate()
		*/

		/*qualificador*/
		update #tempPontos
		set qualificado = 1
		from #tempPontos TP inner join Usuario.Qualificacao Q on TP.usuarioID = Q.usuarioID
		where Q.QualificadorEsquerdaID is not null
		and Q.QualificadorDireitaID is not null

		/*TODO: tratar renovação */


		/*cálculo de pontuacao/perna menor / bonus*/

		update #tempPontos
		set BTCEsquerda = BTCEsquerda + PontosE.btc,
		BTCEsquerdaDia = PontosE.btc
		from #tempPontos P
		inner join (
			select usuarioIDCadeia, sum(TP.valorBTC) as btc 
			from #tempPedidosCadeia TC inner join #tempPedidos TP on TC.pedidoIDReferencia = TP.pedidoID
			where usuarioIDPedido <> usuarioIDCadeia 
			and Lado = 'E'
			group by usuarioIDCadeia
		) PontosE
		on P.usuarioID = PontosE.usuarioIDCadeia

		update #tempPontos
		set BTCDireita = BTCDireita + PontosD.btc,
		BTCDireitaDia = PontosD.btc
		from #tempPontos P
		inner join (
			select usuarioIDCadeia, sum(TP.valorBTC) as btc 
			from #tempPedidosCadeia TC inner join #tempPedidos TP on TC.pedidoIDReferencia = TP.pedidoID
			where usuarioIDPedido <> usuarioIDCadeia 
			and Lado = 'D'
			group by usuarioIDCadeia
		) PontosD
		on P.usuarioID = PontosD.usuarioIDCadeia


		update #tempPontos
		set gap = case when BTCEsquerda >= BTCDireita then BTCDireita Else BTCEsquerda end

		/*teto*/
		update #tempPontos set tetoBonus = [dbo].[fn_ObtemBonusTetoGanhoBinario](T.usuarioID) from #tempPontos T
		update #tempPontos set tetoBTC = tetoBonus / @cotacaoBTCDia

		update #tempPontos set tetoBTC = tetoBonus / @cotacaoBTCDia
		
		select * from #tempPontos

		/*calcula bonus*/
		INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor, ValorBTC)
		SELECT @CategoriaBonificacaoID as Categoria, usuarioID, 0, 0, @dataFim, 
		case 
			when ((gap * @percentualBonus) / 100) > tetoBTC
			then tetoBTC 
			else ((gap * @percentualBonus) / 100)
		end as valor,
		case 
			when ((gap * @percentualBonus) / 100) > tetoBTC
			then tetoBTC 
			else ((gap * @percentualBonus) / 100)
		end as valorBTC	
		FROM #tempPontos
		WHERE receberBonus = 1
		and qualificado = 1
		AND gap > 0


		/* atualiza pontuação */
		insert into Rede.PontosBinarioLinha (UsuarioID, PedidoIDOrigem, Lado, Pontos, ValorBTC, DataReferencia, DataCriacao)
		select usuarioIDCadeia, pedidoID, 
		case when Lado = 'D' then 1 else 0 end as lado, 
		pontos, 
		T.valorBTC,
		@dataInicio, getdate() 
		from #tempPedidosCadeia TC inner join #tempPedidos T on T.usuarioID = TC.usuarioIDPedido 
		where usuarioIDPedido <> usuarioIDCadeia 

		insert into Rede.Posicao (UsuarioID, DataInicio, DataFim, ValorDiaPernaEsquerda, ValorDiaPernaDireita, 
		AcumuladoEsquerda, AcumuladoDireita, 
		DataCriacao)
		select usuarioID, @dataInicio, @dataFim, BTCEsquerdaDia, BTCDireitaDia, 
		case when BTCEsquerda-Gap >= 0 then (BTCEsquerda-Gap) else 0 end, 
		case when BTCDireita-Gap >= 0 then (BTCDireita-Gap) else 0 end, 
		getdate()
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
		RAISERROR ('Erro na execucao de sp_XS_GeraBonusBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
