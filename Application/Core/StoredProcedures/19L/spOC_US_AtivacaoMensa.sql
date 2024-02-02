use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_AtivacaoMensa'))
   Drop Procedure spOC_US_AtivacaoMensa
go

Create Proc [dbo].spOC_US_AtivacaoMensa
    @CicloID INT = 0

As
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2019
-- Description: 
--
-- =============================================================================================

BEGIN

    DECLARE @DataFimCiclo DATETIME;

    IF (@CicloID = 0)
    BEGIN
        -- Buscar Ciclo Atual Ativo
        SELECT @CicloID = MAX(ID)
        FROM Rede.Ciclo (NOLOCK)
        WHERE Ativo = 1;
    END;

    SELECT @DataFimCiclo = DataFinal
    FROM Rede.Ciclo (NOLOCK)
    WHERE ID = @CicloID;

    INSERT INTO Usuario.AtivacaoMensal
    SELECT Ped.UsuarioID,
           CicloID = Cic.ID,
           MIN(Ped.DataCriacao),
           1
    FROM Loja.Pedido Ped (NOLOCK)
        INNER JOIN Loja.PedidoItem Item (NOLOCK)
            ON Item.PedidoID = Ped.ID
        INNER JOIN Loja.Produto PRd (NOLOCK)
            ON PRd.ID = Item.ProdutoID
		INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
			ON LPag.PedidoID = Ped.ID
		INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
			ON LPagSta.PedidoPagamentoID = LPag.ID
        INNER JOIN Rede.Ciclo Cic (NOLOCK)
            ON LPagSta.Data BETWEEN Cic.DataInicial AND Cic.DataFinal
        LEFT JOIN Usuario.AtivacaoMensal Atv (NOLOCK)
            ON Atv.UsuarioID = Ped.UsuarioID
               AND Atv.CicloID = Cic.ID
    WHERE PRd.TipoID IN ( 1, 2 )
		AND LPagSta.StatusID = 3
        AND Cic.ID = @CicloID
        AND Atv.UsuarioID IS NULL
    GROUP BY Ped.UsuarioID,
             Cic.ID,
             Cic.Ativo;

    INSERT INTO Usuario.AtivacaoMensal
    SELECT Ped.UsuarioID,
           Cic.ID,
           MIN(LPagSta.Data),
           1 -- , SUM(Item.BonificacaoUnitaria)
    FROM Loja.Pedido Ped (NOLOCK)
        INNER JOIN Loja.PedidoItem Item (NOLOCK)
            ON Item.PedidoID = Ped.ID
        INNER JOIN Loja.Produto PRd (NOLOCK)
            ON PRd.ID = Item.ProdutoID
		INNER JOIN Loja.PedidoPagamento LPag (NOLOCK)
			ON LPag.PedidoID = Ped.ID
		INNER JOIN Loja.PedidoPagamentoStatus LPagSta (NOLOCK)
			ON LPagSta.PedidoPagamentoID = LPag.ID
        INNER JOIN Rede.Ciclo Cic (NOLOCK)
            ON LPagSta.Data BETWEEN Cic.DataInicial AND Cic.DataFinal
        INNER JOIN Usuario.Usuario Usr (NOLOCK)
            ON Usr.ID = Ped.UsuarioID
        LEFT JOIN Usuario.AtivacaoMensal Atv (NOLOCK)
            ON Atv.UsuarioID = Ped.UsuarioID
               AND Atv.CicloID = Cic.ID
    WHERE PRd.TipoID IN  (3, 4)
		AND LPagSta.StatusID = 3
        AND Cic.ID = @CicloID
        AND Atv.UsuarioID IS NULL
    GROUP BY Ped.UsuarioID,
             Cic.ID,
             Cic.Ativo
    HAVING SUM(Item.BonificacaoUnitaria) >= 150;

	-- Atualizar somenta data de ativação por conta de bloqueio de usuários
    UPDATE Usuario.Usuario
    SET DataAtivacao = AtivacaoMensal.DataAtivacao,
        DataValidade = @DataFimCiclo
    FROM Usuario.Usuario
        INNER JOIN Usuario.AtivacaoMensal
            ON AtivacaoMensal.UsuarioID = Usuario.ID
			AND AtivacaoMensal.CicloID = @CicloID;

    --UPDATE Usuario.Usuario
    --SET DataAtivacao = AtivacaoMensal.DataAtivacao,
    --    DataValidade = @DataFimCiclo
    --FROM Usuario.Usuario
    --    INNER JOIN Usuario.AtivacaoMensal
    --        ON AtivacaoMensal.UsuarioID = Usuario.ID;

END;

go
Grant Exec on spOC_US_AtivacaoMensa To public
go
