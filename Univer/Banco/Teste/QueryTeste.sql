use univerDev
go
--delete Rede.TabuleiroLog
Select * from  Rede.TabuleiroLog

Select * from Rede.TabuleiroNivel Where BoardID = 1 and StatusID < 3

Select id,login, Apelido from usuario.usuario where id in (2581,2583,2584,2589,2590,2587,2588,2582,2585,2586,2591,2593,2594,2595,2580)

Select id,login, Apelido from usuario.usuario where id in (2596,2597,2598,2599,2600,2601,2602,2603,2604,2605,2606)
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
Select id,login, Apelido from usuario.usuario where id in (2590)
--luciano e tony

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



Exec spG_Tabuleiro @UsuarioID=2602,@UsuarioPaiID=2581,@BoardID=10,@Chamada='Completa'