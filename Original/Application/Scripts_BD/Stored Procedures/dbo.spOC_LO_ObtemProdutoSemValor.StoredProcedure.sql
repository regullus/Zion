USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_LO_ObtemProdutoSemValor]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[spOC_LO_ObtemProdutoSemValor]
   @tipoProduto char(1) = 'T'

AS
-- =============================================================================================
-- Author.....: Edemar
-- Create date: 16/02/2018
-- Description: Obtem a lista de produtos sem preço
-- Parametros:
--    tipoProduto
--       T - Todos os produtos
--       P - Só os produtos Pai
--       F - Só os produtos filhos
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   -- Tabela temporária com os produtos
   Select 
      *
   INTO  
      #TProduto 
   From 
      Loja.Produto p (nolock)
   Order by
      Nome;

   -- Aplica filtro de tipo de produto
   If(@tipoProduto = 'P')
   Begin
      Delete 
         #TProduto    
      Where 
	     Composto = 0;
   End

   If(@tipoProduto = 'F')
   Begin
      Delete 
         #TProduto    
      Where 
	     Composto = 1;
   End

   -- Elimina os protudos que já tem preço
   Delete 
      #TProduto
   From 
      #TProduto T,
      Loja.ProdutoValor V (nolock)
   Where 
      T.ID = V.ProdutoID;

   -- Atualiza o nome do produto
   Update 
      #TProduto
   Set
      Nome = substring(SKU + ' - ' + Nome , 1 , 100);


   -- Retorna os dados
   Select  
      *
   From
     #TProduto  

   -- Remove todas as tabelas temporárias
   Drop Table #TProduto;
END  


GO
