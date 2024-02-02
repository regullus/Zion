use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusMovimento'))
   Drop Procedure spDG_RE_GeraBonusMovimento
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusMovimento]
    @UsuarioID	INT,
    @PedidoID	INT,
    @CicloID	INT
AS
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: Gera registros de Bonificacao de Movimento
-- =============================================================================================
BEGIN
	BEGIN TRY
        SET NOCOUNT ON;

		DECLARE @CategoriaID	INT = 14; -- Bônus de Movimento
		DECLARE @RegraID		INT;

		SET @RegraID = 2;

        -- Pedidos pendentes de geração do Bônus de Movimento
        CREATE TABLE #TPedidos (
            UsuarioID			INT,
            PedidoID			INT,
            DataPedido			DATETIME,
            CicloID				INT NULL,
            BonificacaoUnitaria DECIMAL(18, 2)
        );

        INSERT INTO #TPedidos
        SELECT 
			Ped.UsuarioID,
            Ped.ID,
            Ped.DataCriacao,
            CicloID = RCic.ID,
            Item.BonificacaoUnitaria
        FROM 
			Loja.Pedido Ped (NOLOCK)
            INNER JOIN Loja.PedidoItem Item (NOLOCK)
                ON Item.PedidoID = Ped.ID
            INNER JOIN Loja.Produto Prd (NOLOCK)
                ON Prd.ID = Item.ProdutoID
			INNER JOIN Loja.ProdutoValor Val (NOLOCK) 
				ON Val.ProdutoID = Prd.ID
			INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
				ON LPag.PedidoID = Ped.ID
			INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
				ON LPagSta.PedidoPagamentoID = LPag.ID
			INNER JOIN Rede.Ciclo RCic (NOLOCK) 
				ON LPagSta.Data BETWEEN RCic.DataInicial AND RCic.DataFinal
            LEFT JOIN Rede.Bonificacao Bon (NOLOCK)
                ON Bon.PedidoID = Ped.ID
                AND Bon.CategoriaID = @CategoriaID
        WHERE 
			Bon.PedidoID IS NULL
			AND LPagSta.StatusID = 3
            AND Prd.TipoID IN (3, 4) -- Produtos de Recompra
            AND (Ped.ID = @PedidoID OR @PedidoID = 0)
            AND (Ped.UsuarioID = @UsuarioID OR @UsuarioID = 0)
            AND Item.BonificacaoUnitaria > 0
            AND RCic.ID = @CicloID;

        --/*agrupa pedidos, para reduzir 200 pts de quem cumpriu atividade mensal */
        --CREATE TABLE #TPedidosAgrupados (
        --    UsuarioID					INT,
        --    Data						DATETIME,
        --    CicloID						INT NULL,
        --    BonificacaoUnitariaOriginal DECIMAL(18, 2),
        --    BonificacaoUnitariaPagar	DECIMAL(18, 2)
        --);

   --     INSERT INTO #TPedidosAgrupados
   --     SELECT 
			--UsuarioID,
   --         MIN(DataPedido),
   --         @CicloId,
   --         SUM(BonificacaoUnitaria),
			--/*redutor: quem cumpriu ativo mensal, os 200 pts nao podem ser concedidos novamente em bonus. reduzir. Quem nao atingiu os 200 pts, então vale o valor TOTAL.*/
   --         CASE WHEN SUM(BonificacaoUnitaria) >= 200 AND @CicloID < 14
			--	THEN SUM(BonificacaoUnitaria) - 200
			--	ELSE SUM(BonificacaoUnitaria)
			--END
   --     FROM 
			--#TPedidos
   --     GROUP BY 
			--UsuarioID;

        SELECT DISTINCT
            #TPedidos.UsuarioID,
            Upl.Upline,
            DENSE_RANK() OVER (PARTITION BY #TPedidos.UsuarioID ORDER BY Upl.Nivel ASC) AS Nivel,
			CASE WHEN AtvMes.ID IS NOT NULL 
				THEN DENSE_RANK() OVER (PARTITION BY #TPedidos.UsuarioID, CASE WHEN AtvMes.ID IS NOT NULL THEN 1 ELSE 0 END  ORDER BY Upl.Nivel ASC) 
				ELSE NULL
			END AS NivelPago
        INTO #TUpline
        FROM 
			#TPedidos
            INNER JOIN Rede.Upline_Ciclo (NOLOCK) Upl
                ON Upl.UsuarioID = #TPedidos.UsuarioID
				AND Upl.CicloID = #TPedidos.CicloID
            LEFT JOIN Usuario.AtivacaoMensal (NOLOCK) AtvMes
                ON AtvMes.UsuarioID = Upl.Upline
                AND AtvMes.CicloID = #TPedidos.CicloID
        WHERE 
			#TPedidos.CicloID = @CicloID;

        -- Regra 2 - Movimento
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

        DECLARE @ClassificacaoID	INT,
                @Nivel				INT,
                @RegraItemID		INT,
                @Regra				VARCHAR(128);

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
				CicloID
            )
            SELECT
                @CategoriaID				AS [CategoriaID],
                Upl.Upline					AS [UsuarioID],
                T.UsuarioID					AS [ReferenciaID],
                0							AS [StatusID],
                T.DataPedido				AS [Data],
                CONVERT(DECIMAL(18, 2),
					CONVERT(DECIMAL(18, 2), REPLACE(@Regra, '%', '')) / 100 * T.BonificacaoUnitaria
                )							AS [Valor],
                PedidoID					AS [PedidoID],
					--'[NM1]' + 
					--	(
					--		ISNULL(
					--			(SELECT UsuN1.Nome
					--			FROM 
					--				Rede.Upline_Ciclo UplN1 (NOLOCK)
					--				INNER JOIN Usuario.Usuario UsuN1 (NOLOCK)
					--					ON UsuN1.ID = UplN1.Upline
					--				INNER JOIN Usuario.Usuario Pat (NOLOCK) 
					--					ON Pat.ID = UplN1.Upline
					--			WHERE
					--				UplN1.UsuarioID = T.UsuarioID
					--				AND UplN1.CicloID = @CicloID
					--				AND Pat.PatrocinadorDiretoID = Upl.Upline),
					--			(SELECT Nome
					--			FROM
					--				Usuario.Usuario (NOLOCK)
					--			WHERE 
					--				ID = Upl.Upline)
					--		)
					--	) +
					'|[N]' + CONVERT(VARCHAR, Upl.Nivel) + 
					--'|[NP]' + CONVERT(VARCHAR, Upl.NivelPago) + 
					'|[PT]' + CONVERT(VARCHAR, CONVERT(INT, ROUND(T.BonificacaoUnitaria, 0))) +
					'|[%]' + @Regra +
					'|[NM]' + Base.Login
											AS [Descricao],
                @RegraItemId				AS [RegraItemID],
                T.CicloID					AS [CicloID]
            FROM 
				#TPedidos T
                INNER JOIN Usuario.Usuario Base (NOLOCK) 
					ON Base.ID = T.UsuarioID
                INNER JOIN #TUpline Upl
                    ON Upl.UsuarioID = T.UsuarioID
                LEFT JOIN Usuario.UsuarioClassificacao UsrClf (NOLOCK)
                    ON UsrClf.UsuarioID = Upl.Upline
                    AND UsrClf.CicloID = T.CicloID
                INNER JOIN Rede.Classificacao Clf (NOLOCK)
                    ON Clf.Nivel = ISNULL(UsrClf.NivelClassificacao, 0)
            WHERE 
				Clf.ID = @ClassificacaoID
				AND Upl.NivelPago IS NOT NULL
                AND Upl.NivelPago = (@Nivel * -1);

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
        RAISERROR('Erro na execucao de spDG_RE_GeraBonusProdutividade: %d: %s', 16, 1, @error, @message) WITH SETERROR;
    END CATCH;
END; 

go
   Grant Exec on spDG_RE_GeraBonusMovimento To public
go

