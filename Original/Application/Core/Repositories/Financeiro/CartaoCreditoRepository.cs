using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class CartaoCreditoRepository : PersistentRepository<Entities.CartaoCredito>
    {

        public CartaoCreditoRepository(DbContext context)
            : base(context)
        {
        }

        public int GetMaxID()
        {
            return this.GetAll().Any() ? this.GetAll().Max(b => b.ID) : 0;
        }

    }
}
