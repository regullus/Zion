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
    
    public partial class SplitCripto
    {
        public int ID { get; set; }
        public int PedidoID { get; set; }
        public int MoedaID { get; set; }
        public Nullable<double> Valor { get; set; }
        public int MoedaIDCripto { get; set; }
        public Nullable<double> ValorCripto { get; set; }
        public Nullable<double> CotacaoCripto { get; set; }
        public Nullable<int> Plataforma { get; set; }
        public string Carteira { get; set; }
        public Nullable<double> Percentual { get; set; }
        public Nullable<int> Efetivado { get; set; }
        public string IdGateway { get; set; }
        public Nullable<System.DateTime> DataSolicitacaoGateway { get; set; }
        public string IPNID { get; set; }
        public string WithdrawID { get; set; }
        public string TXNID { get; set; }
        public Nullable<System.DateTime> DataEfetivacaoGateway { get; set; }
        public Nullable<int> StatusGateway { get; set; }
        public string StatusGatewayDescricao { get; set; }
    
        public virtual Pedido Pedido { get; set; }
        public virtual Moeda Moeda { get; set; }
        public virtual Moeda Moeda1 { get; set; }
    }
}
