USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_RE_ListaBonusPagos]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spOC_RE_ListaBonusPagos]
   @DataIni       datetime            ,
   @DataFim       datetime            ,
   @Identificacao nvarchar(255) = null,
   @CategoriaId   Int           = null

As
-- =============================================================================================
-- Author.....: 
-- Create date: 08/02/2019
-- Description: Lista os Bonus Pagos
-- =============================================================================================
BEGIN
    -- Necessario para o entity reconhecer retorno de select com tabela temporaria
    Set FMTONLY OFF
	Set NOCOUNT ON

    Create Table #TBonus
    (
        tipoRg          int,
        usuarioID       int,
        login           nvarchar(255),
        nivelAssociacao nvarchar(255),  
        ativo           int,
        categoriaNome   nvarchar(255),
        valor           float,
        data            dateTime
    );

	-- Ajusta os paramentros
	If (@Identificacao is not null and @Identificacao = '' )
	Begin
	   Set @Identificacao = null
	End

	If (@CategoriaId is not null and @CategoriaId = 0 )
	Begin
	   Set @CategoriaId = null
	End

	-- Obtem os Bonus dos Usuarios
    Insert into #TBonus
    Select 
       1                tipoRg,
       U.ID             usuarioID,
       U.Login          login,
       A.Nome           nivelAssociacao,  
       CASE
           WHEN U.DataValidade < DATEADD(day,1, dbo.GetDateZion()) THEN 0  ELSE 1
       END ativo,
       C.Nome           categoriaNome,
       B.Valor          valor,
       B.Data           data
    From 
        Rede.Bonificacao     B (Nolock),
	    Financeiro.Categoria C (Nolock),
	    Usuario.Usuario      U (Nolock),
	    Rede.Associacao      A (Nolock)
    Where     
        B.CategoriaID = C.ID
    and B.UsuarioID = U.ID
    and A.Nivel = U.NivelAssociacao
	and B.Data Between @DataIni and @DataFim
    and B.CategoriaID = Coalesce(@CategoriaId, B.CategoriaID);

	-- Elimina os usuarios que não contem a identificação como parte do login
	If(@Identificacao is not null)
	Begin
	    Delete
		    #TBonus
		Where
		    login not like '%' + @Identificacao + '%'; 
	End
	
	-- Obtem o total por Usuarios
    Insert into #TBonus
    Select 
        2           tipoRg,
        usuarioID   usuarioID,
        Max(Login)  login,
        ''          produtoNome,
        0            ativo,
        'SubTotal ' + Max(Login) categoriaNome,
        Sum(Valor)   valor,
        null         dataLancamento
    From 
        #TBonus
    Group By 
        usuarioID;

    -- Obtem o total geral
    Insert into #TBonus
    Select 
        9             tipoRg,
        999999999     usuarioID,
        'ZZZZZZZ'     login,
        ''            nivelAssociacao,      
        ''            ativo,
        'Total Geral' categoriaNome,  
        Sum(Valor)    valor,
        null          dataLancamento
    From 
        #TBonus
	Where 
        tipoRg = 1
    Group By 
        tipoRg;
   
    -- Retorna dados
    Select 
	    tipoRg       ,
        usuarioID    ,
        login        ,
        nivelAssociacao  ,
        ativo        ,
        categoriaNome,
        valor        ,
        data  
    From 
       #TBonus
    Order By  
       login,
       tipoRg, 
       data;

    -- Remove as Tabelas Temporarias
    Drop Table #TBonus
 
END;


GO
