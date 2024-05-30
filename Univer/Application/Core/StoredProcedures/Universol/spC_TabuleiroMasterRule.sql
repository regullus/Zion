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
        @PagoSistema bit,
		@InfomePagSistema bit,
		@quebraTabuleiro int
    
    -- Quebra para que em venus as regras recomecem, separando de mercurio a jupiter e depois de venus ao sol
	Set @quebraTabuleiro = 5
	--Obtem o MasterID do Tabuleiro do usuario informado
	Select
		@MasterID = MasterID
	From
		Rede.TabuleiroUsuario (nolock)
	Where
		UsuarioID = @UsuarioID and
		BoardID = @BoardID 
		
	--Caso o usuarioId passado seja o master
	if(@MasterID = @UsuarioID)
	Begin
    	--Verifica se master nao pagou o sistema
		Select
			@PagoSistema = PagoSistema,
			@InfomePagSistema = InformePagSistema
		From
			Rede.TabuleiroUsuario (nolock)
		Where
			UsuarioID = @UsuarioID and
			BoardID = @BoardID 
    
		--Verifica total de pagamentos que master recebeu sem pagar o sistema
		Select 
			@total = Count(*)
		From 
			Rede.TabuleiroUsuario (nolock)
		Where
			MasterID = @UsuarioID and
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
		
		--Caso Tenha Fechado algum lado, este não entram no count do select acima
		--Daí soma 4 no @total, pois já fechou um lado
		if Exists (
			Select 
				'OK' 
				From 
					Rede.TabuleiroUsuario (nolock)
				Where
					MasterID = @UsuarioID and
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
					Rede.TabuleiroUsuario (nolock)
				Where
					MasterID = @UsuarioID and
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
				if(@InfomePagSistema = 'true')
				Begin
					Select @retorno = 'NOOK_PAGTO_SISTEMA_INFORME_OK'
				End
				Else
				Begin
					Select @retorno = 'NOOK_PAGTO_SISTEMA'
				End
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

		if(@retorno = 'OK')
		Begin
			--Verifica se usuario esta em board superior e não pagou o master
			Declare @BoardIDMax int
			Select 
				@BoardIDMax = MAX(BoardID)
			From
				Rede.TabuleiroUsuario (nolock)
			Where
				UsuarioID = @UsuarioID and
				StatusID <> 0
			    
			--Verifica se não pagou o Master no board superior
			If Exists (
				Select 
					'OK'
				From
					Rede.TabuleiroUsuario (nolock)
				Where
					BoardID = @BoardIDMax and
					UsuarioID = @UsuarioID and
					PagoMaster = 'false'
			)
			Begin
			    --Não vale para venus
			    if(@BoardID != @quebraTabuleiro) 
				Begin
					Set @retorno = 'NOOK_BOARD_SUPERIOR'
				End
			End
		End
	End
	Else
	Begin
		-- Não sendo o master, retorna ok
		Set @retorno = 'OK'
	End
	
	--Verifica se há convites pendentes
	if Exists (
		Select 'OK'
		From
			Rede.TabuleiroUsuario (nolock)
		Where
			UsuarioID = @UsuarioID and
			StatusID = 2
	)
	Begin
		if(@retorno = 'OK')
		Begin
			 --Não vale para venus
			 --Tem que ver em que board o usuario esta se for maior q venus, da NOOK, se for menor da OK
            if(@BoardID != @quebraTabuleiro) 
            Begin
                Set @retorno = 'NOOK_CONVITE'
            End
    	End
		Else
		Begin
			Set @retorno = @retorno + '_CONVITE'
		End
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroMasterRule To public
go

Exec spC_TabuleiroMasterRule @UsuarioID=2588, @BoardID=1



