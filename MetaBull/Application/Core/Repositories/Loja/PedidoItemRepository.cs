using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class PedidoItemRepository : PersistentRepository<Entities.PedidoItem>
    {
        private DbContext _context;

        public PedidoItemRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public IEnumerable<Entities.PedidoItem> GetByPedido(int idPedido)
        {
            return this.GetByExpression(p => p.PedidoID == idPedido).ToList();
        }
    }
}
