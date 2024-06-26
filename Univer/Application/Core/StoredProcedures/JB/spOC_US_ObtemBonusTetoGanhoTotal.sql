USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemBonusTetoGanhoTotal]    Script Date: 08/07/2019 15:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Proc [dbo].[spOC_US_ObtemBonusTetoGanhoTotal]
   @idUsuario int

As
-- =============================================================================================
-- Author.....: Rui barbosa
-- Create date: 08/07/2019
-- Description: Obtem o teto do bonus ganho total 
-- =============================================================================================
BEGIN
   SET FMTONLY OFF
   SET NOCOUNT ON

   DECLARE @tetoBonus INT;
   DECLARE @tetoAdesao INT;

   --Recupera o valor do pacote Adesão já multiplicado
   SELECT  @tetoAdesao = SUM(POV.Valor) * 4 FROM Loja.Pedido P
	JOIN Loja.PedidoItem PEI ON P.ID = PEI.PedidoID
	JOIN Loja.PedidoPagamento PEP ON P.ID = PEP.PedidoID
	JOIN Loja.PedidoPagamentoStatus PEPS ON PEP.ID = PEPS.PedidoPagamentoID
	JOIN Loja.Produto PO ON PEI.ProdutoID = PO.ID
	JOIN Loja.ProdutoValor POV ON PO.ID = POV.ProdutoID
	WHERE PEI.ProdutoID IN (SELECT ID FROM Loja.Produto WHERE SKU LIKE '%ADE%')
	AND PEPS.StatusID = 3
	AND P.UsuarioID = @idUsuario

   --Verifica se existe alguma aquisição de pacote Acumulação, se sim cálculo o teto (4X a soma de todos os pacotes adquiridos)
   SELECT  @tetoBonus = SUM(POV.Valor) * 4 FROM Loja.Pedido P
	JOIN Loja.PedidoItem PEI ON P.ID = PEI.PedidoID
	JOIN Loja.PedidoPagamento PEP ON P.ID = PEP.PedidoID
	JOIN Loja.PedidoPagamentoStatus PEPS ON PEP.ID = PEPS.PedidoPagamentoID
	JOIN Loja.Produto PO ON PEI.ProdutoID = PO.ID
	JOIN Loja.ProdutoValor POV ON PO.ID = POV.ProdutoID
	WHERE PEI.ProdutoID IN (SELECT ID FROM Loja.Produto WHERE SKU LIKE '%ACU%')
	AND PEPS.StatusID = 3 
	AND P.UsuarioID = @idUsuario

	--Caso não tenha adquirido pacote Acumulação busca o maior pacote Upgrade adquirido e cálcula o teto (4X o valor do maior pacote upgrade adquirido)
	IF (@tetoBonus IS NULL)
		BEGIN
			SELECT @tetoBonus = MAX(POV.Valor) * 4 FROM Loja.Pedido P
			JOIN Loja.PedidoItem PEI ON P.ID = PEI.PedidoID
			JOIN Loja.PedidoPagamento PEP ON P.ID = PEP.PedidoID
			JOIN Loja.PedidoPagamentoStatus PEPS ON PEP.ID = PEPS.PedidoPagamentoID
			JOIN Loja.Produto PO ON PEI.ProdutoID = PO.ID
			JOIN Loja.ProdutoValor POV ON PO.ID = POV.ProdutoID
			WHERE PEI.ProdutoID IN (SELECT ID FROM Loja.Produto WHERE SKU LIKE '%UPG%')
			AND PEPS.StatusID = 3 
			AND P.UsuarioID = @idUsuario
		END

	SELECT ISNULL(@tetoAdesao, 0) + ISNULL(@tetoBonus, 0)
END


--exec spOC_US_ObtemBonusTetoGanhoTotal 1000


