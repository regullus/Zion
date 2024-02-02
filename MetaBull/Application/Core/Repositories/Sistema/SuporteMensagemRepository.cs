using DomainExtension.Repositories;
using System.Data.Entity;

namespace Core.Repositories.Sistema
{
    public class SuporteMensagemRepository : PersistentRepository<Entities.SuporteMensagem>
    {
        public SuporteMensagemRepository(DbContext context)
            : base(context)
        {
        }
    }
}
