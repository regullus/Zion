USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemMaximaPontuacao]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create Proc [dbo].[spOC_US_ObtemMaximaPontuacao]
   @idUsuario int   ,
   @tipoPonto nvarchar(2)

As
-- =============================================================================================
-- Author.....: Edemar
-- Create date: 11/02/2019
-- Description: Obtem os maiores pontos dos ciclos
--
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    -- Obtem Pontos
	Declare 
	   @retorno int = 0;
	
    Select 
       @retorno = CASE		   	
                      WHEN Upper(@tipoPonto) = 'VQ'  THEN Max(P.VQ)
				      WHEN Upper(@tipoPonto) = 'VT'  THEN Max(P.VT)
                      ELSE 0
                  END 	  
	From 
	    Usuario.Pontos P (nolock)
    Where 
	    UsuarioID = @idUsuario
	
	
	Select @retorno;
      
END -- Sp


GO
