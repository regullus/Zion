IF OBJECT_ID('dbo.spOC_US_ObtemBonusTetoGanhoAlavancagem', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[spOC_US_ObtemBonusTetoGanhoAlavancagem];
GO

CREATE Proc [dbo].[spOC_US_ObtemBonusTetoGanhoAlavancagem]
   @idUsuario int

As
-- =============================================================================================
-- Author.....: Vinicius Castro
-- Create date: 08/07/2019
-- Description: Obtem o teto do bonus ganho por alavancagem 
-- =============================================================================================
BEGIN

	BEGIN TRY

		select dbo.fn_ObtemBonusTetoGanhoAlavancagem(@idUsuario)

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de spOC_US_ObtemBonusTetoGanhoAlavancagem: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
END