Declare 
@BoardID int,
@UsuarioID int,
@chamada nvarchar(100),
@UsuarioPaiID int

set @BoardID = 1
set @UsuarioID = 2589
set @chamada = 'Convite'
set @UsuarioPaiID = null

Select 
*
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
			Master = Coalesce(@UsuarioPaiID, Master) and
			StatusID = 1 And --Tem que estar ativo no board
			@chamada <> 'Completa'

Select 
	'OK'
From
	Rede.TabuleiroUsuario
Where
	UsuarioID = @UsuarioID and
	BoardID = @BoardID and
	StatusID = 2 and --Convite
	PagoSistema = 'true' --tem q ter pago o sistema