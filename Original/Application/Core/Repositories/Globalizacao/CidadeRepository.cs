using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
   public class CidadeRepository : CachedRepository<Entities.Cidade>
   {

      public CidadeRepository(DbContext context)
          : base(context)
      {
      }

      public IEnumerable<Entities.Cidade> GetByEstado(int estadoID)
      {
         return cachedRepository.Where(e => e.EstadoID == estadoID);
      }

      public IEnumerable<Entities.Cidade> GetByEstado(string uf)
      {
         return cachedRepository.Where(e => e.Estado.Sigla == uf);
      }

      public Entities.Cidade GetByNome(string nome)
      {
         nome = nome.ToLower();
         return base.GetByExpression(e => e.Nome.ToLower() == nome).FirstOrDefault();
      }

      /// <summary>
      /// Obtem id dado o nome da cidade
      /// </summary>
      /// <param name="nome">nome da cidade</param>
      /// <returns>id</returns>
      public int GetID(string nome)
      {
         nome = nome.ToLower();
         var cidade = cachedRepository.FirstOrDefault(e => e.Nome.ToLower() == nome);
         int CidadeID = 0;

         if (cidade != null)
         {
            CidadeID = cidade.ID;
         }

         return CidadeID;
      }


   }
}
