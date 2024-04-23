use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformaramPagto'))
   Drop Procedure spC_TabuleiroInformaramPagto
go

Create  Proc [dbo].[spC_TabuleiroInformaramPagto]

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

	Select 
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
		tab.InformePagSistema,
		tab.PagoSistema,
		tab.DataInicio,
		tab.DataFim
	From 
		Rede.TabuleiroUsuario tab,
		Rede.TabuleiroBoard tb
	Where
		tab.BoardID = tb.ID And
        tab.InformePagSistema = 'true' and
		tab.PagoSistema = 'false'
	Order By
		tab.BoardID,
		tab.UsuarioID

End -- Sp

go
Grant Exec on spC_TabuleiroInformaramPagto To public
go

--Exec spC_TabuleiroInformaramPagto 


