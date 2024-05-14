use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroGetDate'))
   Drop Procedure spC_TabuleiroGetDate
go

Create  Proc [dbo].[spC_TabuleiroGetDate]
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem Bdados do tabuleiro de um usuario
-- =============================================================================================
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

	Select GetDate() retorno
End -- Sp

go
Grant Exec on spC_TabuleiroGetDate To public
go

--Exec spC_TabuleiroGetDate






