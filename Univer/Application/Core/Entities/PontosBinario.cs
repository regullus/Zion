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
    
    public partial class PontosBinario
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int PedidoIDOrigem { get; set; }
        public int Lado { get; set; }
        public Nullable<double> Pontos { get; set; }
        public Nullable<double> ValorCripto { get; set; }
        public Nullable<System.DateTime> DataReferencia { get; set; }
        public Nullable<System.DateTime> DataCriacao { get; set; }
        public int MoedaIDCripto { get; set; }
    
        public virtual Moeda Moeda { get; set; }
    }
}
