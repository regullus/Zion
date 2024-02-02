using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
	[SerializableAttribute ()]
	[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
	public partial class DadosPortadorElement :AbstractElement
	{
		public String numero { get; set; }

		public String validade { get; set; }

		public String indicador { get; set; }

		[XmlElementAttribute ("codigo-seguranca")]
		public String codigoSeguranca { get; set; }

		[XmlElementAttribute ("nome-portador")]
		public String nomePortador { get; set; }

		public String token { get; set; }

		public Holder ToHolder ()
		{
			Holder holder = new Holder (token);

			holder.number = numero;
			holder.expiration = validade;
			holder.cvv = codigoSeguranca;
			holder.name = nomePortador;

			return holder;
		}
	}
}