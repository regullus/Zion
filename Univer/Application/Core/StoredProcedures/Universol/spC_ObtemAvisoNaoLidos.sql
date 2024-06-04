Use univerDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_ObtemAvisoNaoLidos'))
   Drop Procedure spC_ObtemAvisoNaoLidos
go

Create PROCEDURE [dbo].[spC_ObtemAvisoNaoLidos]
   @UsuarioID   int

AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 16/02/2018
-- Description: Obtem os Avisos n�o lidos do usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   Declare
      @dataInicio   datetime2 = CAST(CONVERT(VARCHAR(8), getdate()-1, 112) + ' 00:00:00' as datetime2),
      @dataFim      datetime2 = CAST(CONVERT(VARCHAR(8), getdate()-1, 112) + ' 23:59:59' as datetime2),
	  @AssociacaoID int       = (Select NivelAssociacao From Usuario.Usuario (nolock) Where ID = @UsuarioID);

   -- Tabela tempor�ria com os pedidos dos usu�rios que pagaram a associa��o
   Create Table #TAvisos
   (	     
      ID int,
	  Titulo nvarchar(255), 
      Texto nvarchar(max),	 	  
      Urgente bit,
	  DataInicio datetime
   );

   -- Sele��o dos Avisos Gerais
   Insert Into #TAvisos 
   Select 
      A.ID,
      A.Titulo,
      A.Texto,
	  A.Urgente,
	  A.DataInicio
   From Usuario.Aviso A (nolock)
   Where (A.UsuarioIDs    is null or Ltrim(Rtrim(A.UsuarioIDs)) = '')
     and (A.AssociacaoIDs is null or Ltrim(Rtrim(A.AssociacaoIDs)) = '')
     and A.DataInicio <= @dataInicio
     and Coalesce(A.DataFim , @dataFim) >= @dataFim
	 and A.Bloqueado = 0;
      
   -- Sele��o dos Avisos especificos do usuario
   Insert Into #TAvisos 
   Select 
      A.ID,
      A.Titulo,
      A.Texto,
	  A.Urgente,
	  A.DataInicio
   From Usuario.Aviso A (nolock)
   Where A.UsuarioIDs like '%,' + CONVERT(VARCHAR(20), @UsuarioID) + ',%'
     and A.DataInicio <= @dataInicio
     and Coalesce(A.DataFim , @dataFim) >= @dataFim
	 and A.Bloqueado = 0;

   -- Sele��o dos Avisos especificos do tipo de Associa��o
   Insert Into #TAvisos 
   Select 
      A.ID,
      A.Titulo,
      A.Texto,
	  A.Urgente,
	  A.DataInicio
   From Usuario.Aviso A (nolock)
   Where A.AssociacaoIDs like '%,' + CONVERT(VARCHAR(20), @AssociacaoID) + ',%'
     and A.DataInicio <= @dataInicio
     and Coalesce(A.DataFim , @dataFim) >= @dataFim
	 and A.Bloqueado = 0;

   -- Elimina os Avisos que j� foram lidos
   Delete 
      #TAvisos
   From 
      #TAvisos A,
	  Usuario.AvisoLido L (nolock)
   Where A.ID = L.AvisoID

   -- Remove o texto dos avisos n�o urgentes
   Update
      #TAvisos
   Set 
      Texto = ''
   Where 
      Urgente = 0;

   -- Retorna os dados
   Select Distinct 
      A.ID,
      A.Titulo,
      A.Texto,
	  A.Urgente,
	  A.DataInicio
   From
      #TAvisos A
   Order by
      A.DataInicio, A.ID

   -- Remove todas as tabelas tempor�rias
   Drop Table #TAvisos;
END  

go
   Grant Exec on spC_ObtemAvisoNaoLidos To public
go

Exec spC_ObtemAvisoNaoLidos @UsuarioID=2580


