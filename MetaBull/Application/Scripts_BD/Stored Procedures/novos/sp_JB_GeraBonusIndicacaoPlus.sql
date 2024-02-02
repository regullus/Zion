
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.sp_JB_GeraBonusIndicacaoPlus', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_GeraBonusIndicacaoPlus];
GO


CREATE PROCEDURE [dbo].[sp_JB_GeraBonusIndicacaoPlus]
	(@usuarioID INT, @nivelAssociacao INT)
	AS
	BEGIN
	   BEGIN TRY
		  Set NoCount On
		  
		  declare @patrocinadorID int, @patrocinadorIsPlus bit
		  DECLARE @CategoriaBonusID int, @percentualBonusPlus float
		  DECLARE @UsuarioIDRecebeBonusPlus int
		  declare @valorBonus float
		  
		  set @CategoriaBonusID = 24; -- Bônus de Indicação Plus
		  set @percentualBonusPlus = 4;

		  
		  IF( NOT EXISTS( SELECT 1 from Usuario.Usuario where ID = @usuarioID
		    AND Bloqueado = 0     -- Patrocinador nao esta bloqueado
			AND RecebeBonus = 1 -- Patrocinador recebe bonus
			AND DataValidade >= getdate() --so recebe se estiver com ativo mensal pago							
			And StatusID = 2 
			And NivelAssociacao > 0
			and DataRenovacao >= getdate()))
		  BEGIN
				print 'Usuario não habilitado para receber o bonus'
				RETURN
		  END
		  
      
		set @valorBonus = (select isnull(PV.Bonificacao,0) from Loja.Produto P inner join Loja.ProdutoValor PV on P.ID = PV.ProdutoID where SKU like 'ADE%' and NivelAssociacao = 1)
		set @valorBonus = (@valorBonus * @percentualBonusPlus) / 100
		set @patrocinadorID = (select PatrocinadorDiretoID from Usuario.Usuario where ID = @usuarioID)
		select @UsuarioIDRecebeBonusPlus = UsuarioPlusID, @patrocinadorIsPlus = IsPlus  from Rede.BonificacaoPlus where UsuarioID = @patrocinadorID 
		
		if(@patrocinadorIsPlus = 1)
		begin
			set @UsuarioIDRecebeBonusPlus = @patrocinadorID
		end
		
		if(@UsuarioIDRecebeBonusPlus is null)
		begin
			print 'nenhum usuario receberá este plus'
			return
		end
		
		IF(EXISTS( select 1 from Rede.Bonificacao where CategoriaID = @CategoriaBonusID and UsuarioID = @UsuarioIDRecebeBonusPlus and ReferenciaID = @usuarioID ))
		BEGIN
			print 'Este usuario já gerou o bonus plus correspondente'
		RETURN
		END
			
		BEGIN TRANSACTION
		
			Insert Into Rede.Bonificacao
			  (CategoriaID,
			   UsuarioID,
			   ReferenciaID,
			   StatusID,
			   Data,
			   Valor)
			VALUES(
				@CategoriaBonusID,
				@UsuarioIDRecebeBonusPlus,
				@usuarioID,
				0,
				getdate(),
				@valorBonus
			)
			
		COMMIT TRANSACTION
		
	   END TRY

	   BEGIN CATCH
		  If @@Trancount > 0
			 ROLLBACK TRANSACTION
	      
		  DECLARE @error int, @message varchar(4000), @xstate int;
		  SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		  RAISERROR ('Erro na execucao de sp_JB_GeraBonusIndicacaoPlus: %d: %s', 16, 1, @error, @message) WITH SETERROR;
	   END CATCH
	END 


GO
