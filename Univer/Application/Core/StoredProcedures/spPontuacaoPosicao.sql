use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spG_Estacao'))
   Drop Procedure spG_Estacao
Go

Create Proc [dbo].[spG_Estacao]
   @UsuarioID int,
   @UsuarioPaiID int,
   @BoardID int
As
Begin
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   
   --Inclui usuario na rede conforme regras 
   --Expecifico para rede do tipo Univer
   declare
         @Check nvarchar(50),
         @Master int,
         @CoodinatorDir int,
         @CoodinatorEsq int,
         @IndicatorDirSup int,
         @IndicatorDirInf int,
         @DonatorDirSup1 int,
         @DonatorDirSup2 int,
         @DonatorDirInf1 int,
         @DonatorDirInf2 int,
         @IndicatorEsqSup int,
         @IndicatorEsqInf int,
         @DonatorEsqSup1 int,
         @DonatorEsqSup2 int,
         @DonatorEsqInf1 int,
         @DonatorEsqInf2 int

   --Procura pai na rede board indicada
   Select 
      UsuarioID,
      BoardID,
      StatusID,
      Ciclo,
      Master,
      CoodinatorDir,
      CoodinatorEsq,
      IndicatorDirSup,
      IndicatorDirInf,
      DonatorDirSup1,
      DonatorDirSup2,
      DonatorDirInf1,
      DonatorDirInf2,
      IndicatorEsqSup,
      IndicatorEsqInf,
      DonatorEsqSup1,
      DonatorEsqSup2,
      DonatorEsqInf1,
      DonatorEsqInf2,
      DataInicio,
      DataFim
   Into #tempEstacao
   From
	   Rede.Estacao
   Where
      BoardID = @BoardID and
      UsuarioID = @UsuarioPaiID and
      StatusID = 1 --Tem que estar ativo no board

   
   If Exists (Select 'ok' From #tempEstacao)
   --Caso esteja ativo no board
   Begin
      --Determina qual a posiçao do pai no board

      Select 
         @Master = Master,
         @CoodinatorDir = CoodinatorDir,
         @CoodinatorEsq = CoodinatorEsq,
         @IndicatorDirSup = IndicatorDirSup,
         @IndicatorDirInf = IndicatorDirInf,
         @DonatorDirSup1 = DonatorDirSup1,
         @DonatorDirSup2 = DonatorDirSup2,
         @DonatorDirInf1 = DonatorDirInf1,
         @DonatorDirInf2 = DonatorDirInf2,
         @IndicatorEsqSup = IndicatorEsqSup,
         @IndicatorEsqInf = IndicatorEsqInf,
         @DonatorEsqSup1 = DonatorEsqSup1,
         @DonatorEsqSup2 = DonatorEsqSup2,
         @DonatorEsqInf1 = DonatorEsqInf1,
         @DonatorEsqInf2 = DonatorEsqInf2
      From 
         #tempEstacao

      Select 
         @Master as Master,
         @CoodinatorDir as CoodinatorDir,
         @CoodinatorEsq as CoodinatorEsq,
         @IndicatorDirSup as IndicatorDirSup,
         @IndicatorDirInf as IndicatorDirInf,
         @DonatorDirSup1 as DonatorDirSup1,
         @DonatorDirSup2 as DonatorDirSup2,
         @DonatorDirInf1 as DonatorDirInf1,
         @DonatorDirInf2 as DonatorDirInf2,
         @IndicatorEsqSup as IndicatorEsqSup,
         @IndicatorEsqInf as IndicatorEsqInf,
         @DonatorEsqSup1 as DonatorEsqSup1,
         @DonatorEsqSup2 as DonatorEsqSup2,
         @DonatorEsqInf1 as DonatorEsqInf1,
         @DonatorEsqInf2 as DonatorEsqInf2

      --Regra: Caso ele seja um donator não pode incluir um novo usuario
      If(@DonatorEsqSup1 = @UsuarioPaiID OR @DonatorEsqSup2 = @UsuarioPaiID OR @DonatorEsqInf1 = @UsuarioPaiID  OR @DonatorEsqInf2 = @UsuarioPaiID)
      Begin
         Select 'NOOK'
      End
      

      --Select 
      --   UsuarioID,
      --   BoardID,
      --   StatusID,
      --   Ciclo,
      --   Master,
      --   CoodinatorDir,
      --   CoodinatorEsq,
      --   IndicatorDirSup,
      --   IndicatorDirInf,
      --   DonatorDirSup1,
      --   DonatorDirSup2,
      --   DonatorDirInf1,
      --   DonatorDirInf2,
      --   IndicatorEsqSup,
      --   IndicatorEsqInf,
      --   DonatorEsqSup1,
      --   DonatorEsqSup2,
      --   DonatorEsqInf1,
      --   DonatorEsqInf2,
      --   DataInicio,
      --   DataFim 
      --From 
      --   #tempEstacao
      
   End
   Else 
   --Caso NÃO esteja ativo no board
   Begin
      Select 'Sem dados'
   End

End -- Sp

go
Grant Exec on spG_Estacao To public
go

exec spG_Estacao 
   @UsuarioID = 2591,
	@UsuarioPaiID =2590,
	@BoardID = 1

