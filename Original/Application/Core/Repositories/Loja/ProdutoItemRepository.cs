using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class ProdutoItemRepository : PersistentRepository<Entities.ProdutoItem>
    {

        public ProdutoItemRepository(DbContext context)
            : base(context)
        {
        }

    }
}
