Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusIndicacao'))
   Drop Procedure spDG_RE_GeraBonusIndicacao
go

-- =============================================================================================
-- Author.....:
-- Create date: 
-- Description: Gera registros de Bonificacao de indicação e bonus amizade
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
   @baseDate varchar(8) null

AS
BEGIN
   BEGIN TRY
      set nocount on

      DECLARE @dataInicio datetime;
      DECLARE @dataFim    datetime;
      
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
	      UsuarioID INT,
	      PedidoId INT,
	      ProdutoID INT,
	      PatrocinadorID INT,
	      DtAtivacao DATETIME,
         VlrBonusVenda float,
         VlrBonusAdicionalVenda float,
	      DtValidade DATETIME,
         QtdeDiretos INT
	   );

      -- Seleção dos pedidos pagos com produtos que paguem bonus de venda
      Insert Into #TUsuarios 
	   Select 
         U.id as usuario,
         P.ID pedido,
         PV.ProdutoID as produto,
         U.PatrocinadorDiretoID,
         PS.Data as DtAtivacao,
         PV.VlrBonusVenda,
         PV.VlrBonusAdicionalVenda,
         U2.DataValidade,
         0
      From 
	      Loja.pedido P (nolock), 
	      Loja.PedidoItem PI (nolock),
         Loja.Produto PR (nolock),
	      Loja.ProdutoValor PV (nolock),
	      Usuario.Usuario U (nolock),
	      Usuario.Usuario U2 (nolock),
	      Loja.PedidoPagamento PP (nolock), 
	      Loja.PedidoPagamentoStatus PS (nolock) 		
      Where P.ID = PI.PedidoID
        and PI.ProdutoID = PR.ID
        and PR.TipoID in (1)  -- Associacao 
	     and PI.ProdutoID = PV.ProdutoID
	     and P.UsuarioID = U.ID
	     and U.PatrocinadorDiretoID = U2.ID
	     and P.ID = PP.PedidoID
	     and PP.ID = PS.PedidoPagamentoID
	     and PS.StatusID = 3      -- somente pedidos pagos        
	     and PV.VlrBonusVenda > 0 -- somente produtos com valor para bonus de venda (indicação)
	     and U.GeraBonus = 1      -- Somente ususarios que geram bonus 1= sim, 0 = nao
	     and U.Bloqueado = 0      -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
	     and U2.RecebeBonus = 1   -- Patrocinador recebe bonus
	     and U2.Bloqueado = 0     -- Patrocinador nao esta bloqueado
	     and U2.DataValidade >= @dataInicio --so recebe se estiver com ativo mensal pago	   	 
        and PS.Data BETWEEN @dataInicio and @dataFim --pega somente pedidos do dia
	     and NOT EXISTS (Select 1 From Rede.Bonificacao B (nolock) Where B.PedidoID = P.ID and B.CategoriaID = 13); -- evita gerar duplicidade de pedidos na rede.bonificação.

      -- Insere na tabela de bonificações todos os bônus que ainda não foram pagos ou a diferença do que resta ser pago
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
		   13 as CategoriaID, -- Bonus Indicacao
		   T.PatrocinadorID as Usuario,
		   T.UsuarioID as Referencia,
		   0 as StatusID,
		   T.DtAtivacao as Data,
		   T.VlrBonusVenda as Valor,
		   T.PedidoId as PedidoID
	   From
		   #TUsuarios AS T
	   Where
		   NOT EXISTS (Select 1 From Rede.Bonificacao B (nolock) Where T.PedidoId = B.PedidoID);


      --------------------------------------------------------------------------------------
      -- INICIO - BONIFICAÇÃO DE ATIVO MENSAL ADICIONAL - AMIZADE
      --------------------------------------------------------------------------------------
  
      -- Obtem a quantidade minima de Diretos para a bonificação adicional - AMIZADE (Qtde zero ou null a bonificação esta desetivada)    
      Declare @QtdeMinDiretos INT = ISNULL((Select convert(INT, Dados) From Sistema.Configuracao (nolock) Where Chave = 'REDE_QTDE_DIRETOS_BONUS_ADICIONAL') , 0);

      if (@QtdeMinDiretos > 0)
      Begin

         -- Tabela temporária com usuários Qualificados para o pagamento do bonus
         Create Table #TUsuarioDiretos
         (	     
            UsuarioID INT,
            QtdeDiretos INT
         );

         -- Obtem o total de diretos ativos do patrocinador 
         Insert Into #TUsuarioDiretos
         Select 
            T.PatrocinadorID as UsuarioID,
            count(U.ID) as QtdeDiretos
         from #TUsuarios T,
              Usuario.Usuario U (nolock)
         Where T.PatrocinadorID = U.PatrocinadorDiretoID      
           and U.DataValidade >= @dataInicio
           and U.GeraBonus = 1
           and U.DataAtivacao < @dataInicio
         Group by T.PatrocinadorID;
   
         -- Atualiza a quantidade de diretos ativos
         Update #TUsuarios
         set QtdeDiretos = TD.QtdeDiretos
         from #TUsuarios T,
              #TUsuarioDiretos TD
         Where T.PatrocinadorID = TD.UsuarioID;          

         -- Insere na tabela de bonificações
         Declare
            @UsuarioID              INT,
	         @PedidoId               INT, 
	         @PatrocinadorID         INT, 
	         @DtAtivacao             DATETIME, 
            @VlrBonusAdicionalVenda DECIMAL(10,2),
	         @DtValidade             DATETIME, 
            @QtdeDiretos            INT;

         Declare
            @AntFetch     INT,
            @PatrociAuxID INT = 0,
            @CountDiretos INT = 0;

         Declare
            curBonus
         Cursor For
         Select
            T.UsuarioID,
	         T.PedidoId, 
	         T.PatrocinadorID, 
	         T.DtAtivacao, 
            T.VlrBonusAdicionalVenda, 
	         T.DtValidade, 
            T.QtdeDiretos 
         From 
            #TUsuarios T
         Order by 
            T.PatrocinadorID, T.DtAtivacao;
      
         Open curBonus
         if (Select @@CURSOR_ROWS) <> 0
         Begin
            Fetch Next From curBonus Into  @UsuarioID, @PedidoId, @PatrocinadorID, @DtAtivacao, @VlrBonusAdicionalVenda, @DtValidade, @QtdeDiretos
            Select @AntFetch = @@fetch_status

            IF (@AntFetch = 0)
            Begin
               Select @PatrociAuxID = @PatrocinadorID,
                      @CountDiretos = @QtdeDiretos;

               While @AntFetch = 0
               Begin
                  if (@PatrociAuxID != @PatrocinadorID)
                  Begin
                     Update Usuario.Usuario Set QtdeDiretos = @CountDiretos Where ID = @PatrociAuxID;
				 
                     Select @PatrociAuxID = @PatrocinadorID,
                            @CountDiretos = @QtdeDiretos;
                  End

                  Select @CountDiretos = @CountDiretos + 1;

                  if (@CountDiretos >= @QtdeMinDiretos)
                  Begin
                     Insert Into Rede.Bonificacao 
                        (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor, PedidoID)		
                     Values 
                        (14, @PatrocinadorID, @UsuarioID, 0,  @DtAtivacao, @VlrBonusAdicionalVenda, @PedidoId);
                  End

                  Fetch Next From curBonus Into  @UsuarioID, @PedidoId, @PatrocinadorID, @DtAtivacao, @VlrBonusAdicionalVenda, @DtValidade, @QtdeDiretos;                  
				      Select @AntFetch = @@fetch_status; -- Para ver se nao é fim do loop  			   
               End -- While
         
		         Update Usuario.Usuario Set QtdeDiretos = @CountDiretos Where ID = @PatrociAuxID;
			   End		 
		   End -- @@CURSOR_ROWS
      End  -- FIM - BONIFICAÇÃO DE ATIVO MENSAL ADICIONAL - AMIZADE

      COMMIT TRANSACTION

      Close curBonus
      Deallocate curBonus

      -- Remove todas as tabelas temporárias
      Drop Table #TUsuarios;

   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on spDG_RE_GeraBonusIndicacao To public
go

-- Exec spDG_RE_GeraBonusIndicacao null
-- Exec spDG_RE_GeraBonusIndicacao '20170621'