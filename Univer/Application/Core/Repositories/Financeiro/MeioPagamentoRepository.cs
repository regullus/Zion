using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
   public class MeioPagamentoRepository : PersistentRepository<Entities.MeioPagamento>
   {

      public MeioPagamentoRepository(DbContext context)
          : base(context)
      {
      }

      public IEnumerable<Entities.MeioPagamento> GetAtivos()
      {
         return this.GetByExpression(x => x.Ativo == true);
      }



   }
}
