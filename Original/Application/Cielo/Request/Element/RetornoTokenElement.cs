using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
	[SerializableAttribute ()]
	[XmlRootAttribute ("retorno-token", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
	public class RetornoTokenElement :AbstractElement
	{
		[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
		public partial class DadosTokenElement
		{
			[XmlElementAttribute ("codigo-token")]
			public String codigoToken { get; set; }

			public int status { get; set; }

			[XmlElementAttribute ("numero-cartao-truncado")]
			public String numeroTruncado { get; set; }
		}

		[XmlTypeAttribute (Namespace = "http://ecommerce.cbmp.com.br")]
		public partial class TokenElement
		{
			[XmlElementAttribute ("dados-token")]
			public DadosTokenElement dadosToken { get; set; }
		}

		public TokenElement token { get; set; }

		public static Token unserialize (Transaction transaction, String response)
		{
			RetornoTokenElement tokenElement = new RetornoTokenElement ();
			tokenElement = tokenElement.unserializeElement (tokenElement, response);

			Token token = new Token ();

			token.code = tokenElement.token.dadosToken.codigoToken;
			token.status = tokenElement.token.dadosToken.status;
			token.number = tokenElement.token.dadosToken.numeroTruncado;

			return token;
		}

        public static Token unserialize(String response)
        {
            RetornoTokenElement tokenElement = new RetornoTokenElement();
            tokenElement = tokenElement.unserializeElement(tokenElement, response);

            Token token = new Token();

            token.code = tokenElement.token.dadosToken.codigoToken;
            token.status = tokenElement.token.dadosToken.status;
            token.number = tokenElement.token.dadosToken.numeroTruncado;

            return token;
        }
	}
}