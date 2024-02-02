using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Relatorios
{
    public class RelatorioSaldoItem
    {
        public int Codigo { get; set; }
        public string Pais { get; set; }
        public string Estado { get; set; }
        public string Cidade { get; set; }
        public string Login { get; set; }
        public DateTime Data { get; set; }
        public double Valor { get; set; }
        public String Bitcoin { get; set; }
        public double Ganho { get; set; }
        public double Saques { get; set; }
        public double Transferencias { get; set; }
        public string Moeda { get; set; }

    }
}

