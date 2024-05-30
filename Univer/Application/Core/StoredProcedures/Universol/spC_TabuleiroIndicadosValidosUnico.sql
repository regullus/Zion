use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroIndicadosValidosUnico'))
   Drop Procedure spC_TabuleiroIndicadosValidosUnico
go

Create  Proc [dbo].[spC_TabuleiroIndicadosValidosUnico]
    @UsuarioID int,
	@BoardID int,
    @retorno nvarchar(10) output
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
        @retornoCursor nvarchar(10),
        @total int,
        @PagoSistema bit,
		@totalBoard int,
		@totalPagoMaster int,
		@count int

	set @retorno = 'NOOK'	
	set @count = 0
	Set @totalPagoMaster = 0

	-- ******* Inicio Cursor *******
	Declare
		@ID int,
		@AntFetch int

	--Cursor
	Declare
		curRegistro 
	Cursor Local For
		Select distinct
		    usu.ID ID
	    From
		    Usuario.Usuario usu,
		    Rede.TabuleiroUsuario tab,
		    Rede.TabuleiroBoard boa
	    Where
		    usu.PatrocinadorDiretoID = @UsuarioID and
		    usu.id = tab.UsuarioID and
		    tab.TabuleiroID is not null and
		    tab.BoardID = boa.id 

	Open curRegistro
	Fetch Next From curRegistro Into  @ID
	Select @AntFetch = @@fetch_status
	While @AntFetch = 0
	Begin
        --loop em todos os usuarios indicados para ver se pagaram o master
		--Pois pela regra 
		--O usuario esta ativo se ele pagou o alvo das galaxias que ele j� pertence
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
			--Usuario nao tem pagamento maior ou igual ao numero de tabuleiros que ele pertence
			Set @retornoCursor = 'NOOK'
		End
        Else
		Begin
			--Usuario nao tem pagamento maior ou igual ao numero de tabuleiros que ele pertence
			Set @retornoCursor = 'OK'
		End

		if (@retornoCursor = 'OK')
		Begin
			--S� efetua o acrecimo de usuarios filhos que est�o ok
			set @count = @count + 1
		End
           
		--Proxima linha do cursor
		Fetch Next From curRegistro Into @ID
		Select @AntFetch = @@fetch_status       
	End -- While
      
	Close curRegistro
	Deallocate curRegistro
	
	--Verifica quantos Boards o usuario esta em que seja master
	Select 
		@totalBoard = count(*)
	from
		Rede.TabuleiroUsuario (nolock)
	Where
		UsuarioID = @UsuarioID and
		MasterID = @UsuarioID and
		TabuleiroID is not null
	
	if(@totalBoard > @count)
	Begin
        --Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
		Set @retorno = 'NOOK'
	End
    Else
    Begin
        Set @retorno = 'OK'
    End
	
	--Os 7 usuarios principais, n�o entram na regra
	If(@UsuarioID <= 2586)
	Begin
		Set @retorno = 'OK'
	End
    
End -- Sp

go
Grant Exec on spC_TabuleiroIndicadosValidosUnico To public
go

Declare @indicadoDoIndicado nvarchar(10)
--cro015
exec spC_TabuleiroIndicadosValidosUnico @UsuarioID=2841, @BoardID=1, @retorno = @indicadoDoIndicado output
Select @indicadoDoIndicado indicadoDoIndicado
--cro016
exec spC_TabuleiroIndicadosValidosUnico @UsuarioID=2842, @BoardID=1, @retorno = @indicadoDoIndicado output
Select @indicadoDoIndicado indicadoDoIndicado





