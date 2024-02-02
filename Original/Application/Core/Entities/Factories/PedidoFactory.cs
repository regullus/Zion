using Core.Builders;
using Core.Services.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Factories
{
    public class PedidoFactory
    {

        private PedidoBuilder pedidoBuilder;

        public PedidoFactory(DbContext context)
        {
            pedidoBuilder = new PedidoBuilder(context);
        }

        public Entities.Pedido Criar(Entities.Usuario usuario, Entities.Produto produto, Entities.ProdutoValor valor, Entities.PedidoPagamento.MeiosPagamento meioPagamento)
        {
            pedidoBuilder.CriarPedido(usuario, usuario.EnderecoPrincipal, usuario.EnderecoPrincipal);
            pedidoBuilder.AdicionarItem(1, produto, valor);
            pedidoBuilder.CalcularSubtotal();
            pedidoBuilder.AplicarTaxas();
            pedidoBuilder.CalcularTotal();
            pedidoBuilder.AdicionarPagamento(meioPagamento);

            var _pedido = pedidoBuilder.GetPedido();

            return _pedido;
        }

        public Entities.Pedido Criar(Models.Loja.CarrinhoModel carrinho)
        {
            //TODO: Construir pedidos para kits

            if (carrinho.Itens.Any(i => !i.Valor.Valor.HasValue || i.Valor.Valor.Value < 0))
            {
                throw new Exception("##Valor inválido");
            }

            if (string.IsNullOrEmpty(carrinho.CodigoPedido))
                pedidoBuilder.CriarPedido(carrinho.Usuario, carrinho.EnderecoEntrega, carrinho.EnderecoFaturamento);
            else
                pedidoBuilder.CriarPedido(carrinho.Usuario, carrinho.EnderecoEntrega, carrinho.EnderecoFaturamento, carrinho.CodigoPedido);

            foreach (var item in carrinho.Itens)
            {
                pedidoBuilder.AdicionarItem(item.Quantidade, item.Produto, item.Valor, item.Opcao);
            }

            pedidoBuilder.CalcularSubtotal();

            pedidoBuilder.AplicarTaxas();

            if (carrinho.ValorFrete > 0 && Core.Helpers.ConfiguracaoHelper.TemChave("FRETE_ID_TAXA_FRETE"))
            {
                var idTaxaFrete = Core.Helpers.ConfiguracaoHelper.GetInt("FRETE_ID_TAXA_FRETE");
                pedidoBuilder.AplicarTaxas(idTaxaFrete, carrinho.ValorFrete);
            }

            if (carrinho.Parcelas > 1 && carrinho.JurosTotal > 0)
            {
                pedidoBuilder.SalvarDadosParcelamento(carrinho.Parcelas, carrinho.JurosTotal);
            }

            pedidoBuilder.CalcularTotal();

            foreach (var pagamento in carrinho.Pagamentos)
            {
                pedidoBuilder.AdicionarPagamento(pagamento.MeioPagamento, pagamento.FormaPagamento, pagamento.Valor, pagamento.ContaID, pagamento.Usuario);
            }

            var _pedido = pedidoBuilder.GetPedido();

            return _pedido;
        }

        public void SalvarDadosPagamentoCripto(string numero, int ReferenciaID, decimal valorCrypto, decimal cotacao, decimal valor, int moedaIDCripto)
        {
            pedidoBuilder.SalvarDadosPagamentoCripto(numero, ReferenciaID, valorCrypto, cotacao, valor, moedaIDCripto);
        }
    }
}
