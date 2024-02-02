using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Cielo.Request.Element;

namespace Cielo.Request
{
	[SerializableAttribute ()]
	[XmlRootAttribute ("requisicao-transacao", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
	public partial class TransactionRequest :AbstractElement
	{
		[XmlElementAttribute ("dados-ec")]
		public DadosEcElement dadosEc { get; set; }

		[XmlElementAttribute ("dados-portador")]
		public DadosPortadorElement dadosPortador { get; set; }

		[XmlElementAttribute ("dados-pedido")]
		public DadosPedidoElement dadosPedido { get; set; }

		[XmlElementAttribute ("forma-pagamento")]
		public FormaPagamentoElement formaPagamento { get; set; }

		[XmlElementAttribute ("url-retorno")]
		public String urlRetorno { get; set; }

		public int autorizar { get; set; }

		public String capturar { get; set; }

		[XmlElementAttribute ("campo-livre")]
		public String campoLivre { get; set; }

		public String bin { get; set; }

		[XmlElementAttribute ("gerar-token")]
		public String gerarToken { get; set; }

        public static TransactionRequest create (Transaction transaction)
        {
            var transactionRequest = new TransactionRequest {
                id = Guid.NewGuid().ToString(),
                versao = Cielo.VERSION,
                dadosEc = new DadosEcElement {
                    numero = transaction.merchant.id,
                    chave = transaction.merchant.key
                },
                dadosPortador = string.IsNullOrEmpty(transaction.holder.token) ?
                    new DadosPortadorElement {
                        numero = transaction.holder.number,
                        validade = transaction.holder.expiration,
                        nomePortador = transaction.holder.name,
                        codigoSeguranca = transaction.holder.cvv
                    } 
                    : new DadosPortadorElement {
                        token = transaction.holder.token
                    },
                dadosPedido = new DadosPedidoElement {
                    numero = transaction.order.number,
                    valor = transaction.order.total,
                    moeda = transaction.order.currency,
                    dataHora = transaction.order.dateTime,
                    descricao = transaction.order.description,
                    idioma = transaction.order.language,
                    taxaEmbarque = transaction.order.shipping,
                    softDescriptor = transaction.order.softDescriptor
                },
                formaPagamento = new FormaPagamentoElement {
                    bandeira = transaction.paymentMethod.issuer,
                    produto = transaction.paymentMethod.product,
                    parcelas = transaction.paymentMethod.installments
                },
                urlRetorno = transaction.returnURL,
                autorizar = (int)transaction.authorize,
                capturar = transaction.capture ? "true" : "false"
            };

            if (transaction.freeField != null) {
                transactionRequest.campoLivre = transaction.freeField;
            }

            if (transaction.bin != null) {
                transactionRequest.bin = transaction.bin;
            }

            if (transaction.generateToken) {
                transactionRequest.gerarToken = "true";
            }

            return transactionRequest;
        }
	}
}

