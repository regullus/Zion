Use MinersBits
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_FI_GeraLancamentos'))
   Drop Procedure spDG_FI_GeraLancamentos
go

-- =============================================================================================
-- Author.....: Mauricio Bortoloto
-- Create date: 06/05/2016
-- Description: Gera registros de lancamento financeiro de bonificacoes ainda nao processadas
-- =============================================================================================

Create Proc [dbo].spDG_FI_GeraLancamentos

AS
BEGIN

	-- Marca os registros ainda não processados como "em processamento"
	UPDATE Rede.Bonificacao 
   SET StatusID = 1 
   WHERE StatusID = 0 
     AND Valor > 0

	-- Insere registros de indicação (em dinheiro)
	INSERT INTO Financeiro.Lancamento
	SELECT
		B.UsuarioID,
		1 AS ContaID,
		6 AS TipoID,
		B.CategoriaID,
		B.ReferenciaID,
		B.Valor,
		(C.Nome + ' (' + U.Login + ')') AS Descricao,
		dbo.GetDateZion() AS DataCriacao,
		B.Data    AS DataLancamento
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
		INNER JOIN Usuario.Usuario AS U ON U.ID = B.ReferenciaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (1) ---B.CategoriaID IN (14, 16)

	/*
	-- Insere registros de indicação direta, indireta e residuais (em pontos)
	INSERT INTO Financeiro.Lancamento
	SELECT
		B.UsuarioID,
		2 AS ContaID,
		6 AS TipoID,
		B.CategoriaID,
		B.ReferenciaID,
		B.Valor,
		(C.Nome + ' (' + U.Login + ')') AS Descricao,
		GETDATE() AS DataCriacao,
		GETDATE() AS DataLancamento
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
		INNER JOIN Usuario.Usuario AS U ON U.ID = B.ReferenciaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (1, 14, 16, 17, 18, 19, 20, 27, 28)
	*/

	-- Insere registros de bônus binário (em dinheiro)
	INSERT INTO Financeiro.Lancamento
	SELECT
		B.UsuarioID,
		1 AS ContaID,
		6 AS TipoID,
		B.CategoriaID,
		B.ReferenciaID,
		B.Valor,
		C.Nome AS Descricao,
		dbo.GetDateZion() AS DataCriacao,
		B.Data    AS DataLancamento
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (3,8,21,22)

	/*
	-- Insere registros de bônus binário e DLD9 (em pontos)
	INSERT INTO Financeiro.Lancamento
	SELECT
		B.UsuarioID,
		2 AS ContaID,
		6 AS TipoID,
		B.CategoriaID,
		B.ReferenciaID,
		B.Valor,
		C.Nome AS Descricao,
		GETDATE() AS DataCriacao,
		GETDATE() AS DataLancamento
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (3, 21)
	*/

	-- Marca os registros "em processamento" como "processados"
	UPDATE Rede.Bonificacao 
   SET StatusID = 2 
   WHERE StatusID = 1 
     AND Valor > 0

END

go
   Grant Exec on spDG_FI_GeraLancamentos To public
go

