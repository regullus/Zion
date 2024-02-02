/*    ==Parâmetros de Script==

    Versão do Servidor de Origem : SQL Server 2016 (13.0.5426)
    Edição do Mecanismo de Banco de Dados de Origem : Microsoft SQL Server Standard Edition
    Tipo do Mecanismo de Banco de Dados de Origem : SQL Server Autônomo

    Versão do Servidor de Destino : SQL Server 2017
    Edição de Mecanismo de Banco de Dados de Destino : Microsoft SQL Server Standard Edition
    Tipo de Mecanismo de Banco de Dados de Destino : SQL Server Autônomo
*/

USE [xsinerdb_dev]
GO
/****** Object:  StoredProcedure [dbo].[sp_XS_Cotacao]    Script Date: 16/12/2019 15:47:56 ******/
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