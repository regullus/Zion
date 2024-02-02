using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Loja
{
    public class PedidoUltimoStatus
    {
        public int PedidoId { get; set; }
        public int StatusId { get; set; }
        public DateTime Data { get; set; }
    }
}
