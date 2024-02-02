/*    ==Parâmetros de Script==

    Versão do Servidor de Origem : SQL Server 2016 (13.0.5426)
    Edição do Mecanismo de Banco de Dados de Origem : Microsoft SQL Server Standard Edition
    Tipo do Mecanismo de Banco de Dados de Origem : SQL Server Autônomo

    Versão do Servidor de Destino : SQL Server 2017
    Edição de Mecanismo de Banco de Dados de Destino : Microsoft SQL Server Standard Edition
    Tipo de Mecanismo de Banco de Dados de Destino : SQL Server Autônomo
*/

USE [xsinerdb_dev]
GO
/****** Object:  StoredProcedure [dbo].[spOC_LO_ListaPedidosVenda]    Script Date: 10/03/2020 16:21:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Proc [dbo].[spOC_LO_ListaPedidosVenda]
   @Tipo          int                 ,
   @DataIni       datetime            ,
   @DataFim       datetime            ,  
   @Login         nvarchar(255) = null,
   @StatusId      int           = null,
   @Categoria     int           = null,
   @Produto       nvarchar(255) = null,
   @ProdutoTipo   int           = null,
   @MeioPagamento int			= null

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
		 produtoID			int          ,
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

	If (@MeioPagamento is not null and @MeioPagamento = 0 )
	Begin
	   Set @MeioPagamento =null;
	End

	If (@Login is not null and @Login = '0' )
	Begin
	   Set @Login =null;
	End

	If (@Produto is not null and @Produto = '0' )
	Begin
	   Set @Produto =null;
	End

	If (@StatusId is not null and @StatusId = '0' )
	Begin
	   Set @StatusId =null;
	End


    -- Obtem os Itens do Pedidos dos Usuarios	 
	Insert into #TPedidoItens     
    Select 
	    1                  as tipoRg,
        P.DataCriacao      as dataPedido,
		U.Login            as login,	
		P.Codigo           as pedido,
		PR.Nome            as produto ,
		PR.ID              as produtoID ,
		PP.ID              as pedidoPagamentoID,
		PP.MeioPagamentoID as meioPagamentoID,
		''                 as meioPagamento,
		PR.ProdutoCategoriaID as produtoCategoriaID,
	    ''                 as categoria,
		PP.Data               as dataStatus,
	    PP.StatusId            as pgtoStatusID,
	    PI.Quantidade      as quantidade,
		PP.ValorCripto        as valor,   --PI.ValorUnitario   as valor, Alterado XSinergia by Rui barbosa
		PP.ValorCripto * PI.Quantidade as total --PI.ValorUnitario * PI.Quantidade as total, Alterado XSinergia by Rui barbosa
    From 
	    Loja.PedidoItem            PI (nolock),
        Loja.Produto               PR (nolock),	  
	    Usuario.Usuario            U  (nolock),	  
	    Loja.pedido                P  (nolock) 
		CROSS APPLY(
			SELECT TOP 1 _PP.ID, _PP.PedidoId, _PP.ValorCripto, _PP.MeioPagamentoID, PS.StatusId, PS.Data FROM 
				loja.PedidoPagamento _PP
				CROSS APPLY (SELECT TOP 1 StatusId, Data FROM loja.PedidoPagamentoStatus WHERE PedidoPagamentoID = _PP.ID  ORDER BY Data DESC) AS PS
			WHERE 
				_PP.Pedidoid=P.ID
			ORDER BY PS.Data DESC) as PP    
    Where P.ID = PI.PedidoID
      and PI.ProdutoID = PR.ID
	  and PR.Composto = 1
      and PR.TipoID in (1,2,4)  -- Produto Físico / Produto Virtual	  -- Alterado para produtos Associação / Upgrade / Físico - XSinergia by Rui barbosa
	  and PR.ProdutoCategoriaID = Coalesce(@Categoria , PR.ProdutoCategoriaID)    
	  and PR.TipoID = Coalesce(@ProdutoTipo , PR.TipoID)    
	  and P.DataCriacao between @DataIni and @DataFim		  
	  and P.ID = PP.PedidoID		   
	  and U.ID = P.UsuarioID
	  and PP.MeioPagamentoID = Coalesce(@MeioPagamento, PP.MeioPagamentoID)
	  and (@Login IS NULL OR  U.login like '%' + @Login + '%') 
	  and (@Produto IS NULL OR PR.Nome like '%' + @Produto + '%')
	  and PP.StatusId = Coalesce(@StatusId , PP.StatusId)     

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
			0                        as produtoID,
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
			0                        as produtoID,
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
		   produtoID     as produtoID,
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
			0                        as produtoID, 	
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
			0                        as produtoID, 	
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
			0                        as produtoID, 	
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
			produtoID     as produtoID, 	
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

