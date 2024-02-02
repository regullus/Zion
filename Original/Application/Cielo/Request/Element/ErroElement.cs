using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
	[SerializableAttribute ()]
	[XmlRootAttribute ("erro", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
	public partial class ErroElement :AbstractElement
	{
		public String codigo { get; set; }

		public String mensagem { get; set; }
		
	}
}

