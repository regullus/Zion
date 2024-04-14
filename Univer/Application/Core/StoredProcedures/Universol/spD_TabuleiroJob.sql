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
        @retorno nvarchar(10)

    Select @dadosTempoPagto = CONVERT(INT, Dados)  from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_PAGAMENTO'
    Select @dadosTempoMaxPagto = CONVERT(INT, Dados) from Sistema.Configuracao where chave = 'TABULEIRO_TEMPO_MAX_PAGAMENTO'
    
    Set @retorno = 'No Action'
   --Remove usuario do Tabuleiro

   --Loop em todos os Tabuleiros Usuarios
   -- ******* Inicio Cursor *******
    Declare
        @ID int,
        @UsuarioID int,
        @TabuleiroID int,
        @BoardID int,
        @Posicao nvarchar(255),
        @DataInicio dateTime,
        @AntFetch int

    --Cursor
    Declare
        curRegistro 
    Cursor Local For
        Select 
            ID,
            UsuarioID,
            TabuleiroID,
            BoardID,
            Posicao,
            DataInicio
        FROM 
            Rede.TabuleiroUsuario
        Where
            InformePag = 0 and
            PagoMaster = 0 and
            StatusID = 1 --Ativo
        Order By 
            ID

    Open curRegistro
    Fetch Next From curRegistro Into  @ID, @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
    Select @AntFetch = @@fetch_status
    While @AntFetch = 0
    Begin
        Set @tempo = DATEADD(mi, @dadosTempoPagto, @DataInicio);
        IF(@tempo < GetDate())
        Begin
             --Inclui excluido na tabela TabuleiroUsuarioExcluidos
            Insert Into
                Rede.TabuleiroUsuarioExcluidos
            SELECT 
                ID,
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
                DataInicio,
                DataFim
            FROM 
                Rede.TabuleiroUsuario
            Where
               ID = @ID

            --Remove usuario que nao pagou no tabuleiroUsuario
            Delete
                Rede.TabuleiroUsuario
            Where
               ID = @ID

            Delete
                Rede.TabuleiroNivel
            Where
                UsuarioID = @UsuarioID and
                BoardID = @BoardID and
				TabuleiroID = @TabuleiroID

            --Remove usuario que nao pagou no tabuleiro
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID) 
			Begin
				Update Rede.Tabuleiro Set DonatorDirSup1 = null Where ID = @TabuleiroID and DonatorDirSup1 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorDirSup2 = null Where ID = @TabuleiroID and DonatorDirSup2 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorDirInf1 = null Where ID = @TabuleiroID and DonatorDirInf1 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorDirInf2 = null Where ID = @TabuleiroID and DonatorDirInf2 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorEsqSup1 = null Where ID = @TabuleiroID and DonatorEsqSup1 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorEsqSup2 = null Where ID = @TabuleiroID and DonatorEsqSup2 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorEsqInf1 = null Where ID = @TabuleiroID and DonatorEsqInf1 = @UsuarioID
			End
			if Exists (Select 'OK' from Rede.Tabuleiro  Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID)
			Begin
				Update Rede.Tabuleiro Set DonatorEsqInf2 = null Where ID = @TabuleiroID and DonatorEsqInf2 = @UsuarioID
			End

            Set @retorno = 'OK'
        End

        --Proxima linha do cursor
        Fetch Next From curRegistro Into @ID, @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
        Select @AntFetch = @@fetch_status       
    End -- While
      
    Close curRegistro
    Deallocate curRegistro

    --Cursor
    Declare
        curRegistro 
    Cursor Local For
        Select 
            ID,
            UsuarioID,
            TabuleiroID,
            BoardID,
            Posicao,
            DataInicio
        FROM 
            Rede.TabuleiroUsuario
        Where
            InformePag = 1 and
            PagoMaster = 0 and
            StatusID = 1 --Ativo
        Order By 
            ID

    Open curRegistro
    Fetch Next From curRegistro Into  @ID, @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
    Select @AntFetch = @@fetch_status
    While @AntFetch = 0
    Begin
        Set @tempo = DATEADD(mi, @dadosTempoMaxPagto, @DataInicio);
        IF(@tempo < GetDate())
        Begin
             --Inclui excluido na tabela TabuleiroUsuarioExcluidos
            Insert Into
                Rede.TabuleiroUsuarioExcluidos
            SELECT 
                ID,
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
                DataInicio,
                DataFim
            FROM 
                Rede.TabuleiroUsuario
            Where
               ID = @ID

            --Remove usuario que nao pagou no tabuleiroUsuario
            Delete
                Rede.TabuleiroUsuario
            Where
               ID = @ID

            Delete
                Rede.TabuleiroNivel
            Where
                UsuarioID = @UsuarioID and
                BoardID = @BoardID

            --Remove usuario que nao pagou no tabuleiro
            if(@Posicao = 'DonatorDirSup1') Update Rede.Tabuleiro Set DonatorDirSup1 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorDirSup2') Update Rede.Tabuleiro Set DonatorDirSup2 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorDirInf1') Update Rede.Tabuleiro Set DonatorDirInf1 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorDirInf2') Update Rede.Tabuleiro Set DonatorDirInf2 = null Where ID = @TabuleiroID 

            if(@Posicao = 'DonatorEsqSup1') Update Rede.Tabuleiro Set DonatorEsqSup1 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorEsqSup2') Update Rede.Tabuleiro Set DonatorEsqSup2 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorEsqInf1') Update Rede.Tabuleiro Set DonatorEsqInf1 = null Where ID = @TabuleiroID 
            if(@Posicao = 'DonatorEsqInf2') Update Rede.Tabuleiro Set DonatorEsqInf2 = null Where ID = @TabuleiroID 
            Set @retorno = 'OK'
        End

        --Proxima linha do cursor
        Fetch Next From curRegistro Into @ID, @UsuarioID, @TabuleiroID, @BoardID, @Posicao, @DataInicio
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
--Exec spD_TabuleiroJob

