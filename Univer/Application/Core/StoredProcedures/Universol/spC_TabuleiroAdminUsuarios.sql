use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroAdminUsuarios'))
   Drop Procedure spC_TabuleiroAdminUsuarios
go

Create  Proc [dbo].[spC_TabuleiroAdminUsuarios]
   @tipo nvarchar(50)
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem Bdados do tabuleiro de um usuario
-- =============================================================================================
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

	Create Table #temp (
		tipo nvarchar(50),
		UsuarioID int,
        Nome nvarchar(255),
        Login nvarchar(255),
        Apelido nvarchar(255),
        Email nvarchar(255),
        Celular nvarchar(255),
        Posicao nvarchar(100),
        Galaxia nvarchar(255),
        Patrocinador nvarchar(255),
		TabuleiroID int,
		BoardID int,
		BoardNome nvarchar(50),
		BoardCor nvarchar(50),
		StatusID int,
		Eterno bit,
		MasterID int,
		InformePag bit,
		UsuarioIDPag int,
		UsuarioPagNome nvarchar(255),
		ValorPagMaster decimal(10,2),
		Ciclo int,
		PagoMaster bit,
		PagoSistema bit, 
		InformePagSistema bit,
		ValorPagoSistema decimal(10,2),
		TotalRecebimento int,
        ConfirmarEmail bit,
		DataInicio datetime,
		DataFim int
	)
    
	--Usuarios que informaram o pagamento ao sistema
    if(@tipo = 'InformePagtoSistema')
    Begin
	    insert into #temp
	    Select
			'InformePagtoSistema',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePagSistema = 'true' and
		    tab.PagoSistema = 'false'
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

	--Usuarios que pagaram o sistema
    if(@tipo = 'PagosSistema')
    Begin
        Insert into #temp
	    Select 
		    'PagosSistema',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
		    0.0 ValorPagoSistema,
			tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePagSistema = 'true' and
		    tab.PagoSistema = 'true'
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

	--Usuarios que necessitem que se valide o pagamento ao master
    if(@tipo = 'InformePagtoMaster')
    Begin
	    insert into #temp
	    Select
		    'InformePagtoMaster',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
		    tab.Ciclo,
			0.0 ValorPagMaster,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePag = 'true' and
            tab.PagoMaster = 'false' 
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

	if(@tipo = 'PagoMaster')
    Begin
	    insert into #temp
	    Select 
		    'PagoMaster',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePag = 'true' and
            tab.PagoMaster = 'true' 
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

    --Usuarios que pagaram o system e esperam confirmacao de que pagaram
    if(@tipo = 'ConfirmarYellow')
    Begin
	    insert into #temp
	    Select
			'InformePagtoSistema',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePag = 'true' and
		    tab.PagoMaster = 'false' and
            tab.UsuarioIDPag = 2000 --2000 é o usuario system que recebe pagamentos 
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

    --Usuarios que pagaram o system, não informaram o pagamento e esperam confirmacao de que pagaram
    if(@tipo = 'ConfirmarRed')
    Begin
	    insert into #temp
	    Select
			'InformePagtoSistema',
		    tab.UsuarioID,
            '' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePag = 'false' and
		    tab.PagoMaster = 'false' and
            tab.UsuarioIDPag = 2000 --2000 é o usuario system que recebe pagamentos 
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End

    
    if(@tipo <> 'InformePagtoSistema' and @tipo <> 'PagosSistema' and @tipo <> 'InformePagtoMaster' and @tipo <> 'PagoMaster' and @tipo <> 'ConfirmarYellow' and @tipo <> 'ConfirmarRed')
    Begin
        insert into #temp
	    Select 
		    'Tudo',
			tab.UsuarioID,
			'' Nome,
            '' Login,
            '' Apelido,
            '' Email,
            '' Celular,
            tab.Posicao,
            Substring(tb.Nome,1,3) + '-' + FORMAT(tab.TabuleiroID, '000000') Galaxia,
            '' Patrocinador,
		    tab.TabuleiroID,
		    tab.BoardID,
		    tb.Nome as BoardNome,
		    tb.Cor as BoardCor,
		    tab.StatusID,
		    'false' as Eterno,
		    tab.MasterID,
		    tab.InformePag,
		    tab.UsuarioIDPag,
			'' UsuarioPagNome,
			0.0 ValorPagMaster,
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
			0.0 ValorPagoSistema,
		    tab.TotalRecebimento,
            'true' ConfirmarEmail,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID and
			tab.StatusID in (1,2)
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End
    
	--Patrocinador
    Update
        tmp
    Set
        tmp.Patrocinador = usu.apelido
    From
        #temp tmp,
        Usuario.Usuario usu
    Where
        tmp.MasterID = usu.id

	--Nome Usuario
    Update
        tmp
    Set
        tmp.Nome = usu.nome,
        tmp.Login = usu.Login,
        tmp.Apelido = usu.Apelido,
        tmp.Email = usu.Email,
        tmp.Celular = usu.Celular,
        tmp.ConfirmarEmail = Case When ( usu.StatusEmailID = 2 ) Then 'true' Else 'false' End  
    From
        #temp tmp,
        Usuario.Usuario usu
    Where
        tmp.UsuarioID = usu.id

	--Nome Usuario Pagamento
    Update
        tmp
    Set
        tmp.UsuarioPagNome = usu.apelido
    From
        #temp tmp,
        Usuario.Usuario usu
    Where
        tmp.UsuarioIDPag = usu.id

	--Valores
	Update
        tmp
    Set
        tmp.ValorPagMaster = boa.Transferencia
    From
        #temp tmp,
        Rede.TabuleiroBoard boa
    Where
        tmp.BoardID = boa.id and
		tmp.UsuarioIDPag is not null

	--Valores
	Update
        tmp
    Set
   	tmp.ValorPagoSistema =  boa.Licenca
    From
        #temp tmp,
        Rede.TabuleiroBoard boa
    Where
        tmp.BoardID = boa.id and
		tmp.PagoSistema <> 0

	Select 
	    tipo,
		UsuarioID,
        Nome,
        LOWER(Login) Login,
        LOWER(Apelido) Apelido,
        Email,
        Celular,
        Posicao,
        Galaxia,
        LOWER(Patrocinador) Patrocinador,
		TabuleiroID,
		BoardID,
		BoardNome,
		BoardCor,
		StatusID,
		Eterno,
		MasterID,
		InformePag,
		UsuarioIDPag,
		LOWER(UsuarioPagNome) UsuarioPagNome,
		ValorPagMaster,
		Ciclo,
		PagoMaster,
		PagoSistema,
		InformePagSistema,
		ValorPagoSistema,
		TotalRecebimento,
        ConfirmarEmail,
		DataInicio,
		DataFim
	From 
		#temp
    
End -- Sp

go
Grant Exec on spC_TabuleiroAdminUsuarios To public
go

--Exec spC_TabuleiroAdminUsuarios @tipo='InformePagtoSistema'
--Exec spC_TabuleiroAdminUsuarios @tipo='ConfirmarYellow'
--Exec spC_TabuleiroAdminUsuarios @tipo='PagosSistema'
--Exec spC_TabuleiroAdminUsuarios @tipo='InformePagtoMaster'
--Exec spC_TabuleiroAdminUsuarios @tipo='PagoMaster'
Exec spC_TabuleiroAdminUsuarios @tipo='Tudo'

