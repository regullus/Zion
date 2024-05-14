use univerDev
go
--Delete Rede.TabuleiroLog 
--36
--Select * from  Rede.TabuleiroLog order by id desc

--Select id,login, apelido from usuario.usuario where id = 2601
Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like 'top160'

--Inidcados
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2604

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select id,login, apelido, PatrocinadorDiretoID, StatusEmailID from usuario.usuario where id = 5182


Select * From Rede.TabuleiroUsuario Where statusid=2 and boardID=2


Select id,login, apelido, PatrocinadorDiretoID, StatusEmailID from usuario.usuario where id in (2607)
Select * From Rede.TabuleiroUsuario Where UsuarioID in (2607) 

Select * From Rede.Tabuleiro where ID = 36

 
 Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id in (
    2597,
2619,
2647,
2658,
2676,
2702,
2709,
2742,
2744,
2774,
2775
)