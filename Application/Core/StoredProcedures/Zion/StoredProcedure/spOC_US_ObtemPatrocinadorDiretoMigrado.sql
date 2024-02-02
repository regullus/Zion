
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemPatrocinadorDiretoMigrado'))
   Drop Procedure spOC_US_ObtemPatrocinadorDiretoMigrado
go

Create PROCEDURE [dbo].[spOC_US_ObtemPatrocinadorDiretoMigrado]
   @UsuarioID   int
 
AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 12/02/2020
-- Description: Obtem o primeiro patrocinado direto que fez a migração
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF;
   Set nocount on;

   Declare 
      @Achou                 int = 0,
      @Count                 int = 0,
      @DataMigracao          datetime = null,
	  @PatrPesquisaID        int = 0,
      @PatrocinadorDiretoID  int = IsNull((Select PatrocinadorDiretoID 
	                                 From Usuario.UsuarioAssinatura (nolock) 
									 Where ID = @UsuarioID) , 0);
     
   If (@PatrocinadorDiretoID > 0) 
   Begin
      While @Achou = 0
      Begin
         Select 
	        @PatrPesquisaID = A.PatrocinadorDiretoID,
			@DataMigracao   = U.DataMigracao
         From 
	        Usuario.UsuarioAssinatura A (nolock),
	        Usuario.Usuario           U (nolock)         
         Where U.ID = @PatrocinadorDiretoID 
	       and A.ID = U.ID;
		  
         If(@DataMigracao is null)
		 Begin
		 	Set @PatrocinadorDiretoID = @PatrPesquisaID;			
		 End
		 Else
		 Begin
		     Set @Achou = 1;		
		 End

		 set @Count = @Count + 1;
		 If (@Count > 999)
		 Begin
		    Set @Achou = 1;
			Set @PatrocinadorDiretoID = 0;			
		 End		
      End
   End

   Select 
      @PatrocinadorDiretoID;

END  

go
   Grant Exec on spOC_US_ObtemPatrocinadorDiretoMigrado To public
go

-- Exec spOC_US_ObtemPatrocinadorDiretoMigrado 2575  



