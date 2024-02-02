using Core.Entities;
using CotacaoBTC.source.BTC;
using System.Collections.Generic;
using System.Net;

namespace CotacaoBTC.source
{
    public class SourceFactory
    {
        public static List<ISource> Sources(Moeda.Moedas moeda, WebClient client)
        {
            var sources = new List<ISource>();

            if (moeda == Moeda.Moedas.BTC)
            {
                // nao consegue rodar 2 consultas proximas, no caso a de LTC abaixo
                //sources.Add(new BTC.CoinMarketCapSource(client));

                sources.Add(new BTC.BlockChainSource(client));
                sources.Add(new BTC.BitStampSource(client));
                sources.Add(new BTC.CryptoCompareSource(client));
                sources.Add(new BTC.CoinDeskSource(client));
            }

            else if (moeda == Moeda.Moedas.LTC)
            {
                sources.Add(new LTC.CryptoCompareSource(client));
                sources.Add(new LTC.BitStampSource(client));
                sources.Add(new LTC.CoinMarketCapSource(client));
            }

            return sources;
        }
    }
}
