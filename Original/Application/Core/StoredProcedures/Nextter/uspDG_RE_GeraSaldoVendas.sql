Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('uspDG_RE_uspDG_RE_GeraSaldoVendas'))
   Drop Procedure uspDG_RE_uspDG_RE_GeraSaldoVendas;
go

Create PROCEDURE [dbo].[uspDG_RE_uspDG_RE_GeraSaldoVendas]
   @baseDate  varchar(8) = null
  
AS
-- =============================================================================================
-- Author.....:  Adamastor
-- Create date:  16/03/2018
-- Description:  Gera registros de Saldo e Lancamento das Vendas para Bonificacao de Pool de Lideres
-- =============================================================================================
BEGIN
   BEGIN TRY
      Set NoCount On

      Declare 	    
	     @dataInicio datetime,
         @dataFim    datetime;
      
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
      Create Table #TLançamento
	  (	            
		 ReferenciaID Int,
	     Valor Float,
	     Descricao nvarchar(500),
	     DataLancamento DateTime,
	     PedidoID Int,
		 ProdutoID Int,
		 TipoID Int
	  );

      -- Seleção dos pedidos pagos
      Insert Into #TLançamento
	  Select 
	     P.UsuarioID as ReferenciaID,
	     PV.Valor as Valor,
	     (PT.Nome + ' (' + U.Login + ')') as Descricao,	  
	     PS.Data as DataLancamento,
	     P.ID as PedidoID,	
		 PR.ID as ProdutoID,
		 PR.TipoID as TipoID
      From 
	     Loja.pedido P (nolock), 
	     Loja.PedidoItem PI (nolock), 
         Loja.Produto PR (nolock), 
		 Loja.ProdutoTipo PT (nolock),               
	     Loja.ProdutoValor PV (nolock),
	     Usuario.Usuario U (nolock),	 
	     Loja.PedidoPagamento PP (nolock), 
	     Loja.PedidoPagamentoStatus PS (nolock) 		
      Where P.ID = PI.PedidoID     
        and PI.ProdutoID = PR.ID
		and PR.TipoID = PT.ID
        and PR.TipoID IN (1,5)  -- Ativo Mensal  
	    and PI.ProdutoID = PV.ProdutoID
	    and P.UsuarioID = U.ID	
	    and P.ID = PP.PedidoID
	    and PP.ID = PS.PedidoPagamentoID
	    and PS.StatusID = 3 -- somente pedidos pagos        
	    and U.GeraBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao
	    and U.Bloqueado = 0 -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao	   
	    and U.DataValidade >= @dataInicio -- so recebe se estiver com ativo mensal pago			 	
        and PS.Data BETWEEN @dataInicio AND @dataFim -- pega somente pedidos do dia
		and Not Exists (Select 1 
		                From Rede.SaldoVendaLancamento L (nolock)
						Where L.PedidoID = P.ID
						  and L.CategoriaID = 15);
				
	  -- Calcula o valor do lançamento
	  Update 
         #TLançamento
	  Set 
	     Valor = (CASE
		             WHEN TipoID = 1 THEN  dbo.TruncZion(Valor * 0.15, 2)
			         WHEN TipoID = 5 THEN  dbo.TruncZion(Valor * 0.20, 2)
			         ELSE 0
			      END);

      -- Apaga lancamentos com o valo zero
	  Delete
		 #TLançamento
      Where Valor = 0;

	  -- Obtem o Saldo anterior
	  Declare	     		
		 @SaldoTotal    Float = IsNull( (Select Top(1) S.Valor 
	                                     From Rede.SaldoVenda S (nolock)
	                                     Where CategoriaID = 15 -- 3 - Bônus de Liderança
	                                     Order by Data desc) , 0);	 

	  -- Calcula Saldo dos Lancamentos do Dia
	  Declare	     		
		 @SaldoTotalDia Float = IsNull( (Select Sum(Valor)
	                                     From #TLançamento), 0);
	 
      -- Gera Saldo e Lancamentos
      BEGIN TRANSACTION

	   -- Inclui Lancamentos
      Insert Into Rede.SaldoVendaLancamento
        (CategoriaID,
		 ReferenciaID,
	     Valor,
	     Descricao,
	     DataCriacao,
	     DataLancamento,
	     PedidoID)
	  Select 
	     15,
	     T.ReferenciaID,
		 T.Valor,
		 T.Descricao,
		 dbo.GetDateZion() as DataCriacao,
		 T.DataLancamento,
         T.PedidoId as PedidoID
	  From
		 #TLançamento T;

      -- Inclui Saldo
	  If(@SaldoTotalDia > 0)	
	  Begin
	     Insert Into Rede.SaldoVenda
           (CategoriaID,
		    Data,
	        Valor)
	     Values
	       (15,	
		    dbo.GetDateZion(),
		    dbo.TruncZion(@SaldoTotal + @SaldoTotalDia, 2));
      End

      COMMIT TRANSACTION

      -- Select * from #TLançamento

      -- Remove todas as tabelas temporárias
      Drop Table #TLançamento;
   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de uspDG_RE_uspDG_RE_GeraSaldoVendas: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on uspDG_RE_uspDG_RE_GeraSaldoVendas To public;
go

-- Exec uspDG_RE_uspDG_RE_GeraSaldoVendas null
-- Exec uspDG_RE_uspDG_RE_GeraSaldoVendas '20180316'

-- Select * from  Rede.SaldoVendaLancamento
-- Select * from  Rede.SaldoVenda