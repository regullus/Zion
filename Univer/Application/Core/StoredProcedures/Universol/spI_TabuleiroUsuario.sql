use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spI_TabuleiroUsuario'))
   Drop Procedure spI_TabuleiroUsuario
go

Create  Proc [dbo].[spI_TabuleiroUsuario]
   @UsuarioID int,
   @MasterID int

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
	
	--Board 1 de 8 - Mercurio
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 1)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
			UsuarioID,
			BoardID,
			TabuleiroID,
			StatusID,
			MasterID,
			InformePag,
			UsuarioIDPag,
			Ciclo,
			Posicao,
			PagoMaster,
			PagoSistema,
			ConviteProximoNivel,
			DireitaFechada,
			EsquerdaFechada,
			DataInicio,
			DataFim,
			Debug
		) 
		Values 
		(
			@UsuarioID, --Ele vira o Master
			1, --Mercurio
			null, --So muda o master quando ele aceitar o convite
			0, --Ativa Depois dele aceitar o convite
			@MasterID,
			0,
			null, --Altera depois dele entrar em um tabuleiroi
			1, --1o ciclo dele, ele acabou de entrar no sistema
			'', --Sem posiçao ainda
			'false', 
			'false',
			'false',
			'false',
			'false',
			GetDate(),
			null,
			'Usuario Iniciado'
		)
	End
	--Board 2 de 8 - Saturno
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 2)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
			UsuarioID,
			BoardID,
			TabuleiroID,
			StatusID,
			MasterID,
			InformePag,
			UsuarioIDPag,
			Ciclo,
			Posicao,
			PagoMaster,
			PagoSistema,
			ConviteProximoNivel,
			DireitaFechada,
			EsquerdaFechada,
			DataInicio,
			DataFim,
			Debug
		) 
		Values 
		(
			@UsuarioID, --Ele vira o Master
			2, --Saturno
			null, --So muda o master quando ele aceitar o convite
			0, --Ativa Depois dele aceitar o convite
			@MasterID,
			0,
			null, --Altera depois dele entrar em um tabuleiroi
			1, --1o ciclo dele, ele acabou de entrar no sistema
			'', --Sem posiçao ainda
			'false', 
			'false',
			'false',
			'false',
			'false',
			GetDate(),
			null,
			'Usuario Iniciado'
		)
	End
    --Board 3 de 8 - Marte
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 3)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
				UsuarioID,
				BoardID,
				TabuleiroID,
				StatusID,
				MasterID,
				InformePag,
				UsuarioIDPag,
				Ciclo,
				Posicao,
				PagoMaster,
				PagoSistema,
				ConviteProximoNivel,
				DireitaFechada,
				EsquerdaFechada,
				DataInicio,
				DataFim,
				Debug
			) 
			Values 
			(
				@UsuarioID, --Ele vira o Master
				3, --Marte
				null, --So muda o master quando ele aceitar o convite
				0, --Ativa Depois dele aceitar o convite
				@MasterID,
				0,
				null, --Altera depois dele entrar em um tabuleiroi
				1, --1o ciclo dele, ele acabou de entrar no sistema
				'', --Sem posiçao ainda
				'false', 
				'false',
				'false',
				'false',
				'false',
				GetDate(),
				null,
				'Usuario Iniciado'
			)
	End
    --Board 4 de 8 - Jupiter
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 4)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
			UsuarioID,
			BoardID,
			TabuleiroID,
			StatusID,
			MasterID,
			InformePag,
			UsuarioIDPag,
			Ciclo,
			Posicao,
			PagoMaster,
			PagoSistema,
			ConviteProximoNivel,
			DireitaFechada,
			EsquerdaFechada,
			DataInicio,
			DataFim,
			Debug
		) 
		Values 
		(
			@UsuarioID, --Ele vira o Master
			3, --Jupiter
			null, --So muda o master quando ele aceitar o convite
			0, --Ativa Depois dele aceitar o convite
			@MasterID,
			0,
			null, --Altera depois dele entrar em um tabuleiroi
			1, --1o ciclo dele, ele acabou de entrar no sistema
			'', --Sem posiçao ainda
			'false', 
			'false',
			'false',
			'false',
			'false',
			GetDate(),
			null,
			'Usuario Iniciado'
		)
	End
    --Board 5 de 8 - Venus
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 5)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
		UsuarioID,
		BoardID,
		TabuleiroID,
		StatusID,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		Posicao,
		PagoMaster,
		PagoSistema,
		ConviteProximoNivel,
		DireitaFechada,
		EsquerdaFechada,
		DataInicio,
		DataFim,
		Debug
	) 
	Values 
	(
		@UsuarioID, --Ele vira o Master
		5, --Venus
		null, --So muda o master quando ele aceitar o convite
		0, --Ativa Depois dele aceitar o convite
		@MasterID,
		0,
		null, --Altera depois dele entrar em um tabuleiroi
		1, --1o ciclo dele, ele acabou de entrar no sistema
		'', --Sem posiçao ainda
		'false', 
		'false',
		'false',
		'false',
		'false',
		GetDate(),
		null,
		'Usuario Iniciado'
	)
	End

    --Board 6 de 8 - Urano
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 6)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
		UsuarioID,
		BoardID,
		TabuleiroID,
		StatusID,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		Posicao,
		PagoMaster,
		PagoSistema,
		ConviteProximoNivel,
		DireitaFechada,
		EsquerdaFechada,
		DataInicio,
		DataFim,
		Debug
	) 
	Values 
	(
		@UsuarioID, --Ele vira o Master
		6, --Urano
		null, --So muda o master quando ele aceitar o convite
		0, --Ativa Depois dele aceitar o convite
		@MasterID,
		0,
		null, --Altera depois dele entrar em um tabuleiroi
		1, --1o ciclo dele, ele acabou de entrar no sistema
		'', --Sem posiçao ainda
		'false', 
		'false',
		'false',
		'false',
		'false',
		GetDate(),
		null,
		'Usuario Iniciado'
	)
	End
    --Board 7 de 8 - Terra
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 7)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
		UsuarioID,
		BoardID,
		TabuleiroID,
		StatusID,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		Posicao,
		PagoMaster,
		PagoSistema,
		ConviteProximoNivel,
		DireitaFechada,
		EsquerdaFechada,
		DataInicio,
		DataFim,
		Debug
	) 
	Values 
	(
		@UsuarioID, --Ele vira o Master
		7, --Terra
		null, --So muda o master quando ele aceitar o convite
		0, --Ativa Depois dele aceitar o convite
		@MasterID,
		0,
		null, --Altera depois dele entrar em um tabuleiroi
		1, --1o ciclo dele, ele acabou de entrar no sistema
		'', --Sem posiçao ainda
		'false', 
		'false',
		'false',
		'false',
		'false',
		GetDate(),
		null,
		'Usuario Iniciado'
	)
	End
	--Board 8 de 8 - Sol
	if Not Exists (Select 'OK' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and BoardID = 8)
	Begin
		INSERT INTO Rede.TabuleiroUsuario (
		UsuarioID,
		BoardID,
		TabuleiroID,
		StatusID,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		Posicao,
		PagoMaster,
		PagoSistema,
		ConviteProximoNivel,
		DireitaFechada,
		EsquerdaFechada,
		DataInicio,
		DataFim,
		Debug
	) 
	Values 
	(
		@UsuarioID, --Ele vira o Master
		8, --Sol
		null, --So muda o master quando ele aceitar o convite
		0, --Ativa Depois dele aceitar o convite
		@MasterID,
		0,
		null, --Altera depois dele entrar em um tabuleiroi
		1, --1o ciclo dele, ele acabou de entrar no sistema
		'', --Sem posiçao ainda
		'false', 
		'false',
		'false',
		'false',
		'false',
		GetDate(),
		null,
		'Usuario Iniciado'
	)
    End

	Select 'OK'
End -- Sp

go
Grant Exec on spI_TabuleiroUsuario To public
go


--Exec spI_TabuleiroUsuario @UsuarioID=2580, @StatusID=2

