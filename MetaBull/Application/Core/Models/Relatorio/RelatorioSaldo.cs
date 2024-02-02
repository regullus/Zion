using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Relatorios
{
    public class RelatorioSaldo
   {
        public List<RelatorioSaldoItem> Itens { get; set; }

        public RelatorioSaldoResumo Resumo { get; set; }

        public RelatorioSaldo()
        {
            Itens = new List<RelatorioSaldoItem>();
            Resumo = new RelatorioSaldoResumo();
        }
    }
}

