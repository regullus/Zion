using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
   public class IRRepository : PersistentRepository<Entities.IR>
   {

      public IRRepository(DbContext context)
         : base(context)
      {
      }

      public IEnumerable<Entities.IR> GetByValor(double valor)
      {
         return base.GetByExpression(s => s.Inicial <= valor && s.Final >= valor);
      }
   }
}
