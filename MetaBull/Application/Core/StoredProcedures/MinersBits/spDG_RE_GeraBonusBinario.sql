Use MinersBits
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusBinario'))
   Drop Procedure spDG_RE_GeraBonusBinario
go

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Gera registros de Posicao e Bonificacao da rede binaria
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusBinario]
   @baseDateIni varchar(8) null, 
   @baseDateFim varchar(8) null,
   @ProcessaBonus int null

AS
BEGIN TRY
BEGIN TRANSACTION

   set nocount on

   DECLARE @dataInicio datetime;
   DECLARE @dataFim datetime;
      
   if (@baseDateIni is null)
   Begin 
      if(@ProcessaBonus = 1)
      Begin
        SET @dataInicio = CAST(CONVERT(VARCHAR(8),dbo.GetDateZion()-1,112) + ' 00:00:00' as datetime2);
        SET @dataFim    = CAST(CONVERT(VARCHAR(8),dbo.GetDateZion()-1,112) + ' 23:59:59' as datetime2);
      End
      Else
      Begin
         SET @dataInicio = CAST(CONVERT(VARCHAR(8),dbo.GetDateZion(),112) + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(CONVERT(VARCHAR(8),dbo.GetDateZion(),112) + ' 23:59:59' as datetime2);
      End
   End
   Else
   Begin
      SET @dataInicio = CAST(@baseDateIni + ' 00:00:00' as datetime2);
      SET @dataFim    = CAST(@baseDateFim + ' 23:59:59' as datetime2);
   End

   DECLARE @valorBonus float;
   DECLARE @faixaPagamentoBonus float;

   DECLARE @valorAfiliacao1 float;
   DECLARE @valorAfiliacao2 float;
   DECLARE @valorAfiliacao3 float;
   DECLARE @valorAfiliacao4 float;
   DECLARE @valorAfiliacao5 float;
   DECLARE @valorAfiliacao6 float;
   DECLARE @valorAfiliacao7 float;
 
   SET @valorAfiliacao1 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
   SET @valorAfiliacao2 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
   SET @valorAfiliacao3 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
   SET @valorAfiliacao4 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   SET @valorAfiliacao5 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 5);
   SET @valorAfiliacao6 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 6);
   SET @valorAfiliacao7 = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 7);

   Declare @limiteBinario1    float =  0.2,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 1) , 0.0),
           @limiteBinario2    float =  0.6,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 2) , 0.0),
           @limiteBinario3    float =  1.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 3) , 0.0),
           @limiteBinario4    float =  2.5,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 4) , 0.0),
           @limiteBinario5    float =  4.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 5) , 0.0),
           @limiteBinario6    float =  8.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 6) , 0.0),
           @limiteBinario7    float = 10.0;  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 7) , 0.0);

   --SET @limiteBinario1 = 0.2 ;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
   --SET @limiteBinario2 = 0.6 ;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
   --SET @limiteBinario3 = 1   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
   --SET @limiteBinario4 = 2.5 ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario5 = 4   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario6 = 8   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario7 = 10  ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);

   Declare @percentualBonusN1 FLOAT =  6.0,
           @percentualBonusN2 FLOAT =  6.5,
           @percentualBonusN3 FLOAT =  9.0,
           @percentualBonusN4 FLOAT = 10.0,
           @percentualBonusN5 FLOAT = 12.0,
           @percentualBonusN6 FLOAT = 15.0,
           @percentualBonusN7 FLOAT = 17.0;

   --SET @percentualBonusN1 =6; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
   --SET @percentualBonusN2 =6.5; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
   --SET @percentualBonusN3 =9; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
   --SET @percentualBonusN4 =10; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @percentualBonusN5 =12; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @percentualBonusN6 =15; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @percentualBonusN7 =17; --(SELECT PercentualBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);


   --SET @limiteBinario1  = 0.2 ;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1);
   --SET @limiteBinario2 = 0.6 ;  --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2);
   --SET @limiteBinario3 = 1   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3);
   --SET @limiteBinario4 = 2.5 ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario5 = 4   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario6 = 8   ; --(SELECT LimiteBinario FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4);
   --SET @limiteBinario7  = 10  ; --(SELECT LimiteBinario FROM Loja.Produt


   Delete Rede.Posicao 
   Where 
   Rede.Posicao.DataFim = @dataFim
      
   CREATE TABLE #tempFranqueadosBinario
   (
      id int IDENTITY(1,1) NOT NULL,
      idFranqueado int,
      perfil varchar(2), /*1=BRONZE, 2=PRATA, 8=OURO */
      assinaturaRede varchar(MAX) COLLATE Latin1_General_CI_AS,
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
      qualificadordireitoid int,   
      acumuladoGanho   float,  
      limiteAssociacao float,        
      dataValidade datetime
   )

   INSERT INTO #tempFranqueadosBinario
   SELECT ID, NivelAssociacao, Assinatura, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, DataValidade
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
      Assinaturaqualificadoesquerda varchar(max) COLLATE Latin1_General_CI_AS,
      Assinaturaqualificadodireita varchar(max) COLLATE Latin1_General_CI_AS
   )
    
   -- INSERE REGISTRO DE AFILIADOS QUALIFICADOS NAS DUAS PERNAS   

   INSERT INTO #tempFranqueadosQualificados
   --SELECT Franqueado.ID, MIN(ESQUERDOS.ID) AS PRIMEIROESQUERDO, MIN(DIREITOS.ID) AS PRIMEIRODIREITO, 0, 0, null, null, null,null
   SELECT Franqueado.ID, 0, 0, 0, 0, null, null, min(esquerdos.assinatura),min(direitos.assinatura)
   FROM Usuario.Usuario Franqueado 
   INNER JOIN Usuario.Usuario ESQUERDOS ON ESQUERDOS.PatrocinadorDiretoID = Franqueado.ID
   INNER JOIN Usuario.Usuario DIREITOS ON DIREITOS.PatrocinadorDiretoID = Franqueado.ID
   WHERE 
   (ESQUERDOS.Assinatura LIKE Franqueado.Assinatura + '0%'
   AND DIREITOS.Assinatura LIKE Franqueado.Assinatura + '1%')
   AND Franqueado.StatusID = 2 /*ativos e pagos*/
   AND ESQUERDOS.StatusID = 2 
   AND DIREITOS.StatusID = 2
   AND Franqueado.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND ESQUERDOS.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND DIREITOS.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND Franqueado.RecebeBonus = 1
   AND ESQUERDOS.GeraBonus = 1
   AND DIREITOS.GeraBonus = 1
   AND Franqueado.DataValidade >= @dataInicio -- data de validade do ativo 
   AND ESQUERDOS.DataValidade  >= @dataInicio -- data de validade do ativo do qualificador esquerdo
   AND DIREITOS.DataValidade   >= @dataInicio -- data de validade do ativo do qualificador direito
   GROUP BY Franqueado.ID 

   -- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA ESQUERDA
      
   INSERT INTO #tempFranqueadosQualificados
   --SELECT Franqueado.ID, 0  AS PRIMEIROESQUERDO, MIN(DIREITOS.ID) AS PRIMEIRODIREITO, 0, 0, null, null, null,null
   SELECT Franqueado.ID, 0, 0, 0, 0, null, null, null ,min(direitos.assinatura)
   FROM Usuario.Usuario Franqueado 
   INNER JOIN Usuario.Usuario DIREITOS ON DIREITOS.PatrocinadorDiretoID = Franqueado.ID
   WHERE 
   DIREITOS.Assinatura LIKE Franqueado.Assinatura + '1%'
   AND Franqueado.StatusID = 2 /*ativos e pagos*/
   AND DIREITOS.StatusID = 2
   AND Franqueado.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND DIREITOS.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND Franqueado.RecebeBonus = 1
   AND DIREITOS.GeraBonus = 1
   and NOT EXISTS (SELECT 1 FROM #tempFranqueadosQualificados WHERE #tempFranqueadosQualificados.idfranqueado = Franqueado.ID)
   AND Franqueado.DataValidade >= @dataInicio -- data de validade do ativo 
   AND DIREITOS.DataValidade   >= @dataInicio -- data de validade do ativo do qualificador direito
   GROUP BY Franqueado.ID 

   -- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA DIREITA
   INSERT INTO #tempFranqueadosQualificados
   --SELECT Franqueado.ID, MIN(ESQUERDOS.ID) AS PRIMEIROESQUERDO, 0 AS PRIMEIRODIREITO, 0, 0, null, null, null,null
   SELECT Franqueado.ID, 0, 0, 0, 0, null, null, mIN(esquerdos.assinatura),null
   FROM Usuario.Usuario Franqueado 
   INNER JOIN Usuario.Usuario ESQUERDOS ON ESQUERDOS.PatrocinadorDiretoID = Franqueado.ID
   WHERE 
   ESQUERDOS.Assinatura LIKE Franqueado.Assinatura + '0%'
   AND Franqueado.StatusID = 2 /*ativos e pagos*/
   AND ESQUERDOS.StatusID = 2 
   AND Franqueado.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND ESQUERDOS.NivelAssociacao IN (7,6,5,4,3,2,1)
   AND Franqueado.RecebeBonus = 1
   AND ESQUERDOS.GeraBonus = 1
   and NOT EXISTS (SELECT 1 FROM #tempFranqueadosQualificados WHERE #tempFranqueadosQualificados.idfranqueado = Franqueado.ID)
   AND Franqueado.DataValidade >= @dataInicio -- data de validade do ativo 
   AND ESQUERDOS.DataValidade  >= @dataInicio -- data de validade do ativo do qualificador esquerdo
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
         WHEN DIREITO.NivelAssociacao = 5 THEN (@valorAfiliacao5)
         WHEN DIREITO.NivelAssociacao = 6 THEN (@valorAfiliacao6)
         WHEN DIREITO.NivelAssociacao = 7 THEN (@valorAfiliacao7)
         ELSE 0
      END
   ),
   valorAfiliacaoEsquerdo = 
   (
      CASE
         WHEN ESQUERDO.NivelAssociacao = 1 THEN (@valorAfiliacao1)
         WHEN ESQUERDO.NivelAssociacao = 2 THEN (@valorAfiliacao2)
         WHEN ESQUERDO.NivelAssociacao = 3 THEN (@valorAfiliacao3)
         WHEN ESQUERDO.NivelAssociacao = 4 THEN (@valorAfiliacao4)
         WHEN ESQUERDO.NivelAssociacao = 5 THEN (@valorAfiliacao5)
         WHEN ESQUERDO.NivelAssociacao = 6 THEN (@valorAfiliacao6)
         WHEN ESQUERDO.NivelAssociacao = 7 THEN (@valorAfiliacao7)

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
     
     --mcr 
     --SELECT * FROM #tempFranqueadosQualificados ORDER BY IDFRANQUEADO
      

   CREATE TABLE #tempPedidos
   (
      idFranqueado int,
      assinaturaRede varchar(MAX) COLLATE Latin1_General_CI_AS,
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

   --mcr      
   --select * from #tempPedidos
   --select * from #tempPedidos where AssinaturaRede LIKE '#0%' ORDER BY AssinaturaRede
      
   UPDATE #tempFranqueadosBinario 
      SET pontosPernaEsquerda = (
         SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
         WHERE #tempPedidos.assinaturaRede LIKE #tempFranqueadosQualificados.Assinaturaqualificadoesquerda + '%'                  
--               AND #tempFranqueadosQualificados.idFranqueadoEsquerdo != #tempPedidos.idFranqueado		-- Define se o qualificador entra ou nao no calculo da pontuação.
      )

   FROM #tempFranqueadosBinario
   INNER JOIN #tempFranqueadosQualificados  ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
      
   UPDATE #tempFranqueadosBinario 
   SET pontosPernaDireita = (
         SELECT SUM(#tempPedidos.pontuacaoPedidos) FROM #tempPedidos 
         WHERE #tempPedidos.assinaturaRede LIKE #tempFranqueadosQualificados.Assinaturaqualificadodireita + '%' 
 --                 AND #tempFranqueadosQualificados.idFranqueadoDireito != #tempPedidos.idFranqueado	-- Define se o qualificador entra ou nao no calculo da pontuação. 
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
      
   --mcr
   --Select #tempFranqueadosBinario.idFranqueado,#tempFranqueadosQualificados.idFranqueadoEsquerdo,#tempFranqueadosQualificados.idFranqueadoDireito from #tempFranqueadosBinario 
   --INNER JOIN #tempFranqueadosQualificados ON #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado
   --WHERE #tempFranqueadosBinario.idFranqueado = #tempFranqueadosQualificados.idFranqueado

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

 UPDATE #tempFranqueadosBinario 
   SET valorBonus =  
      (
      CASE 
         WHEN pontosPernaDireita > pontosPernaEsquerda
         THEN pontosPernaEsquerda
         ELSE pontosPernaDireita
      END /*MINIMO DOS DOIS VALORES.*/
      )
      * (@percentualBonusN5 / 100)
   WHERE perfil = 5

    UPDATE #tempFranqueadosBinario 
   SET valorBonus =  
      (
      CASE 
         WHEN pontosPernaDireita > pontosPernaEsquerda
         THEN pontosPernaEsquerda
         ELSE pontosPernaDireita
      END /*MINIMO DOS DOIS VALORES.*/
      )
      * (@percentualBonusN6 / 100)
   WHERE perfil = 6

    UPDATE #tempFranqueadosBinario 
   SET valorBonus =  
      (
      CASE 
         WHEN pontosPernaDireita > pontosPernaEsquerda
         THEN pontosPernaEsquerda
         ELSE pontosPernaDireita
      END /*MINIMO DOS DOIS VALORES.*/
      )
      * (@percentualBonusN7 / 100)
   WHERE perfil = 7

   UPDATE #tempFranqueadosBinario SET valorBonus = 0 WHERE valorBonus IS NULL

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
   UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario5 WHERE valorBonus > @limiteBinario5 AND perfil = 5
   UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario6 WHERE valorBonus > @limiteBinario5 AND perfil = 6
   UPDATE #tempFranqueadosBinario  SET valorBonus = @limiteBinario7 WHERE valorBonus > @limiteBinario7 AND perfil = 7

   -- Obtem o Acumulado da Associacao (Total de Ganho)
   Update #tempFranqueadosBinario
   Set acumuladoGanho = UG.AcumuladoGanho       
   From #tempFranqueadosBinario T, 
        Usuario.UsuarioGanho UG (nolock)
   where T.idFranqueado = UG.UsuarioID
     and T.dataValidade = UG.DataFim;
   
   -- Obtem o Limite de Ganho da Associacao
   Update #tempFranqueadosBinario
   Set limiteAssociacao = ALG.Valor
   From #tempFranqueadosBinario T,
        Rede.AssociacaoLimiteGanho ALG (nolock)
   where ALG.NivelAssociacao = T.perfil
     and ALG.TipoID = 1; -- Tipo Associacao
     
   -- Verifica o Limite de Ganho da Associacao
   Update #tempFranqueadosBinario 
   Set valorBonus = (limiteAssociacao - acumuladoGanho) 
   Where valorBonus + acumuladoGanho > limiteAssociacao;
      
   /*
   delete from #tempFranqueadosBinario where idFranqueado <> 1736
   delete from #tempFranqueadosQualificados where idFranqueado <> 1736
   */
      
   UPDATE #tempFranqueadosBinario SET 
   pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosLiquidosPernaEsquerda,
   pontosAcumuladoPernaDireita = pontosAcumuladoPernaDireita + pontosLiquidosPernaDireita

   /*8 - INSERE NA TABELA DE PONTUACAO, FRANQUEADOS QUE NAO ESTEJAM LÁ*/

   --Cursor
   
   --mcr
   --drop table ##temp
   --select * into ##temp from #tempFranqueadosBinario
   --select * from ##temp

   Declare
      @id int,
      @AntFetch int

   Declare
      curLocal
   Cursor For
   Select 
      id
   From
      #tempFranqueadosBinario
   Order by id

   Open curLocal

   If (Select @@CURSOR_ROWS) <> 0
   Begin
      Fetch Next From curLocal Into 
          @id
      Select @AntFetch = @@fetch_status

      While @AntFetch = 0
      Begin

         if Exists (Select 'ok' 
                    From Rede.Posicao pos, #tempFranqueadosBinario temp
                    Where pos.UsuarioID = temp.idFranqueado 
                    and pos.DataFim = @dataFim)         
         Begin
            Delete pos
            From 
               Rede.Posicao pos,
               #tempFranqueadosBinario temp
            Where 
               pos.UsuarioID = temp.idFranqueado   and
               pos.DataFim = @dataFim  and
               temp.id = @id
         End

         INSERT INTO Rede.Posicao (UsuarioID, ReferenciaID, DataInicio, DataFim,ValorPernaEsquerda, ValorPernaDireita, AcumuladoEsquerda, AcumuladoDireita, DataCriacao, ValorDiaPernaEsquerda,ValorDiaPernaDireita,QualificadorEsquerdoID,QualificadorDireitoID)
         SELECT 
            temp.idFranqueado, 
            0 Referencia,  
            @dataInicio,  
            @dataFim, 
            dbo.TruncZion(temp.pontosPernaEsquerda, 8), 
            dbo.TruncZion(temp.pontosPernaDireita, 8), 
			   dbo.TruncZion(temp.pontosAcumuladoPernaesquerda, 8),
			   dbo.TruncZion(temp.pontosAcumuladoPernadireita, 8),
--            (ISNULL(AcumuladoEsquerda, 0) + temp.pontosLiquidosPernaEsquerda), 
--            (ISNULL(AcumuladoDireita, 0) + temp.pontosLiquidosPernaDireita), 
            dbo.GetDateZion(), 
            dbo.TruncZion(temp.pontosLiquidosPernaEsquerda, 8),
            dbo.TruncZion(temp.pontosLiquidosPernaDireita, 8) ,
            temp.qualificadoresquerdoid,
            temp.qualificadordireitoid
         FROM 
            #tempFranqueadosBinario as temp 
            LEFT JOIN Rede.Posicao RP ON temp.idFranqueado = RP.UsuarioID and RP.DataFim =
            (
               SELECT max(DataFim) FROM Rede.Posicao
            )
          Where 
             temp.id = @id

         Fetch Next From curLocal Into 
            @id
          
         -- Para ver se nao é fim do loop
         Select @AntFetch = @@fetch_status         
      End -- While
   End -- @@CURSOR_ROWS

   Close curLocal
   Deallocate curLocal

   /*UMA VEZ QUE PONTUOU A TODOS, ACUMULADOS, APAGA QUEM NÃO ESTÁ QUALIFICADO/NÃO PAGOU ATIVO MENSAL */
   /*ANTES DE PAGAR O BONUS*/

   /*2 - REMOVE AFILIADOS QUE AINDA NÃO ESTÃO QUALIFICADOS*/
   --DELETE FROM #tempFranqueadosBinario
   --WHERE idFranqueado NOT IN (
   --   SELECT idFranqueado FROM #tempFranqueadosQualificados
   --)
      
   --/*DINHEIRO*/
   if (@ProcessaBonus = 1)
   Begin
      BEGIN TRANSACTION
      
      -- Gera Bonificacao
      INSERT INTO Rede.Bonificacao 
         (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
      SELECT 
         '3' as Categoria, idFranqueado, 0, 0, @dataFim, dbo.TruncZion(valorBonus, 8) 
      FROM #tempFranqueadosBinario 
           INNER JOIN vw_UsuariosAtivos ON #tempFranqueadosBinario.idFranqueado = vw_UsuariosAtivos.ID
      WHERE valorBonus > 0
       
      -- Atualiza Total de Ganho de Associacao
      Update  Usuario.UsuarioGanho
      Set AcumuladoGanho =  dbo.TruncZion((UG.AcumuladoGanho + T.valorBonus),8)      
      From #tempFranqueadosBinario T, 
           Usuario.UsuarioGanho UG
      where T.idFranqueado = UG.UsuarioID
        and T.dataValidade = UG.DataFim
        and T.valorBonus > 0;
       
      COMMIT TRANSACTION
   End

   --select * from #tempFranqueadosBinario

   drop table #tempPedidos
   drop table #tempFranqueadosBinario 
   drop table #tempFranqueadosQualificados
      
COMMIT TRANSACTION
 
END TRY

BEGIN CATCH
   ROLLBACK TRANSACTION
      
   DECLARE @error int, @message varchar(4000), @xstate int;
   SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
   RAISERROR ('Erro na execucao de spDG_RE_GeraBonusBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
END CATCH 

--exec spDG_RE_GeraBonusBinario null, null, 1
--drop table ##temp
--exec spDG_RE_GeraBonusBinario '20161006','20161006',1

