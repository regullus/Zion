using System.Net;

namespace CotacaoBTC.source.BTC
{
    public class CoinDeskSource : BaseSource, ISource
    {
        public string Name { get { return "CoinDesk"; } }

        public CoinDeskSource(WebClient client) : base(client)
        {

        }

        public double ObtemCotacao()
        {
            //CoinDesk - https://www.coindesk.com/
            var result = BuscarCotacao("https://api.coindesk.com/v1/bpi/currentprice.json");
            return result.bpi.USD.rate;
        }
    }
}
