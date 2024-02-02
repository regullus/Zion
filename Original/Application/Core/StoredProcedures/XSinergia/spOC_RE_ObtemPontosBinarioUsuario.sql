
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_RE_ObtemPontosBinarioUsuario'))
   Drop Procedure spOC_RE_ObtemPontosBinarioUsuario
go
  
Create PROCEDURE [dbo].[spOC_RE_ObtemPontosBinarioUsuario]  
   @UsuarioId     Int     ,    
   @DataIni       datetime,    
   @DataFim       datetime,
   @Totaliza      int = 0  
    
AS  
  
-- =============================================================================================  
-- Author.....: Adamastor  
-- Create date: 27/12/2019  
-- Description: Lista os pontos e valores binarios de um usuario do periodo selecionando  
-- =============================================================================================  
  
BEGIN  
   BEGIN TRY  
      Set nocount on  
  
   -- Temporaria de pontos do Usuario  
   Create Table #TUsuario  (   
      tipoRg               int,  
      dataCriacao          datetime,  
      login                nvarchar(255),  
      pedidoIDOrigem       int,  
      produtoNome          nvarchar(255),     
      pontosEsqueda        float,    
      valorEsqueda         float,      
      pontosDireita        float,  
      valorDireita         float,
	  PatrocinadorDiretoID int ,
	  loginPatrocinador    nvarchar(255),          
   );  
  
   -- Inclui os Pontos e Valores  
   Insert Into #TUsuario  
   Select   
      1 as tipoRg      ,  
      P.DataCriacao    ,  
      '' as login      ,  
      P.PedidoIDOrigem ,  
      '' as produtoNome,  
      Case When P.Lado = 0 Then IsNull(P.Pontos  , 0) Else 0 End pontosEsqueda,  
      Case When P.Lado = 0 Then IsNull(P.ValorCripto, 0) Else 0 End valorEsqueda ,    
      Case When P.Lado = 1 Then IsNull(P.Pontos  , 0) Else 0 End pontosDireita,   
      Case When P.Lado = 1 Then IsNull(P.ValorCripto, 0) Else 0 End valorDireita ,
	  0,
	  ''                    
   From 
      Rede.PontosBinario P (nolock)  
   Where P.UsuarioID = @UsuarioId   
     and P.DataCriacao Between  @DataIni and  @DataFim;  
  
   -- Inclui os Pontos e Valores do Log  
   Insert Into #TUsuario  
   Select   
      1 as tipoRg      ,  
      --P.DataCriacao    ,  
	   CAST(CONVERT(VARCHAR(8), P.DataCriacao, 112) + ' 00:00:00' as datetime2),  
      '' as login      ,  
      P.PedidoIDOrigem ,  
      '' as produtoNome,  
      Case When P.Lado = 0 Then IsNull(P.Pontos  , 0) Else 0 End pontosEsqueda,  
      Case When P.Lado = 0 Then IsNull(P.ValorCripto, 0) Else 0 End valorEsqueda ,    
      Case When P.Lado = 1 Then IsNull(P.Pontos  , 0) Else 0 End pontosDireita,   
      Case When P.Lado = 1 Then IsNull(P.ValorCripto, 0) Else 0 End valorDireita ,
	  0,
	  ''                  
   From 
      Rede.PontosBinarioLog P (nolock)  
   Where P.UsuarioID = @UsuarioId   
     and P.DataCriacao Between  @DataIni and  @DataFim;  
  

   If (@Totaliza = 1)
   Begin
      Insert Into #TUsuario  
      Select   
         5 as tipoRg    ,   
	     CAST(CONVERT(VARCHAR(8), U.DataCriacao , 112) + ' 00:00:00' as datetime2),  
         'Subtotal' as login     ,  
         0 as PedidoIDOrigem,  
         '' as produtoNome   ,  
         Sum(U.PontosEsqueda) as pontosEsqueda,  
         Sum(U.ValorEsqueda)  as valorEsqueda,    
         Sum(U.PontosDireita) as pontosDireita,   
         Sum(U.ValorDireita)  as valorDireita  ,
	     0,
	     ''              
      From 
         #TUsuario U  
      Group by  
         U.dataCriacao;  
   End
  
   If (@Totaliza = 1)
   Begin
      Insert Into #TUsuario  
      Select   
         9 as tipoRg       ,  
         Min(DataCriacao)   ,  
         'Total' as Login   ,  
         0 as pedidoIDOrigem,  
         '' as produtoNome   ,  
         Sum(U.PontosEsqueda) as pontosEsqueda,  
         Sum(U.ValorEsqueda)  as valorEsqueda,    
         Sum(U.PontosDireita) as pontosDireita,   
         Sum(U.ValorDireita)  as valorDireita ,
	     0,
	     ''            
      From #TUsuario U  
      where U.tipoRg = 1  
      Group by  
         U.tipoRg;
   End  
  
   -- Atualiza as informações  
   Update #TUsuario  
   Set   
      login = U.Login,  
      produtoNome = PR.Nome,
	  PatrocinadorDiretoID = U.PatrocinadorDiretoID
   From #TUsuario T  
        inner join Loja.Pedido     P  (nolock) on P.ID = T.PedidoIDOrigem  
        inner join Usuario.Usuario U  (nolock) on U.ID = P.UsuarioID  
        inner join Loja.PedidoItem I  (nolock) on I.PedidoID = P.ID  
        inner join Loja.PRoduto    PR (nolock) on PR.ID = I.ProdutoID  
   Where T.TipoRg = 1;    
    
   Update #TUsuario  
   Set   
	  loginPatrocinador = U.Login
   From #TUsuario T        
        inner join Usuario.Usuario U  (nolock) on U.ID = T.PatrocinadorDiretoID          
   Where T.TipoRg = 1;  
   
       
   -- Retorno dos dados   
   Select 
      *   
   from 
      #TUsuario   
   order by       
      DataCriacao desc, 
	  TipoRg;
  
   -- Remove todas as tabelas temporárias  
   Drop Table #TUsuario;  
  
   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION;  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spOC_RE_ObtemPontosBinarioUsuario: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END   
  
go
   Grant Exec on spOC_RE_ObtemPontosBinarioUsuario To public
go 
