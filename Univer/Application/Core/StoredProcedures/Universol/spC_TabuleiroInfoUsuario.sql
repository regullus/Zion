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
        PatrocinadorID int,
        Patrocinador nvarchar(255),
        PatrocinadorApelido nvarchar(255),
        PatrocinadorCelular nvarchar(100),
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
            PatrocinadorDiretoID,
            '',
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
            temp.Patrocinador = usu.nome,
            temp.PatrocinadorApelido = usu.Apelido,
            temp.PatrocinadorCelular = usu.Celular
        From
            #temp temp,
            Usuario.Usuario usu
        Where
            usu.ID = temp.PatrocinadorID
		
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
			Lower(Apelido),
            Celular,
            '',
            '',
            PatrocinadorDiretoID,
            '',
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
            temp.Patrocinador = usu.nome,
            temp.PatrocinadorApelido = usu.Apelido,
            temp.PatrocinadorCelular = usu.Celular
        From
            #temp temp,
            Usuario.Usuario usu
        Where
            usu.ID = temp.PatrocinadorID
    
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

    Select top(1)
        UsuarioID,
        Nome,
		Apelido,
        Celular,
        Pix,
        Carteira,
        Patrocinador,
        PatrocinadorID,
        PatrocinadorApelido,
        PatrocinadorCelular,
        ConfirmarRecebimento
    From
        #temp

End -- Sp

go
Grant Exec on spC_TabuleiroInfoUsuario To public
go

--Exec spC_TabuleiroInfoUsuario @TargetID=2586, @UsuarioID=2586, @BoardID=1
--Exec spC_TabuleiroInfoUsuario @TargetID=3735, @UsuarioID=2947, @BoardID=1




