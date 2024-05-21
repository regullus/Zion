use univerDev
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
-- Description: Verifica se o Usuario do tabuleiro esta ok com as indicacaoes
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
			@totalBoard = Count(*)
		from
			Rede.TabuleiroUsuario (nolock)
		Where
			UsuarioID = @UsuarioID and
			TabuleiroID is not null 

        if(@totalBoard is null)
        Begin
            set @totalBoard = 0
        End

		Select 
			@totalIndicados = count(*)
		from
			Usuario.Usuario usu (nolock),
			Rede.TabuleiroUsuario tab (nolock)
		Where
			usu.PatrocinadorDiretoID = @UsuarioID and
			usu.ID = tab.UsuarioID and
			tab.PagoMaster = 'true' and
			tab.TabuleiroID is not null 
		
		if(@totalBoard > 1)
		Begin
			Set @totalBoard = @totalBoard -1
		End

		--Select @totalIndicados totalIndicados, @totalBoard totalBoard

        if(@totalIndicados < @totalBoard)
		Begin
			--Usuario nao tem indicacoes maior ou igual ao numero de tabuleiros que ele pertence
			Select @retorno = 'NOOK_SEM_INDICACAO'
		End
		Else
		Begin
			Select @retorno = 'OK'
		End
	End

    Select @retorno as Retorno
End -- Sp

go
Grant Exec on spC_TabuleiroUsuarioRule To public
go
Exec spC_TabuleiroUsuarioRule @UsuarioID=5334
