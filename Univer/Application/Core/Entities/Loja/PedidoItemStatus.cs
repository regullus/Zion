using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class PedidoItemStatus : IPersistentEntity
    {

        public enum TodosStatus
        {
            Indefinido = 0,
            [Description("Aguardando Pagamento")]
            AguardandoPagamento = 1,
            [Description("Aguardando Envio")]
            AguardandoEnvio = 2,
            Enviado = 3,
            Entregue = 4,
            Cancelado = 5
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }
}
