using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroNivelModel
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int BoardID {  get; set; }
        public string BoardNome { get; set; }
        public int TabuleiroID { get; set; }
        public string Posicao { get; set; }
        public int DataInicio { get; set; }
        public Nullable<int> DataFim {  get; set; }
        public int StatusID { get; set; }
        public string Observacao { get; set; }

    }
}
