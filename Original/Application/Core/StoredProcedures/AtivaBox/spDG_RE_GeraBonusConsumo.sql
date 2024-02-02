Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusConsumo'))
   Drop Procedure spDG_RE_GeraBonusConsumo
go

-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 25/05/2017
-- Description: Gera registros de Bonificacao sobre o consumo
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusConsumo]
   @baseDate   varchar(8) null

AS
BEGIN
   BEGIN TRY
      set nocount on

      Declare @VlrBonus float = 10.00;

	   Declare @dataInicio datetime,
              @dataFim    datetime;
      
      if (@baseDate is null)
      Begin 
         SET @dataInicio = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);
      End
      Else
      Begin
         SET @dataInicio = CAST(@baseDate + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(@baseDate + ' 23:59:59' as datetime2);
      End

      -- Obtem a Rede Sequencial
      ;WITH RedeSequencial AS (
	      SELECT 
            RANK() OVER (ORDER BY ID) AS Posicao, ID, Login, GeraBonus, RecebeBonus, DataValidade
	      FROM Usuario.Usuario (nolock) )

         SELECT Posicao, ID, Login, GeraBonus, RecebeBonus, DataValidade,
            CONVERT(DECIMAL, SUBSTRING(CONVERT(VARCHAR,((Posicao-1)-0.005)/4),0,CHARINDEX('.',CONVERT(VARCHAR,((Posicao-1)-0.005)/4),0)))+1 N1,
            CONVERT(DECIMAL, 0) N2,
            CONVERT(DECIMAL, 0) N3
         INTO #RedeSequencial
         FROM RedeSequencial;

      UPDATE #RedeSequencial
      SET N2 = CONVERT(DECIMAL,SUBSTRING(CONVERT(VARCHAR,((N1-1)-0.005)/4),0,CHARINDEX('.',CONVERT(VARCHAR,((N1-1)-0.005)/4),0)))+1;

      UPDATE #RedeSequencial
      SET N3 = CONVERT(DECIMAL,SUBSTRING(CONVERT(VARCHAR,((N2-1)-0.005)/4),0,CHARINDEX('.',CONVERT(VARCHAR,((N2-1)-0.005)/4),0)))+1;

      UPDATE #RedeSequencial
      SET N2 = CASE WHEN (N1 = 1) THEN 0 ELSE N2 END,
          N3 = CASE WHEN (N2 = 1) THEN 0 ELSE N3 END

    
      -- Tabela temporária com os pedidos dos usuários que pagaram a associação
      Create Table #Pedidos
	   (	     
         PedidoId INT,	
         UsuarioID INT,	 	  
         ProdutoID INT,
         DtPgto DATETIME
	   );

      -- Seleção dos pedidos pagos
      INSERT INTO #Pedidos 
	   SELECT 
	      P.ID as PedidoId,
         U.id as UsuarioID,        
         PV.ProdutoID as ProdutoID,
		   PS.Data as DtPgto   		  
      FROM 
	      Loja.pedido P (nolock), 
	      Loja.PedidoItem PI (nolock), 
         Loja.Produto PR (nolock),        
	      Loja.ProdutoValor PV (nolock),
	      Usuario.Usuario U (nolock),	 
	      Loja.PedidoPagamento PP (nolock), 
	      Loja.PedidoPagamentoStatus PS (nolock) 		
      WHERE P.ID = PI.PedidoID     
        and PI.ProdutoID = PR.ID
        and PR.TipoID IN (1)  -- Associacao 
	     and PI.ProdutoID = PV.ProdutoID
	     and P.UsuarioID = U.ID	
	     and P.ID = PP.PedidoID
	     and PP.ID = PS.PedidoPagamentoID
	     and PS.StatusID = 3      -- somente pedidos pagos        
	     and U.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao	   
	     and U.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago			 	
        and PS.Data BETWEEN @dataInicio AND @dataFim; --pega somente pedidos do dia	

      
      -- Gera Bonus
      BEGIN TRANSACTION

      -- Gera Nivel 3
      INSERT INTO Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   SELECT 
         15 as CategoriaID, -- Bonus Consumo
         TU.ID as UsuarioID,
         TP.UsuarioID as Referencia,
         0 as StatusID,
         TP.DtPgto as Data,
         @VlrBonus as Valor,
         TP.PedidoId as PedidoID
      FROM #Pedidos TP,
           #RedeSequencial TR,
           #RedeSequencial TU
      WHERE TP.UsuarioID = TR.ID
        and TR.N3 = TU.Posicao
        and TU.RecebeBonus = 1;  -- Somente ususarios que rebem bonus 1= sim, 0 = nao	 

      -- Gera Nivel 2
      INSERT INTO Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   SELECT 
         15 as CategoriaID, -- Bonus Consumo
         TU.ID as UsuarioID,
         TP.UsuarioID as Referencia,
         0 as StatusID,
         TP.DtPgto as Data,
         @VlrBonus as Valor,
         TP.PedidoId as PedidoID
      FROM #Pedidos TP,
           #RedeSequencial TR,
           #RedeSequencial TU
      WHERE TP.UsuarioID = TR.ID
        and TR.N2 = TU.Posicao 
        and TU.RecebeBonus = 1;  -- Somente ususarios que rebem bonus 1= sim, 0 = nao	 

      -- Gera Nivel 1
      INSERT INTO Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   SELECT 
         15 as CategoriaID, -- Bonus Consumo
         TU.ID as UsuarioID,
         TP.UsuarioID as Referencia,
         0 as StatusID,
         TP.DtPgto as Data,
         @VlrBonus as Valor,
         TP.PedidoId as PedidoID
      FROM #Pedidos TP,
           #RedeSequencial TR,
           #RedeSequencial TU
      WHERE TP.UsuarioID = TR.ID
        and TR.N1 = TU.Posicao
        and TU.RecebeBonus = 1;  -- Somente ususarios que rebem bonus 1= sim, 0 = nao	 
     
      COMMIT TRANSACTION

      --Select * from ##Pedidos

      -- Remove todas as tabelas temporárias
      Drop Table #RedeSequencial;
      Drop Table #Pedidos;

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION;
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusConsumo: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on spDG_RE_GeraBonusConsumo To public
go

-- Exec spDG_RE_GeraBonusConsumo null, 3
-- Exec spDG_RE_GeraBonusConsumo '20170621', 3