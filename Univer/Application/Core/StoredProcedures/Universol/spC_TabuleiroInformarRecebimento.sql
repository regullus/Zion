use Univer
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformarRecebimento'))
   Drop Procedure spC_TabuleiroInformarRecebimento
go

Create  Proc [dbo].[spC_TabuleiroInformarRecebimento]
   @UsuarioID int,
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
    
    Declare 
        @Count int,
        @MasterID int
    
    Update
        rede.TabuleiroUsuario 
    Set
        PagoMaster = 1
    where 
        UsuarioID = @UsuarioID and 
        TabuleiroID  = @TabuleiroID

    --Verifica se já é o 4 recebimento do master
    Select
        @MasterID = MasterID
    From
        rede.TabuleiroUsuario 
    Where
        UsuarioID = @UsuarioID and 
        TabuleiroID  = @TabuleiroID
    if Exists (Select 'OK' From rede.TabuleiroUsuario Where UsuarioID = @MasterID and TabuleiroID  = @TabuleiroID and PagoSistema = 1 and ConviteProximoNivel = 0)
    Begin
        Select 
            @Count = count(*)
        From
            rede.TabuleiroUsuario 
        Where
            MasterID = @MasterID and 
            TabuleiroID  = @TabuleiroID and
            PagoMaster = 1 and
            Posicao in ('DonatorDirSup1','DonatorDirSup2','DonatorDirInf1','DonatorDirInf2','DonatorEsqSup1','DonatorEsqSup2','DonatorEsqInf1','DonatorEsqInf2')

        if(@count>=4)
        Begin
            --Verifica se já não esta no proximo nivel
            if not Exists (Select 'ok' From rede.TabuleiroUsuario Where UsuarioID = @MasterID and TabuleiroID = @TabuleiroID + 1)
            Begin
                --Atualiza para criar o convite para o proximo nivel
                Update
                    rede.TabuleiroUsuario
                Set 
                    ConviteProximoNivel = 1
                Where
                    UsuarioID = @MasterID and 
                    TabuleiroID  = @TabuleiroID
            End
        End
    End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroInformarRecebimento To public
go

--Exec spC_TabuleiroInformarRecebimento @UsuarioID = 2590, @TabuleiroID = 1

--select * from rede.TabuleiroUsuario where UsuarioId = 2587
