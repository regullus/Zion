use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_ObtemPontosUsuarioCiclo'))
   Drop Procedure spOC_ObtemPontosUsuarioCiclo
go

CREATE Proc [dbo].[spOC_ObtemPontosUsuarioCiclo]
   @idUsuario int,
   @idCiclo   int

As
-- =============================================================================================
-- Author.....: 
-- Create date: 08/02/2019
-- Description: 
-- =============================================================================================
BEGIN
    Set nocount on

    Create Table #TUsuario 
        (Id INT);
	    
    Insert into #TUsuario Values (@idUsuario);

	-- Obtem a rede UniLevel
	Declare 
	   @IDAfiliado int;
  
    Declare 
        curRegistro 
    Cursor Local For
	    Select * from #TUsuario;
   
    Open curRegistro;
    Fetch Next From curRegistro Into @IDAfiliado

    While @@fetch_status = 0
    Begin
	    Insert into #TUsuario 
		Select U.ID 
		from  Usuario.Usuario (nolock) U 
        where U.PatrocinadorDiretoID = @IDAfiliado 
          and U.ID <> @IDAfiliado
          and U.StatusID = 2 --Ativos

	    Fetch Next From curRegistro Into @IDAfiliado                     
    End 

    Close curRegistro;
    Deallocate curRegistro;

	-- Retorno
	Select 
	    B.Data      as Data,
		P.PedidoID  as PedidoID, 
		C.Nome      as BonusNome,
		U.Login     as Login    , 
		P.VPQ       as VT, 
		B.Valor     as VQ,  
		B.Descricao as Descricao
    From 
        Rede.Pontos          P (nolock),
	    Rede.Bonificacao     B (nolock),
	    Usuario.Usuario      U (nolock),
	    Financeiro.Categoria C (nolock),
		#TUsuario            T
    Where
	    P.UsuarioID = @idUsuario
    and P.CicloID   = @idCiclo 
	and P.ReferenciaID = T.Id
    and P.PedidoID     = B.PedidoID
    and P.CicloID      = B.CicloID
    and P.UsuarioID    = B.UsuarioID
    and P.ReferenciaID = U.ID
    and B.CategoriaID  = C.ID
	Order By 
	    B.Data desc;

    Drop Table #TUsuario; 

END;

go
Grant Exec on spOC_ObtemPontosUsuarioCiclo To public
go


--Exec spOC_ObtemPontosUsuarioCiclo 1003 , 1