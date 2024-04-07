use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroMasterRule'))
   Drop Procedure spC_TabuleiroMasterRule
go

Create  Proc [dbo].[spC_TabuleiroMasterRule]
    @UsuarioID int,
    @TabuleiroID int
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Verifica se o MAster do tabuleiro já possui mais que 4 pagamentos e ainda não pagou o sistema
-- =============================================================================================

BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on
    
    Declare 
        @retorno nvarchar(4),
        @total int,
        @MasterID int,
        @PagoSistema bit

    --Obtem o MasterID do Tabuleiro do usuario informado
    Select
        @MasterID = MasterID
    From
        Rede.TabuleiroUsuario 
    Where
        UsuarioID = @UsuarioID and
        TabuleiroID = @TabuleiroID 

    --Verifica se master não pagou o sistema
    Select
        @PagoSistema = PagoSistema
    From
        Rede.TabuleiroUsuario 
    Where
        UsuarioID = @MasterID and
        TabuleiroID = @TabuleiroID 
    
    if(@PagoSistema = 0)
    Begin

        --Verifica total de pagamentos que master recebeu sem pagar o sistema
        Select 
            @total = Count(*)
        From 
            Rede.TabuleiroUsuario 
        Where
            MasterID = @MasterID and
            TabuleiroID = @TabuleiroID and
            PagoMaster = 1 

        -- Se total for maior que 4, informa 'NOOK'
        --pois o Master deve pagar o sistema até os 4 primeiros pagamentos
        if(@Total > 3)
        Begin 
            Select @retorno = 'NOOK'
        End
        Else
        Begin
            Select @retorno = 'OK'
        End
    End
    Else
    Begin
        --Já pagou o sistema
        Select @retorno = 'OK'
    End
    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroMasterRule To public
go

--Exec spC_TabuleiroMasterRule @UsuarioID = 2592, @TabuleiroID = 1



