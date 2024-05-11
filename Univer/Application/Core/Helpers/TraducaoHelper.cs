using Core.Repositories.Globalizacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Helpers
{
   public class TraducaoHelper
   {

      private static TraducaoRepository _traducaoRepository;
      private static TraducaoRepository traducaoRepository
      {
         get
         {
            if (_traducaoRepository == null)
            {
               _traducaoRepository = new TraducaoRepository(new Entities.YLEVELEntities());
            }
            return _traducaoRepository;
         }
      }

      private Entities.Idioma _idioma;

      public TraducaoHelper(Entities.Idioma idioma)
      {
         _idioma = idioma;
      }

      public bool TemTraducao(string chave)
      {
         return traducaoRepository.GetByIdiomaChave(_idioma.ID, chave) != null;
      }

      public string this[string chave]
      {
         get
         {
            Entities.Traducao traducao;
            string Retorno = "[" + chave + "]";
            try
            {
               traducao = traducaoRepository.GetByIdiomaChave(_idioma.ID, chave);
               Retorno = traducao != null ? traducao.Texto : "[" + chave + "]";
            }
            catch (Exception)
            {
               Retorno = "[" + chave + "]";
            }
            return Retorno;
         }
      }

      public void Traduzir(ref string texto)
      {
         var regex = new Regex(@"\[[A-Za-z0-9_]+\]");
         var matches = regex.Matches(texto);
         for (var i = 0; i < matches.Count; i++)
         {
            var chave = matches[i].Value.Replace("[", "").Replace("]", "");
            if (TemTraducao(chave))
            {
               texto = texto.Replace(matches[i].Value, this[chave]);
            }
         }
      }

   }
}
