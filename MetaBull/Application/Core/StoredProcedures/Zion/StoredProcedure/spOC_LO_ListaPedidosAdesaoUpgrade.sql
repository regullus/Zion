use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_LO_ListaPedidosAdesaoUpgrade'))
   Drop Procedure spOC_LO_ListaPedidosAdesaoUpgrade
go

CREATE Proc dbo.spOC_LO_ListaPedidosAdesaoUpgrade
   @DataIni       datetime            ,
   @DataFim       datetime            ,
   @Identificacao nvarchar(255) = null,
   @StatusId      Int           = null

As
-- =============================================================================================
-- Author.....: 
-- Create date: 28/02/2019
-- Description: Lista de Pedidos de Adesão e Upgrade
-- =============================================================================================
BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    Create Table #TPedidoItens
    (
	     dataPedido        datetime     ,
		 login             nvarchar(255),	
		 pedido            nvarchar(255),
		 produto           nvarchar(255),
		 pedidoPagamentoID int          ,
		 meioPagamento     nvarchar(255),
		 dataStatus        datetime     ,
		 pgtoStatusID      int,
		 valor             float,
		 juros             float,
		 frete             float,
		 total             float      
    );

    -- Obtem os Itens do Pedidos dos Usuarios	 
	Insert into #TPedidoItens     
    Select 
        P.DataCriacao      as dataPedido,
		U.Login            as login,	
		P.Codigo           as pedido,
		PR.Nome            as produto ,
		PP.ID              as pedidoPagamentoID,
		M.Descricao        as meioPagamento,
	    null,
	    0,
		P.Subtotal         as valor,
		P.ValorJuros		as juros,
		P.ValorFrete       as frete,
		P.Subtotal         as total
    From 
	    Loja.pedido                P  (nolock), 
	    Loja.PedidoItem            PI (nolock),
        Loja.Produto               PR (nolock),	  
	    Usuario.Usuario            U  (nolock),	  
	    Loja.PedidoPagamento       PP (nolock), 
	    Rede.Associacao            A  (nolock),
        Financeiro.MeioPagamento   M  (nolock)
    Where P.ID = PI.PedidoID
      and PI.ProdutoID = PR.ID
      and PR.TipoID in (1,2)  -- Adesao / upgrade	  
	  and P.DataCriacao between @DataIni and @DataFim	    
	  and P.ID = PP.PedidoID	
	  and PP.MeioPagamentoID = M.ID     
	  and U.ID = P.UsuarioID
	  and U.NivelAssociacao = A.Nivel;

    -- Elimina os usuarios que não contem a identificação como parte do login
	If (@Identificacao is not null and @Identificacao != '' )
	Begin
	    Delete
		    #TPedidoItens
		Where
		    login not like '%' + @Identificacao + '%'; 
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

	-- Retorna dados
    Select 
	     1             as tipoRg,
	     dataPedido    as dataPedido,
		 login         as login,
		 pedido        as pedido,
		 ''            as categoria,
		 produto       as produto,
		 meioPagamento as meioPagamento,
		 dataStatus    as dataStatus,
		 pgtoStatusID  as pgtoStatusID,
		 0             as quantidade,
		 valor         as valor,
		 juros         as juros,
		 frete         as frete,
		 total         as total         
    From 
       #TPedidoItens
    Order By
	   dataPedido,
	   login;

    -- Remove as Tabelas Temporarias
    Drop Table #TPedidoItens
END;

go
Grant Exec on spOC_LO_ListaPedidosAdesaoUpgrade To public
go

--Exec spOC_LO_ListaPedidosAdesaoUpgrade @DataIni= '2018-11-01 01:00:00.000', 
--                                       @DataFim= '2019-02-28 01:00:00.000',
--							             @Identificacao='Batman' 
							 
--Exec spOC_LO_ListaPedidosAdesaoUpgrade @DataIni= '2018-11-01 01:00:00.000', 
--							             @DataFim= '2019-02-28 01:00:00.000'

Exec spOC_LO_ListaPedidosAdesaoUpgrade @DataIni= '2018-11-01 01:00:00.000', 
					  	               @DataFim= '2019-02-28 01:00:00.000',
							           @StatusId = 1
							 
--Exec spOC_LO_ListaPedidosAdesaoUpgrade @DataIni= '2018-11-01 01:00:00.000', 
--					                     @DataFim= '2019-02-28 01:00:00.000',
--						                 @Identificacao='batman', 
--					           		     @StatusId = 3