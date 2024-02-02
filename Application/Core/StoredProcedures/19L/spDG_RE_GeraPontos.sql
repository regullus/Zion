use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraPontos'))
   Drop Procedure spDG_RE_GeraPontos
go

Create Proc [dbo].spDG_RE_GeraPontos

As
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: 
--
-- =============================================================================================

BEGIN
	SET FMTONLY OFF
	SET NOCOUNT ON

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
	WHERE 
		LPedItem.BonificacaoUnitaria > 0
		AND LPagSta.StatusID = 3
		AND PRec.ID IS NULL

	SET NOCOUNT OFF 
END


go
Grant Exec on spDG_RE_GeraPontos To public
go
