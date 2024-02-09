SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[sp_ValidacaoCadastro_Insert] 

@ID int,
@Ok bit

AS

IF(NOT EXISTS(SELECT ID FROM Usuario.ValidacaoCadastro WHERE ID = @ID))
	BEGIN
		INSERT INTO Usuario.ValidacaoCadastro(ID, Ok, Data) VALUES (@ID, @Ok, dbo.GetDateZion())
	END

SELECT Ok FROM usuario.ValidacaoCadastro WHERE ID = @ID
