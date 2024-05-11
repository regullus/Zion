using cpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TabuleiroInformaramPagtoModel
    {
        public int UsuarioID { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Celular { get; set; }
        public string Posicao { get; set; }
        public string Galaxia { get; set; }
        public string Patrocinador { get; set; }
        public int? TabuleiroID { get; set; }
        public int BoardID { get; set; }
        public string BoardNome { get; set; }
        public string BoardCor { get; set; }
        public int StatusID { get; set; }
        public bool Eterno { get; set; }
        public int MasterID { get; set; }
        public bool InformePag { get; set; }
        public int? UsuarioIDPag { get; set; }
        public int Ciclo { get; set; }
        public bool PagoMaster { get; set; }
        public bool PagoSistema { get; set;}
        public bool InformePagSistema { get; set; }
        public int TotalRecebimento { get; set; }
        public DateTime DataInicio { get; set; }
        public int? DataFim {  get; set; }
    }
}
