using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
   public class ArquivoRepository : CachedRepository<Entities.Arquivo>
   {

      public ArquivoRepository(DbContext context)
          : base(context)
      {
      }

      public IEnumerable<Entities.Arquivo> GetBySecao(int secaoID)
      {
         var arquivo = base.GetByExpression(a => a.SecaoID == secaoID && a.Ativo == true);

         return arquivo;
      }

      public static IEnumerable<string> BuscarArquivos(string caminhoFisico, string diretorio, string termo)
      {
         var resultado = new List<string>();
         if (Directory.Exists(caminhoFisico + diretorio))
         {
            var arquivos = Directory.EnumerateFiles(caminhoFisico + diretorio, termo);
            foreach (var arquivo in arquivos)
            {
               var info = new FileInfo(arquivo);
               resultado.Add(String.Format("{0}/{1}", diretorio, info.Name));
            }
         }
         return resultado;
      }

        public static IEnumerable<string> BuscarArquivos(string caminhoFisico, string caminhoVirtual, string diretorio, string termo)
        {
            var resultado = new List<string>();
            
            if (Directory.Exists(caminhoFisico + diretorio))
            {
                var arquivos = Directory.EnumerateFiles(caminhoFisico + diretorio, termo);
                foreach (var arquivo in arquivos)
                {
                    var info = new FileInfo(arquivo);

                    var path = Path.Combine(caminhoVirtual, diretorio, info.Name);
                    path = path.Replace("\\", "/");

                    resultado.Add(path);
                }
            }
            return resultado;
        }
    }
}

