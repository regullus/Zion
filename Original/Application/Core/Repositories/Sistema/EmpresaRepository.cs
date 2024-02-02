using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
   public class EmpresaRepository : CachedRepository<Entities.Empresa>
   {

      public EmpresaRepository(DbContext context)
          : base(context)
      {
      }

    }
}

