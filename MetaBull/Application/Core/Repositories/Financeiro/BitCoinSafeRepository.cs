using Core.Entities;
using DomainExtension.Repositories;
using System.Data.Entity;

namespace Core.Repositories.Financeiro
{
    public class BitCoinSafeRepository : PersistentRepository<BitCoinSafe>
    {
        public BitCoinSafeRepository(DbContext context)
        : base(context)
        {
        }
    }
}
