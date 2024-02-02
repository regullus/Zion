use ClubeVantagens
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('BonusIndicacao'))
   Drop Procedure BonusIndicacao
Go

Create PROCEDURE [dbo].[BonusIndicacao]
AS
BEGIN

	-- Tabela temporária com usuários cadastrados com seus respectivos níveis e data de associação
	CREATE TABLE #TUsuarios
	(
		UsuarioID INT,
		PedidoId INT,
		ProdutoID INT,
		PatrocinadorID INT,
		DtAtivacao DATETIME,
		VlrBonusVenda decimal(10,2),
		DtValidade DATETIME
	)

	-- Seleção dos pedidos pagos com produtos que paguem bonus de venda
	INSERT INTO #TUsuarios 
		Select u.id as usuario,P.ID pedido,PV.ProdutoID as produto,u.PatrocinadorDiretoID,PS.Data as DtAtivacao,PV.ValorBonusVenda,U2.DataValidade
		from 
		loja.pedido P, 
		loja.PedidoItem PI,
		loja.ProdutoValor PV ,
		usuario.Usuario U,
		Usuario.Usuario U2,
		loja.PedidoPagamento PP, 
		loja.PedidoPagamentoStatus PS  
		Where
		P.ID = PI.PedidoID
		and PI.ProdutoID = PV.ProdutoID
		and P.UsuarioID = U.ID
		and U.PatrocinadorDiretoID = U2.ID
		and P.ID = PP.PedidoID
		and PP.ID = PS.PedidoPagamentoID
		and PS.StatusID = 3 -- somente pedidos pagos
		and PV.ValorBonusVenda > 0 -- somente produtos com valor para vonus de venda (indicação)
		and U.GeraBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao
		and U.Bloqueado = 0 -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
		and U2.RecebeBonus = 1 -- Patrocinador recebe bonus
		and U2.Bloqueado = 0 -- Patrocinador nao esta bloqueado
		and U2.DataValidade >= GETDATE() --so recebe se estiver com ativo mensal pago
		and PS.Data >= '2016-10-13 17:23:46.597' -- data de inicio de processamento da rotina para nao intereferir nos bonus pagos anteriormente
		and PS.Data >= CONVERT(VARCHAR(8),GETDATE(),112) --pega somente pedidos do dia
		and NOT EXISTS (SELECT 1 FROM rede.Bonificacao RB WHERE RB.PedidoID = P.ID and RB.CategoriaID = 1) -- evita gerar duplicidade de pedidos na rede.bonificação.

	-- Insere na tabela de bonificações todos os bônus que ainda não foram pagos ou a diferença do que resta ser pago
	INSERT INTO Rede.Bonificacao
	SELECT 
		1 as CategoriaID, --1 = bonus builder
		T.PatrocinadorID as Usuario,
		T.UsuarioID as Referencia,
		0 AS StatusID,
		T.DtAtivacao AS Data,
		T.VlrBonusVenda AS Valor,
		T.PedidoId as PedidoID
	FROM 
		#TUsuarios AS T
	Where
		NOT EXISTS (SELECT 1 FROM Rede.Bonificacao R WHERE T.PedidoId = R.PedidoID)

    Select * from #TUsuarios

	-- Remove todas as tabelas temporárias
	DROP TABLE #TUsuarios
	
END

go
   Grant Exec on BonusIndicacao To public
go




