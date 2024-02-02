
using System;

namespace Core.Models.Relatorios
{
    public class RelatorioPontosBinarioModel
    {
        public int tipoRg { get; set; }
        public DateTime dataCriacao { get; set; }
        public string login { get; set; }
        public int pedidoIDOrigem { get; set; }
        public string produtoNome { get; set; }
        public double pontosEsqueda { get; set; }
        public double valorEsqueda { get; set; }
        public double pontosDireita { get; set; }
        public double valorDireita { get; set; }
        public string loginPatrocinador { get; set; }

    }
}

