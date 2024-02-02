Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_FI_GeraLancamentos'))
   Drop Procedure spDG_FI_GeraLancamentos
go

Create Proc [dbo].spDG_FI_GeraLancamentos

AS
-- =============================================================================================
-- Author.....: 
-- Create date: 12/06/2017
-- Description: Gera registros de lancamento financeiro de bonificacoes ainda nao processadas
-- =============================================================================================
BEGIN

   BEGIN TRY
      BEGIN TRANSACTION

	  -- Marca os registros ainda não processados como "em processamento"
	  Update Rede.Bonificacao 
      Set StatusID = 1 
      Where StatusID = 0 
        and Valor > 0

	  -- Insere registros de indicação (em dinheiro)
	  Insert Into Financeiro.Lancamento
         (UsuarioID,
         ContaID,
         TipoID,
         CategoriaID,
         ReferenciaID,
         Valor,
         Descricao,
         DataCriacao,
         DataLancamento,
         PedidoID)
	  Select
		   B.UsuarioID,
		   1 AS ContaID,
		   6 AS TipoID,
		   B.CategoriaID,
		   B.ReferenciaID,
	  	   B.Valor,
		   (C.Nome + ' (' + U.Login + ')') AS Descricao,
		   dbo.GetDateZion() AS DataCriacao,
		   B.Data AS DataLancamento,
           B.PedidoID
	  From Rede.Bonificacao B (nolock)
		   INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
		   INNER JOIN Usuario.Usuario      U (nolock) ON U.ID = B.ReferenciaID
	  Where B.StatusID = 1
        and B.CategoriaID IN (13,14) 
	
	  -- Insere registros de bônus Rede (em dinheiro)
	  Insert Into Financeiro.Lancamento
         (UsuarioID,
         ContaID,
         TipoID,
         CategoriaID,
         ReferenciaID,
         Valor,
         Descricao,
         DataCriacao,
         DataLancamento,
         PedidoID)
	  Select
		   B.UsuarioID,
		   1 AS ContaID,
		   6 AS TipoID,
		   B.CategoriaID,
		   B.ReferenciaID,
		   B.Valor,
		   C.Nome AS Descricao,
		   dbo.GetDateZion() AS DataCriacao,
		   B.Data AS DataLancamento,
           B.PedidoID
	   From Rede.Bonificacao B (nolock)
		    INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
	   Where B.StatusID = 1 
         and B.CategoriaID IN (15,16)

	   -- Marca os registros "em processamento" como "processados"
	   Update Rede.Bonificacao 
       Set StatusID = 2 
       Where StatusID = 1 
         and Valor > 0

       COMMIT TRANSACTION
   END TRY

   BEGIN CATCH
      If @@Trancount > 0
         ROLLBACK TRANSACTION
      
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_FI_GeraLancamentos: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH
END 

go
   Grant Exec on spDG_FI_GeraLancamentos To public
go