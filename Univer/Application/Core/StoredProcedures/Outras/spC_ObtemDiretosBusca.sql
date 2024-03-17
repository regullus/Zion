USE [SRMRACCO]
GO
/****** Object:  StoredProcedure [dbo].[spC_ObtemDiretosBusca]    Script Date: 12/07/2017 11:43:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem os IDs dos usuários diretos, de acordo com o filtro de Login/Nome
-- =============================================================================================

-- spC_ObtemDiretosBusca 44429, 'Lucas', NULL, NULL

ALTER PROC [dbo].[spC_ObtemDiretosBusca]
   @UsuarioID INT,
   @Login NVARCHAR(50),
   @Nome NVARCHAR(100),
   @CPF NVARCHAR(100)
AS
BEGIN

	SET NOCOUNT ON

	SELECT
		U.ID,
		U.Login,
		U.Nome,
		CAST((LEN(U.Assinatura) - LEN(UT.Assinatura) + 1) AS INT) AS Nivel,
		CASE WHEN 
			(UT.Assinatura + '0') = LEFT(U.Assinatura, LEN(UT.Assinatura) + 1)
			THEN 'Esquerda' ELSE 'Direita' 
		END AS Lado
	FROM
		Usuario.Usuario U (NOLOCK)
		INNER JOIN Usuario.Usuario UT (NOLOCK)
			ON UT.Assinatura = LEFT(U.Assinatura, LEN(UT.Assinatura))
	WHERE
		UT.ID = @UsuarioID
		AND (
			(@Login IS NOT NULL AND U.Login IS NOT NULL AND U.Login LIKE '%' + @Login + '%')
			OR (@Nome IS NOT NULL AND U.Nome IS NOT NULL AND U.Nome LIKE '%' + @Nome + '%')	
			OR (@CPF IS NOT NULL AND U.Documento IS NOT NULL AND U.Documento = @CPF)	
		)
	ORDER BY
		LEN(U.Assinatura);
END
