using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class PedidoPagamentoRepository : PersistentRepository<Entities.PedidoPagamento>
    {
        private DbContext _context;

        public PedidoPagamentoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public int ObtemPedidosPendentesPgto(int idUsuario)
        {
            string sql = "EXEC spOC_LO_ObtemPedidosPendentesPgto " + idUsuario;

            return _context.Database.SqlQuery<int>(sql).FirstOrDefault();
        }
    }
}