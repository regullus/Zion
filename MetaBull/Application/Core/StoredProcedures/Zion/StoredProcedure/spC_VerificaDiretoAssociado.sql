create proc spC_VerificaDiretoAssociado 
@idUsuario int, 
@QtdeNiveis int

AS

BEGIN

	DECLARE @Count int = 0;

	WITH Franqueado_Patrocinio_CTE AS
	(

		SELECT pai.ID, pai.PatrocinadorDiretoID, pai.NivelAssociacao as Associacao, pai.StatusID as status, 0 as RecursionLevel 
		FROM Usuario.Usuario (NOLOCK) pai 
		--INNER JOIN Usuario.Usuario filho ON pai.ID = filho.PatrocinadorDiretoID
		WHERE pai.ID = @idUsuario
			 
		UNION ALL
		 
		SELECT FILHO.ID, FILHO.PatrocinadorDiretoID, FILHO.NivelAssociacao as Associacao, FILHO.StatusID as status, (RecursionLevel + 1) AS RecursionLevel 
		FROM Usuario.Usuario (NOLOCK) FILHO  
		INNER JOIN Usuario.Usuario (NOLOCK) PAI_DIRETO   ON FILHO.PatrocinadorDiretoID = PAI_DIRETO.ID
		INNER JOIN Franqueado_Patrocinio_CTE PAI   on PAI.ID = FILHO.PatrocinadorDiretoID
		WHERE (RecursionLevel + 1) <= @QtdeNiveis
		AND FILHO.StatusID = 2
	)

	SELECT CONVERT(bit, IIF(count(0) > 0, 1, 0))  FROM Franqueado_Patrocinio_CTE WHERE ID <> @idUsuario
	
END





