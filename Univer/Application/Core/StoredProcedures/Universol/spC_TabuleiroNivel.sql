use UniverDev
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
   --StatusId = 1 Usuario convidado a entrar no tabuleiro pagamento não efetuado
   --StatusId = 2 Usuario ativo no tabuleiro
   --StatusId = 3 Usuario finalizou o tabuleiro
   SELECT
      tn.ID,
      tn.UsuarioID,
      tn.BoardID,
      boa.Nome as BoardNome,
      boa.Cor as BoardCor,
      tab.TabuleiroID as TabuleiroID,
      tab.Posicao,
      tn.DataInicio,
      tn.DataFim,
      tn.StatusID,
      tn.Observacao
  FROM 
      Rede.TabuleiroNivel tn,
      Rede.TabuleiroBoard boa,
      Rede.TabuleiroUsuario tab
   WHERE
      tn.UsuarioID = @UsuarioID and
      tn.StatusID = coalesce(@StatusID, @StatusID, tn.StatusID) and
      tn.BoardID = boa.id and
      tn.UsuarioID = tab.UsuarioID and
      tn.BoardID = tab.boardID
   Order By 
      StatusID
 
End -- Sp

go
Grant Exec on spC_TabuleiroNivel To public
go
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 1
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 2
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 3

Exec spC_TabuleiroNivel @UsuarioID = 2585, @StatusID = 1
Exec spC_TabuleiroNivel @UsuarioID = 2585, @StatusID = 2
Exec spC_TabuleiroNivel @UsuarioID = 2585, @StatusID = 3

Exec spC_TabuleiroNivel @UsuarioID = 2607, @StatusID = 1
Exec spC_TabuleiroNivel @UsuarioID = 2607, @StatusID = 2
Exec spC_TabuleiroNivel @UsuarioID = 2607, @StatusID = 3

Select * from rede.TabuleiroUsuario

