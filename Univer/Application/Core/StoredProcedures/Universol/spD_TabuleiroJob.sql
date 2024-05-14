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
		@maxBoard int

    Select @dadosTempoPagto = CONVERT(INT, Dados)  from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_PAGAMENTO'
    Select @dadosTempoMaxPagto = CONVERT(INT, Dados) from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_MAX_PAGAMENTO'
    
    Set @retorno = 'No Action'
   --Remove usuario do Tabuleiro

   --Loop em todos os Tabuleiros Usuarios
   -- ******* Inicio Cursor *******
    Declare
        @UsuarioID int,
        @TabuleiroID int,
        @BoardID int,
		@Ciclo int,
        @Posicao nvarchar(255),
        @DataInicio dateTime,
        @AntFetch int

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
            StatusID = 1 --Ativo
        Order By 
            UsuarioID, BoardID

    Open curRegistro
    Fetch Next From curRegistro Into  @UsuarioID, @TabuleiroID, @BoardID, @Ciclo, @Posicao, @DataInicio
    Select @AntFetch = @@fetch_status
    While @AntFetch = 0
    Begin
        Set @tempo = DATEADD(mi, @dadosTempoPagto, @DataInicio);

		--Ultimo board em que o usuario se encontra
		Select @maxBoard = BoardID From Rede.TabuleiroUsuario Where UsuarioID = @UsuarioID and TabuleiroID is not null
		
        IF(@tempo < GetDate())
        Begin
			if(@maxBoard > 1)
			Begin
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
					Debug = 'Removido pelo job 1.1 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
				Where
					UsuarioID = @UsuarioID and
					BoardID = @BoardID
			End
			Else
			Begin
				if(@Ciclo > 1)
				Begin
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
						Debug = 'Removido pelo job 1.1 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
					Where
						UsuarioID = @UsuarioID and
						BoardID = @BoardID
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
							TabuleiroID is not Null --Garante que ele não esta em Saturno
					)
					Begin
						--statusID do usuario para 0 é quando é a primeira entrada do usuario no tabuleiroconvite
						Update
							Rede.TabuleiroUsuario
						Set
							StatusID = 0, -- Esta disponivel para entrar em um tabuleiro
							Posicao = '',
							TabuleiroID = null,
							InformePag = 'false',
							UsuarioIDPag = null,
							Debug = 'Removido pelo job 1.2 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
						Where
							UsuarioID = @UsuarioID and
							BoardID = @BoardID
					End
					Else
					Begin
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
							Debug = 'Removido pelo job 1.3 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
						Where
							UsuarioID = @UsuarioID and
							BoardID = @BoardID						
					End
				End
			End
		
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

            Set @retorno = 'OK'
        End

        --Proxima linha do cursor
        Fetch Next From curRegistro Into @UsuarioID, @TabuleiroID, @BoardID, @Ciclo, @Posicao, @DataInicio
        Select @AntFetch = @@fetch_status       
    End -- While
      
    Close curRegistro
    Deallocate curRegistro

    --Cursor
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
            StatusID = 1 --Ativo
        Order By 
            UsuarioID, BoardID

    Open curRegistro
    Fetch Next From curRegistro Into  @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
    Select @AntFetch = @@fetch_status
    While @AntFetch = 0
    Begin
        Set @tempo = DATEADD(mi, @dadosTempoMaxPagto, @DataInicio);
        IF(@tempo < GetDate())
        Begin
			if(@Ciclo > 1)
			Begin
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
					Debug = 'Removido pelo job 2.1 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
				Where
					UsuarioID = @UsuarioID and
					BoardID = @BoardID
			End
			Else
			Begin
				if(@BoardID > 1)
				Begin
					 --Dois statusID do usuario 
					Update
						Rede.TabuleiroUsuario
					Set
						StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
						Posicao = '',
						TabuleiroID = null,
						InformePag = 'false',
						UsuarioIDPag = null,
						Debug = 'Removido pelo job 2.2 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
					Where
						UsuarioID = @UsuarioID and
						BoardID = @BoardID
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
							TabuleiroID is not Null --Garante que ele não esta em Saturno
					)
					Begin
						--Zera statusID do usuario 
						Update
							Rede.TabuleiroUsuario
						Set
							StatusID = 0, -- Esta disponivel para entrar em um tabuleiro
							Posicao = '',
							TabuleiroID = null,
							InformePag = 'false',
							UsuarioIDPag = null,
							Debug = 'Removido pelo job 2.3 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
						Where
							UsuarioID = @UsuarioID and
							BoardID = @BoardID
					End
					Else
					Begin
						 --Dois statusID do usuario 
						Update
							Rede.TabuleiroUsuario
						Set
							StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
							Posicao = '',
							TabuleiroID = null,
							InformePag = 'false',
							UsuarioIDPag = null,
							Debug = 'Removido pelo job 2.2 @maxBoard=' + TRIM(STR(@maxBoard)) + ' BoardID=' + + TRIM(STR(@BoardID))
						Where
							UsuarioID = @UsuarioID and
							BoardID = @BoardID
					End
				End
			End

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

            Set @retorno = 'OK'
        End

        --Proxima linha do cursor
        Fetch Next From curRegistro Into @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
        Select @AntFetch = @@fetch_status       
    End -- While
      
    Close curRegistro
    Deallocate curRegistro

    -- ******* Fim Cursor *******
    Select @Retorno Retorno
End -- Sp



go
Grant Exec on spD_TabuleiroJob To public
go
/*
Begin Tran

Select * From Rede.TabuleiroUsuario Where UsuarioID = 2594 and BoardID = 1
Exec spD_TabuleiroJob
Select * From Rede.TabuleiroUsuario Where UsuarioID = 2594 and BoardID = 1

Rollback tran
*/
