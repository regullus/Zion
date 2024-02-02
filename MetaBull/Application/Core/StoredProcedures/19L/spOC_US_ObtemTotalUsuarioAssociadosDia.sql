use [19L]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_US_ObtemTotalUsuarioAssociadosDia') )
   Drop Procedure spOC_US_ObtemTotalUsuarioAssociadosDia
go

Create Proc [dbo].spOC_US_ObtemTotalUsuarioAssociadosDia
    @baseDate VARCHAR(8)
   
As
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 13/02/2019
-- Description: Obtem o total de usuarios associados por dia
-- =============================================================================================

BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF;
	Set NOCOUNT ON;
	
	Create Table #Qtde
    ( 
        Dia int,
        Qtd int
    );

	Create Table #Mes
    ( 
        Dia int,
        Qtd int
    );

    Create Table #Retorno
    ( 
        Dia  int,
        Qtd  int,
		Total Int
    );

	Declare 
	    @DtInicio datetime = CAST(Substring(@baseDate,1,6) + '01 00:00:00' as datetime2),
        @DtFim    datetime = CAST(@baseDate + ' 23:59:59' as datetime2);


    -- Inicializa a tabela com os dias do mes as a data informada
    Declare
	    @count    int      = 0 , 
	    @Total    int      = Day(@DtFim);
	
    While @count <= @Total
    Begin
	    Insert Into #Mes Values (@count, 0);
	    Set @count = @count + 1;
    End


	-- Calcula a quantidade total dos meses anteriores
    Insert Into #Qtde
    select 
        0 as Dia,
	    COUNT(ID) as Qtd
    from 
        Usuario.Usuario U (Nolock)
    Where 
        U.DataAtivacao is not null
    and U.DataAtivacao < @DtInicio;


    -- Calcula a quantidade por dia
    Insert Into #Qtde
    Select
        Day(U.DataAtivacao) as Dia, 
        COUNT(ID)           as Qtd
    from 
        Usuario.Usuario U (Nolock)
    Where 
        U.DataAtivacao is not null
    and U.DataAtivacao Between @DtInicio and  @DtFim 
    Group By
        Day(U.DataAtivacao);


    -- Atualiza Quantidade na Tabela Retorno
	Update 
	    #Mes
	Set
	    Qtd = Q.Qtd
	From 
	    #Qtde Q,
		#Mes  R
	Where 
	    Q.Dia = R.Dia;

    -- Calcula o acumulado por dia
    Declare
	    @dia     INT,
        @qtd     INT,
	    @soma    INT = 0;

    Declare curQ Cursor For Select Dia, Qtd From #Mes order by Dia;
    Open curQ;

    if (Select @@CURSOR_ROWS) <> 0
    Begin
        Fetch Next From curQ Into @dia, @qtd;
 
        While @@fetch_status = 0
        Begin
            Set @soma = @soma + @qtd;

			Insert Into #Retorno Values (@dia, @qtd, @soma )
   	       
            Fetch Next From curQ Into @dia, @qtd;
        End   
    End

    Close curQ;
    Deallocate curQ;

	-- Retorno
	Select 
         convert(varchar,Dia)   as Name,
        Total as Value
	From  
        #Retorno 
    where 
	    Dia > 0
	order by 
        Dia;
 
    -- Remove as tabelas
	Drop Table #Qtde;
	Drop Table #Mes;
	Drop Table #Retorno;
		       
END; -- Sp

go
Grant Exec on spOC_US_ObtemTotalUsuarioAssociadosDia To public
go

 --Exec spOC_US_ObtemTotalUsuarioAssociadosDia '20190219'
