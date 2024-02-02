using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class EstoqueMovimento : IPersistentEntity
    {
        public const string TipoSaida = "S";
        public const string TipoEntrada = "E";
    }
}
