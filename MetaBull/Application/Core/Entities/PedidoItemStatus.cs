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
    
    public partial class PedidoItemStatus
    {
        public int ID { get; set; }
        public int PedidoItemID { get; set; }
        public Nullable<int> AdministradorID { get; set; }
        public int StatusID { get; set; }
        public string Mensagem { get; set; }
        public System.DateTime Data { get; set; }
    
        public virtual PedidoItem PedidoItem { get; set; }
        public virtual Administrador Administrador { get; set; }
    }
}
