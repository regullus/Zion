USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemTotalUsuarioAssoc]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create Proc [dbo].[spOC_US_ObtemTotalUsuarioAssoc]
   
As
-- =============================================================================================
-- Author.....: Edemar
-- Create date: 13/02/2019
-- Description: Obtem o total de usuarios por nivel de associacao
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON
	
	Select 
        Max(A.Nome) as Name,
        COUNT(U.id) as Value
    From 
        Usuario.Usuario U (nolock) ,
        Rede.Associacao A (nolock) 
    Where 
        U.NivelAssociacao = A.Nivel
    Group By
        A.Nivel;
		       
END; -- Sp


GO
