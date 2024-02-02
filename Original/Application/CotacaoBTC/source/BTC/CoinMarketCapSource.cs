using System.Net;

namespace CotacaoBTC.source.BTC
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
            //servicos@tronar.com.br
            //Natural=1
            var result = BuscarCotacao("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=BTC");
            return result.data.BTC.quote.USD.price;
        }
    }
}
