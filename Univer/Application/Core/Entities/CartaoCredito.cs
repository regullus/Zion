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
    
    public partial class CartaoCredito
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public Nullable<int> PedidoPagamentoID { get; set; }
        public Nullable<int> PedidoID { get; set; }
        public string Bandeira { get; set; }
        public string FinalCartao { get; set; }
        public string Token { get; set; }
        public Nullable<double> Valor { get; set; }
        public string Descricao { get; set; }
        public System.DateTime DataCriacao { get; set; }
        public Nullable<System.DateTime> DataPagamento { get; set; }
        public Nullable<System.Guid> PagamentoID { get; set; }
        public string TransacaoID { get; set; }
        public string ComprovantePagamento { get; set; }
        public string CodigoAutorizacao { get; set; }
        public string CodigoRetorno { get; set; }
        public string MensagemRetorno { get; set; }
    
        public virtual Usuario Usuario { get; set; }
    }
}