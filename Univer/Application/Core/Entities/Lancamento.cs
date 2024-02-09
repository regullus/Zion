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
    
    public partial class Lancamento
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int ContaID { get; set; }
        public int TipoID { get; set; }
        public Nullable<int> CategoriaID { get; set; }
        public int ReferenciaID { get; set; }
        public Nullable<double> Valor { get; set; }
        public string Descricao { get; set; }
        public System.DateTime DataCriacao { get; set; }
        public System.DateTime DataLancamento { get; set; }
        public Nullable<int> PedidoID { get; set; }
        public Nullable<int> CicloID { get; set; }
        public Nullable<int> RegraItemID { get; set; }
        public int MoedaIDCripto { get; set; }
        public Nullable<double> ValorCripto { get; set; }
    
        public virtual Categoria Categoria { get; set; }
        public virtual Conta Conta { get; set; }
        public virtual Ciclo Ciclo { get; set; }
        public virtual LancamentoTipo LancamentoTipo { get; set; }
        public virtual Moeda Moeda { get; set; }
        public virtual Pedido Pedido { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
