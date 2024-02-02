using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Endereco : IPersistentEntity
    {

        public string EnderecoCompleto
        {
            get { return String.Format("{0}, {1} {2}", this.Logradouro, this.Numero, this.Complemento); }
        }

    }
}
