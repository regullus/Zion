use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUltimoAcesso'))
   Drop Procedure spC_TabuleiroUltimoAcesso
go

Create  Proc [dbo].[spC_TabuleiroUltimoAcesso]
   @UsuarioID int,
   @Chamada nvarchar(50)

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Inclui os 8 registros do usuario que inicia no sistema
-- Observacao: Só deve rodar essa sp no inicio, onde o usuario acaba de aceitar um convite para entrar no sistema
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
	Set FMTONLY OFF
	Set nocount on
	
    declare 
        @tempoDuplicado datetime,
        @dataAtual datetime,
        @retorno nvarchar(4)

    set @retorno = 'OK'

    Select
        @tempoDuplicado = DATEADD(ss, 3, TempoProcess),
        @dataAtual = getdate()
    From
        Rede.TabuleiroUltimoAcesso
    Where
        UsuarioID = @UsuarioID And
        Chamada = @Chamada

    if(@tempoDuplicado is null)
    Begin
        Set @retorno = 'OK'
    End
    Else
    Begin
        if(@tempoDuplicado < @dataAtual)
        Begin
            set @retorno = 'OK'
        End
        Else
        Begin
            set @retorno = 'NOOK'
        End
    End
    
    Select @retorno Retorno

End -- Sp

go
Grant Exec on spC_TabuleiroUltimoAcesso To public
go

--Exec spI_TabuleiroUltimoAcesso @UsuarioID=2580
Exec spC_TabuleiroUltimoAcesso @UsuarioID=2580, @Chamada = 'GetInvite'

