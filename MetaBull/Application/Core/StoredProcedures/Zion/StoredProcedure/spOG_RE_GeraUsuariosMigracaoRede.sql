 
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOG_RE_GeraUsuariosMigracaoRede'))
   Drop Procedure spOG_RE_GeraUsuariosMigracaoRede
go

Create PROCEDURE [dbo].[spOG_RE_GeraUsuariosMigracaoRede] 
   @UsuarioID int
  
AS  
-- =============================================================================================  
-- Author.....: Adamastor  
-- Create date: 03/02/2020  
-- Description: Gera lista de usuarios diretos para migração para uma nova rede 
-- =============================================================================================  
  
BEGIN  
   BEGIN TRY  
      Declare 
         @Assinatura nvarchar(max) = IsNull((Select Assinatura 
                                             From Usuario.Usuario (nolock) 
		   	                                 Where ID = @UsuarioID) , ' ');
      Declare
         @UsuarioPerna nvarchar(1) = SubString(@Assinatura, Len(@Assinatura), 1 );


   If Exists (Select 'Alteracao' From Rede.RedeMigracao Where PatrocinadorID = @UsuarioID)
   Begin
     Select   
	     M.ID           as  ID, 
         M.UsuarioID    as  UsuarioID,
         U.Login        as  Login,
         U.Nome         as  Nome,
         S.Nome         as  NomeAssociacao ,
         --A.Assinatura   as  Assinatura,
         U.DataAtivacao as  DataAtivacao,
         M.Linha        as Linha,
		 Ordem          as Ordem
      From 
	     Rede.RedeMigracao M (nolock),
         Usuario.Usuario   U (nolock),
		 Rede.Associacao   S (nolock)
	  Where M.PatrocinadorDiretoID = @UsuarioID
        and M.UsuarioID = U.ID
	    and U.NivelAssociacao = S.Nivel
	  Order by 
        linha,
        Ordem; 
   End
   Else
   Begin
      Select   
	     0              as  ID, 
         A.ID           as  UsuarioID,
         U.Login        as  Login,
         U.Nome         as  Nome,
         S.Nome         as  NomeAssociacao ,
         --A.Assinatura   as  Assinatura,
         U.DataAtivacao as  DataAtivacao,
         Case  when SubString(A.Assinatura, Len(A.Assinatura), 1 ) = @UsuarioPerna 
               Then 1 -- Derramamento
		       Else 0 -- Construcao
         End            as Linha,
		 0              as Ordem
      From 
         Usuario.UsuarioAssinatura A (nolock),
         Usuario.Usuario           U (nolock),
         Rede.Associacao           S (nolock)
      Where A.PatrocinadorDiretoID = @UsuarioID
        and A.ID = U.ID
        and U.NivelAssociacao = S.Nivel
		and S.Nivel > 0
        and Len(A.Assinatura) > 0
      Order by 
         linha,
         A.Assinatura; 
   End

   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION;  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spOG_RE_GeraUsuariosMigracaoRede: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END  

go
   Grant Exec on spOG_RE_GeraUsuariosMigracaoRede To public
go 
