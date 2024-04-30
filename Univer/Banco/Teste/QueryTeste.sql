use univerDev
go
--Delete Rede.TabuleiroLog 
--36
Select * from  Rede.TabuleiroLog order by id desc

Select id,login, apelido from usuario.usuario where id > 2500 order by id desc --and apelido like '%ti%'
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like '%tita%'

--Inidcados
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2589

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select * from  Rede.TabuleiroUsuario where boardID = 1 and usuarioID in ( 2580,2581)

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 and BoardID =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 and BoardID =1

Select * From Rede.Tabuleiro where id =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589

Begin tran


Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589
Exec spD_TabuleiroExcluirUsuario @UsuarioID=2589, @MasterID=2580, @BoardID =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589


Rollback tran