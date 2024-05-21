use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spG_TabuleiroSair'))
   Drop Procedure spG_TabuleiroSair
go

Create  Proc [dbo].[spG_TabuleiroSair]
    @UsuarioID int,
    @BoardID int

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
        @TabuleiroID int,
        @Posicao nvarchar(255),
        @PosicaoExcluida nvarchar(255),
        @Retorno nvarchar(4),
        @Ciclo int,
        @removido bit,
        @debug nvarchar(max),
        @chamada nvarchar(100)

    Set @removido = 'false'
    set @debug = 'tudo ok'
    set @chamada = 'Removido pelo usuario'

	Select
        @Posicao = Posicao,
        @TabuleiroID = TabuleiroID,
        @Ciclo = Ciclo
    From
        Rede.TabuleiroUsuario (nolock)
    Where
        UsuarioID = @UsuarioID and
        BoardID = @BoardID and
        PagoMaster = 'false' and --Nao pagou o Master
        StatusID = 1       --Nao se ativou

    BEGIN TRY
        BEGIN TRANSACTION

        if(@Ciclo > 1)
        Begin
            set @BoardID = Coalesce(@BoardID,@BoardID,0)
            set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

            set @debug = 'Removido pelo usuario 1.1 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
            set @chamada = 'Removido pelo usuario'

            --Dois statusID do usuario 
            Update
                Rede.TabuleiroUsuario
            Set
                StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
                Posicao = '',
                TabuleiroID = null,
                InformePag = 'false',
                UsuarioIDPag = null,
                Debug = @debug
            Where
                UsuarioID = @UsuarioID and
                BoardID = @BoardID and
                PagoMaster = 'false' and --Nao pagou o Master
                StatusID = 1       --Nao se ativou

            Insert Into Rede.TabuleiroLog
		    Select 
			    Coalesce(@UsuarioID,@UsuarioID,0) as UsuarioID,
			    'Sem Nome' as NomeUsuario,
			    0 as UsuarioPaiID,
			    'Sem Nome' as NomePai,
			    0 as Master,
			    'Sem Nome' as NomeMaster,
			    Coalesce(@BoardID,@BoardID,0) as BoardID,
			    @TabuleiroID as TabuleiroID,
			    'Sem posicao Antiga' as PosicaoAntiga,
			    'Sem posicao Nova' as PosicaoNova,
			    0 as TabuleiroIDAntigo,
			    0 as TabuleiroIDNovo,
			    'Usuario saiu 1' as Chamada,
			    format(getdate(),'dd/MM/yyyy HH:mm:ss') as Data,
			    'Sem Historico' as Historico,
			    'Usuario saiu 1 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID)) as [log]

            Set @removido = 'true'
        End
        Else
        Begin
            if(@BoardID > 1)
            Begin
                set @BoardID = Coalesce(@BoardID,@BoardID,0)
                set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                set @debug = 'Removido pelo usuario 1.2 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                set @chamada = 'Removido pelo usuario'

                --Dois statusID do usuario 
                Update
                    Rede.TabuleiroUsuario
                Set
                    StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
                    Posicao = '',
                    TabuleiroID = null,
                    InformePag = 'false',
                    UsuarioIDPag = null,
                    Debug = @debug
                Where
                    UsuarioID = @UsuarioID and
                    BoardID = @BoardID and
                    PagoMaster = 'false' and --Nao pagou o Master
                    StatusID = 1       --Nao se ativou

                Insert Into Rede.TabuleiroLog
		        Select 
			        Coalesce(@UsuarioID,@UsuarioID,0) as UsuarioID,
			        'Sem Nome' as NomeUsuario,
			        0 as UsuarioPaiID,
			        'Sem Nome' as NomePai,
			        0 as Master,
			        'Sem Nome' as NomeMaster,
			        Coalesce(@BoardID,@BoardID,0) as BoardID,
			        @TabuleiroID as TabuleiroID,
			        'Sem posicao Antiga' as PosicaoAntiga,
			        'Sem posicao Nova' as PosicaoNova,
			        0 as TabuleiroIDAntigo,
			        0 as TabuleiroIDNovo,
			        'Usuario saiu 2' as Chamada,
			        format(getdate(),'dd/MM/yyyy HH:mm:ss') as Data,
			        'Sem Historico' as Historico,
			        'Usuario saiu 2 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID)) as [log]

                Set @removido = 'true'
            End
            Else
            Begin
                set @BoardID = Coalesce(@BoardID,@BoardID,0)
                set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                if Exists (
				    Select 'OK' 
				    From 
					    Rede.TabuleiroUsuario 
				    Where 
					    UsuarioID = @UsuarioID and
					    BoardID = 2 and --Saturno
					    TabuleiroID is Null --Garante que ele não esta em Saturno
			    )
			    Begin
                    set @debug = 'Removido pelo usuario 1.3 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                    set @chamada = 'Removido pelo usuario'

                    --Zera statusID do usuario 
                    Update
                        Rede.TabuleiroUsuario
                    Set
                        StatusID = 0, -- Esta disponivel para entrar em um tabuleiro
                        Posicao = '',
                        TabuleiroID = null,
                        InformePag = 'false',
                        UsuarioIDPag = null,
                        Debug = @debug
                    Where
                        UsuarioID = @UsuarioID and
                        BoardID = @BoardID and
                        PagoMaster = 'false' and --Nao pagou o Master
                        StatusID = 1       --Nao se ativou
                End
                Else
                Begin
                    set @debug = 'Removido pelo usuario 1.4 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                    set @chamada = 'Removido pelo usuario'

                    --Dois statusID do usuario 
                    Update
                        Rede.TabuleiroUsuario
                    Set
                        StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
                        Posicao = '',
                        TabuleiroID = null,
                        InformePag = 'false',
                        UsuarioIDPag = null,
                        Debug = @debug
                    Where
                        UsuarioID = @UsuarioID and
                        BoardID = @BoardID and
                        PagoMaster = 'false' and --Nao pagou o Master
                        StatusID = 1       --Nao se ativou
                End
                Set @removido = 'true'
            End
        End
        
        if(@removido = 'true')
        Begin
            --Remove usuario que nao pagou no tabuleiro
            if(@posicao = 'DonatorDirSup1')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID) 
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorDirSup1 = null Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorDirSup2')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorDirSup2 = null Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorDirInf1')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorDirInf1 = null Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorDirInf2')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorDirInf2 = null Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorEsqSup1')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorEsqSup1 = null Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorEsqSup2')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorEsqSup2 = null Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorEsqInf1')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorEsqInf1 = null Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID
                End
            End
            if(@posicao = 'DonatorEsqInf2')
            Begin
                if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID)
                Begin
                    Set @Retorno = 'OK'
                    Update Rede.Tabuleiro Set DonatorEsqInf2 = null Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID
                End
            End

            Insert Into Rede.TabuleiroLog
            Select 
	            Coalesce(@UsuarioID,@UsuarioID,0) as UsuarioID,
	            '' as NomeUsuario,
	            0 as UsuarioPaiID,
	            '' as NomePai,
	            0 as Master,
	            '' as NomeMaster,
	            Coalesce(@BoardID,@BoardID,0) as BoardID,
	            @TabuleiroID as TabuleiroID,
	            @Posicao as PosicaoAntiga,
	            @posicao as PosicaoNova,
	            0 as TabuleiroIDAntigo,
	            0 as TabuleiroIDNovo,
	            @chamada as Chamada,
	            format(getdate(),'dd/MM/yyyy HH:mm:ss') as Data,
	            '' as Mensagem,
	            @debug as Debug

        End

        Set @retorno = 'OK'

		COMMIT TRANSACTION

	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
		--RAISERROR ('Erro na execucao de sp_XS_AtualizaQualificacaoBinario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
        Select @retorno = 'Error SPGTS: ' + @error + '-' + @message
	END CATCH     

    Select @retorno Retorno

End -- Sp

go
Grant Exec on spG_TabuleiroSair To public
go

--Exec spG_TabuleiroSair @UsuarioID = 2580, @BoardID = 1

