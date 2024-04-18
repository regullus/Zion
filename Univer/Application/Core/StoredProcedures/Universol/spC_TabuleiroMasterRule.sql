use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroMasterRule'))
   Drop Procedure spC_TabuleiroMasterRule
go

Create  Proc [dbo].[spC_TabuleiroMasterRule]
    @UsuarioID int,
    @BoardID int
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Verifica se o Master do tabuleiro já possui mais que 4 pagamentos e ainda nao pagou o sistema
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
        BoardID = @BoardID 

    --Verifica se master nao pagou o sistema
    Select
        @PagoSistema = PagoSistema
    From
        Rede.TabuleiroUsuario 
    Where
        UsuarioID = @MasterID and
        BoardID = @BoardID 
    
	--Verifica total de pagamentos que master recebeu sem pagar o sistema
	Select 
		@total = Count(*)
	From 
		Rede.TabuleiroUsuario 
	Where
		MasterID = @MasterID and
		BoardID = @BoardID and
		PagoMaster = 'true'

    if(@PagoSistema = 0)
    Begin
        -- Se total for maior que 4, informa 'NOOK'
        --pois o Master deve pagar o sistema ate os 4 primeiros pagamentos
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
        --J� pagou o sistema
        Select @retorno = 'OK'
    End

	--Fevifica se master ja teve alguma indicacao
	if(@retorno = 'OK')
	Begin
	    --Nao considera os 7 principais usuarios
		if(@UsuarioID > 2586)
		Begin
		    --Faz a verificacao somente se recebeu mais que 4 pag
			if(@Total > 4)
			Begin 
				Declare
					@totalBoard int,
					@totalIndicados int

				Select 
				  Distinct BoardID
				Into
					#tempBoard
				from
					Rede.TabuleiroUsuario
				Where
					UsuarioID = 2580 

				Select 
					@totalBoard = Count(*)
				From
					#tempBoard

				Select 
				  @totalIndicados = count(*)
				from
					Usuario.Usuario
				Where
					PatrocinadorDiretoID = 2580
	
				if(@totalBoard > @totalIndicados)
				Begin
					--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
					Select @retorno = 'NOOK'
				End
			End
		End
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroMasterRule To public
go

--Exec spC_TabuleiroMasterRule @UsuarioID = 2580, @BoardID = 1



