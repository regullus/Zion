using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Traducao : IPersistentEntity
    {

        public enum Secoes
        {
         Geral = 1,
         Frontend = 2,
      }

        public Secoes Secao
        {
            get { return (Secoes)this.SecaoID; }
            set { this.SecaoID = (int)value; }
        }

    }
}
