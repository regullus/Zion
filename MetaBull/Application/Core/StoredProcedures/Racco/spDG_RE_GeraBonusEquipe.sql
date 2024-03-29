-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Gera registros de Bonificacao sobre equipe
-- =============================================================================================
DROP PROCEDURE [dbo].[spDG_RE_GeraBonusEquipe]
GO
CREATE PROCEDURE [dbo].[spDG_RE_GeraBonusEquipe]
   @UsuarioId	int,
   @PedidoId	int
AS
BEGIN
   BEGIN TRY
      SET NOCOUNT ON
      
	  DECLARE @Kit VarChar(128)
	  DECLARE @DataPedido DateTime
	  DECLARE @VlrBonus Decimal
	  
      -- Tabela buscar dado do pedido do usuário
      SELECT @Kit = Prd.Nome,
	    @DataPedido = Ped.DataCriacao,
	    @VlrBonus = Vlr.Bonificacao
	  FROM Loja.Pedido Ped (NOLOCK)
		INNER JOIN Loja.PedidoItem Item (NOLOCK) ON Item.PedidoID = Ped.ID
		INNER JOIN Loja.Produto Prd (NOLOCK) ON Prd.ID = Item.ProdutoID
		INNER JOIN Loja.ProdutoValor Vlr (NOLOCK) ON Vlr.ProdutoID = Prd.ID
	  WHERE Ped.UsuarioID = @UsuarioId
	   AND Ped.ID = @PedidoId;
      
	  -- Buscar Upline
	  WITH Upline(ID, Assinatura, PatrocinadorPosicaoID)
	  AS
	  (
			SELECT Usr.ID, Usr.Assinatura, Usr.PatrocinadorPosicaoID
			FROM Usuario.Usuario Usr (NOLOCK)
			WHERE Usr.ID = @UsuarioId

			UNION ALL

			SELECT Usr.ID, Usr.Assinatura, Usr.PatrocinadorPosicaoID
			FROM Usuario.Usuario Usr (NOLOCK)
			INNER JOIN Upline ON Upline.PatrocinadorPosicaoID = Usr.ID
			WHERE Upline.ID <> Upline.PatrocinadorPosicaoID
	  )
	  SELECT Upline.ID
	  INTO #Upline
	  FROM Upline
	  INNER JOIN Usuario.Usuario Usr (NOLOCK) ON Upline.ID = Usr.ID
	  WHERE Upline.ID <> @UsuarioId
	   -- AND Usr.RecebeBonus = 1 -- Qualificado
	   -- AND Usr.DataAtivacao <= CAST(CONVERT(VARCHAR(8), dbo.GetDateZion(), 112) + ' 23:59:59' as datetime2) -- Ativo

      -- Gera Bonus
      BEGIN TRANSACTION

      -- Gera Toda Upline
      INSERT INTO Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   SELECT 
         23 as CategoriaID, -- Bonus Equipe
         UP.ID as UsuarioID,
         @UsuarioId as Referencia,
         0 as StatusID,
         @DataPedido as Data,
         @VlrBonus as Valor,
         @PedidoId as PedidoID
      FROM #Upline UP
     
	   -- Insere registros de Bônus de Equipe (pontos)
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
		   2 AS ContaID, -- Pontos
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
		     INNER JOIN #Upline U (nolock) ON U.ID = B.ReferenciaID
	   Where B.StatusID = 1
        and B.CategoriaID IN (23)

      COMMIT TRANSACTION

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION;
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusEquipe: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

