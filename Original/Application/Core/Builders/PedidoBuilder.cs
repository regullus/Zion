using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using Core.Repositories.Rede;
using Core.Repositories.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Builders
{
    internal class PedidoBuilder
    {

        private PedidoRepository pedidoRepository;
        private PedidoItemRepository pedidoItemRepository;
        private PedidoItemStatusRepository pedidoItemStatusRepository;
        private PedidoTaxaRepository pedidoTaxaRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private PedidoPagamentoStatusRepository pedidoPagamentoStatusRepository;
        private MoedaRepository moedaRepository;

        private ProdutoRepository produtoRepository;
        private TaxaRepository taxaRepository;
        private ContaRepository contaRepository;

        private Entities.Pedido _pedido;

        public PedidoBuilder(DbContext context)
        {
            pedidoRepository = new PedidoRepository(context);
            pedidoItemRepository = new PedidoItemRepository(context);
            pedidoItemStatusRepository = new PedidoItemStatusRepository(context);
            pedidoTaxaRepository = new PedidoTaxaRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoPagamentoStatusRepository = new PedidoPagamentoStatusRepository(context);
            moedaRepository = new MoedaRepository(context);

            produtoRepository = new ProdutoRepository(context);
            taxaRepository = new TaxaRepository(context);
            contaRepository = new ContaRepository(context);
        }

        public void CriarPedido(Entities.Usuario usuario, Entities.Endereco enderecoEntrega, Entities.Endereco enderecoFaturamento)
        {
            _pedido = new Entities.Pedido();
            _pedido.Codigo = CriarCodigo();
            _pedido.DataCriacao = App.DateTimeZion;
            _pedido.EnderecoEntregaID = enderecoEntrega.ID == 0 ? 1 : enderecoEntrega.ID;
            _pedido.EnderecoFaturamentoID = enderecoFaturamento.ID == 0 ? 1 : enderecoFaturamento.ID;
            _pedido.UsuarioID = usuario.ID;
            _pedido.Subtotal = 0;
            _pedido.Total = 0;

            pedidoRepository.Save(_pedido);
        }

        public void CriarPedido(Entities.Usuario usuario, Entities.Endereco enderecoEntrega, Entities.Endereco enderecoFaturamento, string codigoPedido)
        {
            _pedido = new Entities.Pedido();
            _pedido.Codigo = codigoPedido;
            _pedido.DataCriacao = App.DateTimeZion;
            _pedido.EnderecoEntregaID = enderecoEntrega.ID == 0 ? 1 : enderecoEntrega.ID;
            _pedido.EnderecoFaturamentoID = enderecoFaturamento.ID == 0 ? 1 : enderecoFaturamento.ID;
            _pedido.UsuarioID = usuario.ID;
            _pedido.Subtotal = 0;
            _pedido.Total = 0;

            pedidoRepository.Save(_pedido);
        }

        public void AdicionarItem(int quantidade, Entities.Produto produto, Entities.ProdutoValor valor, Entities.ProdutoOpcao opcao = null)
        {
            var item = new Entities.PedidoItem()
            {
                BonificacaoUnitaria = valor.Bonificacao,
                PedidoID = _pedido.ID,
                Quantidade = quantidade,
                ProdutoID = produto.ID,
                ProdutoOpcaoID = opcao != null ? (int?)opcao.ID : null,
                ValorUnitario = valor.Valor
            };
            pedidoItemRepository.Save(item);

            var status = new Entities.PedidoItemStatus()
            {
                Data = App.DateTimeZion,
                Mensagem = "",
                PedidoItemID = item.ID,
                Status = Entities.PedidoItemStatus.TodosStatus.AguardandoPagamento
            };
            pedidoItemStatusRepository.Save(status);
        }

        public void CalcularSubtotal()
        {
            _pedido = GetPedido();
            _pedido.Subtotal = (double)_pedido.PedidoItem.Sum(i => i.Quantidade * i.ValorUnitario);
            pedidoRepository.Save(_pedido);
        }

        public void AplicarTaxas(int taxaID, double valor)
        {
            var pedidoTaxa = new Entities.PedidoTaxa()
            {
                PedidoID = _pedido.ID,
                TaxaID = taxaID,
                Valor = valor
            };
            pedidoTaxaRepository.Save(pedidoTaxa);
        }

        public void AplicarTaxas()
        {
            var taxas = taxaRepository.GetByCategoria(22);

            foreach (var taxa in taxas)
            {
                double? valor = 0;
                if (!taxa.TipoProdutoIDs.Any())
                {
                    switch (taxa.Tipo)
                    {
                        case Entities.Taxa.Tipos.Fixo:
                            valor = taxa.Valor;
                            break;
                        case Entities.Taxa.Tipos.Percentual:
                            valor = _pedido.Subtotal * taxa.Valor / 100;
                            break;
                    }
                }
                else
                {
                    foreach (var item in _pedido.PedidoItem)
                    {
                        var produto = produtoRepository.Get(item.ProdutoID);
                        if (taxa.TipoProdutoID.Contains(produto.TipoID))
                        {
                            switch (taxa.Tipo)
                            {
                                case Entities.Taxa.Tipos.Fixo:
                                    valor += item.Quantidade * taxa.Valor;
                                    break;
                                case Entities.Taxa.Tipos.Percentual:
                                    valor += item.Quantidade * item.ValorUnitario * taxa.Valor / 100;
                                    break;
                            }
                        }
                    }
                }

                var pedidoTaxa = new Entities.PedidoTaxa()
                {
                    PedidoID = _pedido.ID,
                    TaxaID = taxa.ID,
                    Valor = valor
                };
                pedidoTaxaRepository.Save(pedidoTaxa);
            }
        }

        public void CalcularTotal()
        {
            _pedido = GetPedido();

            var taxas = 0d;

            if (_pedido.PedidoTaxa != null && _pedido.PedidoTaxa.Any() && _pedido.PedidoTaxa.Any(a => a.Taxa != null))
            {
                taxas = _pedido.PedidoTaxa.Where(w => w.Taxa.CategoriaID != 22 && w.Valor.HasValue).Sum(t => t.Valor).Value;
            }

            var total = _pedido.Subtotal + taxas + _pedido.ValorFrete + _pedido.ValorJuros;

            _pedido.Total = total > 0 ? total : 0;
            pedidoRepository.Save(_pedido);
        }

        public void AdicionarPagamento(Entities.PedidoPagamento.MeiosPagamento meioPagamento, Entities.PedidoPagamento.FormasPagamento formaPagamento = Entities.PedidoPagamento.FormasPagamento.Padrao, decimal? valor = null, int? contaID = null, Entities.Usuario usuario = null, double? ValorCripto = null, double? CotacaoCripto = null)
        {
            Entities.Conta conta = null;
            if (contaID.HasValue)
            {
                conta = contaRepository.Get(contaID.Value);
            }

            Entities.Moeda moeda = ConfiguracaoHelper.GetMoedaPadrao();

            var pagamento = new Entities.PedidoPagamento()
            {
                ContaID = contaID,
                FormaPagamento = formaPagamento,
                MeioPagamento = meioPagamento,
                Mensagem = "",
                MoedaID = (int)(conta != null ? conta.MoedaID : (moeda != null ? moeda.ID : _pedido.Usuario.Pais.MoedaID)),
                Numero = "",
                PedidoID = _pedido.ID,
                ReferenciaID = 0,
                UsuarioID = usuario != null ? usuario.ID : _pedido.UsuarioID,
                Valor = valor.HasValue ? (double)valor.Value : (double)_pedido.Total,
                MoedaIDCripto = 4,
                ValorCripto = ValorCripto,
                CotacaoCripto = CotacaoCripto,
            };
            pedidoPagamentoRepository.Save(pagamento);

            var status = new Entities.PedidoPagamentoStatus()
            {
                Data = App.DateTimeZion,
                PedidoPagamentoID = pagamento.ID,
                Status = Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento
            };
            pedidoPagamentoStatusRepository.Save(status);
        }

        public Entities.Pedido GetPedido()
        {
            return pedidoRepository.Get(_pedido.ID);
        }

        private string CriarCodigo()
        {

            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var tamanho = 10;
            var tamanhoMaximo = caracteres.Length;
            StringBuilder codigo;
            try
            {
                do
                {
                    Random random = new Random();
                    codigo = new StringBuilder(tamanho);
                    for (int indice = 0; indice < tamanho; indice++)
                    {
                        codigo.Append(caracteres[random.Next(0, tamanhoMaximo)]);
                    }
                } while (pedidoRepository.GetByCodigo(codigo.ToString()) != null);
                return codigo.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void SalvarDadosParcelamento(int parcelas, double jurosTotais)
        {
            _pedido = GetPedido();
            _pedido.Parcelamento = parcelas;
            _pedido.ValorJuros = jurosTotais;
            pedidoRepository.Save(_pedido);
        }

        public void SalvarDadosPagamentoCripto(string numero, int ReferenciaID, decimal ValorCripto, decimal cotacao, decimal valor, int moedaIDCripto)
        {
            _pedido = GetPedido();
            _pedido.PedidoPagamento.ToList().ForEach(x => x.Numero = numero);
            _pedido.PedidoPagamento.ToList().ForEach(x => x.ReferenciaID = ReferenciaID);
            _pedido.PedidoPagamento.ToList().ForEach(x => x.Valor = Convert.ToDouble(valor));
            _pedido.PedidoPagamento.ToList().ForEach(x => x.CotacaoCripto = Convert.ToDouble(cotacao));
            _pedido.PedidoPagamento.ToList().ForEach(x => x.ValorCripto = Convert.ToDouble(ValorCripto));
            _pedido.PedidoPagamento.ToList().ForEach(x => x.MoedaIDCripto = moedaIDCripto);
            pedidoRepository.Save(_pedido);
        }
    }
}
