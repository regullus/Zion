using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Relatorios
{
    public class RelatorioSaldoResumo
    {
        public int QuantidadeTotal { get; set; }
        public decimal Valorliquido { get; set; }
        public decimal ValorTotalPagoBTC { get; set; }
        public decimal ValorTotalPagoMAN { get; set; }
        public decimal ValorTotalAviso { get; set; }
        public decimal ValorTotalEstornado { get; set; }
        public decimal ValorTotalCancelado { get; set; }
        public decimal ValorTotalProcessando { get; set; }
        public decimal ValorTotalAprovado { get; set; }
    }
}

