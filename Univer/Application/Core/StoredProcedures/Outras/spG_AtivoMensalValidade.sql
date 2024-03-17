use [ClubeVantagens]
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spG_AtivoMensalValidade'))
   Drop Procedure spG_AtivoMensalValidade

go

Create Proc [dbo].[spG_AtivoMensalValidade]
   
As
Begin
Set nocount on

   Declare 
      @DataHoje datetime,
      @DataIni datetime,
      @DataFim datetime,
      @Total decimal(10,2),
      @ValorCompra decimal(10,2) 

   --Obtem a data de hoje
   set @DataHoje = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)

   --Obtem o valor de compra no periodo para ativação mensal
   Select
      @ValorCompra = convert(decimal(10,2), Dados)
   From 
      Sistema.Configuracao
   Where
      Chave = 'ATIVO_MENSAL_VALOR'

   --Cursor
   Declare
      @UsuarioID int,
      @AntFetch int

  Declare
      CursorUsuario
   Cursor For
   Select 
      ID
   From
      Usuario.Usuario
   Where
      StatusID = 2 --Ativo
   Order by 
      ID 
      
 Open CursorUsuario

   If (Select @@CURSOR_ROWS) <> 0
      Begin
         Fetch Next From CursorUsuario Into @UsuarioID
         Select @AntFetch = @@fetch_status

         While @AntFetch = 0
         Begin
            set @DataFim = null
            set @DataIni = null
            set @Total = 0

            --Obtem a data de inicio da ultima ativacao mensal
            select 
               @DataFim = DataValidade,
               @DataIni = DATEADD(day,-30,DataValidade)
            From 
               usuario.Usuario 
            Where 
               ID = @UsuarioID

            --Obtem Total gasto pelo afiliado apos sua data de ativação - 30 dias
            Select
               @Total = SUM(pedi.Total)
            From
               loja.Pedido pedi,
               loja.PedidoItem peditem,
               loja.Produto prod,
               Loja.PedidoPagamento pedpag,
               Loja.PedidoPagamentoStatus pedstatus
            Where
               pedi.ID = peditem.PedidoID and
               pedi.UsuarioID = @UsuarioID and
               pedi.ID = pedpag.PedidoID and
               prod.ID = peditem.ProdutoID and
               pedpag.ID = pedstatus.PedidoPagamentoID and
               pedstatus.StatusID = 3 and --Pago
               prod.TipoID = 3 and  --Produto 
               pedstatus.Data > @DataIni

            --Teste
            --if(@UsuarioID = 26)
            --Begin
            --Select 
            --      @UsuarioID Usuario,
            --      @Total Total,
            --      @ValorCompra ValorCompra,
            --      @DataIni Dataini
            --End

            --Verifica se compra foi efetuada se sim altera data de validade do afiliado
            if(@Total >= @ValorCompra)
            Begin
               Update
                  Usuario.Usuario
               Set
               DataValidade =
                  Case 
                     When (@DataFim >= @DataHoje and @DataIni <= @DataHoje) Then (DATEADD(day, 30 , @DataFim))
                     When (@DataFim >= @DataHoje and @DataIni > @DataHoje) Then (DataValidade)
                     When (@DataFim < @DataHoje) Then (DATEADD(day, 30 , @DataHoje))
                  End
               Where
                  ID = @UsuarioID
            End 

            Fetch Next From CursorUsuario Into @UsuarioID    
            -- Para ver se nao é fim do loop
            Select @AntFetch = @@fetch_status         
         End -- While
      End -- @@CURSOR_ROWS

      Close CursorUsuario
      Deallocate CursorUsuario

End -- Sp

go
   Grant Exec on spG_AtivoMensalValidade To public
go

--exec spG_AtivoMensalValidade
--select id, login, statusid, datavalidade  from usuario.usuario where login in ('cdvbrasiloficial','cdvbrazil1')