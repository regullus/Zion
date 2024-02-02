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
    
    public partial class ContaDeposito
    {
        public int ID { get; set; }
        public int IDUsuario { get; set; }
        public Nullable<int> IDTipoConta { get; set; }
        public Nullable<int> IDInstituicao { get; set; }
        public int IDMeioPagamento { get; set; }
        public string Agencia { get; set; }
        public string Conta { get; set; }
        public string DigitoConta { get; set; }
        public string ProprietarioConta { get; set; }
        public string IdentificacaoProprietario { get; set; }
        public System.DateTime DataCriacao { get; set; }
        public string CPF { get; set; }
        public string CNPJ { get; set; }
        public int MoedaIDCripto { get; set; }
        public string Litecoin { get; set; }
        public string Bitcoin { get; set; }
        public string Tether { get; set; }
    
        public virtual Instituicao Instituicao { get; set; }
        public virtual MeioPagamento MeioPagamento { get; set; }
        public virtual Moeda Moeda { get; set; }
        public virtual TipoConta TipoConta { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
