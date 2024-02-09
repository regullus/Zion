using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sistema.Models
{
    public class UsuarioRede
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Kit { get; set; }
        public string Classificacao { get; set; }
        public string AtivacaoMensal { get; set; }
        public int Nivel { get; set; }
        public string Lado  { get; set; }
    }
}