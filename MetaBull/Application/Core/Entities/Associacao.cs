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
    
    public partial class Associacao
    {
        public int ID { get; set; }
        public int Nivel { get; set; }
        public string Nome { get; set; }
        public int DuracaoDias { get; set; }
        public Nullable<decimal> PercentualBinario { get; set; }
    }
}
