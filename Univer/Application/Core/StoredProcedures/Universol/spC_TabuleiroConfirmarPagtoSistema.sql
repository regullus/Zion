use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroConfirmarPagtoMaster'))
   Drop Procedure spC_TabuleiroConfirmarPagtoMaster
go

Create  Proc [dbo].[spC_TabuleiroConfirmarPagtoMaster]
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
		@Check bit,
		@TabuleiroID int
    
	Select
        @MasterID = MasterID,
		@TabuleiroID = TabuleiroID
    From
        rede.TabuleiroUsuario (nolock)
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
			rede.TabuleiroUsuario (nolock)
		Where
			UsuarioID = @UsuarioID and 
			BoardID  = @BoardID 
		
		if Exists (
			Select 
				'OK' 
			From 
				rede.TabuleiroUsuario (nolock)
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
				Rede.TabuleiroUsuario (nolock)
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
						rede.TabuleiroUsuario (nolock)
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
			if Exists (
				Select
					'OK'
				From
					Rede.Tabuleiro (nolock)
				Where
					ID = @TabuleiroID and
					StatusID = 2 
			)
			Begin
				Update
					Rede.TabuleiroUsuario
				Set
					DireitaFechada = 'false',
					EsquerdaFechada = 'false'
				Where
					UsuarioID = @MasterID and
					BoardID = @BoardID and
					PagoSistema = 'true' --tem q ter pago o sistema
							
				Update
					Rede.TabuleiroUsuario 
				Set
					StatusId = 2, --Convite
					DataInicio = GetDate(),
					Debug = 'Tabuleiro Fechado -Confirma Pag Sistema - Convite BaordID=' + TRIM(STR(@BoardID))
				Where
					UsuarioID = @MasterID and --Ele vira o Master
					BoardID = @BoardID and
					PagoSistema = 'true' --tem q ter pago o sistema
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
			UsuarioID = @MasterID and 
			TabuleiroID  = @BoardID
	End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroConfirmarPagtoMaster To public
go

/*
Begin Tran

Rollback tran

Exec spC_TabuleiroConfirmarPagtoMaster @UsuarioID=2582, @BoardID=1, @confirmar='true'

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2582

Select * from  Rede.TabuleiroLog order by id desc

Select * from Rede.TabuleiroUsuario where  UsuarioID = 2582 and BoardID = 1
Select * From Rede.Tabuleiro where id = 41

*/