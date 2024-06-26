USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spDG_FI_GeraLancamentos]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

drop Proc [dbo].[spDG_FI_GeraLancamentos]
GO

Create Proc [dbo].[spDG_FI_GeraLancamentos]

AS
-- =============================================================================================
-- Author.....: Marcos Hemmi
-- Create date: 28/01/2017
-- Description: Gera registros de lancamento financeiro de bonificacoes ainda nao processadas
-- =============================================================================================

BEGIN

   BEGIN TRY
      BEGIN TRANSACTION

	  DECLARE @TipoContaDinheiro int = 1
	  DECLARE @TipoContaDinheiroBonusAlavancagem int = 3
	  
	   -- Marca os registros ainda não processados como "em processamento"
	   Update 
          Rede.Bonificacao 
       Set StatusID = 1 
       Where StatusID = 0 
         and Valor > 0

	   -- Insere registros de indicação (em dinheiro)
	   Insert Into Financeiro.Lancamento
          (UsuarioID, ContaID, TipoID, CategoriaID, ReferenciaID, Valor, Descricao, DataCriacao, DataLancamento, PedidoID, RegraItemID, CicloID)
	   Select Distinct
		   B.UsuarioID,  @TipoContaDinheiro AS ContaID, 6 AS TipoID, B.CategoriaID, B.ReferenciaID, B.Valor, B.Descricao AS Descricao, dbo.GetDateZion() AS DataCriacao,
		   B.Data AS DataLancamento, B.PedidoID, B.RegraItemID, B.CicloID
	   From 
           Rede.Bonificacao B (nolock)
		   INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
		   INNER JOIN Usuario.Usuario      U (nolock) ON U.ID = B.ReferenciaID
	   Where 
           B.StatusID = 1
       and B.CategoriaID IN (21,23, 24)  /* indicacao, residual, plus*/
	
	   -- Insere registros de bônus Rede (em dinheiro)
	   Insert Into Financeiro.Lancamento
        (UsuarioID, ContaID, TipoID, CategoriaID, ReferenciaID, Valor, Descricao, DataCriacao, DataLancamento, PedidoID)
	   Select
		   B.UsuarioID, @TipoContaDinheiro AS ContaID, 6 AS TipoID, B.CategoriaID, B.ReferenciaID, B.Valor,
		   C.Nome AS Descricao, dbo.GetDateZion() AS DataCriacao, B.Data AS DataLancamento, B.PedidoID
	   From 
           Rede.Bonificacao B (nolock)
		   INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
	   Where 
           B.StatusID = 1 
       and B.CategoriaID IN (25, 26) /*equipe (binario), carreira*/
	   
	   -- Insere registros de bônus Alavancagem (saldo com saque especifico)
	   Insert Into Financeiro.Lancamento
        (UsuarioID, ContaID, TipoID, CategoriaID, ReferenciaID, Valor, Descricao, DataCriacao, DataLancamento, PedidoID)
	   Select
		   B.UsuarioID, @TipoContaDinheiroBonusAlavancagem AS ContaID, 6 AS TipoID, B.CategoriaID, B.ReferenciaID, B.Valor,
		   C.Nome AS Descricao, dbo.GetDateZion() AS DataCriacao, B.Data AS DataLancamento, B.PedidoID
	   From 
           Rede.Bonificacao B (nolock)
		   INNER JOIN Financeiro.Categoria C (nolock) ON C.ID = B.CategoriaID
	   Where 
           B.StatusID = 1 
       and B.CategoriaID IN (22) /*equipe (binario), carreira*/
	   

	   -- Marca os registros "em processamento" como "processados"
	   Update 
           Rede.Bonificacao 
       Set StatusID = 2 
       Where 
           StatusID = 1 
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
 

GO
