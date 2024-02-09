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
    internal class AssociacaoService : BaseService
    {

        private UsuarioService usuarioService;
        private UsuarioRepository usuarioRepository;
        private BonificacaoRepository bonificacaoRepository;
        private AssociacaoRepository associacaoRepository;
        private QualificacaoRepository qualificacaoRepository;
        private UsuarioGanhoRepository usuarioGanhoRepository;

        public AssociacaoService(DbContext context)
            : base(context)
        {
            usuarioService = new UsuarioService(context);
            usuarioRepository = new UsuarioRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            qualificacaoRepository = new QualificacaoRepository(context);
            usuarioGanhoRepository = new UsuarioGanhoRepository(context);
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
            int nivelAssociacao = 0;
            if (ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
            {
                var usuario = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);
                //Obtem valor total investido
                double total = 0.0;
                if (usuario.Pedido.Any())
                {
                    foreach (var pedido in usuario.Pedido.OrderByDescending(p => p.DataCriacao))
                    {
                        if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago)
                        {
                            total += pedido.PedidoPagamento.Where(w => w.Valor.HasValue).Sum(s => s.Valor).Value;
                        }
                    }
                }
                //Verifica em que nivel valor se encaixa
                if (total > 0)
                {
                    nivelAssociacao = 1; //1: 50 a 499
                    if (total > Convert.ToDouble(associacaoRepository.GetByNivel(1).PercentualBinario)) //499.0
                    {
                        nivelAssociacao = 2; //2: 499 a 4999
                        if (total > Convert.ToDouble(associacaoRepository.GetByNivel(2).PercentualBinario)) //4999.0
                        {
                            nivelAssociacao = 3; //3: 4999 a 49999
                            if (total > Convert.ToDouble(associacaoRepository.GetByNivel(3).PercentualBinario)) //49999.0
                            {
                                nivelAssociacao = 4;//4: acima de 49999 a 500000.00 (-1)
                                if (total >= Convert.ToDouble(associacaoRepository.GetByNivel(4).PercentualBinario)) //500000.00
                                {
                                    nivelAssociacao = 5; //5: igual ou acima de 500000.00
                                }
                            }
                        }
                    }
                }
            }

            if (nivelAssociacao == 0)
            {
                nivelAssociacao = pedidoItem.Produto.NivelAssociacao;
            }

            if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
            {
                Boolean blnArvoreBinaria = false;
                
                if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
                    blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

                if (blnArvoreBinaria)
                {
                    usuarioService.Associar(pedidoItem.Pedido.UsuarioID, nivelAssociacao);
                }
                else
                {
                    if (ConfiguracaoHelper.GetBoolean("REDE_PREENCHIMENTO_SEQUENCIAL"))
                    {
                        usuarioService.AssociarRedeSequencia(pedidoItem.Pedido.UsuarioID, nivelAssociacao);
                    }
                    else
                    {
                        if (ConfiguracaoHelper.GetBoolean("EXIBE_OU_NAO_LADO_DERRAMAMENTO"))
                        {
                            usuarioService.AssociarRedeHierarquiaComDerramamento(pedidoItem.Pedido.UsuarioID, nivelAssociacao);
                        }
                        else
                        {
                            usuarioService.AssociarRedeHierarquia(pedidoItem.Pedido.UsuarioID, nivelAssociacao);
                        }
                    }
                }
            }
            else
            {
                InvestigacaoLog.LogNivelAssociacao(pedidoItem.Pedido.UsuarioID, 12);

                // Alterar somente nível de associação
                usuarioService.AssociarUsuario(pedidoItem.Pedido.UsuarioID, nivelAssociacao);

            }

            var pedidoPagamento = pedidoItem.Pedido.PedidoPagamento.FirstOrDefault();

            var status = new PedidoItemStatus();

            status.Data = App.DateTimeZion;
            status.Mensagem = "";
            status.PedidoItemID = pedidoItem.ID;

            if (ConfiguracaoHelper.GetBoolean("LOJA_TEM_ENTREGA_PRODUTO"))
            {
                if (pedidoItem.Produto.SKU == "ADE0000")
                    status.Status = PedidoItemStatus.TodosStatus.Entregue;
                else
                    status.Status = PedidoItemStatus.TodosStatus.AguardandoEnvio;
            }
            else
            {
                status.Status = PedidoItemStatus.TodosStatus.Entregue;
            }

            pedidoItemStatusRepository.Save(status);


            //libera usuario de avaliacao / standby
            var u = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);

            // Salvar data de ativação, caso não tenha
            if (u.DataAtivacao == null)
            {
                u.DataAtivacao = App.DateTimeZion;
            }

            /*TO DO: configuracao para produto que é ADESAO GRATUITA*/
            if (pedidoItem.Produto.SKU == "ADE0000")
            {
                u.GeraBonus = false;
                u.RecebeBonus = false;
            }
            else
            {
                u.GeraBonus = true;
                u.RecebeBonus = true;
            }

            var contaIDPontos = 2;
            if (pedidoPagamento.ContaID == contaIDPontos)
            {
                u.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Pontos;
            }
            else
            {
                u.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Dinheiro;
            }

            var dataValidadeBase = DateTime.MinValue;
            if (u.Validade > App.DateTimeZion)
            {
                dataValidadeBase = u.Validade;
            }
            else
            {
                dataValidadeBase = App.DateTimeZion;
            }

            //Sp esta tratando a renovação
            //if (ConfiguracaoHelper.GetBoolean("REDE_RENOVACAO_ASSOCIACAO_POSSUI"))
            //    u.DataRenovacao = dataValidadeBase.AddDays(associacaoRepository.GetByNivel(u.NivelAssociacao).DuracaoDias);
            //else
            //    u.DataRenovacao = App.DateTimeZion.AddDays(36500);

            if (ConfiguracaoHelper.GetBoolean("REDE_ATIVO_MENSAL_POSSUI"))
                u.DataValidade = dataValidadeBase.AddDays(30);
            else
                u.DataValidade = u.DataRenovacao;

            //dataValidadeBase = dataValidadeBase.AddMonths(pedidoItem.Quantidade);
            //u.Validade = dataValidadeBase;
            usuarioRepository.Save(u);


            if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
            {
                // === Gera registro de Ganho
                UsuarioGanho usuarioGanho = new UsuarioGanho();
                usuarioGanho.UsuarioID = u.ID;
                usuarioGanho.Atual = true;
                usuarioGanho.Indicador = 0;
                usuarioGanho.AcumuladoGanho = 0;

                if (pedidoPagamento != null && pedidoPagamento.UltimoStatus != null && pedidoPagamento.UltimoStatus.Status == PedidoPagamentoStatus.TodosStatus.Pago)
                    usuarioGanho.DataInicio = pedidoPagamento.UltimoStatus.Data;
                else
                    usuarioGanho.DataInicio = (DateTime)u.DataAtivacao;

                usuarioGanho.DataFim = (DateTime)u.DataValidade;
                usuarioGanho.DataCriacao = App.DateTimeZion;
                usuarioGanho.DataAtingiuLimite = null;

                usuarioGanhoRepository.Save(usuarioGanho);
            }

            //gera registro de Associacao
            usuarioService.GeraUsuarioAssociacao(u, nivelAssociacao, false);

            /*atualiza qualificacao do patrocinador */
            /*
            qualificacaoRepository.AtualizaQualificacao(u.ID);

            if (ConfiguracaoHelper.GetBoolean("BONUS_PLUS_HABILITADO"))
            {
                qualificacaoRepository.AtualizaQualificacaoPlus(u.ID, u.NivelAssociacao);
            }
            */


            /*bonus de indicacao*/
            //if (u.TipoDeAtivacao == Entities.Usuario.TodosTiposAtivacao.Dinheiro)
            //{
            //    bonificacaoRepository.GerarBonusIndicacaoDireta(pedidoItem.Pedido.UsuarioID, pedidoItem.Pedido.ID);
            //}
        }

    }
}
