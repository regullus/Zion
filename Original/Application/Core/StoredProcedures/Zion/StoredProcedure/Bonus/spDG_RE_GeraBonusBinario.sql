Use Zion
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusBinario'))
    Drop Procedure spDG_RE_GeraBonusBinario
go

-- =============================================================================================
-- Author.....: 
-- Create date: 20/08/2020
-- Description: Gera registros de Posicao e Bonificacao da Rede Binaria
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusBinario]
    @DaseDate      VARCHAR(8) = null,
    @ProcessaBonus INT = 1

AS
BEGIN TRY  
   	SET NOCOUNT ON

    -- CONTROLE DE EXECUÇÃO - iNICIO
    DECLARE 
	    @Frequencia VARCHAR(10) = IsNull((  SELECT Dados 
	                                        FROM Sistema.Configuracao 
				  						    WHERE Chave = 'BONUS_BINARIO_FREQUENCIA')
									     , 'NO' );
    DECLARE 									 	   
		@TpFrequencia VARCHAR(10) = SUBSTRING(@Frequencia ,1 , 2),	 	
	    @Dia          VARCHAR(10) = SUBSTRING(@Frequencia ,3 , LEN(@Frequencia) - 2);
        
    If (@TpFrequencia =  'M:')
	Begin
	   If ( @Dia != DATEPART(day, dbo.GetDateZion()) )
	       Return;
	End		
    If (@TpFrequencia =  'S:')
	Begin
	   If ( @Dia != DATEPART(DW , dbo.GetDateZion()) )
	       Return;
	End
    If (@TpFrequencia != 'D:')
	Begin		  
	   Return;
	End
    -- CONTROLE DE EXECUÇÃO - FIM

    DECLARE 
        @DataInicio    DATETIME   = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2),		
        @DataFim       DATETIME   = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2),
        @CategoriaID   INT        = 12, -- Bônus Binario
	    @RegraID	   INT        = 3 ,	
	    @CicloID       INT        = (SELECT ID FROM Rede.Ciclo (NOLOCK) WHERE Ativo = 1),
		@ControleGanho VARCHAR(5) = (SELECT Dados FROM Sistema.Configuracao WHERE Chave = 'REDE_CONTROLA_GANHO_ASSOCIACAO'),
		@QtdeCasas     INT        = 2;

    if (@DaseDate Is Not Null)		
    Begin
       SET @DataInicio = CAST(@DaseDate + ' 00:00:00' as datetime2);
	   SET @DataFim    = CAST(@DaseDate + ' 23:59:59' as datetime2);
    End


    CREATE TABLE #TUsuario (
        id                           INT IDENTITY(1,1) NOT NULL,
        idUsuario                    INT,
        NivelAssociacao              INT,
        assinaturaRede               VARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS,
        pontosPernaEsquerda          FLOAT,
        pontosPernaDireita           FLOAT,
        valorBonus                   FLOAT,
        pontosAcumuladoPernaEsquerda FLOAT,
        pontosAcumuladoPernaDireita  FLOAT,
        times                        int,
        valorTotalPernaEsquerda      FLOAT,
        valorTotalPernaDireita       FLOAT,
        valorAfiliacaoEsquerda       FLOAT,
        valorAfiliacaoDireita        FLOAT,
        pontosLiquidosPernaEsquerda  FLOAT,
        pontosLiquidosPernaDireita   FLOAT,
        qualificadoresquerdoid       INT, 
        qualificadordireitoid        INT,   
        acumuladoGanho               FLOAT,  
        limiteAssociacao             FLOAT,        
        dataValidade                 DATETIME
    );

    INSERT INTO 
        #TUsuario
    SELECT 
        ID, NivelAssociacao, Assinatura, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, DataValidade
    FROM 
        Usuario.Usuario (NOLOCK);


    CREATE TABLE #TUsuarioQualificados(
        IdUsuario            INT,
        IdUsuarioEsquerdo    INT, 
        IdUsuarioDireito     INT,
        VlrAfiliacaoDireito  FLOAT,
        VlrAfiliacaoEsquerdo FLOAT,
        DatPedidoEsquerdo    DATETIME,
        DatPedidoDireito     DATETIME,
        AssEsquerda          VARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
        AssDireita           VARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS
    );
    
    -- INSERE REGISTRO DE USUARIOS QUALIFICADOS NAS DUAS PERNAS   
    INSERT INTO 
        #TUsuarioQualificados
    SELECT 
        U.ID, 0, 0, 0, 0, null, null, MIN(E.assinatura),MIN(D.assinatura)
    FROM 
        Usuario.Usuario            U (NOLOCK)
        INNER JOIN Usuario.Usuario E (NOLOCK) ON E.PatrocinadorDiretoID = U.ID
        INNER JOIN Usuario.Usuario D (NOLOCK) ON D.PatrocinadorDiretoID = U.ID
    WHERE 
	    U.StatusID = 2 /*ativos e pagos*/
    AND U.NivelAssociacao > 0 --IN (7,6,5,4,3,2,1)
	AND U.RecebeBonus = 1
	AND U.DataValidade >= @dataInicio -- data de validade do ativo 
	AND E.StatusID = 2 
	AND E.NivelAssociacao > 0 -- IN (7,6,5,4,3,2,1)
	AND E.GeraBonus = 1
	AND E.DataValidade  >= @dataInicio -- data de validade do ativo do qualificador esquerdo
    AND E.Assinatura LIKE U.Assinatura + '0%'
    AND D.StatusID = 2
    AND D.NivelAssociacao > 0 --IN (7,6,5,4,3,2,1)
    AND D.GeraBonus = 1
    AND D.DataValidade   >= @dataInicio -- data de validade do ativo do qualificador direito
	AND D.Assinatura LIKE U.Assinatura + '1%'
    GROUP BY 
        U.ID; 

    -- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA ESQUERDA      
    INSERT INTO 
        #TUsuarioQualificados
    SELECT 
        U.ID, 0, 0, 0, 0, null, null, null ,MIN(D.assinatura)
    FROM 
        Usuario.Usuario            U (NOLOCK)
        INNER JOIN Usuario.Usuario D (NOLOCK) ON D.PatrocinadorDiretoID = U.ID
    WHERE 
        U.StatusID = 2 /*ativos e pagos*/
    AND U.NivelAssociacao > 0 --N (7,6,5,4,3,2,1)
    AND U.RecebeBonus = 1
    AND U.DataValidade >= @dataInicio -- data de validade do ativo 
    AND D.StatusID = 2
    AND D.NivelAssociacao > 0 --IN (7,6,5,4,3,2,1) 
    AND D.GeraBonus = 1
    AND D.DataValidade   >= @dataInicio -- data de validade do ativo do qualificador direito
    AND D.Assinatura LIKE U.Assinatura + '1%'
    AND Not Exists (SELECT 1 FROM #TUsuarioQualificados WHERE #TUsuarioQualificados.idUsuario = U.ID)
    GROUP BY 
        U.ID; 

    -- INSERE REGISTRO DE AFILIADOS QUALIFICADOS SOMENTE NA PERNA DIREITA
    INSERT INTO 
        #TUsuarioQualificados
    SELECT 
        U.ID, 0, 0, 0, 0, null, null, MIN(E.assinatura),null
    FROM 
        Usuario.Usuario            U (NOLOCK)
        INNER JOIN Usuario.Usuario E (NOLOCK) ON E.PatrocinadorDiretoID = U.ID
     WHERE 
        U.StatusID = 2 /*ativos e pagos*/
    AND U.NivelAssociacao > 0 -- IN (7,6,5,4,3,2,1)
    AND U.RecebeBonus = 1
    AND U.DataValidade >= @dataInicio -- data de validade do ativo    
    AND E.Assinatura LIKE U.Assinatura + '0%'
    AND E.StatusID = 2 
    AND E.NivelAssociacao > 0 -- IN (7,6,5,4,3,2,1)
    AND E.GeraBonus = 1
    AND E.DataValidade  >= @dataInicio -- data de validade do ativo do qualificador esquerdo   
    AND Not Exists (SELECT 1 FROM #TUsuarioQualificados WHERE #TUsuarioQualificados.idUsuario = U.ID)
    GROUP BY 
        U.ID;

    UPDATE  
        #TUsuarioQualificados
    SET 
        IdUsuarioEsquerdo = E.id  
    FROM 
       #TUsuarioQualificados      T
       INNER JOIN Usuario.Usuario E (NOLOCK) ON T.AssEsquerda = E.assinatura;

    UPDATE  
        #TUsuarioQualificados
    SET 
        IdUsuarioDireito = D.id  
    FROM 
        #TUsuarioQualificados      T
        INNER JOIN Usuario.Usuario D (NOLOCK) ON T.AssDireita = D.assinatura;


    DECLARE
        @valorAfiliacao1 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 1),
        @valorAfiliacao2 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 2),
        @valorAfiliacao3 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 3),
        @valorAfiliacao4 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 4),
        @valorAfiliacao5 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 5),
        @valorAfiliacao6 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 6),
        @valorAfiliacao7 float = (SELECT Bonificacao FROM Loja.ProdutoValor PV INNER JOIN Loja.Produto P on PV.ProdutoID = P.ID WHERE P.TipoID = 1 AND P.NivelAssociacao = 7);

    UPDATE #TUsuarioQualificados
    SET VlrAfiliacaoDireito = 
    (
       CASE
          WHEN D.NivelAssociacao = 1 THEN (@valorAfiliacao1)
          WHEN D.NivelAssociacao = 2 THEN (@valorAfiliacao2)
          WHEN D.NivelAssociacao = 3 THEN (@valorAfiliacao3)
          WHEN D.NivelAssociacao = 4 THEN (@valorAfiliacao4)
          WHEN D.NivelAssociacao = 5 THEN (@valorAfiliacao5)
          WHEN D.NivelAssociacao = 6 THEN (@valorAfiliacao6)
          WHEN D.NivelAssociacao = 7 THEN (@valorAfiliacao7)
          ELSE 0
       END
    ),
    VlrAfiliacaoEsquerdo = 
     (
       CASE
          WHEN E.NivelAssociacao = 1 THEN (@valorAfiliacao1)
          WHEN E.NivelAssociacao = 2 THEN (@valorAfiliacao2)
          WHEN E.NivelAssociacao = 3 THEN (@valorAfiliacao3)
          WHEN E.NivelAssociacao = 4 THEN (@valorAfiliacao4)
          WHEN E.NivelAssociacao = 5 THEN (@valorAfiliacao5)
          WHEN E.NivelAssociacao = 6 THEN (@valorAfiliacao6)
          WHEN E.NivelAssociacao = 7 THEN (@valorAfiliacao7)
          ELSE 0
       END
    )
    FROM 
        #TUsuarioQualificados 
        INNER JOIN Usuario.Usuario E (NOLOCK) ON #TUsuarioQualificados.IdUsuarioEsquerdo = E.ID
        INNER JOIN Usuario.Usuario D (NOLOCK) ON #TUsuarioQualificados.IdUsuarioDireito  = D.ID;
      
   UPDATE 
       #TUsuarioQualificados
   SET 
       DatPedidoEsquerdo = A.Data
   FROM 
       #TUsuarioQualificados 
       INNER JOIN Usuario.UsuarioAssociacao A (NOLOCK) ON #TUsuarioQualificados.IdUsuarioEsquerdo = A.UsuarioID
                                                          AND A.Data BETWEEN @dataInicio AND @dataFim
                                                          AND A.NivelAssociacao = 0;

   UPDATE 
       #TUsuarioQualificados
   SET 
       DatPedidoDireito = A.Data
   FROM 
       #TUsuarioQualificados 
       INNER JOIN Usuario.UsuarioAssociacao A ON #TUsuarioQualificados.IdUsuarioDireito = A.UsuarioID
                                                 AND A.Data BETWEEN @dataInicio AND @dataFim
                                                 AND A.NivelAssociacao = 0;
  
    -- Obtem e totaliza os pontos do Pedidos pagos de Rede
    CREATE TABLE #TPedidos (
       IdUsuario int,
       AssinaturaRede varchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS,
       pontuacaoPedidos float
    );
      
    INSERT INTO 
        #TPedidos
    SELECT 
        U.ID, 
	    Assinatura,
        SUM(PV.Bonificacao) AS pontos
    FROM 
        Loja.Pedido                           P   (NOLOCK)
        INNER JOIN Usuario.Usuario            U   (NOLOCK) ON P.UsuarioID = U.ID
        INNER JOIN Loja.PedidoItem            PI  (NOLOCK) ON P.ID = PI.PedidoID
        INNER JOIN Loja.Produto               PR  (NOLOCK) ON PI.ProdutoID = PR.ID
        INNER JOIN Loja.ProdutoValor          PV  (NOLOCK) ON PV.ProdutoID = PR.ID
        INNER JOIN Loja.PedidoPagamento       PP  (NOLOCK) ON P.ID = PP.PedidoID
        INNER JOIN Loja.PedidoPagamentoStatus PPS (NOLOCK) ON PP.ID = PPS.PedidoPagamentoID
    WHERE 
        PPS.Data BETWEEN @dataInicio AND @datafim
    AND U.GeraBonus = 1
    AND PPS.StatusID = 3   /* PAGO */
    AND PR.TipoID IN (1,2) /* Adesão e Upgrade */
    GROUP BY 
       U.ID, 
	   Assinatura;
      
    UPDATE #TUsuario 
    SET 
        pontosPernaEsquerda = ( SELECT SUM(pontuacaoPedidos) 
	                            FROM #TPedidos 
                                WHERE assinaturaRede LIKE #TUsuarioQualificados.AssEsquerda + '%')
    FROM 
        #TUsuario
        INNER JOIN #TUsuarioQualificados  ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario;
      
    UPDATE 
        #TUsuario 
    SET 
        pontosPernaDireita = ( SELECT SUM(pontuacaoPedidos) 
	                           FROM #TPedidos 
                               WHERE assinaturaRede LIKE #TUsuarioQualificados.AssDireita + '%' )
    FROM 
        #TUsuario
        INNER JOIN #TUsuarioQualificados ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario;

    UPDATE #TUsuario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda Is Null;
    UPDATE #TUsuario SET pontosPernaDireita  = 0 WHERE pontosPernaDireita  Is Null;
    UPDATE #TUsuario SET pontosPernaDireita  = 0 WHERE pontosPernaDireita  Is Null;
    UPDATE #TUsuario SET pontosPernaEsquerda = 0 WHERE pontosPernaEsquerda Is Null;

    UPDATE 
	    #TUsuario 
    SET 
	    qualificadoresquerdoid = #TUsuarioQualificados.IdUsuarioEsquerdo 
    FROM 
	    #TUsuario
        INNER JOIN #TUsuarioQualificados ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario
    WHERE #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario

    UPDATE 
	    #TUsuario 
    SET 
	    qualificadordireitoid = #TUsuarioQualificados.IdUsuarioDireito
    FROM 
	    #TUsuario
        INNER JOIN #TUsuarioQualificados ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario
    WHERE #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario
        
    /*subtrai do total, os que foram ativados neste período*/
    UPDATE 
        #TUsuario 
    SET 
        ValorAfiliacaoEsquerda = #TUsuarioQualificados.VlrAfiliacaoEsquerdo
    FROM 
        #TUsuario
        INNER JOIN #TUsuarioQualificados ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario
    WHERE #TUsuarioQualificados.DatPedidoEsquerdo BETWEEN @DataInicio AND @DataFim;
      
    UPDATE 
        #TUsuario 
    SET 
        ValorAfiliacaoDireita = #TUsuarioQualificados.VlrAfiliacaoDireito
    FROM 
        #TUsuario
        INNER JOIN #TUsuarioQualificados ON #TUsuario.idUsuario = #TUsuarioQualificados.idUsuario
    WHERE #TUsuarioQualificados.DatPedidoDireito BETWEEN @DataInicio AND @DataFim;

    UPDATE #TUsuario SET  pontosLiquidosPernaEsquerda = pontosPernaEsquerda
    UPDATE #TUsuario SET  pontosLiquidosPernaDireita  = pontosPernaDireita

    /*Adiciona Acumulado na conta*/
    UPDATE 
	    #TUsuario 
    SET 
        pontosPernaEsquerda = pontosPernaEsquerda + (CASE WHEN (RP.ValorPernaEsquerda >= RP.ValorPernaDireita)  
	                                                      THEN RP.ValorPernaEsquerda - RP.ValorPernaDireita 
							                              ELSE 0 
													 END ),
        pontosPernaDireita  = pontosPernaDireita  + (CASE WHEN (RP.ValorPernaDireita > RP.ValorPernaEsquerda) 
	                                                      THEN RP.ValorPernaDireita - RP.ValorPernaEsquerda  
														  ELSE 0 
													 END),
        pontosAcumuladoPernaEsquerda = RP.AcumuladoEsquerda,
        pontosAcumuladoPernadireita  = RP.AcumuladoDireita
    FROM 
        #TUsuario
        INNER JOIN Rede.Posicao RP (NOLOCK) ON #TUsuario.idUsuario = RP.UsuarioID
    WHERE RP.DataFim = (SELECT MAX(DataFim) FROM Rede.Posicao (NOLOCK) )
      
    UPDATE #TUsuario SET pontosPernaDireita           = 0 WHERE pontosPernaDireita IS NULL
    UPDATE #TUsuario SET pontosPernaEsquerda          = 0 WHERE pontosPernaEsquerda IS NULL
    UPDATE #TUsuario SET valorAfiliacaoEsquerda       = 0 WHERE valorAfiliacaoEsquerda IS NULL
    UPDATE #TUsuario SET valorAfiliacaoDireita        = 0 WHERE valorAfiliacaoDireita IS NULL
    UPDATE #TUsuario SET pontosAcumuladoPernaDireita  = 0 WHERE pontosAcumuladoPernaDireita IS NULL
    UPDATE #TUsuario SET pontosAcumuladoPernaEsquerda = 0 WHERE pontosAcumuladoPernaEsquerda IS NULL
            
    -- Calcula o bonus da menor perna com o percentual do nivel de associação correspondente
    UPDATE #TUsuario 
    SET valorBonus = ( CASE 
                           WHEN pontosPernaDireita > pontosPernaEsquerda
                           THEN pontosPernaEsquerda
                           ELSE pontosPernaDireita
                       END ) * (A.PercentualBinario / 100)
    FROM 
        #TUsuario                   T
         INNER JOIN Rede.Associacao A (NOLOCK) ON T.NivelAssociacao = A.Nivel;

    UPDATE #TUsuario SET valorBonus = 0 WHERE valorBonus IS NULL
   
	--If (UPPER(@ControleGanho) = 'TRUE')
	--Begin

	--    DECLARE 
	--        @limiteBinario1 float =  0.2,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 1) , 0.0),
 --           @limiteBinario2 float =  0.6,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 2) , 0.0),
 --           @limiteBinario3 float =  1.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 3) , 0.0),
 --           @limiteBinario4 float =  2.5,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 4) , 0.0),
 --           @limiteBinario5 float =  4.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 5) , 0.0),
 --           @limiteBinario6 float =  8.0,  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 6) , 0.0),
 --           @limiteBinario7 float = 10.0;  -- ISNULL((Select Valor from Rede.AssociacaoLimiteGanho (nolock) where TipoID = 3 and NivelAssociacao = 7) , 0.0);

 --       /*LIMITE DIARIO DE BONUS DE REDE*/
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario1 WHERE valorBonus > @limiteBinario1 AND NivelAssociacao = 1
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario2 WHERE valorBonus > @limiteBinario2 AND NivelAssociacao = 2
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario3 WHERE valorBonus > @limiteBinario3 AND NivelAssociacao = 3
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario4 WHERE valorBonus > @limiteBinario4 AND NivelAssociacao = 4
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario5 WHERE valorBonus > @limiteBinario5 AND NivelAssociacao = 5
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario6 WHERE valorBonus > @limiteBinario5 AND NivelAssociacao = 6
 --       UPDATE #TUsuario  SET valorBonus = @limiteBinario7 WHERE valorBonus > @limiteBinario7 AND NivelAssociacao = 7

 --       -- Obtem o Acumulado da Associacao (Total de Ganho)
 --       UPDATE 
 --           #TUsuario
 --       SET 
 --           acumuladoGanho = UG.AcumuladoGanho       
 --       FROM 
 --           #TUsuario T, 
 --           Usuario.UsuarioGanho UG (NOLOCK)
 --       WHERE 
 --           T.idUsuario = UG.UsuarioID
 --       AND T.dataValidade = UG.DataFim;
   
 --       -- Obtem o Limite de Ganho da Associacao
 --       UPDATE 
 --           #TUsuario
 --       SET 
 --           limiteAssociacao = ALG.Valor
 --       FROM 
 --           #TUsuario T,
 --           Rede.AssociacaoLimiteGanho ALG (NOLOCK)
 --       WHERE 
 --           ALG.NivelAssociacao = T.NivelAssociacao
 --       AND ALG.TipoID = 1; -- Tipo Associacao
     
 --       -- Verifica o Limite de Ganho da Associacao
 --       Update 
 --           #TUsuario 
 --       Set 
 --           valorBonus = (limiteAssociacao - acumuladoGanho) 
 --       Where valorBonus + acumuladoGanho > limiteAssociacao;
 --   End
            
    UPDATE 
        #TUsuario 
    SET 
        pontosAcumuladoPernaEsquerda = pontosAcumuladoPernaEsquerda + pontosLiquidosPernaEsquerda,
        pontosAcumuladoPernaDireita  = pontosAcumuladoPernaDireita  + pontosLiquidosPernaDireita;

   /* Cursor - INSERE NA TABELA DE PONTUACAO, USUARIOS QUE NAO ESTEJAM LÁ*/
   Declare
      @id int,
      @AntFetch int

   Declare
      curLocal
   Cursor For
   Select 
      id
   From
      #TUsuario
   Order by id;

   Open curLocal;

   If (Select @@CURSOR_ROWS) <> 0
   Begin
   	  BEGIN TRANSACTION

      DELETE 
          Rede.Posicao 
      WHERE 
          DataFim = @DataFim;

      Fetch Next From curLocal Into @id;   
      WHILE (@@FETCH_STATUS = 0)
      BEGIN
   --      If Exists(Select 'ok' From Rede.Posicao P (NOLOCK), #TUsuario T Where P.UsuarioID = T.idUsuario and P.DataFim = @DataFim)         
   --      Begin
   --         Delete P
   --         From 
   --            Rede.Posicao P,
   --            #TUsuario T
   --         Where 
   --             P.UsuarioID = T.idUsuario  
			--and P.DataFim = @dataFim  
			--and T.id = @id;
   --      End

         INSERT INTO Rede.Posicao (
		     UsuarioID, 
			 ReferenciaID, 
			 DataInicio, 
			 DataFim,
			 ValorPernaEsquerda, 
			 ValorPernaDireita, 
			 AcumuladoEsquerda, 
			 AcumuladoDireita, 
			 DataCriacao, 
			 ValorDiaPernaEsquerda,
			 ValorDiaPernaDireita,
			 QualificadorEsquerdoID,
			 QualificadorDireitoID
	     )
         SELECT 
            T.idUsuario, 
            0 Referencia,  
            @dataInicio,  
            @dataFim, 
            dbo.TruncZion(T.pontosPernaEsquerda, @QtdeCasas), 
            dbo.TruncZion(T.pontosPernaDireita, @QtdeCasas), 
			dbo.TruncZion(T.pontosAcumuladoPernaesquerda, @QtdeCasas),
			dbo.TruncZion(T.pontosAcumuladoPernadireita, @QtdeCasas),
            dbo.GetDateZion(), 
            dbo.TruncZion(T.pontosLiquidosPernaEsquerda, @QtdeCasas),
            dbo.TruncZion(T.pontosLiquidosPernaDireita, @QtdeCasas) ,
            T.qualificadoresquerdoid,
            T.qualificadordireitoid
         FROM 
            #TUsuario              T 
            LEFT JOIN Rede.Posicao RP ON T.idUsuario = RP.UsuarioID and RP.DataFim = (SELECT MAX(DataFim) FROM Rede.Posicao (NOLOCK))
         WHERE 
             T.id = @id;

         Fetch Next From curLocal Into @id;             
      End -- While

	  COMMIT TRANSACTION
   End -- @@CURSOR_ROWS

   Close curLocal
   Deallocate curLocal

  
   If (@ProcessaBonus = 1)
   Begin
    --  BEGIN TRANSACTION
      
      -- Gera Bonificacao
      INSERT INTO Rede.Bonificacao (
	      CategoriaID,  
          UsuarioID,  
          ReferenciaID,  
          StatusID,  
          Data,  
          Valor,  
          PedidoID,  
          Descricao,  
          RegraItemID,  
          CicloID,  
          ValorCripto
      )
      SELECT 
	      @CategoriaID AS CategoriaID,  
          idUsuario    AS UsuarioID,  
          0            AS ReferenciaID,  
          0            AS StatusID,  
          @dataFim     AS Data,  
          dbo.TruncZion(valorBonus, @QtdeCasas) AS Valor,  
          0   AS PedidoID,                       
		--'[NM1]' + ISNULL( ( SELECT U1.Nome  
        --                    FROM  Rede.Upline_Ciclo UL1 (NOLOCK)  
        --                          INNER JOIN Usuario.Usuario U1 (NOLOCK) ON U1.ID = UL1.Upline  
        --                          INNER JOIN Usuario.Usuario U2 (NOLOCK) ON U2.ID = UL1.Upline  
        --                    WHERE UL1.UsuarioID = T.UsuarioID  
        --                      AND UL1.CicloID = @CicloID  
        --                      AND U2.PatrocinadorDiretoID = UL.Upline
		--                  ) ,  
        --                  ( SELECT Nome  
        --                    FROM   Usuario.Usuario (NOLOCK)  
        --                    WHERE  ID = UL.Upline
		--                  )  
		--	              ) +  
		--'|[N]' + CONVERT(VARCHAR, UL.Nivel) +   
        --'|[P]' + T.Produto +  
        --'|[NP]' + CONVERT(VARCHAR, UL.NivelPago) +   
        --'|[NM]' + U.Login  
           ''    AS Descricao,  
           null  AS RegraItemID,  
           0     AS CicloID,  
           0     AS ValorCripto  
      FROM #TUsuario 
           INNER JOIN vw_UsuariosAtivos ON #TUsuario.idUsuario = vw_UsuariosAtivos.ID
      WHERE valorBonus > 0;
       
   --   If (UPPER(@ControleGanho) = 'TRUE')
	  --Begin
   --       -- Atualiza Total de Ganho de Associacao
   --       UPDATE  
	  --        Usuario.UsuarioGanho
   --       SET 
	  --        AcumuladoGanho =  dbo.TruncZion((UG.AcumuladoGanho + T.valorBonus),8)      
   --       FROM #TUsuario T, 
   --           Usuario.UsuarioGanho UG
   --       WHERE 
	  --        T.idUsuario = UG.UsuarioID
   --       AND T.dataValidade = UG.DataFim
   --       AND T.valorBonus > 0;
	  -- End
       
   --   COMMIT TRANSACTION
   End

   DROP TABLE #TPedidos
   DROP TABLE #TUsuario 
   DROP TABLE #TUsuarioQualificados
       
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



--exec spDG_RE_GeraBonusBinario '20200821',1

