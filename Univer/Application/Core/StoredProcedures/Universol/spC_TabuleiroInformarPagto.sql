use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformarPagto'))
   Drop Procedure spC_TabuleiroInformarPagto
go

Create  Proc [dbo].[spC_TabuleiroInformarPagto]
   @UsuarioID int,
   @BoardID int,
   @UsuarioIDPag int

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
    
    declare 
        @tempoIni datetime,
        @tempoFim datetime

    Set
        @tempoIni = GetDate()

    Select 
        @tempoFim = DATEADD(mi, 60, DataInicio)
    From 
        rede.TabuleiroUsuario (nolock)
    where 
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 

    if(@tempoFim > @tempoIni)
    Begin
        Update
            rede.TabuleiroUsuario 
        Set
            InformePag = 1,
            UsuarioIDPag = @UsuarioIDPag
        where 
            UsuarioID = @UsuarioID and 
            BoardID  = @BoardID 
            
        Select 'OK'
    End
    Else
    Begin
        Select 'NOOK'
    End

End -- Sp

go
Grant Exec on spC_TabuleiroInformarPagto To public
go

--Exec spC_TabuleiroInformarPagto @UsuarioID = 2587, @TabuleiroID = 1, @UsuarioIDPag = 2580

