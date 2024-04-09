using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroInclusao
    {
        public string Retorno { get; set; }
        public int UsuarioID { get; set; }
        public int UsuarioPaiID { get; set; }
        public int? TabuleiroID {  get; set; }
        public int BoardID { get; set; }
        public string Posicao { get; set; }
        public string Historico { get; set; }
        public string Debug { get; set; }
        public string Chamada { get; set; }
    }
}
