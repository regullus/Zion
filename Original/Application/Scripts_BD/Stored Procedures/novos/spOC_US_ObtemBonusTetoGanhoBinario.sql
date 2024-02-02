IF OBJECT_ID('dbo.spOC_US_ObtemBonusTetoGanhoBinario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[spOC_US_ObtemBonusTetoGanhoBinario];
GO

CREATE PROCEDURE spOC_US_ObtemBonusTetoGanhoBinario (@idUsuario int)
AS

	BEGIN TRY

		select dbo.fn_ObtemBonusTetoGanhoBinario(@idUsuario)

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de spOC_US_ObtemBonusTetoGanhoBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
