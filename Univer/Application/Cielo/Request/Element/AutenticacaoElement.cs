using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
	[SerializableAttribute ()]
	[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
	public partial class AutenticacaoElement :AbstractElement
	{
		public int codigo { get; set; }

		public String mensagem { get; set; }

		[XmlElementAttribute ("data-hora")]
		public String dataHora { get; set; }

		public int valor { get; set; }

		public int eci { get; set; }

		public Authentication ToAuthentication ()
		{
			Authentication authentication = new Authentication ();
			authentication.code = codigo;
			authentication.message = mensagem;
			authentication.dateTime = dataHora;
			authentication.total = valor;
			authentication.eci = eci;

			return authentication;
		}
	}
}