Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusAlavancagem'))
   Drop Procedure spDG_RE_GeraBonusAlavancagem;
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusAlavancagem]  
   @baseDate  varchar(8) = null  
    
AS  
-- =============================================================================================  
-- Author.....:  Adamastor  
-- Create date:  16/03/2018  
-- Description:  Gera registros de Bonificacao de Alavancagem   
-- =============================================================================================  
BEGIN  
   BEGIN TRY  
      set nocount on  
  
      Declare 
	     @QtdeNiveis int = 5 ,   
         @dataInicio datetime,  
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
         Valor float,     
         DtValidade DATETIME,  
         Nivel  INT,  
         Excluir INT,  
         Contador INT  
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
         PV.Valor as Valor,   
         U.DataValidade as DtValidade,  
         1 as Nivel,  
         Case  
            When RIGHT(U.Assinatura, 1) = '0' Then 1           Else 0  
         End as Excluir,        
         0 as Contador  
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
        and PR.TipoID IN (1)  -- Associação    
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
						  and B.CategoriaID = 13);  
  
      -- Obtem os Usuarios superiores qualificados para a bonificação  
      Declare @cnt INT = 0;  
  
      While @@ROWCOUNT > 0  
      Begin  
         Set @cnt = @cnt + 1;  
  
         Insert Into #TUsuarios   
         Select   
            T.PedidoId as PedidoId,  
            T.UsuarioPedID as UsuarioPedID,          
            T.ProdutoID as ProdutoID,  
            T.DtAtivacao as DtAtivacao,   
            U.id as UsuarioID,  
            U.PatrocinadorPosicaoID as PatrocinadorPosicaoID,     
            T.Valor as Valor,   
            U.DataValidade as DtValidade,           
            Case  
               When RIGHT(U.Assinatura, 1) = '0' Then T.Nivel     Else T.Nivel + 1  
            End as Nivel,   
            Case  
               When RIGHT(U.Assinatura, 1) = '0' Then 1           Else 0  
            End as Excluir,    
            Case  
               When U.Assinatura = '#0'          Then @QtdeNiveis Else @cnt  
            End as Contador  
         From #TUsuarios T,  
              Usuario.Usuario U (nolock)   
         Where T.PatrocinadorPosicaoID = U.ID  
           and U.DataValidade >= @dataFim -- so recebe se estiver com ativo mensal pago   
           and U.RecebeBonus = 1   -- Recebe bonus   
           and T.Nivel < @QtdeNiveis  
           and T.Contador = @cnt - 1             
      End;  
  
      -- Remove os patrocinadores em que a bonificação foi gerada em sua primeira perna (zero)  
      Delete #TUsuarios  
      Where Excluir = 1  
         or PatrocinadorPosicaoID is null;  
  
      -- Remove os patrocinadores que não tem pelo memos um usuario ativo em sua primeira perna (zero)  
      -- Gera temporaria agrupando os patrocinadores e marca todos para exclusão  
      Select   
         PatrocinadorPosicaoID ,   
         1 as Excluir  
      Into   
         #TPatrocinador  
      From   
         #TUsuarios  
      Group by   
         PatrocinadorPosicaoID;  
   
      -- Remove a marca de exclusão dos patrocinadores com pelo memos um usuario ativo em sua primeira perna (zero)  
      Update #TPatrocinador   
      Set   
         Excluir = 0  
      From   
         #TPatrocinador T,  
         Usuario.Usuario P (nolock)  
      Where T.PatrocinadorPosicaoID = P.ID  
        and P.RecebeBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao       
        and P.DataValidade >= @dataInicio -- So recebe se estiver com ativo mensal pago   
        and EXISTS (Select 'S'   
                    From Usuario.Usuario U (nolock)   
                    Where P.Assinatura + REPLICATE('0', 7999 - Len(P.Assinatura)) like U.Assinatura + '%'  
                      and P.Assinatura < U.Assinatura  
                      and DataValidade >= @dataInicio); -- So recebe se pelo memos um usuario estiver ativo em sua primeira perna (zero)  
  
      -- Remove os patrocinadores  
      Delete #TPatrocinador  
      Where Excluir = 1       
  
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
         13 as CategoriaID, -- 2 - Bônus de Alavancagem  
         T.PatrocinadorPosicaoID as Usuario,  
         T.UsuarioPedID as Referencia,  
         0 as StatusID,  
         T.DtAtivacao as Data,  
         dbo.TruncZion((T.Valor * 0.12), 2) as Valor,  
         T.PedidoId as PedidoID  
      From  
         #TUsuarios T,  
         #TPatrocinador P  
      Where   
         t.PatrocinadorPosicaoID = P.PatrocinadorPosicaoID   
    
      COMMIT TRANSACTION  
  
      -- Select * from #TUsuarios  
  
      -- Remove todas as tabelas temporárias  
      Drop Table #TUsuarios;  
   Drop Table #TPatrocinador;  
  
   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusAlavancagem: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END   

-- Exec spDG_RE_GeraBonusAlavancagem null
-- Exec spDG_RE_GeraBonusAlavancagem '20180316'

-- Select * from Rede.Bonificacao