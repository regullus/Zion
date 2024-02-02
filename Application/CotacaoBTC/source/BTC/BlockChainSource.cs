using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CotacaoBTC.source.BTC
{
    public class BlockChainSource : BaseSource, ISource
    {
        public string Name { get { return "BlockChain"; } }

        public BlockChainSource(WebClient client) : base(client)
        {

        }

        public double ObtemCotacao()
        {
            //BlockChain - https://www.blockchain.com/
            var result = BuscarCotacao("https://blockchain.info/ticker");
            return result.USD.last;
        }
    }
}
