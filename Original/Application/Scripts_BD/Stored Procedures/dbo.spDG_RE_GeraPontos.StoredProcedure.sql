USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spDG_RE_GeraPontos]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spDG_RE_GeraPontos]
As
BEGIN
	SET FMTONLY OFF
	SET NOCOUNT ON

	-- Pontos Pessoais
	INSERT INTO Rede.Pontos
	SELECT
		UsuarioID		= LPed.UsuarioID,
		ReferenciaID	= LPed.UsuarioID,
		CicloID			= RCic.ID,
		VP				= ISNULL(LPedItem.BonificacaoUnitaria, 0),
		PedidoID		= LPed.ID,
		VPQ				= ISNULL(LPedItem.BonificacaoUnitaria, 0)
	FROM 
		Loja.Pedido LPed (NOLOCK)
		INNER JOIN Loja.PedidoItem LPedItem (NOLOCK) 
			ON LPedItem.PedidoID = LPed.ID
		INNER JOIN Loja.Produto LProd (NOLOCK) 
			ON LProd.ID = LPedItem.ProdutoID
		INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
			ON LPag.PedidoID = LPed.ID
		INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
			ON LPagSta.PedidoPagamentoID = LPag.ID
		INNER JOIN Rede.Ciclo RCic (NOLOCK) 
			ON LPagSta.Data BETWEEN RCic.DataInicial AND RCic.DataFinal
		LEFT JOIN Rede.Pontos PRec (NOLOCK) 
			ON PRec.PedidoID = LPed.ID
			AND PRec.UsuarioID = LPed.UsuarioID
			AND PRec.ReferenciaID = LPed.UsuarioID
	WHERE 
		LPedItem.BonificacaoUnitaria > 0
		AND LPagSta.StatusID = 3
		AND PRec.ID IS NULL

	-- Pontos da Rede
	INSERT INTO Rede.Pontos
	SELECT
		UsuarioID		= Usu.ID,
		ReferenciaID	= LPed.UsuarioID,
		CicloID			= RCic.ID,
		VP				= ISNULL(LPedItem.BonificacaoUnitaria, 0),
		PedidoID		= LPed.ID,
		VPQ				= ISNULL(LPedItem.BonificacaoUnitaria, 0)
	FROM 
		Loja.Pedido LPed (NOLOCK)
		INNER JOIN Loja.PedidoItem LPedItem (NOLOCK) 
			ON LPedItem.PedidoID = LPed.ID
		INNER JOIN Loja.Produto LProd (NOLOCK) 
			ON LProd.ID = LPedItem.ProdutoID
		INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
			ON LPag.PedidoID = LPed.ID
		INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
			ON LPagSta.PedidoPagamentoID = LPag.ID
		INNER JOIN Rede.Ciclo RCic (NOLOCK) 
			ON LPagSta.Data BETWEEN RCic.DataInicial AND RCic.DataFinal
		INNER JOIN Rede.Upline_Ciclo Upl (NOLOCK) 
			ON Upl.UsuarioID = LPed.UsuarioID
			And Upl.CicloID = RCic.ID
		INNER JOIN Usuario.Usuario Usu (NOLOCK) 
			ON Upl.Upline = Usu.ID
		LEFT JOIN Rede.Pontos PRec (NOLOCK) 
			ON PRec.PedidoID = LPed.ID
			AND PRec.UsuarioID = Usu.ID
			AND PRec.ReferenciaID = LPed.UsuarioID
	WHERE 
		LPedItem.BonificacaoUnitaria > 0
		AND LPagSta.StatusID = 3
		AND PRec.ID IS NULL

	SET NOCOUNT OFF 
END

GO
