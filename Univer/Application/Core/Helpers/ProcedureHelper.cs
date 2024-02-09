using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class ProcedureHelper
    {
        public static string ConverterDataInicio(string value)
        {

            //17/04/2022 -> 2022-04-17
            string data = ConverterDataBanco(value) + " 00:00:00";
            //var data = Convert.ToDateTime(value);
            //string.Format("{0}-{1}-{2} 00:00:00", data.Year, data.Month, data.Day);
            return data;
        }

        public static string ConverterDataFim(string value)
        {
            //17/04/2022 -> 2022-04-17
            string data = ConverterDataBanco(value) + " 23:59:59";
            //var data = Convert.ToDateTime(value);
            //return string.Format("{0}-{1}-{2} 23:59:59", data.Year, data.Month, data.Day);
            return data;
            
        }

        /// <summary>
        /// Converte Data 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Data no formato dd/mm/yyyy ou yyyy-mm-dd</returns>
        /// <remarks></remarks>
        public static string ConverterDataBanco(string strData)
        {
            string strRetorno = "";
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    if (strData.Trim().Length > 0)
                    {
                        if (strData.IndexOf("/") > 0)
                        {
                            //17/04/2022 => 2022-04-17
                            strRetorno = strData.Substring(6, 4) + "-" + strData.Substring(3, 2) + "-" + strData.Substring(0, 2);
                        }
                        else if (strData.IndexOf("-") > 0)
                        {
                            //2022-04-17 => 17/04/2022
                            strRetorno = strData.Substring(8, 2) + "/" + strData.Substring(5, 2) + "/" + strData.Substring(0, 4);
                        }
                        else
                        {
                            //20220417 => 17/04/2022
                            strRetorno = strData.Substring(6, 2) + "/" + strData.Substring(4, 2) + "/" + strData.Substring(0, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("[Gerais.ConverteData]" + ex.Message, ex);
                strRetorno = "";
            }

            return strRetorno;
        }


    }
}
