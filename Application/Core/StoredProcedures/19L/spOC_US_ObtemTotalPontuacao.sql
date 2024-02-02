use [19L]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemTotalPontuacao]    Script Date: 15/02/2019 19:45:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER Proc [dbo].[spOC_US_ObtemTotalPontuacao]
   @idUsuario int

As
-- =============================================================================================
-- Author.....: Rui barbosa
-- Create date: 15/02/2019
-- Description: Obtem a pontuação total do usuárop
--
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    -- Obtem Pontos
	Declare 
	   @retorno int = 0;
	
    Select  @retorno = Sum(P.VT)
	From 
	    Usuario.Pontos P (nolock)
    Where 
	    UsuarioID = @idUsuario
	
	Select @retorno;
      
END -- Sp

