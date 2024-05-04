use univerDev
go
--Delete Rede.TabuleiroLog 
--36
Select * from  Rede.TabuleiroLog order by id desc

Select id,login, apelido from usuario.usuario where id > 2500 order by id desc --and apelido like '%ti%'
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like '%tita%'

--Inidcados
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2604

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select * from  Rede.TabuleiroUsuario where boardID = 1 and usuarioID in ( 2580,2581)

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 and BoardID =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589

Select * From Rede.Tabuleiro where id =1
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2604
Select * from Rede.TabuleiroUsuario where  UsuarioID = 2580

Select *
FROM Rede.TabuleiroUsuario 
Where 
UsuarioID = 2604 and BoardID = 2 and StatusID = 1 and PagoMaster = 'true' 


Exec spC_Tabuleiro @id=29, @UsuarioID = 2589

Exec spC_TabuleiroMasterRule @UsuarioID=2594, @BoardID=1

