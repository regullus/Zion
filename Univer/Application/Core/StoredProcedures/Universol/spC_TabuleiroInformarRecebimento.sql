use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformarRecebimento'))
   Drop Procedure spC_TabuleiroInformarRecebimento
go

Create  Proc [dbo].[spC_TabuleiroInformarRecebimento]
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
    
    Update
        rede.TabuleiroUsuario 
    Set
        PagoMaster = 1
    where 
        UsuarioID = @UsuarioID and 
        TabuleiroID  = @TabuleiroID

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroInformarRecebimento To public
go

Exec spC_TabuleiroInformarRecebimento @UsuarioID = 2580, @TabuleiroID = 1

--select * from rede.TabuleiroUsuario where UsuarioId = 2587
