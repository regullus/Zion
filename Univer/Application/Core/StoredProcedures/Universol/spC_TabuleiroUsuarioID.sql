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
		TotalRecebimento,
		tab.DataInicio,
		tab.DataFim
	From 
		Rede.TabuleiroUsuario tab (nolock),
		Rede.TabuleiroBoard tb (nolock)
	Where
		tab.UsuarioID = @UsuarioID and
		tab.BoardID = @BoardID and
		tab.BoardID = tb.ID
	Order By
		BoardID

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
Grant Exec on spC_TabuleiroUsuarioID To public
go

--Exec spC_TabuleiroUsuarioID @UsuarioID=2589, @BoardID=2






