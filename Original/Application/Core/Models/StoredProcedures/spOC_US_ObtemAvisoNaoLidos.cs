using System;

namespace Core.Models.StoredProcedures
{
    public partial class spOC_US_ObtemAvisoNaoLidos
    {
        public int ID { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
        public bool Urgente { get; set; }
        public Nullable<System.DateTime> DataInicio { get; set; }
    }
}
