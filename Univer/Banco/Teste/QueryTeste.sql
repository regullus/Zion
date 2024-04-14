use univerDev
go
--delete Rede.TabuleiroLog
--Select * from  Rede.TabuleiroLog
go

Select id,login from usuario.usuario where id in (2582,2587,2588,2589,2590,2591,2592,2593,2594,2595,2596,2597,2598,2599,2600,2601,2602,2603,2604,2605,2606,2607,2608,2609,2610,2611,2612,2613,2614,2615,2616,2617,2618,2619,2620,2621,2622,2623,2624,2625,2626,2627) 


use univerDev
go
declare @BoardID int,
@UsuarioID int
set @UsuarioID = 2590
set @BoardID =1

Select 
        'Existe'   
    From
        Rede.Tabuleiro 
    Where
        BoardID = @BoardID and
        (
            Master = @UsuarioID Or
            CoordinatorDir = @UsuarioID Or
            IndicatorDirSup = @UsuarioID Or
            IndicatorDirInf = @UsuarioID Or
            DonatorDirSup1 = @UsuarioID Or
            DonatorDirSup2 = @UsuarioID Or
            DonatorDirInf1 = @UsuarioID Or
            DonatorDirInf2 = @UsuarioID Or
            CoordinatorEsq = @UsuarioID Or
            IndicatorEsqSup = @UsuarioID Or
            IndicatorEsqInf = @UsuarioID Or
            DonatorEsqSup1 = @UsuarioID Or
            DonatorEsqSup2 = @UsuarioID Or
            DonatorEsqInf1 = @UsuarioID Or
            DonatorEsqInf2 = @UsuarioID
        ) and
        StatusID = 1 

	Select * from rede.Tabuleiro






