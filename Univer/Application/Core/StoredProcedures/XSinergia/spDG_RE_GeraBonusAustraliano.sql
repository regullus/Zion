 
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusAustraliano'))
   Drop Procedure spDG_RE_GeraBonusAustraliano
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusAustraliano]  
   @baseDate varchar(8) = null  
  
AS  
  
-- =============================================================================================  
-- Author.....: Adamastor  
-- Create date: 03/02/2020  
-- Description: Gera registros de Bonificacao e Lancamento de Rede Australiana
-- =============================================================================================  
  
BEGIN  
   BEGIN TRY  
      Set NoCount On;  
  
      Declare   
         @data          datetime = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-2, 112) + ' 00:00:00' as datetime2),  	
		 @CategoriaID	int      = 23,                       -- Bônus Rede Australiana
		 @DataInicio    datetime = '2020-02-26 00:00:00.000',-- Data de mudanção da Matriz de 2 para 3 nos. 
		 @QtdeNiveis    int      = 5,                        -- Niveis de pagamento
		 @TaxaNivel01   float    = 5.00,                     -- Percentuais a sem pagos      
         @TaxaNivel02   float    = 5.00,
		 @TaxaNivel03   float    = 4.00,
		 @TaxaNivel04   float    = 3.00,
		 @TaxaNivel05   float    = 3.00;

      -- Forçar processamento para um data especifica
      If (@baseDate is not null)  
      Begin   
         Set @data = CAST(@baseDate + ' 00:00:00' as datetime2); 
      End  
      
      -- =========================================================================================================
      -- Obtem os pedidos e usuarios 
	  Create Table #TUsuarios
	  (	     
         PedidoID              int,	
         UsuarioPedID          int,	 	  
         DtPgto                datetime, 
         UsuarioID             int,
         PatrocinadorPosicaoID int,  		     
         Nivel                 int,
		 Valor                 float,
		 Taxa                  float
	  );

	  Insert Into #TUsuarios 
      Select 
	     L.PedidoID              as PedidoID,
         L.UsuarioID             as UsuarioPedID,        
		 L.DataLancamento        as DtPgto,   
		 L.UsuarioID             as UsuarioID,
         U.PatrocinadorPosicaoID as PatrocinadorPosicaoID, 		  
         0                       as Nivel,
		 L.Valor                 as Valor,
		 0                       as Taxa
      From     
	     Financeiro.Lancamento L  (nolock), 		
	     Loja.PedidoItem       PI (nolock), 		
         Loja.Produto          PR (nolock),         
		 Usuario.Usuario       U  (nolock)            
      Where L.TipoID = 4
		and L.DataLancamento >= @data       -- Somente pedidos pagos nos ultimos x dias	 
		and L.DataLancamento > @dataInicio  -- Data de mudanção da Matriz de 2 para 3 nos.
		and PI.PedidoID  = L.PedidoID
		and PR.ID = PI.ProdutoID 
		and PR.TipoID IN (1,2,4,6)          -- Somente pedidos de Associação, Upgrade, Pacote Complementar e Renovacao   
		and U.ID = L.UsuarioID   
		and U.GeraBonus = 1                 -- Somente usuarios que geram bonus 1= sim, 0 = nao
	    and U.Bloqueado = 0;                -- Somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
	 	
	  -- =========================================================================================================
      -- Exclui os pedidos com bonificação paga
	  Delete 
	     #TUsuarios
	  From
	     Financeiro.Lancamento L (nolock),
	     #TUsuarios T
	  Where L.PedidoID = T.PedidoID 
	    and L.CategoriaID = @CategoriaID;

	  -- Exclui os pedidos de usuarios de liderança -- Eles não pagam
	  Delete 
	     #TUsuarios
	  From
	     Usuario.Complemento C (nolock),
	     #TUsuarios T
	  Where C.ID = T.UsuarioPedID 
	    and C.isLideranca = 1;
	    
	  -- =========================================================================================================
	  -- Obtem os Usuarios superiores qualificados para a bonificação
      Declare @cnt INT = 1;

      While @cnt <= @QtdeNiveis
      Begin
         Insert Into #TUsuarios 
         Select 
            T.PedidoId              as PedidoId,
            T.UsuarioPedID          as UsuarioPedID,        
			T.DtPgto                as DtPgto, 
            U.id                    as UsuarioID,
            U.PatrocinadorPosicaoID as PatrocinadorPosicaoID,  			
            @cnt                    as Nivel,
			T.Valor                 as Valor,
			T.Taxa                  as Taxa 
         From 
		    #TUsuarios      T,
            Usuario.Usuario U (nolock) 
         Where T.PatrocinadorPosicaoID = U.ID	
           and U.RecebeBonus = 1   -- Somente ususarios que recebe bonus
		   and U.NivelAssociacao > 0
           and T.Nivel = @cnt - 1;

         Set @cnt = @cnt + 1;
      End;

	  -- Remove os usuarios que geraram os pedidos ou não tem patrocinador
	  Delete 
	     #TUsuarios
	  Where Nivel = 0
	     or PatrocinadorPosicaoID is null;

      -- =========================================================================================================
      -- Atualiza Taxa	  
	  Update 
	     #TUsuarios
	  Set Taxa = Case    
                    When Nivel = 1 Then @TaxaNivel01
		            When Nivel = 2 Then @TaxaNivel02 
		            When Nivel = 3 Then @TaxaNivel03
		            When Nivel = 4 Then @TaxaNivel04
		            When Nivel = 5 Then @TaxaNivel05
                                   Else 0.00 
                 End;

--Select * from #TUsuarios order by  PedidoID, Nivel;

      -- =========================================================================================================
      -- Gera Bonificações com "em processamento" - StatusID = 1 
	  BEGIN TRANSACTION;
	   
	  Insert Into Rede.Bonificacao(  
         CategoriaID,  
         UsuarioID,  
         ReferenciaID,  
         StatusID,  
         Data,  
         Valor,  
         PedidoID,  
         CicloID,  
         RegraItemID,  
         Descricao,  
         ValorCripto)  
      Select  
	     @CategoriaID            as CategoriaID, 
         T.PatrocinadorPosicaoID as Usuario,
         T.UsuarioPedID          as Referencia,
         1                       as StatusID,
         T.DtPgto                as Data,
		 Case    
            When dbo.TruncZion( (T.Valor * T.Taxa / 100 ), 4) > 0   
            Then dbo.TruncZion( (T.Valor * T.Taxa / 100 ), 4)  
            Else         ROUND( (T.Valor * T.Taxa / 100 ), 4)  
         End  as Valor,
         T.PedidoID              as PedidoID,
		 null                    as CicloID,  
         null                    as RegraItemID,   
            '[NP]: ' + CONVERT(varchar(10), T.Nivel)         +  
         ' | [VE]: ' + CONVERT(varchar(10), T.Valor, 127) +    
         ' | [TX]: ' + CONVERT(varchar(10), T.Taxa, 128) as Descricao,  
		 0  as ValorCripto

		 -- Select  
		 --T.UsuarioPedID          as UsuarioPedidoID,
		 --U2.Login          as UsuarioPedido, 
   --      T.PatrocinadorPosicaoID as UsuarioID,
		 --U.Login                 as Usuario,

   --      T.DtPgto                as Data,
		 --Case    
   --         When dbo.TruncZion( (T.Valor * T.Taxa / 100 ), 4) > 0   
   --         Then dbo.TruncZion( (T.Valor * T.Taxa / 100 ), 4)  
   --         Else         ROUND( (T.Valor * T.Taxa / 100 ), 4)  
   --      End  as Valor,
   --      T.PedidoID              as PedidoID,
	  --          'Nivel: ' + CONVERT(varchar(10), T.Nivel)         +  
   --      ' | Valor Ped.: ' + CONVERT(varchar(10), T.Valor, 127) +    
   --      ' | %: ' + CONVERT(varchar(10), T.Taxa, 128) as Descricao 

      From   
         #TUsuarios    T  , Usuario.Usuario U (nolock),  Usuario.Usuario U2 (nolock)
	  Where ROUND((T.Valor * T.Taxa / 100 ) , 4) > 0;
	  --and T.UsuarioID = U.ID
	  --  and T.UsuarioPedID = U2.ID
	  --order by  T.PedidoID , Nivel

	  -- =========================================================================================================
      -- Gera Lancamentos  

	  Declare
	     @Nome nvarchar(255) = IsNull((Select Nome From Financeiro.Categoria (nolock) Where ID = @CategoriaID), ('Bonus ' + @CategoriaID) );

      Insert Into Financeiro.Lancamento (  
         UsuarioID,  
         ContaID,  
         TipoID,  
         CategoriaID,  
         ReferenciaID,  
         Valor,  
         ValorCripto,  
         Descricao,  
         DataCriacao,  
         DataLancamento,  
         PedidoID,  
         CicloID,  
         RegraItemID)  
      Select  
         B.UsuarioID,    
         1 as ContaID,   
         6 as TipoID,  
         B.CategoriaID,  
         B.ReferenciaID,  
         B.Valor,  
         B.ValorCripto,  
         @Nome as Descricao,  
         dbo.GetDateZion() as DataCriacao,  
         B.Data as DataLancamento,  
         B.PedidoID,  
         B.CicloID as CicloID,  
         B.RegraItemID as RegraItemID  
      From   
         Rede.Bonificacao B (nolock)  
      Where B.StatusID = 1   
        and B.CategoriaID = @CategoriaID;  

      -- =========================================================================================================
      -- Marca os registros "em processamento" como "processados"  
      Update Rede.Bonificacao   
      Set   
         StatusID = 2   
      Where StatusID = 1   
        and CategoriaID = @CategoriaID;  

	  COMMIT TRANSACTION;

      -- Remove todas as tabelas temporárias  
      Drop Table #TUsuarios;   
  
   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION;  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusAustraliano: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END  

go
   Grant Exec on spDG_RE_GeraBonusAustraliano To public
go 

--Exec spDG_RE_GeraBonusAustraliano '20200226';

--Select * from    Financeiro.Lancamento where CategoriaID = 23
--Select * from  DELETE Rede.Bonificacao where CategoriaID = 23
--Select * from Usuario.Usuario where ID in (74527,67779,68622,82274,82557,71295,82599,71430,25960,82148)