DROP function [dbo].[fn_ObtemBonusTetoGanhoBinario];
go


CREATE function fn_ObtemBonusTetoGanhoBinario (@idUsuario int)
	returns float
AS
begin

	declare @valorAdesao float = 15;
	declare @teto float = 0;
	declare @nivel int;
	/*limite de binario diario: valor do plano adquirido*/
	set @nivel = (select max(NivelAssociacao) from Usuario.UsuarioAssociacao where UsuarioID = @idUsuario)
	set @teto = (select PV.Valor  from Loja.Produto P inner join Loja.ProdutoValor PV on P.ID = PV.ProdutoID  where SKU like 'ADE%' and NivelAssociacao = @nivel)
	
	if (@teto is not null and @teto > 0) 
	begin
		set @teto = @teto - @valorAdesao
	end

	return @teto;

end