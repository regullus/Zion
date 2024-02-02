  
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_FI_BuscarSolicitacoesSaque'))
   Drop Procedure  spOC_FI_BuscarSolicitacoesSaque
go

CREATE PROCEDURE [dbo].[spOC_FI_BuscarSolicitacoesSaque] 
   @inicial    datetime, 
   @final      datetime, 
   @login      varchar(50) = null, 
   @status     int = 0  , 
   @quantidade int = null  

AS    
 -- =============================================================================================  
-- Author.....: Almeida  
-- Create date: 25/05/2017  
-- Description: Busca saques solicitados.  
-- =============================================================================================  
BEGIN
    SELECT         
       S.ID         AS Codigo,        
       S.USUARIOID  AS UsuarioID,        
       SS.DATA      AS Data,        
       S.DATA       AS DataS,        
       S.Liquido    AS Valor, 
	   S.LiquidoBTC AS ValorCripto,       
       SS.StatusID  as Status,        
       M.Simbolo    as Moeda,        
       S.Bitcoin,  
       S.Fee,  
       SS.Mensagem                
    INTO  #TEMPORARIA2         
    FROM  
	   FINANCEIRO.Saque S (nolock)        
       INNER JOIN  FINANCEIRO.SaqueStatus   SS (nolock) ON S.ID = SS.SaqueID        
       INNER JOIN  GLOBALIZACAO.MOEDA       M  (nolock) ON M.ID = S.MoedaID        
       LEFT JOIN   Financeiro.ContaDeposito CD (nolock) ON CD.IDUsuario = S.USUARIOID         
       INNER JOIN  Financeiro.MeioPagamento MP (nolock) ON CD.IDMeioPagamento = MP.ID         
    Where CAST(S.DATA as Date) >= @inicial 
	  AND CAST(S.DATA as Date) <= @final                   
   -- AND S.Bitcoin IS NOT NULL 
   -- AND LEN(RTRIM(LTRIM(S.Bitcoin))) > 0     
  
   IF @quantidade is null  
      SET ROWCOUNT 50  
   ELSE  
      SET ROWCOUNT @quantidade  
  
   SELECT    
      T.Codigo    as Codigo,        
      U.Login     as Login,        
      U.Nome      as Nome,        
      T.UsuarioID as UsuarioId,        
      T.DataS     as Data,        
      T.Valor     as Valor,      
	  T.ValorCripto  as ValorCripto,    
      T.Status    as Status,        
      T.Moeda     as Moeda,        
      T.Bitcoin   as Bitcoin,  
      T.Mensagem  as Mensagem
   FROM  
      #TEMPORARIA2 T 
	  INNER JOIN #TEMPORARIA2 TT ON TT.Codigo = T.Codigo         
      INNER JOIN  Usuario.Usuario U (nolock) on T.UsuarioID = U.ID         
   WHERE T.DATA = (SELECT MAX(TTT.DATA) FROM #TEMPORARIA2 TTT WHERE TTT.Codigo = T.Codigo) 
     AND (@status = 0 OR T.Status = @status) 
	 AND U.Login = ISNULL(@login, U.LOGIN)        
   GROUP BY        
      T.Codigo,         
      T.UsuarioID,        
      U.Login,        
      U.Nome,        
      T.DataS,        
      T.Valor,
	  T.ValorCripto,        
      T.Status,        
      T.Moeda,        
      T.Bitcoin,   
      T.Fee,  
      T.Mensagem         
   ORDER BY         
      T.Status DESC;      
        
   DROP TABLE #TEMPORARIA2  
END;

go
Grant Exec on spOC_FI_BuscarSolicitacoesSaque To public
go