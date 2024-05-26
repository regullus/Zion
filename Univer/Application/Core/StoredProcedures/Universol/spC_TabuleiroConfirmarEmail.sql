use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroConfirmarEmail'))
   Drop Procedure spC_TabuleiroConfirmarEmail
go

Create  Proc [dbo].[spC_TabuleiroConfirmarEmail]
   @UsuarioID int
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: 
-- =============================================================================================
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

    Update
        Usuario.Usuario
    Set
        StatusEmailID = 2
    Where
        ID = @UsuarioID

    Select 'OK' Retorno
	    
End -- Sp

go
Grant Exec on spC_TabuleiroConfirmarEmail To public
go

--Exec spC_TabuleiroConfirmarEmail @tipo='InformePagtoSistema'
--Exec spC_TabuleiroConfirmarEmail @tipo='ConfirmarYellow'
--Exec spC_TabuleiroConfirmarEmail @tipo='ConfirmarRed'
--Exec spC_TabuleiroConfirmarEmail @tipo='PagosSistema'
--Exec spC_TabuleiroConfirmarEmail @tipo='InformePagtoMaster'
--Exec spC_TabuleiroConfirmarEmail @tipo='PagoMaster'
Exec spC_TabuleiroConfirmarEmail @UsuarioID = 2000

