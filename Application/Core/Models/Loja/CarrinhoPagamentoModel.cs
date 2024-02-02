using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Loja
{
    public class CarrinhoPagamentoModel
    {

        public Entities.PedidoPagamento.MeiosPagamento MeioPagamento;
        public Entities.PedidoPagamento.FormasPagamento FormaPagamento;

        public int? ContaID;
        public decimal? Valor;
        public int? MoedaIDCripto;
        public double? ValorCripto;
        public double? CotacaoCripto;
        public Entities.Usuario Usuario;
        
    }
}
