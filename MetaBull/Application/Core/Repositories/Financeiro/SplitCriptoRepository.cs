using Core.Entities;
using DomainExtension.Repositories;
using System.Data.Entity;

namespace Core.Repositories.Financeiro
{
    public class SplitCriptoRepository : PersistentRepository<SplitCripto>
    {
        public SplitCriptoRepository(DbContext context)
        : base(context)
        {
        }
    }
}
