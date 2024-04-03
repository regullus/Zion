using cpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroBoardModel
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; }
        public string CorTexto { get; set; }
        public int GroupID { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
        public bool Ativo { get; set; }
        public decimal Licenca { get; set;}
        public decimal Transferencia { get; set; }
        public int Indicados { get; set; }
    }
}
