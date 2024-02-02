using System.Net;

namespace CotacaoBTC.source.LTC
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
            var result = BuscarCotacao("https://min-api.cryptocompare.com/data/price?fsym=LTC&tsyms=USD");
            return result.USD;
        }
    }
}
