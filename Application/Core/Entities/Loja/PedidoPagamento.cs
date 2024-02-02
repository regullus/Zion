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
    public partial class PedidoPagamento : IPersistentEntity
    {

        public enum MeiosPagamento
        {
            Indefinido = 1,
            Deposito = 2,
            Boleto = 3,
            Saldo = 4,
            Cartao = 5,
            PayPal = 6,
            CryptoPayments = 7,
            SemPagamento = 8,
            PagSeguro = 9,
            Gratis = 10,
            ViviPay = 11,
            Manual = 12,
            Outros = 13
        }

        public enum FormasPagamento
        {
            [Description("Padrão")]
            Padrao = 1
        }

        public MeiosPagamento MeioPagamento
        {
            get { return (MeiosPagamento)this.MeioPagamentoID; }
            set { this.MeioPagamentoID = (int)value; }
        }

        public FormasPagamento FormaPagamento
        {
            get { return (FormasPagamento)this.FormaPagamentoID; }
            set { this.FormaPagamentoID = (int)value; }
        }

        public PedidoPagamentoStatus UltimoStatus
        {
            get
            {
                if (this.PedidoPagamentoStatus.Count > 0)
                {
                    return this.PedidoPagamentoStatus.OrderByDescending(s => s.Data).FirstOrDefault();
                }
                else
                {
                    return new PedidoPagamentoStatus()
                    {
                        Data = App.DateTimeZion,
                        Status = Entities.PedidoPagamentoStatus.TodosStatus.Indefinido
                    };
                }
            }
        }

    }
}
