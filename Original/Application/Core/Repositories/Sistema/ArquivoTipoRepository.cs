using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
   public class ArquivoTipoRepository : CachedRepository<Entities.ArquivoTipo>
   {

      public ArquivoTipoRepository(DbContext context)
          : base(context)
      {
      }
   }
}