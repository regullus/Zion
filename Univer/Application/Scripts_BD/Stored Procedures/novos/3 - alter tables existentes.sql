ALTER TABLE Loja.Pedido add CicloID int
GO
alter table Usuario.UsuarioAssociacao add Upgrade bit null default 0, DataValidade datetime 