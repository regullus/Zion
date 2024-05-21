use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroFechado'))
   Drop Procedure spC_TabuleiroFechado
go

Create  Proc [dbo].[spC_TabuleiroFechado]
   @TabuleiroID int

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem Bdados do tabuleiro de um usuario
-- =============================================================================================
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

	Declare
		@DonatorDirSup1 int,
		@DonatorDirSup2 int,
		@DonatorDirInf1 int,
		@DonatorDirInf2 int,
		@DonatorEsqSup1 int,
		@DonatorEsqSup2 int,
		@DonatorEsqInf1 int,
		@DonatorEsqInf2 int,
		@BoardID int,
		@pagoMasterDonatorDirSup1 bit,
		@pagoMasterDonatorDirSup2 bit,
		@pagoMasterDonatorDirInf1 bit,
		@pagoMasterDonatorDirInf2 bit,
		@pagoMasterDonatorEsqSup1 bit,
		@pagoMasterDonatorEsqSup2 bit,
		@pagoMasterDonatorEsqInf1 bit,
		@pagoMasterDonatorEsqInf2 bit,
		@retorno nvarchar(10),
        @direita char(3),
        @esquerda char(3),
        @ambos char(3)

    Set @retorno = 'nao'
    Set @direita = 'nao'
    Set @esquerda = 'nao'
    Set @ambos = 'nao'

	Select
		@DonatorDirSup1 = DonatorDirSup1,
		@DonatorDirSup2 = DonatorDirSup2,
		@DonatorDirInf1 = DonatorDirInf1,
		@DonatorDirInf2 = DonatorDirInf2,
		@DonatorEsqSup1 = DonatorEsqSup1,
		@DonatorEsqSup2 = DonatorEsqSup2,
		@DonatorEsqInf1 = DonatorEsqInf1,
		@DonatorEsqInf2 = DonatorEsqInf2,
		@BoardID = BoardID
	From
		Rede.Tabuleiro
	Where
		ID = @TabuleiroID
	
	--Verifica se todos os Donators estão com usuarios
	if (
		@DonatorDirSup1 is not null and
		@DonatorDirSup2 is not null and
		@DonatorDirInf1 is not null and
		@DonatorDirInf2 is not null and
		@DonatorEsqSup1 is not null and
		@DonatorEsqSup2 is not null and
		@DonatorEsqInf1 is not null and
		@DonatorEsqInf2 is not null
	)
	Begin
		--Verifica se todos pagaram o master
		Select @pagoMasterDonatorDirSup1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirSup1 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorDirSup2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirSup2 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorDirInf1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirInf1 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorDirInf2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirInf2 and BoardID = @BoardID and PagoMaster = 'true'

		Select @pagoMasterDonatorEsqSup1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqSup1 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorEsqSup2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqSup2 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorEsqInf1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqInf1 and BoardID = @BoardID and PagoMaster = 'true'
		Select @pagoMasterDonatorEsqInf2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqInf2 and BoardID = @BoardID and PagoMaster = 'true'

		if(
			@pagoMasterDonatorDirSup1 = 'true' and
			@pagoMasterDonatorDirSup2 = 'true' and
			@pagoMasterDonatorDirInf1 = 'true' and
			@pagoMasterDonatorDirInf2 = 'true' and
			@pagoMasterDonatorEsqSup1 = 'true' and
			@pagoMasterDonatorEsqSup2 = 'true' and
			@pagoMasterDonatorEsqInf1 = 'true' and
			@pagoMasterDonatorEsqInf2 = 'true'
		)
		Begin
			set @ambos = 'sim'
		End
		Else
		Begin
			set @ambos = 'nao'
		End
	End
	Else
	Begin
		set @ambos = 'nao'
	End
    
    set @retorno = @ambos

    if(@ambos = 'nao')
    Begin

        --Verifica se Direita esta fechada
	    if (
		    @DonatorDirSup1 is not null and
		    @DonatorDirSup2 is not null and
		    @DonatorDirInf1 is not null and
		    @DonatorDirInf2 is not null
	    )
	    Begin
		    --Verifica se todos pagaram o master
		    Select @pagoMasterDonatorDirSup1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirSup1 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorDirSup2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirSup2 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorDirInf1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirInf1 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorDirInf2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorDirInf2 and BoardID = @BoardID and PagoMaster = 'true'

		    if(
			    @pagoMasterDonatorDirSup1 = 'true' and
			    @pagoMasterDonatorDirSup2 = 'true' and
			    @pagoMasterDonatorDirInf1 = 'true' and
			    @pagoMasterDonatorDirInf2 = 'true' 
		    )
		    Begin
			    set @direita = 'sim'
		    End
		    Else
		    Begin
			    set @direita = 'nao'
		    End
	    End
	    Else
	    Begin
		    set @direita = 'nao'
	    End

        --Verifica se Esquerda esta fechada
	    if (
		    @DonatorEsqSup1 is not null and
		    @DonatorEsqSup2 is not null and
		    @DonatorEsqInf1 is not null and
		    @DonatorEsqInf2 is not null
	    )
	    Begin
		    --Verifica se todos pagaram o master
		    Select @pagoMasterDonatorEsqSup1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqSup1 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorEsqSup2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqSup2 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorEsqInf1 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqInf1 and BoardID = @BoardID and PagoMaster = 'true'
		    Select @pagoMasterDonatorEsqInf2 = PagoMaster From Rede.TabuleiroUsuario Where UsuarioID = @DonatorEsqInf2 and BoardID = @BoardID and PagoMaster = 'true'

		    if(
			    @pagoMasterDonatorEsqSup1 = 'true' and
			    @pagoMasterDonatorEsqSup2 = 'true' and
			    @pagoMasterDonatorEsqInf1 = 'true' and
			    @pagoMasterDonatorEsqInf2 = 'true'
		    )
		    Begin
			    set @esquerda = 'sim'
		    End
		    Else
		    Begin
			    set @esquerda = 'nao'
		    End
	    End
	    Else
	    Begin
		    set @esquerda = 'nao'
	    End

    End
    
    

    if(@direita = 'sim')
        Begin
            set @retorno = 'direita'
        End
    if(@esquerda = 'sim')
    Begin
        set @retorno = 'esquerda'
    End
    if(@esquerda = 'sim' and @direita = 'sim')
    Begin
        set @retorno = 'ambos'
    End
    if(@ambos = 'sim')
    Begin
        set @retorno = 'ambos'
    End

	Select @retorno retorno
End -- Sp

go
Grant Exec on spC_TabuleiroFechado To public
go

--Exec spC_TabuleiroFechado @TabuleiroID=211
Exec spC_TabuleiroFechado @TabuleiroID=211





