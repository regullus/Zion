Use univerDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_UsuariosPaises'))
   Drop Procedure spC_UsuariosPaises
go

Create PROCEDURE [dbo].[spC_UsuariosPaises]

AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 16/02/2018
-- Description: Obtem os Avisos não lidos do usuario
-- =============================================================================================
BEGIN
    --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on

    Declare @total int

    Create Table #temp 
    (
        Total int,
        PaisID int,
        IdiomaID int,
        Nome nvarchar(255),
        Idioma nvarchar(255)
    )
    Insert Into
        #temp
    Select 
        count(*) Total,
        PaisID,
        0,
        '',
        ''
    From
        Usuario.usuario
    Where
        id > 2570
    Group By
        PaisID
    Order By
        Total desc

    Update
        tmp
     Set
        tmp.Nome = p.Nome,
        tmp.IdiomaID = p.IdiomaID
    From
        #temp tmp,
        Globalizacao.Pais p
    Where
        tmp.PaisID = p.ID

    Update
        tmp
    Set
        tmp.Idioma = i.Nome
    From
        #temp tmp,
        Globalizacao.Idioma i
    Where
        tmp.IdiomaID = i.ID
    
    Update
        tmp
    Set
        tmp.Idioma = i.Nome
    From
        #temp tmp,
        Globalizacao.Idioma i
    Where
        tmp.IdiomaID = i.ID

    Select 
        @total=SUM(Total)
    From
        #temp

    Insert Into 
        #temp
    Select
        @total total,
        0 PaisID,
        0 IdiomaID,
        'Total' Nome,
        ''

    Select
        Total,
        Nome,
        Idioma
    From
        #temp
    Order By
        Total Desc
   
END  

go
   Grant Exec on spC_UsuariosPaises To public
go

Exec spC_UsuariosPaises 


