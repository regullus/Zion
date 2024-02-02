--EXEC sp_BonusBinario '20131125', '20131125'

IF OBJECT_ID('dbo.sp_JB_GeraBonusBinario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_GeraBonusBinario];
GO

CREATE PROCEDURE sp_JB_GeraBonusBinario (@baseDateIni varchar(8), @baseDateFim varchar(8))
AS

	BEGIN TRY

		DECLARE @RegraID_Binario INT = 3
		DECLARE @CategoriaBonificacaoID INT = 25
	
		BEGIN TRANSACTION

		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;

		SET @dataInicio = CAST(@baseDateIni + ' 00:00:00' as datetime2);
		SET @dataFim = CAST(@baseDateFim + ' 23:59:59' as datetime2);

		DECLARE @valorBonus float;
		DECLARE @faixaPagamentoBonus float;

		DECLARE @valorAfiliacao1 float;
		DECLARE @valorAfiliacao2 float;
		DECLARE @valorAfiliacao3 float;
		DECLARE @valorAfiliacao4 float;

		SET @valorAfiliacao1 = (SELECT Valor FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
		SET @valorAfiliacao2 = (SELECT Valor FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
		SET @valorAfiliacao3 = (SELECT Valor FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
		SET @valorAfiliacao4 = (SELECT Valor FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);

		DECLARE @limiteBinario1 float;
		DECLARE @limiteBinario2 float;
		DECLARE @limiteBinario3 float;
		DECLARE @limiteBinario4 float;

		DECLARE @percentualBonus FLOAT;

		SET @percentualBonus = 8; /*COLOCAR EM TABELA DE CONFIGURACAO*/
		
		CREATE TABLE #tempUsuariosBinario
		(
			idUsuario int,
			pontosPernaEsquerda float,
			pontosPernaDireita float,
			valorBonus float,
			pontosAcumuladoPernaEsquerda float,
			pontosAcumuladoPernaDireita float,
			valorTotalPernaEsquerda float,
			valorTotalPernaDireita float,
			valorAfiliacaoEsquerda float,
			valorAfiliacaoDireita float,
			pontosLiquidosPernaEsquerda float,
			pontosLiquidosPernaDireita float
		)

		INSERT INTO #tempUsuariosBinario
		SELECT ID, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0
		FROM Usuario.Usuario


		CREATE TABLE #tempUsuariosQualificados
		(
			idUsuario int,
			idUsuarioEsquerdo int, 
			idUsuarioDireito int,
			valorAfiliacaoDireito float,
			valorAfiliacaoEsquerdo float,
			dataPedidoAfiliacaoEsquerdo datetime,
			dataPedidoAfiliacaoDireito datetime
		)
			
		INSERT INTO #tempUsuariosQualificados
		SELECT Usuario.ID, MIN(ESQUERDOS.ID) AS PRIMEIROESQUERDO, MIN(DIREITOS.ID) AS PRIMEIRODIREITO, 0, 0, null, null
		FROM Usuario.Usuario Usuario 
		INNER JOIN Usuario.Usuario ESQUERDOS ON ESQUERDOS.PatrocinadorDiretoID = Usuario.ID
		INNER JOIN Usuario.Usuario DIREITOS ON DIREITOS.PatrocinadorDiretoID = Usuario.ID
		WHERE ESQUERDOS.Assinatura LIKE Usuario.Assinatura + '0%'
		AND DIREITOS.Assinatura LIKE Usuario.Assinatura + '1%'
		AND Usuario.StatusID = 2 /*ativos e pagos*/
		AND ESQUERDOS.StatusID = 2 
		AND DIREITOS.StatusID = 2
		AND Usuario.NivelAssociacao > 0
		AND ESQUERDOS.NivelAssociacao > 0
		AND DIREITOS.NivelAssociacao > 0
		AND Usuario.RecebeBonus = 1
		AND ESQUERDOS.GeraBonus = 1
		AND DIREITOS.GeraBonus = 1
		GROUP BY Usuario.ID 

		UPDATE #tempUsuariosQualificados
		SET valorAfiliacaoDireito = 
		(
			CASE
				WHEN DIREITO.NivelAssociacao = 1 THEN (@valorAfiliacao1)
				WHEN DIREITO.NivelAssociacao = 2 THEN (@valorAfiliacao2)
				WHEN DIREITO.NivelAssociacao = 3 THEN (@valorAfiliacao3)
				WHEN DIREITO.NivelAssociacao = 4 THEN (@valorAfiliacao4)
				ELSE 0
			END
		),
		valorAfiliacaoEsquerdo = 
		(
			CASE
				WHEN ESQUERDO.NivelAssociacao = 0 THEN (@valorAfiliacao1)
				WHEN ESQUERDO.NivelAssociacao = 1 THEN (@valorAfiliacao2)
				WHEN ESQUERDO.NivelAssociacao = 2 THEN (@valorAfiliacao3)
				WHEN ESQUERDO.NivelAssociacao = 3 THEN (@valorAfiliacao4)
				ELSE 0
			END
		)
		FROM #tempUsuariosQualificados 
		INNER JOIN Usuario.Usuario ESQUERDO ON #tempUsuariosQualificados.idUsuarioEsquerdo = ESQUERDO.ID
		INNER JOIN  Usuario.Usuario DIREITO ON #tempUsuariosQualificados.idUsuarioDireito = DIREITO.ID

		
		UPDATE #tempUsuariosQualificados
		SET dataPedidoAfiliacaoEsquerdo = Associacao.Data
		FROM #tempUsuariosQualificados 
		INNER JOIN Usuario.UsuarioAssociacao Associacao ON #tempUsuariosQualificados.idUsuarioEsquerdo = Associacao.UsuarioID
			AND Associacao.Data BETWEEN @dataInicio AND @dataFim
			AND Associacao.NivelAssociacao = 0

		UPDATE #tempUsuariosQualificados
		SET dataPedidoAfiliacaoDireito = Associacao.Data
		FROM #tempUsuariosQualificados 
		INNER JOIN Usuario.UsuarioAssociacao Associacao ON #tempUsuariosQualificados.idUsuarioDireito = Associacao.UsuarioID
			AND Associacao.Data BETWEEN @dataInicio AND @dataFim
			AND Associacao.NivelAssociacao = 0

		
		CREATE TABLE #tempPedidos
		(
			idUsuario int,
			assinaturaRede varchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
			pontuacaoPedidos float
		)
		
		INSERT INTO #tempPedidos
		SELECT U.ID, Assinatura,
		SUM( PV.Bonificacao ) AS pontos
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
		GROUP BY U.ID, Assinatura
		
		
		UPDATE #tempUsuariosBinario 
			SET pontosPernaEsquerda = (
				SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
				WHERE #tempPedidos.assinaturaRede LIKE #tempUsuariosBinario.assinaturaRede + '0%' 
			)
		FROM #tempUsuariosBinario
		INNER JOIN #tempUsuariosQualificados  ON #tempUsuariosBinario.idUsuario = #tempUsuariosQualificados.idUsuario
		
		
		UPDATE #tempUsuariosBinario 
		SET pontosPernaDireita = (
				SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
				WHERE #tempPedidos.assinaturaRede LIKE #tempUsuariosBinario.assinaturaRede + '1%' 
			)
		FROM #tempUsuariosBinario
		INNER JOIN #tempUsuariosQualificados  ON #tempUsuariosBinario.idUsuario = #tempUsuariosQualificados.idUsuario
		

		UPDATE #tempUsuariosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		UPDATE #tempUsuariosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempUsuariosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempUsuariosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		
		select * from #tempUsuariosBinario 
		
		
		/*subtrai do total, os que foram ativados neste período*/
		/*
		UPDATE #tempUsuariosBinario 
		SET ValorAfiliacaoEsquerda = #tempUsuariosQualificados.valorAfiliacaoEsquerdo
		FROM #tempUsuariosBinario
		INNER JOIN #tempUsuariosQualificados ON #tempUsuariosBinario.idUsuario = #tempUsuariosQualificados.idUsuario
		WHERE #tempUsuariosQualificados.dataPedidoAfiliacaoEsquerdo BETWEEN @dataInicio AND @dataFim
		
		UPDATE #tempUsuariosBinario 
		SET ValorAfiliacaoDireita = #tempUsuariosQualificados.valorAfiliacaoDireito
		FROM #tempUsuariosBinario
		INNER JOIN #tempUsuariosQualificados ON #tempUsuariosBinario.idUsuario = #tempUsuariosQualificados.idUsuario
		WHERE #tempUsuariosQualificados.dataPedidoAfiliacaoDireito BETWEEN @dataInicio AND @dataFim

		select * from #tempUsuariosBinario 
		*/
		
		UPDATE #tempUsuariosBinario SET  pontosLiquidosPernaEsquerda = pontosPernaEsquerda
		UPDATE #tempUsuariosBinario SET  pontosLiquidosPernaDireita = pontosPernaDireita

		/*Adiciona Acumulado na conta*/
		UPDATE #tempUsuariosBinario 
		SET 
			pontosPernaEsquerda = pontosPernaEsquerda + (CASE WHEN (RP.ValorPernaEsquerda >= RP.ValorPernaDireita) THEN RP.ValorPernaEsquerda - RP.ValorPernaDireita ELSE 0 END ),
			pontosPernaDireita = pontosPernaDireita + (CASE WHEN (RP.ValorPernaDireita > RP.ValorPernaEsquerda) THEN RP.ValorPernaDireita - RP.ValorPernaEsquerda  ELSE 0 END),
			pontosAcumuladoPernaEsquerda = RP.AcumuladoEsquerda,
			pontosAcumuladoPernadireita = RP.AcumuladoDireita
			
		FROM #tempUsuariosBinario
		INNER JOIN Rede.Posicao RP ON #tempUsuariosBinario.idUsuario = RP.UsuarioID
		WHERE RP.DataFim =
		(
			SELECT max(DataFim) FROM Rede.Posicao
		)

		select * from #tempUsuariosBinario 

		UPDATE #tempUsuariosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempUsuariosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		UPDATE #tempUsuariosBinario SET valorAfiliacaoEsquerda = 0 WHERE valorAfiliacaoEsquerda IS NULL
		UPDATE #tempUsuariosBinario SET valorAfiliacaoDireita = 0 WHERE valorAfiliacaoDireita IS NULL
		UPDATE #tempUsuariosBinario SET pontosAcumuladoPernaDireita = 0 WHERE pontosAcumuladoPernaDireita IS NULL
		UPDATE #tempUsuariosBinario SET pontosAcumuladoPernaEsquerda = 0 WHERE pontosAcumuladoPernaEsquerda IS NULL
		
				
		/*PONTOS LIQUIDOS,ANTES DE CALCULAR O BONUS FINAL*/
		/*
		UPDATE #tempUsuariosBinario 
		SET pontosLiquidosPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita - valorAfiliacaoDireita,
		pontosLiquidosPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda - valorAfiliacaoEsquerda
		*/

		UPDATE #tempUsuariosBinario 
		SET valorBonus = 
		(
		CASE 
			WHEN pontosPernaDireita > pontosPernaEsquerda
			THEN pontosPernaEsquerda
			ELSE pontosPernaDireita
		END /*MINIMO DOS DOIS VALORES.*/
		) * (@percentualBonus / 100)

	
		select * from #tempUsuariosBinario 

		UPDATE #tempUsuariosBinario SET valorBonus = 0 WHERE valorBonus IS NULL

		/*Acumulado*/
		--UPDATE #tempUsuariosBinario SET pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda  
		--UPDATE #tempUsuariosBinario SET pontosAcumuladoPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita 

		
		select * from #tempUsuariosBinario 
		
		/*TEMPORARIO!*/
		
		/*deduz o valor concedido em bonus, da perna maior */
		/*
		UPDATE #tempUsuariosBinario 
		SET pontosLiquidosPernaDireita = pontosLiquidosPernaDireita - valorBonus,
		pontosPernaDireita = pontosPernaDireita - valorBonus
		WHERE pontosLiquidosPernaDireita > pontosLiquidosPernaEsquerda
		
		UPDATE #tempUsuariosBinario 
		SET pontosLiquidosPernaEsquerda = pontosLiquidosPernaEsquerda - valorBonus,
		pontosPernaEsquerda = pontosPernaEsquerda - valorBonus
		WHERE  pontosLiquidosPernaEsquerda >= pontosLiquidosPernaDireita
		
		UPDATE #tempUsuariosBinario SET pontosLiquidosPernaEsquerda = 0 where pontosLiquidosPernaEsquerda < 0
		UPDATE #tempUsuariosBinario SET pontosPernaEsquerda = 0 where pontosPernaEsquerda < 0
		 
		UPDATE #tempUsuariosBinario SET pontosLiquidosPernaDireita = 0 where pontosLiquidosPernaDireita < 0
		UPDATE #tempUsuariosBinario SET pontosPernaDireita = 0 where pontosPernaDireita < 0
		*/
		
		/*LIMITE DIARIO DE BONUS DE REDE*/
		UPDATE #tempUsuariosBinario  SET valorBonus = @limiteBinario1 WHERE valorBonus > @limiteBinario1 AND perfil = 1
		UPDATE #tempUsuariosBinario  SET valorBonus = @limiteBinario2 WHERE valorBonus > @limiteBinario2 AND perfil = 2
		UPDATE #tempUsuariosBinario  SET valorBonus = @limiteBinario3 WHERE valorBonus > @limiteBinario3 AND perfil = 3
		UPDATE #tempUsuariosBinario  SET valorBonus = @limiteBinario4 WHERE valorBonus > @limiteBinario4 AND perfil = 4
		
		/*
		delete from #tempUsuariosBinario where idUsuario <> 1736
		delete from #tempUsuariosQualificados where idUsuario <> 1736
		*/
		
			UPDATE #tempUsuariosBinario SET 
			pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda,
			pontosAcumuladoPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita

			/*8 - INSERE NA TABELA DE PONTUACAO, FRANQUEADOS QUE NAO ESTEJAM LÁ*/
			INSERT INTO Rede.Posicao (UsuarioID, ReferenciaID, DataInicio, DataFim, 
			ValorPernaEsquerda, ValorPernaDireita,
			AcumuladoEsquerda, AcumuladoDireita, DataCriacao)
			SELECT #tempUsuariosBinario.idUsuario, 0 as Referencia,  @dataInicio,  @dataFim, 
			#tempUsuariosBinario.pontosPernaEsquerda, #tempUsuariosBinario.pontosPernaDireita, (ISNULL(AcumuladoEsquerda, 0) + #tempUsuariosBinario.pontosLiquidosPernaEsquerda), (ISNULL(AcumuladoDireita, 0) + #tempUsuariosBinario.pontosLiquidosPernaDireita), 
			GETDATE()
			FROM #tempUsuariosBinario
			LEFT JOIN Rede.Posicao RP ON #tempUsuariosBinario.idUsuario = RP.UsuarioID and RP.DataFim =
			(
				SELECT max(DataFim) FROM Rede.Posicao
			)
			
			
			/*UMA VEZ QUE PONTUOU A TODOS, ACUMULADOS, APAGA QUEM NÃO ESTÁ QUALIFICADO/NÃO PAGOU ATIVO MENSAL */
			/*ANTES DE PAGAR O BONUS*/
			
			/*1- REMOVE QUEM NÃO PAGOU ATIVO MENSAL*/
			/*
			DELETE FROM #tempUsuariosQualificados
			WHERE idUsuario COLLATE SQL_Latin1_General_CP1_CI_AS IN (
				SELECT A1_COD FROM SA1000 
				WHERE A1_XDTQUAL = '' 
				OR DATEADD(day,30, CAST(A1_XDTQUAL AS datetime)) < GETDATE()
			)
			*/
			/*2 - REMOVE AFILIADOS QUE AINDA NÃO ESTÃO QUALIFICADOS*/
			DELETE FROM #tempUsuariosBinario
			WHERE idUsuario NOT IN (
				SELECT idUsuario FROM #tempUsuariosQualificados
			)
			
			/*DINHEIRO*/
			INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
			SELECT '3' as Categoria, idUsuario, 0, 0, @dataFim, valorBonus 
			FROM #tempUsuariosBinario INNER JOIN vw_UsuariosAtivos ON #tempUsuariosBinario.idUsuario = vw_UsuariosAtivos.ID
			WHERE valorBonus > 0
			
		drop table #tempPedidos
		drop table #tempUsuariosBinario 
		drop table #tempUsuariosQualificados
		
		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_BonusBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 