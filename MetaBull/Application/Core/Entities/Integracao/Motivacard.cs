using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Entities
{
    public partial class Motivacard : IPersistentEntity
    {

        public enum TodosStatus
        {
            Indefinido = 0,
            Criado = 1,
            Solicitado = 2,
            Ativado = 3,
            Cancelado = 4
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }
}
