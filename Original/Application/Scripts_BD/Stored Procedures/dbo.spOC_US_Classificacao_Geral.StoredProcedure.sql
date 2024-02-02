USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_Classificacao_Geral]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create Proc [dbo].[spOC_US_Classificacao_Geral]
    @_CicloID NVARCHAR(10)

As
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: 
--
-- =============================================================================================

BEGIN
    DECLARE @CicloID NVARCHAR(10) = @_CicloID;
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;

    DECLARE @UsuarioID NVARCHAR(10),
            @SQLExec NVARCHAR(4000);

    SELECT DISTINCT UsuarioID
    INTO #TMP
    FROM Usuario.Pontos (NOLOCK)
    WHERE CicloID = @CicloID
          AND VT > 0;

    CREATE NONCLUSTERED INDEX _IXTMP ON #TMP ([UsuarioID]);

    WHILE EXISTS (SELECT 1 FROM #TMP)
    BEGIN
      SELECT @UsuarioID = MAX(UsuarioID)
        FROM #TMP;

        EXEC('spOC_US_Classificacao ' + @UsuarioID + ', ' + @CicloID) 

      DELETE FROM #TMP WHERE UsuarioID = @UsuarioID;

    --PRINT @UsuarioID;
    END;

END;


GO
