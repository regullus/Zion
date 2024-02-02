using Core.Helpers;
using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class PedidoItem : IPersistentEntity
    {

        public PedidoItemStatus UltimoStatus
        {
            get
            {
                if (this.PedidoItemStatus.Any())
                {
                    return this.PedidoItemStatus.OrderByDescending(s => s.StatusID).FirstOrDefault();
                }
                else
                {
                    return new PedidoItemStatus()
                    {
                        Data = App.DateTimeZion,
                        Mensagem = "",
                        Status = Entities.PedidoItemStatus.TodosStatus.Indefinido
                    };
                }
            }
        }

        public PedidoItemStatusEntrega UltimoStatusEntrega
        {
            get
            {
                if (this.PedidoItemStatusEntrega.Any())
                {
                    return this.PedidoItemStatusEntrega.OrderByDescending(s => s.StatusID).FirstOrDefault();
                }
                else
                {
                    return new PedidoItemStatusEntrega()
                    {
                        Data = App.DateTimeZion,
                        Mensagem = "",
                        Status = Entities.PedidoItemStatusEntrega.TodosStatus.Indefinido
                    };
                }
            }
        }


    }
}
