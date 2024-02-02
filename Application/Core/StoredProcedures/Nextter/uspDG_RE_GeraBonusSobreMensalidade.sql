Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('uspDG_RE_GeraBonusSobreMensalidade'))
   Drop Procedure uspDG_RE_GeraBonusSobreMensalidade;
go

Create PROCEDURE [dbo].[uspDG_RE_GeraBonusSobreMensalidade]
   @baseDate  varchar(8) = null
      
AS  
-- =============================================================================================  
-- Author.....:  Adamastor  
-- Create date:  16/03/2018  
-- Description:  Gera registros de Bonificacao Sobre Mensalidade - Recorrencia (Ativo Mensal)  
-- =============================================================================================  
BEGIN  
   BEGIN TRY  
      Set NoCount On  
  
      Declare   
      @TotalNivel Int   = 10  , -- Total de Niveis para pagamento de Bonus  
      @Margem     Float = 0.60, -- Margem (60%) do pedido destinada para pagamento do Bonus  
      @dataInicio datetime    ,  
      @dataFim    datetime    ;  
        
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
  
      -- Cria Tabela de Fator de Bonificação por Nivel    
      Create Table #TFatorNivel  
      (        
         Nivel Int,   
         NivelClassificacao Int,  
         FatorBonus Float       
      );  
  
      Declare   
         @QtdeClass Int = 0,  
         @QtdeNivel Int = 1;  
  
      While @QtdeNivel <= @TotalNivel  
      Begin  
         Set @QtdeClass = 0;  
  
         While @QtdeClass < 8  
         Begin  
            Insert #TFatorNivel Values(@QtdeNivel, @QtdeClass, 0.05);  
            Set @QtdeClass = @QtdeClass + 1;  
         End;  
  
         Set @QtdeNivel = @QtdeNivel + 1;  
      End;  
  
      Update #TFatorNivel Set FatorBonus = 0.20 Where Nivel = 1  and NivelClassificacao in (0,1,2,3,4,5);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 1  and NivelClassificacao in (6,7);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 2  and NivelClassificacao in (0,5,6,7);  
      Update #TFatorNivel Set FatorBonus = 0.20 Where Nivel = 2  and NivelClassificacao in (1,2,3,4);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 3  and NivelClassificacao in (0,1,2,3,4,5,6);      
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 4  and NivelClassificacao in (0,2);  
      Update #TFatorNivel Set FatorBonus = 0.00 Where Nivel = 7  and NivelClassificacao in (0);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 7  and NivelClassificacao in (6,7);  
      Update #TFatorNivel Set FatorBonus = 0.00 Where Nivel = 8  and NivelClassificacao in (0,1);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 8  and NivelClassificacao in (3,4,5,6,7);  
      Update #TFatorNivel Set FatorBonus = 0.00 Where Nivel = 9  and NivelClassificacao in (0,1,2);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 9  and NivelClassificacao in (3,4);  
      Update #TFatorNivel Set FatorBonus = 0.15 Where Nivel = 9  and NivelClassificacao in (5,6);  
      Update #TFatorNivel Set FatorBonus = 0.20 Where Nivel = 9  and NivelClassificacao in (7);  
      Update #TFatorNivel Set FatorBonus = 0.00 Where Nivel = 10 and NivelClassificacao in (0,1,2,3);  
      Update #TFatorNivel Set FatorBonus = 0.10 Where Nivel = 10 and NivelClassificacao in (4);  
      Update #TFatorNivel Set FatorBonus = 0.15 Where Nivel = 10 and NivelClassificacao in (5);  
      Update #TFatorNivel Set FatorBonus = 0.20 Where Nivel = 10 and NivelClassificacao in (6,7);  
  
      -- Tabela temporária com usuários cadastrados com seus respectivos níveis e data de associação  
      Create Table #TUsuarios  
      (        
         PedidoId INT,   
         UsuarioPedID INT,       
         ProdutoID INT,  
         DtAtivacao DATETIME,   
         UsuarioID INT,  
         PatrocinadorDiretoID INT,    
         NivelClassificacao INT,  
         Valor float,     
         Bonus float,     
         DtValidade DATETIME,  
         Nivel  INT  
      );  
  
      -- Seleção dos pedidos pagos  
      Insert Into #TUsuarios   
      Select   
         P.ID as PedidoId,  
         U.id as UsuarioPedID,          
         PV.ProdutoID as ProdutoID,  
         PS.Data as DtAtivacao,     
         U.id as UsuarioID,  
         U.PatrocinadorDiretoID as PatrocinadorDiretoID,   
         U.NivelClassificacao as NivelClassificacao,  
         PV.Valor as Valor,   
         0 as Bonus,  
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
        and PR.TipoID IN (1)  -- Ativo Mensal    
        and PI.ProdutoID = PV.ProdutoID  
        and P.UsuarioID = U.ID   
        and P.ID = PP.PedidoID  
        and PP.ID = PS.PedidoPagamentoID  
        and PS.StatusID = 3 -- somente pedidos pagos          
        and U.GeraBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao  
        and U.Bloqueado = 0 -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao      
        and U.DataValidade >= @dataInicio -- so recebe se estiver com ativo mensal pago       
        and PS.Data BETWEEN @dataInicio AND @dataFim -- pega somente pedidos do dia   
		and Not Exists (Select 1 
		                From Rede.Bonificacao B (nolock)
						Where B.PedidoID = P.ID
						  and B.CategoriaID = 14);
				  
      -- Obtem os Usuarios superiores qualificados para a bonificação  
      Declare @cnt INT = 1;  
  
      While @cnt <= @TotalNivel  
      Begin        
         Insert Into #TUsuarios   
         Select   
            T.PedidoId as PedidoId,  
            T.UsuarioPedID as UsuarioPedID,          
            T.ProdutoID as ProdutoID,  
            T.DtAtivacao as DtAtivacao,   
            U.id as UsuarioID,  
            U.PatrocinadorDiretoID as PatrocinadorDiretoID,    
            U.NivelClassificacao as NivelClassificacao,   
            T.Valor as Valor,   
            T.Bonus as Bonus,   
            U.DataValidade as DtValidade,           
            @cnt as Nivel        
         From 
	        #TUsuarios T,  
            Usuario.Usuario U (nolock)   
         Where T.PatrocinadorDiretoID = U.ID  
           and U.DataValidade >= @dataFim -- so recebe se estiver com ativo mensal pago   
           and U.RecebeBonus = 1   -- Recebe bonus   
           and T.Nivel = @cnt - 1;  
      
         Set @cnt = @cnt + 1;  
      End;  
  
      -- Calcula a Bonificação   
      Update   
         #TUsuarios   
      Set   
         Bonus = dbo.TruncZion(U.Valor * @Margem * P.FatorBonus , 2)  
      From   
         #TUsuarios   U,  
         #TFatorNivel P  
      Where U.Nivel = P.Nivel  
        and U.NivelClassificacao = P.NivelClassificacao;  
            
      -- Remove os usuario com Bonificação zero ou com patrocinador nulo ou nivel igual a zero (usuario que gerou a compra)  
      Delete   
         #TUsuarios  
      Where Bonus = 0.0       
         or Nivel = 0;  
  
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
         14 as CategoriaID, -- 2 - Bônus Sobre Mensalidades  
         T.UsuarioID as Usuario,  
         T.UsuarioPedID as Referencia,  
         0 as StatusID,  
         T.DtAtivacao as Data,  
         T.Bonus as Valor,  
         T.PedidoId as PedidoID  
      From  
         #TUsuarios T; 
    
      COMMIT TRANSACTION  
  
      -- Select * from #TUsuarios  
  
      -- Remove todas as tabelas temporárias  
      Drop Table #TUsuarios;  
      Drop Table #TFatorNivel;    
   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION;  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de uspDG_RE_GeraBonusSobreMensalidade: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END   

-- Exec uspDG_RE_GeraBonusSobreMensalidade null
-- Exec uspDG_RE_GeraBonusSobreMensalidade '20180316'

-- Select * from Rede.Bonificacao