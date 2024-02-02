using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public enum Periodos
    {
        Matutino = 0,
        Vespertino = 1,
        Nortuno = 2
    }
    public partial class AgendamentoItem : IPersistentEntity
    {

    }
}
