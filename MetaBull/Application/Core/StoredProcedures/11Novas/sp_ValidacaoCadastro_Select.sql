SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_ValidacaoCadastro_Select]

as

DECLARE @Total int, @Valido int, @invalido int

SELECT  @Total = COUNT(0) from Usuario.Usuario
SELECT  @Valido = COUNT(0) from Usuario.ValidacaoCadastro WHERE ok = 1
SELECT  @invalido = COUNT(0) from Usuario.ValidacaoCadastro WHERE ok = 0

SELECT  @Valido as Válidos, @invalido as 'Inválidos', @Total as 'Total'
