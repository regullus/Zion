use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUsuarioID'))
   Drop Procedure spC_TabuleiroUsuarioID
go

Create  Proc [dbo].[spC_TabuleiroUsuarioID]
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

    --Obtem para um dado tabuleiro
	Select 
		tab.UsuarioID,
		tab.TabuleiroID,
		tab.BoardID,
		tb.Nome as BoardNome,
		tb.Cor as BoardCor,
		tab.StatusID,
		tab.MasterID,
		tab.InformePag,
		tab.UsuarioIDPag,
		tab.Ciclo,
		tab.Posicao,
		tab.PagoMaster,
		tab.InformePagSistema,
		tab.PagoSistema,
		tab.DataInicio,
		tab.DataFim
	From 
		Rede.TabuleiroUsuario tab,
		Rede.TabuleiroBoard tb
	Where
		tab.UsuarioID = @UsuarioID and
		tab.BoardID = @BoardID and
		tab.BoardID = tb.ID
	Order By
		BoardID

End -- Sp

go
Grant Exec on spC_TabuleiroUsuarioID To public
go

--Exec spC_TabuleiroUsuarioID @UsuarioID=2587, @BoardID=1






