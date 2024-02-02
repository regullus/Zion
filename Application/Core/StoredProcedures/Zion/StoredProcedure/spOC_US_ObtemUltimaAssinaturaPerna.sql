
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemUltimaAssinaturaPerna'))
   Drop Procedure spOC_US_ObtemUltimaAssinaturaPerna
go

Create PROCEDURE [dbo].[spOC_US_ObtemUltimaAssinaturaPerna]
   @UsuarioID   int,
   @Perna       CHAR(1),
   @RedeBinaria int = 0

AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 23/01/2018
-- Description: Obtem a ultima assinatura da perna
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   Declare @Carac CHAR(1) = '0';
   If (@RedeBinaria = 1)
   Begin
      Set @Carac = @Perna;
   End
     
   Declare 
      @Assinatura NVARCHAR(max) = ISNULL((Select Assinatura from Usuario.Usuario (nolock) where ID = @UsuarioID) , '');
   Declare 
      @AssSufixo  NVARCHAR(max) = REPLICATE(@Carac, 7999 - Len(@Assinatura));
    
   Select top (1) 
      U.Assinatura 
   From 
      Usuario.Usuario U (nolock) 
   Where U.NivelAssociacao > 0 
     and Len(Assinatura) > 0
     and @Assinatura + @Perna + @AssSufixo like U.Assinatura + '%'
   Order by 
      U.Assinatura desc;

END  

go
   Grant Exec on spOC_US_ObtemUltimaAssinaturaPerna To public
go

-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '0' , 0
-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '1' , 0
-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '2' , 0
-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '3' , 0

-- Rede Binaria 
-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '0' , 1 
-- Exec spOC_US_ObtemUltimaAssinaturaPerna 1000 , '1' , 1