using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class TraducaoRepository : CachedRepository<Entities.Traducao>
    {

        private static DomainExtension.Repositories.PersistentRepository<Entities.Traducao> _traducaoRepository;

        public TraducaoRepository(DbContext context)
            : base(context)
        {
            _traducaoRepository = new DomainExtension.Repositories.PersistentRepository<Entities.Traducao>(context);
        }

        public Entities.Traducao GetByIdiomaChave(int idiomaID, string chave)
        {
            chave = chave.ToUpper();
            return cachedRepository.FirstOrDefault(t => t.IdiomaID == idiomaID && t.Chave == chave);
        }

        public Entities.Traducao GetDirect(int id)
        {
            return _traducaoRepository.Get(id);
        }

        public IEnumerable<Entities.Traducao> GetDirectAll()
        {
            return _traducaoRepository.GetAll();
        }

        public void LimparCacheTraducoes()
        {
            ClearCache();
        }
    }
}
