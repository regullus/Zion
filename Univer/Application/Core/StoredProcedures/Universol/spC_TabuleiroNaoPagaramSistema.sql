use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroNaoPagaramSistema'))
   Drop Procedure spC_TabuleiroNaoPagaramSistema
go

Create  Proc [dbo].[spC_TabuleiroNaoPagaramSistema]

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
		
	Create Table #temp (
		UsuarioID int,
		TabuleiroID int,
		BoardID int,
		BoardNome nvarchar(50),
		BoardCor nvarchar(50),
		StatusID int,
		Eterno bit,
		MasterID int,
		InformePag bit,
		UsuarioIDPag int, --*
		Ciclo int,
		Posicao nvarchar(100),
		PagoMaster bit,
		PagoSistema bit, --*
		InformePagSistema bit,
		TotalRecebimento int, --*
		DataInicio datetime,
		DataFim int
	)

	insert into #temp
	Select 
		tab.UsuarioID,
		tab.TabuleiroID,
		tab.BoardID,
		tb.Nome as BoardNome,
		tb.Cor as BoardCor,
		tab.StatusID,
		'false' as Eterno,
		tab.MasterID,
		tab.InformePag,
		tab.UsuarioIDPag,
		tab.Ciclo,
		tab.Posicao,
		tab.PagoMaster,
		tab.PagoSistema,
		tab.InformePagSistema,
		tab.TotalRecebimento,
		tab.DataInicio,
		tab.DataFim
	From 
		Rede.TabuleiroUsuario tab (nolock),
		Rede.TabuleiroBoard tb (nolock)
	Where
		tab.BoardID = tb.ID And
		tab.PagoSistema = 'false'
	Order By
		tab.BoardID,
		tab.UsuarioID

	Select 
		UsuarioID,
		TabuleiroID,
		BoardID,
		BoardNome,
		BoardCor,
		StatusID,
		Eterno,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		Posicao,
		PagoMaster,
		PagoSistema,
		InformePagSistema,
		TotalRecebimento,
		DataInicio,
		DataFim
	From 
		#temp

End -- Sp

go
Grant Exec on spC_TabuleiroNaoPagaramSistema To public
go

--Exec spC_TabuleiroNaoPagaramSistema 


