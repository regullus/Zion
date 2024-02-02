use ClubeVantagens
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('BonusBinario1'))
   Drop Procedure BonusBinario1
Go

Create PROCEDURE [dbo].[BonusBinario1] (@baseDateIni varchar(8) null, @baseDateFim varchar(8) null)
AS

	BEGIN TRY

		BEGIN TRANSACTION

		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;
		
		if (@baseDateIni is null)
		Begin 
			SET @dataInicio = CAST(CONVERT(VARCHAR(8),GETDATE()-1,112) + ' 00:00:00' as datetime2);
			SET @dataFim = CAST(CONVERT(VARCHAR(8),GETDATE()-1,112) + ' 23:59:59' as datetime2);
		End
		Else
		Begin
			SET @dataInicio = CAST(@baseDateIni + ' 00:00:00' as datetime2);
			SET @dataFim = CAST(@baseDateFim + ' 23:59:59' as datetime2);
		End


		DECLARE @valorBonus float;
		DECLARE @faixaPagamentoBonus float;

		DECLARE @valorAfiliacao1 float;
		DECLARE @valorAfiliacao2 float;
		DECLARE @valorAfiliacao3 float;
		DECLARE @valorAfiliacao4 float;
	
		SET @valorAfiliacao1 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
		SET @valorAfiliacao2 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
		SET @valorAfiliacao3 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
		SET @valorAfiliacao4 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);

		DECLARE @limiteBinario1 float;
		DECLARE @limiteBinario2 float;
		DECLARE @limiteBinario3 float;
		DECLARE @limiteBinario4 float;

		SET @limiteBinario1 = 200;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
		SET @limiteBinario2 = 800;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
		SET @limiteBinario3 = 1600; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
		SET @limiteBinario4 = 5000; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);

		DECLARE @percentualBonusN1 FLOAT;
		DECLARE @percentualBonusN2 FLOAT;
		DECLARE @percentualBonusN3 FLOAT;
		DECLARE @percentualBonusN4 FLOAT;

		SET @percentualBonusN1 =100; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
		SET @percentualBonusN2 =100; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
		SET @percentualBonusN3 =100; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
		SET @percentualBonusN4 =100; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);

   Delete Rede.Posicao 
   Where 
   Rede.Posicao.DataInicio >= @dataInicio and Rede.Posicao.DataFim <= @dataFim
   
   		
		CREATE TABLE #tempFranqueadosBinario
		(
			idFranqueado int,
			perfil varchar(2), /*1=BRONZE, 2=PRATA, 8=OURO */
			assinaturaRede varchar(8000) COLLATE SQL_Latin1_General_CP1_CI_AS,
			pontosPernaEsquerda float,
			pontosPernaDireita float,
			valorBonus float,
			pontosAcumuladoPernaEsquerda float,
			pontosAcumuladoPernaDireita float,
			times int,
			valorTotalPernaEsquerda float,
			valorTotalPernaDireita float,
			valorAfiliacaoEsquerda float,
			valorAfiliacaoDireita float,
			pontosLiquidosPernaEsquerda float,
			pontosLiquidosPernaDireita float,
			qualificadoresquerdoid int, 
			qualificadordireitoid int           --Ari
		)

		INSERT INTO #tempFranqueadosBinario
		SELECT ID, NivelAssociacao, Assinatura, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0,0  --Ari
		FROM Usuario.Usuario


		CREATE TABLE #tempFranqueadosQualificados
		(
			idFranqueado int,
			idFranqueadoEsquerdo int, 
			idFranqueadoDireito int,
			valorAfiliacaoDireito float,
			valorAfiliacaoEsquerdo float,
			dataPedidoAfiliacaoEsquerdo datetime,
			dataPedidoAfiliacaoDireito datetime,
			Assinaturaqualificadoesquerda varchar(max),
			Assinaturaqualificadodireita varchar(max)
		)
		
		-- INSERE GERISTRO DE AFILIADOS QUALIFICADOS NAS DUAS PERNAS	

		INSERT INTO #tempFranqueadosQualificados
		
--		SELECT Franqueado.ID, MIN(ESQUERDOS.ID) AS PRIMEIROESQUERDO, MIN(DIREITOS.ID) AS PRIMEIRODIREITO, 0, 0, null, null, null,null
		SELECT Franqueado.ID, 0, 0, 0, 0, null, null, mIN(esquerdos.assinatura),min(direitos.assinatura)
		FROM Usuario.Usuario Franqueado 
		INNER JOIN Usuario.Usuario ESQUERDOS ON ESQUERDOS.PatrocinadorDiretoID = Franqueado.ID
		INNER JOIN Usuario.Usuario DIREITOS ON DIREITOS.PatrocinadorDiretoID = Franqueado.ID
		WHERE 
		(ESQUERDOS.Assinatura LIKE Franqueado.Assinatura + '0%'
		AND DIREITOS.Assinatura LIKE Franqueado.Assinatura + '1%')
		AND Franqueado.StatusID = 2 /*ativos e pagos*/
		AND ESQUERDOS.StatusID = 2 
		AND DIREITOS.StatusID = 2
		AND Franqueado.NivelAssociacao IN (4,3,2,1)
		AND ESQUERDOS.NivelAssociacao IN (4,3,2,1)
		AND DIREITOS.NivelAssociacao IN (4,3,2,1)
		AND Franqueado.RecebeBonus = 1
		AND ESQUERDOS.GeraBonus = 1
		AND DIREITOS.GeraBonus = 1
--		AND Franqueado.DataValidade >= (GetDate()-1) -- data de validade do ativo mensal
--		AND ESQUERDOS.DataValidade  >= (GetDate()-1) -- data de validade do ativo mensal do qualificador esquerdo
--		AND DIREITOS.DataValidade   >= (GetDate()-1) -- data de validade do ativo mensal do qualificador direito
		--AND FRANQUEADO.ID = 69 
		GROUP BY Franqueado.ID 

		-- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA ESQUERDA
		
		INSERT INTO #tempFranqueadosQualificados

--		SELECT Franqueado.ID, 0  AS PRIMEIROESQUERDO, MIN(DIREITOS.ID) AS PRIMEIRODIREITO, 0, 0, null, null, null,null
		SELECT Franqueado.ID, 0, 0, 0, 0, null, null, null ,min(direitos.assinatura)
		FROM Usuario.Usuario Franqueado 
		INNER JOIN Usuario.Usuario DIREITOS ON DIREITOS.PatrocinadorDiretoID = Franqueado.ID
		WHERE 
		DIREITOS.Assinatura LIKE Franqueado.Assinatura + '1%'
		AND Franqueado.StatusID = 2 /*ativos e pagos*/
		AND DIREITOS.StatusID = 2
		AND Franqueado.NivelAssociacao IN (4,3,2,1)
		AND DIREITOS.NivelAssociacao IN (4,3,2,1)
		AND Franqueado.RecebeBonus = 1
		AND DIREITOS.GeraBonus = 1
		and NOT EXISTS (SELECT 1 FROM #tempFranqueadosQualificados WHERE #tempFranqueadosQualificados.idfranqueado = Franqueado.ID)
--		AND Franqueado.DataValidade >= (GetDate()-1) -- data de validade do ativo mensal
--		AND DIREITOS.DataValidade   >= (GetDate()-1) -- data de validade do ativo mensal do qualificador direito
		--AND FRANQUEADO.ID = 69 
		GROUP BY Franqueado.ID 

		-- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA DIREITA
		INSERT INTO #tempFranqueadosQualificados

--		SELECT Franqueado.ID, MIN(ESQUERDOS.ID) AS PRIMEIROESQUERDO, 0 AS PRIMEIRODIREITO, 0, 0, null, null, null,null
		SELECT Franqueado.ID, 0, 0, 0, 0, null, null, mIN(esquerdos.assinatura),null
		FROM Usuario.Usuario Franqueado 
		INNER JOIN Usuario.Usuario ESQUERDOS ON ESQUERDOS.PatrocinadorDiretoID = Franqueado.ID
		WHERE 
		ESQUERDOS.Assinatura LIKE Franqueado.Assinatura + '0%'
		AND Franqueado.StatusID = 2 /*ativos e pagos*/
		AND ESQUERDOS.StatusID = 2 
		AND Franqueado.NivelAssociacao IN (4,3,2,1)
		AND ESQUERDOS.NivelAssociacao IN (4,3,2,1)
		AND Franqueado.RecebeBonus = 1
		AND ESQUERDOS.GeraBonus = 1
		and NOT EXISTS (SELECT 1 FROM #tempFranqueadosQualificados WHERE #tempFranqueadosQualificados.idfranqueado = Franqueado.ID)
--		AND Franqueado.DataValidade >= (GetDate()-1) -- data de validade do ativo mensal
--		AND ESQUERDOS.DataValidade  >= (GetDate()-1) -- data de validade do ativo mensal do qualificador esquerdo
		--AND FRANQUEADO.ID = 69 
		GROUP BY Franqueado.ID

		UPDATE  #tempFranqueadosQualificados
		SET idfranqueadoesquerdo = ESQUERDO.id  FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.Usuario ESQUERDO ON #tempFranqueadosQualificados.Assinaturaqualificadoesquerda = ESQUERDO.assinatura

		UPDATE  #tempFranqueadosQualificados
		SET idfranqueadodireito = DIREITO.id  FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.Usuario DIREITO ON #tempFranqueadosQualificados.Assinaturaqualificadodireita = DIREITO.assinatura

/*
		UPDATE  #tempFranqueadosQualificados
		SET Assinaturaqualificadoesquerda = ESQUERDO.Assinatura  FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.Usuario ESQUERDO ON #tempFranqueadosQualificados.idFranqueadoEsquerdo = ESQUERDO.ID
		
		UPDATE  #tempFranqueadosQualificados
		SET Assinaturaqualificadodireita = DIREITO.Assinatura  FROM #tempFranqueadosQualificados 
		INNER JOIN  Usuario.Usuario DIREITO ON #tempFranqueadosQualificados.idFranqueadoDireito = DIREITO.ID
*/


		UPDATE #tempFranqueadosQualificados
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
		FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.Usuario ESQUERDO ON #tempFranqueadosQualificados.idFranqueadoEsquerdo = ESQUERDO.ID
		INNER JOIN  Usuario.Usuario DIREITO ON #tempFranqueadosQualificados.idFranqueadoDireito = DIREITO.ID

		
		UPDATE #tempFranqueadosQualificados
		SET dataPedidoAfiliacaoEsquerdo = Associacao.Data
		FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.UsuarioAssociacao Associacao ON #tempFranqueadosQualificados.idFranqueadoEsquerdo = Associacao.UsuarioID
			AND Associacao.Data BETWEEN @dataInicio AND @dataFim
			AND Associacao.NivelAssociacao = 0

		UPDATE #tempFranqueadosQualificados
		SET dataPedidoAfiliacaoDireito = Associacao.Data
		FROM #tempFranqueadosQualificados 
		INNER JOIN Usuario.UsuarioAssociacao Associacao ON #tempFranqueadosQualificados.idFranqueadoDireito = Associacao.UsuarioID
			AND Associacao.Data BETWEEN @dataInicio AND @dataFim
			AND Associacao.NivelAssociacao = 0

		
		SELECT * FROM #tempFranqueadosQualificados ORDER BY IDFRANQUEADO
		
		CREATE TABLE #tempPedidos
		(
			idFranqueado int,
			assinaturaRede varchar(8000) COLLATE SQL_Latin1_General_CP1_CI_AS,
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
			WHERE PPAGST.Data BETWEEN @dataInicio AND @datafim
			AND U.GeraBonus = 1
			AND PPAGST.StatusID = 3 /*PAGO */
			AND Prod.TipoID IN (1,2) /*"PRODUTOS" NAO DEVEM IR PRO CALCULO DO BINARIO. APENAS KITS.*/
		GROUP BY U.ID, Assinatura
		
		select * from #tempPedidos
		
		select * from #tempPedidos where AssinaturaRede LIKE '#0%' ORDER BY AssinaturaRede
		
		UPDATE #tempFranqueadosBinario 
			SET pontosPernaEsquerda = (
				SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
				WHERE #tempPedidos.assinaturaRede LIKE #tempFranqueadosQualificados.Assinaturaqualificadoesquerda + '%'				      
					 AND #tempFranqueadosQualificados.idFranqueadoEsquerdo != #tempPedidos.idFranqueado
			)
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados  ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		
		
		UPDATE #tempFranqueadosBinario 
		SET pontosPernaDireita = (
				SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
				WHERE #tempPedidos.assinaturaRede LIKE #tempFranqueadosQualificados.Assinaturaqualificadodireita + '%' 
				       AND #tempFranqueadosQualificados.idFranqueadoDireito != #tempPedidos.idFranqueado 
			)
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados  ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado

		UPDATE #tempFranqueadosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		UPDATE #tempFranqueadosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempFranqueadosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempFranqueadosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		

		UPDATE #tempFranqueadosBinario 
		SET qualificadoresquerdoid = #tempFranqueadosQualificados.idFranqueadoEsquerdo 
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		WHERE #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado

		UPDATE #tempFranqueadosBinario 
		SET qualificadordireitoid = #tempFranqueadosQualificados.idFranqueadoDireito
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		WHERE #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		
		Select #tempFranqueadosBinario.idFranqueado,#tempFranqueadosQualificados.idFranqueadoEsquerdo,#tempFranqueadosQualificados.idFranqueadoDireito from #tempFranqueadosBinario 
		INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		WHERE #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado

		/*subtrai do total, os que foram ativados neste período*/
		UPDATE #tempFranqueadosBinario 
		SET ValorAfiliacaoEsquerda = #tempFranqueadosQualificados.valorAfiliacaoEsquerdo
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		WHERE #tempFranqueadosQualificados.dataPedidoAfiliacaoEsquerdo BETWEEN @dataInicio AND @dataFim
		
		UPDATE #tempFranqueadosBinario 
		SET ValorAfiliacaoDireita = #tempFranqueadosQualificados.valorAfiliacaoDireito
		FROM #tempFranqueadosBinario
		INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
		WHERE #tempFranqueadosQualificados.dataPedidoAfiliacaoDireito BETWEEN @dataInicio AND @dataFim


		UPDATE #tempFranqueadosBinario SET  pontosLiquidosPernaEsquerda = pontosPernaEsquerda
		UPDATE #tempFranqueadosBinario SET  pontosLiquidosPernaDireita = pontosPernaDireita

		/*Adiciona Acumulado na conta*/
		UPDATE #tempFranqueadosBinario 
		SET 
			pontosPernaEsquerda = pontosPernaEsquerda + (CASE WHEN (RP.ValorPernaEsquerda >= RP.ValorPernaDireita) THEN RP.ValorPernaEsquerda - RP.ValorPernaDireita ELSE 0 END ),
			pontosPernaDireita = pontosPernaDireita + (CASE WHEN (RP.ValorPernaDireita > RP.ValorPernaEsquerda) THEN RP.ValorPernaDireita - RP.ValorPernaEsquerda  ELSE 0 END),
			pontosAcumuladoPernaEsquerda = RP.AcumuladoEsquerda,
			pontosAcumuladoPernadireita = RP.AcumuladoDireita
			
		FROM #tempFranqueadosBinario
		INNER JOIN Rede.Posicao RP ON #tempFranqueadosBinario.idFranqueado = RP.UsuarioID
		WHERE RP.DataFim =
		(
			SELECT max(DataFim) FROM Rede.Posicao
		)

		
		UPDATE #tempFranqueadosBinario SET pontosPernaDireita = 0 WHERE pontosPernaDireita IS NULL
		UPDATE #tempFranqueadosBinario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda IS NULL
		UPDATE #tempFranqueadosBinario SET valorAfiliacaoEsquerda = 0 WHERE valorAfiliacaoEsquerda IS NULL
		UPDATE #tempFranqueadosBinario SET valorAfiliacaoDireita = 0 WHERE valorAfiliacaoDireita IS NULL
		UPDATE #tempFranqueadosBinario SET pontosAcumuladoPernaDireita = 0 WHERE pontosAcumuladoPernaDireita IS NULL
		UPDATE #tempFranqueadosBinario SET pontosAcumuladoPernaEsquerda = 0 WHERE pontosAcumuladoPernaEsquerda IS NULL
		
				
		/*PONTOS LIQUIDOS,ANTES DE CALCULAR O BONUS FINAL*/
		/*
		UPDATE #tempFranqueadosBinario 
		SET pontosLiquidosPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita - valorAfiliacaoDireita,
		pontosLiquidosPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda - valorAfiliacaoEsquerda
		*/

		UPDATE #tempFranqueadosBinario 
		SET valorBonus = 
			(
			CASE 
				WHEN pontosPernaDireita > pontosPernaEsquerda
				THEN pontosPernaEsquerda
				ELSE pontosPernaDireita
			END /*MINIMO DOS DOIS VALORES.*/
			) * (@percentualBonusN1 / 100)
		WHERE perfil = 1 

		
		UPDATE #tempFranqueadosBinario 
		SET valorBonus = 
			(
			CASE 
				WHEN pontosPernaDireita > pontosPernaEsquerda
				THEN pontosPernaEsquerda
				ELSE pontosPernaDireita
			END /*MINIMO DOS DOIS VALORES.*/
			) * (@percentualBonusN2 / 100)
		WHERE perfil = 2 
		

		UPDATE #tempFranqueadosBinario 
		SET valorBonus =  
			(
			CASE 
				WHEN pontosPernaDireita > pontosPernaEsquerda
				THEN pontosPernaEsquerda
				ELSE pontosPernaDireita
			END /*MINIMO DOS DOIS VALORES.*/
			)
			* (@percentualBonusN3 / 100)
		WHERE perfil = 3


		UPDATE #tempFranqueadosBinario 
		SET valorBonus =  
			(
			CASE 
				WHEN pontosPernaDireita > pontosPernaEsquerda
				THEN pontosPernaEsquerda
				ELSE pontosPernaDireita
			END /*MINIMO DOS DOIS VALORES.*/
			)
			* (@percentualBonusN4 / 100)
		WHERE perfil = 4



		UPDATE #tempFranqueadosBinario SET valorBonus = 0 WHERE valorBonus IS NULL

		/*Acumulado*/
		--UPDATE #tempFranqueadosBinario SET pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda  
		--UPDATE #tempFranqueadosBinario SET pontosAcumuladoPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita 

		

		
		/*TEMPORARIO!*/
		
		/*deduz o valor concedido em bonus, da perna maior */
		/*
		UPDATE #tempFranqueadosBinario 
		SET pontosLiquidosPernaDireita = pontosLiquidosPernaDireita - valorBonus,
		pontosPernaDireita = pontosPernaDireita - valorBonus
		WHERE pontosLiquidosPernaDireita > pontosLiquidosPernaEsquerda
		
		UPDATE #tempFranqueadosBinario 
		SET pontosLiquidosPernaEsquerda = pontosLiquidosPernaEsquerda - valorBonus,
		pontosPernaEsquerda = pontosPernaEsquerda - valorBonus
		WHERE  pontosLiquidosPernaEsquerda >= pontosLiquidosPernaDireita
		
		UPDATE #tempFranqueadosBinario SET pontosLiquidosPernaEsquerda = 0 where pontosLiquidosPernaEsquerda < 0
		UPDATE #tempFranqueadosBinario SET pontosPernaEsquerda = 0 where pontosPernaEsquerda < 0
		 
		UPDATE #tempFranqueadosBinario SET pontosLiquidosPernaDireita = 0 where pontosLiquidosPernaDireita < 0
		UPDATE #tempFranqueadosBinario SET pontosPernaDireita = 0 where pontosPernaDireita < 0
		*/
		
		/*LIMITE DIARIO DE BONUS DE REDE*/
		UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario1 WHERE valorBonus > @limiteBinario1 AND perfil = 1
		UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario2 WHERE valorBonus > @limiteBinario2 AND perfil = 2
		UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario3 WHERE valorBonus > @limiteBinario3 AND perfil = 3
		UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario4 WHERE valorBonus > @limiteBinario4 AND perfil = 4
		
		/*
		delete from #tempFranqueadosBinario where idFranqueado <> 1736
		delete from #tempFranqueadosQualificados where idFranqueado <> 1736
		*/
		
			UPDATE #tempFranqueadosBinario SET 
			pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosPernaEsquerda,
			pontosAcumuladoPernaDireita = pontosAcumuladoPernaDireita + pontosPernaDireita

			/*8 - INSERE NA TABELA DE PONTUACAO, FRANQUEADOS QUE NAO ESTEJAM LÁ*/
	
			INSERT INTO Rede.Posicao (UsuarioID, ReferenciaID, DataInicio, DataFim, 
			ValorPernaEsquerda, ValorPernaDireita,
			AcumuladoEsquerda, AcumuladoDireita, DataCriacao, ValorDiaPernaEsquerda,ValorDiaPernaDireita,QualificadorEsquerdoID,QualificadorDireitoID)
			SELECT #tempFranqueadosBinario.idFranqueado, 0 as Referencia,  @dataInicio,  @dataFim, 
			#tempFranqueadosBinario.pontosPernaEsquerda, #tempFranqueadosBinario.pontosPernaDireita, (ISNULL(AcumuladoEsquerda, 0) + #tempFranqueadosBinario.pontosLiquidosPernaEsquerda), (ISNULL(AcumuladoDireita, 0) + #tempFranqueadosBinario.pontosLiquidosPernaDireita), 
			GETDATE(), #tempFranqueadosBinario.pontosLiquidosPernaEsquerda,#tempFranqueadosBinario.pontosLiquidosPernaDireita,#tempFranqueadosBinario.qualificadoresquerdoid,#tempFranqueadosBinario.qualificadordireitoid
			FROM #tempFranqueadosBinario
			LEFT JOIN Rede.Posicao RP ON #tempFranqueadosBinario.idFranqueado = RP.UsuarioID and RP.DataFim =
			(
				SELECT max(DataFim) FROM Rede.Posicao
			)
			
			
			/*UMA VEZ QUE PONTUOU A TODOS, ACUMULADOS, APAGA QUEM NÃO ESTÁ QUALIFICADO/NÃO PAGOU ATIVO MENSAL */
			/*ANTES DE PAGAR O BONUS*/
			

			/*2 - REMOVE AFILIADOS QUE AINDA NÃO ESTÃO QUALIFICADOS*/
			DELETE FROM #tempFranqueadosBinario
			WHERE idFranqueado NOT IN (
				SELECT idFranqueado FROM #tempFranqueadosQualificados
			)
			
			/*DINHEIRO*/
			INSERT INTO Rede.Bonificacao (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
			SELECT '3' as Categoria, idFranqueado, 0, 0, @dataFim, valorBonus 
			FROM #tempFranqueadosBinario INNER JOIN vw_UsuariosAtivos ON #tempFranqueadosBinario.idFranqueado = vw_UsuariosAtivos.ID
			WHERE valorBonus > 0


       select * from #tempFranqueadosBinario

		drop table #tempPedidos
		drop table #tempFranqueadosBinario 
		drop table #tempFranqueadosQualificados
		
		COMMIT TRANSACTION
 
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_BonusBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 

