using Core.Entities;
using Core.Helpers;
using Core.Repositories.Rede;
using Core.Repositories.Usuario;
using Core.Services.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Loja.Produtos
{
    internal class AtivoMensalService : BaseService
    {

        private UsuarioRepository usuarioRepository;
        private BonificacaoRepository bonificacaoRepository;
        private AtivacaoMensalRepository ativacaoMensalRepository;
        private CicloRepository cicloRepository;

        public AtivoMensalService(DbContext context)
              : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            ativacaoMensalRepository = new AtivacaoMensalRepository(context);
            cicloRepository = new CicloRepository(context);
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
            //var qtde = pedidoItem.Quantidade;
            var u = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);

            var dataValidadeBase = DateTime.MinValue;

            if (u.Validade > App.DateTimeZion)
            {
                dataValidadeBase = u.Validade;
            }
            else
            {
                dataValidadeBase = App.DateTimeZion;
            }
            u.DataValidade = dataValidadeBase.AddDays(30);
            //dataValidadeBase = dataValidadeBase.AddMonths(qtde);
            //u.Validade = dataValidadeBase;

            usuarioRepository.Save(u);

            var status = new PedidoItemStatus();

            status.Data = App.DateTimeZion;
            status.Mensagem = "";
            status.PedidoItemID = pedidoItem.ID;

            if (ConfiguracaoHelper.GetBoolean("LOJA_TEM_ENTREGA_PRODUTO"))
                status.Status = PedidoItemStatus.TodosStatus.AguardandoEnvio;
            else
                status.Status = PedidoItemStatus.TodosStatus.Entregue;

            pedidoItemStatusRepository.Save(status);

            if (ConfiguracaoHelper.GetBoolean("ATIVO_MENSAL_POR_CICLO"))
            {

                var am = ativacaoMensalRepository.GetByExpression(a => a.UsuarioID == u.ID).FirstOrDefault();
                if(am == null)
                {
                    am = new AtivacaoMensal();
                }
                var ciclo = cicloRepository.GetAtivo();
                am.CicloID = ciclo.ID;
                am.DataAtivacao = App.DateTimeZion;
                am.UsuarioID = u.ID;
                am.Ultimo = true;

                ativacaoMensalRepository.Save(am);

            }


            var pedidoPagamento = pedidoItem.Pedido.PedidoPagamento.FirstOrDefault();
            if (pedidoPagamento.ContaID == 1) /*apenas se foi pago em dinheiro*/
            {
                /*bonus de ativo mensal*/
                /*bonificacaoRepository.GerarBonusAtivoMensal(pedidoItem.Pedido.UsuarioID, pedidoItem.Pedido.Usuario.NivelAssociacao);*/
            }

        }
    }
}
