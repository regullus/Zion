use univerDev
go

If Exists (Select 'Sp' From sysobjects Where id = object_id('sp_I_SuporteNovaInteracao'))
   Drop Procedure sp_I_SuporteNovaInteracao
go
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE proc [dbo].[sp_I_SuporteNovaInteracao]
    @SuporteId int,
    @Guid uniqueidentifier,
    @Texto nvarchar(max)

AS
BEGIN
if exists (Select 'OK' From sistema.SuporteMensagem where SuporteID = @SuporteId)
Begin
    INSERT INTO sistema.SuporteMensagem(SuporteID, Guid, Texto, Data) VALUES (@SuporteId, @Guid, @Texto, getdate());
    SELECT @@IDENTITY;
End
Else
Begin
    SELECT 0;
End

END

go
Grant Exec on sp_I_SuporteNovoChamado To public
go

--Begin tran

--Select * from Sistema.SuporteMensagem

--EXEC sp_I_SuporteNovaInteracao '3', 'd54da8aa-cd69-4707-989b-6a2d8b58356e', N'banana'
--Select * from Sistema.SuporteMensagem
--Rollback tran