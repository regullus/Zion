using cpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroUsuarioModel
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int TabuleiroID { get; set; }
        public int BoardID { get; set; }
        public int StatusID { get; set; }
        public int MasterID { get; set; }
        public int Ciclo { get; set; }
        public string Posicao { get; set; }
        public bool PagoMaster { get; set; }
        public bool PagoSistema { get; set;}
        public DateTime DataInicio { get; set; }
        public int? DataFim {  get; set; }
    }
}
