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
    
    public partial class Saque
    {
        public Saque()
        {
            this.SaqueStatus = new HashSet<SaqueStatus>();
        }
    
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int BancoID { get; set; }
        public int MoedaID { get; set; }
        public Nullable<double> Total { get; set; }
        public Nullable<double> Taxas { get; set; }
        public Nullable<double> Impostos { get; set; }
        public Nullable<double> Liquido { get; set; }
        public Nullable<double> IR { get; set; }
        public Nullable<double> INSS { get; set; }
        public string Carteira { get; set; }
        public Nullable<double> Fee { get; set; }
        public Nullable<System.DateTime> Data { get; set; }
        public int MoedaIDCripto { get; set; }
        public Nullable<double> LiquidoCripto { get; set; }
        public Nullable<double> TotalCripto { get; set; }
    
        public virtual Banco Banco { get; set; }
        public virtual Moeda Moeda { get; set; }
        public virtual Moeda Moeda1 { get; set; }
        public virtual ICollection<SaqueStatus> SaqueStatus { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}