Use Zion
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusClassificacao'))
   Drop Procedure spDG_RE_GeraBonusClassificacao
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusClassificacao]
    @DaseDate varchar(8) = null

AS
-- =============================================================================================
-- Author.....: 
-- Create date: 17/08/2020
-- Description: Gera registros de Bonificacao de indicação Direta
-- =============================================================================================
BEGIN
	BEGIN TRY
		SET NOCOUNT ON

        DECLARE 
           @DataInicio  DATETIME = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2),		
           @DataFim     DATETIME = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2),
           @CategoriaID	INT      = 14, -- Bônus de Classificação
	       @RegraID		INT      = 3 ,	
		   @CicloID     INT      = (SELECT ID FROM Rede.Ciclo (NOLOCK) WHERE Ativo = 1);

      
		if (@DaseDate Is Not Null)		
		Begin
			SET @DataInicio = CAST(@DaseDate + ' 00:00:00' as datetime2);
			SET @DataFim    = CAST(@DaseDate + ' 23:59:59' as datetime2);
		End

        BEGIN TRANSACTION

		Insert Into Rede.Bonificacao(	
			CategoriaID,
		    UsuarioID,
			ReferenciaID,
			StatusID,
			Data,
			Valor,
			PedidoID,
			Descricao,
			RegraItemID,
			CicloID,
			ValorCripto
		)
		Select
			@CategoriaID         AS CategoriaID, 
			U.ID                 AS Usuario,
			U.NivelClassificacao AS Referencia,
			0                    AS StatusID,
			dbo.GetDateZion()    AS Data,		
			R.Regra              AS Valor,
			Null                 AS PedidoID,
			''                   AS Descricao,
			R.id  			     AS RegraItemID,
			@CicloID             AS CicloID,
			0                    AS ValorCripto
		From 
           Usuario.Usuario                         U  (NOLOCK)
		   Inner Join Usuario.UsuarioClassificacao UC (NOLOCK) ON UC.UsuarioID = U.ID
		   Inner Join Rede.RegraItem               R  (NOLOCK) ON R.ClassificacaoNivel = UC.NivelClassificacao
		   Left  Join Rede.Bonificacao             B  (NOLOCK) ON B.UsuarioID = U.ID And B.ReferenciaID = U.NivelClassificacao
		Where 
           R.RegraID = @RegraID
		And B.UsuarioID Is Null

	    COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
      IF @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error INT, @message VARCHAR(4000), @xstate INT;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusClassificacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 
 
go
   Grant Exec on spDG_RE_GeraBonusClassificacao To public
go
