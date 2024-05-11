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
-- Description: Inclui os 8 registros do usuario que inicia no sistema
-- Observacao: Só deve rodar essa sp no inicio, onde o usuario acaba de aceitar um convite para entrar no sistema
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
	Set FMTONLY OFF
	Set nocount on
	
	Declare @count int

	Select
		@count = Count(*)
	From
		Rede.TabuleiroUsuario 
	Where
		UsuarioID = @UsuarioID and
		BoardID = 1 --Primeiro board que ele entra

    BEGIN TRY
        BEGIN TRANSACTION

		if(@count<=0)
		Begin
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
					InformePagSistema,
					PagoSistema,
					DireitaFechada,
					EsquerdaFechada,
					TotalRecebimento,
					DataInicio,
					DataFim,
					Debug
				) 
				Values 
				(
					@UsuarioID, 
					1, --Mercurio
					null, --So muda o master quando ele aceitar o convite
					0, --Ativa Depois dele aceitar o convite
					@MasterID,
					0,
					null, --Altera depois dele entrar em um tabuleiroi
					0, --Será incrementado depois
					'', --Sem posicao ainda
					'false', 
					'false',
					'false',
					'false',
					'false',
					0,
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
					InformePagSistema,
					PagoSistema,
					DireitaFechada,
					EsquerdaFechada,
					TotalRecebimento,
					DataInicio,
					DataFim,
					Debug
				) 
				Values 
				(
					@UsuarioID,
					2, --Saturno
					null, --So muda o master quando ele aceitar o convite
					0, --Ativa Depois dele aceitar o convite
					@MasterID,
					0,
					null, --Altera depois dele entrar em um tabuleiroi
					0, --Será incrementado depois
					'', --Sem posi�ao ainda
					'false', 
					'false',
					'false',
					'false',
					'false',
					0,
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
						InformePagSistema,
						PagoSistema,
						DireitaFechada,
						EsquerdaFechada,
						TotalRecebimento,
						DataInicio,
						DataFim,
						Debug
					) 
					Values 
					(
						@UsuarioID,
						3, --Marte
						null, --So muda o master quando ele aceitar o convite
						0, --Ativa Depois dele aceitar o convite
						@MasterID,
						0,
						null, --Altera depois dele entrar em um tabuleiroi
						0, --Será incrementado depois
						'', --Sem posi�ao ainda
						'false', 
						'false',
						'false',
						'false',
						'false',
						0,
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
					InformePagSistema,
					PagoSistema,
					DireitaFechada,
					EsquerdaFechada,
					TotalRecebimento,
					DataInicio,
					DataFim,
					Debug
				) 
				Values 
				(
					@UsuarioID,
					4, --Jupiter
					null, --So muda o master quando ele aceitar o convite
					0, --Ativa Depois dele aceitar o convite
					@MasterID,
					0,
					null, --Altera depois dele entrar em um tabuleiroi
					0, --Será incrementado depois
					'', --Sem posi�ao ainda
					'false', 
					'false',
					'false',
					'false',
					'false',
					0,
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
				InformePagSistema,
				PagoSistema,
				DireitaFechada,
				EsquerdaFechada,
				TotalRecebimento,
				DataInicio,
				DataFim,
				Debug
			) 
			Values 
			(
				@UsuarioID,
				5, --Venus
				null, --So muda o master quando ele aceitar o convite
				0, --Ativa Depois dele aceitar o convite
				@MasterID,
				0,
				null, --Altera depois dele entrar em um tabuleiroi
				0, --Será incrementado depois
				'', --Sem posi�ao ainda
				'false', 
				'false',
				'false',
				'false',
				'false',
				0,
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
				InformePagSistema,
				PagoSistema,
				DireitaFechada,
				EsquerdaFechada,
				TotalRecebimento,
				DataInicio,
				DataFim,
				Debug
			) 
			Values 
			(
				@UsuarioID,
				6, --Urano
				null, --So muda o master quando ele aceitar o convite
				0, --Ativa Depois dele aceitar o convite
				@MasterID,
				0,
				null, --Altera depois dele entrar em um tabuleiroi
				0, --Será incrementado depois
				'', --Sem posi�ao ainda
				'false', 
				'false',
				'false',
				'false',
				'false',
				0,
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
				InformePagSistema,
				PagoSistema,
				DireitaFechada,
				EsquerdaFechada,
				TotalRecebimento,
				DataInicio,
				DataFim,
				Debug
			) 
			Values 
			(
				@UsuarioID,
				7, --Terra
				null, --So muda o master quando ele aceitar o convite
				0, --Ativa Depois dele aceitar o convite
				@MasterID,
				0,
				null, --Altera depois dele entrar em um tabuleiroi
				0, --Será incrementado depois
				'', --Sem posi�ao ainda
				'false', 
				'false',
				'false',
				'false',
				'false',
				0,
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
				InformePagSistema,
				PagoSistema,
				DireitaFechada,
				EsquerdaFechada,
				TotalRecebimento,
				DataInicio,
				DataFim,
				Debug
			) 
			Values 
			(
				@UsuarioID,
				8, --Sol
				null, --So muda o master quando ele aceitar o convite
				0, --Ativa Depois dele aceitar o convite
				@MasterID,
				0,
				null, --Altera depois dele entrar em um tabuleiroi
				0, --Será incrementado depois
				'', --Sem posi�ao ainda
				'false', 
				'false',
				'false',
				'false',
				'false',
				0,
				GetDate(),
				null,
				'Usuario Iniciado'
			)
			End
		End

		Update
			Rede.TabuleiroUsuario
		Set
			TabuleiroID = null,
			StatusID = 0,
			MasterID = @MasterID,
			InformePag = 0,
			UsuarioIDPag = null,
			Ciclo = 0,
			Posicao = '',
			PagoMaster = 'false',
			InformePagSistema = 'false',
			PagoSistema = 'false',
			DireitaFechada = 'false',
			EsquerdaFechada = 'false',
			TotalRecebimento = 0,
			DataInicio = GetDate(),
			DataFim = null,
			Debug = 'Usuario Iniciado'
		Where
			UsuarioID = @UsuarioID
			
		Select 'OK'
		COMMIT TRANSACTION
	END TRY
		BEGIN CATCH
		ROLLBACK TRANSACTION
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		Select 'NOOK - ' + 'Error SPGTS: ' + @error + '-' + @message
	END CATCH
End -- Sp

go
Grant Exec on spI_TabuleiroUsuario To public
go


--Exec spI_TabuleiroUsuario @UsuarioID=2580, @StatusID=2

