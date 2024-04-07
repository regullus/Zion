use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroBoard'))
   Drop Procedure spC_TabuleiroBoard
go

Create  Proc [dbo].[spC_TabuleiroBoard]
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
    Declare @BoardID int

    Select
        @BoardID = BoardID
    From
        Rede.Tabuleiro
    Where
        ID = @TabuleiroID

    SELECT 
        ID,
        Nome,
        Cor,
        CorTexto,
        GroupID,
        DataInicial,
        DataFinal,
        Ativo,
        Licenca,
        Transferencia,
        indicados
    FROM 
        Rede.TabuleiroBoard
    WHERE
        ID = @BoardID

End -- Sp

go
Grant Exec on spC_TabuleiroBoard To public
go

--Exec spC_TabuleiroBoard @TabuleiroID = 3


