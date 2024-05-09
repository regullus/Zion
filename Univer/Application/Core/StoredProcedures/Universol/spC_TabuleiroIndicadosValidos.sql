use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroIndicadosValidos'))
   Drop Procedure spC_TabuleiroIndicadosValidos
go

Create  Proc [dbo].[spC_TabuleiroIndicadosValidos]
    @UsuarioID int,
	@BoardID int
As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Verifica se o Master do tabuleiro jah possui mais que 4 pagamentos e ainda nao pagou o sistema
-- =============================================================================================

BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
    Set nocount on
    
	Declare 
        @retorno nvarchar(10),
		@retornoCursor nvarchar(10),
        @total int,
        @PagoSistema bit,
		@totalBoard int,
		@totalPagoMaster int,
		@count int

	set @retorno = 'OK'	
	set @count = 0
	Set @totalPagoMaster = 0

	if(@UsuarioID > 2586)
	Begin
		-- ******* Inicio Cursor *******
		Declare
			@ID int,
			@AntFetch int

		--Cursor
		Declare
			curRegistro 
		Cursor Local For
			Select
				ID
			From
				usuario.usuario (nolock)
			Where
				PatrocinadorDiretoID = @UsuarioID

		Open curRegistro
		Fetch Next From curRegistro Into  @ID
		Select @AntFetch = @@fetch_status
		While @AntFetch = 0
		Begin
		    --loop em todos os usuarios indicados para ver se pagaram o master
			--Pois pela regra 
			--O usuario esta ativo se ele pagou o alvo das galaxias que ele já pertence
			--O indidicado esta ativo se obedecer a regra acima
			
			set @retornoCursor = 'OK'
			
			--Total de Boards que o usuario filho se encontra
			Select 
				@totalBoard = count(*)
			from
				Rede.TabuleiroUsuario (nolock)
			Where
				UsuarioID = @ID and
				TabuleiroID is not null

			--Total de pagamentos ao master dos Boards que o usuario filho efetuou
			Select 
				@totalPagoMaster = @totalPagoMaster + count(*)
			from
				Rede.TabuleiroUsuario (nolock)
			Where
				UsuarioID = @ID and
				PagoMaster = 'true' and
				TabuleiroID is not null
			
			if(@totalBoard > @totalPagoMaster)
			Begin
				--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
				Set @retornoCursor = 'NOOK'
			End
		
			if (@retornoCursor = 'OK')
			Begin
			    --Só efetua o acrecimo de usuarios filhos que estão ok
				set @count = @count + 1
			End

			--Proxima linha do cursor
			Fetch Next From curRegistro Into @ID
			Select @AntFetch = @@fetch_status       
		End -- While
      
		Close curRegistro
		Deallocate curRegistro
	End
	
	--Verifica quantos Boards o usuario esta em que seja master
	Select 
		@totalBoard = count(*)
	from
		Rede.TabuleiroUsuario (nolock)
	Where
		UsuarioID = @UsuarioID and
		MasterID = @UsuarioID and
		TabuleiroID is not null
	
	if(@totalBoard > @totalPagoMaster)
	Begin
		--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
		Set @retorno = 'NOOK'
	End
	--Se for o 1º pagamento tudo bem ele receber
	if Exists (
		Select 
			'OK' 
		From 
			Rede.TabuleiroUsuario (nolock)
		Where
			UsuarioID = @UsuarioID and
			BoardID = @BoardID and
			TotalRecebimento = 0
		)
		Begin
			Set @retorno = 'OK'
		End
	
	--Os 7 usuarios principais, não entram na regra
	If(@UsuarioID <= 2586)
	Begin
		Set @retorno = 'OK'
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroIndicadosValidos To public
go

--Exec spC_TabuleiroIndicadosValidos @UsuarioID=2581, @BoardID=1



