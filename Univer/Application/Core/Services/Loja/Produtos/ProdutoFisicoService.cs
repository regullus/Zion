using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Loja.Produtos
{
    internal class ProdutoFisicoService : BaseService
    {

        public ProdutoFisicoService(DbContext context)
            : base(context)
        {
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
        }

    }
}
