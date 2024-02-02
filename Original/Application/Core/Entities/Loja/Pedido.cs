using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Pedido : IPersistentEntity
    {

        public enum TodosTiposdeEntrega
        {
            Envio = 0,
            Retirada = 1
        }

        public TodosTiposdeEntrega TipoEntrega
        {
            get
            {
                if (this.TipoEntregaID == 1)
                {
                    return TodosTiposdeEntrega.Envio;
                }
                else
                {
                    return TodosTiposdeEntrega.Retirada;
                }
            }
            set
            {
                TipoEntregaID = (value == TodosTiposdeEntrega.Retirada) ? 1 : 0;
            }
        }

        public PedidoPagamentoStatus.TodosStatus StatusAtual
        {
            get
            {
                if (this.PedidoPagamento.Any())
                {
                    if (this.PedidoPagamento.Count == 1)
                    {
                        return this.PedidoPagamento.FirstOrDefault().UltimoStatus.Status;
                    }
                    else
                    {
                        return this.PedidoPagamento.OrderByDescending(p => p.UltimoStatus.StatusID).FirstOrDefault().UltimoStatus.Status;
                    }
                }
                else
                {
                    return PedidoPagamentoStatus.TodosStatus.Indefinido;
                }
            }
        }


        public PedidoItemStatusEntrega.TodosStatus StatusEntregaAtual
        {
            get
            {
                if (this.PedidoItem.Any())
                {
                    var qtdeItens = this.PedidoItem.Sum(i => i.Quantidade) ;
                    var itens = this.PedidoItem.ToList();
                    var qtdeEntregue = 0;
                    var qtdeCancelado = 0;
                    foreach(var item in this.PedidoItem)
                    {
                        if(item.PedidoItemStatusEntrega.Any())
                        {
                            qtdeEntregue = item.PedidoItemStatusEntrega.Count(s => s.StatusID == 3);
                            qtdeCancelado = item.PedidoItemStatusEntrega.Count(s => s.StatusID == 4);
                        }
                    }

                    if (qtdeEntregue >= qtdeItens )
                    {
                        return PedidoItemStatusEntrega.TodosStatus.Entregue;
                    }
                    else if (qtdeCancelado >= qtdeItens)
                    {
                        return PedidoItemStatusEntrega.TodosStatus.Cancelado;
                    }
                    else
                    {
                        return PedidoItemStatusEntrega.TodosStatus.AguardandoEntrega;
                    }
                }
                else
                {
                    return PedidoItemStatusEntrega.TodosStatus.Indefinido;
                }
            }
        }
    }
}
