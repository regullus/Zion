using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
	[SerializableAttribute ()]
	[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
	public partial class DadosPedidoElement :AbstractElement
	{
		public String numero { get; set; }

		public int valor { get; set; }

		public int moeda { get; set; }

		[XmlElementAttribute ("data-hora")]
		public String dataHora { get; set; }

		[XmlElementAttribute ("descricao", IsNullable = false)]
		public String descricao { get; set; }

		[XmlElementAttribute ("idioma", IsNullable = false)]
		public String idioma { get; set; }

		[XmlElementAttribute ("taxa-embarque", IsNullable = false)]
		public int taxaEmbarque { get; set; }

		[XmlElementAttribute ("soft-descriptor")]
		public String softDescriptor { get; set; }

		public Order ToOrder ()
		{
			Order order = new Order (numero, valor, dataHora, moeda);

			order.description = descricao;
			order.language = idioma;
			order.shipping = taxaEmbarque;
			order.softDescriptor = softDescriptor;

			return order;
		}
	}
}