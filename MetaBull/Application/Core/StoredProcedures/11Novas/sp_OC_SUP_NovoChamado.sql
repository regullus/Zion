SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE proc [dbo].[sp_OC_SUP_NovoChamado]

@UsuarioId int,
@Guid uniqueidentifier,
@Assunto nvarchar(128),
@Texto nvarchar(max)

AS

DECLARE @SuporteID int

INSERT INTO sistema.Suporte(UsuarioID, Data, Assunto, Respondido) VALUES (@UsuarioID, dbo.GetDateZion(), @Assunto, 0);
INSERT INTO sistema.SuporteMensagem(SuporteID, Guid, Texto, Data) VALUES (@@IDENTITY, @Guid, @Texto, dbo.GetDateZion());

SELECT @@IDENTITY;
