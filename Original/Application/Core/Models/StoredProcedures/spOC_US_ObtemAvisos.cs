﻿using System;

namespace Core.Models.StoredProcedures
{
    public partial class spOC_US_ObtemAvisos
    {
        public int ID { get; set; }
        public string Titulo { get; set; }
        public string Video { get; set; }
        public bool Urgente { get; set; }
        public Nullable<System.DateTime> DataInicio { get; set; }
        public Nullable<System.DateTime> DataFim { get; set; }
        public int TipoID { get; set; }
        public string TipoNome { get; set; }
        public Nullable<System.DateTime> DataLeitura { get; set; }
    }
}
