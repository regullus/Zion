Use Zion
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusIndicacao'))
   Drop Procedure spDG_RE_GeraBonusIndicacao
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
    @UsuarioID	INT = null,
    @PedidoID	INT = null

AS
-- =============================================================================================
-- Author.....: 
-- Create date: 17/08/2020
-- Description: Gera registros de Bonificacao de indicação Direta
-- =============================================================================================
BEGIN
    BEGIN TRY  
        SET NOCOUNT ON  
    
        DECLARE   
            @CategoriaID INT = 11, -- Bônus de Indicação Direta  
            @RegraID     INT = 1 ,   
            @CicloID     INT = (SELECT ID FROM Rede.Ciclo (NOLOCK) WHERE Ativo = 1);  
  
     
        -- Pedidos pendentes de geração do Bônus de Indicação Direta  
        CREATE TABLE #TPedidos (  
            UsuarioID    INT,  
            PedidoId     INT,  
            AssociacaoID INT,  
            Produto      Varchar(128),  
            DataPedido   DateTime,  
            Pontos       Decimal(18,2),  
            CicloID      INT NULL  
        );  
     
        INSERT INTO #TPedidos  
        SELECT   
            P.UsuarioID,   
            P.ID,   
            Pr.NivelAssociacao,   
            Pr.Nome,   
            P.DataCriacao,  
            Pontos = PI.BonificacaoUnitaria,  
            CicloID = C.ID  
        FROM   
            Loja.Pedido                           P       (NOLOCK)  
            INNER JOIN Loja.PedidoItem            PI      (NOLOCK) ON PI.PedidoID = P.ID  
            INNER JOIN Loja.Produto               PR      (NOLOCK) ON PR.ID = PI.ProdutoID  
            LEFT  JOIN Rede.Bonificacao           B       (NOLOCK) ON B.PedidoID = P.ID AND B.CategoriaID = @CategoriaID  
            INNER JOIN Loja.PedidoPagamento       PP      (NOLOCK) ON PP.PedidoID = P.ID  
            INNER JOIN Loja.PedidoPagamentoStatus PPS     (NOLOCK) ON PPS.PedidoPagamentoID = PP.ID   
            INNER JOIN Rede.Ciclo                 C       (NOLOCK) ON PPS.Data BETWEEN C.DataInicial AND C.DataFinal  
        WHERE   
            B.PedidoID IS NULL  
        AND PPS.StatusID = 3  
        AND Pr.TipoID IN (1, 2) -- Produtos de Ativação  
        AND P.ID = COALESCE(@PedidoId ,P.ID)  
        AND P.UsuarioID = COALESCE(@UsuarioId,P.UsuarioID)  
        AND PI.BonificacaoUnitaria > 0  
        AND C.ID = @CicloID;  
  
        SELECT DISTINCT 
           P.UsuarioID,  
           UC.Upline,  
           DENSE_RANK() OVER (PARTITION BY P.UsuarioID ORDER BY UC.Nivel ASC) AS Nivel,  
           CASE WHEN AM.ID IS NOT NULL   
               THEN DENSE_RANK() OVER (PARTITION BY P.UsuarioID, CASE WHEN AM.ID IS NOT NULL THEN 1 ELSE 0 END  ORDER BY UC.Nivel ASC)   
               ELSE NULL  
           END AS NivelPago  
        INTO #TUpline  
        FROM   
            #TPedidos  P
            INNER JOIN Rede.Upline_Ciclo      UC (NOLOCK) ON UC.UsuarioID = P.UsuarioID  AND UC.CicloID = P.CicloID  
            LEFT  JOIN Usuario.AtivacaoMensal AM (NOLOCK) ON AM.UsuarioID = UC.Upline AND AM.CicloID = P.CicloID  
        WHERE   
            P.CicloID = @CicloID;  
  
        -- Regra 1 - Indicação Direta  
        DECLARE C_REGRAITEM CURSOR FOR  
        SELECT    
            AssociacaoID, Nivel, RegraItem.ID, Regra   
        FROM   
            Rede.RegraItem (NOLOCK)  
        WHERE   
            RegraID = @RegraID  
        AND Ativo = 1  
        ORDER BY   
            AssociacaoID,   
            Nivel DESC;  
     
        DECLARE   
            @AssociacaoID INT,  
            @Nivel INT,  
            @RegraItemId INT,  
            @Regra VARCHAR(128);  
  
        OPEN C_REGRAITEM;  
        FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @RegraItemId, @Regra;  
  
        WHILE(@@FETCH_STATUS = 0)  
        BEGIN  
            -- Checar se existe pedidos para este nível de associacao e aplicação de regra  
            IF EXISTS (SELECT 1 FROM #TPedidos WHERE AssociacaoID = @AssociacaoID OR @AssociacaoID = 0)  
            BEGIN  
                BEGIN TRANSACTION  
  
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
                    UL.Upline    AS UsuarioID,  
                    T.UsuarioID  AS ReferenciaID,  
                    0            AS StatusID,  
                    T.DataPedido AS Data,  
                    CONVERT(DECIMAL, @Regra) / 100 * T.Pontos AS Valor,  
                    T.PedidoId   AS PedidoID,                       
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
					'|[N]' + CONVERT(VARCHAR, UL.Nivel) +   
                    '|[P]' + T.Produto +  
                  --'|[NP]' + CONVERT(VARCHAR, UL.NivelPago) +   
                    '|[NM]' + U.Login  
                                  AS Descricao,  
                    @RegraItemId  AS RegraItemID,  
                    T.CicloID     AS CicloID,  
                    0             AS ValorCripto  
                FROM   
                    #TPedidos                  T  
                    INNER JOIN Usuario.Usuario U (NOLOCK) ON U.ID = T.UsuarioID  
                    INNER JOIN #TUpline        UL         ON UL.UsuarioID = T.UsuarioID  
                WHERE   
                    (T.AssociacaoID = @AssociacaoID OR @AssociacaoID = 0)  
                AND UL.NivelPago IS NOT NULL  
                AND UL.NivelPago = (@Nivel * -1)  
  
            COMMIT TRANSACTION  
        END  
  
        FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @RegraItemId, @Regra  
    END  
  
    CLOSE C_REGRAITEM  
    DEALLOCATE C_REGRAITEM  
  
    -- Remove todas as tabelas temporárias  
    DROP TABLE #TPedidos;  
  
 END TRY  
 BEGIN CATCH  
      IF @@Trancount > 0  
         ROLLBACK TRANSACTION  
        
      DECLARE @error INT, @message VARCHAR(4000), @xstate INT;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END   
 
go
   Grant Exec on spDG_RE_GeraBonusIndicacao To public
go
