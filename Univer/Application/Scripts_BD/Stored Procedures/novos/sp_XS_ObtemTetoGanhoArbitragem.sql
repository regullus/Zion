IF OBJECT_ID('dbo.sp_XS_ObtemTetoGanhoArbitragem', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_ObtemTetoGanhoArbitragem];
GO

CREATE Proc [dbo].[sp_XS_ObtemTetoGanhoArbitragem]
   @idUsuario int

As
-- =============================================================================================
-- Create date: 02/12/2019
-- Description: Obtem o teto do bonus ganho por arbitragem
-- =============================================================================================
BEGIN

	BEGIN TRY

		select dbo.fn_ObtemBonusTetoGanhoArbitragem(@idUsuario)

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_XS_ObtemTetoGanhoArbitragem: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 
END