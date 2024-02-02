
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_FI_RelatorioPagamento'))
   Drop Procedure spOC_FI_RelatorioPagamento
go

  
CREATE PROC [dbo].[spOC_FI_RelatorioPagamento]  
 @DataIni DATETIME,  
 @DataFim DATETIME  
  
AS  

-- =============================================================================================  
-- Author.....  :   
-- Create date  :   
-- Description  : Exibe Relatório de Pagamentos    
-- Modified by  : Marcos Hemmi  
-- Modified date: 07/02/2018  
-- =============================================================================================  

BEGIN  
     
 --Necessario para o entity reconhecer retorno de select com tabela temporaria  
 SET FMTONLY OFF  
 SET NOCOUNT ON  
  
 SELECT  
  usr.Login,   
  Usr.Nome,   
  Usr.Celular,   
  prd.SKU,   
  prd.Nome      AS Produto,   
  end_p.Logradouro,  
  end_p.Numero,  
  end_p.Complemento,  
  end_p.CodigoPostal,  
  cid_p.Nome      AS Cidade,  
  est_p.Nome      AS Estado,  
  end_ent.Logradouro    AS LogradouroAlt,  
  end_ent.Numero     AS NumeroAlt,  
  end_ent.Complemento    AS ComplementoAlt,  
  end_ent.CodigoPostal   AS CodigoPostalAlt,  
  cid_ent.Nome     AS CidadeAlt,  
  est_ent.Nome     AS EstadoAlt,  
  convert(varchar,sta.Data, 103) AS DataPagamento,   
  ped.Codigo,  
  ISNULL(pag.Valor, 0)   AS Total,  
  ISNULL(pag.ValorCripto, 0)                    AS TotalBTC,     -- Alterado XSinergia by Rui barbosa  
  Usr.Email                                        -- Alterado XSinergia by Rui barbosa  
 FROM   
  Loja.Pedido ped (NOLOCK)  
  INNER JOIN Usuario.Usuario usr (NOLOCK)   
   ON usr.ID = ped.UsuarioID  
  INNER JOIN Loja.PedidoPagamento pag (NOLOCK)   
   ON pag.PedidoID = ped.ID  
  INNER JOIN Loja.PedidoPagamentoStatus sta (NOLOCK)   
   ON sta.PedidoPagamentoID = pag.ID  
  INNER JOIN Loja.PedidoItem item (NOLOCK)   
   ON item.PedidoID = ped.ID  
  INNER JOIN Loja.Produto prd (NOLOCK)   
   ON prd.ID = item.ProdutoID  
  LEFT JOIN Usuario.Endereco end_p (NOLOCK)   
   ON end_p.UsuarioID = Usr.ID  
   AND end_p.Nome = '[PRINCIPAL]'  
  LEFT JOIN Globalizacao.Cidade cid_p (NOLOCK)   
   ON cid_p.ID = end_p.CidadeID  
  LEFT JOIN Globalizacao.Estado est_p (NOLOCK)   
   ON est_p.ID = cid_p.EstadoID  
  LEFT JOIN Usuario.Endereco end_ent (NOLOCK)   
   ON end_ent.UsuarioID = Usr.ID  
   AND end_ent.Nome = '[ALTERNATIVO]'  
  LEFT JOIN Globalizacao.Cidade cid_ent (NOLOCK)   
   ON cid_ent.ID = end_ent.CidadeID  
  LEFT JOIN Globalizacao.Estado est_ent (NOLOCK)   
   ON est_ent.ID = cid_ent.EstadoID  
 WHERE   
  sta.StatusID = 3  
  AND (  
   (@DataIni IS NOT NULL AND @DataFim IS NOT NULL AND sta.Data BETWEEN @DataIni AND @DataFim)  
   OR (@DataIni IS NULL AND @DataFim IS NOT NULL AND sta.Data <= @DataFim)  
   OR (@DataIni IS NOT NULL AND @DataFim IS NULL AND sta.Data >= @DataIni)  
   OR (@DataIni IS NULL AND @DataFim IS NULL)  
  )  
  
END  

go
   Grant Exec on spOC_FI_RelatorioPagamento To public
go


