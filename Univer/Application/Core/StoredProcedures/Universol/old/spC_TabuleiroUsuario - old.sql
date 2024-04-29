use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUsuario'))
   Drop Procedure spC_TabuleiroUsuario
go

Create  Proc [dbo].[spC_TabuleiroUsuario]
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

	if Exists(
		Select 
			'OK'
		From 
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID = @TabuleiroID and
			StatusID = 1
	)
	Begin
	    --Select '1'
		Select 
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
		From 
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID = @TabuleiroID and
			StatusID = 1
	End
	Else
	Begin
		--Select '2'
		Select TOP(1)
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
		From 
			Rede.TabuleiroUsuario
		Where
			UsuarioID = @UsuarioID and
			StatusID = 1
		Order By
			TabuleiroID
	End

End -- Sp

go
Grant Exec on spC_TabuleiroUsuario To public
go

--Exec spC_TabuleiroUsuario @UsuarioID=2582, @TabuleiroID=9




