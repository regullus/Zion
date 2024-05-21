use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spD_TabuleiroJob'))
   Drop Procedure spD_TabuleiroJob
go

Create  Proc [dbo].[spD_TabuleiroJob]

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
        @dadosTempoPagto int,
        @dadosTempoMaxPagto int,
        @tempo datetime,
        @DonatorDirSup1 int,
        @DonatorDirSup2 int,
        @DonatorDirInf1 int,
        @DonatorDirInf2 int,
        @DonatorEsqSup1 int,
        @DonatorEsqSup2 int,
        @DonatorEsqInf1 int,
        @DonatorEsqInf2 int,
        @retorno nvarchar(10),
		@maxBoard int,
        @debug nvarchar(max),
        @chamada nvarchar(100)

    Select @dadosTempoPagto = CONVERT(INT, Dados)  from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_PAGAMENTO'
    Select @dadosTempoMaxPagto = CONVERT(INT, Dados) from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_MAX_PAGAMENTO'
    
    Set @retorno = 'No Action'

    BEGIN TRY
    BEGIN TRANSACTION
    --Remove usuario do Tabuleiro

       --Loop em todos os Tabuleiros Usuarios
       -- ******* Inicio Cursor VERMELHO *******
        Declare
        @UsuarioID int,
        @TabuleiroID int,
        @BoardID int,
		@Ciclo int,
        @Posicao nvarchar(255),
        @DataInicio dateTime,
        @AntFetch int,
        @removido bit

        set @debug = 'tudo ok 1'
        set @chamada = 'Job 1'

        --Cursor
        Declare
            curRegistro 
        Cursor Local For
            Select 
                UsuarioID,
                TabuleiroID,
                BoardID,
			    Ciclo,
                Posicao,
                DataInicio
            FROM 
                Rede.TabuleiroUsuario
            Where
                InformePag = 'false' and
                PagoMaster = 'false' and
                StatusID = 1 and --Ativo
                TabuleiroID is not null
            Order By 
                UsuarioID, BoardID

        Open curRegistro
        Fetch Next From curRegistro Into  @UsuarioID, @TabuleiroID, @BoardID, @Ciclo, @Posicao, @DataInicio
        Select @AntFetch = @@fetch_status
        While @AntFetch = 0
        Begin
            Set @tempo = DATEADD(mi, @dadosTempoPagto, @DataInicio);
            Set @removido = 'false'

		    --Ultimo board em que o usuario se encontra
		    Select @maxBoard = BoardID From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID is not null
		
            IF(@tempo < GetDate())
            Begin
			    if(@maxBoard > 1)
			    Begin
                    set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                    set @BoardID = Coalesce(@BoardID,@BoardID,0)
                    set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                    set @debug = 'Removido pelo job 1.1 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                    set @chamada = 'Job 1.1'
				    
                    --statusID do usuario para 2 é convite
				    Update
					    Rede.TabuleiroUsuario
				    Set
					    StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
					    Posicao = '',
					    TabuleiroID = null,
					    InformePag = 'false',
					    UsuarioIDPag = null,
					    DataInicio = GetDate(),
					    Debug = @debug
				    Where
					    UsuarioID = @UsuarioID and
					    BoardID = @BoardID

                    Set @removido = 'true'
			    End
			    Else
			    Begin
				    if(@Ciclo > 1)
				    Begin
                        set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                        set @BoardID = Coalesce(@BoardID,@BoardID,0)
                        set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                        set @debug = 'Removido pelo job 1.2 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                        set @chamada = 'Job 1.2'
					    
                        --statusID do usuario para 2 é convite
					    Update
						    Rede.TabuleiroUsuario
					    Set
						    StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
						    Posicao = '',
						    TabuleiroID = null,
						    InformePag = 'false',
						    UsuarioIDPag = null,
						    DataInicio = GetDate(),
						    Debug = @debug
					    Where
						    UsuarioID = @UsuarioID and
						    BoardID = @BoardID

                        Set @removido = 'true'
				    End
				    Else
				    Begin
				        --Checa se ele não esta em Saturno mesmo, daí pode dar como StatusId 0, como um novo usuario
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
                            set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                            set @BoardID = Coalesce(@BoardID,@BoardID,0)
                            set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                            set @debug = 'Removido pelo job 1.3 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                            set @chamada = 'Job 1.3'
						    
                            --statusID do usuario para 0 é quando é a primeira entrada do usuario no tabuleiroconvite
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
							    BoardID = @BoardID
                            
                            Set @removido = 'true'
					    End
					    Else
					    Begin
                            set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                            set @BoardID = Coalesce(@BoardID,@BoardID,0)
                            set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                            set @debug = 'Removido pelo job 1.4 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                            set @chamada = 'Job 1.4'
						    
                            --statusID do usuario para 2 é convite
						    Update
							    Rede.TabuleiroUsuario
						    Set
							    StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
							    Posicao = '',
							    TabuleiroID = null,
							    InformePag = 'false',
							    UsuarioIDPag = null,
							    DataInicio = GetDate(),
							    Debug = @debug
						    Where
							    UsuarioID = @UsuarioID and
							    BoardID = @BoardID			
                            
                            Set @removido = 'true'
					    End
				    End
			    End
		        
                if (@removido = 'true')
                Begin
			        --Remove usuario do tabuleiro
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

                    Set @Debug = @debug + ' PosicaoRemovida=' + @posicao

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
			            '' as Historico,
			            @debug as [log]

                End
                Set @retorno = 'OK'
            End

            --Proxima linha do cursor
            Fetch Next From curRegistro Into @UsuarioID, @TabuleiroID, @BoardID, @Ciclo, @Posicao, @DataInicio
            Select @AntFetch = @@fetch_status       
        End -- While
      
        Close curRegistro
        Deallocate curRegistro

       --Loop em todos os Tabuleiros Usuarios
       -- ******* Inicio Cursor AMARELO *******
        Declare
            curRegistro 
        Cursor Local For
            Select 
                UsuarioID,
                TabuleiroID,
                BoardID,
                Posicao,
                DataInicio
            FROM 
                Rede.TabuleiroUsuario
            Where
                InformePag = 'true' and
                PagoMaster = 'false' and
                StatusID = 1 and --Ativo
                TabuleiroID is not null
            Order By 
                UsuarioID, BoardID
        
        set @debug = 'tudo ok 2'
        set @chamada = 'Job 2'

        Open curRegistro
        Fetch Next From curRegistro Into  @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
        Select @AntFetch = @@fetch_status
        While @AntFetch = 0
        Begin
            Set @tempo = DATEADD(mi, @dadosTempoMaxPagto, @DataInicio);
            set @removido = 'false'
            
            IF(@tempo < GetDate())
            Begin
			    if(@Ciclo > 1)
			    Begin
                    set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                    set @BoardID = Coalesce(@BoardID,@BoardID,0)
                    set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                    set @debug = 'Removido pelo job 2.1 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                    set @chamada = 'Job 2.1'

				    --Dois statusID do usuario 
				    Update
					    Rede.TabuleiroUsuario
				    Set
					    StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
					    Posicao = '',
					    TabuleiroID = null,
					    InformePag = 'false',
					    UsuarioIDPag = null,
					    DataInicio = GetDate(),
					    Debug = @debug
				    Where
					    UsuarioID = @UsuarioID and
					    BoardID = @BoardID

                    set @removido = 'true'
			    End
			    Else
			    Begin
				    if(@BoardID > 1)
				    Begin
                        set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                        set @BoardID = Coalesce(@BoardID,@BoardID,0)
                        set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                        set @debug = 'Removido pelo job 2.2 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                        set @chamada = 'Job 2.2'
                        
                        Select @debug Debug

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
						    BoardID = @BoardID

                        set @removido = 'true'
				    End
				    Else
				    Begin
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
                            set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                            set @BoardID = Coalesce(@BoardID,@BoardID,0)
                            set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                            set @debug = 'Removido pelo job 2.3 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                            set @chamada = 'Job 2.3'

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
							    BoardID = @BoardID

                            set @removido = 'true'
					    End
					    Else
					    Begin
                            set @maxBoard = Coalesce(@maxBoard,@maxBoard,0)
                            set @BoardID = Coalesce(@BoardID,@BoardID,0)
                            set @TabuleiroID = Coalesce(@TabuleiroID,@TabuleiroID,0)

                            set @debug = 'Removido pelo job 2.4 UsuarioID=' + TRIM(STR(@UsuarioID)) + ' Posicao=' + @Posicao + ' maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + TRIM(STR(@BoardID)) + ' TabuleiroID=' + TRIM(STR(@TabuleiroID))
                            set @chamada = 'Job 2.4'
						    
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
							    BoardID = @BoardID
                           
                            set @removido = 'true'
					    End
				    End
			    End
                
                if (@removido = 'true')
                Begin
                    --Remove usuario do tabuleiro
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

                    Set @Debug = @debug + ' PosicaoRemovida=' + @posicao
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
            End

            --Proxima linha do cursor
            Fetch Next From curRegistro Into @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
            Select @AntFetch = @@fetch_status       
        End -- While
      
        Close curRegistro
        Deallocate curRegistro

		COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
		DECLARE @error int, @message varchar(4000), @xstate int;
		SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
        Set @Retorno = 'Error Job: ' + TRIM(STR(@error)) + '-' + @message
	END CATCH     

    -- ******* Fim Cursor *******
    Select @Retorno Retorno
End -- Sp

go
Grant Exec on spD_TabuleiroJob To public
go
/*
Begin Tran
--3760
Exec spD_TabuleiroJob

Select * from  Rede.TabuleiroLog order by id desc

Rollback tran
*/
