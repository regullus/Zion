Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_LO_ObtemPedidosPendentesPgto'))
   Drop Procedure spOC_LO_ObtemPedidosPendentesPgto
go

Create PROCEDURE [dbo].spOC_LO_ObtemPedidosPendentesPgto
   @UsuarioID INT

AS
-- =============================================================================================
-- Author.....: Adamastor
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

go
   Grant Exec on spOC_LO_ObtemPedidosPendentesPgto To public
go

-- Exec spOC_LO_ObtemPedidosPendentesPgto 1000
-- Exec spOC_LO_ObtemPedidosPendentesPgto 2010