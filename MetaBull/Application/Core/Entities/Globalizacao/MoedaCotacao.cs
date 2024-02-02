using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class MoedaCotacao : IPersistentEntity
    {

        public enum Tipos
        {
            Indefinido = 1,
            Entrada = 2,
            Saida = 3
        }

        public Tipos Tipo
        {
            get { return (Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

    }
}
