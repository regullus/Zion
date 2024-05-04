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
        @MasterID int,
		@Check bit
    
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
			BoardID  = @BoardID 

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
				@count = TotalRecebimento
			From 
				Rede.TabuleiroUsuario 
			Where 
				UsuarioID = @MasterID And 
				BoardID = @BoardID
				
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
						BoardID  = @BoardID + 1 and
						TabuleiroID is not null
					)
				Begin
					--Verifica se ja pagou o sistema
					--Atualiza para criar o convite para o proximo nivel
					Update
						rede.TabuleiroUsuario
					Set 
						StatusID = 2, --Convite Proximo Nivel
						DataInicio = GetDate()
					Where
						UsuarioID = @MasterID and 
						BoardID  = @BoardID + 1 
				End
			End
		
			--Verifica se já fechou o tabuleiro para abrir outro
			Exec spG_Tabuleiro @UsuarioID=@UsuarioID, @UsuarioPaiID=@MasterID, @BoardID=@BoardID, @Chamada='Completa'
		
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
			UsuarioID = @MasterID and 
			TabuleiroID  = @BoardID
	End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroConfirmarPagtoSistema To public
go

/*
Begin Tran

Rollback tran

Exec spC_TabuleiroConfirmarPagtoSistema @UsuarioID=2582, @BoardID=1, @confirmar='true'

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2582


*/