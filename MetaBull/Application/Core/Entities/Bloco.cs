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
    
    public partial class Bloco
    {
        public Bloco()
        {
            this.Paises = new HashSet<Pais>();
        }
    
        public int ID { get; set; }
        public string Nome { get; set; }
    
        public virtual ICollection<Pais> Paises { get; set; }
    }
}
