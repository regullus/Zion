use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spI_TabuleiroUltimoAcesso'))
   Drop Procedure spI_TabuleiroUltimoAcesso
go

Create  Proc [dbo].[spI_TabuleiroUltimoAcesso]
   @UsuarioID int,
   @Chamada nvarchar(50)

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Inclui os 8 registros do usuario que inicia no sistema
-- Observacao: Só deve rodar essa sp no inicio, onde o usuario acaba de aceitar um convite para entrar no sistema
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
	Set FMTONLY OFF
	Set nocount on
	if Exists (Select 'OK' From Rede.TabuleiroUltimoAcesso Where UsuarioID = @UsuarioID)
    Begin
        Update
            Rede.TabuleiroUltimoAcesso
        Set
            TempoProcess = getdate(),
            Chamada = @Chamada,
            Total = Total + 1
        Where
            UsuarioID = @UsuarioID
    End
    Else
    Begin
        Insert Into 
            Rede.TabuleiroUltimoAcesso
        Select
            @UsuarioID,
            @Chamada,
            0,
            GetDate()
    End
    
    Select 'OK' Retorno

End -- Sp

go
Grant Exec on spI_TabuleiroUltimoAcesso To public
go

Exec spI_TabuleiroUltimoAcesso @UsuarioID=2580, @Chamada = 'GetInvite'

