using Core.Entities;
using Core.Repositories.Loja;
using Core.Services.Loja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanceladorPedidos
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cancelador de pedidos expirados");

            var context = new YLEVELEntities();

            var pedidoRepository = new PedidoRepository(context);
            var service = new PedidoService(context);

            var pedidos = pedidoRepository.GetPedidosVencidos();

            foreach (var pedido in pedidos)
            {
                service.ExpirarAguardandoPagamento(pedido);
            }

        }
    }
}
