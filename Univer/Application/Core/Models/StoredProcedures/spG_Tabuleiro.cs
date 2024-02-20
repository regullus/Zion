using System;

namespace Core.Models.StoredProcedures
{
    public partial class spG_Tabuleiro
    {
        public string Retorno { get; set; }
        public int UsuarioID { get; set; }
        public int UsuarioPaiID { get; set; }
        public int TabuleiroID { get; set; }
        public int BoardID { get; set; }
        public string Posicao { get; set; }
        public string Historico { get; set; }
        public string Chamada { get; set; }
    }
}
