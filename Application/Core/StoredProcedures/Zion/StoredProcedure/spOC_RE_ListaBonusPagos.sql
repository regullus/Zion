
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spOC_RE_ListaBonusPagos'))
   Drop Procedure  spOC_RE_ListaBonusPagos
go

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

   Create Table #TBonus (
      tipoRg          int,
      usuarioID       int,
      login           nvarchar(255),
      nivelAssociacao nvarchar(255),  
      ativo           int,
      categoriaNome   nvarchar(255),
      valor           float,
      data            dateTime,
	  descricao       nvarchar(255),
	  pedidoID        int,
	  loginPedido     nvarchar(255)
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
      C.Nome               categoriaNome,
      B.Valor              valor,
      B.Data               data,
	  B.Descricao          descricao,
	  B.PedidoID pedidoID,
	  ''                   loginPedido   
   From 
      Rede.Bonificacao     B (Nolock),
	  Financeiro.Categoria C (Nolock),
	  Usuario.Usuario      U (Nolock),
	  Rede.Associacao      A (Nolock)
   Where B.CategoriaID = C.ID
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

   -- Obtem os usuarios dos pedidos
   Update
      #TBonus
   Set 
      loginPedido = U.Login
   From
      #TBonus T,
	  Loja.Pedido P (nolock),
	  Usuario.Usuario U (nolock)
   Where T.pedidoID = P.ID
     and P.UsuarioID = U.ID;
       
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
      null         data,
	  ''           descricao,
	  0   	       pedidoID,
	  ''           loginPedido  
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
      null          data,
	  ''            descricao,
	  0   	        pedidoID,
	  ''            loginPedido  
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
      nivelAssociacao,
      ativo        ,
      categoriaNome,
      valor        ,
      data         ,
	  descricao    ,
	  pedidoID     ,
	  loginPedido
   From 
      #TBonus
   Order By  
      login,
      tipoRg, 
      data;

   -- Remove as Tabelas Temporarias
   Drop Table #TBonus
 
END;

go
Grant Exec on spOC_RE_ListaBonusPagos To public
go


--Exec spOC_RE_ListaBonusPagos @DataIni= '2018-11-01 01:00:00.000', 
--                             @DataFim= '2019-02-28 01:00:00.000',
--							   @Identificacao='Batman' 
							 
--Exec spOC_RE_ListaBonusPagos @DataIni= '2018-11-01 01:00:00.000', 
--							   @DataFim= '2019-02-28 01:00:00.000'

--Exec spOC_RE_ListaBonusPagos @DataIni= '2018-11-01 01:00:00.000', 
--							   @DataFim= '2019-02-28 01:00:00.000',
--							   @CategoriaId = 13
							 
--Exec spOC_RE_ListaBonusPagos @DataIni= '2020-01-13 01:00:00.000', 
--							 @DataFim= '2020-02-06 01:00:00.000',
--							 @Identificacao='', 
--							 @CategoriaId = 14