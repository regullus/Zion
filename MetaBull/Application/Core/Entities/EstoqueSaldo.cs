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
    
    public partial class EstoqueSaldo
    {
        public int ID { get; set; }
        public int ArmazemID { get; set; }
        public int ProdutoID { get; set; }
        public System.DateTime Data { get; set; }
        public int Quantidade { get; set; }
    
        public virtual Armazem Armazem { get; set; }
        public virtual Produto Produto { get; set; }
    }
}
