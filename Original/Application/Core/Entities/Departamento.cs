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
    
    public partial class Departamento
    {
        public Departamento()
        {
            this.Departamento1 = new HashSet<Departamento>();
            this.ProdutoDepartamento = new HashSet<ProdutoDepartamento>();
        }
    
        public int ID { get; set; }
        public Nullable<int> DepartamentoID { get; set; }
        public string Nome { get; set; }
        public int Ordem { get; set; }
        public bool Disponivel { get; set; }
    
        public virtual ICollection<Departamento> Departamento1 { get; set; }
        public virtual Departamento Departamento2 { get; set; }
        public virtual ICollection<ProdutoDepartamento> ProdutoDepartamento { get; set; }
    }
}