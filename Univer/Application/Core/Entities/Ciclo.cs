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
    
    public partial class Ciclo
    {
        public Ciclo()
        {
            this.AtivacaoMensal = new HashSet<AtivacaoMensal>();
            this.Lancamento = new HashSet<Lancamento>();
            this.Bonificacao = new HashSet<Bonificacao>();
        }
    
        public int ID { get; set; }
        public string Nome { get; set; }
        public System.DateTime DataInicial { get; set; }
        public System.DateTime DataFinal { get; set; }
        public bool Ativo { get; set; }
    
        public virtual ICollection<AtivacaoMensal> AtivacaoMensal { get; set; }
        public virtual ICollection<Lancamento> Lancamento { get; set; }
        public virtual ICollection<Bonificacao> Bonificacao { get; set; }
    }
}