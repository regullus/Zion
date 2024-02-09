use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_Classificacao_Geral'))
   Drop Procedure spOC_US_Classificacao_Geral
go

Create Proc [dbo].spOC_US_Classificacao_Geral
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

go
Grant Exec on spOC_US_Classificacao_Geral To public
go
