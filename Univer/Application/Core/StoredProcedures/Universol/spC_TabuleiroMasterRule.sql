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
        @retorno nvarchar(50),
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
		PagoMaster = 'true' and
		Posicao in (
			'DonatorDirSup1',
			'DonatorDirSup2',
			'DonatorDirInf1',
			'DonatorDirInf2',
			'DonatorEsqSup1',
			'DonatorEsqSup2',
			'DonatorEsqInf1',
			'DonatorEsqInf2'
		)
		
	--Caso Tenha Fechado algum lado, este não entram no conunt do select acima
	--Daí soma 4 no @total, pois já fechou um lado
	if Exists (
		Select 
			'OK' 
			From 
				Rede.TabuleiroUsuario 
			Where
				MasterID = @MasterID and
				BoardID = @BoardID and
				DireitaFechada = 'true'
		)
	Begin
		Set @total = @total + 4
	End
	if Exists (
		Select 
			'OK' 
			From 
				Rede.TabuleiroUsuario 
			Where
				MasterID = @MasterID and
				BoardID = @BoardID and
				EsquerdaFechada = 'true'
		)
	Begin
		Set @total = @total + 4
	End
			
    if(@PagoSistema = 'false')
    Begin
        -- Se total for maior que 4, informa 'NOOK'
        --pois o Master deve pagar o sistema ate os 4 primeiros pagamentos
        if(@Total > 3)
        Begin 
            Select @retorno = 'NOOK_PAGTO_SISTEMA'
        End
        Else
        Begin
            Select @retorno = 'OK'
        End
    End
    Else
    Begin
        --Jah pagou o sistema
        Select @retorno = 'OK'
    End

	--Fevifica se master ja teve alguma indicacao

	--Nao considera os 7 principais usuarios
	if(@UsuarioID > 2586)
	Begin
	    
		--Faz a verificacao somente se recebeu mais que 1 pag
		if(@Total >= 1)
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
				UsuarioID = @UsuarioID and
				TabuleiroID is not null
			
			Select 
				@totalBoard = Count(*)
			From
				#tempBoard
				
			Select 
				@totalIndicados = count(*)
			from
				Usuario.Usuario usu,
				Rede.TabuleiroUsuario tab
			Where
				usu.PatrocinadorDiretoID = 2588 and
				usu.ID = tab.UsuarioID and
				tab.InformePag = 'true' and
				tab.PagoMaster = 'true' and
				tab.TabuleiroID is not null
	
			if(@totalBoard > @totalIndicados)
			Begin
				--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
				if(@Retorno = 'NOOK_PAGTO_SISTEMA')
				Begin
					Select @retorno = 'NOOK_PAGTO_SISTEMA_SEM_INDICACAO'
				End
				Else
				Begin
					Select @retorno = 'NOOK_SEM_INDICACAO'
				End
			End
		End
	End
	

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroMasterRule To public
go

Exec spC_TabuleiroMasterRule @UsuarioID=2588, @BoardID=1



