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
    
    public partial class UsuarioClassificacao
    {
        public int ID { get; set; }
        public int UsuarioID { get; set; }
        public int NivelClassificacao { get; set; }
        public System.DateTime Data { get; set; }
        public Nullable<int> CicloID { get; set; }
        public int NivelReconhecimento { get; set; }
    
        public virtual Usuario Usuario { get; set; }
        public virtual Ciclo Ciclo { get; set; }
    }
}
