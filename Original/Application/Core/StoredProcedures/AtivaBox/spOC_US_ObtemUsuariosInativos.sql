Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemUsuariosInativos'))
   Drop Procedure spOC_US_ObtemUsuariosInativos
go

-- =============================================================================================
-- Author.....:
-- Create date: 
-- Description: Gera registros de Bonificacao de indicação e bonus amizade
-- =============================================================================================

Create PROCEDURE [dbo].[spOC_US_ObtemUsuariosInativos]
   @UsuarioID INT,
   @QtdeNiveis int = 7

AS
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on
    
   Declare @AssSufixoFim NVARCHAR(30) = REPLICATE('9', @QtdeNiveis);
   Declare @Assinatura NVARCHAR(max)  = ISNULL((Select Assinatura from Usuario.Usuario (nolock) where ID = @UsuarioID) , '');
   Declare @DiasPerdaPosicao INT      = ISNULL((Select convert(int, Dados) From Sistema.Configuracao (nolock) Where Chave = 'DIAS_PERDA_POSICAO_INATIVIDADE') , 0);

   -- Tabela temporária com os pedidos dos usuários que pagaram a associação
   Create Table #Inativos
   (	     
     UsuarioID INT,	
     Login NVARCHAR(100),	 	  
     DataAtivacao DATETIME,
     DataValidade DATETIME,
	  DataRenovacao DATETIME,
	  PatrocinadorID INT,
	  Patrocinador NVARCHAR(100),
	  Status NVARCHAR(100),
	  Email NVARCHAR(100),
	  NivelAssociacao INT,
	  Associacao NVARCHAR(100),
	  Nivel INT
   );

   INSERT iNTO #Inativos 
   Select 
      U.ID , 
      U.Login, 
	   U.DataAtivacao,
	   U.DataValidade,
	   U.DataRenovacao,
      U.PatrocinadorDiretoID,  
	   '',
	   'INATIVO',
	   U.Email,
      U.NivelAssociacao, 
	   '',
      LEN(U.Assinatura) - LEN(@Assinatura)
   From Usuario.Usuario U (nolock)
   Where U.Assinatura BETWEEN @Assinatura + '0' 
                          AND @Assinatura + @AssSufixoFim
     and U.DataValidade < dbo.GetDateZion();

   -- Obtem o Patrocinador
   Update #Inativos Set Patrocinador = U.Login 
   From #Inativos TI,
        Usuario.Usuario U (nolock)
   Where U.ID = TI.PatrocinadorID;

   -- Obtem a Associacao
   Update #Inativos Set Associacao = A.Nome 
   From #Inativos TI,
        Rede.Associacao A (nolock)
   Where A.Nivel = TI.NivelAssociacao;

   IF(@DiasPerdaPosicao > 0)
   Begin
      Update #Inativos Set Status = 'CANCELADO' 
      From #Inativos TI
      Where TI.DataValidade < DATEADD(day, -@DiasPerdaPosicao, dbo.GetDateZion());
   End

   -- Retorna os usuarios indativos
   Select * 
   From #Inativos 
   Order by Nivel, Login;

   -- Remove todas as tabelas temporárias
   Drop Table #Inativos;
END  

go
   Grant Exec on spDG_RE_GeraBonusIndicacao To public
go

-- Exec spOC_US_ObtemUsuariosInativos 1000
-- Exec spOC_US_ObtemUsuariosInativos 2010