use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformarPagtoSistema'))
   Drop Procedure spC_TabuleiroInformarPagtoSistema
go

Create  Proc [dbo].[spC_TabuleiroInformarPagtoSistema]
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
    
    Update
        rede.TabuleiroUsuario 
    Set
        PagoSistema = 1
    where 
        UsuarioID = @UsuarioID and 
        TabuleiroID  = @BoardID

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroInformarPagtoSistema To public
go

--Exec spC_TabuleiroInformarPagtoSistema @UsuarioID = 2580, @BoardID = 1


