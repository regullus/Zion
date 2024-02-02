using Core.Helpers;
using Core.Entities;
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
    internal class UpgradeService : BaseService
    {

        private UsuarioService usuarioService;
        private UsuarioRepository usuarioRepository;
        private BonificacaoRepository bonificacaoRepository;
        private QualificacaoRepository qualificacaoRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;

        public UpgradeService(DbContext context)
            : base(context)
        {
            usuarioService = new UsuarioService(context);
            usuarioRepository = new UsuarioRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            qualificacaoRepository = new QualificacaoRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
            if (string.IsNullOrWhiteSpace(pedidoItem.Pedido.Usuario.Assinatura))
            {
                var usr = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);
                usr.Status = Entities.Usuario.TodosStatus.NaoAssociado;
                usuarioRepository.Save(usr);

                Boolean blnArvoreBinaria = false;
                if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
                    blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

                if (blnArvoreBinaria)
                {
                    usuarioService.Associar(usr.ID, pedidoItem.Produto.NivelAssociacao);
                }
                else
                {
                    if (ConfiguracaoHelper.GetBoolean("REDE_PREENCHIMENTO_SEQUENCIAL"))
                        usuarioService.AssociarRedeSequencia(usr.ID, pedidoItem.Produto.NivelAssociacao);
                    else
                        usuarioService.AssociarRedeHierarquia(usr.ID, pedidoItem.Produto.NivelAssociacao);
                }
            }

            if (usuarioService.Upgrade(pedidoItem.Pedido.UsuarioID, pedidoItem.Produto.NivelAssociacao))
            {
                var status = new PedidoItemStatus();

                status.Data = App.DateTimeZion;
                status.Mensagem = "";
                status.PedidoItemID = pedidoItem.ID;

                if (ConfiguracaoHelper.GetBoolean("LOJA_TEM_ENTREGA_PRODUTO"))
                    status.Status = PedidoItemStatus.TodosStatus.AguardandoEnvio;
                else
                    status.Status = PedidoItemStatus.TodosStatus.Entregue;

                pedidoItemStatusRepository.Save(status);
            }

            var pedidoPagamento = pedidoItem.Pedido.PedidoPagamento.FirstOrDefault();
            var u = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);
            var contaIDPontos = 2;
            if (pedidoPagamento.ContaID == contaIDPontos)
            {
                u.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Pontos;
            }
            else
            {
                u.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Dinheiro;
            }
            usuarioRepository.Save(u);

            /*por ser um upgrade, apaga a associacao anterior (para nao acumular) da tabela de usuarioQualificacao*/
            var associacaoAnterior = usuarioAssociacaoRepository.GetByExpression(uq => uq.UsuarioID == u.ID && uq.NivelAssociacao < pedidoItem.Produto.NivelAssociacao && u.DataValidade >= App.DateTimeZion).OrderBy(ord => ord.NivelAssociacao).FirstOrDefault();
            if (associacaoAnterior != null)
            {
                usuarioAssociacaoRepository.Delete(associacaoAnterior);
            }

            //gera registro de Associacao
            usuarioService.GeraUsuarioAssociacao(u, pedidoItem.Produto.NivelAssociacao, true);


            /*atualiza qualificacao do patrocinador */
            //qualificacaoRepository.AtualizaQualificacao(u.ID);

            /*bonus de indicacao*/
            //if (u.TipoDeAtivacao == Entities.Usuario.TodosTiposAtivacao.Dinheiro)
            //{
            //   bonificacaoRepository.GerarBonusIndicacao(pedidoItem.Pedido.UsuarioID, pedidoItem.Produto.NivelAssociacao);
            //}

        }

    }
}
