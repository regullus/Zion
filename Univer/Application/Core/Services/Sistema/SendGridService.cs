using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace Core.Services.Sistema
{
    public class SendGridService
   {
      public SendGridService()
        {
       
        }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="strApiKey"></param>
      /// <param name="strTitulo"></param>
      /// <param name="strMensagem"></param>
      /// <param name="strEmailOrigem"></param>
      /// <param name="strEmailDestino"></param>
      /// <returns></returns>
      public static async Task<Response> SendGridEnviaSync(string strApiKey, string strTitulo, string strMensagem, string strEmailOrigem, string strEmailDestino, List<KeyValuePair<string, string>> campos = null)
      {
         Response response = null;
         try
         {
            if (campos != null)
            {
               foreach (var campo in campos)
               {
                  strTitulo = strTitulo.Replace(String.Format("[{0}]", campo.Key), campo.Value);
                  strMensagem = strMensagem.Replace(String.Format("[{0}]", campo.Key), campo.Value);
               }
            }

            var sgMsg = new SendGridMessage();
            sgMsg.Subject = strTitulo;
            sgMsg.From = new EmailAddress(strEmailOrigem);
            sgMsg.AddTo(strEmailDestino);
            sgMsg.HtmlContent = strMensagem;

            var client = new SendGridClient(strApiKey);
            response = await client.SendEmailAsync(sgMsg);
         }
         catch (Exception)
         {

         }

         return response;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strApiKey"></param>
      /// <param name="strTitulo"></param>
      /// <param name="strMensagem"></param>
      /// <param name="strEmailOrigem"></param>
      /// <param name="strEmailDestino"></param>
      public static void SendGridEnviaAsync(string strApiKey, string strTitulo, string strMensagem, string strEmailOrigem, string strEmailDestino, List<KeyValuePair<string, string>> campos = null)
      {
         try
         {
            if (campos != null)
            {
               foreach (var campo in campos)
               {
                  strTitulo   = strTitulo.Replace(String.Format("[{0}]", campo.Key), campo.Value);
                  strMensagem = strMensagem.Replace(String.Format("[{0}]", campo.Key), campo.Value);
               }
            }

            var sgMsg = new SendGridMessage();
            sgMsg.Subject = strTitulo;
            sgMsg.From = new EmailAddress(strEmailOrigem);
            sgMsg.AddTo(strEmailDestino);
            sgMsg.HtmlContent = strMensagem;

            var client = new SendGridClient(strApiKey);
            client.SendEmailAsync(sgMsg);
         }
         catch (Exception )
         {

         }
      }
   }
}
