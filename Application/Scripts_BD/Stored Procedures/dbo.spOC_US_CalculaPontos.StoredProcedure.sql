USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_CalculaPontos]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[spOC_US_CalculaPontos]
    @idUsuario INT = 0,
    @CicloID INT = 0
AS
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    SET FMTONLY OFF;
    SET NOCOUNT ON;

    DECLARE @DataInicioCiclo DATETIME;
    DECLARE @DataFimCiclo DATETIME;

    IF (@CicloID = 0)
    BEGIN

        -- Buscar Ciclo Atual Ativo
        SELECT @CicloID = C.ID,
               @DataInicioCiclo = C.DataInicial,
               @DataFimCiclo = C.DataFinal
        FROM Rede.Ciclo C (NOLOCK)
        WHERE C.ID =
        (
            SELECT MAX(ID) FROM Rede.Ciclo (NOLOCK) WHERE Ativo = 1
        );

    END;
    ELSE
    BEGIN

        -- Buscar Ciclo Atual Ativo
        SELECT @DataInicioCiclo = DataInicial,
               @DataFimCiclo = DataFinal
        FROM Rede.Ciclo (NOLOCK)
        WHERE ID = @CicloID;

    END;

    IF (@idUsuario = 0)
    BEGIN

        INSERT INTO Usuario.Pontos
		( UsuarioID, CicloID, VP, VB, VL, VQ, VKI, VPQ, VLQ, VBQ, VKIQ )
        SELECT Usr.ID,
               @CicloID,
               0,
               0,
               0,
               0,
               0,
			   0,
			   0,
			   0,
			   0
        FROM Usuario.Usuario (NOLOCK) Usr
            LEFT JOIN Usuario.Pontos (NOLOCK) Pto
                ON Pto.UsuarioID = Usr.ID
                   AND Pto.CicloID = @CicloID
        WHERE Pto.UsuarioID IS NULL;

        UPDATE Usuario.Pontos
        SET 
			VP = 0,
			VKI = 0,
			VPQ = 0
        WHERE CicloID = @CicloID;

    END;
    ELSE
    BEGIN

        INSERT INTO Usuario.Pontos
		( UsuarioID, CicloID, VP, VB, VL, VQ, VKI, VPQ, VLQ, VBQ, VKIQ )
        SELECT Usr.ID,
               @CicloID,
               0,
               0,
               0,
               0,
               0,
			   0,
			   0,
			   0,
			   0
        FROM Usuario.Usuario (NOLOCK) Usr
            LEFT JOIN Usuario.Pontos (NOLOCK) Pto
                ON Pto.UsuarioID = Usr.ID
                   AND Pto.CicloID = @CicloID
        WHERE Pto.UsuarioID IS NULL
              AND Usr.ID = @idUsuario;

        UPDATE Usuario.Pontos
        SET 
			VP = 0,
			VKI = 0,
			VPQ = 0
        WHERE CicloID = @CicloID
              AND UsuarioID = @idUsuario;

    END;

    IF (@idUsuario = 0)
    BEGIN
        -- Pontos Pessoais
        SELECT Pontos.UsuarioID,
               VP = SUM(ISNULL(PtoRec.VP, 0))
        INTO #PP
        FROM Usuario.Pontos (NOLOCK)
            LEFT JOIN Rede.Pontos PtoRec (NOLOCK)
                ON PtoRec.UsuarioID = Pontos.UsuarioID
                   AND PtoRec.ReferenciaID = PtoRec.UsuarioID
                   AND PtoRec.CicloID = Pontos.CicloID
        WHERE Pontos.CicloID = @CicloID
        GROUP BY Pontos.UsuarioID;

		CREATE INDEX IXPP ON #PP (UsuarioID);

        UPDATE Usuario.Pontos
        SET VP = #PP.VP
        FROM Usuario.Pontos
            INNER JOIN #PP
                ON #PP.UsuarioID = Pontos.UsuarioID
        WHERE CicloID = @CicloID;

		UPDATE UPontos
		SET UPontos.VPQ = CASE WHEN TblVPQ.VPQ < 0 THEN 0 ELSE TblVPQ.VPQ END
		FROM 
			Usuario.Pontos UPontos
			INNER JOIN (
				SELECT 
					PRec.UsuarioID,
					PRec.CicloID,
					SUM(ISNULL(PRec.VP, 0)) AS [VPQ]
				FROM
					#PP PP
					INNER JOIN Rede.Pontos PRec (NOLOCK)
						ON PRec.UsuarioID = PP.UsuarioID
						AND PRec.ReferenciaID = PRec.UsuarioID
						AND PRec.CicloID = @CicloID
				GROUP BY
					PRec.UsuarioID,
					PRec.CicloID
			) TblVPQ
			ON 
				TblVPQ.UsuarioID = UPontos.UsuarioID
				AND TblVPQ.CicloID = UPontos.CicloID;
    END;
    ELSE
    BEGIN
        -- Pontos Pessoais
        SELECT Pontos.UsuarioID,
               VP = SUM(ISNULL(PtoRec.VP, 0))
        INTO #PPU
        FROM Usuario.Pontos (NOLOCK)
            LEFT JOIN Rede.Pontos PtoRec (NOLOCK)
                ON PtoRec.UsuarioID = Pontos.UsuarioID
                   AND PtoRec.ReferenciaID = PtoRec.UsuarioID
                   AND PtoRec.CicloID = Pontos.CicloID
        WHERE Pontos.UsuarioID = @idUsuario
              AND Pontos.CicloID = @CicloID
        GROUP BY Pontos.UsuarioID;

		CREATE INDEX IXPPU ON #PPU (UsuarioID);

        UPDATE Usuario.Pontos
        SET VP = #PPU.VP
        FROM Usuario.Pontos
            INNER JOIN #PPU
                ON #PPU.UsuarioID = Pontos.UsuarioID
        WHERE CicloID = @CicloID;

		UPDATE UPontos
		SET UPontos.VPQ = CASE WHEN TblVPQ.VPQ < 0 THEN 0 ELSE TblVPQ.VPQ END
		FROM 
			Usuario.Pontos UPontos
			INNER JOIN (
				SELECT 
					PRec.UsuarioID,
					PRec.CicloID,
					SUM(ISNULL(PRec.VP, 0)) AS [VPQ]
				FROM
					Rede.Pontos PRec (NOLOCK)
				WHERE
					PRec.UsuarioID = @idUsuario
					AND PRec.ReferenciaID = PRec.UsuarioID
					AND PRec.CicloID = @CicloID
				GROUP BY
					PRec.UsuarioID,
					PRec.CicloID
			) TblVPQ
			ON 
				TblVPQ.UsuarioID = UPontos.UsuarioID
				AND TblVPQ.CicloID = UPontos.CicloID;
    END;
    ----

    IF (@idUsuario = 0)
    BEGIN

        DELETE FROM Usuario.PontosLinha
        WHERE CicloID = @CicloID;

        -- Pontos por Linha
        INSERT INTO Usuario.PontosLinha
		( UsuarioID, DiretoID, CicloID, VP, VBQ, VG, VML, VQ, VPQ, VGQ, VB)
        SELECT Usr.ID,
               Dir.ID,
               @CicloID,
               0,
               0,
			   0,
			   0,
               0,
               0,
               0,
			   0
        FROM Usuario.Usuario (NOLOCK) Usr
        INNER JOIN Usuario.Usuario (NOLOCK) Dir
        ON Dir.PatrocinadorDiretoID = Usr.ID
        AND Dir.ID                   <> Usr.ID
        LEFT JOIN Usuario.PontosLinha (NOLOCK) Linha
        ON Linha.UsuarioID          = Usr.ID
        AND Linha.DiretoID           = Dir.ID
        AND Linha.CicloID            = @CicloID
        WHERE Linha.UsuarioID IS NULL;

        UPDATE Usuario.PontosLinha
        SET VP = 0,
            VBQ = 0,
            VG = 0,
            VML = 0,
            VQ = 0
        WHERE CicloID = @CicloID;

        SELECT Linha.UsuarioID,
               Linha.DiretoID,
               VP = SUM(   CASE
                               WHEN (PtoRec.ReferenciaID = Linha.DiretoID) THEN
                                   PtoRec.VP
                               ELSE
                                   0
                           END
                       ),
               VG = SUM(   CASE
                               WHEN (PtoRec.ReferenciaID <> Linha.DiretoID) THEN
                                   PtoRec.VP
                               ELSE
                                   0
                           END
                       )
        INTO #PL
        FROM Rede.Pontos PtoRec (NOLOCK)
            INNER JOIN Usuario.PontosLinha Linha (NOLOCK)
                ON Linha.DiretoID = PtoRec.UsuarioID
                   AND Linha.CicloID = PtoRec.CicloID
        WHERE PtoRec.CicloID = @CicloID
        GROUP BY Linha.UsuarioID,
                 Linha.DiretoID;

        CREATE NONCLUSTERED INDEX IXTMP
        ON [dbo].[#PL]
        (
            [UsuarioID],
            [DiretoID]
        )
        INCLUDE
        (
            [VP],
            [VG]
        );

		CREATE INDEX IXPL ON [#PL] (UsuarioID);

        UPDATE Usuario.PontosLinha
        SET VP = #PL.VP,
            VG = #PL.VG
        FROM Usuario.PontosLinha
            INNER JOIN #PL
                ON #PL.UsuarioID = PontosLinha.UsuarioID
                   AND #PL.DiretoID = PontosLinha.DiretoID
        WHERE PontosLinha.CicloID = @CicloID;

        -- Somar VT das Linhas ao VT
        SELECT Pontos.UsuarioID,
               VB = SUM(Linha.VBQ),
               VL = SUM(Linha.VT)
        INTO #PLT
        FROM Usuario.Pontos (NOLOCK)
            INNER JOIN Usuario.PontosLinha Linha (NOLOCK)
                ON Linha.UsuarioID = Pontos.UsuarioID
                   AND Linha.CicloID = Pontos.CicloID
        WHERE Pontos.CicloID = @CicloID
        GROUP BY Pontos.UsuarioID;

		CREATE INDEX IXPL ON [#PLT] (UsuarioID);

        UPDATE Usuario.Pontos
        SET VB = #PLT.VB,
            VL = #PLT.VL
        FROM Usuario.Pontos
            INNER JOIN #PLT
                ON #PLT.UsuarioID = Pontos.UsuarioID
        WHERE Pontos.CicloID = @CicloID;

    END;
    ELSE
    BEGIN
        DELETE FROM Usuario.PontosLinha
        WHERE UsuarioID = @idUsuario
              AND CicloID = @CicloID;

        -- Pontos por Linha
        INSERT INTO Usuario.PontosLinha
		( UsuarioID, DiretoID, CicloID, VP, VBQ, VG, VML, VQ, VPQ, VGQ, VB)
        SELECT Usr.ID,
               Dir.ID,
               @CicloID,
               0,
               0,
			   0,
			   0,
               0,
               0,
			   0,
               0
        FROM Usuario.Usuario (NOLOCK) Usr
        INNER JOIN Usuario.Usuario (NOLOCK) Dir
        ON Dir.PatrocinadorDiretoID = Usr.ID
        AND Dir.ID                   <> Usr.ID
        LEFT JOIN Usuario.PontosLinha (NOLOCK) Linha
        ON Linha.UsuarioID          = Usr.ID
        AND Linha.DiretoID           = Dir.ID
        AND Linha.CicloID            = @CicloID
        WHERE Linha.UsuarioID IS NULL
			AND Usr.ID = @idUsuario;

        UPDATE Usuario.PontosLinha
        SET VP = 0,
            VBQ = 0,
            VG = 0,
            VML = 0,
            VQ = 0
        WHERE CicloID = @CicloID
              AND (UsuarioID = @idUsuario);

        SELECT Linha.UsuarioID,
               Linha.DiretoID,
               VP = SUM(   CASE
                               WHEN (PtoRec.ReferenciaID = Linha.DiretoID) THEN
                                   PtoRec.VP
                               ELSE
                                   0
                           END
                       ),
               VG = SUM(   CASE
                               WHEN (PtoRec.ReferenciaID <> Linha.DiretoID) THEN
                                   PtoRec.VP
                               ELSE
                                   0
                           END
                       )
        INTO #PLU
        FROM Rede.Pontos PtoRec (NOLOCK)
            INNER JOIN Usuario.PontosLinha Linha (NOLOCK)
                ON Linha.DiretoID = PtoRec.UsuarioID
                   AND Linha.CicloID = PtoRec.CicloID
        WHERE (Linha.UsuarioID = @idUsuario)
              AND PtoRec.CicloID = @CicloID
        GROUP BY Linha.UsuarioID,
                 Linha.DiretoID;

		CREATE INDEX IXPLU ON [#PLU] (UsuarioID);

        UPDATE Usuario.PontosLinha
        SET VP = #PLU.VP,
            VG = #PLU.VG
        FROM Usuario.PontosLinha
            INNER JOIN #PLU
                ON #PLU.UsuarioID = PontosLinha.UsuarioID
                   AND #PLU.DiretoID = PontosLinha.DiretoID
        WHERE PontosLinha.CicloID = @CicloID;

        -- Somar VT das Linhas ao VT
        SELECT Pontos.UsuarioID,
               VB = SUM(Linha.VBQ),
               VL = SUM(Linha.VT)
        INTO #PLTU
        FROM Usuario.Pontos (NOLOCK)
            INNER JOIN Usuario.PontosLinha Linha (NOLOCK)
                ON Linha.UsuarioID = Pontos.UsuarioID
                   AND Linha.CicloID = Pontos.CicloID
        WHERE Pontos.CicloID = @CicloID
              AND (
                      Pontos.UsuarioID = @idUsuario
                      OR @idUsuario = 0
                  )
        GROUP BY Pontos.UsuarioID;

		CREATE INDEX IXPLTU ON [#PLTU] (UsuarioID);

        UPDATE Usuario.Pontos
        SET VB = #PLTU.VB,
            VL = #PLTU.VL
        FROM Usuario.Pontos
            INNER JOIN #PLTU
                ON #PLTU.UsuarioID = Pontos.UsuarioID
        WHERE Pontos.CicloID = @CicloID;


    END;

    SET NOCOUNT OFF;


END;


GO
