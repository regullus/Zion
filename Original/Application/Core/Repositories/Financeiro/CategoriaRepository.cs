using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class CategoriaRepository : CachedRepository<Entities.Categoria>
    {

        public CategoriaRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Categoria> GetByTipo(Entities.Lancamento.Tipos tipo)
        {
            return cachedRepository.Where(c => c.Tipo == tipo);
        }

    }
}
