Use ??????????
go
If OBJECT_ID (N'dbo.TruncZion', N'FN') IS NOT NULL 
   Drop Function TruncZion;
go

CREATE FUNCTION dbo.TruncZion(@valor float, @casas int = 0)  
RETURNS float   

AS   
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 31/05/2017
-- Description: Retorna um float truncado com o numero de casa especificadas entre 0 e 9.
-- =============================================================================================
BEGIN 

   Declare @P float = POWER(10, @casas);
 
   RETURN FLOOR( @valor * @P ) / @P ;

END;  

go
Grant Exec on TruncZion To public
go

-- Declare @a float = 0.3;              Select @a , dbo.TruncZion(@a,8) , round(@a, 8, 1)  
-- Declare @a float = 0.3111111999999;  Select @a , dbo.TruncZion(@a,8) , round(@a, 8, 1) 
-- declare @a float = 9567.18991312535; Select @a , dbo.TruncZion(@a,8) , round(@a, 8, 1) 

