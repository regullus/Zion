SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP FUNCTION [dbo].[fn_Get_Uplines_Diretos]

GO

CREATE FUNCTION [dbo].[fn_Get_Uplines_Diretos](@idUsuario int, @QtdeNiveis int)
RETURNS @tempFranqueados TABLE (UserID int, Nivel int)
AS

BEGIN

	WITH Franqueado_Patrocinio_CTE AS
	(

		SELECT FILHO.ID, FILHO.PatrocinadorDiretoID, 0 as RecursionLevel 
		FROM Usuario.Usuario FILHO
		--INNER JOIN Usuario.Usuario filho ON pai.ID = filho.PatrocinadorDiretoID
		WHERE FILHO.ID = @idUsuario
			 
		UNION ALL
		 
		SELECT PAI.ID, PAI.PatrocinadorDiretoID, (RecursionLevel + 1) AS RecursionLevel 
		FROM Usuario.Usuario PAI
		--INNER JOIN Usuario.Usuario PAI_DIRETO ON PAI.PatrocinadorDiretoID = PAI_DIRETO.ID
		INNER JOIN Franqueado_Patrocinio_CTE FILHO on PAI.ID = FILHO.PatrocinadorDiretoID
		WHERE (RecursionLevel + 1) <= @QtdeNiveis
	)

	INSERT INTO @tempFranqueados 
	SELECT ID, RecursionLevel FROM Franqueado_Patrocinio_CTE WHERE ID <> @idUsuario
	OPTION (MAXRECURSION 9999)
	
	RETURN
END





