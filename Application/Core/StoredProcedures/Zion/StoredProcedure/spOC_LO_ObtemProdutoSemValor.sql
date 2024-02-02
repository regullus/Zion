Use ??????????
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_LO_ObtemProdutoSemValor'))
   Drop Procedure spOC_LO_ObtemProdutoSemValor
go

Create PROCEDURE [dbo].[spOC_LO_ObtemProdutoSemValor]
   @tipoProduto char(1) = 'T'

AS
-- =============================================================================================
-- Author.....: Adamastor
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

go
   Grant Exec on spOC_LO_ObtemProdutoSemValor To public
go

-- Exec spOC_LO_ObtemProdutoSemValor T
-- Exec spOC_LO_ObtemProdutoSemValor P
-- Exec spOC_LO_ObtemProdutoSemValor F



