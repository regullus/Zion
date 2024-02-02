using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class BlockchainRepository : PersistentRepository<BlockchainLog>
    {
        public BlockchainRepository(DbContext context)
            : base(context)
        {
        }
    }
}
