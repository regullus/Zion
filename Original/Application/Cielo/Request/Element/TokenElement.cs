using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Cielo.Request.Element
{
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ecommerce.cbmp.com.br")]
    public class TokenElement : AbstractElement
    {
        [XmlTypeAttribute(Namespace = "http://ecommerce.cbmp.com.br")]
        public partial class DadosTokenElement
        {
            [XmlElementAttribute("codigo-token")]
            public String codigoToken { get; set; }

            public int status { get; set; }

            [XmlElementAttribute("numero-cartao-truncado")]
            public String numeroTruncado { get; set; }
        }

        [XmlElementAttribute("dados-token")]
        public DadosTokenElement dadosToken { get; set; }

        public Token ToToken()
        {
            Token token = new Token();
            token.code = dadosToken.codigoToken;
            token.status = dadosToken.status;
            token.number = dadosToken.numeroTruncado;

            return token;
        }
    }
}
