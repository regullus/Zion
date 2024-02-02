using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CotacaoBTC.source
{
    public abstract class BaseSource
    {
        protected WebClient client;

        protected BaseSource(WebClient client)
        {
            this.client = client;

            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            this.client.UseDefaultCredentials = true;
        }

        protected void AddHeader(string key, string value)
        {
            client.Headers.Add(key, value);
        }

        protected dynamic BuscarCotacao(string url)
        {
            var data = client.DownloadString(url);
            return JsonConvert.DeserializeObject<dynamic>(data);
        }
    }
}
