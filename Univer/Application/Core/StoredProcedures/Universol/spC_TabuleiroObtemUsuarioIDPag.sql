use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroObtemUsuarioIDPag'))
   Drop Procedure spC_TabuleiroObtemUsuarioIDPag
go

Create  Proc [dbo].[spC_TabuleiroObtemUsuarioIDPag]
   @UsuarioID int,
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
    Declare @UsuarioIDPag int

    Select
        @UsuarioIDPag = UsuarioIDPag
    From
        Rede.TabuleiroUsuario
    Where
        UsuarioID = @UsuarioID and
        BoardID = @BoardID
    if(@UsuarioIDPag is null)
    Begin
        set @UsuarioIDPag = 0
    End
    Select @UsuarioIDPag UsuarioIDPag
     
End -- Sp

go
Grant Exec on spC_TabuleiroObtemUsuarioIDPag To public
go

Exec spC_TabuleiroObtemUsuarioIDPag @UsuarioID=2642,@BoardID=1


