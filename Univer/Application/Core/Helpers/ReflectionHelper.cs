using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Helpers
{
    internal class ReflectionHelper
    {

        public static void Replace(ref string texto, object objeto)
        {
            try
            {
                var regex = new Regex(@"\[[A-Za-z0-9_\.]+\]");
                var matches = regex.Matches(texto);
                if (matches.Count > 0)
                {
                    var data = GetData(objeto);
                    for (var i = 0; i < matches.Count; i++)
                    {
                        var chave = matches[i].Value.Replace("[", "").Replace("]", "");
                        if (data.Any(d => d.Key == chave))
                        {
                            texto = texto.Replace(matches[i].Value, data.FirstOrDefault(d => d.Key == chave).Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERRO Em REFLECTIONHELPER.Replace: " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""));
            }
        }

        private static List<KeyValuePair<string, string>> GetData(object objeto)
        {
            //TODO: Recuperar informações em profundidade
            var data = new List<KeyValuePair<string, string>>();
            try
            {
                var path = objeto.GetType().BaseType.Name + ".";
                foreach (var property in objeto.GetType().GetProperties())
                {
                    switch (property.MemberType)
                    {
                        case System.Reflection.MemberTypes.Property:
                            if (property.PropertyType == typeof(string))
                            {
                                if (property.Name != null && property.GetValue(objeto) != null)
                                {
                                    data.Add(new KeyValuePair<string, string>(path + property.Name, property.GetValue(objeto).ToString()));
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //TODO gravar erro
            }
            return data;
        }

    }
}
