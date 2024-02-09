-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Verifica qualificacao dos usuários
-- =============================================================================================
DROP PROCEDURE [dbo].[spDG_US_Qualificacao]
GO
CREATE PROCEDURE [dbo].[spDG_US_Qualificacao]
   @UsuarioId	int,
   @PedidoId	int
AS
BEGIN
   BEGIN TRY
      SET NOCOUNT ON
      
	  DECLARE @Kit VarChar(128)
	  DECLARE @DataPedido DateTime
	  DECLARE @VlrBonus Decimal
	  DECLARE @IdPatrocinador Int
	  DECLARE @Assinatura VarChar(Max)
	  
      -- Tabela buscar dado do pedido do usuário
      SELECT @IdPatrocinador = Usr.PatrocinadorDiretoID,
		@Kit = Prd.Nome,
	    @DataPedido = Ped.DataCriacao
	  FROM Loja.Pedido Ped (NOLOCK)
		INNER JOIN Loja.PedidoItem Item (NOLOCK) ON Item.PedidoID = Ped.ID
		INNER JOIN Loja.Produto Prd (NOLOCK) ON Prd.ID = Item.ProdutoID
		INNER JOIN Loja.ProdutoValor Vlr (NOLOCK) ON Vlr.ProdutoID = Prd.ID
		INNER JOIN Usuario.Usuario Usr (NOLOCK) ON Usr.ID = Ped.UsuarioID
	  WHERE Ped.UsuarioID = @UsuarioId
	   AND Ped.ID = @PedidoId;
      
	  -- Buscar a assinatura do Patrocinador
	  SELECT @Assinatura = LTRIM(RTRIM(Assinatura))
	  FROM Usuario.Usuario Usr (NOLOCK)
	  WHERE Usr.ID = @IdPatrocinador

	  IF EXISTS ( SELECT 1
				  FROM Usuario.Qualificacao Qlf (NOLOCK)
				  WHERE Qlf.UsuarioID = @IdPatrocinador )
	  BEGIN
			DECLARE @CicloID INT

			-- Buscar Ciclo Atual Ativo
			SELECT TOP 1 @CicloID = ID
			FROM Rede.Ciclo (NOLOCK)
			WHERE Ativo = 1
			ORDER BY DataFinal

			INSERT INTO Usuario.Qualificacao
			( UsuarioID, CicloID, QualificadorEsquerdaID, QualificadorDireitaID, DataQualificacao, Ultimo )
			SELECT @IdPatrocinador, @CicloID, Null, Null, Null, 1
	  END

	  IF EXISTS ( SELECT 1
				  FROM Usuario.Qualificacao Qlf (NOLOCK)
				  WHERE Qlf.UsuarioID = @IdPatrocinador
				   AND (Qlf.QualificadorEsquerdaID IS NULL OR Qlf.QualificadorDireitaID IS NULL ) )
	  BEGIN

		  -- Checar lado esquerda
		  IF EXISTS ( SELECT 1
					  FROM Usuario.Usuario Usr (NOLOCK)
					  WHERE LTRIM(RTRIM(Usr.Assinatura)) + '0' = LEFT(@Assinatura, LEN(LTRIM(RTRIM(Usr.Assinatura))) + 1)
					   AND Usr.ID = @UsuarioId )
		  BEGIN
				UPDATE Usuario.Qualificacao
				SET QualificadorEsquerdaID = @UsuarioId,
					DataQualificacao = CASE WHEN (QualificadorEsquerdaID IS NOT NULL AND QualificadorDireitaID IS NOT NULL) THEN
						GETDATE() ELSE NULL END
				WHERE UsuarioID = @IdPatrocinador
		  END
		  ELSE
		  BEGIN
				UPDATE Usuario.Qualificacao
				SET QualificadorDireitaID = @UsuarioId,
					DataQualificacao = CASE WHEN (QualificadorEsquerdaID IS NOT NULL AND QualificadorDireitaID IS NOT NULL) THEN
						GETDATE() ELSE NULL END
				WHERE UsuarioID = @IdPatrocinador
		  END

	  END

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION;
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusEquipe: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END