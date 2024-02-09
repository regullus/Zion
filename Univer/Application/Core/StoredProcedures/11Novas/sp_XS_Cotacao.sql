SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_XS_Cotacao]

@moedaBTC int,
@moedaUSD int,
@moedaTipoSaida int,
@cotacaoBTCDia float OUT

AS

	SELECT @cotacaoBTCDia = Valor FROM Globalizacao.MoedaCotacao WHERE MoedaOrigemID = @moedaBTC AND MoedaDestinoID = @moedaUSD AND TipoID = @moedaTipoSaida

	IF(@cotacaoBTCDia is null OR @cotacaoBTCDia = 0)
	BEGIN
		SELECT @cotacaoBTCDia = convert(float, Dados) FROM Sistema.Configuracao WHERE CHAVE  = 'COTACAO_BTC_USD_DEFAULT'
	END

	IF(@cotacaoBTCDia is null OR @cotacaoBTCDia = 0)
	BEGIN
		SELECT @cotacaoBTCDia = 8000
	END
GO