using DomainExtension.Repositories;
using System.Data.Entity;

namespace Core.Repositories.Sistema
{
    public class RegraIPRepository : PersistentRepository<Entities.RegraIP>
    {
        public RegraIPRepository(DbContext context)
            : base(context)
        {
        }
    }
}