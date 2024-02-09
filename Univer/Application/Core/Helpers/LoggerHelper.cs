using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class LoggerHelper
    {

        private static string _logFolder = "d:/logs/";

        public static void WriteFile(string content, string fileName)
        {
            try
            {
                content = App.DateTimeZion.ToString("dd/MM/yyyy hh:mm:ss") + "-" + content;
                fileName = App.DateTimeZion.ToString("yyyyMMdd") + "_" + fileName;
                string path = Core.Helpers.ConfiguracaoHelper.TemChave("DIRETORIO_LOG") ? Core.Helpers.ConfiguracaoHelper.GetString("DIRETORIO_LOG") : System.Web.HttpContext.Current.Server.MapPath(_logFolder);
                string fullPath = path + fileName + ".txt";
                File.AppendAllLines(fullPath, new string[1] { content });
            }
            catch { }
        }

        public static string GetFileContent(string fileName)
        {
            try
            {
                string path = Core.Helpers.ConfiguracaoHelper.TemChave("DIRETORIO_LOG") ? Core.Helpers.ConfiguracaoHelper.GetString("DIRETORIO_LOG") : System.Web.HttpContext.Current.Server.MapPath(_logFolder);
                string fullPath = path + fileName + ".txt";

                string ret = "";
                if (File.Exists(fullPath))
                {
                    ret = File.ReadAllText(fullPath);
                }
                return ret;
            }
            catch
            {
                return "";
            }
        }


    }
}
