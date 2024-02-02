using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class IdiomaRepository : CachedRepository<Entities.Idioma>
    {

        public IdiomaRepository(DbContext context)
            : base(context)
        {
        }

        public Entities.Idioma GetBySigla(string sigla)
        {
            return cachedRepository.FirstOrDefault(i => i.Sigla == sigla);
        }

    }
}
