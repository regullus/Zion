
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.sp_XS_BitcoinSafeDecimais', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_XS_BitcoinSafeDecimais];
GO


CREATE PROCEDURE [dbo].[sp_XS_BitcoinSafeDecimais]
	@valorBruto float,
	@pedidoID int = null,
	@bonusID int = null,
	@valorLiquido float output
	AS
	BEGIN
	
		declare @diferenca float
	
	   BEGIN TRY
		
			set @valorLiquido  = round(@valorBruto, 4,1)
			
			if @valorLiquido > 0
			begin
				set @diferenca = @valorBruto - @valorLiquido 
				if @diferenca > 0
				begin
					insert into Financeiro.BitCoinSafe 
					values (@pedidoID, @bonusID, @valorBruto, @valorLiquido, @diferenca, getdate())
					
					select @valorLiquido
				end
			end
			else
			begin
				set @valorLiquido = @valorBruto
			end
		   
	   END TRY
	   

   BEGIN CATCH
	  If @@Trancount > 0
		 ROLLBACK TRANSACTION
	  
	  DECLARE @error int, @message varchar(4000), @xstate int;
	  SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
	  RAISERROR ('Erro na execucao de sp_XS_BitcoinSafeDecimais: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 


GO
