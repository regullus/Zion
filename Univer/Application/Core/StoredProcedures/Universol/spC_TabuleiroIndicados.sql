use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroIndicados'))
   Drop Procedure spC_TabuleiroIndicados
go

Create  Proc [dbo].[spC_TabuleiroIndicados]
   @UsuarioID int

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

	Create Table #temp(
		ID int,
		BoardID int,
		BoardNome nvarchar(255),
		TabuleiroID int,
		StatusID int,
		MasterID int,
		Master nvarchar(255),
		UsuarioIDPag int,
		UsuarioPag nvarchar(255),
		Nome nvarchar(255),
		Login nvarchar(255),
		Apelido nvarchar(255),
		Celular nvarchar(255),
		InformePag bit,
		PagoMaster bit,
		InformePagSistema bit,
		PagoSistema bit,
		TotalRecebimento int,
		Posicao nvarchar(255),
		DataInicio datetime
	)

	insert Into #temp
	Select
		usu.ID,
		tab.BoardID,
		boa.Nome,
		tab.TabuleiroID,
		tab.StatusID,
		tab.MasterID,
		'' Master,
		UsuarioIDPag,
		'' UsuarioPag,
		usu.Nome,
		usu.Login,
		usu.Apelido,
		usu.Celular,
		tab.InformePag,
		tab.PagoMaster,
		tab.InformePagSistema,
		tab.PagoSistema,
		tab.TotalRecebimento,
		tab.Posicao,
		tab.DataInicio
	From
		Usuario.Usuario usu,
		Rede.TabuleiroUsuario tab,
		Rede.TabuleiroBoard boa
	Where
		usu.PatrocinadorDiretoID = @UsuarioID and
		usu.id = tab.UsuarioID and
		tab.TabuleiroID is not null and
		tab.BoardID = boa.id

	Update
		tmp
	Set
		tmp.Master = LOWER(usu.apelido)
	From
		#temp tmp,
		Usuario.Usuario usu
	Where
		usu.id = tmp.MasterID

	Update
		tmp
	Set
		tmp.UsuarioPag = LOWER(usu.apelido)
	From
		#temp tmp,
		Usuario.Usuario usu
	Where
		usu.id = tmp.UsuarioIDPag
	
	Select
		ID as usuarioID,
		BoardID,
		BoardNome,
		TabuleiroID,
		StatusID,
		MasterID,
		Master,
		UsuarioIDPag,
		UsuarioPag,
		Nome,
		Login,
		Apelido,
		Celular,
		InformePag,
		PagoMaster,
		InformePagSistema,
		PagoSistema,
		TotalRecebimento,
		Posicao,
		DataInicio
	From
		#temp
	Order by
		BoardID,
		TabuleiroID

End -- Sp

go
Grant Exec on spC_TabuleiroIndicados To public
go

Exec spC_TabuleiroIndicados @UsuarioID=2580






