DROP function [dbo].[fn_ObtemBonusTetoGanhoAlavancagem];
go

CREATE function fn_ObtemBonusTetoGanhoAlavancagem (@idUsuario int)
returns float
AS
begin

	declare @teto float = 0;
   DECLARE @tetoBonus INT;
   DECLARE @tetoAdesao INT;

   declare @valorAdesao float = 50
   declare @vezes_teto_alavancagem int = 2
   
   declare @valor_total float, @qtde_associacoes int 
   
   SET @valor_total = (SELECT sum(POV.Valor) FROM Usuario.usuarioAssociacao UA 
   inner join Loja.Produto P on UA.NivelAssociacao = P.NivelAssociacao	
   inner join Loja.ProdutoValor POV ON P.ID = POV.ProdutoID
   where usuarioID = @idUsuario
   and P.SKU like 'ADE%'
   and UA.DataValidade >= getdate())
   
   SET @qtde_associacoes = (SELECT count(UA.ID) FROM Usuario.usuarioAssociacao UA 
   where usuarioID = @idUsuario
   and UA.DataValidade >= getdate())
   
   set @teto = (isnull(@valor_total,0) * @vezes_teto_alavancagem) - (isnull(@qtde_associacoes,0) * @valorAdesao * @vezes_teto_alavancagem)

	return @teto;

end