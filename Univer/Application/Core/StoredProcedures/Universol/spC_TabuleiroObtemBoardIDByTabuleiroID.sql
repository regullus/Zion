use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroObtemBoardIDByTabuleiroID'))
   Drop Procedure spC_TabuleiroObtemBoardIDByTabuleiroID
go

Create  Proc [dbo].[spC_TabuleiroObtemBoardIDByTabuleiroID]
    @UsuarioID int,
    @TabuleiroID int
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem BoardID de um usuario em um tabuleiro
-- =============================================================================================

BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on
    
	Declare @retorno nvarchar(255)

	Select 
		@retorno = BoardID
	From 
		Rede.TabuleiroUsuario
	Where
		UsuarioID = @UsuarioID and
		TabuleiroID = @TabuleiroID

    if (@retorno is null)
	Begin
		set @retorno = 0
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroObtemBoardIDByTabuleiroID To public
go

--Exec spC_TabuleiroObtemBoardIDByTabuleiroID @UsuarioID=2591,@TabuleiroId=1




