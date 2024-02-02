USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_LO_ObtemListaPedidos]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spOC_LO_ObtemListaPedidos]
   @DataIni  datetime = '2019-01-31 01:00:00.000',
   @DataFim  datetime = '2019-01-31 01:00:00.000',
   @Login    nvarchar(100) = null,
   @StatusId Int = 3

As
-- =============================================================================================
-- Author.....: 
-- Create date: 08/02/2019
-- Description: 

-- Adesão / Upgrade

-- Periodo ___/___/___ ate ___/___/___
-- Login   _____________
-- Status  ___________

-- Data Login Pedido Produto Meio Pgto status Valor Juros Frete Total 
-- ---- ----- ------ ------- --------- ------ ----- ----- ----- ----- 
-- =============================================================================================
BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    Create Table #TUsuario 
        (Id INT);
	      
     Select
         P.DataCriacao      as dataPedido,
		 U.Login            as login,
		 P.Codigo           as pedido,
		 PR.Nome            as produto ,
		 M.Descricao        as meioPagamento,
		 PS.Data            as dataStatus,
		 PS.StatusID        as meioPgtoStatusID,
		 P.Subtotal         as valor,
		 P.Subtotal			as juros,
		 P.Subtotal         as frete,
		 P.Subtotal         as total
     From 
	     Loja.pedido                P  (nolock), 
	     Loja.PedidoItem            PI (nolock),
         Loja.Produto               PR (nolock),	  
	     Usuario.Usuario            U  (nolock),	  
	     Loja.PedidoPagamento       PP (nolock), 
	     Loja.PedidoPagamentoStatus PS (nolock),
		 Rede.Associacao            A  (nolock),
		 Financeiro.MeioPagamento   M  (nolock)
      Where P.ID = PI.PedidoID
        and PI.ProdutoID = PR.ID
        and PR.TipoID in (1,2)  -- Adesao / upgrade	  
		and U.Login = Coalesce(@Login, U.Login)
		and P.DataCriacao between @DataIni and @DataFim	    
	    and P.ID = PP.PedidoID
	    and PP.ID = PS.PedidoPagamentoID
	    and PS.StatusID = Coalesce(@StatusId,PS.StatusID) -- somente pedidos pagos   		
		and PP.MeioPagamentoID = M.ID     
		and U.ID = P.UsuarioID
		and U.NivelAssociacao = A.Nivel
      Order By
	      P.DataCriacao,
	      U.ID;

    Drop Table #TUsuario; 

END;


GO
