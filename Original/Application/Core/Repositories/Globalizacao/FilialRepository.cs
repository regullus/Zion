using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class FilialRepository : CachedRepository<Entities.Filial>
    {
        public FilialRepository(DbContext context) : base(context)
        {
        }

    }
}
