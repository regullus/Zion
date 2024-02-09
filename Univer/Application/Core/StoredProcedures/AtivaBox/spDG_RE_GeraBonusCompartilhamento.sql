Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusCompartilhamento'))
   Drop Procedure spDG_RE_GeraBonusCompartilhamento
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusCompartilhamento]
   @baseDate       varchar(8) null,
   @QtdeNiveis     int = 7,
   @QtdeMinDiretos int = 5

AS
BEGIN
   BEGIN TRY
      set nocount on

      Declare @VlrBonus float = 5.00;

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

      -- Tabela temporária com usuários cadastrados com seus respectivos níveis e data de associação
      Create Table #TUsuarios
	   (	     
         PedidoId INT,	
         UsuarioPedID INT,	 	  
         ProdutoID INT,
         DtAtivacao DATETIME, 
         UsuarioID INT,
         PatrocinadorPosicaoID INT,  		     
         DtValidade DATETIME,
         Nivel  INT
	   );

      
      -- Tabela temporária com usuários Qualificados para o pagamento do bonus
      Create Table #TUsuarioQualificado
      (	     
         UsuarioID INT
      );

      -- Seleção dos pedidos pagos
      Insert Into #TUsuarios 
	   Select 
	      P.ID as PedidoId,
         U.id as UsuarioPedID,        
         PV.ProdutoID as ProdutoID,
		   PS.Data as DtAtivacao,   
		   U.id as UsuarioID,
         U.PatrocinadorPosicaoID as PatrocinadorPosicaoID, 		  
         U.DataValidade as DtValidade,
         0 as Nivel
      From 
	      Loja.pedido P (nolock), 
	      Loja.PedidoItem PI (nolock), 
         Loja.Produto PR (nolock),        
	      Loja.ProdutoValor PV (nolock),
	      Usuario.Usuario U (nolock),	 
	      Loja.PedidoPagamento PP (nolock), 
	      Loja.PedidoPagamentoStatus PS (nolock) 		
      Where P.ID = PI.PedidoID     
        and PI.ProdutoID = PR.ID
        and PR.TipoID IN (6)  -- Renovacao   
	     and PI.ProdutoID = PV.ProdutoID
	     and P.UsuarioID = U.ID	
	     and P.ID = PP.PedidoID
	     and PP.ID = PS.PedidoPagamentoID
	     and PS.StatusID = 3      -- somente pedidos pagos        
	     and U.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
	     and U.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao	   
	     and U.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago			 	
        and PS.Data BETWEEN @dataInicio AND @dataFim; --pega somente pedidos do dia	
	      
      -- Obtem os Usuarios superiores qualificados para a bonificação
      Declare @cnt INT = 1;

      While @cnt <= @QtdeNiveis
      Begin

         Insert Into #TUsuarios 
         Select 
            T.PedidoId as PedidoId,
            T.UsuarioPedID as UsuarioPedID,        
            T.ProdutoID as ProdutoID,
			   T.DtAtivacao as DtAtivacao, 
            U.id as UsuarioID,
            U.PatrocinadorPosicaoID as PatrocinadorPosicaoID,  			
            U.DataValidade as DtValidade,
            @cnt as Nivel
         From #TUsuarios T,
              Usuario.Usuario U (nolock) 
         Where T.PatrocinadorPosicaoID = U.ID
           and U.DataValidade >= @dataFim --so recebe se estiver com ativo mensal pago	
           and U.RecebeBonus = 1   -- Recebe bonus
           and T.Nivel = @cnt - 1;

         Set @cnt = @cnt + 1;
      End;

	   -- Remove os usuarios que geraram os pedidos ou não tem patrocinador
	   Delete #TUsuarios
	   Where Nivel = 0
	      or PatrocinadorPosicaoID is null;

      -- Obtem os Usuarios qualificados para o pagamento do bonus (Ter indicado x usuarios ativos)
      Insert Into #TUsuarioQualificado
      Select T.PatrocinadorPosicaoID as UsuarioID
      from #TUsuarios T,
           Usuario.Usuario U (nolock)
      Where T.PatrocinadorPosicaoID = U.PatrocinadorDiretoID      
        and U.DataValidade >= @dataInicio
        and U.GeraBonus = 1
      Group by T.PatrocinadorPosicaoID
      Having count(U.ID) >= @QtdeMinDiretos;

      -- Gera Bonus
      BEGIN TRANSACTION

      Insert Into Rede.Bonificacao
        (CategoriaID,
         UsuarioID,
         ReferenciaID,
         StatusID,
         Data,
         Valor,
         PedidoID)
	   Select 
         16 as CategoriaID, -- Bonus Consumo
         T.PatrocinadorPosicaoID as Usuario,
         T.UsuarioPedID as Referencia,
         0 as StatusID,
         T.DtAtivacao as Data,
         @VlrBonus as Valor,
         T.PedidoId as PedidoID
	   From
		   #TUsuarios AS T,
         #TUsuarioQualificado TQ
      where 
         T.PatrocinadorPosicaoID = TQ.UsuarioID;
	  
      COMMIT TRANSACTION

      --Select * from #TUsuarios

      -- Remove todas as tabelas temporárias
      Drop Table #TUsuarios;
      Drop Table #TUsuarioQualificado;

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusCompartilhamento: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on spDG_RE_GeraBonusCompartilhamento To public
go

-- Exec spDG_RE_GeraBonusCompartilhamento null, 7, 5
-- Exec spDG_RE_GeraBonusCompartilhamento '20170621', 7, 5