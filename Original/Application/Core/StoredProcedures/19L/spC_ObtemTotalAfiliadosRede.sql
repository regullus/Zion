use [19L]
GO
/****** Object:  StoredProcedure [dbo].[spC_ObtemTotalAfiliadosRede]    Script Date: 30/01/2019 13:45:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spC_ObtemTotalAfiliadosRede]
   @idUsuario int

As
-- =============================================================================================
-- Author.....: Rui barbosa
-- Create date: 30/01/2019
-- Description: Conta quantidade de afiliados na rede do usuário informado
-- =============================================================================================
BEGIN
   Set nocount on

   Declare @IDAfiliado int;
   Declare @AntFetch int = 0;
   Create Table #ProximoNivel (Id INT);
	    
   Insert into #ProximoNivel
  	 Select usu.ID from  usuario.Usuario (nolock) usu
				   where 
			       usu.ID = @idUsuario and
			       usu.StatusID = 2 --Ativos

   Declare 
        curRegistro 
   Cursor Local For
	   Select * from #ProximoNivel
       Open curRegistro
       Fetch Next From curRegistro Into @IDAfiliado

       While @@fetch_status = 0
		  Begin
			Insert into #ProximoNivel Select usu.ID from  usuario.Usuario (nolock) usu 
									  where  usu.PatrocinadorDiretoID = @IDAfiliado 
									  and @IDAfiliado <> usu.ID
									  and usu.StatusID = 2 --Ativos
			Fetch Next From curRegistro Into @IDAfiliado                     
          End 

       Close curRegistro
       Deallocate curRegistro

	   Declare @total int;

	   Select @total = count(*) from #ProximoNivel
	   if (@total = 0)
		  Select count(*) from #ProximoNivel
	   else
		  Select count(*) - 1 from #ProximoNivel

   Drop Table #ProximoNivel; 
END


--exec spC_ObtemTotalAfiliadosRede 1000

