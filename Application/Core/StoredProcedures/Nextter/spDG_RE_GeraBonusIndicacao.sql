Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusIndicacao'))
   Drop Procedure spDG_RE_GeraBonusIndicacao
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
   @baseDate varchar(8) null

AS
-- =============================================================================================
-- Author.....:
-- Create date: 24/01/2018
-- Description: Gera registros de Bonificacao de indicação
-- =============================================================================================
BEGIN
   BEGIN TRY
      set nocount on

      DECLARE @dataInicio datetime;
      DECLARE @dataFim    datetime;
      
      if (@baseDate is null)
      Begin 
         SET @dataInicio = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);
      End
      Else
      Begin
         SET @dataInicio = CAST(@baseDate + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(@baseDate + ' 23:59:59' as datetime2);
      End

      -- Tabela temporária com usuários cadastrados com seus respectivos níveis e data de associação
      Create Table #TUsuarios
	   (
	      UsuarioID INT,
	      PedidoId INT,
	      ProdutoID INT,
		  DtAtivacao DATETIME,
	      Patrocinador1ID INT,
		  Patrocinador2ID INT,
		  Patrocinador3ID INT,	     
          Valor float,          
	      DtValidade DATETIME
	   );

      -- Seleção dos pedidos pagos com produtos que paguem bonus de Indicacao
      Insert Into #TUsuarios 
	   Select 
         U.id as UsuarioID,
         P.ID as PedidoId,
         PV.ProdutoID as ProdutoID,
		 PS.Data as DtAtivacao,
         U.PatrocinadorDiretoID as Patrocinador1ID,
		 0 as Patrocinador2ID,
	     0 as Patrocinador3ID,       
         PV.Valor as Valor,
         U2.DataValidade as DtValidade
      From 
	     Loja.pedido P (nolock), 
	     Loja.PedidoItem PI (nolock),
         Loja.Produto PR (nolock),
	     Loja.ProdutoValor PV (nolock),
	     Usuario.Usuario U (nolock),
	     Usuario.Usuario U2 (nolock),
	     Loja.PedidoPagamento PP (nolock), 
	     Loja.PedidoPagamentoStatus PS (nolock) 		
      Where P.ID = PI.PedidoID
        and PI.ProdutoID = PR.ID
        and PR.TipoID in (1)  -- Associacao 
	    and PI.ProdutoID = PV.ProdutoID
	    and P.UsuarioID = U.ID
	    and U.PatrocinadorDiretoID = U2.ID
	    and P.ID = PP.PedidoID
	    and PP.ID = PS.PedidoPagamentoID
	    and PS.StatusID = 3      -- somente pedidos pagos        
	    and PV.VlrBonusVenda > 0 -- somente produtos com valor para bonus de venda (indicação)
	    and U.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
	    and U.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
	    and U2.RecebeBonus = 1   -- Patrocinador recebe bonus
	    and U2.Bloqueado = 0     -- Patrocinador nao esta bloqueado
	    and U2.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	   	 
        and PS.Data BETWEEN @dataInicio and @dataFim --pega somente pedidos do dia
	    and NOT EXISTS (Select 1 From Rede.Bonificacao B (nolock) Where B.PedidoID = P.ID and B.CategoriaID = 13); -- evita gerar duplicidade de pedidos na rede.bonificação.

      -- Atualisa o patrodinados de Segunda Geracao 
	  Update #TUsuarios
	  Set Patrocinador2ID = U.ID
	  From 
	     #TUsuarios T,
	     Usuario.Usuario U (nolock)	  		
      Where T.Patrocinador1ID = U.ID       
	    and U.RecebeBonus = 1   -- Patrocinador recebe bonus
	    and U.Bloqueado = 0     -- Patrocinador nao esta bloqueado
	    and U.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	
		
      -- Atualisa o patrodinados de Terceira Geracao 
	  Update #TUsuarios
	  Set Patrocinador3ID = U.ID
	  From 
	     #TUsuarios T,
	     Usuario.Usuario U (nolock)	  		
      Where T.Patrocinador2ID = U.ID       
	    and U.RecebeBonus = 1   -- Patrocinador recebe bonus
	    and U.Bloqueado = 0     -- Patrocinador nao esta bloqueado
	    and U.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	

      -- Insere na tabela de bonificações todos os bônus que ainda não foram pagos ou a diferença do que resta ser pago
      BEGIN TRANSACTION

	  -- Bonificacao de Primeira Geracao
      Insert Into Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   Select 
		   13 as CategoriaID, -- 1 - Bônus de Indicação
		   T.Patrocinador1ID as Usuario,
		   T.UsuarioID as Referencia,
		   0 as StatusID,
		   T.DtAtivacao as Data,
		   dbo.TruncZion((T.Valor * 0.50), 2) as Valor,
		   T.PedidoId as PedidoID
	   From
		   #TUsuarios AS T

      -- Bonificacao de Segunda Geracao
      Insert Into Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   Select 
		   13 as CategoriaID, -- Bonus Indicacao
		   T.Patrocinador2ID as Usuario,
		   T.UsuarioID as Referencia,
		   0 as StatusID,
		   T.DtAtivacao as Data,
		   dbo.TruncZion((T.Valor * 0.30), 2) as Valor,
		   T.PedidoId as PedidoID
	   From
		   #TUsuarios AS T
	   Where
	       T.Patrocinador2ID > 0

       -- Bonificacao de Terceira Geracao
       Insert Into Rede.Bonificacao
         (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   Select 
		   13 as CategoriaID, -- Bonus Indicacao
		   T.Patrocinador3ID as Usuario,
		   T.UsuarioID as Referencia,
		   0 as StatusID,
		   T.DtAtivacao as Data,
		   dbo.TruncZion((T.Valor * 0.50), 2) as Valor,
		   T.PedidoId as PedidoID
	   From
		   #TUsuarios AS T
	   Where
	       T.Patrocinador3ID > 0

      COMMIT TRANSACTION

      -- Remove todas as tabelas temporárias
      Drop Table #TUsuarios;

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on spDG_RE_GeraBonusIndicacao To public
go

-- Exec spDG_RE_GeraBonusIndicacao null
-- Exec spDG_RE_GeraBonusIndicacao '20170621'