use univerDev
go
--Select id,login from usuario.usuario where id in (2580,2581,2582,2583,2584,2585,2586) order by id
--Select id,login from usuario.usuario where id in (2587,2588,2589,2590,2591,2593,2595,2594,2596,2597,2598,2599,2600,2601,2602) order by id

--Select id,login, * from usuario.usuario where id in (2580,2581,2582,2583,2584,2585,2586,2587,2588,2589,2590,2591,2593,2595,2594,2596,2597,2598,2599,2600,2601,2602) 

--select * from Rede.TabuleiroUsuario where tabuleiroID = 1


delete Rede.TabuleiroLog
Select * from  Rede.TabuleiroLog

select * from Rede.Tabuleiro where boardID = 1

select 
    tn.ID,
    tn.UsuarioID,
    usu.login,
    usu.PatrocinadorDiretoID,
    tn.StatusID,
    tn.Observacao
from 
    Rede.TabuleiroNivel tn,
    usuario.usuario usu
where 
    tn.BoardID = 1 and
    tn.UsuarioID = usu.ID

--Select * from usuario.usuario
Select id,login, Apelido from usuario.usuario where id in (2590,2595)
--2595 luciano e 2590 tony

--2582	SOLIDARIO
--2591 Enzo
--2606	paulo

Select
    UsuarioID,
    DireitaFechada,
    EsquerdaFechada
From
    Rede.TabuleiroUsuario
Where
    --UsuarioID = @Master and
    TabuleiroID = 1 and 
    BoardID = 1 and
    StatusID = 1

Select * from Rede.TabuleiroUsuario where boardID = 1 and Posicao = 'Master'

Select * from Rede.TabuleiroUsuario where TabuleiroID = 9

Select * from Rede.TabuleiroNivel where usuarioID in (2590,2595)

Select 
    * 
from 
Rede.Tabuleiro 
Where id in (1,9)

Select * from Rede.TabuleiroUsuario where TabuleiroID = 9

Select id,login, Apelido from usuario.usuario where id in (2582,2585,2591,2593,2586,2594,2595)


-- USUARIOID in (2591,2595)

Select * from Rede.TabuleiroUsuario Where USUARIOID in (2591)

Select * from Rede.TabuleiroUsuario Where USUARIOID in (2595)

Select * from Rede.TabuleiroNivel Where BoardID = 1