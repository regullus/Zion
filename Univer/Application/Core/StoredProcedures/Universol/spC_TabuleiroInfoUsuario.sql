use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInfoUsuario'))
   Drop Procedure spC_TabuleiroInfoUsuario
go

Create  Proc [dbo].[spC_TabuleiroInfoUsuario]
   @TargetID int,
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
		@MasterID int,
		@TabuleiroID int

    Create table #temp 
    (
        UsuarioID int,
        Nome nvarchar(255),
		Apelido nvarchar(255),
        Celular nvarchar(100),
        Pix nvarchar(255),
        Carteira nvarchar(255),
        ConfirmarRecebimento bit
    )

	Select
		@TabuleiroID = TabuleiroID
	From
		Rede.TabuleiroUsuario (nolock)
	Where
		UsuarioID = @UsuarioID And
		BoardID = @BoardID

    Select 
        @MasterID = Master 
    from
        Rede.Tabuleiro (nolock)
    Where
        id = @TabuleiroID

    If(@MasterID = @UsuarioID)
    Begin
        insert into #temp
        Select
            @TargetID,
            Nome,
			Lower(Apelido),
            Celular,
            '',
            '',
            'false'
        from
            Usuario.Usuario (nolock)
        Where 
            id = @TargetID
		
        Update
            temp
        Set
            ConfirmarRecebimento = 'true'
        From
            #temp temp,
            Rede.TabuleiroUsuario usu
        Where
            temp.UsuarioID = usu.UsuarioID and
			BoardID = @BoardID and
            usu.PagoMaster = 'false'
    End
	
    If(@MasterID = @TargetID)
    Begin
        insert into #temp
        Select
            @TargetID,
            Nome,
			lower(Apelido),
            Celular,
            '',
            '',
            'false'
        from
            Usuario.Usuario (nolock)
        Where 
            id = @TargetID
    
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
		Apelido,
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

--Exec spC_TabuleiroInfoUsuario @TargetID=2580, @UsuarioID=2581, @BoardID=1
--Exec spC_TabuleiroInfoUsuario @TargetID=2589, @UsuarioID=2581, @BoardID=1



