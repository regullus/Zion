
IF OBJECT_ID('dbo.sp_JB_AtualizaQualificacaoPlus', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_AtualizaQualificacaoPlus];
GO

CREATE PROCEDURE sp_JB_AtualizaQualificacaoPlus (@usuarioID INT, @nivelAssociacao INT)
AS

	BEGIN TRY

		BEGIN TRANSACTION
		/*com base em um usuario, atualiza a qualificacao para bonus Plus (a partir de seu 3 direto) */
		
		declare @idCiclo int
		set @idCiclo = (select ID from Rede.Ciclo where Ativo = 1)
		
		declare @patrocinadorID int
		
		set @patrocinadorID = (select PatrocinadorDiretoID from Usuario.Usuario where ID = @usuarioID)

		IF (NOT EXISTS(SELECT 1 FROM Rede.BonificacaoPlus where UsuarioID = @usuarioID))
		BEGIN
			insert into Rede.BonificacaoPlus 
			select @usuarioID, 0, RBP.UsuarioPlusID, getdate()
			from Rede.BonificacaoPlus RBP
			where UsuarioID = @patrocinadorID
		END
		
		
		declare @qtde_diretos int
		
		/*se o patrocinador possui mais do que 2 diretos, ele é plus*/
		set @qtde_diretos = ( select count(*) from Usuario.Usuario where PatrocinadorDiretoID = @patrocinadorID and StatusID = 2 and NivelAssociacao > 0)
		if (@qtde_diretos > 2)
		begin
			update Rede.BonificacaoPlus set IsPlus = 1 where UsuarioID = @patrocinadorID
			update Rede.BonificacaoPlus set UsuarioPlusID = @patrocinadorID where UsuarioID = @usuarioID
		end
		
		
		/*gera bonus plus*/
		exec sp_JB_GeraBonusIndicacaoPlus @usuarioID, @nivelAssociacao
		

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_JB_AtualizaQualificacaoPlus: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	END CATCH 