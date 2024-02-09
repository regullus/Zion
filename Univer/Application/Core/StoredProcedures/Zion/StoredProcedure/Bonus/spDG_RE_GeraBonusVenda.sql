Use Zion
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusVenda'))
   Drop Procedure spDG_RE_GeraBonusVenda
go

Create PROCEDURE [dbo].spDG_RE_GeraBonusVenda
    @UsuarioID	INT = null,
    @PedidoID	INT = null

AS
-- =============================================================================================
-- Author.....: 
-- Create date: 18/01/2019
-- Description: Gera registros de Bonificacao de Venda
-- =============================================================================================
BEGIN
	BEGIN TRY
        SET NOCOUNT ON;

		DECLARE 
		   @CategoriaID	INT = 13, -- Bônus de Venda
		   @RegraID		INT = 2 ,
		   @CicloID     INT = (SELECT ID FROM Rede.Ciclo (NOLOCK) WHERE Ativo = 1);

        -- Pedidos pendentes de geração do Bônus de Venda
        CREATE TABLE #TPedidos (
            UsuarioID			INT,
            PedidoID			INT,
            DataPedido			DATETIME,
            CicloID				INT NULL,
            BonificacaoUnitaria DECIMAL(18, 2)
        );

        INSERT INTO 
		    #TPedidos
        SELECT 
			P.UsuarioID,
            P.ID,
            P.DataCriacao,
            CicloID = C.ID,
            PI.BonificacaoUnitaria
        FROM 
			Loja.Pedido                           P   (NOLOCK)
            INNER JOIN Loja.PedidoItem            PI  (NOLOCK) ON PI.PedidoID = P.ID
            INNER JOIN Loja.Produto               PR  (NOLOCK) ON PR.ID = PI.ProdutoID
			--INNER JOIN Loja.ProdutoValor          Val (NOLOCK) ON Val.ProdutoID = PR.ID
			INNER JOIN Loja.PedidoPagamento       PP  (NOLOCK) ON PP.PedidoID = P.ID
			INNER JOIN Loja.PedidoPagamentoStatus PPS (NOLOCK) ON PPS.PedidoPagamentoID = PP.ID
			INNER JOIN Rede.Ciclo                 C   (NOLOCK) ON PPS.Data BETWEEN C.DataInicial AND C.DataFinal
            LEFT  JOIN Rede.Bonificacao           B   (NOLOCK) ON B.PedidoID = P.ID  AND B.CategoriaID = @CategoriaID
        WHERE 
			B.PedidoID IS NULL
		AND PPS.StatusID = 3
        AND PR.TipoID IN (3, 4) -- Produtos de Recompra
	    AND P.ID = COALESCE(@PedidoId ,P.ID)
		AND P.UsuarioID = COALESCE(@UsuarioId,P.UsuarioID)
        AND PI.BonificacaoUnitaria > 0
        AND C.ID = @CicloID;


        SELECT DISTINCT
            T.UsuarioID,
            UC.Upline,
            DENSE_RANK() OVER (PARTITION BY T.UsuarioID ORDER BY UC.Nivel ASC) AS Nivel,
			CASE WHEN AM.ID IS NOT NULL 
				THEN DENSE_RANK() OVER (PARTITION BY T.UsuarioID, CASE WHEN AM.ID Is Not Null THEN 1 ELSE 0 END  ORDER BY UC.Nivel ASC) 
				ELSE NULL
			END AS NivelPago
        INTO 
		    #TUpline
        FROM 
			#TPedidos                         T
            INNER JOIN Rede.Upline_Ciclo      UC (NOLOCK) ON UC.UsuarioID = T.UsuarioID AND UC.CicloID = T.CicloID
            LEFT  JOIN Usuario.AtivacaoMensal AM (NOLOCK) ON AM.UsuarioID = UC.Upline AND AM.CicloID = T.CicloID
        WHERE 
			T.CicloID = @CicloID;


	
        DECLARE C_REGRAITEM CURSOR FOR
        SELECT 
			ClassificacaoID, Nivel, RegraItem.ID, Regra
        FROM 
			Rede.RegraItem (NOLOCK)
        WHERE 
			RegraID = @RegraID
        AND Ativo = 1
        ORDER BY 
			ClassificacaoID,
            Nivel DESC;

        DECLARE 
		   @ClassificacaoID	INT,
           @Nivel			INT,
           @RegraItemID		INT,
           @Regra			VARCHAR(128);

        OPEN C_REGRAITEM;
        FETCH NEXT FROM C_REGRAITEM INTO @ClassificacaoID, @Nivel, @RegraItemID, @Regra;

        WHILE (@@FETCH_STATUS = 0)
        BEGIN
            BEGIN TRANSACTION;

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
                @CategoriaID				AS CategoriaID,
                TU.Upline					AS UsuarioID,
                TP.UsuarioID				AS ReferenciaID,
                0							AS StatusID,
                TP.DataPedido				AS Data, 
                CONVERT(DECIMAL(18, 2), CONVERT(DECIMAL(18, 2), REPLACE(@Regra, '%', '')) / 100 * TP.BonificacaoUnitaria) AS Valor,
                PedidoID					AS PedidoID,					
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
			 --	            ) +  
			   '|[N]'  + CONVERT(VARCHAR, TU.Nivel) + 
			 --'|[NP]' + CONVERT(VARCHAR, TU.NivelPago) + 
			   '|[PT]' + CONVERT(VARCHAR, CONVERT(INT, ROUND(TP.BonificacaoUnitaria, 0))) +
			   '|[%]'  + @Regra +
		       '|[NM]' + U.Login
											AS Descricao,
                @RegraItemId				AS RegraItemID,
                TP.CicloID					AS CicloID,
				0                           AS ValorCripto
            FROM 
				#TPedidos                               TP
                INNER JOIN Usuario.Usuario              U  (NOLOCK) ON U.ID = TP.UsuarioID
                INNER JOIN #TUpline                     TU          ON TU.UsuarioID = TP.UsuarioID
                LEFT  JOIN Usuario.UsuarioClassificacao UC (NOLOCK) ON UC.UsuarioID = TU.Upline AND UC.CicloID = TP.CicloID
                INNER JOIN Rede.Classificacao           C  (NOLOCK) ON C.Nivel = ISNULL(UC.NivelClassificacao, 0)
            WHERE 
				 C.ID = @ClassificacaoID
			AND TU.NivelPago Is Not Null
            AND TU.NivelPago = (@Nivel * -1);

            COMMIT TRANSACTION;

            FETCH NEXT FROM C_REGRAITEM INTO @ClassificacaoID, @Nivel, @RegraItemID, @Regra;
        END;

        CLOSE C_REGRAITEM;
        DEALLOCATE C_REGRAITEM;

        -- Remove todas as tabelas temporárias
        DROP TABLE #TPedidos;
    END TRY
    BEGIN CATCH
        IF @@Trancount > 0
            ROLLBACK TRANSACTION;

        DECLARE @error INT, @message VARCHAR(4000), @xstate INT;
        SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
        RAISERROR('Erro na execucao de spDG_RE_GeraBonusVenda: %d: %s', 16, 1, @error, @message) WITH SETERROR;
    END CATCH;
END; 

go
   Grant Exec on spDG_RE_GeraBonusVenda To public
go
