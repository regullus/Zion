using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class AgendamentoItemRepository : PersistentRepository<Entities.AgendamentoItem>
    {
        public AgendamentoItemRepository(DbContext context)
            : base(context)
        {
        }
    }
}
