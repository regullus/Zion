use Univer
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

   Declare 
       @pin int,
       @corMercurio nvarchar(100),
       @corSaturno nvarchar(100),
       @corMarte nvarchar(100),
       @corJupiter nvarchar(100),
       @corVenus nvarchar(100),
       @corUrano nvarchar(100),
       @corTerra nvarchar(100),
       @corSol nvarchar(100)
   
   Select @CorMercurio = CorTexto From Rede.TabuleiroBoard Where ID = 1
   Select @corSaturno = CorTexto From Rede.TabuleiroBoard Where ID = 2
   Select @corMarte = CorTexto From Rede.TabuleiroBoard Where ID = 3
   Select @corJupiter = CorTexto From Rede.TabuleiroBoard Where ID = 4
   Select @corVenus = CorTexto From Rede.TabuleiroBoard Where ID = 5
   Select @corUrano = CorTexto From Rede.TabuleiroBoard Where ID = 6
   Select @corTerra = CorTexto From Rede.TabuleiroBoard Where ID = 7
   Select @CorSol = CorTexto From Rede.TabuleiroBoard Where ID = 8
 
    CREATE TABLE #temp
    (
        ID int NULL,
        BoardID int NULL,
        StatusID int NULL,
        Master int NULL,
        NomeMaster nvarchar(255) NULL,
        ApelidoMaster nvarchar(15) NULL,
        corMaster nvarchar(20),
        pinMaster int NULL,
        CoordinatorDir int NULL,
        NomeCoordinatorDir nvarchar(255) NULL,
        ApelidoCoordinatorDir nvarchar(15) NULL,
        corCoordinatorDir nvarchar(20),
        pinCoordinatorDir int NULL,
        IndicatorDirSup int NULL,
        NomeIndicatorDirSup nvarchar(255) NULL,
        ApelidoIndicatorDirSup nvarchar(15) NULL,
        corIndicatorDirSup nvarchar(20),
        pinIndicatorDirSup int NULL,
        IndicatorDirInf int NULL,
        NomeIndicatorDirInf nvarchar(255) NULL,
        ApelidoIndicatorDirInf nvarchar(15) NULL,
        corIndicatorDirInf nvarchar(20),
        pinIndicatorDirInf int NULL,
        DonatorDirSup1 int NULL,
        NomeDonatorDirSup1 nvarchar(255) NULL,
        ApelidoDonatorDirSup1 nvarchar(15) NULL,
        corDonatorDirSup1 nvarchar(20),
        pinDonatorDirSup1 int NULL,
        DonatorDirSup2 int NULL,
        NomeDonatorDirSup2 nvarchar(255) NULL,
        ApelidoDonatorDirSup2 nvarchar(15) NULL,
        corDonatorDirSup2 nvarchar(20),
        pinDonatorDirSup2 int NULL,
        DonatorDirInf1 int NULL,
        NomeDonatorDirInf1 nvarchar(255) NULL,
        ApelidoDonatorDirInf1 nvarchar(15) NULL,
        corDonatorDirInf1 nvarchar(20),
        pinDonatorDirInf1 int NULL,
        DonatorDirInf2 int NULL,
        NomeDonatorDirInf2 nvarchar(255) NULL,
        ApelidoDonatorDirInf2 nvarchar(15) NULL,
        corDonatorDirInf2 nvarchar(20),
        pinDonatorDirInf2 int NULL,
        CoordinatorEsq int NULL,
        NomeCoordinatorEsq nvarchar(255) NULL,
        ApelidoCoordinatorEsq nvarchar(15) NULL,
        corCoordinatorEsq nvarchar(20),
        pinCoordinatorEsq int NULL,
        IndicatorEsqSup int NULL,
        NomeIndicatorEsqSup nvarchar(255) NULL,
        ApelidoIndicatorEsqSup nvarchar(15) NULL,
        corIndicatorEsqSup nvarchar(20),
        pinIndicatorEsqSup int NULL,
        IndicatorEsqInf int NULL,
        NomeIndicatorEsqInf nvarchar(255) NULL,
        ApelidoIndicatorEsqInf nvarchar(15) NULL,
        corIndicatorEsqInf nvarchar(20),
        pinIndicatorEsqInf int NULL,
        DonatorEsqSup1 int NULL,
        NomeDonatorEsqSup1 nvarchar(255) NULL,
        ApelidoDonatorEsqSup1 nvarchar(15) NULL,
        corDonatorEsqSup1 nvarchar(20),
        pinDonatorEsqSup1 int NULL,
        DonatorEsqSup2 int NULL,
        NomeDonatorEsqSup2 nvarchar(255) NULL,
        ApelidoDonatorEsqSup2 nvarchar(15) NULL,
        corDonatorEsqSup2 nvarchar(20),
        pinDonatorEsqSup2 int NULL,
        DonatorEsqInf1 int NULL,
        NomeDonatorEsqInf1 nvarchar(255) NULL,
        ApelidoDonatorEsqInf1 nvarchar(15) NULL,
        corDonatorEsqInf1 nvarchar(20),
        pinDonatorEsqInf1 int NULL,
        DonatorEsqInf2 int NULL,
        NomeDonatorEsqInf2 nvarchar(255) NULL,
        ApelidoDonatorEsqInf2 nvarchar(15) NULL,
        corDonatorEsqInf2 nvarchar(20),
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
        '' as ApelidoMaster,
        'white' as corMaster,
        0 as pinMaster,
        CoordinatorDir,
        '' as NomeCoordinatorDir,
        '' as ApelidoCoordinatorDir,
        'white' as corCoordinatorDir,
        0 as pinCoordinatorDir,
        IndicatorDirSup,
        '' as NomeIndicatorDirSup,
        '' as ApelidoIndicatorDirSup,
        'white' as corIndicatorDirSup,
        0 as pinIndicatorDirSup,
        IndicatorDirInf,
        '' as NomeIndicatorDirInf,
        '' as ApelidoIndicatorDirInf,
        'white' as corIndicatorDirInf,
        0 as pinIndicatorDirInf,
        DonatorDirSup1,
        '' as NomeDonatorDirSup1,
        '' as ApelidoDonatorDirSup1,
        'white' as corDonatorDirSup1,
        0 as pinDonatorDirSup1,
        DonatorDirSup2,
        '' as NomeDonatorDirSup2,
        '' as ApelidoDonatorDirSup2,
        'white' as corDonatorDirSup2,
        0 as pinDonatorDirSup2,
        DonatorDirInf1,
        '' as NomeDonatorDirInf1,
        '' as ApelidoDonatorDirInf1,
        'white' as corDonatorDirInf1,
        0 as pinDonatorDirInf1,
        DonatorDirInf2,
        '' as NomeDonatorDirInf2,
        '' as ApelidoDonatorDirInf2,
        'white' as corDonatorDirInf2,
        0 as pinDonatorDirInf2,
        CoordinatorEsq,
        '' as NomeCoordinatorEsq,
        '' as ApelidoCoordinatorEsq,
        'white' as corCoordinatorEsq,
        0 as pinCoordinatorEsq,
        IndicatorEsqSup,
        '' as NomeIndicatorEsqSup,
        '' as ApelidoIndicatorEsqSup,
        'white' as corIndicatorEsqSup,
        0 as pinIndicatorEsqSup,
        IndicatorEsqInf,
        '' as NomeIndicatorEsqInf,
        '' as ApelidoIndicatorEsqInf,
        'white' as corIndicatorEsqInf,
        0 as pinIndicatorEsqInf,
        DonatorEsqSup1,
        '' as NomeDonatorEsqSup1,
        '' as ApelidoDonatorEsqSup1,
        'white' as corDonatorEsqSup1,
        0 as pinDonatorEsqSup1,
        DonatorEsqSup2,
        '' as NomeDonatorEsqSup2,
        '' as ApelidoDonatorEsqSup2,
        'white' as corDonatorEsqSup2,
        0 as pinDonatorEsqSup2,
        DonatorEsqInf1,
        '' as NomeDonatorEsqInf1,
        '' as ApelidoDonatorEsqInf1,
        'white' as corDonatorEsqInf1,
        0 as pinDonatorEsqInf1,
        DonatorEsqInf2,
        '' as NomeDonatorEsqInf2,
        '' as ApelidoDonatorEsqInf2,
        'white' as corDonatorEsqInf2,
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

   Update tmp Set tmp.ApelidoMaster = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.Master = usu.id 
   Update tmp Set tmp.ApelidoCoordinatorDir = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.CoordinatorDir = usu.id 
   Update tmp Set tmp.ApelidoIndicatorDirSup = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.IndicatorDirSup = usu.id 
   Update tmp Set tmp.ApelidoIndicatorDirInf = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.IndicatorDirInf = usu.id 
   Update tmp Set tmp.ApelidoDonatorDirSup1 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorDirSup1 = usu.id 
   Update tmp Set tmp.ApelidoDonatorDirSup2 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorDirSup2 = usu.id 
   Update tmp Set tmp.ApelidoDonatorDirInf1 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorDirInf1 = usu.id 
   Update tmp Set tmp.ApelidoDonatorDirInf2 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorDirInf2 = usu.id 
   Update tmp Set tmp.ApelidoCoordinatorEsq = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.CoordinatorEsq = usu.id 
   Update tmp Set tmp.ApelidoIndicatorEsqSup = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.IndicatorEsqSup = usu.id 
   Update tmp Set tmp.ApelidoIndicatorEsqInf = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.IndicatorEsqInf = usu.id 
   Update tmp Set tmp.ApelidoDonatorEsqSup1 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqSup1 = usu.id 
   Update tmp Set tmp.ApelidoDonatorEsqSup2 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqSup2 = usu.id 
   Update tmp Set tmp.ApelidoDonatorEsqInf1 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqInf1 = usu.id 
   Update tmp Set tmp.ApelidoDonatorEsqInf2 = usu.Apelido From #temp tmp, usuario.usuario usu Where tmp.DonatorEsqInf2 = usu.id 

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
   
   --Master
   if exists (Select 'Existe' From #temp Where pinMaster = 1) update #temp set corMaster = @corMercurio
   if exists (Select 'Existe' From #temp Where pinMaster = 2) update #temp set corMaster = @corSaturno
   if exists (Select 'Existe' From #temp Where pinMaster = 3) update #temp set corMaster = @corMarte
   if exists (Select 'Existe' From #temp Where pinMaster = 4) update #temp set corMaster = @corJupiter
   if exists (Select 'Existe' From #temp Where pinMaster = 5) update #temp set corMaster = @corVenus
   if exists (Select 'Existe' From #temp Where pinMaster = 6) update #temp set corMaster = @corUrano
   if exists (Select 'Existe' From #temp Where pinMaster = 7) update #temp set corMaster = @corTerra
   if exists (Select 'Existe' From #temp Where pinMaster = 8) update #temp set corMaster = @corSol

   --CoordinatorDir
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 1) update #temp set corCoordinatorDir = @corMercurio
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 2) update #temp set corCoordinatorDir = @corSaturno
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 3) update #temp set corCoordinatorDir = @corMarte
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 4) update #temp set corCoordinatorDir = @corJupiter
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 5) update #temp set corCoordinatorDir = @corVenus
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 6) update #temp set corCoordinatorDir = @corUrano
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 7) update #temp set corCoordinatorDir = @corTerra
   if exists (Select 'Existe' From #temp Where pinCoordinatorDir = 8) update #temp set corCoordinatorDir = @corSol
   
   --IndicatorDirSup
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 1) update #temp set corIndicatorDirSup = @corMercurio
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 2) update #temp set corIndicatorDirSup = @corSaturno
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 3) update #temp set corIndicatorDirSup = @corMarte
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 4) update #temp set corIndicatorDirSup = @corJupiter
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 5) update #temp set corIndicatorDirSup = @corVenus
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 6) update #temp set corIndicatorDirSup = @corUrano
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 7) update #temp set corIndicatorDirSup = @corTerra
   if exists (Select 'Existe' From #temp Where pinIndicatorDirSup = 8) update #temp set corIndicatorDirSup = @corSol

   --IndicatorDirInf
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 1) update #temp set corIndicatorDirInf = @corMercurio
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 2) update #temp set corIndicatorDirInf = @corSaturno
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 3) update #temp set corIndicatorDirInf = @corMarte
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 4) update #temp set corIndicatorDirInf = @corJupiter
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 5) update #temp set corIndicatorDirInf = @corVenus
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 6) update #temp set corIndicatorDirInf = @corUrano
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 7) update #temp set corIndicatorDirInf = @corTerra
   if exists (Select 'Existe' From #temp Where pinIndicatorDirInf = 8) update #temp set corIndicatorDirInf = @corSol

   --DonatorDirSup1
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 1) update #temp set corDonatorDirSup1 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 2) update #temp set corDonatorDirSup1 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 3) update #temp set corDonatorDirSup1 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 4) update #temp set corDonatorDirSup1 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 5) update #temp set corDonatorDirSup1 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 6) update #temp set corDonatorDirSup1 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 7) update #temp set corDonatorDirSup1 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup1 = 8) update #temp set corDonatorDirSup1 = @corSol

   --DonatorDirSup2
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 1) update #temp set corDonatorDirSup2 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 2) update #temp set corDonatorDirSup2 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 3) update #temp set corDonatorDirSup2 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 4) update #temp set corDonatorDirSup2 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 5) update #temp set corDonatorDirSup2 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 6) update #temp set corDonatorDirSup2 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 7) update #temp set corDonatorDirSup2 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorDirSup2 = 8) update #temp set corDonatorDirSup2 = @corSol

   --DonatorDirInf1
      if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 1) update #temp set corDonatorDirInf1 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 2) update #temp set corDonatorDirInf1 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 3) update #temp set corDonatorDirInf1 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 4) update #temp set corDonatorDirInf1 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 5) update #temp set corDonatorDirInf1 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 6) update #temp set corDonatorDirInf1 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 7) update #temp set corDonatorDirInf1 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf1 = 8) update #temp set corDonatorDirInf1 = @corSol

   --DonatorDirInf2
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 1) update #temp set corDonatorDirInf2 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 2) update #temp set corDonatorDirInf2 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 3) update #temp set corDonatorDirInf2 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 4) update #temp set corDonatorDirInf2 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 5) update #temp set corDonatorDirInf2 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 6) update #temp set corDonatorDirInf2 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 7) update #temp set corDonatorDirInf2 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorDirInf2 = 8) update #temp set corDonatorDirInf2 = @corSol


   --CoordinatorEsq
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 1) update #temp set corCoordinatorEsq = @corMercurio
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 2) update #temp set corCoordinatorEsq = @corSaturno
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 3) update #temp set corCoordinatorEsq = @corMarte
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 4) update #temp set corCoordinatorEsq = @corJupiter
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 5) update #temp set corCoordinatorEsq = @corVenus
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 6) update #temp set corCoordinatorEsq = @corUrano
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 7) update #temp set corCoordinatorEsq = @corTerra
   if exists (Select 'Existe' From #temp Where pinCoordinatorEsq = 8) update #temp set corCoordinatorEsq = @corSol


   --IndicatorEsqSup
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 1) update #temp set corIndicatorEsqSup = @corMercurio
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 2) update #temp set corIndicatorEsqSup = @corSaturno
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 3) update #temp set corIndicatorEsqSup = @corMarte
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 4) update #temp set corIndicatorEsqSup = @corJupiter
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 5) update #temp set corIndicatorEsqSup = @corVenus
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 6) update #temp set corIndicatorEsqSup = @corUrano
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 7) update #temp set corIndicatorEsqSup = @corTerra
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqSup = 8) update #temp set corIndicatorEsqSup = @corSol

   --IndicatorEsqInf

   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 1) update #temp set corIndicatorEsqInf = @corMercurio
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 2) update #temp set corIndicatorEsqInf = @corSaturno
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 3) update #temp set corIndicatorEsqInf = @corMarte
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 4) update #temp set corIndicatorEsqInf = @corJupiter
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 5) update #temp set corIndicatorEsqInf = @corVenus
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 6) update #temp set corIndicatorEsqInf = @corUrano
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 7) update #temp set corIndicatorEsqInf = @corTerra
   if exists (Select 'Existe' From #temp Where pinIndicatorEsqInf = 8) update #temp set corIndicatorEsqInf = @corSol

   --DonatorEsqSup1
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 1) update #temp set corDonatorEsqSup1 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 2) update #temp set corDonatorEsqSup1 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 3) update #temp set corDonatorEsqSup1 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 4) update #temp set corDonatorEsqSup1 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 5) update #temp set corDonatorEsqSup1 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 6) update #temp set corDonatorEsqSup1 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 7) update #temp set corDonatorEsqSup1 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup1 = 8) update #temp set corDonatorEsqSup1 = @corSol


   --DonatorEsqSup2
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 1) update #temp set corDonatorEsqSup2 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 2) update #temp set corDonatorEsqSup2 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 3) update #temp set corDonatorEsqSup2 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 4) update #temp set corDonatorEsqSup2 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 5) update #temp set corDonatorEsqSup2 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 6) update #temp set corDonatorEsqSup2 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 7) update #temp set corDonatorEsqSup2 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorEsqSup2 = 8) update #temp set corDonatorEsqSup2 = @corSol

   --DonatorEsqInf1
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 1) update #temp set corDonatorEsqInf1 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 2) update #temp set corDonatorEsqInf1 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 3) update #temp set corDonatorEsqInf1 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 4) update #temp set corDonatorEsqInf1 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 5) update #temp set corDonatorEsqInf1 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 6) update #temp set corDonatorEsqInf1 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 7) update #temp set corDonatorEsqInf1 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf1 = 8) update #temp set corDonatorEsqInf1 = @corSol

--DonatorEsqInf2
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 1) update #temp set corDonatorEsqInf2 = @corMercurio
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 2) update #temp set corDonatorEsqInf2 = @corSaturno
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 3) update #temp set corDonatorEsqInf2 = @corMarte
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 4) update #temp set corDonatorEsqInf2 = @corJupiter
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 5) update #temp set corDonatorEsqInf2 = @corVenus
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 6) update #temp set corDonatorEsqInf2 = @corUrano
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 7) update #temp set corDonatorEsqInf2 = @corTerra
   if exists (Select 'Existe' From #temp Where pinDonatorEsqInf2 = 8) update #temp set corDonatorEsqInf2 = @corSol

    Select 
        ID,
        BoardID,
        StatusID,
        Master,
        NomeMaster,
        ApelidoMaster,
        corMaster,
        pinMaster,
        CoordinatorDir,
        NomeCoordinatorDir,
        ApelidoCoordinatorDir,
        corCoordinatorDir,
        pinCoordinatorDir,
        IndicatorDirSup,
        NomeIndicatorDirSup,
        ApelidoIndicatorDirSup,
        corIndicatorDirSup,
        pinIndicatorDirSup,
        IndicatorDirInf,
        NomeIndicatorDirInf,
        ApelidoIndicatorDirInf,
        corIndicatorDirInf,
        pinIndicatorDirInf,
        DonatorDirSup1,
        NomeDonatorDirSup1,
        ApelidoDonatorDirSup1,
        corDonatorDirSup1,
        pinDonatorDirSup1,
        DonatorDirSup2,
        NomeDonatorDirSup2,
        ApelidoDonatorDirSup2,
        corDonatorDirSup2,
        pinDonatorDirSup2,
        DonatorDirInf1,
        NomeDonatorDirInf1,
        ApelidoDonatorDirInf1,
        corDonatorDirInf1,
        pinDonatorDirInf1,
        DonatorDirInf2,
        NomeDonatorDirInf2,
        ApelidoDonatorDirInf2,
        corDonatorDirInf2,
        pinDonatorDirInf2,
        CoordinatorEsq,
        NomeCoordinatorEsq,
        ApelidoCoordinatorEsq,
        corCoordinatorEsq,
        pinCoordinatorEsq,
        IndicatorEsqSup,
        NomeIndicatorEsqSup,
        ApelidoIndicatorEsqSup,
        corIndicatorEsqSup,
        pinIndicatorEsqSup,
        IndicatorEsqInf,
        NomeIndicatorEsqInf,
        ApelidoIndicatorEsqInf,
        corIndicatorEsqInf,
        pinIndicatorEsqInf,
        DonatorEsqSup1,
        NomeDonatorEsqSup1,
        ApelidoDonatorEsqSup1,
        corDonatorEsqSup1,
        pinDonatorEsqSup1,
        DonatorEsqSup2,
        NomeDonatorEsqSup2,
        ApelidoDonatorEsqSup2,
        corDonatorEsqSup2,
        pinDonatorEsqSup2,
        DonatorEsqInf1,
        NomeDonatorEsqInf1,
        ApelidoDonatorEsqInf1,
        corDonatorEsqInf1,
        pinDonatorEsqInf1,
        DonatorEsqInf2,
        NomeDonatorEsqInf2,
        ApelidoDonatorEsqInf2,
        corDonatorEsqInf2,
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



