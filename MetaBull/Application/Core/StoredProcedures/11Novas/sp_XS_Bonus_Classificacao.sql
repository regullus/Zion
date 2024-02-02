SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_XS_Bonus_Classificacao]
@baseDate varchar(8) null
AS
BEGIN
	BEGIN TRY
		Set NoCount On

		BEGIN TRANSACTION
		  
		DECLARE @dataInicio datetime;
		DECLARE @dataFim    datetime;
		DECLARE @CategoriaBonusID int, @CategoriaRegraID int ;
		declare @cotacaoBTCPadrao float;
		  
		set @CategoriaBonusID = 23; -- 23 - Bonus Rap de Vendas
		set @CategoriaRegraID = 2 -- Regra de Bônus Rap de Vendas
      
		if (@baseDate is null)
		Begin 
			SET @dataInicio = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2);
			SET @dataFim    = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);
		End
		Else
		Begin
			SET @dataInicio = CAST(@baseDate + ' 00:00:00' as datetime2);
			SET @dataFim    = CAST(@baseDate + ' 23:59:59' as datetime2);
		End

		/*cotacao btc "media" */
		set @cotacaoBTCPadrao = (select avg (cotacao) from (select top 20 CotacaoBTC cotacao from Loja.PedidoPagamento where CotacaoBTC is not null order by ID desc) C) 

		Insert Into Rede.Bonificacao
			(CategoriaID,
			UsuarioID,
			ReferenciaID,
			StatusID,
			Data,
			Valor,
			ValorCripto,
			PedidoID)
		Select
			@CategoriaBonusID as CategoriaID, 
			Usr.ID as Usuario,
			Usr.NivelClassificacao as Referencia,
			0 as StatusID,
			dbo.GetDateZion() as Data,
			Regra.Regra as Valor,
			Round(Convert(Decimal(18,4), Regra.Regra) / @cotacaoBTCPadrao, 4) as ValorCripto,
			Null as PedidoID
		From Usuario.Usuario Usr (Nolock)
		Inner JOin Usuario.UsuarioClassificacao Clf (Nolock) On Clf.UsuarioID = Usr.ID
		Inner Join Rede.RegraItem Regra (Nolock) On Regra.ClassificacaoNivel = Clf.NivelClassificacao
		Left Join Rede.Bonificacao Bonus (Nolock) On Bonus.UsuarioID = Usr.ID
												And Bonus.ReferenciaID = Usr.NivelClassificacao
		Where Regra.RegraID = @CategoriaRegraID
		 And Bonus.UsuarioID Is Null

	COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
	  
		ROLLBACK TRANSACTION
	  
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		RAISERROR ('Erro na execucao de sp_XS_Bonus_Classificacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END
GO