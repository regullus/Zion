using Core.Helpers;
using Core.Repositories.Usuario;
using Core.Services.Sistema;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;

namespace Core.Services.Usuario
{

    public class CurlService
    {
        /**************************
        * Merchant configuration  -  SANDBOX MODE
        **************************/
        //private string x_login = "3nsHqiaUTA";
        //private string x_trans_key = "xtp4UjgiNt";

        //private string x_login_for_webpaystatus = "HXloE9NNW2";
        //private string x_trans_key_for_webpaystatus = "zlO62Y7JZs";

        //private string secret_key = "Pu3ejHlQaZlQX3uzaHvc2OFMCb3VqjsiU";

        //private bool sandbox = true;
        /*********************************
         * End of Merchant configuration *
         *********************************/
        /**************************
        * Merchant configuration  -  LIVE
        **************************/
        private string x_login = "jwFXSzwfHW";
        private string x_trans_key = "yAZzxFwJ5C";

        private string x_login_for_webpaystatus = "h4HC78TpjD";
        private string x_trans_key_for_webpaystatus = "zegQgf170A";

        private string secret_key = "oTm8eRgjUKbP4fVWuIZ2GJ92tbMXShztw";
        
        private bool sandbox = false;
        /*********************************
         * End of Merchant configuration *
         *********************************/



        /*****************************************************
         * ---- PLEASE DON'T CHANGE ANYTHING BELOW HERE ---- *
         *****************************************************/
        string[] url = new string[4] { "", "", "", "" };
        /*
		url[0]:create
		url[1]:status
		url[2]:exchange
		url[3]:banks
        */

        private int errors = 0;

        const string formatter = "{0,10}{1,16}";

        private UsuarioRepository usuarioRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;
        private UsuarioStatusRepository usuarioStatusRepository;

        public CurlService(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            usuarioStatusRepository = new UsuarioStatusRepository(context);
        }

        /*
        public string GetStatus(string invoice)
        {
            Hashtable post_values = new Hashtable();

            post_values.Add("x_login", this.x_login_for_webpaystatus);
            post_values.Add("x_trans_key", this.x_trans_key_for_webpaystatus);
            post_values.Add("x_invoice", invoice);

            string response = this.curl(this.url[0], post_values);
            return response;
        }
        */

        public string curl(string url, Hashtable post_values)
        {
            String post_string = "";
            String post_response = "";
            HttpWebRequest objRequest;
            foreach (DictionaryEntry field in post_values)
            {
                post_string += field.Key + "=" + field.Value + "&";
            }
            post_string = post_string.TrimEnd('&');

            try
            {
                // create an HttpWebRequest object to communicate with AstroPay transaction server
                objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = post_string.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded, charset=utf-8";

                // post data is sent as a stream
                StreamWriter myWriter = null;
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(post_string);
                myWriter.Close();

                // returned values are returned as a stream, then read into a string
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }

                // the response string is broken into an array
                // The split character specified here must match the delimiting character specified above
            }
            catch (Exception e)
            {
                throw new Exception("Error ocurred in HttpWebRequest: " + e.Message);
            }
            return post_response;
        }


        public string curl(string url, string post_values)
        {
            String post_string = "";
            String post_response = "";
            HttpWebRequest objRequest;

            post_string = post_values.TrimEnd('&');

            try
            {
                // create an HttpWebRequest object to communicate with AstroPay transaction server
                objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = post_string.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded, charset=utf-8";

                // post data is sent as a stream
                StreamWriter myWriter = null;
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(post_string);
                myWriter.Close();

                // returned values are returned as a stream, then read into a string
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }

                // the response string is broken into an array
                // The split character specified here must match the delimiting character specified above
            }
            catch (Exception e)
            {
                throw new Exception("Error ocurred in HttpWebRequest: " + e.Message);
            }
            return post_response;
        }

        public string GetSign(string key, string message)
        {
            System.Security.Cryptography.HMAC hmac = System.Security.Cryptography.HMAC.Create("HMACSHA256");
            hmac.Key = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

    }
}
