SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE proc [dbo].[sp_OC_SUP_Resposta]

@SuporteId int,
@Administriador int,
@Guid uniqueidentifier,
@Texto nvarchar(max)

AS

UPDATE sistema.Suporte set Respondido = 1 where id = @SuporteId;

INSERT INTO sistema.SuporteMensagem(SuporteID, Guid, Texto, Data, AdministradorID) VALUES (@SuporteId, @Guid, @Texto, dbo.GetDateZion(), @Administriador);

SELECT @@IDENTITY;
