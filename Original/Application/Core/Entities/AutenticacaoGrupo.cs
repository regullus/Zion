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
    
    public partial class AutenticacaoGrupo
    {
        public AutenticacaoGrupo()
        {
            this.AutenticacaoGrupoRegra = new HashSet<AutenticacaoGrupoRegra>();
            this.AutenticacaoUsuarioGrupo = new HashSet<AutenticacaoUsuarioGrupo>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<AutenticacaoGrupoRegra> AutenticacaoGrupoRegra { get; set; }
        public virtual ICollection<AutenticacaoUsuarioGrupo> AutenticacaoUsuarioGrupo { get; set; }
    }
}