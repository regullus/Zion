use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemMaximaPontuacao'))
   Drop Procedure spOC_US_ObtemMaximaPontuacao
go

Create Proc [dbo].spOC_US_ObtemMaximaPontuacao
   @idUsuario int   ,
   @tipoPonto nvarchar(2),
   @TipoCiclo nvarchar(10) = null

As
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 11/02/2019
-- Description: Obtem os maiores pontos dos ciclos
--
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF

	Set NOCOUNT ON

	-- Verifica Ciclo	
	--Declare 
	--   @idCiclo int = ISNULL( (Select top(1) ID from Rede.Ciclo (nolock) where Ativo = 1) , 0);

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

go
Grant Exec on spOC_US_ObtemMaximaPontuacao To public
go

--Exec spOC_US_ObtemPontuacaoCiclo 1000 , 'VQ', 'ANTERIOR';
--Exec spOC_US_ObtemPontuacaoCiclo 1000 , 'VT';
--Exec spOC_US_ObtemPontuacaoCiclo 1000 , 1
---Select * from Usuario.Pontos



