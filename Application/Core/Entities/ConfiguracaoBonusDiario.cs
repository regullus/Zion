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
    
    public partial class ConfiguracaoBonusDiario
    {
        public int ID { get; set; }
        public System.DateTime DataReferencia { get; set; }
        public double Valor { get; set; }
        public Nullable<int> AssociacaoID { get; set; }
        public bool IsPercentual { get; set; }
    
        public virtual Associacao Associacao { get; set; }
    }
}
