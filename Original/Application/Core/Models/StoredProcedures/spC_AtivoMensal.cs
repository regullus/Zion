using System;

namespace Core.Models.StoredProcedures
{
    public partial class spC_AtivoMensal
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public Nullable<System.DateTime> DataValidade { get; set; }
        public int Status { get; set; }
        public string Lado { get; set; }
    }
}
