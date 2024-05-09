use Univer
go
--usuario.usuario
--autenticacao
--Loja.Pedido
--Usuario.Endereco

declare @usuarioId int
select @usuarioId = 2586

--select login from usuario.usuario where Id > @usuarioId

--select * from Sistema.Suportemensagem
update Loja.Pedido set EnderecoEntregaID = 1, EnderecoFaturamentoID = 1 where usuarioId > @usuarioId
delete Usuario.Endereco where UsuarioId > @usuarioId
delete Rede.Posicao where usuarioid > @usuarioId
delete Sistema.SuporteMensagem where SuporteID in (select id from Sistema.Suporte  where usuarioid > @usuarioId)
delete Sistema.Suporte  where usuarioid > @usuarioId
delete Usuario.AtivacaoMensal where usuarioid > @usuarioId
delete Usuario.Complemento where id > @usuarioId
delete Usuario.UsuarioStatus where usuarioid > @usuarioId
delete Financeiro.Lancamento where usuarioid > @usuarioId

delete Loja.PedidoItemStatus where PedidoItemID in ( select id from Loja.PedidoItem where PedidoID in (Select id from Loja.Pedido where usuarioid > @usuarioId))
delete Loja.PedidoItem where PedidoID in (Select id from Loja.Pedido where usuarioid > @usuarioId) 

delete Loja.PedidoPagamentoStatus where PedidoPagamentoID in ( select id from Loja.PedidoPagamento where PedidoID in (Select id from Loja.Pedido where usuarioid > @usuarioId))
delete Loja.PedidoPagamento where PedidoID in (Select id from Loja.Pedido where usuarioid > @usuarioId) 

delete rede.Bonificacao where usuarioid > @usuarioId
delete rede.Bonificacao where PedidoID in (select id from Loja.Pedido where usuarioid > @usuarioId)
delete Financeiro.Lancamento where usuarioid > @usuarioId
delete Financeiro.Lancamento where PedidoID in (select id from Loja.Pedido where usuarioid > @usuarioId)

delete Loja.Pedido where usuarioid > @usuarioId
delete Rede.Posicao where usuarioid > @usuarioId
delete Financeiro.saquestatus where Saqueid in (select id from Financeiro.saque where usuarioid > @usuarioId)
delete Financeiro.saque where usuarioid > @usuarioId
delete usuario.banco  where usuarioid > @usuarioId
delete from Usuario.UsuarioGanho  where usuarioid > @usuarioId
update usuario.usuario set NivelAssociacao = 0 where id > @usuarioId

update usuario.usuario set idAutenticacao = 1 where id > @usuarioId
delete Autenticacao where  username in (select login from usuario.usuario where id > @usuarioId)

delete Financeiro.ContaDeposito Where IDUsuario > @usuarioID

delete usuario.usuario where id > @usuarioId

--select login from usuario.usuario where Id > @usuarioId


