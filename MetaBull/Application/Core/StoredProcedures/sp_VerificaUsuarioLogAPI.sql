/****** Object:  StoredProcedure [dbo].[sp_VerificaUsuarioLogAPI]    Script Date: 12/13/2017 16:07:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================================================================
-- Author.....: 
-- Create date: 13/12/2017
-- Description: 
-- =============================================================================================

CREATE PROC [dbo].[sp_VerificaUsuarioLogAPI]
	@UsuarioId int, 
	@ExternoID int, 
	@ActionName nvarchar(50),
	@ControllerName nvarchar(50),
	@Mensagem nvarchar(1000), 
	@Objeto nvarchar(4000)
AS   
BEGIN
  
    DECLARE @count INT,
		@id INT 
    
    SELECT 
		@count	= COUNT('X'),
		@id		= MAX(ID)
	FROM
		Usuario.LogAPI (NOLOCK)
	WHERE
		(UsuarioID = @UsuarioID OR @UsuarioID IS NULL)
		AND (ExternoID = @ExternoID OR @ExternoID IS NULL)
		AND ActionName = @ActionName
		AND ControllerName = @ControllerName
		AND Mensagem = @Mensagem
		AND Objeto = @Objeto;
	
	IF (@count > 0)
	BEGIN	
		UPDATE
			Usuario.LogAPI
		SET
			Quantidade = ISNULL(Quantidade, 0) + 1,
			UltimaData = dbo.GetDateZion()
		WHERE
			ID = @id;
	
	END
	
	SELECT @count AS Total
END
