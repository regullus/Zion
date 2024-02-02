using Core.Entities;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrigePedidos
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new YLEVELEntities();

            PedidoPagamentoRepository pedidoPagamentoRepository = new PedidoPagamentoRepository(context);

            var pedidosPagamentos = pedidoPagamentoRepository
                                    .GetByExpression(p => p.Pedido.DataCriacao <= new DateTime(2020, 02, 20, 9, 22, 21) && p.PedidoPagamentoStatus.Any(a => a.StatusID == 3))
                                    .ToList();

            var pagos = PedidosPagos(context);

            var gatewayCount = 0;
            var manualCount = 0;
            
            foreach (var pedidoPagamento in pedidosPagamentos)
            {
                var pago = pagos.FirstOrDefault(f => f.endereco == pedidoPagamento.Numero);

                if(pago == null)
                {
                    var pagamento = pedidoPagamentoRepository.Get(pedidoPagamento.ID);

                    manualCount++;
                    Console.WriteLine("Manual: {0}", pedidoPagamento.PedidoID.ToString());
                    pagamento.MeioPagamentoID = (int)Core.Entities.PedidoPagamento.MeiosPagamento.Manual;

                    pedidoPagamentoRepository.Save(pagamento);

                    Log(pedidoPagamento.PedidoID.ToString() + ":" + pedidoPagamento.ID);
                }
                else
                {
                    gatewayCount++;
                    Console.WriteLine("Gateway: {0}", pedidoPagamento.PedidoID.ToString());
                }

            }

            Console.WriteLine("Total Manual: {0}", manualCount.ToString());
            Console.WriteLine("Total Gateway: {0}", gatewayCount.ToString());


            Console.ReadKey();



        }

        public static void Log(string texto)
        {
            using (StreamWriter writer = new StreamWriter(@"\LogProgram.txt", true))
            {
                writer.WriteLine(texto);
            }
        }

        public static List<PedidoPago> PedidosPagos(DbContext _context)
        {
            string sql = "SELECT * FROM loja.pedidospagos";

            return _context.Database.SqlQuery<PedidoPago>(sql).ToList();
        }
    }

    public class PedidoPago
    {
        public int id { get; set; }
        public string descricao { get; set; }
        public string endereco { get; set; }
        public double valor { get; set; }
    }
}
