use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroBoardID'))
   Drop Procedure spC_TabuleiroBoardID
go

Create  Proc [dbo].[spC_TabuleiroBoardID]
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
        Rede.TabuleiroBoard (nolock)
    WHERE
        ID = @BoardID

End -- Sp

go
Grant Exec on spC_TabuleiroBoardID To public
go

--Exec spC_TabuleiroBoardID @TaabuleiroID=1


