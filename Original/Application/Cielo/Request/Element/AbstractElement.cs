using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Cielo.Request.Element
{
	public class AbstractElement
	{
		[XmlAttributeAttribute ()]
		public String id { get; set; }

		[XmlAttributeAttribute ()]
		public String versao { get; set; }

		protected T unserializeElement<T> (T element, String response)
		{
			XmlSerializer serializer = new XmlSerializer (typeof(T));

			try {
				using (TextReader reader = new StringReader (response)) {
					element = (T)serializer.Deserialize (reader);
				}
			} catch (System.InvalidOperationException e) {
				using (TextReader reader = new StringReader (response)) {
					serializer = new XmlSerializer (typeof(ErroElement));

					ErroElement erro = (ErroElement)serializer.Deserialize (reader);

					throw new CieloException(erro.mensagem, erro.codigo, e);
                }
			}

			return element;
		}
	}
}

