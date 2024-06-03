use univerDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('sp_I_SuporteNovoChamado'))
   Drop Procedure sp_I_SuporteNovoChamado
go
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE proc [dbo].[sp_I_SuporteNovoChamado]
    @UsuarioId int,
    @Guid uniqueidentifier,
    @Assunto nvarchar(128),
    @Texto nvarchar(max)

AS
BEGIN
    DECLARE @SuporteID int

    INSERT INTO sistema.Suporte(UsuarioID, Data, Assunto, Respondido) VALUES (@UsuarioID, getdate(), @Assunto, 0);
    INSERT INTO sistema.SuporteMensagem(SuporteID, Guid, Texto, Data) VALUES (@@IDENTITY, @Guid, @Texto, getdate());

    SELECT @@IDENTITY;
END

go
Grant Exec on sp_I_SuporteNovoChamado To public
go
