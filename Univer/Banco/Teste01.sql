use Univer
Go
Select * From Rede.TabuleiroUsuario --Where StatusID = 1 and UsuarioID = 2580 and BoardID = 2 
where UsuarioID = 2580

Select * from rede.TabuleiroUsuario --where BoardID in (1,2) order by UsuarioID, ID
Select * from rede.Tabuleiro --where BoardID in (2)

--2580,2581,2582,2583,2584,2586

--Select * from rede.TabuleiroLog
Select * from rede.TabuleiroNivel

--Select * from usuario.usuario
exec spG_Tabuleiro @UsuarioID = 2580, @UsuarioPaiID =2580, @BoardID = 1, @Chamada = 'NovaInscricaoBoard1'
exec spG_Tabuleiro @UsuarioID = 2580, @UsuarioPaiID =2580, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2581, @UsuarioPaiID =2581, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2582, @UsuarioPaiID =2582, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2583, @UsuarioPaiID =2583, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2584, @UsuarioPaiID =2584, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2585, @UsuarioPaiID =2585, @BoardID = 2, @Chamada = 'NovaInscricao'
exec spG_Tabuleiro @UsuarioID = 2586, @UsuarioPaiID =2586, @BoardID = 2, @Chamada = 'NovaInscricao'

exec spG_Tabuleiro @UsuarioID = 2587, @UsuarioPaiID =2587, @BoardID = 2, @Chamada = 'NovaInscricao'
