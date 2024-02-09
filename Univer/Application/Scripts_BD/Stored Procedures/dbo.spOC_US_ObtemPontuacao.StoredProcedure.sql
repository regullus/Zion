USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_US_ObtemPontuacao]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....: Edemar
-- Create date: 23/05/2017
-- Description: Obtem os pontos da perna menor da rede binario de um usuario
-- =============================================================================================

Create Proc [dbo].[spOC_US_ObtemPontuacao]
   @idUsuario int

As
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF

   Declare @assinatura varchar(max) = (Select Assinatura from Usuario.Usuario (nolock) where id = @idUsuario);

   Declare @pernaDireita  float = ISNULL( (Select 
                                             SUM( PV.Bonificacao ) 
                                           From Loja.Pedido P (nolock)
                                             INNER JOIN Usuario.Usuario U (nolock) ON P.UsuarioID = U.ID
                                             INNER JOIN Loja.PedidoItem PI (nolock) ON P.ID = PI.PedidoID
                                             INNER JOIN Loja.Produto PR (nolock) ON PI.ProdutoID = PR.ID
                                             INNER JOIN Loja.ProdutoValor PV (nolock) ON PV.ProdutoID = PR.ID
                                             INNER JOIN Loja.PedidoPagamento PP (nolock) ON P.ID = PP.PedidoID
                                             INNER JOIN Loja.PedidoPagamentoStatus PPS (nolock) ON PP.ID = PPS.PedidoPagamentoID
                                           where u.Assinatura LIKE @assinatura + '1%'                                          
                                             and U.GeraBonus = 1
                                             and PPS.StatusID = 3 
                                             and PR.TipoID IN (1,2) 
		                                    ) , 0 )

	Declare @pernaEsquerda float = ISNULL( (Select 
                                             SUM( PV.Bonificacao ) 
                                           From Loja.Pedido P (nolock)
                                             INNER JOIN Usuario.Usuario U (nolock) ON P.UsuarioID = U.ID
                                             INNER JOIN Loja.PedidoItem PI (nolock) ON P.ID = PI.PedidoID
                                             INNER JOIN Loja.Produto PR (nolock) ON PI.ProdutoID = PR.ID
                                             INNER JOIN Loja.ProdutoValor PV (nolock) ON PV.ProdutoID = PR.ID
                                             INNER JOIN Loja.PedidoPagamento PP (nolock) ON P.ID = PP.PedidoID
                                             INNER JOIN Loja.PedidoPagamentoStatus PPS ON PP.ID = PPS.PedidoPagamentoID
                                           where u.Assinatura LIKE @assinatura + '0%'                                           
                                             and U.GeraBonus = 1
                                             and PPS.StatusID = 3 
                                             and PR.TipoID IN (1,2) 
										            ) , 0 )
 
   -- Verifica qual o valor menor e exibe
	if (@pernaDireita < @pernaEsquerda)
       Select round(@pernaDireita , 8, 1) Pontos
    else
       Select round(-@pernaEsquerda, 8, 1) Pontos
     
END -- Sp


GO
