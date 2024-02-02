-- =============================================================================================
-- Author.....:
-- Create date: 
-- Description: Gera registros de Bonificacao de indicação e bonus amizade
-- =============================================================================================
DROP PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
GO
CREATE PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
AS
BEGIN
   BEGIN TRY
      Set NoCount On
	  
	  -- Pedidos pendentes de geração do Bônus de Indicação Direta
	   Create Table #TPedidos
	   (
	      UsuarioID INT,
	      PedidoId INT,
	      AssociacaoID INT,
		  DataPedido DateTime
	   );
	  
	  Insert Into #TPedidos
	  Select Ped.UsuarioID, Ped.ID, Prd.NivelAssociacao, Ped.DataCriacao
	  From Loja.Pedido Ped (NOLOCK)
	  INNER JOIN Loja.PedidoItem Item (NOLOCK) ON Item.PedidoID = Ped.ID
	  INNER JOIN Loja.Produto Prd (NOLOCK) ON Prd.ID = Item.ProdutoID
	  LEFT JOIN Rede.Bonificacao Bon (NOLOCK) ON Bon.PedidoID = Ped.ID
												AND Bon.CategoriaID = 22 -- Bônus de Indicação Direta
	  WHERE Bon.PedidoID IS NULL
	   AND Prd.TipoID = 1 -- Produtos de Ativação

	  -- Regra 2 - Indicação Direta
	  DECLARE C_REGRAITEM CURSOR FOR
	  SELECT AssociacaoID, Nivel, Regra FROM Rede.RegraItem (NOLOCK)
	  WHERE RegraID = 2
	   and Ativo = 1
	  ORDER BY AssociacaoID, Nivel DESC
	  
	  DECLARE @AssociacaoID INT,
			@Nivel INT,
			@Regra VARCHAR(128)

	  OPEN C_REGRAITEM

	  FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @Regra

	  WHILE(@@FETCH_STATUS = 0)
	  BEGIN

		-- Checar se existe pedidos para este nível de associacao e aplicação de regra
		IF EXISTS (SELECT 1 FROM #TPedidos WHERE AssociacaoID = @AssociacaoID)
		BEGIN
			-- Carregar nível de usuário para aplicação da regra
			IF (@Nivel = -1)
			BEGIN
				BEGIN TRANSACTION

			    Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   PedidoID)
			   Select 
				   22 as CategoriaID, -- Bônus de Indicação Direta
				   Nivel1.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
				   CONVERT(DECIMAL, @Regra) as Valor,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID

				COMMIT TRANSACTION
			END

			IF (@Nivel = -2)
			BEGIN
				BEGIN TRANSACTION

			    Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   PedidoID)
			   Select 
				   22 as CategoriaID, -- Bônus de Indicação Direta
				   Nivel2.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
				   CONVERT(DECIMAL, @Regra) as Valor,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
				INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID

				COMMIT TRANSACTION
			END

			IF (@Nivel = -3)
			BEGIN
				BEGIN TRANSACTION

			    Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   PedidoID)
			   Select 
				   22 as CategoriaID, -- Bônus de Indicação Direta
				   Nivel3.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
				   CONVERT(DECIMAL, @Regra) as Valor,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
				INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
				INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID

				COMMIT TRANSACTION
			END

			IF (@Nivel = -4)
			BEGIN
				BEGIN TRANSACTION

			    Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   PedidoID)
			   Select 
				   22 as CategoriaID, -- Bônus de Indicação Direta
				   Nivel4.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
				   CONVERT(DECIMAL, @Regra) as Valor,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
				INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
				INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID
				INNER JOIN Usuario.Usuario Nivel4 (NOLOCK) ON Nivel4.ID = Nivel3.PatrocinadorDiretoID And Nivel4.ID <> Nivel3.ID

				COMMIT TRANSACTION
			END

			IF (@Nivel = -5)
			BEGIN
				BEGIN TRANSACTION

			    Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   PedidoID)
			   Select 
				   22 as CategoriaID, -- Bônus de Indicação Direta
				   Nivel5.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
				   CONVERT(DECIMAL, @Regra) as Valor,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
				INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
				INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID
				INNER JOIN Usuario.Usuario Nivel4 (NOLOCK) ON Nivel4.ID = Nivel3.PatrocinadorDiretoID And Nivel4.ID <> Nivel3.ID
				INNER JOIN Usuario.Usuario Nivel5 (NOLOCK) ON Nivel5.ID = Nivel4.PatrocinadorDiretoID And Nivel5.ID <> Nivel4.ID

				COMMIT TRANSACTION
			END

		END

		FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @Regra
	  END

	  CLOSE C_REGRAITEM
	  DEALLOCATE C_REGRAITEM

	  Insert Into Financeiro.Lancamento
        (UsuarioID,
         ContaID,
         TipoID,
         CategoriaID,
         ReferenciaID,
         Valor,
         Descricao,
         DataCriacao,
         DataLancamento,
         PedidoID)
	   Select
		   B.UsuarioID,
		   1 AS ContaID, -- Dinheiro
		   6 AS TipoID,
		   B.CategoriaID,
		   B.ReferenciaID,
	  	   B.Valor,
		   (C.Nome + ' (' + U.Login + ')') AS Descricao,
		   dbo.GetDateZion() AS DataCriacao,
		   B.Data AS DataLancamento,
           B.PedidoID
	   From Rede.Bonificacao B (nolock)
		     INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
		     INNER JOIN Usuario.Usuario U (nolock) ON U.ID = B.ReferenciaID
			 INNER JOIN #TPedidos P (nolock) ON P.ID = B.PedidoID
	   Where B.StatusID = 1
        and B.CategoriaID IN (22) 

      -- Remove todas as tabelas temporárias
      Drop Table #TPedidos;

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

