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
        @Retorno nvarchar(4)

    Select
        @Posicao = Posicao,
        @TabuleiroID = TabuleiroID
    From
        Rede.TabuleiroUsuario
    Where
        UsuarioID = @UsuarioID and
        BoardID = @BoardID and
        PagoMaster = 'false' and --Nao pagou o Master
        StatusID = 1       --Nao se ativou
		
	--Zera statusID do usuario 
	Update
		Rede.TabuleiroUsuario
	Set
        StatusID = 2, -- Esta disponivel para entrar em um tabuleiro
        Posicao = '',
        TabuleiroID = null,
		Ciclo = Ciclo - 1,
        InformePag = 'false',
        UsuarioIDPag = null,
        Debug = 'Usuario saiu'
	Where
		UsuarioID = @UsuarioID and
		BoardID = @BoardID and
		PagoMaster = 'false' and --Nao pagou o Master
        StatusID = 1       --Nao se ativou

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

--Exec spG_TabuleiroSair @UsuarioID = 2580, @BoardID = 1

