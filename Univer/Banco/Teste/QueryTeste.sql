use univerDev
go
--Delete Rede.TabuleiroLog 
--36
--Select * from  Rede.TabuleiroLog order by id desc

--Select id,login, apelido from usuario.usuario where id = 2583
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like '%nata84%'

--Inidcados
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2604

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select id,login, apelido, PatrocinadorDiretoID, StatusEmailID from usuario.usuario where id > 2500 and apelido like '%dinamite%'

Select * From Rede.Tabuleiro where ID = 35
Select * From Rede.TabuleiroUsuario Where UsuarioID = 2846 and BoardID = 1
Select * From Rede.TabuleiroUsuario Where UsuarioID = 2593 and BoardID = 1

Select * from  Rede.TabuleiroLog order by id desc

Select
	'OK'
From
	Rede.TabuleiroUsuario
Where
	MasterID = @MasterID and
	BoardID = @BoardID and
	UsuarioId = @DonatorEsqSup1 and
	Posicao = 'DonatorEsqSup1'