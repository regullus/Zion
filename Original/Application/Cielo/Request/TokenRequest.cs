using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Cielo.Request.Element;

namespace Cielo.Request
{
	[SerializableAttribute ()]
	[DesignerCategoryAttribute ("code")]
	[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
	[XmlRootAttribute ("requisicao-token", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
	public partial class TokenRequest :AbstractElement
	{
		[XmlElementAttribute ("dados-ec")]
		public DadosEcElement dadosEc { get; set; }

		[XmlElementAttribute ("dados-portador")]
		public DadosPortadorElement dadosPortador { get; set; }

		public static TokenRequest create (Transaction transaction)
		{
			var tokenRequest = new TokenRequest {
                id = Guid.NewGuid().ToString(),
				versao = Cielo.VERSION,
				dadosEc = new DadosEcElement {
					numero = transaction.merchant.id,
					chave = transaction.merchant.key
				},
				dadosPortador = new DadosPortadorElement {
					numero = transaction.holder.number,
					validade = transaction.holder.expiration,
					nomePortador = transaction.holder.name
				}
			};

			return tokenRequest;
		}

        public static TokenRequest create(Merchant merchant, Holder holder)
        {
            var tokenRequest = new TokenRequest
            {
                id = Guid.NewGuid().ToString(),
                versao = Cielo.VERSION,
                dadosEc = new DadosEcElement
                {
                    numero = merchant.id,
                    chave = merchant.key
                },
                dadosPortador = new DadosPortadorElement
                {
                    numero = holder.number,
                    validade = holder.expiration,
                    nomePortador = holder.name
                }
            };

            return tokenRequest;
        }
	}
}

