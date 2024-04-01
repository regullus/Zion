use Univer
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
        TabuleiroID = @TabuleiroID
    
End -- Sp

go
Grant Exec on spC_TabuleiroUsuario To public
go

Exec spC_TabuleiroUsuario @UsuarioId = 2587, @TabuleiroID = 1






