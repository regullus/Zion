use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_RedeUpline_Ciclo'))
   Drop Procedure spOC_US_RedeUpline_Ciclo
go

Create Proc [dbo].spOC_US_RedeUpline_Ciclo
    @CicloID INT,
    @UsuarioID INT = NULL,
    @PatrocinadorID INT = NULL

As
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: 
--
-- =============================================================================================

BEGIN

    CREATE TABLE #Processado
    (
        UsuarioID INT,
        PatrocinadorID INT
    );

    CREATE TABLE #Processar
    (
        UsuarioID INT,
        PatrocinadorID INT
    );

    DELETE FROM #Processado;
    DELETE FROM #Processar;

    SET NOCOUNT ON;

    IF @UsuarioID IS NULL
       OR @PatrocinadorID IS NULL
    BEGIN
        DELETE FROM Rede.Upline_Ciclo
        WHERE CicloID = @CicloID;
        INSERT INTO #Processar
        VALUES
        (1000, NULL);
    END;
    ELSE
    BEGIN
        DELETE FROM Rede.Upline_Ciclo
        WHERE UsuarioID = @UsuarioID;
        INSERT INTO #Processar
        VALUES
        (@UsuarioID, @PatrocinadorID);
    END;

    WHILE EXISTS (SELECT 1 FROM #Processar)
    BEGIN

        DELETE FROM #Processado;

        INSERT INTO Rede.Upline_Ciclo
        SELECT P.UsuarioID,
               U.Upline,
               U.Nivel + 1 AS Nivel,
               @CicloID
        FROM Rede.Upline_Ciclo AS U
            INNER JOIN #Processar AS P
                ON U.UsuarioID = P.PatrocinadorID
		WHERE U.CicloID = @CicloID;

        INSERT INTO #Processado
        SELECT U.ID AS UsuarioID,
               P.UsuarioID AS PatrocinadorID
        FROM Usuario.Usuario (NOLOCK) AS U
            INNER JOIN #Processar AS P
                ON U.PatrocinadorDiretoID = P.UsuarioID
        WHERE U.ID <> 1000;

        INSERT INTO Rede.Upline_Ciclo
        SELECT UsuarioID,
               PatrocinadorID,
               0 AS Nivel,
               @CicloID AS CicloID
        FROM #Processado;

        DELETE FROM #Processar;

        INSERT INTO #Processar
        SELECT *
        FROM #Processado;

    END;

    SET NOCOUNT OFF;

    DROP TABLE #Processado;
    DROP TABLE #Processar;

END;

go
Grant Exec on spOC_US_CalculaPontos To public
go
