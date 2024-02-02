using System.Net;

namespace CotacaoBTC.source.BTC
{
    public class BitStampSource : BaseSource, ISource
    {
        public BitStampSource(WebClient client) : base(client)
        {

        }

        public string Name { get { return "BitStamp"; } }

        public double ObtemCotacao()
        {

            //BitCoinAverage - https://www.bitstamp.net/api/
            var result = BuscarCotacao("https://www.bitstamp.net/api/v2/ticker/btcusd/");
            return result.last;
        }
    }
}
