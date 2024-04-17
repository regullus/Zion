use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUsuario'))
   Drop Procedure spC_TabuleiroUsuario
go

Create  Proc [dbo].[spC_TabuleiroUsuario]
   @UsuarioID int,
   @BoardID int 

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

	if (@BoardID is null or @BoardID = 0)
	Begin
	    --Obtem todos os Boards (niveis)
		Select 
			tab.ID,
			tab.UsuarioID,
			tab.TabuleiroID,
			tab.BoardID,
			tb.Nome as BoardNome,
			tb.Cor as BoardCor,
			tab.StatusID,
			tab.MasterID,
			tab.InformePag,
			tab.Ciclo,
			tab.Posicao,
			tab.PagoMaster,
			tab.PagoSistema,
			tab.DataInicio,
			tab.DataFim
		From 
			Rede.TabuleiroUsuario tab,
			Rede.TabuleiroBoard tb
		Where
			tab.UsuarioID = @UsuarioID and
			tab.StatusID = 1 and
			tab.BoardID = tb.ID

		Order By
			TabuleiroID
	End
	Else
	Begin
	    --Obtem para um dado tabuleiro
		Select 
			tab.ID,
			tab.UsuarioID,
			tab.TabuleiroID,
			tab.BoardID,
			tb.Nome as BoardNome,
			tb.Cor as BoardCor,
			tab.StatusID,
			tab.MasterID,
			tab.InformePag,
			tab.Ciclo,
			tab.Posicao,
			tab.PagoMaster,
			tab.PagoSistema,
			tab.DataInicio,
			tab.DataFim
		From 
			Rede.TabuleiroUsuario tab,
			Rede.TabuleiroBoard tb
		Where
			tab.UsuarioID = @UsuarioID and
			tab.BoardID = @BoardID and
			tab.StatusID = 1 and
			tab.BoardID = tb.ID
		Order By
			TabuleiroID
	End

End -- Sp

go
Grant Exec on spC_TabuleiroUsuario To public
go
Exec spC_TabuleiroUsuario @UsuarioID=2580, @BoardID=1
Exec spC_TabuleiroUsuario @UsuarioID=2580, @BoardID=null






