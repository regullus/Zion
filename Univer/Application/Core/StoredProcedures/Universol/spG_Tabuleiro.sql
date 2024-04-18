use UniverDev
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
    'Principal, para incluir um novo usuario'
    'Donator', quando um UsuariiPaiID passado eh um Donator que nao pode ser pai de ninguem, dai seleciona um pai valido, nao USAR EXTERNAMENTE eh uma chamada recurssiva desta sp
    'Convite' quando um usuario jah eh veterano e eh convidado para entrar novamente nos tabuleiros, nao USAR EXTERNAMENTE eh uma chamada  recurssiva desta sp
    'PaiValido' chama novamente a essa sp com um pai valido, ocorre quando nao eh passado um pai valido no sistema, nao USAR EXTERNAMENTE eh uma chamada  recurssiva desta sp
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
            @PagoMasterDireita bit,
            @PagoMasterEsquerda bit,
			@PagoMaster bit,
            @log nvarchar(max),
            @count int,
            @direitaFechada bit,
            @esquerdaFechada bit,
            @tabuleiroFechado bit,
            @BoardIDProximo int,
            @aux int
           
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
    Set @PagoMasterDireita = 'false'
    Set @PagoMasterEsquerda = 'false'
	Set @PagoMaster = 'false'
    Set @log = 'Inicio |'
    Set @Historico = ''
    Set @direitaFechada = 'false'
    Set @esquerdaFechada = 'false'
    Set @tabuleiroFechado = 'false'
    Set @BoardIDProximo = 0
    Set @aux =0

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

    Set @log = @log + 'Usuario: ' + TRIM(STR(@UsuarioID))

    --Ferifica se novo usuario jah se encontra em algum tabuleiro
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
        --Regra: Caso usuario jah exista no tabuleiro, nao se pode inclui-lo novamente
        Set @Historico = '01 usuario (' + TRIM(STR(@UsuarioID)) + ') jah se encontra no tabuleiro (0). Chamada: ' + @Chamada
        Set @log = @log + '|01 jah se encontra no tabuleiro'
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
                Set @log = @log + '| 03 UsuarioPaiID eh null - #temp Criada'
                --Caso @UsuarioPaiID seja null, obtem primeiro tabuleiro disponivel no board passado como paramentro
                Insert Into #temp
                Select top(1)
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
                Set @log = @log + '| 04 UsuarioPaiID nao eh null: ' + TRIM(STR(@UsuarioPaiID)) + ' BoardID='+ TRIM(STR(@BoardID))
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
                --Determina qual a posicao do pai no board
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
      
                --Regra: Caso ele seja um donator nao pode incluir um novo usuario
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
				    --***Donator nao pode incluir um usuario, dai busca um pai valido, se possivel, para usar na inclussao do novo usuario***

                    --nao continua o processo se for um donator
                    Set @log = @log + '| 07 DONATOR'
                    Set @Continua = 'false'

                    --Obtem Master do usuario pai passado como parametro e usa este master como pai
                    Select Top(1)
                        @MasterTabuleiro = MasterID
                    From 
                        Rede.TabuleiroUsuario
                    Where
                        UsuarioID = @UsuarioPaiID and
                        BoardID = @BoardID

                    --Caso nao encontre um master com o usuario pai informado, obtem o primeiro master valido e usa este como pai
                    if (@MasterTabuleiro is null Or @MasterTabuleiro = 0)
                    Begin
                        Set @log = @log + '| 08 obtem o primeiro master valido'
                        
						--Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuario nesse tabuleiro
                        Select Top(1)
                            @MasterTabuleiro = UsuarioID 
                        From 
                            Rede.TabuleiroUsuario
                        Where
                            BoardID = @BoardID and
							StatusID = 1 --Ativo

                        if(@MasterTabuleiro is null Or @MasterTabuleiro = 0)
                        Begin
                            Set @log = @log + '| 09 eh um donator'
                            --Problemas nenhum pai foi encontrado!
                            Set @Historico = '02 Quando o Pai (' + TRIM(STR(@MasterTabuleiro)) + ') eh um Donator, nao eh possivel adicionar um novo usuario. Chamada: ' + @Chamada
                            Set @PosicaoFilho = 'Quando o Pai eh um Donator, nao eh possivel adicionar um novo usuario'
                        End
                        Else 
                        Begin
                            Set @log = @log + '| 10 nao eh um donator'
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
                        Set @log = @log + '| 12 Chama a sp novamente recursivo, agora com um pai valido jah que o antigo era um donator'
                        Set @Historico = '09.2 @UsuarioID=' + TRIM(STR(@UsuarioID)) + ',@UsuarioPaiID=' + TRIM(STR(@MasterTabuleiro)) + ',@BoardID=' + TRIM(STR(@BoardID))
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

                    --Verifica se novo usuario jah esta no tabuleiro do pai
                    If(@DonatorEsqSup1 = @UsuarioID OR @DonatorEsqSup2 = @UsuarioID OR @DonatorEsqInf1 = @UsuarioID  OR @DonatorEsqInf2 = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'usuario (' + TRIM(STR(@UsuarioID)) + ') jah se encontra no tabuleiro (1)'
                    End
                    If(@DonatorDirSup1 = @UsuarioID OR @DonatorDirSup2 = @UsuarioID OR @DonatorDirInf1 = @UsuarioID  OR @DonatorDirInf2 = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'usuario (' + TRIM(STR(@UsuarioID)) + ') jah se encontra no tabuleiro (2)'
                    End
                    If(@IndicatorDirSup = @UsuarioID OR @IndicatorDirInf = @UsuarioID OR @IndicatorEsqSup = @UsuarioID  OR @IndicatorEsqInf = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'usuario (' + TRIM(STR(@UsuarioID)) + ') jah se encontra no tabuleiro (3)'
                    End
                    If(@CoordinatorDir = @UsuarioID OR @CoordinatorEsq = @UsuarioID)
                    Begin
                        Set @PosicaoPai = 'usuario (' + TRIM(STR(@UsuarioID)) + ') jah se encontra no tabuleiro (4)'
                    End
               
                    Set @log = @log + '| 13.1 TabuleiroID: ' + TRIM(STR(@ID)) + ' Master: ' + TRIM(STR(@Master))
                    
                    --Verifica se houve pagamento ao Master na Direita
                    If Exists ( 
                        Select 
                            'OK' 
                        From 
                            Rede.TabuleiroUsuario 
                        Where 
                            BoardID = @BoardID and
							MasterID = @Master and
                            PagoMaster = 0 and 
                            (
                                Posicao = 'DonatorDirSup1' or
                                Posicao = 'DonatorDirSup2' or
                                Posicao = 'DonatorDirInf1' or
                                Posicao = 'DonatorDirInf2' 
                            )
                    )
                    Begin
                       Set @log = @log + '| 21 PagoMaster false direita'
                       --Select 'panda 04 [' + @log +']'
                       Set @PagoMasterDireita = 'false'     
                    End
                    Else 
                    Begin
                        --Verifica se as 4 posicoes de donators da direita estao ocupadas
                        Set @count = 0
                        Select 
                            @count = count(*) 
                        From 
                            Rede.TabuleiroUsuario 
                        Where 
                            BoardID = @BoardID And 
							MasterID = @Master and
							(
                                Posicao = 'DonatorDirSup1' or
                                Posicao = 'DonatorDirSup2' or
                                Posicao = 'DonatorDirInf1' or
                                Posicao = 'DonatorDirInf2' 
                            )
                        Set @log = @log + '| 22 PagoMaster direita count: ' + TRIM(STR(@count))
                        if(@count=4)
                        Begin
                            Set @log = @log + '| 22.1 PagoMaster direita true'
                            Set @PagoMasterDireita = 'true'     
                        End
                        Else
                        Begin
                            Set @log = @log + '| 22.2 PagoMaster direita false'
                            Set @PagoMasterDireita = 'false'     
                        End
                    End

                    --Verifica se houve pagamento ao Master na Esquerda
                    If Exists ( 
                        Select 
                            'OK' 
                        From 
                            Rede.TabuleiroUsuario 
                        Where 
                            BoardID = @BoardID and
							MasterID = @Master and
                            PagoMaster = 0 and 
                            (
                                Posicao = 'DonatorEsqSup1' or
                                Posicao = 'DonatorEsqSup2' or
                                Posicao = 'DonatorEsqInf1' or
                                Posicao = 'DonatorEsqInf2' 
                            )
                    )
                    Begin
                       Set @log = @log + '| 21 PagoMaster esquerda false esquerda'
                       Set @PagoMasterEsquerda = 'false'     
                    End
                    Else 
                    Begin
                        --Verifica se as 4 posicoes de donators da  esquerda estao ocupadas
                        Set @count = 0
                        Select 
                            @count = count(*) 
                        From 
                            Rede.TabuleiroUsuario 
                        Where 
                            BoardID = @BoardID and
							MasterID = @Master and
							(
                                Posicao = 'DonatorEsqSup1' or
                                Posicao = 'DonatorEsqSup2' or
                                Posicao = 'DonatorEsqInf1' or
                                Posicao = 'DonatorEsqInf2' 
                            )
                        Set @log = @log + '| 22 PagoMaster  esquerda count: ' + TRIM(STR(@count))
                        if(@count=4)
                        Begin
                            Set @log = @log + '| 22.1 PagoMaster esquerda true'
                            Set @PagoMasterEsquerda = 'true'     
                        End
                        Else
                        Begin
                            Set @log = @log + '| 22.2 PagoMaster esquerda false'
                            Set @PagoMasterEsquerda = 'false'     
                        End
                    End

                    --Verificar se lado esquerdo e lado direito fecharam
                    Select
                        @direitaFechada = DireitaFechada,
                        @esquerdaFechada = EsquerdaFechada
                    From
                        Rede.TabuleiroUsuario
                    Where
                        UsuarioID = @Master and
                        BoardID = @BoardID
                    
                    --**************** Verifica se o tabuleiro esta completo na Direita **************** 
                    if(
                        @Master is not null And 
                        @CoordinatorDir is not null And 
                        @IndicatorDirSup is not null And 
                        @IndicatorDirInf is not null And 
                        @DonatorDirSup1 is not null And 
                        @DonatorDirSup2 is not null And 
                        @DonatorDirInf1 is not null And 
                        @DonatorDirInf2 is not null And 
                        @PosicaoPai <> 'Donator' and
                        @PagoMasterDireita = 'true' and
                        @Chamada = 'Completa'
                    ) 
                    --Tabuleiro completo DITEITA
                    Begin
                        Set @log = @log + '| 23.1 TABULEIRO COMPLETO DIREITA'
                        Set @Historico = '08.1 - Check Completa true DIREITA'
                        
                        if (@direitaFechada = 'false')
                        Begin
                            Set @log = @log + '| 23.2.1 Update TABULEIRO DIREITA'

							Update 
								Rede.TabuleiroUsuario 
							Set
								DireitaFechada = 'true'
							Where
								UsuarioID = @Master and
								BoardID = @BoardID 

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
                                1, --StatusId = 1 eh ativo
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
               
                            Set @log = @log + '| 28.1 Altera Posicao no tabuleiro: ' + TRIM(STR(@Identity))
                            --inclui novo usuario no TabuleiroUsuario 1

                            --Update posicao no TabuleiroUsuario do pessoal da Esquerda
                            Select
                                @Ciclo = MAX(Ciclo) + 1
                            From
                                Rede.TabuleiroUsuario
                            Where
                                UsuarioID = @UsuarioID and
                                BoardID = @BoardID

                            if(@Ciclo is null)
                            Begin
                                Set @Ciclo = 1
                            End
							
							--Master
                            Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'Master',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update Master'
							Where
								UsuarioID = @CoordinatorDir and --Ele vira o Master
								BoardID = @BoardID

                            --CoordinatorDir
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'CoordinatorDir',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update CoordinatorDir'
							Where
								UsuarioID = @IndicatorDirSup and --Ele vira o CoordinatorDir
								BoardID = @BoardID

                            --CoordinatorEsq
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'CoordinatorEsq',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update CoordinatorDir'
							Where
								UsuarioID = @IndicatorDirInf and --Ele vira o CoordinatorEsq
								BoardID = @BoardID
                            
                            --IndicatorDirSup
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorDirSup',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update IndicatorDirSup'
							Where
								UsuarioID = @DonatorDirSup1 and --Ele vira o CoordinatorEsq
								BoardID = @BoardID
                            
                            --IndicatorDirInf
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorDirInf',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update IndicatorDirInf'
							Where
								UsuarioID = @DonatorDirSup2 and --Ele vira o CoordinatorEsq
								BoardID = @BoardID

                            --IndicatorEsqSup
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorEsqSup',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update IndicatorEsqSup'
							Where
								UsuarioID = @DonatorDirInf1 and --Ele vira o CoordinatorEsq
								BoardID = @BoardID

                            --IndicatorEsqInf
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId = 1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorDir, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorEsqInf',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Direita Fechada - Update IndicatorEsqInf'
							Where
								UsuarioID = @DonatorDirInf2 and --Ele vira o CoordinatorEsq
								BoardID = @BoardID
                                                       
                            Set @direitaFechada = 'true'
                        End
                    End
                    
                    --**************** Verifica se o tabuleiro esta completo na Esquerda **************** 
                    if(
                        @Master is not null And 
                        @IndicatorEsqSup is not null And 
                        @IndicatorEsqInf is not null And 
                        @CoordinatorEsq is not null And 
                        @DonatorEsqSup1 is not null And 
                        @DonatorEsqSup2 is not null And 
                        @DonatorEsqInf1 is not null And 
                        @DonatorEsqInf2 is not null And
                        @PosicaoPai <> 'Donator' and
                        @PagoMasterEsquerda = 'true' and
                        @Chamada = 'Completa'
                    ) 
                    --Tabuleiro completo ESQUERDA
                    Begin
                        Set @log = @log + '| 23.2 TABULEIRO COMPLETO ESQUERDA'
                        Set @Historico = '08.2 - Check Completa true ESQUERDA'
                        
                        if(@esquerdaFechada = 'false')
                        Begin
                            Set @log = @log + '| 23.2.1 Update TABULEIRO ESQUERDA'
                            --Seta true para todos os usuarios do tabuleiro, informando que o lado direito esta finalizado
                            
                            Set @log = @log + '| 23.2.3 Cria novo tabuleiro'

							Update 
								Rede.TabuleiroUsuario 
							Set
								EsquerdaFechada = 'true'
							Where
								UsuarioID = @Master and
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
                                1, --StatusId = 1 eh ativo
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
                            Select
                                @Ciclo = MAX(Ciclo) + 1
                            From
                                Rede.TabuleiroUsuario
                            Where
                                UsuarioID = @UsuarioID and
                                BoardID = @BoardID

                            if(@Ciclo is null)
                            Begin
                                Set @Ciclo = 1
                            End

                            --Master
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'Master',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update Master'
							Where
								UsuarioID = @CoordinatorEsq and --Ele vira o Master
								BoardID = @BoardID

                            --CoordinatorDir
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'CoordinatorDir',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update CoordinatorDir'
							Where
								UsuarioID = @IndicatorEsqSup and --Ele vira o Master
								BoardID = @BoardID

                            --CoordinatorEsq
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'CoordinatorEsq',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update CoordinatorEsq'
							Where
								UsuarioID = @IndicatorEsqInf and --Ele vira o Master
								BoardID = @BoardID
                            
                            --IndicatorDirSup
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorDirSup',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update CoordinatorEsq'
							Where
								UsuarioID = @DonatorEsqSup1 and --Ele vira o IndicatorDirSup
								BoardID = @BoardID
                            
                            --IndicatorDirInf
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorDirInf',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update IndicatorDirInf'
							Where
								UsuarioID = @DonatorEsqSup2 and --Ele vira o IndicatorDirSup
								BoardID = @BoardID

                            --IndicatorEsqSup
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorEsqSup',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update IndicatorEsqSup'
							Where
								UsuarioID = @DonatorEsqInf1 and --Ele vira o IndicatorDirSup
								BoardID = @BoardID

                            --IndicatorEsqInf
							Update
								Rede.TabuleiroUsuario 
							Set
                                TabuleiroID = @Identity,
								StatusId =1, --Ativo
								informePag = 1, 
                                MasterID = @CoordinatorEsq, --Fixo pois o CoordinatorDir vira o master
                                Ciclo = coalesce(@Ciclo,@Ciclo,1),
                                Posicao = 'IndicatorEsqInf',
                                PagoMaster = 'true',
                                PagoSistema = 'true',
                                DireitaFechada = 'false',
                                EsquerdaFechada = 'false',
								DataInicio = GetDate(),
                                Debug = 'Esquerda Fechada - Update IndicatorEsqSup'
							Where
								UsuarioID = @DonatorEsqInf2 and --Ele vira o IndicatorEsqInf
								BoardID = @BoardID

                            Set @esquerdaFechada = 'true'
                        End
                    End
                    
                    Set @log = @log + '| 40.0 Verifica se encerra tabuleiro direitaFechada: ' + TRIM(STR(@direitaFechada)) + ' esquerdaFechada: ' + TRIM(STR(@esquerdaFechada))
                    
                    --Encerra tabuleiro
                    if (@direitaFechada = 'true' and @esquerdaFechada = 'true')
                    Begin
                        --*************** Encerra tabuleiro Inicio ****************    
                        --Verifica se tabuleiro jah esta fechado
                        Set @log = @log + '| 40.1 Modo Encerra tabuleiro'

                        Set @tabuleiroFechado = 'false'
                        
                        --Caso o tabuleiro esteja fechado:
                        Select
                            @tabuleiroFechado = 'true'
                        From
                            Rede.Tabuleiro
                        Where
                            ID = @ID and
                            StatusID = 2 --Finalizado
                        
                        Set @log = @log + '| 41.0 Verifica se tabuleiro jah esta fechado: ' + TRIM(STR(@tabuleiroFechado))
                        
						if(@tabuleiroFechado = 'false')
                        Begin
                            Set @log = @log + '| 26 Encerra tabuleiro'

                            Update
                                Rede.Tabuleiro
                            Set
                                StatusId = 2, --Finalizado
                                DataFim = CONVERT(VARCHAR(8),GETDATE(),112)
                            Where
                                ID = @ID

							--Zera Inf de fechamento, pois usuarios jah estao em outro tabuleiro
							Update
								Rede.TabuleiroUsuario
							Set
								DireitaFechada = 'false',
								EsquerdaFechada = 'false'
							Where
								UsuarioID = @Master and
								BoardID = @BoardID
							
                            --Usuario finalizou o Board 1, este eh um convite para ele entrar no sistema no board 1 novamente
                            If (@BoardID = 1)
                            Begin
                                Set @log = @log + '| 27 Convite para: Master: ' + TRIM(STR(@UsuarioPaiID))
                                
								--Master
								Update
									Rede.TabuleiroUsuario 
								Set
									StatusId = 2, --Convite
									Debug = 'Tabuleiro Fechado - Convite (1)'
								Where
									UsuarioID = @Master and --Ele vira o Master
									BoardID = @BoardID
                            End
						End
					   --*************** Encerra tabuleiro Fim ****************
                    End
                    Else
                    --Tabuleiro incompleto
                    Begin
                        if(@Chamada <> 'Completa')
                        Begin
                            Set @log = @log + '| 34 TABULEIRO INCOMPLETO'
                            --Verifica se tabuleiro possui posicoes livres
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
                                Set @log = @log + '| 34.1 Posicoes estao ocupadas'
                                Set @Historico = '04 nao ha Posicoes livres no momento para o usuario: ' + TRIM(STR(@UsuarioID)) + ' no Tabuleiro: ' +TRIM(STR(@ID)) + ' no BoardID: ' + TRIM(STR(@BoardID))
                            End
                            Else
                            Begin
                                Set @log = @log + '| 34.2 ha Posicoes livres para o TabuleiroID=' + TRIM(STR(@ID))
                            
                                --*********INICIO UPDATES***********

                                --*********** MASTER **************
                                if(@PosicaoPai = 'Master')
                                Begin
                                    Set @log = @log + '| 34.3.1 Pai eh master:' + @PosicaoPai
                                    --*********** COORDINATOR **************
                                    --Verifica se ha coordinator, caso nao inclui usuario como coordinator na direita
                                    if (@CoordinatorDir is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 14.1 COORDINATOR DIREITA'
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
                                    --Verifica se ha coordinator, caso nao inclui usuario como coordinator na esquerda
                                    if (@CoordinatorEsq is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 14.1 COORDINATOR ESQUERDA'
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
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir'))
                                Begin
                                    Set @log = @log + '| 34.3.2 Pai eh master ou CoordinatorDir:' + @PosicaoPai
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator superior direita
                                    if (@IndicatorDirSup is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 15.1 INDICATOR DIREITA SUP'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorDirSup = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorDirSup = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorDirSup'
											Set @PagoMaster = 'true'
                                        End
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator inferior direita
                                    if (@IndicatorDirInf is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 15.1 INDICATOR DIREITA Inf'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorDirInf = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorDirInf = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorDirInf'
											Set @PagoMaster = 'true'
                                        End
                                End
            
                                --*********** COORDINATOR ESQUERDA **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq'))
                                Begin
                                    Set @log = @log + '| 34.3.3 Pai eh master ou CoordinatorEsq:' + @PosicaoPai
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator superior esquerda
                                    if (@IndicatorEsqSup is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 16.1 INDICATOR ESQUERDA Sup'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorEsqSup = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorEsqSup = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorEsqSup'
											Set @PagoMaster = 'true'
                                        End
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator inferior esquerda
                                    if (@IndicatorEsqInf is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 16.1 INDICATOR ESQUERDA Inf'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorEsqInf = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorEsqInf = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorEsqInf'
											Set @PagoMaster = 'true'
                                        End
                                End
            
                                --*********** INDICATOR DIREITA Superior ************** 
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup'))
                                Begin
                                    Set @log = @log + '| 34.3.4 Pai eh master ou CoordinatorEsq ou CoordinatorDir ou IndicatorDirSup:' + @PosicaoPai
                                    if (@DonatorDirSup1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 17.1 DONATOR DIREITA Sup 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirSup1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirSup1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirSup1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorDirSup2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 17.1 DONATOR DIREITA Sup 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirSup2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirSup2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirSup2'
											Set @PagoMaster = 'false'
                                        End
                                End
            
                                --*********** INDICATOR DIREITA Inferior ************** 
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or @PosicaoPai = 'IndicatorDirInf'))
                                Begin
                                    Set @log = @log + '| 34.3.5 Pai eh master ou CoordinatorDir ou IndicatorDirSup ou IndicatorDirInf:' + @PosicaoPai
                                    if (@DonatorDirInf1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 18.1 DONATOR DIREITA Inf 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirInf1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirInf1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirInf1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorDirInf2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 18.1 DONATOR DIREITA Inf 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirInf2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirInf2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirInf2'
											Set @PagoMaster = 'false'
                                        End
                                End
            
                                --*********** INDICATOR ESQUERDA Superior **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup'))
                                Begin
                                    Set @log = @log + '| 34.3.6 Pai eh master ou CoordinatorEsq ou IndicatorEsqSup ou IndicatorEsqSup:' + @PosicaoPai
                                    if (@DonatorEsqSup1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 19 DONATOR ESQUERDA Sup 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqSup1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqSup1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqSup1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorEsqSup2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 19 DONATOR ESQUERDA Sup 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqSup2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqSup2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqSup2'
											Set @PagoMaster = 'false'
                                        End
                                End

                                --*********** INDICATOR ESQUERDA Inferior **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @PosicaoPai = 'IndicatorEsqInf'))
                                Begin
                                    Set @log = @log + '| 34.3.7 Pai eh master ou CoordinatorEsq ou IndicatorEsqSup ou IndicatorEsqInf:' + @PosicaoPai
                                    if (@DonatorEsqInf1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 20.1 DONATOR ESQUERDA inf 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqInf1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqInf1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqInf1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorEsqInf2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 20.1 DONATOR ESQUERDA inf 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqInf2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqInf2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqInf2'
											Set @PagoMaster = 'false'
                                        End
                                End

                                --**********************Segunda Passagem Caso nao seja incluido acima *******************************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @DireitaFinalizada = 'true' Or  @EsquerdaFinalizada = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.1 Pai eh master:' + @PosicaoPai
                                    --*********** COORDINATOR **************
                                    --Verifica se ha coordinator, caso nao inclui usuario como coordinator na direita
                                    if (@CoordinatorDir is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 14.2 COORDINATOR DIREITA'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                CoordinatorDir = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @CoordinatorDir = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'CoordinatorDir'
											Set @PagoMaster = 'true'
                                        End
                                    --Verifica se ha coordinator, caso nao inclui usuario como coordinator na esquerda
                                    if (@CoordinatorEsq is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 14.2 COORDINATOR ESQUERDA'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                CoordinatorEsq = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @CoordinatorEsq = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'CoordinatorEsq'
											Set @PagoMaster = 'true'
                                        End
                                End --Master
           
                                --*********** COORDINATOR DIREITA 2 **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'  Or @IndicadorDireitaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.2 Pai eh master ou CoordinatorDir:' + @PosicaoPai
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator superior direita
                                    if (@IndicatorDirSup is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 15.2 INDICATOR DIREITA SUP'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorDirSup = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorDirSup = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorDirSup'
											Set @PagoMaster = 'true'
                                        End
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator inferior direita
                                    if (@IndicatorDirInf is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 15.2 INDICATOR DIREITA Inf'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorDirInf = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorDirInf = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorDirInf'
											Set @PagoMaster = 'true'
                                        End
                                End
            
                                --*********** COORDINATOR ESQUERDA 2 **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'  Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.3 Pai eh master ou CoordinatorEsq:' + @PosicaoPai
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator superior esquerda
                                    if (@IndicatorEsqSup is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 16.2 INDICATOR ESQUERDA Sup'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorEsqSup = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorEsqSup = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorEsqSup'
											Set @PagoMaster = 'true'
                                        End
                                    --Verifica se ha Indicator, caso nao inclui usuario como indicator inferior esquerda
                                    if (@IndicatorEsqInf is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 16.2 INDICATOR ESQUERDA Inf'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                IndicatorEsqInf = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @IndicatorEsqInf = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'IndicatorEsqInf'
											Set @PagoMaster = 'true'
                                        End
                                End
            
                                --*********** INDICATOR DIREITA Superior 2 ************** 
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.4 Pai eh master ou CoordinatorEsq ou CoordinatorDir ou IndicatorDirSup:' + @PosicaoPai
                                    if (@DonatorDirSup1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 17.2 DONATOR DIREITA Sup 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirSup1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirSup1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirSup1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorDirSup2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 17.2 DONATOR DIREITA Sup 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirSup2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirSup2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirSup2'
											Set @PagoMaster = 'false'
                                        End
                                End
            
                                --*********** INDICATOR DIREITA Inferior 2 ************** 
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorDir' Or @PosicaoPai = 'IndicatorDirSup' Or @PosicaoPai = 'IndicatorDirInf' Or  @EsquerdaFinalizada = 'true' Or @IndicadorDireitaSuperiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.5 Pai eh master ou CoordinatorDir ou IndicatorDirSup ou IndicatorDirInf:' + @PosicaoPai
                                    if (@DonatorDirInf1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 18.2 DONATOR DIREITA Inf 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirInf1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirInf1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirInf1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorDirInf2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 18.2 DONATOR DIREITA Inf 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorDirInf2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorDirInf2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorDirInf2'
											Set @PagoMaster = 'false'
                                        End
                                End
            
                                --*********** INDICATOR ESQUERDA Superior 2 **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @DireitaFinalizada = 'true' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaInferiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.6 Pai eh master ou CoordinatorEsq ou IndicatorEsqSup ou IndicatorEsqSup:' + @PosicaoPai
                                    if (@DonatorEsqSup1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 19.2 DONATOR ESQUERDA Sup 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqSup1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqSup1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqSup1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorEsqSup2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 19.2 DONATOR ESQUERDA Sup 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqSup2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqSup2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqSup2'
											Set @PagoMaster = 'false'
                                        End
                                End
            
                                --*********** INDICATOR ESQUERDA Inferior 2 **************
                                if(@Incluido = 'false' and (@PosicaoPai = 'Master' Or @PosicaoPai = 'CoordinatorEsq' Or @PosicaoPai = 'IndicatorEsqSup' Or @PosicaoPai = 'IndicatorEsqInf' Or @DireitaFinalizada = 'true' Or @IndicadorEsquerdaSuperiorFinalizado = 'true'))
                                Begin
                                    Set @log = @log + '| 34.4.7 Pai eh master ou CoordinatorEsq ou IndicatorEsqSup ou IndicatorEsqInf:' + @PosicaoPai
                                    if (@DonatorEsqInf1 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 20.2 DONATOR ESQUERDA inf 1'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqInf1 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqInf1 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqInf1'
											Set @PagoMaster = 'false'
                                        End
                                    if (@DonatorEsqInf2 is null and @Incluido = 'false')
                                        Begin
                                            Set @log = @log + '| 20.2 DONATOR ESQUERDA inf 2'
                                            Update
                                                Rede.Tabuleiro
                                            Set
                                                DonatorEsqInf2 = @UsuarioID
                                            Where
                                                ID = @ID
               
                                            Set @DonatorEsqInf2 = @UsuarioID
                                            Set @Incluido = 'true'
                                            Set @PosicaoFilho = 'DonatorEsqInf2'
											Set @PagoMaster = 'false'
                                        End
                                End

								--Atualiza Rede.TabuleiroUsuario para o novo usuario no Tabuleiro
								Update
									Rede.TabuleiroUsuario
								Set
									StatusID = 1,
									TabuleiroID = @ID,
									MasterID = @Master,
									InformePag = 'false',
									PagoMaster = @PagoMaster,
									PagoSistema = 'false',
									Ciclo = 1,
									Posicao = @PosicaoFilho,
									DataInicio = GetDate(),
									Debug = 'Update: Posicoes livres'
								Where
									UsuarioId = @UsuarioID and
									BoardID = @BoardID

                                --*********FIM UPDATES***********
                            End
                        End
                        Else 
                        Begin
							Set @log = @log + '|100 nao completou o tabuleiro ainda'
                            Set @Historico = '07 - Check Completa false: chamada:' + @Chamada
                        End
                    End

                    --Verifica se Master jah teve 4 pagamentos se sim cria convite para entrar em nivel superior
                    Set @log = @log + '|50 Verifica se Master jah teve 4 pagamentos para gerar convite'

                    Select 
                        @count = count(*) 
                    From 
                        Rede.TabuleiroUsuario 
                    Where 
                        TabuleiroId = @ID And 
                        PagoMaster = 1 and 
                        (
                            Posicao = 'DonatorDirSup1' or
                            Posicao = 'DonatorDirSup2' or
                            Posicao = 'DonatorDirInf1' or
                            Posicao = 'DonatorDirnf2' or
                            Posicao = 'DonatorEsqSup1' or
                            Posicao = 'DonatorEsqSup2' or
                            Posicao = 'DonatorEsqInf1' or
                            Posicao = 'DonatorEsqInf2' 
                        )

                    --Envia convite para o master para um nivel superior se jah teve 4 agamentos
                    If(@count >= 4)
                    Begin
                        Set @log = @log + '|50.1 jah teve 4 pagamentos, verifica se ja esta em nivel superior'
                        
                            --*********** Promove Usuario Master para novo Board ***********
                            --Sobe para proximo Board
                            Set @BoardID = @BoardID + 1
                            --Verifica se ainda ha board acima do master
                            IF Not Exists (Select 'Existe' From Rede.TabuleiroBoard Where ID = @BoardID)
                            Begin
                                Set @log = @log + '| 29 Sem board Superior'
                                --Caso nao haja mais board superiores volta ao inicio
                                Set @BoardID = 1
                            End
                            
                            --Verifica se ele esta ou nao no Board superior
							if Exists (
								Select 
									'OK' 
								From
									Rede.TabuleiroUsuario 
								Where 
									UsuarioID = @Master and 
									BoardID = @BoardID and
									StatusID = 0 --nao esta no board superior, se estivesse seria = 1 e 2 jah foi convidado
								)	
							Begin
								--nao estando no BoardSuperior, envia um convite para sua entrada nele
								Set @log = @log + '| 101.0 nao deveria entrar aqui!!!'
								Update
									Rede.TabuleiroUsuario 
								Set 
									StatusID = 2 --StatusID 2 eh um convite para entrar no proximo board
								Where 
									UsuarioID = @Master and 
									BoardID = @BoardID and
									StatusID = 0 --nao esta no boardSuperior
							End
                    End
					Else
					Begin
						Set @log = @log + '|50.2 nao teve 4 pagamentos'
					End
                End
            End
            Else 
            Begin
                Set @log = @log + '| 05.1 #temp nao tem conteudo'
                if(@Chamada <> 'Completa')
                Begin
                    --Caso nao exista o tabuleiro com o board informado
                    Set @log = @log + '| 37 Caso nao exista o tabuleiro com o board informado'
                    --Verifica se ha usuario ativo no board corrente
                    if Exists(Select 'Existe' From Rede.TabuleiroUsuario Where StatusID = 1 and BoardID = @BoardID)
                    Begin
                        Set @log = @log + '| 38 Obtem primeiro Master ativo '
                        --Obtem primeiro Master ativo no primeiro tabuleiro da tabela, e inclui o novo usuario nesse tabuleiro
                        Select Top(1)
                            @MasterTabuleiro = UsuarioID 
                        From 
                            Rede.TabuleiroUsuario
                        Where
                            StatusID = 1 and
                            BoardID = @BoardID
            
                        if(@MasterTabuleiro is null)
                        Begin
                            Set @log = @log + '| 39 Master eh null'
                            Set @Historico = '05 usuario pai (' + TRIM(STR(@MasterTabuleiro)) + ') nao existe! Chamada: ' + @Chamada
                        End
                        Else
                        Begin
                            Set @log = @log + '| 40 Chama novamente essa sp recursivo UsuarioID: ' + TRIM(STR(@UsuarioID)) + ' Master: ' + TRIM(STR(@MasterTabuleiro)) + ' BoardID: ' + TRIM(STR(@BoardID))
                            --Chama novamente essa sp, agora com um pai valido
                            Set @Historico = '09.1 @UsuarioID=' + TRIM(STR(@UsuarioID)) + ',@UsuarioPaiID=' + TRIM(STR(@MasterTabuleiro)) + ',@BoardID=' + TRIM(STR(@BoardID))
                            Exec spG_Tabuleiro @UsuarioID = @UsuarioID, @UsuarioPaiID = @MasterTabuleiro, @BoardID = @BoardID, @Chamada = 'PaiValido'
                        End
                    End
                    Else
                    Begin
					    --nao deve entrar aqui, mas por precausao, cria o log
                        Set @log = @log + '| 41 nao existe usuario no board informado UsuarioID: '  + TRIM(STR(@tabuleiroFechado)) + ' BoardID: '  + TRIM(STR(@tabuleiroFechado))
                    End
                End
            End
        End
        Else
        Begin
            Set @log = @log + '| 43 Usuario nao cadastrado'
            Set @Historico = '06 Novo usuario ' + TRIM(STR(@UsuarioID)) + ' nao esta cadastrado! Chamada: ' + @Chamada
        End
    End
    
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
        Coalesce(@Historico,@Historico,'Sem  Dados'),
        Coalesce(@log,@log,'Sem Dados')
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

--Exec spG_Tabuleiro @UsuarioID=2587,@UsuarioPaiID=2584,@BoardID=2,@Chamada='Convite'