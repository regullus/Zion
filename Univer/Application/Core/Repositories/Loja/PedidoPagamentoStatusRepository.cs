using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class PedidoPagamentoStatusRepository : PersistentRepository<Entities.PedidoPagamentoStatus>
    {

        public PedidoPagamentoStatusRepository(DbContext context)
            : base(context)
        {
        }

    }
}
