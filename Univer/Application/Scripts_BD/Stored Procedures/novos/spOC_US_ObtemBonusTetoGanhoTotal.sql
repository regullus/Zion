
USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemBonusTetoGanhoTotal]    Script Date: 08/07/2019 15:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Proc [dbo].[spOC_US_ObtemBonusTetoGanhoTotal]
   @idUsuario int

As
-- =============================================================================================
-- Author.....: Vinicius Castro
-- Create date: 08/07/2019
-- Description: Obtem o teto do bonus ganho total 
-- =============================================================================================
BEGIN
   SET FMTONLY OFF
   SET NOCOUNT ON

   DECLARE @tetoBonus INT;
   DECLARE @tetoAdesao INT;

   declare @valorAdesao float = 50
   declare @vezes_teto_total int = 4
   
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
   
   select (isnull(@valor_total,0) * @vezes_teto_total) - (isnull(@qtde_associacoes,0) * @valorAdesao * @vezes_teto_total) as valor
   
END


--exec spOC_US_ObtemBonusTetoGanhoTotal 1000


