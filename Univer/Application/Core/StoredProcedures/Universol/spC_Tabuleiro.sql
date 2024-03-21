use [UniverDEv]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_Tabuleiro'))
   Drop Procedure spC_Tabuleiro
go

Create  Proc [dbo].[spC_Tabuleiro]
   @id int

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem niveis no tabuleito de um usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   Declare @pin int
 
CREATE TABLE #temp
(
   ID int NULL,
   BoardID int NULL,
   StatusID int NULL,
   Master int NULL,
   NomeMaster nvarchar(255) NULL,
   pinMaster int NULL,
   CoordinatorDir int NULL,
   NomeCoordinatorDir nvarchar(255) NULL,
   pinCoordinatorDir int NULL,
   IndicatorDirSup int NULL,
   NomeIndicatorDirSup nvarchar(255) NULL,
   pinIndicatorDirSup int NULL,
   IndicatorDirInf int NULL,
   NomeIndicatorDirInf nvarchar(255) NULL,
   pinIndicatorDirInf int NULL,
   DonatorDirSup1 int NULL,
   NomeDonatorDirSup1 nvarchar(255) NULL,
   pinDonatorDirSup1 int NULL,
   DonatorDirSup2 int NULL,
   NomeDonatorDirSup2 nvarchar(255) NULL,
   pinDonatorDirSup2 int NULL,
   DonatorDirInf1 int NULL,
   NomeDonatorDirInf1 nvarchar(255) NULL,
   pinDonatorDirInf1 int NULL,
   DonatorDirInf2 int NULL,
   NomeDonatorDirInf2 nvarchar(255) NULL,
   pinDonatorDirInf2 int NULL,
   CoordinatorEsq int NULL,
   NomeCoordinatorEsq nvarchar(255) NULL,
   pinCoordinatorEsq int NULL,
   IndicatorEsqSup int NULL,
   NomeIndicatorEsqSup nvarchar(255) NULL,
   pinIndicatorEsqSup int NULL,
   IndicatorEsqInf int NULL,
   NomeIndicatorEsqInf nvarchar(255) NULL,
   pinIndicatorEsqInf int NULL,
   DonatorEsqSup1 int NULL,
   NomeDonatorEsqSup1 nvarchar(255) NULL,
   pinDonatorEsqSup1 int NULL,
   DonatorEsqSup2 int NULL,
   NomeDonatorEsqSup2 nvarchar(255) NULL,
   pinDonatorEsqSup2 int NULL,
   DonatorEsqInf1 int NULL,
   NomeDonatorEsqInf1 nvarchar(255) NULL,
   pinDonatorEsqInf1 int NULL,
   DonatorEsqInf2 int NULL,
   NomeDonatorEsqInf2 nvarchar(255) NULL,
   pinDonatorEsqInf2 int NULL,
   DataInicio int NULL,
   DataFim int NULL
)
   Insert Into
      #temp
   SELECT 
      ID,
      BoardID,
      StatusID,
      Master,
      '' as NomeMaster,
      0 as pinMaster,
      CoordinatorDir,
      '' as NomeCoordinatorDir,
      0 as pinCoordinatorDir,
      IndicatorDirSup,
      '' as NomeIndicatorDirSup,
      0 as pinIndicatorDirSup,
      IndicatorDirInf,
      '' as NomeIndicatorDirInf,
      0 as pinIndicatorDirInf,
      DonatorDirSup1,
      '' as NomeDonatorDirSup1,
      0 as pinDonatorDirSup1,
      DonatorDirSup2,
      '' as NomeDonatorDirSup2,
      0 as pinDonatorDirSup2,
      DonatorDirInf1,
      '' as NomeDonatorDirInf1,
      0 as pinDonatorDirInf1,
      DonatorDirInf2,
      '' as NomeDonatorDirInf2,
      0 as pinDonatorDirInf2,
      CoordinatorEsq,
      '' as NomeCoordinatorEsq,
      0 as pinCoordinatorEsq,
      IndicatorEsqSup,
      '' as NomeIndicatorEsqSup,
      0 as pinIndicatorEsqSup,
      IndicatorEsqInf,
      '' as NomeIndicatorEsqInf,
      0 as pinIndicatorEsqInf,
      DonatorEsqSup1,
      '' as NomeDonatorEsqSup1,
      0 as pinDonatorEsqSup1,
      DonatorEsqSup2,
      '' as NomeDonatorEsqSup2,
      0 as pinDonatorEsqSup2,
      DonatorEsqInf1,
      '' as NomeDonatorEsqInf1,
      0 as pinDonatorEsqInf1,
      DonatorEsqInf2,
      '' as NomeDonatorEsqInf2,
      0 as pinDonatorEsqInf2,
      DataInicio,
      DataFim
   FROM 
      Rede.Tabuleiro
   Where 
      Id = @id

   --User
   Update tmp Set tmp.NomeMaster = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.Master = usu.id 
   Update tmp Set tmp.NomeCoordinatorDir = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.CoordinatorDir = usu.id 
   Update tmp Set tmp.NomeIndicatorDirSup = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.IndicatorDirSup = usu.id 
   Update tmp Set tmp.NomeIndicatorDirInf = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.IndicatorDirInf = usu.id 
   Update tmp Set tmp.NomeDonatorDirSup1 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorDirSup1 = usu.id 
   Update tmp Set tmp.NomeDonatorDirSup2 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorDirSup2 = usu.id 
   Update tmp Set tmp.NomeDonatorDirInf1 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorDirInf1 = usu.id 
   Update tmp Set tmp.NomeDonatorDirInf2 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorDirInf2 = usu.id 
   Update tmp Set tmp.NomeCoordinatorEsq = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.CoordinatorEsq = usu.id 
   Update tmp Set tmp.NomeIndicatorEsqSup = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.IndicatorEsqSup = usu.id 
   Update tmp Set tmp.NomeIndicatorEsqInf = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.IndicatorEsqInf = usu.id 
   Update tmp Set tmp.NomeDonatorEsqSup1 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqSup1 = usu.id 
   Update tmp Set tmp.NomeDonatorEsqSup2 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqSup2 = usu.id 
   Update tmp Set tmp.NomeDonatorEsqInf1 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqInf1 = usu.id 
   Update tmp Set tmp.NomeDonatorEsqInf2 = usu.Nome From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqInf2 = usu.id 

   --Pin

   --update #temp set pinMaster = max(tu.BoardID) FROM Rede.TabuleiroUsuario tu, #temp tmp Where tu.UsuarioID = tmp.Master
   
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select master from #temp) Update #temp Set pinmaster= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select CoordinatorDir from #temp) Update #temp Set pinCoordinatorDir= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select IndicatorDirSup from #temp) Update #temp Set pinIndicatorDirSup= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select IndicatorDirInf from #temp) Update #temp Set pinIndicatorDirInf= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorDirSup1 from #temp) Update #temp Set pinDonatorDirSup1= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorDirSup2 from #temp) Update #temp Set pinDonatorDirSup2= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorDirInf1 from #temp) Update #temp Set pinDonatorDirInf1= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorDirInf2 from #temp) Update #temp Set pinDonatorDirInf2= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select CoordinatorEsq from #temp) Update #temp Set pinCoordinatorEsq= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select IndicatorEsqSup from #temp) Update #temp Set pinIndicatorEsqSup= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select IndicatorEsqInf from #temp) Update #temp Set pinIndicatorEsqInf= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorEsqSup1 from #temp) Update #temp Set pinDonatorEsqSup1= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorEsqSup2 from #temp) Update #temp Set pinDonatorEsqSup2= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorEsqInf1 from #temp) Update #temp Set pinDonatorEsqInf1= @Pin 
   Select @pin = max(BoardID) FROM Rede.TabuleiroUsuario Where UsuarioID = (select DonatorEsqInf2 from #temp) Update #temp Set pinDonatorEsqInf2= @Pin 
   
   Update #temp Set pinMaster=0 Where pinMaster is null
   Update #temp Set pinCoordinatorDir=0 Where pinCoordinatorDir is null
   Update #temp Set pinIndicatorDirSup=0 Where pinIndicatorDirSup is null
   Update #temp Set pinIndicatorDirInf=0 Where pinIndicatorDirInf is null
   Update #temp Set pinDonatorDirSup1=0 Where pinDonatorDirSup1 is null
   Update #temp Set pinDonatorDirSup2=0 Where pinDonatorDirSup2 is null
   Update #temp Set pinDonatorDirInf1=0 Where pinDonatorDirInf1 is null
   Update #temp Set pinDonatorDirInf2=0 Where pinDonatorDirInf2 is null
   Update #temp Set pinCoordinatorEsq=0 Where pinCoordinatorEsq is null
   Update #temp Set pinIndicatorEsqSup=0 Where pinIndicatorEsqSup is null
   Update #temp Set pinIndicatorEsqInf=0 Where pinIndicatorEsqInf is null
   Update #temp Set pinDonatorEsqSup1=0 Where pinDonatorEsqSup1 is null
   Update #temp Set pinDonatorEsqSup2=0 Where pinDonatorEsqSup2 is null
   Update #temp Set pinDonatorEsqInf1=0 Where pinDonatorEsqInf1 is null
   Update #temp Set pinDonatorEsqInf2=0 Where pinDonatorEsqInf2 is null

   Select 
      ID,
      BoardID,
      StatusID,
      Master,
      NomeMaster,
      pinMaster,
      CoordinatorDir,
      NomeCoordinatorDir,
      pinCoordinatorDir,
      IndicatorDirSup,
      NomeIndicatorDirSup,
      pinIndicatorDirSup,
      IndicatorDirInf,
      NomeIndicatorDirInf,
      pinIndicatorDirInf,
      DonatorDirSup1,
      NomeDonatorDirSup1,
      pinDonatorDirSup1,
      DonatorDirSup2,
      NomeDonatorDirSup2,
      pinDonatorDirSup2,
      DonatorDirInf1,
      NomeDonatorDirInf1,
      pinDonatorDirInf1,
      DonatorDirInf2,
      NomeDonatorDirInf2,
      pinDonatorDirInf2,
      CoordinatorEsq,
      NomeCoordinatorEsq,
      pinCoordinatorEsq,
      IndicatorEsqSup,
      NomeIndicatorEsqSup,
      pinIndicatorEsqSup,
      IndicatorEsqInf,
      NomeIndicatorEsqInf,
      pinIndicatorEsqInf,
      DonatorEsqSup1,
      NomeDonatorEsqSup1,
      pinDonatorEsqSup1,
      DonatorEsqSup2,
      NomeDonatorEsqSup2,
      pinDonatorEsqSup2,
      DonatorEsqInf1,
      NomeDonatorEsqInf1,
      pinDonatorEsqInf1,
      DonatorEsqInf2,
      NomeDonatorEsqInf2,
      pinDonatorEsqInf2,
      DataInicio,
      DataFim
   From
      #Temp
   Order By 
      StatusID
 
End -- Sp

go
Grant Exec on spC_Tabuleiro To public
go

Exec spC_Tabuleiro @id = 1
--Exec spC_Tabuleiro @id = 20

