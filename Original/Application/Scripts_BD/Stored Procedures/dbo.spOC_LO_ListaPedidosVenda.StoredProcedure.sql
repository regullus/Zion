USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_LO_ListaPedidosVenda]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spOC_LO_ListaPedidosVenda]
   @Tipo          int                 ,
   @DataIni       datetime            ,
   @DataFim       datetime            ,  
   @Login         nvarchar(255) = null,
   @StatusId      int           = null,
   @Categoria     int           = null,
   @Produto       nvarchar(255) = null

As
-- =============================================================================================
-- Author.....: 
-- Create date: 28/02/2019
-- Description: Lista de Pedidos de Venda
-- Parametro..: Tipo = 1 - Analitico 
--                     2 - Sintetico
-- =============================================================================================
BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    Create Table #TPedidoItens
    (
	     tipoRg             int          ,
	     dataPedido         datetime     ,
		 login              nvarchar(255),	
		 pedido             nvarchar(255),
		 produto            nvarchar(255),
		 pedidoPagamentoID  int          ,
		 meioPagamentoID    int          ,
		 meioPagamento      nvarchar(255),
		 produtoCategoriaID int          ,
		 categoria          nvarchar(255),
		 dataStatus         datetime     ,
		 pgtoStatusID       int,
		 quantidade         int,
		 valor              float,	
		 total              float      
    );

	If (@Categoria is not null and @Categoria = 0 )
	Begin
	   Set @Categoria =null;
	End

    -- Obtem os Itens do Pedidos dos Usuarios	 
	Insert into #TPedidoItens     
    Select 
	    1                  as tipoRg,
        P.DataCriacao      as dataPedido,
		U.Login            as login,	
		P.Codigo           as pedido,
		PR.Nome            as produto ,
		PP.ID              as pedidoPagamentoID,
		PP.MeioPagamentoID as meioPagamentoID,
		''                 as meioPagamento,
		PR.ProdutoCategoriaID as produtoCategoriaID,
	    ''                 as categoria,
		null               as dataStatus,
	    0                  as pgtoStatusID,
	    PI.Quantidade      as quantidade,
		PI.ValorUnitario   as valor,
		PI.ValorUnitario * PI.Quantidade as total
    From 
	    Loja.pedido                P  (nolock), 
	    Loja.PedidoItem            PI (nolock),
        Loja.Produto               PR (nolock),	  
	    Usuario.Usuario            U  (nolock),	  
	    Loja.PedidoPagamento       PP (nolock)     
    Where P.ID = PI.PedidoID
      and PI.ProdutoID = PR.ID
      and PR.TipoID in (3,4)  -- Produto Físico / Produto Virtual	  
	  and PR.ProdutoCategoriaID = Coalesce(@Categoria , PR.ProdutoCategoriaID)    
	  and P.DataCriacao between @DataIni and @DataFim		  
	  and P.ID = PP.PedidoID		   
	  and U.ID = P.UsuarioID;	

    -- Elimina os usuarios
	If (@Login is not null and @Login != '' )
	Begin
	    Delete
		    #TPedidoItens
		Where
		    login not like '%' + @Login + '%'; 
	End

	-- Elimina os produtos
	If (@Produto is not null and @Produto != '' )
	Begin
	    Delete
		    #TPedidoItens
		Where
		    produto not like '%' + @Produto + '%'; 
	End

	-- Obtem o ultimo status de pagamento
	Update
	    #TPedidoItens     
    Set
		dataStatus   = PPS.Data,
		pgtoStatusID = PPS.StatusID  
    From 
	     #TPedidoItens              T
		 CROSS APPLY (Select Top 1  Data , StatusID 
                      From Loja.PedidoPagamentoStatus (nolock) 
                      Where PedidoPagamentoID = T.pedidoPagamentoID	                   
                      Order By Data desc) as PPS;

    -- Elimina os status	
    If (@StatusId is not null and @StatusId != 0 )
	Begin
	    Delete 
	        #TPedidoItens
	    Where 
	        pgtoStatusID != @StatusId;
	End
	Else
	Begin
	    Delete 
	        #TPedidoItens
	    Where 
	        pgtoStatusID = 0;
	End


	Declare @zero float = 0;

	-- Totaliza conforme do tipo selecionado
	If(@Tipo = 1)
	Begin
	    -- Atualiza o Meio de pagamento
		Update 
		    #TPedidoItens
		Set 
		    meioPagamento = M.Descricao
		from
		    #TPedidoItens T,
			Financeiro.MeioPagamento M (nolock)
		Where 
		    M.ID = T.meioPagamentoID;

	    -- Obtem o total por Usuarios
        Insert into #TPedidoItens
        Select 
            2                        as tipoRg,
            null                     as dataPedido,
		    login                    as login,	
		    ''                       as pedido,
		    ''                       as produto ,
		    0                        as pedidoPagamentoID,
			0                        as meioPagamentoID,
		    'SubTotal ' + Max(login) as meioPagamento,
			0                        as produtoCategoriaID,
			''                       as categoria,
	        null                     as dataStatus,
	        0                        as pgtoStatusID,
	 	    Sum(quantidade)          as quantidade,
		    Sum(valor)               as valor,		
		    Sum(total)               as total		  	   		      
        From 
            #TPedidoItens
        Group By 
            login;

        -- Obtem o total geral dos usuarios
        Insert into #TPedidoItens
        Select 		   
			9                        as tipoRg,
            null                     as dataPedido,
		    'ZZZZZZZZZZZZZZZZZZZ'    as login,	
		    ''                       as pedido,
		    ''                       as produto ,
		    0                        as pedidoPagamentoID,
			0                        as meioPagamentoID,
		    'Total Geral'            as meioPagamento,
			0                        as produtoCategoriaID,
			''                       as categoria,
	        null                     as dataStatus,
	        0                        as pgtoStatusID,
	 	    Sum(quantidade)          as quantidade,
		    Sum(valor)               as valor,		
		    Sum(total)               as total
        From 
            #TPedidoItens
	    Where 
            tipoRg = 1
        Group By 
            tipoRg;

        -- Retorna dados
        Select 
	       tipoRg        as tipoRg,
           dataPedido    as dataPedido,
		   login         as login,	
		   pedido        as pedido,
		   categoria     as categoria,
		   produto       as produto,		
	 	   meioPagamento as meioPagamento,		
		   dataStatus    as dataStatus,
	       pgtoStatusID  as pgtoStatusID,
	       quantidade    as quantidade,
		   valor         as valor,
		   @zero         as juros,
		   @zero         as frete,
		   total         as total
        From 
            #TPedidoItens
        Order By
	        login,
            tipoRg, 
            dataPedido;
	End
	Else
	Begin
	    -- Atualiza o Meio de pagamento
		Update 
		    #TPedidoItens
		Set 
		    categoria = PC.Nome
		From
		    #TPedidoItens T,
			Loja.ProdutoCategoria PC (nolock)
		Where 
		    PC.ID = T.produtoCategoriaID;

        -- Obtem o total por Categoria e Status de pagamento
        Insert into #TPedidoItens
        Select 
            2                        as tipoRg,
            null                     as dataPedido,
		    ''                       as login,	
		    ''                       as pedido,
		    ''                       as produto ,
		    0                        as pedidoPagamentoID,
			0                        as meioPagamentoID,
		    'SubTotal ' + categoria  as meioPagamento,
			0                        as produtoCategoriaID,
			categoria                as categoria,
	        null                     as dataStatus,
	        pgtoStatusID             as pgtoStatusID,
	 	    Sum(quantidade)          as quantidade,
		    Sum(valor)               as valor,		
		    Sum(total)               as total		  	   		      
        From 
            #TPedidoItens
        Group By 
            categoria,
			pgtoStatusID;

	    -- Obtem o total por Categoria 
        Insert into #TPedidoItens
        Select 
            3                        as tipoRg,
            null                     as dataPedido,
		    ''                       as login,	
		    ''                       as pedido,
		    ''                       as produto ,
		    0                        as pedidoPagamentoID,
			0                        as meioPagamentoID,
		    'SubTotal ' + categoria  as meioPagamento,
			0                        as produtoCategoriaID,
			categoria                as categoria,
	        null                     as dataStatus,
	        0                        as pgtoStatusID,
	 	    Sum(quantidade)          as quantidade,
		    Sum(valor)               as valor,		
		    Sum(total)               as total		  	   		      
        From 
            #TPedidoItens
			Where 
            tipoRg = 1
        Group By 
            categoria;

	    -- Obtem o total geral
        Insert into #TPedidoItens
        Select 		   
			9                        as tipoRg,
            null                     as dataPedido,
		    ''                       as login,	
		    ''                       as pedido,
		    ''                       as produto ,
		    0                        as pedidoPagamentoID,
			0                        as meioPagamentoID,
		    'Total Geral'            as meioPagamento,
			0                        as produtoCategoriaID,
			'ZZZZZZZZZZZZZZZZZZZ'    as categoria,
	        null                     as dataStatus,
	        0                        as pgtoStatusID,
	 	    Sum(quantidade)          as quantidade,
		    Sum(valor)               as valor,		
		    Sum(total)               as total
        From 
            #TPedidoItens
	    Where 
            tipoRg = 1
        Group By 
            tipoRg;

		-- Retorna dados
        Select 
	        tipoRg        as tipoRg,
            dataPedido    as dataPedido,
		    login         as login,	
		    pedido        as pedido,
			categoria     as categoria,
		    produto       as produto,		
	 	    meioPagamento as meioPagamento,		
		    dataStatus    as dataStatus,
	        pgtoStatusID  as pgtoStatusID,
	        quantidade    as quantidade,
		    valor         as valor,
			@zero         as juros,
		    @zero         as frete,
		    total         as total
        From 
            #TPedidoItens
		Where 
		    tipoRg > 1
        Order By
	        categoria,
            tipoRg;            
	End

    -- Remove as Tabelas Temporarias
    Drop Table #TPedidoItens
END;


GO
