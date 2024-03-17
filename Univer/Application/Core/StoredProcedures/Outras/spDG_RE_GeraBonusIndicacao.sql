Use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusIndicacao'))
   Drop Procedure spDG_RE_GeraBonusIndicacao
go

Create PROCEDURE [dbo].[spDG_RE_GeraBonusIndicacao]
AS
BEGIN TRY
   BEGIN TRANSACTION
   -- Tabela temporária com usuários cadastrados com seus respectivos níveis e data de associação
   Create Table #TUsuarios
	(
	   UsuarioID INT,
	   PedidoId INT,
	   ProdutoID INT,
	   PatrocinadorID INT,
	   DtAtivacao DATETIME,
      VlrBonusVenda decimal(10,2),
      VlrBonusAdicionalVenda decimal(10,2),
	   DtValidade DATETIME,
      QtdeDiretos INT
	 )

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
      U2.QtdeDiretos
   From 
	   Loja.pedido P, 
	   Loja.PedidoItem PI,
	   Loja.ProdutoValor PV ,
	   Usuario.Usuario U,
	   Usuario.Usuario U2,
	   Loja.PedidoPagamento PP, 
	   Loja.PedidoPagamentoStatus PS  		
   Where 
          P.ID = PI.PedidoID
	   and PI.ProdutoID = PV.ProdutoID
	   and P.UsuarioID = U.ID
	   and U.PatrocinadorDiretoID = U2.ID
	   and P.ID = PP.PedidoID
	   and PP.ID = PS.PedidoPagamentoID
	   and PS.StatusID = 3 -- somente pedidos pagos
	   and PV.Bonificacao > 0 -- somente produtos com valor para vonus de venda (indicação)
	   and U.GeraBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao
	   and U.Bloqueado = 0 -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
	   and U2.RecebeBonus = 1 -- Patrocinador recebe bonus
	   and U2.Bloqueado = 0 -- Patrocinador nao esta bloqueado
	   and U2.DataValidade >= GETDATE() --so recebe se estiver com ativo mensal pago
	   and PS.Data >= '2016-10-13 17:23:46.597' -- data de inicio de processamento da rotina para nao intereferir nos bonus pagos anteriormente
	   and PS.Data >= CONVERT(VARCHAR(8),GETDATE(),112) --pega somente pedidos do dia
	   and NOT EXISTS (Select 1 From rede.Bonificacao RB Where RB.PedidoID = P.ID and RB.CategoriaID = 1) -- evita gerar duplicidade de pedidos na rede.bonificação.


   -- Insere na tabela de bonificações todos os bônus que ainda não foram pagos ou a diferença do que resta ser pago
   Insert Into Rede.Bonificacao
	Select 
		1 as CategoriaID, --1 = bonus builder
		T.PatrocinadorID as Usuario,
		T.UsuarioID as Referencia,
		0 as StatusID,
		T.DtAtivacao as Data,
		T.VlrBonusVenda as Valor,
		T.PedidoId as PedidoID
	 From
		#TUsuarios AS T
	 Where
		NOT EXISTS (Select 1 From Rede.Bonificacao R Where T.PedidoId = R.PedidoID)


   --------------------------------------------------------------------------------------
   -- INICIO - BONIFICAÇÃO DE ATIVO MENSAL ADICIONAL - AMIZADE
   --------------------------------------------------------------------------------------
  
   -- Obtem a quantidade minima de Diretos para a bonificação adicional - AMIZADE (Qtde zero ou null a bonificação esta desetivada)
   Declare
       @Param_QtdeMinDiretos INT = 0

   Select 
      @Param_QtdeMinDiretos = convert(int, Dados)
   From 
      Sistema.Configuracao c
   Where
      Chave = 'REDE_QTDE_DIRETOS_BONUS_ADICIONAL'

   If (@Param_QtdeMinDiretos > 0)
   Begin
      Declare
         @UsuarioID              INT,
	      @PedidoId               INT, 
	      @PatrocinadorID         INT, 
	      @DtAtivacao             DATETIME, 
         @VlrBonusAdicionalVenda DECIMAL(10,2),
	      @DtValidade             DATETIME, 
         @QtdeDiretos            INT

      Declare
         @AntFetch     INT,
         @PatrociAuxID INT = 0,
         @CountDiretos INT = 0

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
         T.PatrocinadorID, T.DtAtivacao
      
      Open curBonus
      If (Select @@CURSOR_ROWS) <> 0
      Begin
         Fetch Next From curBonus Into  @UsuarioID, @PedidoId, @PatrocinadorID, @DtAtivacao, @VlrBonusAdicionalVenda, @DtValidade, @QtdeDiretos
         Select @AntFetch = @@fetch_status

         IF (@AntFetch = 0)
            Select @PatrociAuxID = @PatrocinadorID,
                   @CountDiretos = @QtdeDiretos

         While @AntFetch = 0
         Begin
            If (@PatrociAuxID != @PatrocinadorID)
            Begin
               Update Usuario.Usuario Set QtdeDiretos = @CountDiretos Where ID = @PatrociAuxID

               Select @PatrociAuxID = @PatrocinadorID,
                      @CountDiretos = @QtdeDiretos
            End

            Select @CountDiretos = @CountDiretos + 1

            If (@CountDiretos >= @Param_QtdeMinDiretos)
            Begin
               Insert Into Rede.Bonificacao 
                  (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor, PedidoID)		
               Values 
                  (1, @PatrocinadorID, @UsuarioID, 0,  @DtAtivacao, @VlrBonusAdicionalVenda, @PedidoId);
            End

            Fetch Next From curBonus Into  @UsuarioID, @PedidoId, @PatrocinadorID, @DtAtivacao, @VlrBonusAdicionalVenda, @DtValidade, @QtdeDiretos
            Select @AntFetch = @@fetch_status -- Para ver se nao é fim do loop  
         End -- While
      End -- @@CURSOR_ROWS
   End  -- FIM - BONIFICAÇÃO DE ATIVO MENSAL ADICIONAL - AMIZADE


   Select * from #TUsuarios

   -- Remove todas as tabelas temporárias
   Drop Table #TUsuarios

   COMMIT TRANSACTION
END TRY

BEGIN CATCH
   ROLLBACK TRANSACTION
      
   DECLARE @error int, @message varchar(4000), @xstate int;
   SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
   RAISERROR ('Erro na execucao de spDG_RE_GeraBonusIndicacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
END CATCH 

go
   Grant Exec on spDG_RE_GeraBonusIndicacao To public
go