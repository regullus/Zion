using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class SaqueStatus : IPersistentEntity
    {
        // 26/08/2015 - Rab - Incluido Aprovado e Reprovado
        public enum TodosStatus
        {
            Indefinido = 0,
            Solicitado = 1,
            Processando = 2,
            Efetuado = 3,
            Cancelado = 4,
            Estornado = 5,
            Aprovado = 6,
            Reprovado = 7,
            Pago = 8,
            Aviso = 9,
            Reprocessando = 10
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }
    }
}
