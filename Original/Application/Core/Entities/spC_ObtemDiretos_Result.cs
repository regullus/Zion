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
    
    public partial class spC_ObtemDiretos_Result
    {
        public int ID { get; set; }
        public int IDAfiliado { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public Nullable<int> PatrocinadorDiretoID { get; set; }
        public string LoginPatrocinadorDireto { get; set; }
        public string NomePatrocinadorDireto { get; set; }
        public Nullable<System.DateTime> DataAtivacao { get; set; }
        public Nullable<System.DateTime> DataValidade { get; set; }
        public Nullable<int> DerramamentoID { get; set; }
        public Nullable<int> AssociacaoID { get; set; }
        public string NomeAssociacao { get; set; }
        public Nullable<System.DateTime> Data { get; set; }
        public Nullable<int> Nivel { get; set; }
    }
}
