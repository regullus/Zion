using System.Net;

namespace CotacaoBTC.source
{
    public class CryptoCompareBaseSource : BaseSource 
    {
        public CryptoCompareBaseSource(WebClient client) : base(client)
        {
            AddHeader("Authorization", "Apikey cc4cf47338b2f8ea97dc7960108768e25b1da837cac64854a0046c87de65835a");
        }

    }
}
