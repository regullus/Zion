Use ??????????
go
If OBJECT_ID (N'dbo.StringToListInt', N'FN') IS NOT NULL 
   Drop Function StringToListInt;
go

CREATE FUNCTION dbo.StringToListInt(@strString varchar(4000))  
RETURNS  @Result TABLE(Value INT)   

AS   
-- =============================================================================================
-- Author.....: Raphael
-- Create date: ??/12/2019
-- Description: Retorna um float truncado com o numero de casa especificadas entre 0 e 9.
-- =============================================================================================
BEGIN 

   Declare @x XML 

   SELECT @x = cast('<A>'+ replace(@strString,',','</A><A>')+ '</A>' as xml)

   INSERT INTO @Result SELECT t.value('.', 'int') as inVal from @x.nodes('/A') as x(t)

   RETURN;

END;  

go
Grant Exec on StringToListInt To public
go
