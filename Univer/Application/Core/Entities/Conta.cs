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
    
    public partial class Conta
    {
        public Conta()
        {
            this.ContaOperacoes = new HashSet<ContaOperacao>();
            this.Lancamento = new HashSet<Lancamento>();
            this.PedidoPagamento = new HashSet<PedidoPagamento>();
        }
    
        public int ID { get; set; }
        public Nullable<int> MoedaID { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; }
        public bool PermiteSaque { get; set; }
    
        public virtual ICollection<ContaOperacao> ContaOperacoes { get; set; }
        public virtual Moeda Moeda { get; set; }
        public virtual ICollection<Lancamento> Lancamento { get; set; }
        public virtual ICollection<PedidoPagamento> PedidoPagamento { get; set; }
    }
}