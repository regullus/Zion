use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInfoUsuario'))
   Drop Procedure spC_TabuleiroInfoUsuario
go

Create  Proc [dbo].[spC_TabuleiroInfoUsuario]
   @idTarget int,
   @idUsuario int,
   @idTabuleiro int

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
    Declare @master int

    Create table #temp 
    (
        UsuarioID int,
        Nome nvarchar(255),
        Celular nvarchar(100),
        Pix nvarchar(255),
        Carteira nvarchar(255),
        ConfirmarRecebimento bit
    )

    Select 
        @master = Master 
    from
        Rede.Tabuleiro
    Where
        id = @idTabuleiro

    If(@master = @idUsuario)
    Begin
        insert into #temp
        Select
            @idTarget,
            Nome,
            Celular,
            '',
            '',
            0
        from
            Usuario.Usuario
        Where 
            id = @idTarget
    End

    If(@master = @idTarget)
    Begin
        insert into #temp
        Select
            @idTarget,
            Nome,
            Celular,
            '',
            '',
            0
        from
            Usuario.Usuario
        Where 
            id = @idTarget
    
        Update 
            temp
        Set
            temp.Pix = cd.Litecoin, --Usando o campo litecoin para pix
            temp.Carteira = cd.Tether
        From 
            #temp temp,
            Financeiro.ContaDeposito cd
        Where
            temp.UsuarioID = cd.IDUsuario
    End

    Select
        UsuarioID,
        Nome,
        Celular,
        Pix,
        Carteira,
        ConfirmarRecebimento
    From
        #temp

End -- Sp

go
Grant Exec on spC_TabuleiroInfoUsuario To public
go

Exec spC_TabuleiroInfoUsuario @idTarget = 2580, @idUsuario = 2581, @idTabuleiro = 1
Exec spC_TabuleiroInfoUsuario @idTarget = 2581, @idUsuario = 2580, @idTabuleiro = 1
Exec spC_TabuleiroInfoUsuario @idTarget = 2581, @idUsuario = 2582, @idTabuleiro = 1

    --Select * from Rede.Tabuleiro Where id = 1
