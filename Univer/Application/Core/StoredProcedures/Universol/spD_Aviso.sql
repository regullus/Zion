Use univerDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spD_Aviso'))
   Drop Procedure spD_Aviso
go

Create PROCEDURE [dbo].[spD_Aviso]
   @ID   int

AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 16/02/2018
-- Description: Obtem os Avisos do usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   Delete Usuario.AvisoLido Where AvisoID = @ID
   Delete Usuario.Aviso Where ID = @ID

END  

go
   Grant Exec on spD_Aviso To public
go
--Exec spD_Aviso 2586
Exec spD_Aviso 4
