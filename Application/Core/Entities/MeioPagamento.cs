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
    
    public partial class MeioPagamento
    {
        public MeioPagamento()
        {
            this.ContaDeposito = new HashSet<ContaDeposito>();
        }
    
        public int ID { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
    
        public virtual ICollection<ContaDeposito> ContaDeposito { get; set; }
    }
}
