
using System;
using System.Collections.Generic;

namespace Core.Models.Relatorios
{
    public class RelatorioPedidosModel
    {
        public int tipoRg { get; set; }
        public DateTime? dataPedido { get; set; }
        public string login { get; set; }
        public string pedido { get; set; }
        public string categoria { get; set; }
        public string produto { get; set; }
        public int produtoID { get; set; }
        public string meioPagamento { get; set; }
        public DateTime? dataStatus { get; set; }
        public int pgtoStatusID { get; set; }
        public int quantidade { get; set; }
        public double valor { get; set; }
        public double juros { get; set; }
        public double frete { get; set; }
        public double total { get; set; }

    }
}

