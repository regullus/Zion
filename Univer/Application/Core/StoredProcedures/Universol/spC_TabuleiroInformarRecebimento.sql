use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroInformarRecebimento'))
   Drop Procedure spC_TabuleiroInformarRecebimento
go

Create  Proc [dbo].[spC_TabuleiroInformarRecebimento]
   @UsuarioID int,
   @UsuarioPaiID int,
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
        @Count int,
        @MasterID int
    
    if not Exists (
        Select 'OK'
        From
            rede.TabuleiroUsuario 
        where 
            InformePag = 1 and
            UsuarioID = @UsuarioID and 
            BoardID  = @BoardID 
    )
    Begin
        Update
            rede.TabuleiroUsuario 
        Set
            InformePag = 1,
            UsuarioIDPag = @UsuarioPaiID
        where 
            UsuarioID = @UsuarioID and 
            BoardID  = @BoardID 
    End

    Update
        rede.TabuleiroUsuario 
    Set
        PagoMaster = 1
    where 
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 

    --Verifica se já é o 4 recebimento do master
    Select
        @MasterID = MasterID
    From
        rede.TabuleiroUsuario 
    Where
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 

    if Exists (
		Select 
			'OK' 
		From 
			rede.TabuleiroUsuario 
		Where 
			UsuarioID = @MasterID and 
			BoardID  = @BoardID and 
			PagoSistema = 1 and 
			ConviteProximoNivel = 0
		)
    Begin
        Select 
            @Count = count(*)
        From
            rede.TabuleiroUsuario 
        Where
            MasterID = @MasterID and 
            BoardID  = @BoardID and
            PagoMaster = 1 and
            Posicao in ('DonatorDirSup1','DonatorDirSup2','DonatorDirInf1','DonatorDirInf2','DonatorEsqSup1','DonatorEsqSup2','DonatorEsqInf1','DonatorEsqInf2')

        if(@count>=4)
        Begin
            --Verifica se já não esta no proximo nivel
            if not Exists (
				Select 
					'OK' 
				From 
					rede.TabuleiroUsuario 
				Where 
					UsuarioID = @MasterID and 
					BoardID  = @BoardID + 1
				)
            Begin
                --Atualiza para criar o convite para o proximo nivel
                Update
                    rede.TabuleiroUsuario
                Set 
                    ConviteProximoNivel = 1
                Where
                    UsuarioID = @MasterID and 
                    BoardID  = @BoardID 
            End
        End
    End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroInformarRecebimento To public
go

--Exec spC_TabuleiroInformarRecebimento @UsuarioID = 2590, @BoardID = 1
