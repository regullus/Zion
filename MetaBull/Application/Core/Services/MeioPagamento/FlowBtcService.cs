using Core.Helpers;
using Core.Repositories.Financeiro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Core.Services.MeioPagamento
{
    public class FlowBtcService
    {
        private SaqueRepository saqueRepository;
        private SaqueStatusRepository saqueStatusRepository;

        private string apiKey;
        private string user_id;
        private string privateKey;
        private string flowBTCUrl;

        public FlowBtcService(DbContext context)
        {
            this.saqueRepository = new SaqueRepository(context);
            this.saqueStatusRepository = new SaqueStatusRepository(context);
            
            //apikey = Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_KEY");
            this.apiKey = Core.Helpers.ConfiguracaoHelper.GetString("FLOWBTC_APIKEY"); 
            this.user_id = Core.Helpers.ConfiguracaoHelper.GetString("FLOWBTC_LOGIN"); 
            this.privateKey = Core.Helpers.ConfiguracaoHelper.GetString("FLOWBTC_PRIVATEKEY");
            this.flowBTCUrl = "https://api.flowbtc.com:8400";
        }

        public FlowBtcResponseWithdrawResponse Pagar(float valor, string destino)
        {
            System.Threading.Thread.Sleep(3000);

            var serializer = new JavaScriptSerializer();
            var apiNonce = App.DateTimeZion.Ticks;

            var withdrawCoinsRequest = new
            {
                apiKey = this.apiKey,
                apiNonce = apiNonce,
                apiSig = this.Assinatura(apiNonce + this.user_id + this.apiKey),
                ins = "BTCUSD",
                product = "BTC",
                amount = valor.ToString(new CultureInfo("en")),
                sendToAddress = destino,
            };

            var json = serializer.Serialize(withdrawCoinsRequest);
            var url = this.flowBTCUrl + "/ajax/v1/Withdraw";
            var request = HttpWebRequest.Create(url);
            var response = Post(json, request);

            return JsonConvert.DeserializeObject<FlowBtcResponseWithdrawResponse>(response);
        }

        public FlowBtcBalanceResponse Saldo()
        {
            var serializer = new JavaScriptSerializer();
            var apiNonce = App.DateTimeZion.Ticks;

            var withdrawCoinsRequest = new
            {
                apiKey = this.apiKey,
                apiNonce = apiNonce,
                apiSig = this.Assinatura(apiNonce + this.user_id + this.apiKey)
            };

            var json = serializer.Serialize(withdrawCoinsRequest);
            var url = this.flowBTCUrl + "/ajax/v1/GetAccountInfo";
            var request = HttpWebRequest.Create(url);
            var response = Post(json, request);

            return JsonConvert.DeserializeObject<FlowBtcBalanceResponse>(response);
        }

        private string Post(string json, WebRequest request)
        {
            request.Method = "POST";
            request.ContentType = "application/json";

            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();

            dataStream = response.GetResponseStream();

            using (var reader = new StreamReader(dataStream))
            {
                return reader.ReadToEnd();
            }
        }

        private string Assinatura(string assinatura)
        {
            var keyByte = Encoding.ASCII.GetBytes(this.privateKey);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var bytes = Encoding.ASCII.GetBytes(assinatura);
                var hash = hmacsha256.ComputeHash(bytes);

                var sbinary = "";

                for (int i = 0; i < hash.Length; i++)
                    sbinary += hash[i].ToString("X2");
                return sbinary.ToUpper();
            }
        }
    }

    public class FlowBtcResponseWithdrawResponse
    {
        public bool isAccepted { get; set; }
        public string rejectReason { get; set; }
    }


    public class FlowBtcBalanceResponse
    {
        public Currency[] currencies { get; set; }
        public Productpair[] productPairs { get; set; }
        public bool isAccepted { get; set; }
    }

    public class Currency
    {
        public string name { get; set; }
        public float balance { get; set; }
        public float hold { get; set; }
    }

    public class Productpair
    {
        public string productPairName { get; set; }
        public int productPairCode { get; set; }
        public int tradeCount { get; set; }
        public int tradeVolume { get; set; }
    }

}
