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
        public bool Respondido { get; set; }
        public DateTime Data { get; set; }
    }
}