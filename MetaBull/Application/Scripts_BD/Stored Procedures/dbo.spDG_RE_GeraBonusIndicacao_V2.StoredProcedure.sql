USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spDG_RE_GeraBonusIndicacao_V2]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
	CREATE PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao_V2]
		@baseDate varchar(8) null
	AS
	BEGIN
	   BEGIN TRY
		  Set NoCount On
		  
		  DECLARE @dataInicio datetime;
		  DECLARE @dataFim    datetime;
      
		  if (@baseDate is null)
		  Begin 
			 SET @dataInicio = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2);
			 SET @dataFim    = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);
		  End
		  Else
		  Begin
			 SET @dataInicio = CAST(@baseDate + ' 00:00:00' as datetime2);
			 SET @dataFim    = CAST(@baseDate + ' 23:59:59' as datetime2);
		  End

		  -- Pedidos pendentes de geração do Bônus de Indicação Direta
		   Create Table #TPedidos
		   (
			  UsuarioID INT,
			  PedidoId INT,
			  AssociacaoID INT,
			  Kit VarChar(128),
			  DataPedido DateTime
		   );
		  
		  Insert Into #TPedidos
		  Select Ped.UsuarioID, Ped.ID, Prd.NivelAssociacao, Prd.Nome, Ped.DataCriacao
		  From Loja.Pedido Ped (NOLOCK)
		  INNER JOIN Loja.PedidoItem Item (NOLOCK) ON Item.PedidoID = Ped.ID
		  INNER JOIN Loja.PedidoPagamento PP (NOLOCK) ON PP.PedidoID = Ped.ID
		  INNER JOIN Loja.PedidoPagamentoStatus PS (NOLOCK) ON PS.PedidoPagamentoID = PP.ID
		  INNER JOIN Loja.Produto Prd (NOLOCK) ON Prd.ID = Item.ProdutoID
		  LEFT JOIN Rede.Bonificacao Bon (NOLOCK) ON Bon.PedidoID = Ped.ID
													AND Bon.CategoriaID = 13 -- Bônus de Indicação Direta
		  WHERE Bon.PedidoID IS NULL
		   AND PS.StatusID = 3      -- somente pedidos pagos  
		   and PS.Data BETWEEN @dataInicio and @dataFim --pega somente pedidos do dia
		   AND Prd.TipoID = 1 -- Produtos de Ativação

		-- Regra 2 - Indicação Direta
		CREATE TABLE #TIndicacao 
		( 
			BonificacaoId	INT,
			Nivel			INT   
		); 
		  
		  DECLARE C_REGRAITEM CURSOR FOR
		  SELECT AssociacaoID, Nivel, Regra FROM Rede.RegraItem (NOLOCK)
		  WHERE RegraID = 1
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
					OUTPUT inserted.ID, 1 INTO #TIndicacao
				   Select 
					   13 as CategoriaID, -- Bônus de Indicação Direta
					   Nivel1.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel1.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel1.NivelAssociacao = 2) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.25, 2)
					   ELSE
							CONVERT(DECIMAL(18,2), @Regra) END as Valor,
					   T.PedidoId as PedidoID
					FROM #TPedidos T
					INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
					INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel1.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel1.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel1.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
				
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
					OUTPUT inserted.ID, 2 INTO #TIndicacao
				   Select 
					   13 as CategoriaID, -- Bônus de Indicação Direta
					   Nivel2.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel2.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel2.NivelAssociacao = 2) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.25, 2)
					   ELSE
							CONVERT(DECIMAL(18,2), @Regra) END as Valor,
					   T.PedidoId as PedidoID
					FROM #TPedidos T
					INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
					INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
					INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel2.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel2.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel2.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
						
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
					OUTPUT inserted.ID, 3 INTO #TIndicacao
				   Select 
					   13 as CategoriaID, -- Bônus de Indicação Direta
					   Nivel3.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel3.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel3.NivelAssociacao = 2) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.25, 2)
					   ELSE
							CONVERT(DECIMAL(18,2), @Regra) END as Valor,
					   T.PedidoId as PedidoID
					FROM #TPedidos T
					INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
					INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
					INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
					INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel3.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel3.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel3.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
						
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
					OUTPUT inserted.ID, 4 INTO #TIndicacao
				   Select 
					   13 as CategoriaID, -- Bônus de Indicação Direta
					   Nivel4.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel4.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel4.NivelAssociacao = 2) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.25, 2)
					   ELSE
							CONVERT(DECIMAL(18,2), @Regra) END as Valor,
					   T.PedidoId as PedidoID
					FROM #TPedidos T
					INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
					INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
					INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
					INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID
					INNER JOIN Usuario.Usuario Nivel4 (NOLOCK) ON Nivel4.ID = Nivel3.PatrocinadorDiretoID And Nivel4.ID <> Nivel3.ID
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel4.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel4.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel4.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
						
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
					OUTPUT inserted.ID, 5 INTO #TIndicacao
				   Select 
					   13 as CategoriaID, -- Bônus de Indicação Direta
					   Nivel5.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel5.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel5.NivelAssociacao = 2) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.25, 2)
					   ELSE
							CONVERT(DECIMAL(18,2), @Regra) END as Valor,
					   T.PedidoId as PedidoID
					FROM #TPedidos T
					INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
					INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
					INNER JOIN Usuario.Usuario Nivel2 (NOLOCK) ON Nivel2.ID = Nivel1.PatrocinadorDiretoID And Nivel2.ID <> Nivel1.ID
					INNER JOIN Usuario.Usuario Nivel3 (NOLOCK) ON Nivel3.ID = Nivel2.PatrocinadorDiretoID And Nivel3.ID <> Nivel2.ID
					INNER JOIN Usuario.Usuario Nivel4 (NOLOCK) ON Nivel4.ID = Nivel3.PatrocinadorDiretoID And Nivel4.ID <> Nivel3.ID
					INNER JOIN Usuario.Usuario Nivel5 (NOLOCK) ON Nivel5.ID = Nivel4.PatrocinadorDiretoID And Nivel5.ID <> Nivel4.ID
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel5.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel5.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel5.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
					
					COMMIT TRANSACTION
				END

			END

			FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @Regra
		  END

		  CLOSE C_REGRAITEM
		  DEALLOCATE C_REGRAITEM

		  -- Remove todas as tabelas temporárias
		  Drop Table #TPedidos;
		  Drop Table #TIndicacao;

	   END TRY

	   BEGIN CATCH
		  If @@Trancount > 0
			 ROLLBACK TRANSACTION
	      
		  DECLARE @error int, @message varchar(4000), @xstate int;
		  SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		  RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao_V2: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	   END CATCH
	END 


GO
