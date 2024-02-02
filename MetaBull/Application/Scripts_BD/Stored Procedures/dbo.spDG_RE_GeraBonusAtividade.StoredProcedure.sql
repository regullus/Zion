USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spDG_RE_GeraBonusAtividade]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
	CREATE PROCEDURE [dbo].[spDG_RE_GeraBonusAtividade]
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

		  Declare @QtdeMinDiretos INT = ISNULL((Select convert(INT, Dados) From Sistema.Configuracao (nolock) Where Chave = 'REDE_QTDE_DIRETOS_BONUS_ADICIONAL') , 0);

         
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
													AND Bon.CategoriaID = 14 -- Bônus Atividade
		  WHERE Bon.PedidoID IS NULL
		   AND PS.StatusID = 3      -- somente pedidos pagos  
		   -- AND PS.Data BETWEEN @dataInicio and @dataFim --pega somente pedidos do dia
		   AND Prd.TipoID IN (1, 2, 3, 4, 5) -- Produtos de Atividade


		 -- Tabela temporária com usuários Qualificados para o pagamento do bonus
         Create Table #TUsuarioDiretos
         (	     
            UsuarioID INT,
            QtdeDiretos INT
         );

         -- Obtem o total de diretos ativos do patrocinador 
         Insert Into #TUsuarioDiretos
         Select 
            Base.ID as UsuarioID,
            count(U.ID) as QtdeDiretos
         from Usuario.Usuario Base (nolock),
              Usuario.Usuario U (nolock)
         Where Base.ID = U.PatrocinadorDiretoID      
           and U.DataValidade >= @dataInicio
           and U.GeraBonus = 1
           and U.DataAtivacao < @dataInicio
         Group by Base.ID;

		-- Regra 2 - Atividade
		CREATE TABLE #TIndicacao 
		( 
			BonificacaoId	INT,
			Nivel			INT   
		); 
		  
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
					OUTPUT inserted.ID, 1 INTO #TIndicacao
				   Select 
					   14 as CategoriaID, -- Bônus de Atividade
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
					   14 as CategoriaID, -- Bônus de Atividade
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
					   14 as CategoriaID, -- Bônus de Atividade
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
					   14 as CategoriaID, -- Bônus de Atividade
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
					INNER JOIN #TUsuarioDiretos Diretos (NOLOCK) ON Diretos.UsuarioID = Nivel4.ID--  and Diretos.QtdeDiretos >= @QtdeMinDiretos
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
					   14 as CategoriaID, -- Bônus de Atividade
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
					INNER JOIN #TUsuarioDiretos Diretos (NOLOCK) ON Diretos.UsuarioID = Nivel5.ID and Diretos.QtdeDiretos >= @QtdeMinDiretos
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel5.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel5.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel5.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
					
					COMMIT TRANSACTION
				END

				IF (@Nivel = -6)
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
					OUTPUT inserted.ID, 6 INTO #TIndicacao
				   Select 
					   14 as CategoriaID, -- Bônus de Atividade
					   Nivel6.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel6.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel6.NivelAssociacao = 2) THEN
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
					INNER JOIN Usuario.Usuario Nivel6 (NOLOCK) ON Nivel6.ID = Nivel5.PatrocinadorDiretoID And Nivel6.ID <> Nivel5.ID
					INNER JOIN #TUsuarioDiretos Diretos (NOLOCK) ON Diretos.UsuarioID = Nivel6.ID and Diretos.QtdeDiretos >= @QtdeMinDiretos
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel6.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel6.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel6.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
					
					COMMIT TRANSACTION
				END

				IF (@Nivel = -7)
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
					OUTPUT inserted.ID, 7 INTO #TIndicacao
				   Select 
					   14 as CategoriaID, -- Bônus de Atividade
					   Nivel7.ID as Usuario,
					   T.UsuarioID as Referencia,
					   0 as StatusID,
					   T.DataPedido as Data,
					   CASE WHEN (Nivel7.NivelAssociacao = 3) THEN
							ROUND(CONVERT(DECIMAL(18,2), @Regra) * 0.5, 2)
						    WHEN (Nivel7.NivelAssociacao = 2) THEN
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
					INNER JOIN Usuario.Usuario Nivel6 (NOLOCK) ON Nivel6.ID = Nivel5.PatrocinadorDiretoID And Nivel6.ID <> Nivel5.ID
					INNER JOIN Usuario.Usuario Nivel7 (NOLOCK) ON Nivel7.ID = Nivel6.PatrocinadorDiretoID And Nivel7.ID <> Nivel6.ID
					INNER JOIN #TUsuarioDiretos Diretos (NOLOCK) ON Diretos.UsuarioID = Nivel7.ID and Diretos.QtdeDiretos >= @QtdeMinDiretos
					WHERE
						T.AssociacaoID = @AssociacaoID
						AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
						AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
						AND Nivel7.Bloqueado = 0     -- Patrocinador nao esta bloqueado
						AND Nivel7.RecebeBonus = 1 -- Patrocinador recebe bonus
						AND Nivel7.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
					
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
