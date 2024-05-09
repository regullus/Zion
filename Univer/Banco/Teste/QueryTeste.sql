use univerDev
go
--Delete Rede.TabuleiroLog 
--36
--Select * from  Rede.TabuleiroLog order by id desc

--Select id,login, apelido from usuario.usuario where id > 2500 order by id desc --and apelido like '%cacado%'
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like '%mara%'

--Inidcados
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 2604

--Select usuarioID, count(*) Total from Rede.TabuleiroUsuario Group by UsuarioID
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2587 --Mario
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2589 --maria
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2588 --pedro
--Select * from Rede.TabuleiroUsuario where  UsuarioID = 2591 --enzo

Select id,login, apelido, PatrocinadorDiretoID, StatusEmailID from usuario.usuario where id > 2500 and apelido like '%dinamite%'

Select * From Rede.Tabuleiro where ID = 87
Select * From Rede.TabuleiroUsuario Where UsuarioID = 2602 and BoardID = 1

Select * from  Rede.TabuleiroLog order by id desc

--"Exec spG_Tabuleiro @UsuarioID=2638,@UsuarioPaiID=2583,@BoardID=1,@Chamada='ConviteNew'

Select * from Globalizacao.Traducao (nolock) where texto like '%Convite(s)%'

Exec spC_TabuleiroUsuarioID @UsuarioID=2589, @BoardID=2