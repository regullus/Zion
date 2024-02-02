using Core.Helpers;
using cpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Sistema
{
    public class EmailService
    {
        private SmtpClient smtp;

        public EmailService()
        {
            Encryption objEncryption = new Encryption();
            smtp = new SmtpClient(Helpers.ConfiguracaoHelper.GetString("SMTP_HOST"), Helpers.ConfiguracaoHelper.GetInt("SMTP_PORT"));
            smtp.EnableSsl = Helpers.ConfiguracaoHelper.GetBoolean("SMTP_ENABLE_SSL");
            smtp.UseDefaultCredentials = Helpers.ConfiguracaoHelper.GetBoolean("SMTP_USE_DEFAULT_CREDENTIALS");
            string strPassword = Helpers.ConfiguracaoHelper.GetString("SMTP_PASSWORD");

            strPassword = objEncryption.Morpho(strPassword, "Ava2014", TipoCriptografia.Descriptografa);
            if (!smtp.UseDefaultCredentials)
            {
                smtp.Credentials = new NetworkCredential(Helpers.ConfiguracaoHelper.GetString("SMTP_USERNAME"), strPassword);
            }
        }

        public void Send(string from, string to, string subject, string body, bool html = true, List<KeyValuePair<string, string>> fields = null)
        {
            ConfiguracaoHelper.Replace(ref body);
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    subject = subject.Replace(String.Format("[{0}]", field.Key), field.Value);
                    body = body.Replace(String.Format("[{0}]", field.Key), field.Value);
                }
            }

            var bcc = "";
            if (Helpers.ConfiguracaoHelper.TemChave("EMAIL_CC"))
            {
                bcc = Helpers.ConfiguracaoHelper.GetString("EMAIL_CC");
            }

            var mail = new MailMessage();
            mail.From = new MailAddress(from);
            mail.IsBodyHtml = html;
            mail.Subject = subject;
            mail.Body = body;
            mail.To.Add(to);

            if (!string.IsNullOrWhiteSpace(bcc))
            {
                mail.Bcc.Add(bcc);
            }

            ServicePointManager.ServerCertificateValidationCallback =
                delegate(object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                         System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                { return true; };
            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            { 
                
                string erro = ex.Message;
                //Todo tratar erro
            }
          
        }

    }
}
