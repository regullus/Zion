use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUsuario'))
   Drop Procedure spC_TabuleiroUsuario
go

Create  Proc [dbo].[spC_TabuleiroUsuario]
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

	if(@UsuarioID is null or @UsuarioID = 0)
	Begin
		--Obtem os 10 primeiros ativos no Board 1
		Insert Into #temp
		Select distinct top(60)
			tab.UsuarioID,
			tab.TabuleiroID,
			tab.BoardID,
			tb.Nome as BoardNome,
			tb.Cor as BoardCor,
			tab.StatusID,
			'true' as Eterno,
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
			Rede.TabuleiroBoard tb (nolock),
            Rede.Tabuleiro redtab (nolock)
		Where
			tab.BoardID = tb.ID And
			tab.StatusID = 1 And
			tab.BoardID = 1 and
            redtab.id = tab.TabuleiroID and
            redtab.StatusID <> 2
		Order By
			TabuleiroID
	End
	Else 
	Begin
	    --Obtem todos os Boards (niveis) do usuario
		Insert Into #temp
		Select 
			tab.UsuarioID,
			tab.TabuleiroID,
			tab.BoardID,
			tb.Nome as BoardNome,
			tb.Cor as BoardCor,
			tab.StatusID,
			'true' as Eterno,
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
			tab.UsuarioID = @UsuarioID and
			tab.BoardID = tb.ID
		Order By
			TabuleiroID
	End

	--Verifica se disponibiliza Mercurio para o usuario
	--Se ele nao pagou o alvo no proximo nivel, estando nele, nao abilita
	-- a entrada em mercurio

	--Verifica se a reentrada em mercurio StatusID = 2 no BoardID = 1
	If Exists( 
		Select 'OK'
		From
			#temp
		Where
			BoardID = 1 and
			StatusID = 2
	)
	Begin
		--Havendo reentrada, verifica em que boardID superior o usuario se encontra
		Declare @BoardID int
		Select 
			@BoardID = MAX(BoardID)
		From
			#temp
		Where
			StatusID <> 0
		
		--Verifica se nao pagou o Master no board superior
		If Exists (
			Select 
				'OK'
			From
				#temp
			Where
				BoardID = @BoardID and
				PagoMaster = 'false'
		)
		Begin
			--Nao estando pago, nao abilita mercurio para reentrada
			Update
				#temp
			Set 
				Eterno = 'false'
			Where
				BoardID = 1 --Mercurio
		End
	End

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
Grant Exec on spC_TabuleiroUsuario To public
go

Exec spC_TabuleiroUsuario @UsuarioID=2619





