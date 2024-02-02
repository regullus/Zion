using System.Net;

namespace CotacaoBTC.source.BTC
{
    public class CryptoCompareSource : CryptoCompareBaseSource, ISource
    {
        public CryptoCompareSource(WebClient client) : base(client)
        {
        }

        public string Name { get { return "CryptoCompare"; } }

        public double ObtemCotacao()
        {
            // https://min-api.cryptocompare.com/documentation
            var result = BuscarCotacao("https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD");
            return result.USD;
        }
    }
}
