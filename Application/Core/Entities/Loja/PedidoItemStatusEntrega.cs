using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class PedidoItemStatusEntrega : IPersistentEntity
    {

        public enum TodosStatus
        {
            Indefinido = 0,
            [Description("Aguardando Entrega")]
            AguardandoEntrega = 1,
            Entregue = 3,
            Cancelado = 4,
            [Description("Na transportadora")]
            NaTransportadora = 5,
            [Description("Em separação")]
            EmSeparacao = 6
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }
}
