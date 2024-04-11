use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroNivel'))
   Drop Procedure spC_TabuleiroNivel
go

Create  Proc [dbo].[spC_TabuleiroNivel]
   @UsuarioID int,
   @StatusID int

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
   --StatusId = 1 Usuario convidado a entrar no tabuleiro pagamento não efetuado
   --StatusId = 2 Usuario ativo no tabuleiro
   --StatusId = 3 Usuario finalizou o tabuleiro
   Declare @count int

   Create Table #temp (
        ID int,
        UsuarioID int,
        BoardID int,
        BoardNome nvarchar(50),
        BoardCor nvarchar(50),
        TabuleiroID  int,
        Posicao nvarchar(100),
        DataInicio int,
        DataFim int,
        StatusID int,
        Observacao nvarchar(255)
   )

   Create Table #temp3 (
        ID int,
        UsuarioID int,
        TabuleiroUsuarioID int,
        BoardID int,
        BoardNome nvarchar(50),
        BoardCor nvarchar(50),
        TabuleiroID  int,
        Posicao nvarchar(100),
        DataInicio int,
        DataFim int,
        StatusID int,
        Observacao nvarchar(255)
   )

   If(@StatusID = null)
    Begin
        Insert Into #temp
        SELECT
            tn.ID,
            tn.UsuarioID,
            tn.BoardID,
            boa.Nome as BoardNome,
            boa.Cor as BoardCor,
            tab.TabuleiroID as TabuleiroID,
            tab.Posicao,
            tn.DataInicio,
            tn.DataFim,
            tn.StatusID,
            tn.Observacao
        FROM 
            Rede.TabuleiroNivel tn,
            Rede.TabuleiroBoard boa,
            Rede.TabuleiroUsuario tab
        WHERE
            tn.UsuarioID = @UsuarioID and
            tn.BoardID = boa.id and
            tn.UsuarioID = tab.UsuarioID and
            tn.BoardID = tab.boardID and
            tab.StatusID = 1
        Order By 
            StatusID
    End

   If(@StatusID = 1)
    Begin
        Insert Into #temp
        SELECT
            tn.ID,
            tn.UsuarioID,
            tn.BoardID,
            boa.Nome as BoardNome,
            boa.Cor as BoardCor,
            0 as TabuleiroID,
            '' as Posicao,
            tn.DataInicio,
            tn.DataFim,
            tn.StatusID,
            tn.Observacao
        FROM 
            Rede.TabuleiroNivel tn,
            Rede.TabuleiroBoard boa
        WHERE
            tn.UsuarioID = @UsuarioID and
            tn.StatusID = 1 and
            tn.BoardID = boa.id
        Order By 
            StatusID
        
        -- ******* Inicio Cursor *******
        Declare
            @ID int,
            @BoardID int,
            @AntFetch int,
            @TabuleiroID int

        Declare
            curRegistro 
        Cursor Local For
            Select 
                ID,
                BoardID
            FROM 
                #temp

        Open curRegistro
        Fetch Next From curRegistro Into  @ID, @BoardID
        Select @AntFetch = @@fetch_status
        While @AntFetch = 0
        Begin
            Select top(1)
                @TabuleiroID = ID
            From
                Rede.TabuleiroUsuario
            Where
                StatusID = 1 and
                BoardID = @BoardID and
                Posicao = 'Master'
            Order by
                ID

            Update 
                #temp
            Set
                TabuleiroID = @TabuleiroID
            Where
                ID = @ID

            Fetch Next From curRegistro Into @ID, @BoardID
            Select @AntFetch = @@fetch_status       
        End
    End

   If(@StatusID = 2)
    Begin
        Insert Into #temp
        SELECT
            tn.ID,
            tn.UsuarioID,
            tn.BoardID,
            boa.Nome as BoardNome,
            boa.Cor as BoardCor,
            tab.TabuleiroID as TabuleiroID,
            tab.Posicao,
            tn.DataInicio,
            tn.DataFim,
            tn.StatusID,
            tn.Observacao
        FROM 
            Rede.TabuleiroNivel tn,
            Rede.TabuleiroBoard boa,
            Rede.TabuleiroUsuario tab
        WHERE
            tn.UsuarioID = @UsuarioID and
            tn.StatusID = 2 and
            tn.BoardID = boa.id and
            tn.UsuarioID = tab.UsuarioID and
            tn.BoardID = tab.boardID and
            tab.StatusID = 1
        Order By 
            StatusID
    End

    Select 
        ID,
        UsuarioID,
        BoardID,
        BoardNome,
        BoardCor,
        TabuleiroID ,
        Posicao,
        DataInicio,
        DataFim,
        StatusID,
        Observacao
    From 
        #temp
    Order By
        TabuleiroID
    
End -- Sp

go
Grant Exec on spC_TabuleiroNivel To public
go
Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 1

--Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 2
--Exec spC_TabuleiroNivel @UsuarioID = 2581, @StatusID = 2
--Exec spC_TabuleiroNivel @UsuarioID = 2580, @StatusID = 3




