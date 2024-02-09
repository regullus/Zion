using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Cielo.Request.Element;

namespace Cielo.Request
{
	[SerializableAttribute ()]
	[DesignerCategoryAttribute ("code")]
	[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
	[XmlRootAttribute ("requisicao-autorizacao-tid", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
	public partial class AuthorizationRequest :AbstractElement
	{
		[XmlElementAttribute ("tid")]
		public String tid { get; set; }

		[XmlElementAttribute ("dados-ec")]
		public DadosEcElement dadosEc { get; set; }

		public static AuthorizationRequest create (Transaction transaction)
		{
			var authorizationRequest = new AuthorizationRequest {
				id = Guid.NewGuid().ToString(),
				versao = Cielo.VERSION,
				tid = transaction.tid,
				dadosEc = new DadosEcElement {
					numero = transaction.merchant.id,
					chave = transaction.merchant.key
				}
			};

			return authorizationRequest;
		}
	}
}

