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
    
    public partial class PedidoTaxa
    {
        public int ID { get; set; }
        public int PedidoID { get; set; }
        public Nullable<int> TaxaID { get; set; }
        public Nullable<double> Valor { get; set; }
    
        public virtual Taxa Taxa { get; set; }
        public virtual Pedido Pedido { get; set; }
    }
}