


/*categorias de Bonus*/

delete from Financeiro.Categoria where TipoID = 6
go
set IDENTITY_INSERT Financeiro.Categoria on
go
insert into Financeiro.Categoria (ID, TipoID, Nome) values (21, 6, 'Bonus Venda Direta')
insert into Financeiro.Categoria (ID, TipoID, Nome)  values (22, 6, 'Bonus Alavancagem')
insert into Financeiro.Categoria (ID, TipoID, Nome)  values (23, 6, 'Bonus Residual')
insert into Financeiro.Categoria (ID, TipoID, Nome)  values (24, 6, 'Bonus Plus')
insert into Financeiro.Categoria (ID, TipoID, Nome) values (25, 6, 'Bonus Equipe')
insert into Financeiro.Categoria (ID, TipoID, Nome)  values (26, 6, 'Bonus Plano Carreira')
go
set IDENTITY_INSERT Financeiro.Categoria off
go

/*Regras*/

insert into Rede.Regra (ID, Nome, Datacriacao, Ativo) values (1, 'Regra Bonus Indicação', getdate(), 1)
insert into Rede.RegraItem (ID, RegraID, Nivel, AssociacaoID, Regra, DataCriacao, Ativo, ClassificacaoID) 
values(1, 1, -1, null, '0.04', getdate(), 1, null )