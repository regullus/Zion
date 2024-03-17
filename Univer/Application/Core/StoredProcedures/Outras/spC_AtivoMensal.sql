use ClubeVantagens
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_AtivoMensal'))
   Drop Procedure spC_AtivoMensal
Go

--delete [Usuario].[Afiliados]
go


Create  Proc [dbo].[spC_AtivoMensal]
@id int

As
Begin
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set NOCOUNT ON

CREATE TABLE #Retorno(
   [ID] [int] NOT NULL,
   [Login] [nvarchar](50) NOT NULL,
   [Nome] [nvarchar](100) NOT NULL,
   [DataValidade] [datetime] NULL,
   [Status] [int] NOT NULL,
   [Lado] char(2) NOT NULL
) 
   
   declare @Data datetime
   Select @Data = GetDate()

   If Not Exists (Select 'dados' From Usuario.Afiliados where id = @id)
   Begin
      Exec spC_ObtemDiretos @id,26,1,6,-1
   End

   --Usuario Principal
   Insert into #Retorno
   Select 
      ID,
	   Login Login,
      Nome Nome,
      DataValidade DataValidade,
      Case When (DataValidade >= @Data) Then 1 Else 0 End [Status],
      'US' Lado
   From
      Usuario.Usuario
   Where
      ID = @id

   if Exists (
      Select Top 1
         ID
      From
         Usuario.Afiliados
      Where
         ID = @id And
         Nivel = 1 and
         DerramamentoID = 1 and
         DataValidade >= @Data
      Order by 
         IDAfiliado   
   )
   Begin
     --Primeiro da direita
      Insert into #Retorno
      Select Top 1
         IDAfiliado,
	      Login Login,
         Nome Nome,
         DataValidade DataValidade,
         Case When (DataValidade >= @Data) Then 1 Else 0 End [Status],
         'LE'
      From
         Usuario.Afiliados
      Where
         ID = @id And
         Nivel = 1 and
         DerramamentoID = 1 and
         DataValidade >= @Data
      Order by 
         IDAfiliado
   End
   Else
   Begin
      Insert into #Retorno values (0,' ', '', null,0,'LE')
   End

   if Exists (
      Select Top 1
         ID
      From
         Usuario.Afiliados
      Where
         ID = @id And
         Nivel = 1 and
         DerramamentoID = 2 and
         DataValidade >= @Data
      Order by 
         IDAfiliado   
   )
   Begin
     --Primeiro da direita
      Insert into #Retorno
      Select Top 1
         IDAfiliado,
	      Login Login,
         Nome Nome,
         DataValidade DataValidade,
         Case When (DataValidade >= @Data) Then 1 Else 0 End [Status],
         'LD'
      From
         Usuario.Afiliados
      Where
         ID = @id And
         Nivel = 1 and
         DerramamentoID = 2 and
         DataValidade >= @Data
      Order by 
         IDAfiliado
   End
   Else
   Begin
      Insert into #Retorno values (0,' ', '', null,0,'LD')
   End

   Select 
      [ID],
	   [Login],
      [Nome],
      [DataValidade],
      [Status],
      [Lado]
   From 
      #Retorno

End -- Sp

go
Grant Exec on spC_AtivoMensal To public
go

--Exec spC_AtivoMensal @id=26
--Exec spC_AtivoMensal @id=72
