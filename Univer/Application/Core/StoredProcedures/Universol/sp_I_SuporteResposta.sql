use univerDev
go

If Exists (Select 'Sp' From sysobjects Where id = object_id('sp_I_SuporteResposta'))
   Drop Procedure sp_I_SuporteResposta
go
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE proc [dbo].[sp_I_SuporteResposta]
    @SuporteId int,
    @Administriador int,
    @Guid uniqueidentifier,
    @Texto nvarchar(max)

AS
BEGIN
UPDATE sistema.Suporte set Respondido = 1 where id = @SuporteId;

INSERT INTO sistema.SuporteMensagem(SuporteID, Guid, Texto, Data, AdministradorID) VALUES (@SuporteId, @Guid, @Texto, getdate(), @Administriador);

SELECT @@IDENTITY;

END

go
Grant Exec on sp_I_SuporteNovoChamado To public
go