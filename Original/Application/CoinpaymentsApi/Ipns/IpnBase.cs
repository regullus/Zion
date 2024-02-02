using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api.Ipns
{
    [DataContract]
    public abstract class IpnBase
    {
        [DataMember(Name = "ipn_version")]
        public string IpnVersion { get; set; }
        [DataMember(Name = "ipn_type")]
        public string IpnType { get; set; }
        [DataMember(Name = "ipn_mode")]
        public string IpnMode { get; set; }
        [DataMember(Name = "ipn_id")]
        public string IpnId { get; set; }
        [DataMember(Name = "merchant")]
        public string Merchant { get; set; }

        [IgnoreDataMember]
        public string Source { get; set; }

        public bool SigIsValid(string hmacSent)
        {
            bool valid = true;
            var calcHmac = CryptoUtil.CalcSignature(Source, CoinpaymentsSettings.Default.IpnSecret);
            if (hmacSent != calcHmac)
            {
                WriteFile("hmacSent: " + hmacSent, "IpnBase");
                WriteFile("calcHmac: " + calcHmac, "IpnBase");
                valid = false;
            }
            else if (Merchant != CoinpaymentsSettings.Default.MerchantId)
            {
                WriteFile("Merchant: " + Merchant, "IpnBase");
                WriteFile("CoinpaymentsSettings.Default.MerchantId: " + CoinpaymentsSettings.Default.MerchantId, "IpnBase");
                valid = false;
            }
            return valid;
        }

        private static string _logFolder = "d:/logs/";

        public static void WriteFile(string content, string fileName)
        {
            try
            {
                content = DateTime.Now.ToString("hh:mm:ss") + " " + content;
                fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + fileName + ".txt";
                string path = _logFolder;
                string fullPath = path + fileName;
                File.AppendAllLines(fullPath, new string[1] { content });
            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                erro = "";
            }

        }

        public static T Parse<T>(NameValueCollection form)
            where T : IpnBase
        {
            // auto_mapping to title case doesn't work easily
            // so we'll use json as intermediary
            var dict = new Dictionary<string, string>();
            foreach (var key in form.AllKeys)
                dict.Add(key, form[key]);

            var str = JsonSerializer.SerializeToString(dict);
            var req = JsonSerializer.DeserializeFromString<T>(str);

            //
            req.Source = CryptoUtil.DictionaryToFormData(dict);

            return req;
        }
    }
}
