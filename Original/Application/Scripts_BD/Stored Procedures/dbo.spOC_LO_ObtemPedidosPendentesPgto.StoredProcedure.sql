USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_LO_ObtemPedidosPendentesPgto]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[spOC_LO_ObtemPedidosPendentesPgto]
   @UsuarioID INT

AS
-- =============================================================================================
-- Author.....: Edemar
-- Create date: 05/12/2017
-- Description: Obtem a quantidade de pedidos não pagos do usuario informado
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on
    
   -- Tabela temporária com os pedidos dos usuário 
   Create Table #TPedidos
   (	     
      PedidoId INT
   );

   Insert Into #TPedidos 
   Select p.ID   
   From Loja.Pedido P (nolock)
   Where P.UsuarioID = @UsuarioID;
      
   -- Remove os pedidos Pagos
   Delete #TPedidos
   From #TPedidos T,
        Loja.PedidoPagamento PP (nolock),
		Loja.PedidoPagamentoStatus PPS (nolock)        
   Where PP.PedidoID = T.PedidoID
     And PPS.PedidoPagamentoID = pp.ID
	 And PPS.StatusID = 3;

   -- Retorna a quantidade de pedidos não pagos
   Select count(*)
   From #TPedidos;

   -- Remove todas as tabelas temporárias
   Drop Table #TPedidos;
END  


GO
