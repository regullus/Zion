/*limpa dados*/

delete from Financeiro.Lancamento where UsuarioID <> 1000

delete from Loja.PedidoItemStatus where PedidoItemID in (
	select ID from Loja.PedidoItem where PedidoID in (
		select ID from Loja.Pedido where UsuarioID <> 1000
	)
)
delete from Loja.PedidoItem where PedidoID in (
	select ID from Loja.Pedido where UsuarioID <> 1000
)
delete from Loja.PedidoPagamentoStatus where PedidoPagamentoID in (
	select ID from Loja.PedidoPagamento where PedidoID in(
		select ID from Loja.Pedido where UsuarioID <> 1000
	)
)
delete from Loja.PedidoPagamento where PedidoID in (
	select ID from Loja.Pedido where UsuarioID <> 1000
)

delete from Rede.Bonificacao where PedidoID in (
	select ID from Loja.Pedido where UsuarioID <> 1000
)

delete from Loja.Pedido where UsuarioID <> 1000

delete from Financeiro.ContaDeposito where IDUsuario <> 1000
delete from Financeiro.SaqueStatus where SaqueID in(
	select ID from Financeiro.Saque where UsuarioID <> 1000
) 
delete from Financeiro.Saque where UsuarioID <> 1000
delete from Financeiro.CartaoCredito where UsuarioID <> 1000
delete from Usuario.Endereco where UsuarioID <> 1000
delete from Usuario.AtivacaoMensal where UsuarioID <> 1000
delete from Usuario.Banco where UsuarioID <> 1000
delete from Usuario.Documento where UsuarioID <> 1000
delete from Usuario.UsuarioStatus where UsuarioID <> 1000
delete from Usuario.UsuarioGanho where UsuarioID <> 1000
delete from Rede.Posicao where UsuarioID <> 1000
delete from Rede.Pontos where UsuarioID <> 1000
delete from Rede.Bonificacao where UsuarioID <> 1000
delete from Usuario.UsuarioClassificacao where UsuarioID <> 1000
delete from Usuario.UsuarioAssociacao where UsuarioID <> 1000
delete from Usuario.UsuarioStatus where UsuarioID <> 1000
delete from Usuario.Pontos where UsuarioID <> 1000
delete from Usuario.PontosLinha where UsuarioID <> 1000
truncate table Usuario.LogAcesso 

update Usuario.Usuario set UltimoDireitaID = null, UltimoEsquerdaID = null where ID = 1000
delete from Usuario.Usuario where ID > 1000

truncate table Rede.RegraItem
truncate table Rede.Regra

truncate table Usuario.Pontos
truncate table Usuario.PontosLinha