using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
   public class LogRepository : PersistentRepository<Entities.Log>
   {

      public LogRepository(DbContext context)
          : base(context)
      {
      }

      public Entities.Log GetByLocal(string local)
      {
         return base.GetByExpression(x => x.Local == local).FirstOrDefault();
      }

   }
}
