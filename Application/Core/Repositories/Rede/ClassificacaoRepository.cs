using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class ClassificacaoRepository : CachedRepository<Entities.Classificacao>
    {

        public ClassificacaoRepository(DbContext context)
            : base(context)
        {
        }

        public Entities.Classificacao GetByNivel(int nivel)
        {
            return cachedRepository.FirstOrDefault(a => a.Nivel == nivel);
        }

    }
}
