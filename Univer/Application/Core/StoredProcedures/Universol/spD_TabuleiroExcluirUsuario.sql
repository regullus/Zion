use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spD_TabuleiroExcluirUsuario'))
   Drop Procedure spD_TabuleiroExcluirUsuario
go

Create Proc [dbo].[spD_TabuleiroExcluirUsuario]
   @UsuarioID int,
   @TabuleiroID int

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
		@BoardID int,
		@Posicao nvarchar(255),
		@Retorno nvarchar(50)

	Set @Retorno = 'NOOK'
	
	--Verifica se usuario não esta ativo no tabuleiro, se sim pode excluí-lo
	if Exists (
	Select 
		'OK'
	FROM 
		Rede.TabuleiroUsuario
	Where
		InformePag = 0 and
		PagoMaster = 0 and
		StatusID = 1 and --Ativo
		UsuarioID = @UsuarioID and
		TabuleiroID = @TabuleiroID
	)
	Begin
	    
		Select
			@BoardID = BoardID,
			@posicao = Posicao
		From
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID = @TabuleiroID

		Insert Into
			Rede.TabuleiroUsuarioExcluidos
		SELECT 
			ID,
			UsuarioID,
			TabuleiroID,
			BoardID,
			StatusID,
			MasterID,
			InformePag,
			Ciclo,
			Posicao,
			PagoMaster,
			PagoSistema,
			DataInicio,
			DataFim
		FROM 
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID = @TabuleiroID

	 --Remove usuario que nao pagou no tabuleiroUsuario
		Delete
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID = @TabuleiroID

		Delete
			Rede.TabuleiroNivel
		Where
			UsuarioID = @UsuarioID and
			BoardID = @BoardID and
			TabuleiroID = @TabuleiroID
	
		--Remove usuario que nao pagou no tabuleiro
		if Exists (Select 'OK' from Rede.Tabuleiro Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID) 
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorDirSup1 = null Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorDirSup2 = null Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorDirInf1 = null Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorDirInf2 = null Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorEsqSup1 = null Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorEsqSup2 = null Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorEsqInf1 = null Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID
		End
		if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID)
		Begin
			Set @Retorno = 'OK'
			Update Rede.Tabuleiro Set DonatorEsqInf2 = null Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID
		End
    
	End

    Select @Retorno

End -- Sp

go
Grant Exec on spD_TabuleiroExcluirUsuario To public
go

Exec spD_TabuleiroExcluirUsuario @UsuarioID = 2595, @TabuleiroID = 1

