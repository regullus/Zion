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
    Set NOCOUNT ON

    /*
    Chamada pode ser:
    'Principal, para incluir um novo usu�rio'
    'Donator', quando um UsuariiPaiID passado � um Donator que n�o pode ser pai de ninguem, Da� seleciona um pai valido, N�O USAR EXTERNAMENTE � uma chamada recurssiva desta sp
    'Convite' quando um usuario j� � veterano e � convidado para entrar novamente nos tabuleiros, N�O USAR EXTERNAMENTE � uma chamada  recurssiva desta sp
    'PaiValido' chama novamente a essa sp com um pai valido, ocorre quando n�o � passado um pai valido no sistema, N�O USAR EXTERNAMENTE � uma chamada  recurssiva desta sp
    'Completa' Para quando o tabuleiro foi finalizado
    */
   
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
            @IncluiUsuAutBoard bit,
            @Continua bit,
            @CoordinatorDir int,
            @IndicatorDirSup int,
            @IndicatorDirInf int,
            @DonatorDirSup1 int,
            @DonatorDirSup2 int,
            @DonatorDirInf1 int,
            @DonatorDirInf2 int,
            @CoordinatorEsq int,
            @IndicatorEsqSup int,
            @IndicatorEsqInf int,
            @DonatorEsqSup1 int,
            @DonatorEsqSup2 int,
            @DonatorEsqInf1 int,
            @DonatorEsqInf2 int,
            @PagoMaster bit,
            @log nvarchar(max),
            @count int
           
    Set @Incluido = 'false'
    Set @DireitaFinalizada = 'false'
    Set @EsquerdaFinalizada = 'false'
    Set @IndicadorDireitaSuperiorFinalizado = 'false'
    Set @IndicadorDireitaInferiorFinalizado = 'false'
    Set @IndicadorEsquerdaSuperiorFinalizado = 'false'
    Set @IndicadorEsquerdaInferiorFinalizado = 'false'
    Set @DataInicio  = CONVERT(VARCHAR(8),GETDATE(),112)
    Set @DataFim  = CONVERT(VARCHAR(8),GETDATE(),112)
    Set @Continua = 'true'
    Set @PagoMaster = 'false'
    Set @log = ''
    Set @Historico = ''

    --CUIDADO se true vai incluir o usuario master que fechou um tabuleiro automaticamente em um novo board superior
    --Tambem ira incluir automaticamente esse mesmo master no board1
    Set @IncluiUsuAutBoard = 'false'

    Create Table #temp 
    (
        ID int not null,
        BoardID int not null,
        StatusID int not null,
        Master int not null,
        CoordinatorDir int null,
        IndicatorDirSup int null,
        IndicatorDirInf int null,
        DonatorDirSup1 int null,
        DonatorDirSup2 int null,
        DonatorDirInf1 int null,
        DonatorDirInf2 int null,
        CoordinatorEsq int null,
        IndicatorEsqSup int null,
        IndicatorEsqInf int null,
        DonatorEsqSup1 int null,
        DonatorEsqSup2 int null,
        DonatorEsqInf1 int null,
        DonatorEsqInf2 int null,
        DataInicio int null,
        DataFim int null
    )

    --Ferifica se novo usu�rio j� se encontra em algum tabuleiro
    if Exists (
    Select 
        'Existe'   
    From
        Rede.Tabuleiro 
    Where
        BoardID = @BoardID and
        (
            Master = @UsuarioID Or
            CoordinatorDir = @UsuarioID Or
            IndicatorDirSup = @UsuarioID Or
            IndicatorDirInf = @UsuarioID Or
            DonatorDirSup1 = @UsuarioID Or
            DonatorDirSup2 = @UsuarioID Or
            DonatorDirInf1 = @UsuarioID Or
            DonatorDirInf2 = @UsuarioID Or
            CoordinatorEsq = @UsuarioID Or
            IndicatorEsqSup = @UsuarioID Or
            IndicatorEsqInf = @UsuarioID Or
            DonatorEsqSup1 = @UsuarioID Or
            DonatorEsqSup2 = @UsuarioID Or
            DonatorEsqInf1 = @UsuarioID Or
            DonatorEsqInf2 = @UsuarioID
        ) and
        StatusID = 1 And --Tem que estar ativo no board
        @Chamada <> 'Completa'
    )
    Begin
        --Regra: Caso usu�rio j� exista no tabuleiro, n�o se pode inclu�-lo novamente
        Set @Historico = '01 Usu�rio (' + TRIM(STR(@UsuarioID)) + ') j� se encontra no tabuleiro (0). Chamada: ' + @Chamada
        Set @log = @log + '|01 J� se encontra no tabuleiro'
    End
    Else
    Begin
        Set @log = @log + '| 01 Existe'
        if Exists (
            Select 
                'Novo Usuario Existe' 
            From
                Usuario.Usuario
            Where 
                ID = @UsuarioID
        )
        Begin
            Set @log = @log + '| 02 Novo Usuario Existe'

            --*************INICIO POPULA #temp***********

            --Procura pai na rede board indicada
            if (@UsuarioPaiID is null)
            Begin
                Set @log = @log + '| 03 UsuarioPaiID � null - #temp Criada'
                --Caso @UsuarioPaiID seja null, obtem primeiro tabuleiro disponivel no board passado como paramentro
                Insert Into #temp
                Select 
                    tab.ID as ID,
                    tab.BoardID as BoardID,
                    tab.StatusID as StatusID,
                    tab.Master as Master,
                    tab.CoordinatorDir as CoordinatorDir,
                    tab.IndicatorDirSup as IndicatorDirSup,
                    tab.IndicatorDirInf as IndicatorDirInf,
                    tab.DonatorDirSup1 as DonatorDirSup1,
                    tab.DonatorDirSup2 as DonatorDirSup2,
                    tab.DonatorDirInf1 as DonatorDirInf1,
                    tab.DonatorDirInf2 as DonatorDirInf2,
                    tab.CoordinatorEsq as CoordinatorEsq,
                    tab.IndicatorEsqSup as IndicatorEsqSup,
                    tab.IndicatorEsqInf as IndicatorEsqInf,
                    tab.DonatorEsqSup1 as DonatorEsqSup1,
                    tab.DonatorEsqSup2 as DonatorEsqSup2,
                    tab.DonatorEsqInf1 as DonatorEsqInf1,
                    tab.DonatorEsqInf2 as DonatorEsqInf2,
                    tab.DataInicio as DataInicio,
                    tab.DataFim as DataFim
                From
                    Rede.Tabuleiro Tab
                Where
                    tab.BoardID = @BoardID and
                    tab.StatusID = 1 --Tem que estar ativo no board
                End
            Else
            Begin
                Set @log = @log + '| 04 UsuarioPaiID n�o � null - #temp Criada'
                Insert Into #temp
                Select 
                    tab.ID as ID,
                    tab.BoardID as BoardID,
                    tab.StatusID as StatusID,
                    tab.Master as Master,
                    tab.CoordinatorDir as CoordinatorDir,
                    tab.IndicatorDirSup as IndicatorDirSup,
                    tab.IndicatorDirInf as IndicatorDirInf,
                    tab.DonatorDirSup1 as DonatorDirSup1,
                    tab.DonatorDirSup2 as DonatorDirSup2,
                    tab.DonatorDirInf1 as DonatorDirInf1,
                    tab.DonatorDirInf2 as DonatorDirInf2,
                    tab.CoordinatorEsq as CoordinatorEsq,
                    tab.IndicatorEsqSup as IndicatorEsqSup,
                    tab.IndicatorEsqInf as IndicatorEsqInf,
                    tab.DonatorEsqSup1 as DonatorEsqSup1,
                    tab.DonatorEsqSup2 as DonatorEsqSup2,
                    tab.DonatorEsqInf1 as DonatorEsqInf1,
                    tab.DonatorEsqInf2 as DonatorEsqInf2,
                    tab.DataInicio as DataInicio,
                    tab.DataFim as DataFim
                From
                    Rede.Tabuleiro Tab,
                    Usuario.Usuario Usu
                Where
                    usu.id = @UsuarioPaiID and
                    tab.BoardID = @BoardID and
                    (
                        tab.Master = @UsuarioPaiID Or
                        tab.CoordinatorDir = @UsuarioPaiID Or
                        tab.IndicatorDirSup = @UsuarioPaiID Or
                        tab.IndicatorDirInf = @UsuarioPaiID Or
                        tab.DonatorDirSup1 = @UsuarioPaiID Or
                        tab.DonatorDirSup2 = @UsuarioPaiID Or
                        tab.DonatorDirInf1 = @UsuarioPaiID Or
                        tab.DonatorDirInf2 = @UsuarioPaiID Or
                        tab.CoordinatorEsq = @UsuarioPaiID Or
                        tab.IndicatorEsqSup = @UsuarioPaiID Or
                        tab.IndicatorEsqInf = @UsuarioPaiID Or
                        tab.DonatorEsqSup1 = @UsuarioPaiID Or
                        tab.DonatorEsqSup2 = @UsuarioPaiID Or
                        tab.DonatorEsqInf1 = @UsuarioPaiID Or
                        tab.DonatorEsqInf2 = @UsuarioPaiID
                    )  and
                    tab.StatusID = 1 --Tem que estar ativo no board
            End

            --*************FIM POPULA #temp***********

            --Tudo ok continua o processo
            If Exists (Select 'ok' From #temp)
            --Caso esteja ativo no board
            Begin
                Set @log = @log + '| 05 Temp tem conteudo'
                --Determina qual a posi�ao do pai no board
                Select 
                    @ID = ID,
                    @Master = Master,
                    @DataInicio = DataInicio,
                    @CoordinatorDir = CoordinatorDir,
                    @IndicatorDirSup = IndicatorDirSup,
                    @IndicatorDirInf = IndicatorDirInf,
                    @DonatorDirSup1 = DonatorDirSup1,
                    @DonatorDirSup2 = DonatorDirSup2,
                    @DonatorDirInf1 = DonatorDirInf1,
                    @DonatorDirInf2 = DonatorDirInf2,
                    @CoordinatorEsq = CoordinatorEsq,
                    @IndicatorEsqSup = IndicatorEsqSup,
                    @IndicatorEsqInf = IndicatorEsqInf,
                    @DonatorEsqSup1 = DonatorEsqSup1,
                    @DonatorEsqSup2 = DonatorEsqSup2,
                    @DonatorEsqInf1 = DonatorEsqInf1,
                    @DonatorEsqInf2 = DonatorEsqInf2
                From 
                    #temp
                
                --Verifica se lado direito esta completo
                Set @log = @log + '| 06 Verifica se lado direito e esquerdo est�o completos'

                if(@CoordinatorDir is not Null and @IndicatorDirSup is not Null and @IndicatorDirInf is not Null and @DonatorDirSup1  is not Null and @DonatorDirSup2 is not Null and @DonatorDirInf1 is not Null and @DonatorDirInf2  is not Null)
                Begin
                    Set @DireitaFinalizada = 'True'
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
            
                --Verifica se lado esquerdo esta completo
                if(@CoordinatorEsq is not Null and @IndicatorEsqSup is not Null and @IndicatorEsqInf is not Null and @DonatorEsqSup1  is not Null and @DonatorEsqSup2 is not Null and @DonatorEsqInf1 is not Null and @DonatorEsqInf2  is not Null)
                Begin
                    Set @EsquerdaFinalizada = 'True'
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
      
                --Regra: Caso ele seja um donator n�o pode incluir um novo usuario
                If(@DonatorEsqSup1 = @UsuarioPaiID OR @DonatorEsqSup2 = @UsuarioPaiID OR @DonatorEsqInf1 = @UsuarioPaiID  OR @DonatorEsqInf2 = @UsuarioPaiID)
                Begin
                    Set @PosicaoPai = 'Donator'
                End
                If(@DonatorDirSup1 = @UsuarioPaiID OR @DonatorDirSup2 = @UsuarioPaiID OR @DonatorDirInf1 = @UsuarioPaiID  OR @DonatorDirInf2 = @UsuarioPaiID)
                Begin
                    Set @PosicaoPai = 'Donator'
                End

                    --*********** DONATOR **************
                if(@PosicaoPai = 'Donator')
                Begin
                    --N�o continua o processo se for um donator
                    Set @log = @log + '| 07 N�o continua o processo se for um donator'
                    Set @Continua = 'false'
                    --Obtem Master do usuario pai passado como parametro e usa este master como pai
                    Select Top(1)
                        @MasterTabuleiro = MasterID
                    From 
                        Rede.TabuleiroUsuario
                    Where
                        StatusID = 1 and
                        UsuarioID = @UsuarioPaiID and
                        BoardID = @BoardID

                    --Caso n�o encontre um master com o usuario pai informado, obtem o primeiro master valido e usa este como pai
                    if (@MasterTabuleiro is null Or @MasterTabuleiro = 0)
                    Begin
                        Set @log = @log + '| 08 obtem o primeiro master valido'
                        --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usu�rio nesse tabuleiro
                        Select Top(1)
                            @MasterTabuleiro = UsuarioID 
                        From 
                            Rede.TabuleiroUsuario
                        Where
                            StatusID = 1 and
                            BoardID = @BoardID

                        if(@MasterTabuleiro is null Or @MasterTabuleiro = 0)
                        Begin
                            Set @log = @log + '| 09 � um donator'
                            --Problemas nenhum pai foi encontrado!
                            Set @Historico = '02 Quando o Pai (' + TRIM(STR(@MasterTabuleiro)) + ') � um Donator, n�o � poss�vel adicionar um novo usu�rio. Chamada: ' + @Chamada
                            Set @PosicaoFilho = 'Quando o Pai � um Donator, n�o � poss�vel adicionar um novo usu�rio'
                        End
                        Else 
                        Begin
                            Set @log = @log + '| 10 n�o � um donator'
                            Set @UsuarioPaiID = @MasterTabuleiro
                        End
                    End
                    Else
                    Begin
                        Set @log = @log + '| 11 set UsuarioPaiID'
                        Set @UsuarioPaiID = @MasterTabuleiro
                    End

                    if(@MasterTabuleiro is not Null)
                    Begin
                        Set @log = @log + '| 12 Chama a sp novamente recurssivo'
                        Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @MasterTabuleiro, @BoardID = @BoardID, @Chamada = 'Donator'
                    End
                End
                
                if(@Continua = 'true')
                Begin
                    Set @log = @log + '| 13 Continua'
                    --IndicatorDirSup
                    if (@IndicatorDirSup = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'IndicatorDirSup'
                    End
                    --IndicatorDirInf
                    if (@IndicatorDirInf = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'IndicatorDirInf'
                    End
                    --IndicatorEsqSup            
                    if (@IndicatorEsqSup = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'IndicatorEsqSup'
                    End
                    --IndicatorEsqInf
                    if (@IndicatorEsqInf = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'IndicatorEsqInf'
                    End
                    --CoordinatorDir
                    if (@CoordinatorDir = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'CoordinatorDir'
                    End
                    --CoordinatorEsq
                    if (@CoordinatorEsq = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'CoordinatorEsq'
                    End
                    --Master
                    if (@Master = @UsuarioPaiID )
                    Begin
                        Set @PosicaoPai = 'Master'
                    End

                    --Verifica se novo usuario j� esta no tebuleiro do pai
                    If(@DonatorEsqSup1 = @UsuarioID OR @DonatorEsqSup2 = @UsuarioID OR @DonatorEsqInf1 = @UsuarioID  OR @DonatorEsqInf2 = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'Usu�rio (' + TRIM(STR(@UsuarioID)) + ') j� se encontra no tabuleiro (1)'
                    End
                    If(@DonatorDirSup1 = @UsuarioID OR @DonatorDirSup2 = @UsuarioID OR @DonatorDirInf1 = @UsuarioID  OR @DonatorDirInf2 = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'Usu�rio (' + TRIM(STR(@UsuarioID)) + ') j� se encontra no tabuleiro (2)'
                    End
                    If(@IndicatorDirSup = @UsuarioID OR @IndicatorDirInf = @UsuarioID OR @IndicatorEsqSup = @UsuarioID  OR @IndicatorEsqInf = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'Usu�rio (' + TRIM(STR(@UsuarioID)) + ') j� se encontra no tabuleiro (3)'
                    End
                    If(@CoordinatorDir = @UsuarioID OR @CoordinatorEsq = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'Usu�rio (' + TRIM(STR(@UsuarioID)) + ') j� se encontra no tabuleiro (4)'
                    End
               
                    --Verifica se houve pagamento ao Master
                    If Exists (Select 'OK' From Rede.TabuleiroUsuario Where TabuleiroId = @ID And PagoMaster = 0)
                    Begin
                       Set @log = @log + '| 21 PagoMaster false'
                       Set @PagoMaster = 'false'     
                    End
                    Else 
                    Begin
                        Set @log = @log + '| 22 PagoMaster'
                        --Verifica se as 15 posi�oes est�o ocupadas
                        Select @count = count(*) From Rede.TabuleiroUsuario Where TabuleiroId = @ID
                        if(@count=15)
                        Begin
                            Set @log = @log + '| 22.1 PagoMaster true'
                            Set @PagoMaster = 'true'     
                        End
                        Else
                        Begin
                            Set @log = @log + '| 22.2 PagoMaster false'
                            Set @PagoMaster = 'False'     
                        End
                    End
                  
                    --Verifica se o tabuleiro esta completo
                    if(
                        @Master is not null And 
                        @CoordinatorDir is not null And 
                        @IndicatorDirSup is not null And 
                        @IndicatorDirInf is not null And 
                        @IndicatorEsqSup is not null And 
                        @IndicatorEsqInf is not null And 
                        @DonatorDirSup1 is not null And 
                        @DonatorDirSup2 is not null And 
                        @DonatorDirInf1 is not null And 
                        @DonatorDirInf2 is not null And 
                        @CoordinatorEsq is not null And 
                        @DonatorEsqSup1 is not null And 
                        @DonatorEsqSup2 is not null And 
                        @DonatorEsqInf1 is not null And 
                        @DonatorEsqInf2 is not null And
                        @PosicaoPai <> 'Donator' and
                        @PagoMaster = 'true' and
                        @Chamada = 'Completa'
                    ) 
                    --Tabuleiro completo
                    Begin
                        Set @log = @log + '| 23 TABULEIRO COMPLETO'
                        Set @Historico = '08 - Check Completa true'

                        --Verifica se todos pagaram o Master, para realmente encerrar o Tabuleiro
                        Select
                            @Ciclo = MAX(Ciclo)
                        From
                            Rede.TabuleiroUsuario
                        Where
                            UsuarioID = @UsuarioID and
                            BoardID = @BoardID

                        if(@Ciclo is null)
                            Begin
                                Set @Ciclo = 1
                            End

                        if Not Exists (Select 'Existe' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID = @ID and BoardID = @BoardID And Ciclo = @Ciclo)
                        Begin
                            Set @log = @log + '| 24 Obtem Master do usuario passado como parametro'
                            --Obtem Master do usuario passado como parametro
                            Select Top(1)
                                @MasterTabuleiro = MasterID 
                            From 
                                Rede.TabuleiroUsuario
                            Where
                                StatusID = 1 and
                                UsuarioID = @UsuarioPaiID and
                                BoardID = @BoardID

                            --inclui novo usuario no TabuleiroUsuario
                            Insert Into Rede.TabuleiroUsuario
                            (
                                UsuarioID,
                                TabuleiroID,
                                BoardID,
                                StatusID,
                                MasterID,
                                InformePag,
                                Ciclo,
                                Posicao,
                                PagoMaster,
                                PagoSistema,
                                ConviteProximoNivel,
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
                                0,
                                @Ciclo,
                                @PosicaoFilho,
                                'false',
                                'False',
                                'False',
                                GetDate(),
                                null
                            )

                            if not Exists (Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioID and BoardID = @BoardID and StatusID = 2)
                            Begin
                                Set @log = @log + '| 25 Insert Rede.TabuleiroNivel'
                                Insert Into Rede.TabuleiroNivel
                                (
                                    UsuarioID,
                                    BoardID,
                                    DataInicio,
                                    DataFim,
                                    StatusID,
                                    Observacao
                                )
                                VALUES
                                (
                                    @UsuarioID,
                                    @BoardID,
                                    @DataInicio,
                                    null,
                                    2, -- Ativo no tabuleiro
                                    'Novo Usu�rio (1)'
                                )
                            End
                        End
              
                        --Encerra tabuleiro
                        Set @log = @log + '| 26 Encerra tabuleiro'
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
                        Begin
                            Set @Ciclo = 1
                        End

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

                        Update Rede.TabuleiroNivel
                        Set
                            StatusID = 3, --Finalizado
                            DataFim = @DataFim,
                            Observacao = Observacao + ' | Finalizado.' 
                        Where
                            UsuarioID = @Master and 
                            BoardID = @BoardID and
                            StatusID = 2 --Em andamento

                        --Usuario finalizou o Board 1, este � um convite para ele entrar no sistema no board 1 novamente
                        If (@BoardID = 1)
                        Begin
                            Set @log = @log + '| 27 Convite'
                            --Novo N�vel
                            Insert Into Rede.TabuleiroNivel
                            (
                                UsuarioID,
                                BoardID,
                                DataInicio,
                                DataFim,
                                StatusID,
                                Observacao
                            )
                            VALUES
                            (
                                @Master,
                                1,
                                @DataInicio,
                                null,
                                1,
                                'Convite (1)'
                            )
                        End
                        
                        --**** Cria dois novos tabuleiros ****
                        --Cria Tabuleiro da Direita
                        Insert into Rede.Tabuleiro
                        (
                            BoardID,
                            StatusID,
                            Master,
                            CoordinatorDir,
                            CoordinatorEsq,
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
                            1, --StatusId = 1 � ativo
                            @CoordinatorDir, --Master
                            @IndicatorDirSup, --CoordinatorDir
                            @IndicatorDirInf, --CoordinatorEsq
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
                        Set @log = @log + '| 28 Posicao no tabuleiro'
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'Master',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @CoordinatorDir And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'CoordinatorDir',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @IndicatorDirSup And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'CoordinatorEsq',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @IndicatorDirInf And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorDirSup',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @DonatorDirSup1 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorDirInf',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @DonatorDirSup2 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorEsqSup',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
                        Where
                            UsuarioID = @DonatorDirInf1 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorEsqInf',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorDir
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
                            CoordinatorDir,
                            CoordinatorEsq,
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
                            1, --StatusId = 1 � ativo
                            @CoordinatorEsq, --Master
                            @IndicatorEsqSup, --CoordinatorDir
                            @IndicatorEsqInf, --CoordinatorEsq
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
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @CoordinatorEsq And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'CoordinatorDir',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @IndicatorEsqSup And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'CoordinatorEsq',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @IndicatorEsqInf And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorDirSup',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @DonatorEsqSup1 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorDirInf',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @DonatorEsqSup2 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorEsqSup',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @DonatorEsqInf1 And
                            StatusID = 1 and
                            BoardID = @BoardID
                        Update
                            Rede.TabuleiroUsuario
                        Set
                            Posicao = 'IndicatorEsqInf',
                            TabuleiroID = @Identity,
                            MasterID = @CoordinatorEsq
                        Where
                            UsuarioID = @DonatorEsqInf2 And
                            StatusID = 1 and
                            BoardID = @BoardID

                        --*********** Promove Usuario Master para novo Board ***********
                        --Sobe para proximo Board
                        Set @BoardID = @BoardID + 1
                        --Verifica se ainda h� board acima do master
                        IF Not Exists (Select 'Existe' From Rede.TabuleiroBoard Where ID = @BoardID)
                        Begin
                            Set @log = @log + '| 29 Sem board Superior'
                            --Caso n�o haja mais board superiores volta ao inicio
                            Set @BoardID = 1
                        End
                  
                        --Obtem Master do usuario passado como parametro e inclui o novo usu�rio nesse tabuleiro
                        Select Top(1)
                            @UsuarioPaiID = MasterID 
                        From 
                            Rede.TabuleiroUsuario
                        Where
                            UsuarioID = @Master and 
                            TabuleiroID = @ID and 
                            BoardID = @BoardID and
                            StatusID = 1

                        --Caso n�o encontre um Pai obtem o primeiro pai disponivel
                        if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
                        Begin
                            Set @log = @log + '| 30 Obtem primeiro Master ativo'
                            --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usu�rio nesse tabuleiro
                            Select Top(1)
                                @UsuarioPaiID = UsuarioID 
                            From 
                                Rede.TabuleiroUsuario
                            Where
                                StatusID = 1 and
                                BoardID = @BoardID

                            if(@UsuarioPaiID is null Or @UsuarioPaiID = 0)
                            Begin
                                if not Exists (Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioPaiID and BoardID = @BoardID and StatusID = 1)
                                Begin
                                    Set @log = @log + '| 31 N�o foi poss�vel encontrar um master'
                                    Set @Historico = '03 N�o foi poss�vel encontrar um master para o usuario: ' + TRIM(STR(@UsuarioPaiID)) + ' para o Board: ' + TRIM(STR(@BoardID)) + '. Chamada: ' + @Chamada
                                    INSERT INTO Rede.TabuleiroNivel (UsuarioID, BoardID, DataInicio, DataFim, StatusID, Observacao) VALUES (@UsuarioPaiID, @BoardID, @DataInicio, null, 1, 'Convite (2)')
                                    Set @PosicaoFilho = '***'
                                End
                            End
                            Else
                            Begin
                                if not Exists(Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioPaiID and BoardID = @BoardID and StatusID = 1)
                                Begin
                                    Set @log = @log + '| 32 Inclui master em novo com novo ciclo 1'
                                    --Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (1)
                                    INSERT INTO Rede.TabuleiroNivel (UsuarioID, BoardID, DataInicio, DataFim, StatusID, Observacao) VALUES (@UsuarioPaiID, @BoardID, @DataInicio, null, 1,'Convite (3)')
                                End
                            End
                        End
                        Else
                        Begin
                            if not Exists( Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioPaiID and BoardID = @BoardID)
                            Begin
                                Set @log = @log + '| 33 Inclui master em novo com novo ciclo 2'
                                --Inclui master em novo com novo ciclo e em novo tabuleiro em board superior (2)
                                INSERT INTO Rede.TabuleiroNivel (UsuarioID, BoardID, DataInicio, DataFim, StatusID, Observacao) VALUES (@UsuarioPaiID, @BoardID, @DataInicio, null, 1,'Convite (4)')
                            End
                        End
                    End
                    Else
                    --Tabuleiro incompleto
                    Begin
                        if(@Chamada <> 'Completa')
                        Begin
                            Set @log = @log + '| 34 TABULEIRO INCOMPLETO'

                            --Verifica se tabuleiro possui possi��es livres
                             if(
                                @Master is not null And 
                                @CoordinatorDir is not null And 
                                @IndicatorDirSup is not null And 
                                @IndicatorDirInf is not null And 
                                @IndicatorEsqSup is not null And 
                                @IndicatorEsqInf is not null And 
                                @DonatorDirSup1 is not null And 
                                @DonatorDirSup2 is not null And 
                                @DonatorDirInf1 is not null And 
                                @DonatorDirInf2 is not null And 
                                @CoordinatorEsq is not null And 
                                @DonatorEsqSup1 is not null And 
                                @DonatorEsqSup2 is not null And 
                                @DonatorEsqInf1 is not null And 
                                @DonatorEsqInf2 is not null 
                            ) 
                            Begin
                                Set @log = @log + '| 34.1 Posic�es est�o ocupadas'
                                Set @Historico = '04 N�o h� posi��es livres no momento para o usuario: ' + TRIM(STR(@UsuarioID)) + ' no Tabuleiro: ' +TRIM(STR(@ID)) + ' no BoardID: ' + TRIM(STR(@BoardID))
                            End
                            Else
                            Begin
                                Set @log = @log + '| 34.2 H� posi��es livres'
                            
                                --*********INICIO UPDATES***********

                                --*********** MASTER **************
                                if(@PosicaoPai = 'Master' Or @DireitaFinalizada = 'true' Or  @EsquerdaFinalizada = 'true')
                                Begin
                                    --*********** COORDINATOR **************
                                    Set @log = @log + '| 14 COORDINATOR'
                                    --Verifica se h� coordinator, caso n�o inclui usuario como coordinator na direita
                                    if (@CoordinatorDir is null and @Incluido = 'false')
                                        Begin
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                CoordinatorDir = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @CoordinatorDir = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'CoordinatorDir'
                                        End
                                    --Verifica se h� coordinator, caso n�o inclui usuario como coordinator na esquerda
                                    if (@CoordinatorEsq is null and @Incluido = 'false')
                                        Begin
                                        Update
                                            Rede.Tabuleiro
                                        Set
                                            CoordinatorEsq = @UsuarioID
                                        Where
                                            ID = @ID
               
                                        Set @CoordinatorEsq = @UsuarioID
                                        Set @Incluido = 'true'
                                        Set @PosicaoFilho = 'CoordinatorEsq'
                                        End
                                End --Master
            
                                --*********** COORDINATOR DIREITA **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'  Or @IndicadorDireitaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 15 COORDINATOR DIREITA'
                                    --Verifica se h� Indicator, caso n�o inclui usuario como indicator superior direita
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
                                    --Verifica se h� Indicator, caso n�o inclui usuario como indicator inferior direita
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'  Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 16 COORDINATOR ESQUERDA'
                                    --Verifica se h� Indicator, caso n�o inclui usuario como indicator superior esquerda
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
                                    --Verifica se h� Indicator, caso n�o inclui usuario como indicator inferior esquerda
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 17 INDICATOR DIREITA Superior'
                                    if (@DonatorDirSup1 is null and @Incluido = 'false')
                                        Begin
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or @PosicaoPai = 'IndicatorDirInf' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 18 INDICATOR DIREITA Inferior'
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @DireitaFinalizada = 'true' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 19 INDICATOR ESQUERDA Superior'
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @PosicaoPai = 'IndicatorEsqInf' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 20 INDICATOR ESQUERDA Superior'
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

                                --*********FIM UPDATES***********

                                Select
                                    @Ciclo = MAX(Ciclo)
                                From
                                    Rede.TabuleiroUsuario
                                Where
                                    UsuarioID = @UsuarioID and
                                    BoardID = @BoardID

                                if(@Ciclo is null)
                                Begin
                                    Set @Ciclo = 1
                                End

                                if Not Exists (Select 'Existe' From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID = @ID and BoardID = @BoardID And Ciclo = @Ciclo)
                                Begin
                                    Set @log = @log + '| 35 Obtem Master do usuario'
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
                                        InformePag,
                                        Ciclo,
                                        Posicao,
                                        PagoMaster,
                                        PagoSistema,
                                        ConviteProximoNivel,
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
                                        0,
                                        @Ciclo,
                                        Coalesce(@PosicaoFilho,@PosicaoFilho,'1'),
                                        'false',
                                        'False',
                                        'False',
                                        GetDate(),
                                        null
                                    )

                                    if not Exists (Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioID and BoardID = @BoardID and StatusID = 1)
                                    Begin
                                        Set @log = @log + '| 36 insert Into Rede.TabuleiroNivel'
                                        Insert Into Rede.TabuleiroNivel
                                        (
                                            UsuarioID,
                                            BoardID,
                                            DataInicio,
                                            DataFim,
                                            StatusID,
                                            Observacao
                                        )
                                        VALUES
                                        (
                                            @UsuarioID,
                                            @BoardID,
                                            @DataInicio,
                                            null,
                                            2, --inclus�o normal
                                            'Novo Usu�rio (2)'
                                        )
                                    End
                                End
                            End
                        End
                        Else 
                        Begin
                            Set @Historico = '07 - Check Completa false'
                        End
                    End
                End
            End
            Else 
            Begin
                Set @log = @log + '| 05.1 #temp n�o tem conteudo'
                if(@Chamada <> 'Completa')
                Begin
                    --Caso N�O exista o tabuleiro com o board informado
                    Set @log = @log + '| 37 Caso N�O exista o tabuleiro com o board informado'
                    --Verifica se h� usuario ativo no board corrente
                    if Exists( Select 'Existe' From Rede.TabuleiroUsuario Where StatusID = 1 and BoardID = @BoardID)
                    Begin
                        Set @log = @log + '| 38 Obtem primeiro Master ativo '
                        --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usu�rio nesse tabuleiro
                        Select Top(1)
                            @MasterTabuleiro = UsuarioID 
                        From 
                            Rede.TabuleiroUsuario
                        Where
                            StatusID = 1 and
                            BoardID = @BoardID
            
                        if(@MasterTabuleiro is null)
                        Begin
                            Set @log = @log + '| 39 Master � null'
                            Set @Historico = '05 Usu�rio pai (' + TRIM(STR(@MasterTabuleiro)) + ') n�o existe! Chamada: ' + @Chamada
                        End
                        Else
                        Begin
                            Set @log = @log + '| 40 Chama novamente essa sp recursivo UsuarioID: ' + TRIM(STR(@UsuarioID)) + ' Master: ' + TRIM(STR(@MasterTabuleiro)) + ' BoardID: ' + TRIM(STR(@BoardID))
                            --Chama novamente essa sp, agora com um pai valido
                            if(@Chamada = 'Convite')
                            Begin
                                --J� estando no tabuleiro seta para StatusID = 2 --(J� incluido)
                                Set @log = @log + '| 43.1 Chamada � convite: BoardID=' + TRIM(STR(@BoardID))

                                Delete
                                    Rede.TabuleiroNivel
                                Where
                                    UsuarioID = @UsuarioID And
                                    BoardID = @BoardID And
                                    StatusID = 1 --Aguardando ser incluido

                                Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @MasterTabuleiro, @BoardID = @BoardID, @Chamada = @Chamada    
                            End
                            Else
                            Begin
                                Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @MasterTabuleiro, @BoardID = @BoardID, @Chamada = 'PaiValido'
                            End
                        End
                    End
                    Else
                    Begin
                        Set @log = @log + '| 41 N�o existindo usuario no board'
                        --N�o existindo usuario no board corrente cria um novo
                        Insert into Rede.Tabuleiro
                        (
                            BoardID,
                            StatusID,
                            Master,
                            CoordinatorDir,
                            CoordinatorEsq,
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
                            1, --StatusId = 1 � ativo
                            @UsuarioID, --Master
                            Null, --CoordinatorDir
                            Null, --CoordinatorEsq
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
                            InformePag,
                            Ciclo,
                            Posicao,
                            PagoMaster,
                            PagoSistema,
                            ConviteProximoNivel,
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
                            0,
                            @Ciclo,
                            'Master',
                            'false',
                            'False',
                            'False',
                            GetDate(),
                            null
                        )

                        if not Exists (Select 'Existe' From Rede.TabuleiroNivel Where UsuarioID = @UsuarioID and BoardID = @BoardID and StatusID = 1)
                        Begin
                            Set @log = @log + '| 42 Into Rede.TabuleiroNivel'
                            Insert Into Rede.TabuleiroNivel
                            (
                                UsuarioID,
                                BoardID,
                                DataInicio,
                                DataFim,
                                StatusID,
                                Observacao
                            )
                            VALUES
                            (
                                @UsuarioID,
                                @BoardID,
                                @DataInicio,
                                null,
                                1,
                                'Novo Usu�rio (3)'
                            )
                        End
                    End
                End
            End
        End
        Else
        Begin
            Set @log = @log + '| 43 Usuario n�o cadastrado'
            Set @Historico = '06 Novo usu�rio ' + TRIM(STR(@UsuarioID)) + ' n�o est� cadastrado! Chamada: ' + @Chamada
        End
    End

    --No Debug
    --set @log = null

    Insert Into Rede.TabuleiroLog 
    (
        UsuarioID,
        UsuarioPaiID,
        BoardID,
        Data,
        Mensagem,
        Debug
    )
    Values
    (
        Coalesce(@UsuarioID,@UsuarioID,0),
        Coalesce(@UsuarioPaiID,@UsuarioPaiID,0),
        Coalesce(@BoardID,@BoardID,0),
        CONVERT(VARCHAR(8),GETDATE(),112),
        Coalesce(@Historico,@Historico,''),
        Coalesce(@log,@log,'')
    )
        
    if(@Historico is null or @Historico = '')
    Begin
        if(@Chamada <> 'PaiValido')
        Begin
            Select 
                'OK' as Retorno, 
                @UsuarioID as UsuarioID, 
                @UsuarioPaiID as UsuarioPaiID, 
                @ID TabuleiroID, 
                @BoardID as BoardID, 
                COALESCE(@PosicaoFilho,@PosicaoFilho,'') as Posicao, 
                '' as Historico, 
                '' as Debug,
                @chamada as Chamada
        End
    End
    Else
    Begin
        if(@Chamada <> 'PaiValido')
        Begin
            Select 
                'NOOK' as Retorno, 
                @UsuarioID as UsuarioID, 
                @UsuarioPaiID as UsuarioPaiID, 
                @ID TabuleiroID, 
                @BoardID as BoardID, 
                COALESCE(@PosicaoFilho,@PosicaoFilho,'') as Posicao,  
                @Historico as Historico, 
                @log as Debug, 
                @chamada as Chamada
        End
    End
   
    Set @Historico = ''
   
    Drop Table #temp

End -- Sp

go
Grant Exec on spG_Tabuleiro To public
go

--exec spG_Tabuleiro @UsuarioID = 2596, @UsuarioPaiID =2580, @BoardID = 1, @Chamada = 'Principal'
--exec spG_Tabuleiro @UsuarioID = 2596, @UsuarioPaiID =2580, @BoardID = 1, @Chamada = 'Completa'

