USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spDG_RE_GeraBonusIndicacao]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
    @UsuarioID	INT,
    @PedidoID	INT,
    @CicloID	INT
AS
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: Gera registros de Bonificacao de indicação e expansão
-- =============================================================================================
BEGIN
	BEGIN TRY
		SET NOCOUNT ON
		
		DECLARE @CategoriaID		INT = 13; -- Bônus de Indicação Direta
		DECLARE @RegraID			INT;
		SET @RegraID = 1;
	  
		-- Pedidos pendentes de geração do Bônus de Indicação Direta
		CREATE TABLE #TPedidos (
			UsuarioID INT,
			PedidoId INT,
			AssociacaoID INT,
			Produto Varchar(128),
			DataPedido DateTime,
			Pontos Decimal(18,2),
			CicloID INT NULL
		);
	  
		INSERT INTO #TPedidos
		SELECT 
			Ped.UsuarioID, 
			Ped.ID, 
			Prd.NivelAssociacao, 
			Prd.Nome, 
			LPagSta.Data,
			Pontos = Item.BonificacaoUnitaria,
			CicloID = RCic.ID
		FROM 
			Loja.Pedido Ped (NOLOCK)
			INNER JOIN Loja.PedidoItem Item (NOLOCK) 
				ON Item.PedidoID = Ped.ID
			INNER JOIN Loja.Produto Prd (NOLOCK) 
				ON Prd.ID = Item.ProdutoID
			LEFT JOIN Rede.Bonificacao Bon (NOLOCK) 
				ON Bon.PedidoID = Ped.ID
				AND Bon.CategoriaID = @CategoriaID
			INNER JOIN Loja.ProdutoValor Val (NOLOCK) 
				ON Val.ProdutoID = Prd.ID
			INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
				ON LPag.PedidoID = Ped.ID
			INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
				ON LPagSta.PedidoPagamentoID = LPag.ID
			INNER JOIN Rede.Ciclo RCic (NOLOCK) 
				ON LPagSta.Data BETWEEN RCic.DataInicial AND RCic.DataFinal
		WHERE 
			Bon.PedidoID IS NULL
			AND LPagSta.StatusID = 3
			AND Prd.TipoID IN (1, 2) -- Produtos de Ativação
			AND (Ped.ID = @PedidoId OR @PedidoId = 0)
			AND (Ped.UsuarioID = @UsuarioId OR @UsuarioId = 0)
			AND Item.BonificacaoUnitaria > 0
			AND RCic.ID = @CicloID;

		SELECT 
			DISTINCT #TPedidos.UsuarioID,
			Upl.Upline,
			DENSE_RANK() OVER (PARTITION BY #TPedidos.UsuarioID ORDER BY Upl.Nivel ASC) AS Nivel,
			CASE WHEN AtvMes.ID IS NOT NULL 
				THEN DENSE_RANK() OVER (PARTITION BY #TPedidos.UsuarioID, CASE WHEN AtvMes.ID IS NOT NULL THEN 1 ELSE 0 END  ORDER BY Upl.Nivel ASC) 
				ELSE NULL
			END AS NivelPago
		INTO #TUpline
		FROM #TPedidos
		INNER JOIN Rede.Upline_Ciclo Upl (NOLOCK) 
			ON Upl.UsuarioID = #TPedidos.UsuarioID
			AND Upl.CicloID = #TPedidos.CicloID
		LEFT JOIN Usuario.AtivacaoMensal AtvMes (NOLOCK)  
			ON AtvMes.UsuarioID = Upl.Upline
			AND AtvMes.CicloID = #TPedidos.CicloID
		WHERE 
			#TPedidos.CicloID = @CicloID;

		-- Regra 1 - Indicação Direta
		DECLARE C_REGRAITEM CURSOR FOR
		SELECT 
			ClassificacaoID, AssociacaoID, Nivel, RegraItem.ID, Regra 
		FROM 
			Rede.RegraItem (NOLOCK)
		WHERE 
			RegraID = @RegraID
			AND Ativo = 1
		ORDER BY 
			AssociacaoID, Nivel DESC;
	  
		DECLARE @ClassificacaoID INT,
				@AssociacaoID INT,
				@Nivel INT,
				@RegraItemId INT,
				@Regra VARCHAR(128);

		OPEN C_REGRAITEM;
		FETCH NEXT FROM C_REGRAITEM INTO @ClassificacaoID, @AssociacaoID, @Nivel, @RegraItemId, @Regra;

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
					 CicloID)
				SELECT 
					@CategoriaID				AS [CategoriaID],
					Upl.Upline					AS [UsuarioID],
					T.UsuarioID					AS [ReferenciaID],
					0							AS [StatusID],
					T.DataPedido				AS [Data],
					CONVERT(DECIMAL, @Regra) / 100 * T.Pontos 	AS [Valor],
					T.PedidoId					AS [PedidoID], 
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
						'|[P]' + T.Produto +
						'|[NP]' + CONVERT(VARCHAR, Upl.NivelPago) + 
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
					(T.AssociacaoID = @AssociacaoID OR @AssociacaoID = 0)
					AND (Clf.ID = @ClassificacaoID OR @ClassificacaoID IS NULL)
					AND Upl.NivelPago IS NOT NULL
					AND Upl.NivelPago = (@Nivel * -1)

				COMMIT TRANSACTION
			END

		FETCH NEXT FROM C_REGRAITEM INTO @ClassificacaoID, @AssociacaoID, @Nivel, @RegraItemId, @Regra
		END

		CLOSE C_REGRAITEM
		DEALLOCATE C_REGRAITEM

		-- Remove todas as tabelas temporárias
		DROP TABLE #TPedidos;

		-- Reduzir valores para pedidos feitos anteriores a 31/11
		IF (@CicloID = 1)
		BEGIN
			UPDATE Rede.Bonificacao 
			SET Valor = 100
			WHERE RegraItemID = 1
				AND CicloID = 1

			UPDATE Financeiro.Lancamento
			SET Valor = 100
			WHERE RegraItemID = 1
				AND CicloID = 1
		END

		-- Promocao de final de ano, multiplicador conforme volume de indicacoes por afiliado
		-- Aplicando primeiro os 50% sobre o valor original, caso tenha mais de 1 pedido de ativaçao
		IF (@CicloID = 2 OR @CicloID = 3)
		BEGIN
			SELECT Bon.UsuarioID,
				Count(Ped.ID) Qtde
			INTO #TPromocao_50
			FROM 
				Rede.Bonificacao (NOLOCK) Bon
				INNER JOIN Usuario.Usuario (NOLOCK) Dir 
					ON Dir.PatrocinadorDiretoID = Bon.UsuarioID
				INNER JOIN Loja.Pedido Ped (NOLOCK)
					ON Ped.UsuarioID = Dir.ID
					AND Ped.ID = Bon.PedidoID
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
			WHERE Bon.RegraItemID = 1
				AND LPagSta.StatusID = 3
				AND Prd.TipoID IN (1,2) -- Produtos de Ativação
				AND Item.BonificacaoUnitaria > 0
				AND RCic.ID = @CicloID
				AND LPagSta.Data BETWEEN '2018-12-15 00:00:000' AND '2019-01-31 23:59:000'
			GROUP BY Bon.UsuarioID;

			DELETE Financeiro.Lancamento
			FROM Financeiro.Lancamento
			INNER JOIN #TPromocao_50
				ON #TPromocao_50.UsuarioID = Financeiro.Lancamento.UsuarioID
			WHERE Financeiro.Lancamento.CicloID = @CicloID
				AND Financeiro.Lancamento.RegraItemID = 1
				AND #TPromocao_50.Qtde > 1

			UPDATE Rede.Bonificacao
			SET RegraItemID = 80, -- Regra dos 50%,
				Valor = Valor * 1.5,
				StatusID = 0
			FROM Rede.Bonificacao
			INNER JOIN #TPromocao_50
				ON #TPromocao_50.UsuarioID = Rede.Bonificacao.UsuarioID
			WHERE Rede.Bonificacao.CicloID = @CicloID
				AND Rede.Bonificacao.RegraItemID = 1
				AND #TPromocao_50.Qtde > 1

			SELECT Bon.UsuarioID,
				Count(Ped.ID) Qtde
			INTO #TPromocao_100
			FROM 
				Rede.Bonificacao (NOLOCK) Bon
				INNER JOIN Usuario.Usuario (NOLOCK) Dir 
					ON Dir.PatrocinadorDiretoID = Bon.UsuarioID
				INNER JOIN Loja.Pedido Ped (NOLOCK)
					ON Ped.UsuarioID = Dir.ID
					AND Ped.ID = Bon.PedidoID
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
			WHERE Bon.RegraItemID = 80
				AND LPagSta.StatusID = 3
				AND Prd.TipoID IN (1,2) -- Produtos de Ativação
				AND Item.BonificacaoUnitaria > 0
				AND RCic.ID = @CicloID
				AND LPagSta.Data BETWEEN '2018-12-15 00:00:000' AND '2019-01-31 23:59:000'
			GROUP BY Bon.UsuarioID;

			DELETE Financeiro.Lancamento
			FROM Financeiro.Lancamento
			INNER JOIN #TPromocao_100
				ON #TPromocao_100.UsuarioID = Financeiro.Lancamento.UsuarioID
			WHERE Financeiro.Lancamento.CicloID = @CicloID
				AND Financeiro.Lancamento.RegraItemID = 80
				AND #TPromocao_100.Qtde > 2

			UPDATE Rede.Bonificacao
			SET RegraItemID = 81, -- Regra dos 100%,
				Valor = Valor / 1.5 * 2,
				StatusID = 0
			FROM Rede.Bonificacao
			INNER JOIN #TPromocao_100
				ON #TPromocao_100.UsuarioID = Rede.Bonificacao.UsuarioID
			WHERE Rede.Bonificacao.CicloID = @CicloID
				AND Rede.Bonificacao.RegraItemID = 80
				AND #TPromocao_100.Qtde > 2
		END

	END TRY
	BEGIN CATCH
      IF @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error INT, @message VARCHAR(4000), @xstate INT;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 
 


GO
