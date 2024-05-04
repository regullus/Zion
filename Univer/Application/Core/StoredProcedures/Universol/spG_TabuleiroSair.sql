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
        @Retorno nvarchar(4),
        @Ciclo int

    Select
        @Posicao = Posicao,
        @TabuleiroID = TabuleiroID,
        @Ciclo = Ciclo
    From
        Rede.TabuleiroUsuario
    Where
        UsuarioID = @UsuarioID and
        BoardID = @BoardID and
        PagoMaster = 'false' and --Nao pagou o Master
        StatusID = 1       --Nao se ativou

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
            Debug = 'Usuario saiu 1'
        Where
            UsuarioID = @UsuarioID and
		    BoardID = @BoardID and
		    PagoMaster = 'false' and --Nao pagou o Master
            StatusID = 1       --Nao se ativou
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
				Debug = 'Usuario saiu 1'
			Where
				UsuarioID = @UsuarioID and
				BoardID = @BoardID and
				PagoMaster = 'false' and --Nao pagou o Master
				StatusID = 1       --Nao se ativou
		End
		Else
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
				Debug = 'Usuario saiu 2'
			Where
				UsuarioID = @UsuarioID and
				BoardID = @BoardID and
				PagoMaster = 'false' and --Nao pagou o Master
				StatusID = 1       --Nao se ativou
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

    Select @retorno Retorno
    
End -- Sp

go
Grant Exec on spG_TabuleiroSair To public
go

--Exec spG_TabuleiroSair @UsuarioID = 2580, @BoardID = 1

