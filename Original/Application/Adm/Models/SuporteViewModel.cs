using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Models
{
    public class SuporteViewModel
    {
        public int ID { get; set; }
        public string Assunto { get; set; }
        public DateTime Data { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Idioma { get; set; }
    }
}