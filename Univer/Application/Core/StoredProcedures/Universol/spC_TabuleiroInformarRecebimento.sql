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
-- Description: 
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
		PagoMaster = 'true',
		InformePag = 'true',
        ciclo = ciclo + 1,
        UsuarioIDPag = @UsuarioPaiID
    where 
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 
    
    --Atualiza recebimento do Master
    Select
        @MasterID = MasterID
    From
        rede.TabuleiroUsuario (nolock)
    Where
        UsuarioID = @UsuarioID and 
        BoardID  = @BoardID 

	Update
		rede.TabuleiroUsuario 
	Set
		TotalRecebimento = TotalRecebimento + 1
    Where
        UsuarioID = @MasterID and 
        BoardID  = @BoardID 

    if Exists (
		Select 
			'OK' 
		From 
			rede.TabuleiroUsuario (nolock)
		Where 
			UsuarioID = @MasterID and 
			BoardID  = @BoardID and 
			StatusID = 1
		)
    Begin
        Select 
            @Count = TotalRecebimento
        From
            rede.TabuleiroUsuario (nolock)
        Where
            MasterID = @MasterID and 
            BoardID  = @BoardID 

        if(@count>=4)
        Begin
            --Verifica se ja nao esta no proximo nivel
            if not Exists (
				Select 
					'OK' 
				From 
					rede.TabuleiroUsuario (nolock) 
				Where 
					UsuarioID = @MasterID and 
					BoardID  = @BoardID + 1
				)
            Begin
			    --Verifica se jah pagou o sistema
				if Exists (
					Select 
						'OK' 
					From 
						rede.TabuleiroUsuario (nolock)
					Where 
						UsuarioID = @MasterID and 
						BoardID  = @BoardID and
						PagoSistema = 'true'
					)
				Begin
					--Atualiza para criar o convite para o proximo nivel
					Update
						rede.TabuleiroUsuario
					Set 
						StatusID = 2, --Convite Proximo Nivel
                        DataInicio = GetDate()
					Where
						UsuarioID = @MasterID and 
						BoardID  = @BoardID + 1 and
						PagoSistema = 'true' --tem q ter pago o sistema
				End
            End
        End
    End

    Select 'OK'

End -- Sp

go
Grant Exec on spC_TabuleiroInformarRecebimento To public
go

--Exec spC_TabuleiroInformarRecebimento @UsuarioID=2591,@UsuarioPaiID=2580,@BoardID =1