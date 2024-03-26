using Core.Repositories.Globalizacao;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Helpers;

namespace Core.Services.Globalizacao
{
    public class GeolocalizacaoService
    {

        private static Dictionary<string, string> cacheIPs;
        private PaisRepository paisRepository;

        public GeolocalizacaoService(DbContext context)
        {
            paisRepository = new PaisRepository(context);
        }

        public Entities.Pais GetByIP()
        {
            Entities.Pais pais = paisRepository.GetBySigla("BR");

            if (Helpers.ConfiguracaoHelper.GetBoolean("DETECTAR_PAIS"))
            {
                var ip = GetIP();
                if (!string.IsNullOrEmpty(ip) && ip.Length >= 7)
                {
                    string cod = GetSigla(ip);
                    if (String.IsNullOrEmpty(cod))
                    {
                        if (ConfiguracaoHelper.GetString("IDIOMA_PADRAO") != null)
                        {
                            cod = ConfiguracaoHelper.GetString("IDIOMA_PADRAO");
                        }
                        else
                        {
                            cod = "en-US";
                        }
                    }
                    pais = paisRepository.GetByCod(cod);
                }
            }

            return pais;
        }

        private string GetIP()
        {
            var request = System.Web.HttpContext.Current.Request;

            string ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(ip))
                ip = request.UserHostAddress;

            if (ip == "::1")
            {
                ip = "127.0.0.1";
            }

            return ip;
        }

        private string GetSigla(string ip)
        {
            string sigla = "";

            if (cacheIPs == null)
            {
                cacheIPs = new Dictionary<string, string>();
            }

            if (!cacheIPs.TryGetValue(ip, out sigla))
            {
                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(Core.Helpers.ConfiguracaoHelper.GetString("URL_GEOLOCALIZACAO") + ip);
                    httpWebRequest.ContentType = "text/json";
                    httpWebRequest.Method = "GET";

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var responseText = streamReader.ReadToEnd();
                        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseText);
                        sigla = json["country_code"].ToString();
                    }

                    cacheIPs.Add(ip, sigla);
                    if (cacheIPs.Count > 1000)
                    {
                        cacheIPs = cacheIPs.Skip(100).ToDictionary(c => c.Key, c => c.Value);
                    }
                }
                catch
                {
                }
            }

            return sigla;
        }

    }
}
