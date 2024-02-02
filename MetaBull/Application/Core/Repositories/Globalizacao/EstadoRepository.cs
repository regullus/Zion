using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
   public class EstadoRepository : CachedRepository<Entities.Estado>
   {

      public EstadoRepository(DbContext context)
          : base(context)
      {
      }

      public IEnumerable<Entities.Estado> GetByPais(int paisID)
      {
         return cachedRepository.Where(e => e.PaisID == paisID);
      }

      public Entities.Estado GetByNome(string nome)
      {
         nome = nome.ToLower();
         return base.GetByExpression(e => e.Nome.ToLower() == nome).FirstOrDefault();
      }

      /// <summary>
      /// Obtem id dado o nome do estado
      /// </summary>
      /// <param name="nome">nome do estado</param>
      /// <returns>id</returns>
      public int GetID(string nome)
      {
         nome = nome.ToLower();
         var estado = cachedRepository.FirstOrDefault(e => e.Nome.ToLower() == nome);
         int EstadoID = 0;

         if (estado != null)
         {
            EstadoID = estado.ID;
         }

         return EstadoID;
      }

   }
}
