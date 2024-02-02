using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.Models.Loja
{
    public enum CarrinhoTipoFrete
    {
        Gratis = 0,
        Correio = 1
    }

    public class CarrinhoModel
    {
        private const string SESSION_KEY = "_carrinho";

        public Entities.Usuario Usuario;
        public List<CarrinhoItemModel> Itens;
        public List<CarrinhoPagamentoModel> Pagamentos;
        public List<CarrinhoTaxaModel> Taxas;
        public Entities.Endereco EnderecoEntrega;
        public Entities.Endereco EnderecoFaturamento;

        public Core.Entities.CorreioFrete Frete;

        public string CodigoPedido { get; set; }

        public int Parcelas { get; private set; }

        public double JurosTotal { get; private set; }

        public double? Subtotal
        {
            get { return Itens.Sum(i => i.Quantidade * i.Valor.Valor); }
        }

        public double? SubtotalBonificacao
        {
            get { return Itens.Sum(i => i.Quantidade * i.Valor.Bonificacao); }
        }

        public double ValorFrete
        {
            get
            {
                double val = 0;
                if (Frete != null) val = Frete.Valor;
                return val;
            }
        }

        public double Total
        {
            get { return (double)(Subtotal + ValorFrete + (double)Taxas.Sum(t => t.Valor)); }
        }

        public double TotalBonificacao
        {
            get { return (double)(SubtotalBonificacao + ValorFrete + (double)Taxas.Sum(t => t.Valor)); }
        }

        public bool Vazio
        {
            get { return !this.Itens.Any(); }
        }

        public CarrinhoModel(Entities.Usuario usuario)
        {
            Usuario = usuario;
            Resetar();
        }

        public void Resetar()
        {
            Itens = new List<CarrinhoItemModel>();
            Pagamentos = new List<CarrinhoPagamentoModel>();
            Taxas = new List<CarrinhoTaxaModel>();
        }

        public void Adicionar(Entities.Produto produto, Entities.ProdutoValor valor, int quantidade = 1, Entities.ProdutoOpcao opcao = null)
        {
            var item = Itens.FirstOrDefault(i => i.Produto.ID == produto.ID && i.Valor.ID == valor.ID);
            if (item != null)
            {
                item.Quantidade += quantidade;
            }
            else
            {
                Itens.Add(new CarrinhoItemModel()
                {
                    Opcao = opcao,
                    Produto = produto,
                    Quantidade = quantidade,
                    Valor = valor
                });
            }
            CalcularTaxas();
        }

        public void Atualizar(int produtoID, int quantidade)
        {
            var item = Itens.FirstOrDefault(i => i.Produto.ID == produtoID);
            if (item != null)
            {
                if (quantidade > 0)
                {
                    item.Quantidade = quantidade;
                }
                else
                {
                    Itens.Remove(item);
                }
            }
            CalcularTaxas();
        }

        public void Remover(int produtoID)
        {
            var item = Itens.FirstOrDefault(i => i.Produto.ID == produtoID);
            if (item != null)
            {
                Itens.Remove(item);
            }
            CalcularTaxas();
        }

        public void Adicionar(Entities.PedidoPagamento.MeiosPagamento meioPagamento, Entities.PedidoPagamento.FormasPagamento formaPagamento, Entities.Usuario.TodosTiposAtivacao tipoPagamento)
        {
            if (tipoPagamento == Entities.Usuario.TodosTiposAtivacao.Dinheiro)
            {
                Adicionar(meioPagamento, formaPagamento);
            }
            else
            {
                var contaIDPontos = 2;
                Adicionar(meioPagamento, formaPagamento, null, contaIDPontos);
            }
        }

        public void Adicionar(Entities.PedidoPagamento.MeiosPagamento meioPagamento, Entities.PedidoPagamento.FormasPagamento formaPagamento, Entities.Usuario usuario = null, int? contaID = null, decimal? valor = null)
        {
            Pagamentos.Add(new CarrinhoPagamentoModel()
            {
                ContaID = contaID,
                FormaPagamento = formaPagamento,
                MeioPagamento = meioPagamento,
                Usuario = usuario,
                Valor = valor
            });
            CalcularTaxas();
        }

        public void Limpar()
        {
            HttpContext.Current.Session[SESSION_KEY] = null;

            Itens = new List<CarrinhoItemModel>();
            Pagamentos = new List<CarrinhoPagamentoModel>();
            EnderecoEntrega = null;
            EnderecoFaturamento = null;
        }

        public void LimparFrete()
        {
            Frete = null;
        }

        public void SetarFrete(CarrinhoTipoFrete tipoFrete)
        {
            if (EnderecoEntrega != null)
            {
                SetarFrete(tipoFrete, EnderecoEntrega.CodigoPostal);
            }
        }

        public void SetarFrete(CarrinhoTipoFrete tipoFrete, string cep)
        {
            if (!Core.Helpers.ConfiguracaoHelper.TemChave("FRETE_HABILITADO") || !Core.Helpers.ConfiguracaoHelper.GetBoolean("FRETE_HABILITADO"))
            {
                Frete = null;
                return;
            }

            if (tipoFrete == CarrinhoTipoFrete.Correio)
            {
                //TODO: campo "comprimento", "altura" e "largura" do produto 
                var comprimentoTotal = 30;
                var larguraTotal = 30;
                var alturaTotal = 30;
                var pesoTotal = Itens.Sum(i => i.Produto.Peso);

                var f = Services.Integracao.CorreiosService.Calcular("", cep, (float)pesoTotal, comprimentoTotal, alturaTotal, larguraTotal, Subtotal);

                if (f != null && f.Valor > 0)
                {
                    Frete = f;
                }
            }

            if (tipoFrete == CarrinhoTipoFrete.Gratis)
            {
                Frete = new Entities.CorreioFrete() { PrazoDias = 0, Valor = 0 };
            }
        }

        private void CalcularTaxas()
        {
            foreach (var taxa in Taxas)
            {
                double? valor = 0;
                if (!taxa.Taxa.TipoProdutoIDs.Any())
                {
                    switch (taxa.Taxa.Tipo)
                    {
                        case Entities.Taxa.Tipos.Fixo:
                            valor = taxa.Taxa.Valor;
                            break;
                        case Entities.Taxa.Tipos.Percentual:
                            valor = Subtotal * taxa.Taxa.Valor / 100;
                            break;
                    }
                }
                else
                {
                    foreach (var item in Itens)
                    {
                        if (taxa.Taxa.TipoProdutoID.Contains(item.Produto.TipoID))
                        {
                            switch (taxa.Taxa.Tipo)
                            {
                                case Entities.Taxa.Tipos.Fixo:
                                    valor += item.Quantidade * taxa.Taxa.Valor;
                                    break;
                                case Entities.Taxa.Tipos.Percentual:
                                    valor += item.Quantidade * item.Valor.Valor * taxa.Taxa.Valor / 100;
                                    break;
                            }
                        }
                    }
                }

                taxa.Valor = (double)valor;
            }
        }

        public void SetarDadosParcelamento(int parcelas, double jurosTotais)
        {
            JurosTotal = jurosTotais / 100;
            Parcelas = parcelas;
        }
    }
}
