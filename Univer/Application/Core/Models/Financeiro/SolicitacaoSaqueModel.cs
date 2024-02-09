using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Financeiro
{
    public class SolicitacaoSaqueModel
    {
        public int Codigo { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public int UsuarioID { get; set; }
        public DateTime Data { get; set; }
        public double Liquido { get; set; }
        public double? LiquidoBTC { get; set; }
        public double? LiquidoLTC { get; set; }
        public double? TotalBTC { get; set; }
        public double? TotalLTC { get; set; }
        public int Status { get; set; }
        public string Moeda { get; set; }
        public string Bitcoin { get; set; }
        public string Litecoin { get; set; }
        public double? Fee { get; set; }
        public string Mensagem { get; set; }
    }
}
