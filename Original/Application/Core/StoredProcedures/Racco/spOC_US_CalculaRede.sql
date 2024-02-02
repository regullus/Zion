USE [SRMRACCO]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_CalculaRede]    Script Date: 12/07/2017 10:22:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Proc [dbo].[spOC_US_CalculaRede]
   @idUsuario int
As
BEGIN
	--Necessario para o entity reconhecer retorno de select com tabela temporaria
	--Set FMTONLY OFF

	IF EXISTS (SELECT 1 FROM Usuario.Rede WHERE ID = @idUsuario AND DATEADD(HH, 24, DataAtualizacao) < GETDATE())
	BEGIN
		RETURN
	END

	Declare @QtdeTotalEsquerda Int
	Declare @QtdeTotalDireita Int
	Declare @QtdeCicloEsquerda Int
	Declare @QtdeCicloDireita Int
	Declare @QtdeAtivoEsquerda Int
	Declare @QtdeAtivoDireita Int
	Declare @PontosEsquerda Int
	Declare @PontosDireita Int

	Declare @DataInicioCiclo DateTime
	
	Declare @assinatura nvarchar(Max)
	Select @assinatura = LTRIM(RTRIM(Assinatura)) from Usuario.Usuario (nolock) where ID = @idUsuario

	IF (@assinatura != '')
	BEGIN
		-- Data Inicio do Ciclo Ativo
		SELECT @DataInicioCiclo = DataInicial FROM Rede.Ciclo (nolock) WHERE Ativo = 1 ORDER BY DataInicial DESC	
		IF (@DataInicioCiclo IS NULL)
			SET @DataInicioCiclo = GETDATE()	

		-- Select @DataInicioCiclo

		-- QTDE - Lado Esquerdo
		SELECT 
			@QtdeTotalEsquerda = COUNT(1),
		 	@QtdeCicloEsquerda = SUM(CASE WHEN (Usr.DataCriacao > @DataInicioCiclo) THEN 1 ELSE 0 END),
			@QtdeAtivoEsquerda = SUM(CASE WHEN (Usr.DataValidade >= GETDATE()) THEN 1 ELSE 0 END)
		FROM Usuario.Usuario Usr (NOLOCK) 
		WHERE LEFT(Usr.Assinatura, LEN(@assinatura)+1) =  (@assinatura + '0')

		-- QTDE - Lado Direito
		SELECT 
			@QtdeTotalDireita = COUNT(1),
			@QtdeCicloDireita = SUM(CASE WHEN (Usr.DataCriacao > @DataInicioCiclo) THEN 1 ELSE 0 END),
			@QtdeAtivoDireita = SUM(CASE WHEN (Usr.DataValidade >= GETDATE()) THEN 1 ELSE 0 END)
		FROM Usuario.Usuario Usr (NOLOCK) 
		WHERE LEFT(Usr.Assinatura, LEN(@assinatura)+1) =  (@assinatura + '1')

		-- PONTOS - Lado Esquerdo
		SELECT @PontosEsquerda = SUM(Lan.Valor)
		FROM Usuario.Usuario Usr (NOLOCK)
		INNER JOIN Financeiro.Lancamento Lan (NOLOCK) ON Lan.ReferenciaID = Usr.ID
		WHERE LEFT(Usr.Assinatura, LEN(@assinatura)+1) =  (@assinatura + '0')
		 AND Lan.UsuarioID = @idUsuario
		 AND Lan.CategoriaID = 23


		-- PONTOS - Lado Direito
		SELECT @PontosDireita = SUM(Lan.Valor)
		FROM Usuario.Usuario Usr (NOLOCK)
		INNER JOIN Financeiro.Lancamento Lan (NOLOCK) ON Lan.ReferenciaID = Usr.ID
		WHERE LEFT(Usr.Assinatura, LEN(@assinatura)+1) =  (@assinatura + '1')
		 AND Lan.UsuarioID = @idUsuario
		 AND Lan.CategoriaID = 23

		IF NOT EXISTS (SELECT 1 FROM Usuario.Rede (NOLOCK) WHERE UsuarioID = @idUsuario)
		BEGIN

			INSERT INTO Usuario.Rede
			SELECT
				UsuarioID = @idUsuario,
				ReferenciaID = @idUsuario,
				DataAtualizacao = GETDATE(),
				QtdeTotalEsquerda = ISNULL(@QtdeTotalEsquerda, 0),
				QtdeTotalDireita = ISNULL(@QtdeTotalDireita, 0),
				QtdeCicloEsquerda = ISNULL(@QtdeCicloEsquerda, 0),
				QtdeCicloDireita = ISNULL(@QtdeCicloDireita, 0),
				QtdeAtivoEsquerda = ISNULL(@QtdeAtivoEsquerda, 0),
				QtdeAtivoDireita = ISNULL(@QtdeAtivoDireita, 0),
				PontosEsquerda = ISNULL(@PontosEsquerda, 0),
				PontosDireita = ISNULL(@PontosDireita, 0)
			
		END
		ELSE
		BEGIN

			UPDATE Usuario.Rede
			SET
				DataAtualizacao = GETDATE(),
				QtdeTotalEsquerda = ISNULL(@QtdeTotalEsquerda, 0),
				QtdeTotalDireita = ISNULL(@QtdeTotalDireita, 0),
				QtdeCicloEsquerda = ISNULL(@QtdeCicloEsquerda, 0),
				QtdeCicloDireita = ISNULL(@QtdeCicloDireita, 0),
				QtdeAtivoEsquerda = ISNULL(@QtdeAtivoEsquerda, 0),
				QtdeAtivoDireita = ISNULL(@QtdeAtivoDireita, 0),
				PontosEsquerda = ISNULL(@PontosEsquerda, 0),
				PontosDireita = ISNULL(@PontosDireita, 0)
			WHERE UsuarioID = @idUsuario

		END
	END
    
END -- Sp

-- spOC_US_CalculaRede 1000
-- select * from Usuario.Rede where UsuarioID = 1000

--SELECT 
--	COUNT(1)
--FROM Usuario.Usuario Usr (NOLOCK) 
--WHERE LEFT(Usr.Assinatura + '0', LEN('#0')+1) =  ('#0' + '0')