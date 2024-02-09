using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
    public class ConfiguracaoRepository : CachedRepository<Entities.Configuracao>
    {

        public ConfiguracaoRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Configuracao> GetByCategoria(Entities.Configuracao.Categorias categoria)
        {
            return cachedRepository.Where(c => c.Categoria == categoria);
        }

        public Entities.Configuracao GetByChave(string chave)
        {
            if (!String.IsNullOrEmpty(chave))
            {
                return cachedRepository.FirstOrDefault(c => c.Chave == chave);
            }
            return null;
        }

    }
}
