use univerDev
go
--delete Rede.TabuleiroLog
--Select * from  Rede.TabuleiroLog
--Select id,login from usuario.usuario where id in (2582,2587,2588,2589,2590,2591,2592,2593,2594,2595,2596,2597,2598,2599,2600,2601,2602,2603,2604,2605,2606,2607,2608,2609,2610,2611,2612,2613,2614,2615,2616,2617,2618,2619,2620,2621,2622,2623,2624,2625,2626,2627) 

Select * from Rede.TabuleiroUsuario 
where 
UsuarioID = 2581 and
BoardID = 1

  
  --Master
INSERT INTO Rede.TabuleiroUsuario (
    UsuarioID,
    TabuleiroID,
    BoardID,
    StatusID,
    MasterID,
    InformePag,
    UsuarioIDPag,
    Ciclo,
    Posicao,
    PagoMaster,
    PagoSistema,
    ConviteProximoNivel,
    DireitaFechada,
    EsquerdaFechada,
    DataInicio,
    DataFim,
    Debug
) 
Values 
(
    @CoordinatorDir, --Ele vira o Master
    @Identity,
    @BoardID,
    1, --Ativo
    @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
    1,
    null,
    coalesce(@Ciclo,@Ciclo,1),
    'Master',
    'true',
    'true',
    'false',
    'false',
    'false',
    GetDate(),
    null,
    'Direita Fechada'
)











