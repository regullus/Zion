using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
   public class AssociacaoLimiteGanhoRepository : CachedRepository<Entities.AssociacaoLimiteGanho>
   {

      public AssociacaoLimiteGanhoRepository(DbContext context)
          : base(context)
      {
      }

      public Entities.AssociacaoLimiteGanho GetByTipo(Entities.AssociacaoLimiteGanho.Tipos tipo, int nivelAssociacao)
      {
         return cachedRepository.FirstOrDefault(a => a.TipoID == (int)tipo && a.NivelAssociacao == nivelAssociacao);
      }

   }
}
