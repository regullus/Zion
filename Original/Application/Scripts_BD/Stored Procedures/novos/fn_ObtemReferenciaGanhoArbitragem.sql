DROP function [dbo].[fn_ObtemReferenciaGanhoArbitragem];
go

CREATE function fn_ObtemReferenciaGanhoArbitragem (@idUsuario int)
returns float
AS
begin

   declare @teto float = 0;
   declare @somaValor float = 0;
   declare @somaBTC float = 0;
   
   /*teto de bonus arbitragem: soma de BTC de todos pacotes pagos */
	select @somaValor = sum(P.Total), @somaBTC = sum(PPG.ValorBTC) from Loja.Pedido P
	inner join Loja.PedidoItem PI on P.ID = PI.PedidoID
	inner join Loja.Produto PROD on PI.ProdutoID = PROD.ID
	inner join Loja.PedidoPagamento PPG on P.ID = PPG.PedidoID
	inner join Loja.PedidoPagamentoStatus PPGS on PPG.ID = PPGS.PedidoPagamentoID
	where P.UsuarioID = @idUsuario
	and PPGS.StatusID = 3
	and PROD.TipoID in (1,2);

	set @teto = @somaBTC
   
	return @teto;

end