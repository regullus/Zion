use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spG_TabuleiroSair'))
   Drop Procedure spG_TabuleiroSair
go

Create  Proc [dbo].[spG_TabuleiroSair]
    @UsuarioID int,
    @TabuleiroID int

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
        @Posicao nvarchar(255),
        @Retorno nvarchar(4),
        @BoardID int

    Select
        @Posicao = Posicao,
        @BoardID = BoardID
    From
        Rede.TabuleiroUsuario
    Where
        UsuarioID = @UsuarioID and
        TabuleiroID = @TabuleiroID and
        PagoMaster = 0 and --Não pagou o Master
        StatusID = 1       --Não se ativou

    --Remove usuario que nao pagou no tabuleiroUsuario
    Delete
        Rede.TabuleiroUsuario
    Where
        UsuarioID = @UsuarioID and
        TabuleiroID = @TabuleiroID and
        PagoMaster = 0 and --Não pagou o Master
        StatusID = 1       --Não se ativou

    Delete
        Rede.TabuleiroNivel 
    Where 
        UsuarioID = @UsuarioID And
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

    Select @retorno Retorno
    
End -- Sp

go
Grant Exec on spG_TabuleiroSair To public
go

--Exec spG_TabuleiroSair @UsuarioID = 2596, @TabuleiroID = 9

