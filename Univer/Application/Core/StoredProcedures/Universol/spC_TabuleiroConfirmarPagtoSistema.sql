use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroConfirmarPagtoSistema'))
   Drop Procedure spC_TabuleiroConfirmarPagtoSistema
go

Create  Proc [dbo].[spC_TabuleiroConfirmarPagtoSistema]
   @UsuarioID int,
   @BoardID int,
   @Confirmar bit

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem niveis no tabuleito de um usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

	Declare 
        @Count int,
        @MasterID int
    
	Select
        @MasterID = MasterID
    From
        rede.TabuleiroUsuario 
    Where
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 

	if(@Confirmar = 'true')
	Begin
		Update
			rede.TabuleiroUsuario 
		Set
			PagoSistema = 'true'
		where 
			UsuarioID = @UsuarioID and 
			TabuleiroID  = @BoardID

		--Verifica se ja e o 4 recebimento do master
		Select
			@MasterID = MasterID
		From
			rede.TabuleiroUsuario 
		Where
			UsuarioID = @UsuarioID and 
			BoardID  = @BoardID 

		if Exists (
			Select 
				'OK' 
			From 
				rede.TabuleiroUsuario 
			Where 
				UsuarioID = @MasterID and 
				BoardID  = @BoardID and 
				PagoSistema = 'true' and 
				StatusID = 1
			)
		Begin
			Select 
				@Count = count(*)
			From
				rede.TabuleiroUsuario 
			Where
				MasterID = @MasterID and 
				BoardID  = @BoardID and
				PagoMaster = 'true' and
				Posicao in ('DonatorDirSup1','DonatorDirSup2','DonatorDirInf1','DonatorDirInf2','DonatorEsqSup1','DonatorEsqSup2','DonatorEsqInf1','DonatorEsqInf2')

			if(@count>=4)
			Begin
				--Verifica se ja nao esta no proximo nivel
				if not Exists (
					Select 
						'OK' 
					From 
						rede.TabuleiroUsuario 
					Where 
						UsuarioID = @MasterID and 
						BoardID  = @BoardID + 1
					)
				Begin
					--Atualiza para criar o convite para o proximo nivel
					Update
						rede.TabuleiroUsuario
					Set 
						StatusID = 2 --Convite Proximo Nivel
					Where
						UsuarioID = @MasterID and 
						BoardID  = @BoardID 
				End
			End
		End
	End
	Else 
	Begin
		Update
			rede.TabuleiroUsuario 
		Set
			PagoSistema = 'false',
			InformePagSistema = 'false'
		where 
			UsuarioID = @UsuarioID and 
			TabuleiroID  = @BoardID
	End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroConfirmarPagtoSistema To public
go

--Exec spC_TabuleiroConfirmarPagtoSistema @UsuarioID = 2580, @BoardID = 1

--Exec spC_TabuleiroConfirmarPagtoSistema @UsuarioID=2587, @BoardID=1
