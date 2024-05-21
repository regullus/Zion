use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroAdminUsuarios'))
   Drop Procedure spC_TabuleiroAdminUsuarios
go

Create  Proc [dbo].[spC_TabuleiroAdminUsuarios]
   @tipo nvarchar(10)

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
		UsuarioIDPag int, --*
		Ciclo int,
		PagoMaster bit,
		PagoSistema bit, --*
		InformePagSistema bit,
		TotalRecebimento int, --*
		DataInicio datetime,
		DataFim int
	)
    
    if(@tipo = 'InformePagto')
    Begin
	    insert into #temp
	    Select 
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
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
		    tab.TotalRecebimento,
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

    if(@tipo = 'Pagos')
    Begin
        insert into #temp
	    Select 
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
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
		    tab.TotalRecebimento,
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

    if(@tipo = 'ConfirmarRecebimento')
    Begin
        insert into #temp
	    Select 
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
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
		    tab.TotalRecebimento,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID And
            tab.InformePagSistema = 'true' and
            tab.PagoMaster = 'false' 
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End



    if(@tipo <> 'InformePagto' and @tipo <> 'Pagos' and @tipo <> 'ConfirmarRecebimento')
    Begin
        insert into #temp
	    Select 
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
		    tab.Ciclo,
		    tab.PagoMaster,
		    tab.PagoSistema,
		    tab.InformePagSistema,
		    tab.TotalRecebimento,
		    tab.DataInicio,
		    tab.DataFim
	    From 
		    Rede.TabuleiroUsuario tab (nolock),
		    Rede.TabuleiroBoard tb (nolock)
	    Where
		    tab.BoardID = tb.ID
	    Order By
		    tab.BoardID,
		    tab.UsuarioID
    End
    
    Update
        tmp
    Set
        Patrocinador = usu.apelido
    From
        #temp tmp,
        Usuario.Usuario usu
    Where
        tmp.MasterID = usu.id

    Update
        tmp
    Set
        Nome = usu.nome,
        Login = usu.Login,
        Apelido = usu.Apelido,
        Email = usu.Email,
        Celular = usu.Celular
    From
        #temp tmp,
        Usuario.Usuario usu
    Where
        tmp.UsuarioID = usu.id

	Select 
		UsuarioID,
        Nome,
        Login,
        Email,
        Celular,
        Posicao,
        Galaxia,
        Patrocinador,
		TabuleiroID,
		BoardID,
		BoardNome,
		BoardCor,
		StatusID,
		Eterno,
		MasterID,
		InformePag,
		UsuarioIDPag,
		Ciclo,
		PagoMaster,
		PagoSistema,
		InformePagSistema,
		TotalRecebimento,
		DataInicio,
		DataFim
	From 
		#temp

End -- Sp

go
Grant Exec on spC_TabuleiroAdminUsuarios To public
go

--Exec spC_TabuleiroAdminUsuarios @UsuarioID=null





