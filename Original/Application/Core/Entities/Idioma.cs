//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Idioma
    {
        public Idioma()
        {
            this.Paises = new HashSet<Pais>();
            this.Traducoes = new HashSet<Traducao>();
            this.Aviso = new HashSet<Aviso>();
        }
    
        public int ID { get; set; }
        public string Sigla { get; set; }
        public string Nome { get; set; }
    
        public virtual ICollection<Pais> Paises { get; set; }
        public virtual ICollection<Traducao> Traducoes { get; set; }
        public virtual ICollection<Aviso> Aviso { get; set; }
    }
}