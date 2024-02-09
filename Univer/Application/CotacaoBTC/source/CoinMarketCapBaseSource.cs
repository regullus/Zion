using System.Net;

namespace CotacaoBTC.source
{
    public class CoinMarketCapBaseSource : BaseSource
    {
        public CoinMarketCapBaseSource(WebClient client) : base(client)
        {
            AddHeader("X-CMC_PRO_API_KEY", "52eb8a47-1d60-4d3e-aefb-d5723bc7fefc");
        }
        
    }
}
