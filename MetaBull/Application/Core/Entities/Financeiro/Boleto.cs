using Core.Helpers;
using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Boleto : IPersistentEntity
    {

        public enum TodosStatus
        {
            Indefinido = 0,
            [Description("Aguardando Pagamento")]
            AguardandoPagamento = 1,
            Vencido = 2,
            [Description("Pago Total")]
            PagoTotal = 3,
            [Description("Pago Parcial")]
            PagoParcial = 4,
            Cancelado = 5
        }

        public TodosStatus Status
        {
            get
            {
                if (this.DataVencimento < App.DateTimeZion && (this.StatusID != 3 && this.StatusID != 5))
                {
                    return TodosStatus.Vencido;
                }
                return (TodosStatus)this.StatusID;
            }
            set { this.StatusID = (int)value; }
        }

    }
}
