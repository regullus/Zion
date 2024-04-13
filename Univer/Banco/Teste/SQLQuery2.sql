use univerDev
go
Select * 
From 
#temp temp, 
Rede.TabuleiroUsuario tab 
Where 
temp.DonatorDirSup1 = tab.UsuarioID and 
tab.informePag = 0


Select * from Rede.TabuleiroUsuario
where boardid=1 and
usuarioID = 2591


--2591 Enzo

Select distinct
posicao
From 
    Rede.TabuleiroUsuario 
Where
    UsuarioID = 2591 And 
    PagoMaster = 0 And 
    InformePag = 1 and
    --@tempo > DataInicio and 
    TabuleiroID = 9



Select * from Rede.TabuleiroNivel where boardID = 1 --usuarioID = 2591

select * from Rede.TabuleiroUsuario where tabuleiroID = 1 -- usuarioid= 2591

Select 
    tn.*
From
    Rede.TabuleiroNivel tn,
    Rede.TabuleiroUsuario tab
Where
    tn.BoardID = 1 and
    tn.StatusID = 2 and  --Em andamento
    tn.UsuarioID = tab.usuarioID and
    tn.BoardID = tab.BoardID and
    tab.TabuleiroID = 1

Update 
    tn
Set
    tn.StatusID = 3, --Finalizado
    tn.DataFim = CONVERT(VARCHAR(8),GETDATE(),112),
    tn.Observacao = Observacao + ' | Finalizado.' 
From
    Rede.TabuleiroNivel tn,
    Rede.TabuleiroUsuario tab
Where
    tn.BoardID = 1 and
    tn.StatusID = 2 and  --Em andamento
    tn.UsuarioID = tab.usuarioID and
    tn.BoardID = tab.BoardID and
    tab.TabuleiroID = 1

