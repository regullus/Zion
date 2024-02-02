using System.Net;

namespace CotacaoBTC.source.LTC
{
    public class CoinMarketCapSource : CoinMarketCapBaseSource, ISource
    {
        public CoinMarketCapSource(WebClient client) : base(client)
        {
        }

        public string Name { get { return "CoinMarketCap"; } }

        public double ObtemCotacao()
        {

            //https://coinmarketcap.com/
            var result = BuscarCotacao("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=LTC");
            return result.data.LTC.quote.USD.price;
        }
    }
}
