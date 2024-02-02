using System;
using Cielo.Request.Element;
using System;
using System.Xml.Serialization;
using System.ComponentModel;


namespace Cielo.Request
{
    [SerializableAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(Namespace = "http://ecommerce.cbmp.com.br")]
    [XmlRootAttribute("requisicao-consulta", Namespace = "http://ecommerce.cbmp.com.br", IsNullable = false)]
    public partial class ConsultationRequest : AbstractElement
    {
        public String tid { get; set; }

        [XmlElementAttribute("dados-ec")]
        public DadosEcElement dadosEc { get; set; }

        public static ConsultationRequest create(String tid, Merchant merchant)
        {
            return new ConsultationRequest
            {
                id = Guid.NewGuid().ToString(),
                versao = Cielo.VERSION,
                tid = tid,
                dadosEc = new DadosEcElement
                {
                    numero = merchant.id,
                    chave = merchant.key
                }
            };
        }
    }
}
