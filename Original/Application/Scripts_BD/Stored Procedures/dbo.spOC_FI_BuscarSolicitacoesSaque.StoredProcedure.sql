USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_FI_BuscarSolicitacoesSaque]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....: Bruno Almeida
-- Create date: 25/05/2017
-- Description: Busca saques solicitados.
-- =============================================================================================


 CREATE PROCEDURE [dbo].[spOC_FI_BuscarSolicitacoesSaque] @inicial datetime, @final datetime, @login varchar(50) = null  , @quantidade int = null
 AS      
 SELECT       
            S.ID AS Codigo,      
            S.USUARIOID AS UsuarioID,      
            SS.DATA AS Data,      
            S.DATA AS DataS,      
            S.Liquido AS Valor,      
            SS.StatusID as Status,      
            M.Simbolo as Moeda,      
            S.Bitcoin,
            S.Fee       
      
INTO  #TEMPORARIA2       
FROM  FINANCEIRO.Saque S      
INNER JOIN  FINANCEIRO.SaqueStatus SS ON S.ID = SS.SaqueID      
INNER JOIN  GLOBALIZACAO.MOEDA M ON M.ID = S.MoedaID      
LEFT JOIN   Financeiro.ContaDeposito CD ON CD.IDUsuario = S.USUARIOID       
INNER JOIN  Financeiro.MeioPagamento MP ON CD.IDMeioPagamento = MP.ID       
Where      
            CAST(S.DATA as Date) >= @inicial AND       
            CAST(S.DATA as Date) <= @final    
            
   --         AND
			--S.Bitcoin IS NOT NULL AND
	  -- 	    LEN(RTRIM(LTRIM(S.Bitcoin))) > 0   

IF @quantidade is null
	SET ROWCOUNT 10
ELSE
	SET ROWCOUNT @quantidade

SELECT  
             T.Codigo as Codigo,      
             U.Login as Login,      
             T.UsuarioID as UsuarioId,      
             T.DataS AS Data,      
             T.Valor as Valor,      
             T.Status as Status,      
             T.Moeda as Moeda,      
             T.Bitcoin as Bitcoin
      
FROM         #TEMPORARIA2 T INNER JOIN #TEMPORARIA2 TT ON TT.Codigo = T.Codigo       
INNER JOIN  Usuario.Usuario U on T.UsuarioID = U.ID       
WHERE      
         T.DATA = (SELECT MAX(TTT.DATA) FROM #TEMPORARIA2 TTT WHERE TTT.Codigo = T.Codigo) AND       
         T.Status in (1,10) AND    
   U.Login = ISNULL(@login, U.LOGIN)      
GROUP BY      
            T.Codigo,       
            T.UsuarioID,      
            U.Login,      
            T.DataS,      
            T.Valor,      
            T.Status,      
            T.Moeda,      
            T.Bitcoin, 
            T.Fee       
ORDER BY       
            T.Status DESC       
      
DROP TABLE #TEMPORARIA2 




GO
