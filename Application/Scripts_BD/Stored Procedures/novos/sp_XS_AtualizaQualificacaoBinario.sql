
IF OBJECT_ID('dbo.sp_XS_AtualizaQualificacaoBinario', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_AtualizaQualificacaoBinario];
GO

CREATE PROCEDURE sp_XS_AtualizaQualificacaoBinario (@usuarioID INT)
AS

	BEGIN TRY

		BEGIN TRANSACTION
		/*com base em um usuario, atualiza a qualificacao de seu patrocinador ao binario (um direto de cada lado)*/
		
		declare @idCiclo int
		set @idCiclo = (select ID from Rede.Ciclo where Ativo = 1)

		declare @idPai int, @lado int, @qualificador int
		select @idPai = P.ID , -- U.Assinatura, U.ID, P.ID as IDPai ,
		@lado = substring(U.Assinatura, LEN(P.Assinatura)+1  ,1)
		from Usuario.Usuario U inner join Usuario.Usuario P on U.PatrocinadorDiretoID = P.ID
		where U.ID = @usuarioID

		if(@lado = 1)
		begin
			set @qualificador = (select QualificadorDireitaID from Usuario.Qualificacao where UsuarioID = @idPai)
			if(@qualificador is null)
			begin
				if exists(select 1 from Usuario.Qualificacao where UsuarioID = @idPai)
				begin
					update Usuario.Qualificacao set QualificadorDireitaID = @usuarioID, DataQualificacao = GETDATE() where UsuarioID = @idPai
				end
				else
				begin
					insert into Usuario.Qualificacao (UsuarioID, QualificadorEsquerdaID, QualificadorDireitaID, DataQualificacao) 
					values (@idPai, null, @usuarioID, getdate())
				end
			end	
		end
		else
		begin
			set @qualificador = (select QualificadorEsquerdaID from Usuario.Qualificacao where UsuarioID = @idPai)
			if(@qualificador is null)
			begin
				if exists(select 1 from Usuario.Qualificacao where UsuarioID = @idPai)
				begin
					update Usuario.Qualificacao set QualificadorEsquerdaID = @usuarioID, DataQualificacao = GETDATE() where UsuarioID = @idPai
				end
				else
				begin
					insert into Usuario.Qualificacao (UsuarioID, QualificadorEsquerdaID, QualificadorDireitaID, DataQualificacao) 
					values (@idPai, @usuarioID, null, getdate())
				end
			end	
		end

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_XS_AtualizaQualificacaoBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 