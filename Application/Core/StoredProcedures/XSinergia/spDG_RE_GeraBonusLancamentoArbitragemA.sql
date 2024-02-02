 
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusLancamentoArbitragemA'))
   Drop Procedure spDG_RE_GeraBonusLancamentoArbitragemA
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusLancamentoArbitragemA]  
   @baseDate varchar(8) = null  
  
AS  
  
-- =============================================================================================  
-- Author.....: Adamastor  
-- Create date: 14/01/2020  
-- Description: Gera registros de Bonificacao e Lancamento de Arbitragem  
-- =============================================================================================  
  
BEGIN  
   BEGIN TRY  
      Set nocount on  
  
      Declare   
         @data datetime;            
        
      If (@baseDate is null)  
      Begin   
         Set @data = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);  
      End  
      Else  
      Begin  
         Set @data = CAST(@baseDate + ' 23:59:59' as datetime2);  
      End  
  
      -- =========================================================================================================
      -- Obtem os valores de Arbitragem por Associação  
      Declare    
         @dia int = DATEPART(DW, @data);  
  
      Create Table #TArbitragem  (        
         NivelAssociacao int,   
         Percentual      float           
      );  
  
      Insert into #TArbitragem  
      Select   
         A.NivelAssociacao,  
         Case    
            When @dia = 1 Then A.PercentualDomingo  
            When @dia = 2 Then A.PercentualSegunda  
            When @dia = 3 Then A.PercentualTerca  
            When @dia = 4 Then A.PercentualQuarta  
            When @dia = 5 Then A.PercentualQuinta  
            When @dia = 6 Then A.PercentualSexta           
            Else               A.PercentualSabado  
         End as Percentual  
      From 
	     Financeiro.ArbitragemPeriodo (nolock) P   
         left join Financeiro.Arbitragem (nolock) A on A.ArbitragemPeriodoID = P.ID  
      Where @data Between P.dataInicio and P.dataFim;  
	 
      -- =========================================================================================================
      -- Exclui as Bonificação, Lançamento e Ganho em caso de reprocessamento 
	 -- Update
	 --    Usuario.UsuarioGanho
	 -- Set
	 --    AcumuladoGanho = AcumuladoGanho - L.ValorCripto
	 -- From
	 --    Financeiro.Lancamento L (nolock),
		-- Usuario.UsuarioGanho  G 
	 -- Where L.UsuarioID = G.UsuarioID
	 --   and L.CategoriaID = 21
		--and DataLancamento = @data 
	 --   and @data between G.DataInicio and G.DataFim; 
	   
  --    Delete Rede.Bonificacao  
  --    Where CategoriaID = 21  
  --      and Data = @data;  
  
  --    Delete Financeiro.Lancamento  
  --    Where CategoriaID = 21  
  --      and DataLancamento = @data;  
  
      -- =========================================================================================================
      -- Usuarios Migrados sem pedidos  
	  Create Table #TUsuario  (        
         ID              int     ,   
         NivelAssociacao int     ,   
         DataCriacao     datetime,  
		 DataIni         datetime,
		 DataFim         datetime,
         PedidoID        int     ,   
         ValorMig        float   , 
		 ValorPac        float   ,
		 AcumuladoGanho  float   ,  
		 ValorBonus      float   , 
		 BonusReduzido   varchar(10)         
      );  

      Insert Into #TUsuario(  
         ID             ,   
         NivelAssociacao,    
         DataCriacao    , 
		 DataIni        ,
		 DataFim        , 
         PedidoID       ,  
         ValorMig       ,
		 ValorPac       ,
		 AcumuladoGanho ,
		 ValorBonus     ,
		 BonusReduzido  )       
      Select   
         U.ID ,  
         U.NivelAssociacao,  
         @data,  
		 null as DataIni, 
         null as DataFim,  
         null as PedidoID,  
         IsNull(C.MaximoGanhos, 0) / 2 as ValorMig,
		 0 as ValorPac,
		 0 as AcumuladoGanho,
		 0 as ValorBonus,
		 '0' as BonusReduzido
      From      
         Usuario.Usuario     U (nolock),    
         Usuario.Complemento C (nolock)       
      Where U.ID = C.ID    
        and C.IsLideranca = 0 
		and U.RecebeBonus = 1;

      -- =========================================================================================================
	  -- Obtem o periodo de ganho e o ganho acumulado dos Usuario
	  Update
	     #TUsuario
	  Set
	     DataIni        = G.DataInicio,
		 DataFim        = G.DataFim,
		 AcumuladoGanho = Case    
                             When G.Indicador = 0   
                             Then IsNull(G.AcumuladoGanho, 0)  
                             Else 0  
                          End     
	  From
	     Usuario.UsuarioGanho G (nolock),
		 #TUsuario            U
	  Where G.UsuarioID = U.ID
	    and @data between G.DataInicio and G.DataFim
		and DataAtingiuLimite is not null;

      -- =========================================================================================================
	  -- Elimina os Usuarios que já estao com o limite atingido e ja foram marcados
	  --Delete 
	  --  #TUsuario 	     
	  --From   
	  --   Usuario.UsuarioGanho G, 
		 --#TUsuario U
	  --Where G.UsuarioID = U.ID
	  --  and @data between G.DataInicio and G.DataFim
	    
      -- Elimina os usuarios que não tem UsuarioGanho no periodo da data de processamento 
      Delete 
	     #TUsuario
	  Where DataIni is null;

	  -- =========================================================================================================
	  -- Seleção dos pacotes complementares pagos  
	  Create Table #TPacotes (        
         ID              int,      
         Valor           float              
      );  

	  Insert Into #TPacotes(  
         ID   ,    
         Valor)       
      Select   
         L.UsuarioID,   
         Sum(IsNull(L.ValorCripto,0)) as Valor  
      From     
	     Financeiro.Lancamento L (nolock),  
		 #TUsuario             U             
      Where L.TipoID = 4
		and L.DataLancamento between U.DataIni and @data  
		and U.ID = L.UsuarioID   
	  Group by 
	     L.UsuarioID;
		 
      -- Atualiza o valor dos pacotes na Table #TUsuario
      Update #TUsuario
	  Set
	     ValorPac = P.Valor
	  From
	     #TUsuario U,
		 #TPacotes P
	  Where U.ID = P.ID;

	  -- =========================================================================================================
      -- Calcula Bonificações  
	  Update 
	     #TUsuario
	  Set
         ValorBonus = Case    
                         When dbo.TruncZion(( (U.ValorMig + U.ValorPac ) * A.Percentual / 100 ), 4) > 0   
                         Then dbo.TruncZion(( (U.ValorMig + U.ValorPac ) * A.Percentual / 100 ), 4)  
                         Else         ROUND(( (U.ValorMig + U.ValorPac ) * A.Percentual / 100 ), 4)  
                      End     
      From   
         #TUsuario    U  
         Inner join  #TArbitragem A ON U.NivelAssociacao = A.NivelAssociacao   
      Where ROUND(( (U.ValorMig + U.ValorPac) * A.Percentual / 100 ), 4) > 0;

	  -- Verifica se bonificação ultrapassa o limite e paga o bonus so ate o limite
	  Declare
         @Fator int = IsNull((Select convert(int, Dados) From Sistema.Configuracao (nolock) Where Chave = 'FATOR_MULTIPLICADOR_TETO') , 2); 

	  Update 
	     #TUsuario
	  Set 
	     BonusReduzido = 1,
	     ValorBonus    = Case    
                            When dbo.TruncZion(( ((U.ValorMig + U.ValorPac) * @Fator) - U.AcumuladoGanho ), 4) > 0   
                            Then dbo.TruncZion(( ((U.ValorMig + U.ValorPac) * @Fator) - U.AcumuladoGanho ), 4)  
                            Else         ROUND(( ((U.ValorMig + U.ValorPac) * @Fator) - U.AcumuladoGanho ), 4)
                         End     		 
	  From 
	     #TUsuario U
	  Where U.AcumuladoGanho + U.ValorBonus > (U.ValorMig + U.ValorPac) * @Fator;

	   -- Apaga usuarios com bonus zerados
	  Delete
	      #TUsuario
	  Where ValorBonus = 0;

      -- =========================================================================================================
      -- Gera Bonificações com "em processamento" - StatusID = 1  
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
         21 as CategoriaID, -- Bonus Arbitragem  
         U.ID as UsuarioID,  
         0  as ReferenciaID,  
         1 as  StatusID,  
         @data as Data,  
         0 as Valor,  
         U.PedidoID as PedidoID,  
         null as CicloID,  
         null as RegraItemID,   
         ' | N:' + CONVERT(varchar(10), U.NivelAssociacao) +  
         ' | M:' + CONVERT(varchar(10), U.ValorMig, 127)   +  
		 ' | P:' + CONVERT(varchar(10), U.ValorPac, 127)   +    
         ' | %:' + CONVERT(varchar(10), A.Percentual, 128) +
		 ' | R:' + U.BonusReduzido as Descricao,  
         U.ValorBonus as ValorCripto       
      From   
         #TUsuario    U  
         Inner join  #TArbitragem A ON U.NivelAssociacao = A.NivelAssociacao
	  Where U.ValorBonus > 0;

	  -- =========================================================================================================
      -- Gera Lancamentos  
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
         2 as ContaID,   
         6 as TipoID,  
         B.CategoriaID,  
         B.ReferenciaID,  
         B.Valor,  
         B.ValorCripto,  
         C.Nome as Descricao,  
         dbo.GetDateZion() as DataCriacao,  
         B.Data as DataLancamento,  
         B.PedidoID,  
         null as CicloID,  
         null as RegraItemID  
      From   
         Rede.Bonificacao B (nolock)  
         Inner join Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID  
      Where B.StatusID = 1   
        and B.CategoriaID IN (21)  
        and B.Data = @data;  

	  -- =========================================================================================================
      -- Atualiza o acumulado de ganho do usuario
      Update 
	     Usuario.UsuarioGanho
	  Set
          AcumuladoGanho = IsNull(G.AcumuladoGanho, 0) + IsNull(U.ValorBonus, 0)  
	  From   
	     Usuario.UsuarioGanho G, 
		 #TUsuario U
	  Where G.UsuarioID = U.ID
	    and U.ValorBonus > 0;

	  -- Marca os que atingiram o limite 
      Update 
	     Usuario.UsuarioGanho
	  Set
         DataAtingiuLimite = @Data   
	  From   
	     Usuario.UsuarioGanho G, 
		 #TUsuario U
	  Where G.UsuarioID = U.ID
	    and G.DataAtingiuLimite is null
	    and U.ValorBonus < 0;

	  -- Marca os que atingiram o limite 
      Update 
	     Usuario.UsuarioGanho
	  Set
         DataAtingiuLimite = @Data   
	  From   
	     Usuario.UsuarioGanho G, 
		 #TUsuario U
	  Where G.UsuarioID = U.ID
	    and G.DataAtingiuLimite is null
	    and dbo.TruncZion(G.AcumuladoGanho, 4) >= dbo.TruncZion((U.ValorMig + U.ValorPac) * @Fator, 4);

      -- Suspende a geração de todos os tipos de bonus
      Update 
	     Usuario.Usuario
	  Set
         RecebeBonus = 0   
	  From   
	     Usuario.UsuarioGanho G, 
		 Usuario.Usuario U
	  Where G.UsuarioID = U.ID
	    and G.DataAtingiuLimite is not null;

      -- =========================================================================================================
      -- Marca os registros "em processamento" como "processados"  
      Update Rede.Bonificacao   
      Set   
         StatusID = 2   
      Where StatusID = 1   
        and CategoriaID IN (21)  
        and Data = @data;  
  
      -- Remove todas as tabelas temporárias  
      Drop Table #TArbitragem;  
      Drop Table #TUsuario;  
	  Drop Table #TPacotes;  
  
   END TRY  
  
   BEGIN CATCH  
      If @@Trancount > 0  
         ROLLBACK TRANSACTION;  
        
      DECLARE @error int, @message varchar(4000), @xstate int;  
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();  
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusLancamentoArbitragemA: %d: %s', 16, 1, @error, @message) WITH SETERROR;  
   END CATCH  
END  

go
   Grant Exec on spDG_RE_GeraBonusLancamentoArbitragemA To public
go 