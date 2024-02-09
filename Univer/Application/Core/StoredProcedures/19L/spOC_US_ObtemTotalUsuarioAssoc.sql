use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemTotalUsuarioAssoc'))
   Drop Procedure spOC_US_ObtemTotalUsuarioAssoc
go

Create Proc [dbo].spOC_US_ObtemTotalUsuarioAssoc
   
As
-- =============================================================================================
-- Author.....: Adamastor
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

go
Grant Exec on spOC_US_ObtemTotalUsuarioAssoc To public
go

