Use ??????????
go
If OBJECT_ID (N'dbo.GetDateZion', N'FN') IS NOT NULL 
   Drop Function GetDateZion;
go

CREATE FUNCTION dbo.GetDateZion()  
RETURNS DateTime  

AS   
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 25/052017
-- Description: Retorna um DateTime com o fuso horario horario do cliente. Definido o horario de 
--              Brasilia como padrão.
-- =============================================================================================
BEGIN  
    Declare @timeZion int = ISNULL((Select convert(int, Dados) 
	                                 From Sistema.Configuracao (nolock) 
									         Where Chave = 'DATE_TIME_ZION') , -3); -- P Papa UTC −3 horas
     
    RETURN DATEADD(hour,@timeZion,GetDate()); 
END;  

go
Grant Exec on GetDateZion To public
go

--Select  getdate() , dbo.GetDateZion() 


