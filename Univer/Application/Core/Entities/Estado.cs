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
    
    public partial class Estado
    {
        public Estado()
        {
            this.Cidades = new HashSet<Cidade>();
            this.Enderecos = new HashSet<Endereco>();
            this.Armazem = new HashSet<Armazem>();
            this.Filial = new HashSet<Filial>();
        }
    
        public int ID { get; set; }
        public int PaisID { get; set; }
        public string Sigla { get; set; }
        public string Nome { get; set; }
    
        public virtual ICollection<Cidade> Cidades { get; set; }
        public virtual ICollection<Endereco> Enderecos { get; set; }
        public virtual Pais Pais { get; set; }
        public virtual ICollection<Armazem> Armazem { get; set; }
        public virtual ICollection<Filial> Filial { get; set; }
    }
}