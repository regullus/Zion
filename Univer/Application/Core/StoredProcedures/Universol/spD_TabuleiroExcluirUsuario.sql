use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spD_TabuleiroExcluirUsuario'))
   Drop Procedure spD_TabuleiroExcluirUsuario
go

Create Proc [dbo].[spD_TabuleiroExcluirUsuario]
   @UsuarioID int,
   @MasterID int,
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

	Declare
	    @TabuleiroID int,
		@Posicao nvarchar(255),
		@retorno nvarchar(50),
		@Ciclo int

	Set @retorno = 'NOOK'
	
	--Verifica se usuario esta ativo no tabuleiro, se sim pode exclui-lo
	if Exists (
	Select 
		'OK'
	FROM 
		Rede.TabuleiroUsuario (nolock)
	Where
	    UsuarioID = @UsuarioID and
		BoardID = @BoardID and
		PagoMaster = 'false' and
		StatusID = 1 --Ativo
	)
	Begin
	    BEGIN TRY
        BEGIN TRANSACTION

			Select
				@TabuleiroID = TabuleiroID,
				@posicao = Posicao,
				@Ciclo = Ciclo
			From
				Rede.TabuleiroUsuario (nolock)
			Where
				UsuarioID = @UsuarioID and
				BoardID = @BoardID

			if(@Ciclo > 1)
			Begin
				--Dois statusID do usuario 
				Update
					Rede.TabuleiroUsuario
				Set
					StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
					Posicao = '',
					TabuleiroID = null,
					InformePag = 'false',
					UsuarioIDPag = null,
					DataInicio = GetDate(),
					Debug = 'Removido pelo master:' + TRIM(STR(@MasterID))
				Where
					UsuarioID = @UsuarioID and
					BoardID = @BoardID
			End
			Else
			Begin
				if(@BoardID > 1)
				Begin
					 --Dois statusID do usuario 
					Update
						Rede.TabuleiroUsuario
					Set
						StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
						Posicao = '',
						TabuleiroID = null,
						InformePag = 'false',
						UsuarioIDPag = null,
						Debug = 'Removido pelo master:' + TRIM(STR(@MasterID))
					Where
						UsuarioID = @UsuarioID and
						BoardID = @BoardID

				End
				Else
				Begin
					--Zera statusID do usuario 
					Update
						Rede.TabuleiroUsuario
					Set
						StatusID = 0, -- Esta disponivel para entrar em um tabuleiro
						Posicao = '',
						TabuleiroID = null,
						InformePag = 'false',
						UsuarioIDPag = null,
						Debug = 'Removido pelo master:' + TRIM(STR(@MasterID))
					Where
						UsuarioID = @UsuarioID and
						BoardID = @BoardID
				End
			End
		
			--Remove usuario que nao pagou no tabuleiro
			if(@posicao = 'DonatorDirSup1')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID) 
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorDirSup1 = null Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorDirSup2')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorDirSup2 = null Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorDirInf1')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorDirInf1 = null Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorDirInf2')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorDirInf2 = null Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorEsqSup1')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorEsqSup1 = null Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorEsqSup2')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorEsqSup2 = null Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorEsqInf1')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorEsqInf1 = null Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID
				End
			End
			if(@posicao = 'DonatorEsqInf2')
			Begin
				if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID)
				Begin
					Set @retorno = 'OK'
					Update Rede.Tabuleiro Set DonatorEsqInf2 = null Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID
				End
			End

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		--RAISERROR ('Erro na execucao de sp_XS_AtualizaQualificacaoBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
        Select @retorno = 'Error SPDEU: ' + @error + '-' + @message
	END CATCH     

	End
	Else 
	Begin
		Set @retorno = 'Convidado nao esta ativo no tabuleiro'
	End
    Select @retorno

End -- Sp

go
Grant Exec on spD_TabuleiroExcluirUsuario To public
go

--Exec spD_TabuleiroExcluirUsuario @UsuarioID=2590, @TabuleiroID =1

