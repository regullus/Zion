use [Univer]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroNivel'))
   Drop Procedure spC_TabuleiroNivel
go

Create  Proc [dbo].[spC_TabuleiroNivel]
   @UsuarioID int,
   @StatusID int

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem niveis no tabuleito de um usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   SELECT
      ID,
      UsuarioID,
      BoardID,
      DataInicio,
      DataFim,
      StatusID,
      Observacao
  FROM 
      Rede.TabuleiroNivel
   WHERE
      UsuarioID = @UsuarioID and
      StatusID = @StatusID
 
End -- Sp

go
Grant Exec on spC_TabuleiroNivel To public
go

Exec spC_TabuleiroNivel @UsuarioID = 2587, @StatusID = 1
Exec spC_TabuleiroNivel @UsuarioID = 2587, @StatusID = 2
Exec spC_TabuleiroNivel @UsuarioID = 2587, @StatusID = 3

Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 1
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 2
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 3
