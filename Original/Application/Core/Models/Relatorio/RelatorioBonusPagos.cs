
using System;

namespace Core.Models.Relatorios
{
    public class RelatorioBonusPagosModel
    {
        public int tipoRg { get; set; }
        public int usuarioID { get; set; }
        public string login { get; set; }
        public string nivelAssociacao { get; set; }
        public int ativo { get; set; }
        public string categoriaNome { get; set; }
        public double valor { get; set; }
        public DateTime? data { get; set; }
        public string descricao { get; set; }
        public int? pedidoID { get; set; }
        public string loginPedido { get; set; }
    }
}

