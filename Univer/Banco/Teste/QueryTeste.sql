use univerDev
go
--Delete Rede.TabuleiroLog 
--36
Select * from  Rede.TabuleiroLog order by id desc

Select id,login, apelido from usuario.usuario where id > 2500 order by id desc --and apelido like '%ti%'
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like '%elia%'

--Inidcados
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2589

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select * from  Rede.TabuleiroUsuario where boardID = 1 and usuarioID = 2580

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 and BoardID =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 and BoardID =1

Select * From Rede.TabuleiroUsuario Where BoardID = 1


Select * from rede.tabuleiro where BoardID = 1 and Master = 2581
/*
Para ser ativo

O usuario esta ativo se ele pagou o alvo das galaxias que ele já pertence
O indidicado esta ativo se obedecer a regra acima
Select * From Rede.TabuleiroUsuario Where  UsuarioID = 2589


*/

