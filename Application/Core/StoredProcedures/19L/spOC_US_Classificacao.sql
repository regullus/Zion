use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_Classificacao'))
   Drop Procedure spOC_US_Classificacao
go

Create Proc [dbo].spOC_US_Classificacao
     @idUsuario INT, 
     @CicloID INT = 0

As
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: 
--
-- =============================================================================================

BEGIN
	DECLARE @NivelUsuario INT;

	--Necessario para o entity reconhecer retorno de select com tabela temporaria
	SET FMTONLY OFF;
	SET NOCOUNT ON;

	DECLARE @DataInicioCiclo DATETIME;
	DECLARE @DataFimCiclo DATETIME;

	IF (@CicloID = 0)
	BEGIN
		-- Buscar Ciclo Atual Ativo
		SELECT @CicloID = MAX(ID)
			 , @DataInicioCiclo = MAX(DataInicial)
		  	 , @DataFimCiclo = MAX(DataFinal)
		FROM Rede.Ciclo(NOLOCK)
		WHERE Ativo = 1;
	END;
	ELSE
	BEGIN
		-- Buscar Ciclo Atual Ativo
		SELECT @DataInicioCiclo = MAX(DataInicial)
			, @DataFimCiclo = MAX(DataFinal)
		FROM Rede.Ciclo(NOLOCK)
		WHERE ID = @CicloID;
	END;

WITH CTO AS (
	SELECT Usr.ID
		, Nivel = ISNULL(Usr.NivelClassificacao,0)
		, PP = ISNULL(MAX(Pto.VPQ),0)
		, PKI = ISNULL(MAX(Pto.VKI),0)
		, Linhas = ISNULL(COUNT(1),0)
	FROM Usuario.Usuario Usr(NOLOCK)
	INNER JOIN Usuario.Pontos Pto(NOLOCK)
		ON Pto.UsuarioID = Usr.ID
	LEFT JOIN Usuario.PontosLinha Linha(NOLOCK)
		ON Linha.UsuarioID = Usr.ID
			AND Linha.CicloID = Pto.CicloID
	WHERE Usr.ID = @idUsuario
		AND Pto.CicloID = @CicloID
	GROUP BY Usr.ID
		, ISNULL(Usr.NivelClassificacao,0))
		SELECT * 
		  INTO #UsrLinhas 
		  FROM CTO

	-- Loop dos níveis existentes atuais
	DECLARE @Nivel INT
		, @Pontos INT
		, @LinhaMinimo INT
		, @VML INT;

	IF EXISTS ( SELECT 1 FROM #UsrLinhas )
	BEGIN
		DECLARE C_NIVEIS CURSOR FORWARD_ONLY
		FOR
		SELECT Atual.Nivel
			, New.Pontos
			, Vml.LinhaMinimo
			, Vml.VML
		FROM Rede.Classificacao Atual(NOLOCK)
		INNER JOIN Rede.Classificacao New(NOLOCK)
			ON New.Nivel = Atual.Nivel + 1
		INNER JOIN Rede.ClassificacaoVML Vml(NOLOCK)
			ON Vml.ClassificacaoID = New.ID
		ORDER BY New.Nivel;

		OPEN C_NIVEIS;

		FETCH NEXT
		FROM C_NIVEIS
		INTO @Nivel
			, @Pontos
			, @LinhaMinimo
			, @VML;

		DECLARE @VT_LINHAS INT = 0;
		DECLARE @VP INT = 0;
		DECLARE @VKI INT = 0;
		DECLARE @LINHAS INT = 0;
		DECLARE @PP_NECESSARIO INT = 0;

		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			SET @PP_NECESSARIO = 0;

			-- SELECT @Nivel, @Pontos, @LinhaMinimo, @VML
			SELECT @VP = MAX(Usr.PP)
				 , @VKI = MAX(Usr.PKI)
				 -- , @VT_LINHAS = SUM(ISNULL(CASE WHEN Linha.VT > @VML THEN @VML ELSE Linha.VT END,0))
				 , @VT_LINHAS = SUM(ISNULL(Linha.VT * @VML / 100,0))
				 , @LINHAS = COUNT(1)
			FROM #UsrLinhas Usr(NOLOCK)
			INNER JOIN Usuario.Pontos Pto(NOLOCK)
				ON Pto.UsuarioID = Usr.ID
			LEFT JOIN Usuario.PontosLinha Linha(NOLOCK)
				ON Linha.UsuarioID = Usr.ID
					AND Linha.CicloID = Pto.CicloID
			   AND Linha.VT > 0
			WHERE Pto.CicloID = @CicloID;

			-- Se tiver menos linhas a diferença precisa estar no VP
			IF (@LINHAS < @LinhaMinimo)
			BEGIN
				SET @PP_NECESSARIO = (@LinhaMinimo - @LINHAS) * @VML;

				IF ((@VP + @VKI) < @PP_NECESSARIO)
				BEGIN
					BREAK;
				END;
			END;

			IF ((@VT_LINHAS + @VP + @VKI) < @Pontos)
			BEGIN
				BREAK;
			END;

			--END
			FETCH NEXT
			FROM C_NIVEIS
			INTO @Nivel
				, @Pontos
				, @LinhaMinimo
				, @VML;
		END;

		CLOSE C_NIVEIS;

		DEALLOCATE C_NIVEIS;


			UPDATE P
			SET P.VQ = ISNULL(@VT_LINHAS, 0) + ISNULL(@VP, 0) + ISNULL(@VKI, 0)
			FROM Usuario.Pontos AS P
			INNER JOIN #UsrLinhas AS L
				ON L.ID = P.UsuarioID
			INNER JOIN Usuario.Usuario AS U
				ON U.ID = P.UsuarioID
			WHERE P.UsuarioID = @idUsuario
				AND P.CicloID = @CicloID;
		END;

		DELETE Usuario.UsuarioClassificacao
		FROM Usuario.UsuarioClassificacao
		INNER JOIN #UsrLinhas
			ON #UsrLinhas.ID = Usuario.UsuarioClassificacao.UsuarioID
		WHERE UsuarioClassificacao.CicloID = @CicloID;

		INSERT INTO Usuario.UsuarioClassificacao
		SELECT #UsrLinhas.ID
			, ISNULL(@Nivel,0)
			, GETDATE()
			, ISNULL(@Nivel,0)
			, @CicloID
		FROM #UsrLinhas;

END;

go
Grant Exec on spOC_US_Classificacao To public
go
