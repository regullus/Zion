use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TabuleiroUsuarioRule'))
   Drop Procedure spC_TabuleiroUsuarioRule
go

Create  Proc [dbo].[spC_TabuleiroUsuarioRule]
    @UsuarioID int
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
        @PagoSistema bit
    	
	--Fevifica se usuario ja teve alguma indicacao
	Set @retorno = 'OK'

	--Nao considera os 7 principais usuarios
	if(@UsuarioID > 2586)
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
			TabuleiroID is not null and
			posicao not like 'Donator%'
			
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
			usu.PatrocinadorDiretoID = @UsuarioID and
			usu.ID = tab.UsuarioID and
			tab.TabuleiroID is not null 
		
		if(@totalBoard > @totalIndicados)
		Begin
			--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
			Select @retorno = 'NOOK_SEM_INDICACAO'
		End
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroUsuarioRule To public
go
--Exec spC_TabuleiroUsuarioRule @UsuarioID=2587
--Exec spC_TabuleiroUsuarioRule @UsuarioID=2822


