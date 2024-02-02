--EXEC sp_XS_GeraBonusIndicacao '20190701'

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.sp_XS_GeraBonusIndicacao', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_GeraBonusIndicacao];
GO


CREATE PROCEDURE [dbo].[sp_XS_GeraBonusIndicacao]
	@baseDate varchar(8) null
	AS
	BEGIN
	   BEGIN TRY
		  Set NoCount On
		  
		  BEGIN TRANSACTION
		  
		  DECLARE @dataInicio datetime;
		  DECLARE @dataFim    datetime;
		  DECLARE @CategoriaBonusID int, @CategoriaRegraID int ;
		  declare @cotacaoBTCPadrao float;
		  declare @valorLiquidoPagamento float;
		  
		  set @CategoriaBonusID = 13; -- Bônus de Indicação Direta
		  set @CategoriaRegraID = 1 -- Regra de Bônus de Indicação Direta
      
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

		  /*cotacao btc "media" */
			set @cotacaoBTCPadrao = (select avg (cotacao) from (select top 20 CotacaoBTC cotacao from Loja.PedidoPagamento where CotacaoBTC is not null order by ID desc) C) 

		  
		  -- Pedidos pendentes de geração do Bônus de Indicação Direta
		   Create Table #TPedidos
		   (
			  UsuarioID INT,
			  PedidoId INT,
			  NivelAssociacao INT,
			  Kit VarChar(128),
			  DataPedido DateTime,
			  Valor Decimal(18,2),
			  ValorBTC float
		   );
		  
		  Insert Into #TPedidos
		  Select Ped.UsuarioID, Ped.ID, Prd.NivelAssociacao, Prd.Nome, Ped.DataCriacao, Item.BonificacaoUnitaria, 
		  Item.BonificacaoUnitaria / (isnull(PP.CotacaoBTC, @cotacaoBTCPadrao) )
		  From Loja.Pedido Ped (NOLOCK)
		  INNER JOIN Loja.PedidoItem Item (NOLOCK) ON Item.PedidoID = Ped.ID
		  INNER JOIN Loja.PedidoPagamento PP (NOLOCK) ON PP.PedidoID = Ped.ID
		  INNER JOIN Loja.PedidoPagamentoStatus PS (NOLOCK) ON PS.PedidoPagamentoID = PP.ID
		  INNER JOIN Loja.Produto Prd (NOLOCK) ON Prd.ID = Item.ProdutoID
		  LEFT JOIN Rede.Bonificacao Bon (NOLOCK) ON Bon.PedidoID = Ped.ID
													AND Bon.CategoriaID = @CategoriaBonusID
		  WHERE Bon.PedidoID IS NULL
		   AND PS.StatusID = 3      -- somente pedidos pagos  
		   and PS.Data BETWEEN @dataInicio and @dataFim --pega somente pedidos do dia
		   AND (Prd.TipoID = 1 OR Prd.TipoID = 2)-- Produtos de Ativação ou Upgrade

		   
			-- Regra 2 - Indicação Direta
			CREATE TABLE #TIndicacao 
			( 
				BonificacaoId	INT,
				Nivel			INT   
			); 
			
		  DECLARE C_REGRAITEM CURSOR LOCAL FOR
		  SELECT AssociacaoID, Nivel, Regra FROM Rede.RegraItem (NOLOCK)
		  WHERE RegraID = @CategoriaRegraID
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
			IF EXISTS (SELECT 1 FROM #TPedidos WHERE NivelAssociacao = @Nivel)
			BEGIN

				Insert Into Rede.Bonificacao
				  (CategoriaID,
				   UsuarioID,
				   ReferenciaID,
				   StatusID,
				   Data,
				   Valor,
				   ValorBTC,
				   PedidoID)
				OUTPUT inserted.ID, 1 INTO #TIndicacao
			   Select 
				   @CategoriaBonusID as CategoriaID, 
				   Nivel1.ID as Usuario,
				   T.UsuarioID as Referencia,
				   0 as StatusID,
				   T.DataPedido as Data,
					(CONVERT(DECIMAL(18,2), (@Regra)) / 100)  * T.Valor as Valor,
					(CONVERT(DECIMAL(18,2), (@Regra)) / 100) * T.ValorBTC as ValorBTC,
				   T.PedidoId as PedidoID
				FROM #TPedidos T
				INNER JOIN Usuario.Usuario Base (NOLOCK) ON Base.ID = T.UsuarioID
				INNER JOIN Usuario.Usuario Nivel1 (NOLOCK) ON Nivel1.ID = Base.PatrocinadorDiretoID And Nivel1.ID <> Base.ID
				WHERE
					T.NivelAssociacao = @Nivel
					AND Base.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
					AND Base.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
					AND Nivel1.Bloqueado = 0     -- Patrocinador nao esta bloqueado
					AND Nivel1.RecebeBonus = 1 -- Patrocinador recebe bonus
					And Nivel1.StatusID = 2 
					And Nivel1.NivelAssociacao > 0
					/*AND Nivel1.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago							*/
					/*and Nivel1.DataRenovacao >= getdate()*/
						
			END

			FETCH NEXT FROM C_REGRAITEM INTO @AssociacaoID, @Nivel, @Regra
		  END

		  CLOSE C_REGRAITEM
		  DEALLOCATE C_REGRAITEM
		  
		
		/*casas decimais de BTC*/		
	
		DECLARE C_BTC CURSOR LOCAL FOR
			SELECT BonificacaoId, RB.ValorBTC FROM #TIndicacao T inner join Rede.Bonificacao RB on T.BonificacaoId = RB.ID

		DECLARE @BonificacaoID INT, @valorBrutoBTC float, @valorLiquidoPagamentoBTC float

		OPEN C_BTC

		FETCH NEXT FROM C_BTC INTO @BonificacaoID, @valorBrutoBTC
		
		WHILE(@@FETCH_STATUS = 0)
		BEGIN
			exec sp_XS_BitcoinSafeDecimais @valorBrutoBTC, null, @BonificacaoID, @valorLiquidoPagamentoBTC output
			select @valorBrutoBTC
			select @valorLiquidoPagamentoBTC
			update Rede.Bonificacao set ValorBTC = @valorLiquidoPagamentoBTC where ID = @BonificacaoID

			FETCH NEXT FROM C_BTC INTO @BonificacaoID, @valorBrutoBTC
		END

		CLOSE C_BTC
		DEALLOCATE C_BTC

		  
		

		-- Remove todas as tabelas temporárias
		Drop Table #TPedidos;
		Drop Table #TIndicacao;

		COMMIT TRANSACTION

	END TRY

   BEGIN CATCH
	  
		 ROLLBACK TRANSACTION
	  
	  DECLARE @error int, @message varchar(4000), @xstate int;
	  SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
	  RAISERROR ('Erro na execucao de sp_XS_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 


GO
