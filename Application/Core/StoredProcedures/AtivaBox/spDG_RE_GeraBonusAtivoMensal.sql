use AtivaBox
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusAtivoMensal'))
   Drop Procedure spDG_RE_GeraBonusAtivoMensal
go

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description:  Gera registros de Bonificacao de indicação da primeira compra e bonus Amizade 
-- =============================================================================================

Create Proc [dbo].spDG_RE_GeraBonusAtivoMensal
As
BEGIN TRY
   Set nocount on
   
   --Cursor
   Declare
      @UsuarioID int,
      @PedidoID  int,
      @Valor     Decimal(10,2),
      @Data      datetime,
      @AntFetch  int,
      @MaxNivel  int = 3 -- Valor Defaut

   -- Obtem o Nivel maximo de diretos parametrizado no sistema
   Select
      @MaxNivel = convert(int, Dados)
   From 
      Sistema.Configuracao
   Where
      Chave = 'REDE_QTDE_NIVEIS_DIRETOS'

   Declare
      CursorUsuario
   Cursor For
      Select 
          u.id usuario,
          P.ID pedido,
		    PV.Bonificacao,
          PS.Data
      From 
         Loja.pedido P (nolock), 
         Loja.PedidoItem PI (nolock),
         Loja.Produto PROD (nolock),
         Loja.ProdutoValor PV (nolock),
         usuario.Usuario U (nolock),
         Usuario.Usuario U2 (nolock),
         Loja.PedidoPagamento PP (nolock), 
         Loja.PedidoPagamentoStatus PS (nolock) 
      Where P.ID = PI.PedidoID
        and PI.ProdutoID = PV.ProdutoID
        and PI.ProdutoID = PROD.ID 
        and PROD.AtivoMensal = 1 
        and P.UsuarioID = U.ID
        and U.PatrocinadorDiretoID = U2.ID
        and P.ID = PP.PedidoID
        and PP.ID = PS.PedidoPagamentoID
        and PS.StatusID = 3    -- somente pedidos pagos
		  and PV.Bonificacao > 0 -- somente produtos com valor para vonus de venda (indicação)
        and U.GeraBonus = 1    -- Somente ususarios que geram bonus 1= sim, 0 = nao
        and U.Bloqueado = 0    -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao
        and U2.RecebeBonus = 1 -- Patrocinador recebe bonus
        and U2.Bloqueado = 0   -- Patrocinador nao esta bloqueado
        and U2.DataValidade >= dbo.GetDateZion() --so recebe se estiver com ativo mensal pago

        and PS.Data >= CONVERT(VARCHAR(8),dbo.GetDateZion(),112)      
        and NOT EXISTS (SELECT 1 FROM rede.Bonificacao RB WHERE RB.PedidoID = P.ID And RB.CategoriaID = 12)
      
   Open CursorUsuario

      If (Select @@CURSOR_ROWS) <> 0
      Begin
         BEGIN TRANSACTION

         Fetch Next From CursorUsuario Into  @UsuarioID, @PedidoID, @Valor, @Data 
         Select @AntFetch = @@fetch_status

         While @AntFetch = 0
         Begin
           --Exec spC_ObtemSuperiores @UsuarioID,@UsuarioID,1,6,-1 
           Exec spC_ObtemSuperiores @UsuarioID, @UsuarioID, 1, @MaxNivel, -1 

           Insert Into Rede.Bonificacao
             ([CategoriaID],[UsuarioID],[ReferenciaID],[StatusID],[Data],[Valor],[PedidoID])
           Select
              12 CategoriaID,           -- Bonus Active
              sup.IDSuperior UsuarioID, -- usuario que recebe
              @UsuarioID ReferenciaID,  -- Usuario de quem se recebeu
              0 StatusID,               -- 0 a ser pago - a rotina de atualiza lancamento pega o q for zero
              @Data Data,               -- data do pedido
              @Valor Valor,             -- valor da bonificacao
              @PedidoID PedidoID        -- pedido que gerou a bonificacao
           From
              Usuario.Superiores sup,
              Usuario.Usuario    usu 
           Where
              sup.id = @UsuarioID and
              sup.IDSuperior = usu.id and
              usu.RecebeBonus = 1 and   -- Patrocinador recebe bonus
              usu.Bloqueado = 0 and     -- Patrocinador nao esta bloqueado
              usu.DataValidade >= GETDATE() --so recebe se estiver com ativo mensal pago

            Fetch Next From CursorUsuario Into  @UsuarioID, @PedidoID, @Valor, @Data             
            Select @AntFetch = @@fetch_status   -- Para ver se nao é fim do loop      
         End -- While

         COMMIT TRANSACTION
      End -- @@CURSOR_ROWS

      Close CursorUsuario
      Deallocate CursorUsuario

END TRY

BEGIN CATCH
   If @@Trancount > 0
      ROLLBACK TRANSACTION
      
   DECLARE @error int, @message varchar(4000), @xstate int;
   SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
   RAISERROR ('Erro na execucao de spDG_RE_GeraBonusAtivoMensal: %d: %s', 16, 1, @error, @message) WITH SETERROR;
END CATCH 

go
   Grant Exec on spDG_RE_GeraBonusAtivoMensal To public
go

--exec spG_AtivoMensalPagamento
--select * from Rede.Bonificacao order by id desc
