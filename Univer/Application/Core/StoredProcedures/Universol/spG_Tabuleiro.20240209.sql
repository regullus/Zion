use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spG_Tabuleiro'))
   Drop Procedure spG_Tabuleiro
Go

Create Proc [dbo].[spG_Tabuleiro]
   @UsuarioID int,
   @UsuarioPaiID int,
   @BoardID int,
   @Chamada nvarchar(100)
As
Begin
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   
   --Inclui usuario na rede conforme regras 
   --Expecifico para rede do tipo Univer
   declare
         @ID int,
         @PosicaoPai nvarchar(50),
         @PosicaoFilho nvarchar(150),
         @Incluido bit, -- se true foi incluido
         @DireitaFinalizada bit,
         @EsquerdaFinalizada bit,
         @IndicadorDireitaSuperiorFinalizado bit,
         @IndicadorDireitaInferiorFinalizado bit,
         @IndicadorEsquerdaSuperiorFinalizado bit,
         @IndicadorEsquerdaInferiorFinalizado bit,
         @DataInicio int,
         @DataFim int,
         @Identity as int,
         @Ciclo int,
         @Master int,
         @MasterTabuleiro int,
         @Historico nvarchar(255),
         @NovoTabuleiroID int,
         @IncluiBoardAutomatico bit,
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
         @DonatorEsqInf2 int,
         @Debug bit

   Set @Debug = 'true'
   Set @Incluido = 'false'
   Set @DireitaFinalizada = 'false'
   Set @EsquerdaFinalizada = 'false'
   Set @IndicadorDireitaSuperiorFinalizado = 'false'
   Set @IndicadorDireitaInferiorFinalizado = 'false'
   Set @IndicadorEsquerdaSuperiorFinalizado = 'false'
   Set @IndicadorEsquerdaInferiorFinalizado = 'false'
   Set @DataInicio  = CONVERT(VARCHAR(8),GETDATE(),112)
   Set @DataFim  = CONVERT(VARCHAR(8),GETDATE(),112)
   Set @IncluiBoardAutomatico = 'false'

   Create Table #temp 
   (
      ID int not null,
      BoardID int not null,
      StatusID int not null,
      Master int not null,
      CoodinatorDir int null,
      CoodinatorEsq int null,
      IndicatorDirSup int null,
      IndicatorDirInf int null,
      DonatorDirSup1 int null,
      DonatorDirSup2 int null,
      DonatorDirInf1 int null,
      DonatorDirInf2 int null,
      IndicatorEsqSup int null,
      IndicatorEsqInf int null,
      DonatorEsqSup1 int null,
      DonatorEsqSup2 int null,
      DonatorEsqInf1 int null,
      DonatorEsqInf2 int null,
      DataInicio int null,
      DataFim int null
   )

   --Ferifica se novo usuário já se encontra em algum tabuleiro
   if Exists (
   Select 
      'Existe'   
   From
	   Rede.Tabuleiro tab
   Where
      tab.BoardID = @BoardID and
      (
         tab.Master = @UsuarioID Or
         tab.CoodinatorDir = @UsuarioID Or
         tab.CoodinatorEsq = @UsuarioID Or
         tab.IndicatorDirSup = @UsuarioID Or
         tab.IndicatorDirInf = @UsuarioID Or
         tab.DonatorDirSup1 = @UsuarioID Or
         tab.DonatorDirSup2 = @UsuarioID Or
         tab.DonatorDirInf1 = @UsuarioID Or
         tab.DonatorDirInf2 = @UsuarioID Or
         tab.IndicatorEsqSup = @UsuarioID Or
         tab.IndicatorEsqInf = @UsuarioID Or
         tab.DonatorEsqSup1 = @UsuarioID Or
         tab.DonatorEsqSup2 = @UsuarioID Or
         tab.DonatorEsqInf1 = @UsuarioID Or
         tab.DonatorEsqInf2 = @UsuarioID
      ) and
      tab.StatusID = 1 --Tem que estar ativo no board
	)
   Begin
      --Regra: Caso usuário já exista no tabuleiro, não se pode incluí-lo novamente
      Set @Historico = 'Usuário ' + STR(@UsuarioID) + ' já se encontra no tabuleiro'
   End
   Else
	Begin
      if Exists (
      Select 
         'Novo Usuario Existe' 
      From
         Usuario.Usuario
      Where 
         ID = @UsuarioID
      )
      Begin
         --Procura pai na rede board indicada
         if (@UsuarioPaiID is null)
         Begin
            --Caso @UsuarioPaiID seja null, obtem primeiro tabuleiro disponivel no board passado como paramentro
            Insert Into #temp
            Select 
               tab.ID,
               tab.BoardID,
               tab.StatusID,
               tab.Master,
               tab.CoodinatorDir,
               tab.CoodinatorEsq,
               tab.IndicatorDirSup,
               tab.IndicatorDirInf,
               tab.DonatorDirSup1,
               tab.DonatorDirSup2,
               tab.DonatorDirInf1,
               tab.DonatorDirInf2,
               tab.IndicatorEsqSup,
               tab.IndicatorEsqInf,
               tab.DonatorEsqSup1,
               tab.DonatorEsqSup2,
               tab.DonatorEsqInf1,
               tab.DonatorEsqInf2,
               tab.DataInicio,
               tab.DataFim
            From
	            Rede.Tabuleiro Tab
            Where
               tab.BoardID = @BoardID and
               tab.StatusID = 1 --Tem que estar ativo no board
         End
         Else
         Begin
            Insert Into #temp
            Select 
               tab.ID,
               tab.BoardID,
               tab.StatusID,
               tab.Master,
               tab.CoodinatorDir,
               tab.CoodinatorEsq,
               tab.IndicatorDirSup,
               tab.IndicatorDirInf,
               tab.DonatorDirSup1,
               tab.DonatorDirSup2,
               tab.DonatorDirInf1,
               tab.DonatorDirInf2,
               tab.IndicatorEsqSup,
               tab.IndicatorEsqInf,
               tab.DonatorEsqSup1,
               tab.DonatorEsqSup2,
               tab.DonatorEsqInf1,
               tab.DonatorEsqInf2,
               tab.DataInicio,
               tab.DataFim
            From
	            Rede.Tabuleiro Tab,
               Usuario.Usuario Usu
            Where
               usu.id = @UsuarioPaiID and
               tab.BoardID = @BoardID and
               (
                  tab.Master = @UsuarioPaiID Or
                  tab.CoodinatorDir = @UsuarioPaiID Or
                  tab.CoodinatorEsq = @UsuarioPaiID Or
                  tab.IndicatorDirSup = @UsuarioPaiID Or
                  tab.IndicatorDirInf = @UsuarioPaiID Or
                  tab.DonatorDirSup1 = @UsuarioPaiID Or
                  tab.DonatorDirSup2 = @UsuarioPaiID Or
                  tab.DonatorDirInf1 = @UsuarioPaiID Or
                  tab.DonatorDirInf2 = @UsuarioPaiID Or
                  tab.IndicatorEsqSup = @UsuarioPaiID Or
                  tab.IndicatorEsqInf = @UsuarioPaiID Or
                  tab.DonatorEsqSup1 = @UsuarioPaiID Or
                  tab.DonatorEsqSup2 = @UsuarioPaiID Or
                  tab.DonatorEsqInf1 = @UsuarioPaiID Or
                  tab.DonatorEsqInf2 = @UsuarioPaiID
               )  and
               tab.StatusID = 1 --Tem que estar ativo no board
         End
        
         If Exists (Select 'ok' From #temp)
         --Caso esteja ativo no board
         Begin
            --Select 'Existe #temp' --Teste
            --Determina qual a posiçao do pai no board
            Select 
               @ID = ID,
               @Master = Master,
               @DataInicio = DataInicio,
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
               #temp

            --Verifica se lado direito esta completo
            if(@CoodinatorDir is not Null and @IndicatorDirSup is not Null and @IndicatorDirInf is not Null and @DonatorDirSup1  is not Null and @DonatorDirSup2 is not Null and @DonatorDirInf1 is not Null and @DonatorDirInf2  is not Null)
            Begin
               Set @DireitaFinalizada = 'True'
            End
            --Verifica se lado esquerdo esta completo
            if(@CoodinatorEsq is not Null and @IndicatorEsqSup is not Null and @IndicatorEsqInf is not Null and @DonatorEsqSup1  is not Null and @DonatorEsqSup2 is not Null and @DonatorEsqInf1 is not Null and @DonatorEsqInf2  is not Null)
            Begin
               Set @EsquerdaFinalizada = 'True'
            End

             --Verifica se indicador direito superior esta completo
            if(@IndicatorDirSup is not Null and @DonatorDirSup1 is not Null and @DonatorDirSup2 is not Null)
            Begin
               Set @IndicadorDireitaSuperiorFinalizado = 'True'
            End
            --Verifica se indicador direito inferior esta completo
            if(@IndicatorDirInf is not Null and @DonatorDirInf1 is not Null and @DonatorDirInf2 is not Null)
            Begin
               Set @IndicadorDireitaInferiorFinalizado = 'True'
            End
            --Verifica se indicador esquerdo superior esta completo
            if(@IndicatorEsqSup is not Null and @DonatorEsqSup1 is not Null and @DonatorEsqSup2 is not Null)
            Begin
               Set @IndicadorEsquerdaSuperiorFinalizado = 'True'
            End
            --Verifica se indicador esquerdo inferio esta completo
            if(@IndicatorEsqInf is not Null and @DonatorEsqInf1 is not Null and @DonatorEsqInf2 is not Null)
            Begin
               Set @IndicadorEsquerdaInferiorFinalizado = 'True'
            End
   
            --Verifica se lado esquerdo esta completo
      
            --Regra: Caso ele seja um donator não pode incluir um novo usuario
            If(@DonatorEsqSup1 = @UsuarioPaiID OR @DonatorEsqSup2 = @UsuarioPaiID OR @DonatorEsqInf1 = @UsuarioPaiID  OR @DonatorEsqInf2 = @UsuarioPaiID)
               Set @PosicaoPai = 'Donator'
            If(@DonatorDirSup1 = @UsuarioPaiID OR @DonatorDirSup2 = @UsuarioPaiID OR @DonatorDirInf1 = @UsuarioPaiID  OR @DonatorDirInf2 = @UsuarioPaiID)
               Set @PosicaoPai = 'Donator'
      
            --IndicatorDirSup
            if (@IndicatorDirSup = @UsuarioPaiID )
               Set @PosicaoPai = 'IndicatorDirSup'
            
            --IndicatorDirInf
            if (@IndicatorDirInf = @UsuarioPaiID )
               Set @PosicaoPai = 'IndicatorDirInf'
            
            --IndicatorEsqSup            
            if (@IndicatorEsqSup = @UsuarioPaiID )
               Set @PosicaoPai = 'IndicatorEsqSup'
            
            --IndicatorEsqInf
            if (@IndicatorEsqInf = @UsuarioPaiID )
               Set @PosicaoPai = 'IndicatorEsqInf'
            
            --CoodinatorDir
            if (@CoodinatorDir = @UsuarioPaiID )
               Set @PosicaoPai = 'CoodinatorDir'
            --CoodinatorEsq
            if (@CoodinatorEsq = @UsuarioPaiID )
               Set @PosicaoPai = 'CoodinatorEsq'

            --Master
            if (@Master = @UsuarioPaiID )
               Set @PosicaoPai = 'Master'
         
            --Verifica se novo usuario já esta no tebuleiro do pai
            If(@DonatorEsqSup1 = @UsuarioID OR @DonatorEsqSup2 = @UsuarioID OR @DonatorEsqInf1 = @UsuarioID  OR @DonatorEsqInf2 = @UsuarioID)
               Set @PosicaoPai = 'Usuário já se encontra no tabuleiro'
            If(@DonatorDirSup1 = @UsuarioID OR @DonatorDirSup2 = @UsuarioID OR @DonatorDirInf1 = @UsuarioID  OR @DonatorDirInf2 = @UsuarioID)
		       Set @PosicaoPai = 'Usuário já se encontra no tabuleiro'
		      If(@IndicatorDirSup = @UsuarioID OR @IndicatorDirInf = @UsuarioID OR @IndicatorEsqSup = @UsuarioID  OR @IndicatorEsqInf = @UsuarioID)
               Set @PosicaoPai = 'Usuário já se encontra no tabuleiro'
		      If(@CoodinatorDir = @UsuarioID OR @CoodinatorEsq = @UsuarioID)
               Set @PosicaoPai = 'Usuário já se encontra no tabuleiro'
            
            --*********** MASTER **************
            if(@PosicaoPai = 'Master' Or @DireitaFinalizada = 'true' Or  @EsquerdaFinalizada = 'true')
            Begin
               --Select 'Entrei: Master' as Entrei --Teste

               --*********** COORDINATOR **************
               --Verifica se há cordinator, caso não inclui usuario como coordinator na direita
               if (@CoodinatorDir is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        CoodinatorDir = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @CoodinatorDir = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'CoodinatorDir'
                  End
               --Verifica se há cordinator, caso não inclui usuario como coordinator na esquerda
               if (@CoodinatorEsq is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        CoodinatorEsq = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @CoodinatorEsq = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'CoodinatorEsq'
                  End
            End --Master
            
            --*********** COORDINATOR DIREITA **************
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorDir' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'  Or @IndicadorDireitaInferiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: CoodinatorDir' as Entrei --Teste
               --Verifica se há Indicator, caso não inclui usuario como indicator superior direita
               if (@IndicatorDirSup is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        IndicatorDirSup = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @IndicatorDirSup = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'IndicatorDirSup'
                  End
               --Verifica se há Indicator, caso não inclui usuario como indicator inferior direita
               if (@IndicatorDirInf is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        IndicatorDirInf = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @IndicatorDirInf = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'IndicatorDirInf'
                  End
            End
            
            --*********** COORDINATOR ESQUERDA **************
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorEsq' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'  Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: CoodinatorEsq' as Entrei --Teste
               --Verifica se há Indicator, caso não inclui usuario como indicator superior esquerda
               if (@IndicatorEsqSup is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        IndicatorEsqSup = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @IndicatorEsqSup = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'IndicatorEsqSup'
                  End
               --Verifica se há Indicator, caso não inclui usuario como indicator inferior esquerda
               if (@IndicatorEsqInf is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        IndicatorEsqInf = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @IndicatorEsqInf = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'IndicatorEsqInf'
                  End
            End
            
            --*********** INDICATOR DIREITA Superior ************** 
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorEsq' Or @PosicaoPai = 'CoodinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaInferiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: IndicatorDirSup' as Entrei --Teste
               if (@DonatorDirSup1 is null and @Incluido = 'false')
                  Begin
                     --Select 'Entrei: IndicatorDirSup - DonatorDirSup1' as Entrei --Teste
                     Update
	                     Rede.Tabuleiro
                     Set
                        DonatorDirSup1 = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @DonatorDirSup1 = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'DonatorDirSup1'
                  End
               if (@DonatorDirSup2 is null and @Incluido = 'false')
               Begin
                  --Select 'Entrei: IndicatorDirSup - DonatorDirSup2' as Entrei --Teste
                  Update
	                  Rede.Tabuleiro
                  Set
                     DonatorDirSup2 = @UsuarioID
                  Where
                     ID = @ID
               
                  Set @DonatorDirSup2 = @UsuarioID
                  Set @Incluido = 'true'
                  Set @PosicaoFilho = 'DonatorDirSup2'
               End
            End
            
            --*********** INDICATOR DIREITA Inferior ************** 
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or @PosicaoPai = 'IndicatorDirInf' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: IndicatorDirInf' as Entrei --Teste
               if (@DonatorDirInf1 is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        DonatorDirInf1 = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @DonatorDirInf1 = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'DonatorDirInf1'
                  End
               if (@DonatorDirInf2 is null and @Incluido = 'false')
               Begin
                  Update
	                  Rede.Tabuleiro
                  Set
                     DonatorDirInf2 = @UsuarioID
                  Where
                     ID = @ID
               
                  Set @DonatorDirInf2 = @UsuarioID
                  Set @Incluido = 'true'
                  Set @PosicaoFilho = 'DonatorDirInf2'
               End
            End
            
            --*********** INDICATOR ESQUERDA Superior **************
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @DireitaFinalizada = 'true' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: IndicatorEsqInf' as Entrei --Teste
               if (@DonatorEsqSup1 is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        DonatorEsqSup1 = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @DonatorEsqSup1 = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'DonatorEsqSup1'
                  End
               if (@DonatorEsqSup2 is null and @Incluido = 'false')
               Begin
                  Update
	                  Rede.Tabuleiro
                  Set
                     DonatorEsqSup2 = @UsuarioID
                  Where
                     ID = @ID
               
                  Set @DonatorEsqSup2 = @UsuarioID
                  Set @Incluido = 'true'
                  Set @PosicaoFilho = 'DonatorEsqSup2'
               End
            End
            
            --*********** INDICATOR ESQUERDA Inferior **************
            if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoodinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @PosicaoPai = 'IndicatorEsqInf' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'))
            Begin
               --Select 'Entrei: IndicatorEsqInf' as Entrei --Teste
               if (@DonatorEsqInf1 is null and @Incluido = 'false')
                  Begin
                     Update
	                     Rede.Tabuleiro
                     Set
                        DonatorEsqInf1 = @UsuarioID
                     Where
                        ID = @ID
               
                     Set @DonatorEsqInf1 = @UsuarioID
                     Set @Incluido = 'true'
                     Set @PosicaoFilho = 'DonatorEsqInf1'
                  End
               if (@DonatorEsqInf2 is null and @Incluido = 'false')
               Begin
                  Update
	                  Rede.Tabuleiro
                  Set
                     DonatorEsqInf2 = @UsuarioID
                  Where
                     ID = @ID
               
                  Set @DonatorEsqInf2 = @UsuarioID
                  Set @Incluido = 'true'
                  Set @PosicaoFilho = 'DonatorEsqInf2'
               End
            End

            --*********** DONATOR **************
            if(@PosicaoPai = 'Donator')
            Begin
               --Select 'Entrei: Donator' as Entrei --Teste
               
               --Obtem Master do usuario passado como parametro e inclui o novo usuário nesse tabuleiro
               Select Top(1)
                  @UsuarioPaiID = MasterID 
               From 
                  Rede.TabuleiroUsuario
               Where
                  StatusID = 1 and
                  UsuarioID = @UsuarioPaiID and
                  BoardID = @BoardID

               if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
               Begin
                  --Master acima não foi encontrado. Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuário nesse tabuleiro
                  Select Top(1)
                     @UsuarioPaiID = UsuarioID 
                  From 
                     Rede.TabuleiroUsuario
                  Where
                     StatusID = 1 and
                     BoardID = @BoardID

                  if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
                  Begin
                     Set @Historico = 'Quando o Pai é um Donator, não é possível adicionar um novo usuário'
                     Set @PosicaoFilho = 'Quando o Pai é um Donator, não é possível adicionar um novo usuário'
                  End
                  Else
                  Begin
                     Set @Historico = 'Obtém um pai valido, o informado é um Donator (1)'
                     Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = @BoardID, @Chamada = 'Obtém um pai valido, o informado é um Donator (1)'
                  End
               End
               Else
               Begin
                  --Chama novamente essa sp, agora com um pai valido
                  Set @Historico = 'Obtém um pai valido, o informado é um Donator (2)'
                  Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = @BoardID, @Chamada = 'Obtém um pai valido, o informado é um Donator (2)'
               End
            End
            
            --Select @PosicaoPai as PosicaoPai, @PosicaoFilho as PosicaoFilho, @Incluido as incluido --Teste

            --Verifica se o tabuleiro esta completo
            if(
               @Master is not null And 
               @CoodinatorDir is not null And 
               @CoodinatorEsq is not null And 
               @IndicatorDirSup is not null And 
               @IndicatorDirInf is not null And 
               @IndicatorEsqSup is not null And 
               @IndicatorEsqInf is not null And 
               @DonatorDirSup1 is not null And 
               @DonatorDirSup2 is not null And 
               @DonatorDirInf1 is not null And 
               @DonatorDirInf2 is not null And 
               @DonatorEsqSup1 is not null And 
               @DonatorEsqSup2 is not null And 
               @DonatorEsqInf1 is not null And 
               @DonatorEsqInf2 is not null And
               @PosicaoPai <> 'Donator'
            ) 
            Begin
               --Select 'MCR 4: Tabuleiro completo' --Teste
               --Tabuleiro completo

               Select
                  @Ciclo = MAX(Ciclo)
               From
                  Rede.TabuleiroUsuario
               Where
                  UsuarioID = @UsuarioID and
                  BoardID = @BoardID

               if(@Ciclo is null)
                  Set @Ciclo = 1

               if Not Exists (Select 'Existe' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID = @ID and BoardID = @BoardID And Ciclo = @Ciclo)
               Begin
                  --Select 'MCR 5: Obtem Master do usuario passado como parametro'
                  --Obtem Master do usuario passado como parametro
                  Select Top(1)
                     @MasterTabuleiro = MasterID 
                  From 
                     Rede.TabuleiroUsuario
                  Where
                     StatusID = 1 and
                     UsuarioID = @UsuarioPaiID and
                     BoardID = @BoardID
                  
                  --Select 'MCR `5.1' as MCR, @UsuarioID as UsuarioID, @ID as ID, @BoardID as BoardID, @MasterTabuleiro as MasterID, @Ciclo as Ciclo, @PosicaoFilho as PosicaoFilho --Teste

                  --inclui novo usuario no TabuleiroUsuario 3
                  Insert Into Rede.TabuleiroUsuario
                  (
                     UsuarioID,
	                  TabuleiroID,
                     BoardID,
                     StatusID,
                     MasterID,
                     Ciclo,
                     Posicao,
                     PagoMaster,
                     PagoSistema,
	                  DataInicio,
	                  DataFim
                  ) 
                  Values 
                  (
                     @UsuarioID,
                     @ID,
                     @BoardID,
                     1, --Ativo
                     @MasterTabuleiro,
                     @Ciclo,
                     @PosicaoFilho,
                     'false',
                     'False',
                     @DataInicio,
                     null
                  )

                  --Select 'MCR 5.3' as MCR, * From Rede.TabuleiroUsuario

               End
              
               --Select 'MCR 6: Encerra tabuleiro'
               --Encerra tabuleiro
               Update
                  Rede.Tabuleiro
               Set
                  StatusId = 2, --Finalizado
                  DataFim = CONVERT(VARCHAR(8),GETDATE(),112)
               Where
                  ID = @ID

               Select
                  @Ciclo = MAX(Ciclo)
               From
                  Rede.TabuleiroUsuario
               Where
                  UsuarioID = @UsuarioPaiID and
                  BoardID = @BoardID

               if(@Ciclo is null)
                  Set @Ciclo = 1

               Update
                  Rede.TabuleiroUsuario
               Set
                  StatusID = 2, --Finalizado
                  DataFim = @DataFim
               Where
                   UsuarioID = @Master and 
                   TabuleiroID = @ID and 
                   BoardID = @BoardID and
                   StatusID = 1

               --**** Cria dois novos tabuleiros ****
               --Cria Tabuleiro da Direita
               Insert into Rede.Tabuleiro
               (
                  BoardID,
                  StatusID,
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
               )
               Values
               (
                  @BoardID,
                  1, --StatusId = 1 é ativo
                  @CoodinatorDir, --Master
                  @IndicatorDirSup, --CoodinatorDir
                  @IndicatorDirInf, --CoodinatorEsq
                  @DonatorDirSup1, --IndicatorDirSup
                  @DonatorDirSup2, --IndicatorDirInf
                  null, --DonatorDirSup1
                  null, --DonatorDirSup2
                  null, --DonatorDirInf1
                  null, --DonatorDirInf2
                  @DonatorDirInf1, --IndicatorEsqSup
                  @DonatorDirInf2, --IndicatorEsqInf
                  null, --DonatorEsqSup1
                  null, --DonatorEsqSup2
                  null, --DonatorEsqInf1
                  null, --DonatorEsqInf2
                  @DataInicio, --DataInicio
                  null --DataFim
               )

               Set @Identity = @@IDentity
               
               --Update posicao no TabuleiroUsuario do pessoal da direita
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'Master',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @CoodinatorDir And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'CoodinatorDir',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @IndicatorDirSup And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'CoodinatorEsq',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @IndicatorDirInf And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorDirSup',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @DonatorDirSup1 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorDirInf',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @DonatorDirSup2 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorEsqSup',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @DonatorDirInf1 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorEsqInf',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorDir
               Where
                  UsuarioID = @DonatorDirInf2 And
                  StatusID = 1 and
                  BoardID = @BoardID

               --Cria Tabuleiro da Esquerda
               Insert into Rede.Tabuleiro
               (
                  BoardID,
                  StatusID,
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
               )
               Values
               (
                  @BoardID,
                  1, --StatusId = 1 é ativo
                  @CoodinatorEsq, --Master
                  @IndicatorEsqSup, --CoodinatorDir
                  @IndicatorEsqInf, --CoodinatorEsq
                  @DonatorEsqSup1, --IndicatorDirSup
                  @DonatorEsqSup2, --IndicatorDirInf
                  null, --DonatorDirSup1
                  null, --DonatorDirSup2
                  null, --DonatorDirInf1
                  null, --DonatorDirInf2
                  @DonatorEsqInf1, --IndicatorEsqSup
                  @DonatorEsqInf2, --IndicatorEsqInf
                  null, --DonatorEsqSup1
                  null, --DonatorEsqSup2
                  null, --DonatorEsqInf1
                  null, --DonatorEsqInf2
                  CONVERT(VARCHAR(8),GETDATE(),112), --DataInicio
                  null --DataFim
               )

               Set @Identity = @@IDentity

               --Update posicao no TabuleiroUsuario do pessoal da Esquerda
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'Master',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @CoodinatorEsq And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'CoodinatorDir',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @IndicatorEsqSup And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'CoodinatorEsq',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @IndicatorEsqInf And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorDirSup',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @DonatorEsqSup1 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorDirInf',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @DonatorEsqSup2 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorEsqSup',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @DonatorEsqInf1 And
                  StatusID = 1 and
                  BoardID = @BoardID
               Update
                  Rede.TabuleiroUsuario
               Set
                 Posicao = 'IndicatorEsqInf',
                 TabuleiroID = @Identity,
                 MasterID = @CoodinatorEsq
               Where
                  UsuarioID = @DonatorEsqInf2 And
                  StatusID = 1 and
                  BoardID = @BoardID

               --*********** Promove Usuario Master para novo Board ***********
               --Sobe para proximo Board
               Set @BoardID = @BoardID + 1
               --Verifica se ainda há board acima do master
               IF Not Exists (Select 'Existe' From Rede.Board Where ID = @BoardID)
               Begin
                  --Caso não haja mais board superiores volta ao inicio
                  Set @BoardID = 1
               End

               --Inclui master em novo com novo ciclo e em novo tabuleiro em board superior
               --Select 'MCR 8: @UsuarioID = '+ STR(@UsuarioID) + ', @UsuarioPaiID = '+ STR(@UsuarioPaiID) + ', @BoardID = ' + STR(@BoardID) --Teste
               --Obtem um master ativo no novo board superior

               --Obtem Master do usuario passado como parametro e inclui o novo usuário nesse tabuleiro
               Select Top(1)
                  @UsuarioPaiID = MasterID 
               From 
                  Rede.TabuleiroUsuario
               Where
                  StatusID = 1 and
                  UsuarioID = @UsuarioPaiID and
                  BoardID = @BoardID
               
               --Caso não encontre um Pai obtem o primeiro pai disponivel
               if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
               Begin
                  --Select 'MCR 9: Sem um pai'
                  --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuário nesse tabuleiro
                  Select Top(1)
                     @UsuarioPaiID = UsuarioID 
                  From 
                     Rede.TabuleiroUsuario
                  Where
                     StatusID = 1 and
                     BoardID = @BoardID

                  if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
                  Begin
                     Set @Historico = 'Não foi possível encontrar um master para o usuario: ' + STR(@UsuarioID) + ' para o Board: ' + STR(@BoardID)
                     Set @PosicaoFilho = '***'
                  End
                  Else
                  Begin
                     --Select 'MCR 10: UsuarioID = '+ STR(@UsuarioID) + ' and @UsuarioPaiID = ' + STR(@UsuarioPaiID) + ' and BoardID = '+ STR(@BoardID) + ' And Ciclo = ' + STR(@Ciclo) --Teste
                     --Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (1)
                     Exec spG_Tabuleiro @UsuarioID = @UsuarioPaiID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = @BoardID, @Chamada = 'Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (1)'
                  End
               End
               Else
               Begin
                  --Select 'MCR 11: UsuarioID = '+ STR(@UsuarioID) + ' and @UsuarioPaiID = ' + STR(@UsuarioPaiID) + ' and BoardID = '+ STR(@BoardID) + ' And Ciclo = ' + STR(@Ciclo) --Teste
                  --Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (2)
                  Exec spG_Tabuleiro @UsuarioID = @UsuarioPaiID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = @BoardID, @Chamada = 'Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (2)'
               End
            End
            Else
            Begin
               --Select 'MCR  12: Tabuleiro incompleto', @Chamada as Chamada --Teste
               Select
                  @Ciclo = MAX(Ciclo)
               From
                  Rede.TabuleiroUsuario
               Where
                  UsuarioID = @UsuarioID and
                  BoardID = @BoardID

               if(@Ciclo is null)
                  Set @Ciclo = 1

               if Not Exists (Select 'Existe' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID = @ID and BoardID = @BoardID And Ciclo = @Ciclo)
               Begin
                  --Obtem Master do usuario passado como parametro
                  Select Top(1)
                     @MasterTabuleiro = MasterID 
                  From 
                     Rede.TabuleiroUsuario
                  Where
                     StatusID = 1 and
                     UsuarioID = @UsuarioPaiID and
                     BoardID = @BoardID
                  
                  --inclui novo usuario no TabuleiroUsuario 1
                  Insert Into Rede.TabuleiroUsuario
                  (
                     UsuarioID,
	                  TabuleiroID,
                     BoardID,
                     StatusID,
                     MasterID,
                     Ciclo,
                     Posicao,
                     PagoMaster,
                     PagoSistema,
	                  DataInicio,
	                  DataFim
                  ) 
                  Values 
                  (
                     @UsuarioID,
                     @ID,
                     @BoardID,
                     1, --Ativo
                     Coalesce(@MasterTabuleiro,@MasterTabuleiro,1),
                     @Ciclo,
                     Coalesce(@PosicaoFilho,@PosicaoFilho,'1'),
                     'false',
                     'False',
                     @DataInicio,
                     null
                  )
              End
            End

            /*   
            if(@IndicadorDireitaSuperiorFinalizado = 'true')
               Select 'IndicadorDireitaSuperiorFinalizado Finalizado' --Teste
            if(@IndicadorDireitaInferiorFinalizado = 'true')
               Select 'IndicadorDireitaInferiorFinalizado Finalizado' --Teste
            if(@IndicadorEsquerdaSuperiorFinalizado = 'true')
               Select 'IndicadorEsquerdaSuperiorFinalizado Finalizado' --Teste
            if(@IndicadorEsquerdaInferiorFinalizado = 'true')
               Select 'IndicadorEsquerdaInferiorFinalizado Finalizado' --Teste
            */
         
         End
         Else 
         --Caso NÃO exista o tabuleiro com o board informado
         Begin
            --Select 'MCR 30: NÃO Existe #temp', @UsuarioPaiID as UsuarioPaiID, @BoardID as BoardID, @Chamada as Chamada --Teste
            --Verifica se há usuario ativo no board corrente
            if Exists( Select 'Existe' From Rede.TabuleiroUsuario Where StatusID = 1 and BoardID = @BoardID)
            Begin
               --Select 'MCR  30.1: Existe', @BoardID as BoardID
               --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuário nesse tabuleiro
               Select Top(1)
                  @UsuarioPaiID = UsuarioID 
               From 
                  Rede.TabuleiroUsuario
               Where
                  StatusID = 1 and
                  BoardID = @BoardID
            
               if(@UsuarioPaiID is null)
               Begin
                  Set @Historico = 'Usuário pai ' + STR(@UsuarioPaiID) + ' não existe'
               End
               Else
               Begin
                  --Chama novamente essa sp, agora com um pai valido
                  Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = @BoardID, @Chamada = 'Chama novamente essa sp, agora com um pai valido'
               End
            End
            Else
            Begin
               --Select 'MCR  30.2: Não Existe', @BoardID as BoardID
               --Não existindo usuario no board corrente cria um novo
               Insert into Rede.Tabuleiro
               (
                  BoardID,
                  StatusID,
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
               )
               Values
               (
                  @BoardID,
                  1, --StatusId = 1 é ativo
                  @UsuarioID, --Master
                  Null, --CoodinatorDir
                  Null, --CoodinatorEsq
                  Null, --IndicatorDirSup
                  Null, --IndicatorDirInf
                  null, --DonatorDirSup1
                  null, --DonatorDirSup2
                  null, --DonatorDirInf1
                  null, --DonatorDirInf2
                  Null, --IndicatorEsqSup
                  Null, --IndicatorEsqInf
                  null, --DonatorEsqSup1
                  null, --DonatorEsqSup2
                  null, --DonatorEsqInf1
                  null, --DonatorEsqInf2
                  @DataInicio, --DataInicio
                  null --DataFim
               )

               Set @Identity = @@IDentity
               Set @Ciclo = 1

               --inclui novo usuario no TabuleiroUsuario 2
               Insert Into Rede.TabuleiroUsuario
               (
                  UsuarioID,
	               TabuleiroID,
                  BoardID,
                  StatusID,
                  MasterID,
                  Ciclo,
                  Posicao,
                  PagoMaster,
                  PagoSistema,
	               DataInicio,
	               DataFim
               ) 
               Values 
               (
                  @UsuarioID,
                  @Identity,
                  @BoardID,
                  1, --Ativo
                  @UsuarioID,
                  @Ciclo,
                  'Master',
                  'false',
                  'False',
                  @DataInicio,
                  null
               )
            End
            --Caso o Board seja 2 é porque o usuario subiu de board, assim
            --Além de subir no board deve ser criado uma nova entrada no board 1 para esse usuario
            if (@BoardID = 2)
            Begin
               --Select 'Incluido novamente' --Teste
               --Obtem primeiro master de um tabuleiro disponivel para usuario entrar no board 1 
               --Obtem Master do usuario passado como parametro e inclui o novo usuário nesse tabuleiro
               Select Top(1)
                  @UsuarioPaiID = MasterID 
               From 
                  Rede.TabuleiroUsuario
               Where
                  StatusID = 1 and
                  UsuarioID = @UsuarioPaiID and
                  BoardID = 1

               if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
               Begin
                  --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuário nesse tabuleiro
                  Select Top(1)
                     @UsuarioPaiID = UsuarioID 
                  From 
                     Rede.TabuleiroUsuario
                  Where
                     StatusID = 1 and
                     BoardID = 1

                  if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
                  Begin
                     Set @Historico = 'Não há um tabuleiro diponível para usuario ' + STR(@UsuarioID) + ' Entrar novamente no Board 1 estando no Board 2'
                  End
                  Else
                  Begin
                     --Select 'MCR 19: UsuarioPaiID 1: ' + STR(@UsuarioPaiID) --Teste
                     --Chama novamente essa sp, para inclui o master novamente no Board=1
                     Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = 1, @Chamada = 'Chama novamente essa sp, para inclui o master novamente no Board=1 (1)' --*****
                  End
               End
               Else
               Begin
                  --Select 'MCR  20: UsuarioPaiID 2: ' + STR(@UsuarioPaiID), @Chamada as Chamada --Teste
                  --Chama novamente essa sp, para inclui o master novamente no Board=1
                  Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @UsuarioPaiID, @BoardID = 1, @Chamada = 'Chama novamente essa sp, para inclui o master novamente no Board=1 (2)'
               End
            End
         End
      End
      Else
      Begin
         Set @Historico = 'Novo usuário ' + STR(@UsuarioID) + ' não está cadastrado!'
      End
   End
   
   if(@Historico is not Null)
   Begin
      Insert Into Rede.TabuleiroLog 
      (
         UsuarioID,
         UsuarioPaiID,
         BoardID,
         Data,
         Mensagem
      )
      Values
      (
         Coalesce(@UsuarioID,@UsuarioID,0),
         Coalesce(@UsuarioPaiID,@UsuarioPaiID,0),
         Coalesce(@BoardID,@BoardID,0),
         CONVERT(VARCHAR(8),GETDATE(),112),
         @Historico
      )
   End

   if(@Debug = 'true')
   Begin
      Select @UsuarioID as UsuarioID, @ID TabuleiroID, @PosicaoFilho as Posicao, @Historico as historico
      Set @Historico = ''
   End
   
   Drop Table #temp

End -- Sp

go
Grant Exec on spG_Tabuleiro To public
go

--exec spG_Tabuleiro @UsuarioID = 2591, @UsuarioPaiID =2590, @BoardID = 1, @Chamada = 'Principal'
