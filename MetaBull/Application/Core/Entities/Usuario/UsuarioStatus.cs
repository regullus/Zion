using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class UsuarioStatus : IPersistentEntity
    {

        public Usuario.TodosStatus Status
        {
            get { return (Usuario.TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }
}
