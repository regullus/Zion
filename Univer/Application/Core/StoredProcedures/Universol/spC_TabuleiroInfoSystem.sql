use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInfoSystem'))
   Drop Procedure spC_TabuleiroInfoSystem
go

Create  Proc [dbo].[spC_TabuleiroInfoSystem]

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
    Declare @idTarget int

    set @idTarget = 2000 --Usuario para do systema e o 2000

    insert into #temp
    Select
        @idTarget,
        Nome,
        Celular,
        '',
        '',
        'false'
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
Grant Exec on spC_TabuleiroInfoSystem To public
go

--Exec spC_TabuleiroInfoSystem 


