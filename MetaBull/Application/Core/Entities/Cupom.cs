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
    
    public partial class Cupom
    {
        public Cupom()
        {
            this.Pedido = new HashSet<Pedido>();
        }
    
        public int ID { get; set; }
        public int TipoID { get; set; }
        public string Codigo { get; set; }
        public string BlocoIDs { get; set; }
        public string PaisIDs { get; set; }
        public bool Disponivel { get; set; }
        public Nullable<System.DateTime> DataInicio { get; set; }
        public Nullable<System.DateTime> DataFim { get; set; }
        public int Total { get; set; }
        public int Utilizado { get; set; }
        public int PorCliente { get; set; }
        public Nullable<double> Valor { get; set; }
    
        public virtual ICollection<Pedido> Pedido { get; set; }
    }
}