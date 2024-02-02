using Core.Helpers;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Loja.Produtos
{
    public abstract class BaseService
    {

        protected PedidoItemStatusRepository pedidoItemStatusRepository;

        public BaseService(DbContext context)
        {
            pedidoItemStatusRepository = new PedidoItemStatusRepository(context);
        }

        public virtual void Liberar(Entities.PedidoItem pedidoItem)
        {
            var status = new Entities.PedidoItemStatus()
            {
                Data = App.DateTimeZion,
                Mensagem = "",
                PedidoItemID = pedidoItem.ID,
                Status = Entities.PedidoItemStatus.TodosStatus.AguardandoEnvio
            };
            pedidoItemStatusRepository.Save(status);
        }

    }
}
