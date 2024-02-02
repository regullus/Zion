using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class PedidoPagamentoStatus : IPersistentEntity
    {

        public enum TodosStatus
        {
            Indefinido = 0,
            [Description("Aguardando Pagamento")]
            AguardandoPagamento = 1,
            [Description("Aguardando Confirmação")]
            AguardandoConfirmacao = 2,
            Pago = 3,
            Cancelado = 4,
            Expirado = 5,
            [Description("Pagamento Parcial")]
            PagamentoParcial = 6
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }
}
