DROP function [dbo].[fn_ObtemBonusTetoGanhoArbitragem];
go

CREATE function fn_ObtemBonusTetoGanhoArbitragem (@idUsuario int)
returns float
AS
begin

   declare @teto float = 0;
   declare @somaValor float = 0;
   declare @somaBTC float = 0;
  
	set @somaBTC = (select dbo.fn_ObtemReferenciaGanhoArbitragem (@idUsuario))
	
	set @teto = @somaBTC * 2;
  
   /*teto de bonus arbitragem: soma de BTC de todos pacotes pagos * 200% */
   
	return @teto;

end