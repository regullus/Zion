use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spD_TabuleiroExcluirUsuario'))
   Drop Procedure spD_TabuleiroExcluirUsuario
go

Create Proc [dbo].[spD_TabuleiroExcluirUsuario]
   @UsuarioID int,
   @MasterID int,
   @BoardID int

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
	    @TabuleiroID int,
		@Posicao nvarchar(255),
		@Retorno nvarchar(50)

	Set @Retorno = 'NOOK'
	
	--Verifica se usuario nao esta ativo no tabuleiro, se sim pode exclui-lo
	if Exists (
	Select 
		'OK'
	FROM 
		Rede.TabuleiroUsuario
	Where
	    UsuarioID = @UsuarioID and
		BoardID = @BoardID and
		PagoMaster = 'false' and
		StatusID = 1 --Ativo
	)
	Begin
	    
		Select
			@TabuleiroID = TabuleiroID,
			@posicao = Posicao
		From
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			BoardID = @BoardID

		--Zera statusID do usuario 
		Update
			Rede.TabuleiroUsuario
		Set
			StatusID = 0, -- Esta disponivel para entrar em um tabuleiro
			Posicao = '',
			TabuleiroID = null,
			InformePag = 'false',
			UsuarioIDPag = null,
			Debug = 'Removido pelo master:' + TRIM(STR(@MasterID))
		Where
			UsuarioID = @UsuarioID and
			BoardID = @BoardID

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
	Else 
	Begin
		Set @Retorno = 'Convidado nao esta ativo no tabuleiro'
	End
    Select @Retorno

End -- Sp

go
Grant Exec on spD_TabuleiroExcluirUsuario To public
go

--Exec spD_TabuleiroExcluirUsuario @UsuarioID=2590, @TabuleiroID =1

