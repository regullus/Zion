using System;

namespace Core.Models.StoredProcedures
{
    public partial class spC_ObtemDiretos
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
