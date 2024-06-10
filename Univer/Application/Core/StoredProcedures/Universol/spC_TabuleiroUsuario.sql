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
		UsuarioIDPag int, 
		Ciclo int,
		Posicao nvarchar(100),
		PagoMaster bit,
		PagoSistema bit,
		InformePagSistema bit,
		TotalRecebimento int, 
		DataInicio datetime,
		DataFim int
	)

	if(@UsuarioID is null or @UsuarioID = 0)
	Begin
		--Obtem os 60 primeiros ativos no Board 1
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
            tab.UsuarioID = tab.MasterID and
            redtab.StatusID <> 2 and (
                redtab.DonatorDirSup1 is null or
                redtab.DonatorDirSup2 is null or
                redtab.DonatorDirInf1 is null or
                redtab.DonatorDirInf2 is null or
                redtab.DonatorEsqSup1 is null or
                redtab.DonatorEsqSup2 is null or
                redtab.DonatorEsqInf1 is null or
                redtab.DonatorEsqInf2 is null
            )
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
    
    --Verifica se Status2 esta ok para enviar convite
	--Declare
	--	@UsuarioIDCur int,
 --       @BoardIDCur int,
 --       @total int,
	--	@AntFetch int
    
	----Cursor
	--Declare
	--	curRegistro 
	--Cursor Local For
 --       Select 
 --           UsuarioID,
 --           BoardID
	--    From
	--	    #temp
	--    Where
 --           StatusID = 2 and
 --           BoardID > 1

	--Open curRegistro
	--Fetch Next From curRegistro Into  @UsuarioIDCur, @BoardIDCur
	--Select @AntFetch = @@fetch_status
	--While @AntFetch = 0
	--Begin
 --       --Verifica se ele é master no board anterior


 --       --Verifica se ha 4 pagamentos mesmo, par liberar o convite
 --       Select 
 --           @total = Count(*)
 --       From
 --           Rede.TabuleiroUsuario
 --       Where
 --           MasterID = @UsuarioIDCur and
 --           BoardID = @BoardIDCur - 1 and
 --           UsuarioID <> @UsuarioIDCur and
 --           PagoMaster = 'true' and
 --           Posicao like 'Donator%'

 --       --Caso Tenha Fechado algum lado, este não entram no count do select acima
	--	--Daí soma 4 no @total, pois já fechou um lado
	--	if Exists (
	--		Select 
	--			'OK' 
	--			From 
	--				Rede.TabuleiroUsuario (nolock)
	--			Where
	--				MasterID = @UsuarioIDCur and
	--				BoardID = @BoardIDCur - 1 and
	--				DireitaFechada = 'true'
	--		)
	--	Begin
 --       	Set @total = @total + 4
	--	End

	--	if Exists (
	--		Select 
	--			'OK' 
	--			From 
	--				Rede.TabuleiroUsuario (nolock)
	--			Where
	--				MasterID = @UsuarioIDCur and
	--				BoardID = @BoardIDCur - 1 and
	--				EsquerdaFechada = 'true'
	--		)
	--	Begin
 --       	Set @total = @total + 4
	--	End
        
 --       if(@total < 4)
 --       Begin
 --           --Se for menor que 4, não envia convite para entrar na proxima galaxia
 --           Update
 --               #temp
 --           Set
 --               StatusID = 0
 --           Where
 --               UsuarioID = @UsuarioIDCur and
 --               BoardID = @BoardIDCur
 --       End
            
	--	--Proxima linha do cursor
	--	Fetch Next From curRegistro Into @UsuarioIDCur, @BoardIDCur
	--	Select @AntFetch = @@fetch_status       
	--End -- While
      
	--Close curRegistro
	--Deallocate curRegistro

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

--Exec spC_TabuleiroUsuario @UsuarioID=null

Exec spC_TabuleiroUsuario @UsuarioID=2599


