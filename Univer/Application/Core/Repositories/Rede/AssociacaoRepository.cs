using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class AssociacaoRepository : CachedRepository<Entities.Associacao>
    {

        public AssociacaoRepository(DbContext context)
            : base(context)
        {
        }

        public Entities.Associacao GetByNivel(int nivel)
        {
            return cachedRepository.FirstOrDefault(a => a.Nivel == nivel);
        }

    }
}
