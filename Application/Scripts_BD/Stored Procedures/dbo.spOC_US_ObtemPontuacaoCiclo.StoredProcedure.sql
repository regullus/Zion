USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemPontuacaoCiclo]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create Proc [dbo].[spOC_US_ObtemPontuacaoCiclo]
   @idUsuario int   ,
   @tipoPonto nvarchar(2),
   @TipoCiclo nvarchar(10) = null

As
-- =============================================================================================
-- Author.....: Edemar
-- Create date: 28/01/2019
-- Description: Obtem os pontos do usuario no ciclo
--
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF

	Set NOCOUNT ON

	-- Verifica Ciclo	
	Declare 
	   @idCiclo int = ISNULL( (Select top(1) ID from Rede.Ciclo (nolock) where Ativo = 1) , 0);

	if (Upper(@TipoCiclo) = 'ANTERIOR' )	
	Begin
	   set @idCiclo = ISNULL( ( Select top(1) ID 
	                            from Rede.Ciclo (nolock) 
	                            Where DataFinal < (Select top(1) DataInicial from Rede.Ciclo (nolock) where ID = @idCiclo)
								  and ID != @idCiclo
	                            order by DataFinal
							  ) , 0 );
	End
		

    -- Obtem Pontos
	Declare 
	   @retorno int = 0;
	
    Select 
       @retorno = CASE		   	
                      WHEN Upper(@tipoPonto) = 'VQ'  THEN P.VQ
				      WHEN Upper(@tipoPonto) = 'VT'  THEN P.VT                              
                      ELSE 0
                  END 	  
	From 
	   Usuario.Pontos P (nolock)
    Where 
	        UsuarioID = @idUsuario
        and CicloID   = @idCiclo;
	
	
	Select @retorno;
      
END -- Sp


GO
