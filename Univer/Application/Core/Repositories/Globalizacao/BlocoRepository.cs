using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class BlocoRepository : CachedRepository<Entities.Bloco>
    {

        public BlocoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
