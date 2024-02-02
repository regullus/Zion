
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_LO_ListaPedidosVenda'))
   Drop Procedure spOC_LO_ListaPedidosVenda
go

CREATE Proc dbo.spOC_LO_ListaPedidosVenda
   @Tipo          int                 ,
   @DataIni       datetime            ,
   @DataFim       datetime            ,  
   @Login         nvarchar(255) = null,
   @StatusId      int           = null,
   @Categoria     int           = null,
   @Produto       nvarchar(255) = null,
   @ProdutoTipo   int           = null

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

	If (@ProdutoTipo is not null and @ProdutoTipo = 0 )
	Begin
	   Set @ProdutoTipo =null;
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
		PP.Valor           as valor,  
		PP.Valor * PI.Quantidade as total 
    From 
	    Loja.pedido                P  (nolock), 
	    Loja.PedidoItem            PI (nolock),
        Loja.Produto               PR (nolock),	  
	    Usuario.Usuario            U  (nolock),	  
	    Loja.PedidoPagamento       PP (nolock)     
    Where P.ID = PI.PedidoID
      and PI.ProdutoID = PR.ID      
	  and PR.ProdutoCategoriaID = Coalesce(@Categoria , PR.ProdutoCategoriaID)    
	  and PR.TipoID = Coalesce(@ProdutoTipo , PR.TipoID)    
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
		   ISNULL(valor, 0) as valor,
		   @zero         as juros,
		   @zero         as frete,
		   ISNULL(total, 0) as total
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
		    ISNULL(valor, 0) as valor,
			@zero         as juros,
		    @zero         as frete,
		    ISNULL(total, 0) as total
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

go
Grant Exec on spOC_LO_ListaPedidosVenda To public
go

--Exec spOC_LO_ListaPedidosVenda @Tipo   = 1,
--                               @DataIni= '2018-11-01 01:00:00.000', 
--                               @DataFim= '2019-02-28 01:00:00.000',
--							     @Identificacao='Batman' 
							 
Exec spOC_LO_ListaPedidosVenda @Tipo   = 1,
                               @DataIni= '2019-03-01 01:00:00.000', 
					           @DataFim= '2019-03-01 23:00:00.000',
							   @StatusId = 3

--Exec spOC_LO_ListaPedidosVenda @Tipo   = 1,
--                               @DataIni= '2018-11-01 01:00:00.000', 
--					  	       @DataFim= '2019-02-28 01:00:00.000',						
--							   @StatusId = 1,
--							   @Produto = 'Sunglasses'
							 
--Exec spOC_LO_ListaPedidosVenda @Tipo   = 2,
--                               @DataIni= '2018-11-01 01:00:00.000', 
--					             @DataFim= '2019-02-28 01:00:00.000',
--						         @Identificacao='batman', 
--					           	 @StatusId = 3

