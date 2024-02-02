use [ClubeVantagens]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('AtualizarLancamentos1'))
   Drop Procedure AtualizarLancamentos1

go

Create Proc [dbo].[AtualizarLancamentos1]
AS
BEGIN

	-- Marca os registros ainda não processados como "em processamento"
	UPDATE Rede.Bonificacao SET StatusID = 1 WHERE StatusID = 0 AND Valor > 0

	-- Insere registros de indicação (em dinheiro)
	INSERT INTO Financeiro.Lancamento
	SELECT
		B.UsuarioID,
		1 AS ContaID, -- define a carteira do lancamento, como se fosse a conta corrente do sistema. 1 = dinheiro, 2= pontos. Assim podemos ter extrato de pontos e de valores.
		6 AS TipoID, -- tipo bonus 6=bonus, 10=transferencia...
		B.CategoriaID, -- categoria do bonus
		B.ReferenciaID,  -- di do usuario de origem do bonus
		B.Valor,     --Valor do bonus a ser creditado
		(C.Nome + ' (' + U.Login + ')') AS Descricao, -- inclui o login de origem do bonus para melhor identificação do bonus para quem recebe este bonus.
		GETDATE() AS DataCriacao, -- data que o bonus foi gravado.
		GETDATE() AS DataLancamento, -- data do lançamento do bonus.
		B.PedidoID
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
		INNER JOIN Usuario.Usuario AS U ON U.ID = B.ReferenciaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (1,12) ---B.CategoriaID IN (14, 16)

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
		GETDATE() AS DataCriacao,
		GETDATE() AS DataLancamento,
		B.PedidoID
	FROM
		Rede.Bonificacao AS B
		INNER JOIN Financeiro.Categoria AS C ON C.ID = B.CategoriaID
	WHERE
		B.StatusID = 1 AND B.CategoriaID IN (3)

	/*
	-- Insere registros de bônus binário com tipo de pontos (2)
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
	UPDATE Rede.Bonificacao SET StatusID = 2 WHERE StatusID = 1 AND Valor > 0

END
go
   Grant Exec on AtualizarLancamentos1 To public
go

