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
    internal class RenovacaoAssinaturaService : BaseService
    {

        private UsuarioRepository usuarioRepository;
        private BonificacaoRepository bonificacaoRepository;
        private AssociacaoRepository associacaoRepository;
        private UsuarioGanhoRepository usuarioGanhoRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;

        public RenovacaoAssinaturaService(DbContext context)
              : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            usuarioGanhoRepository = new UsuarioGanhoRepository(context);
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
            var usuario = usuarioRepository.Get(pedidoItem.Pedido.UsuarioID);

            var dataValidadeBase = DateTime.MinValue;

            if (usuario.DataRenovacao > App.DateTimeZion)
            {
                dataValidadeBase = usuario.Validade;
            }
            else
            {
                dataValidadeBase = App.DateTimeZion;
            }
            int nivelAssociacao = usuario.NivelAssociacao;
            if (ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
            {
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

            //Sp esta tratando renovação
            //if (ConfiguracaoHelper.GetBoolean("REDE_RENOVACAO_ASSOCIACAO_POSSUI"))
            //    usuario.DataRenovacao = dataValidadeBase.AddDays(associacaoRepository.GetByNivel(nivelAssociacao).DuracaoDias);
            //else
            //    usuario.DataRenovacao = App.DateTimeZion.AddDays(36500);

            //Sp esta tratando renovacao
            //if (ConfiguracaoHelper.GetBoolean("REDE_ATIVO_MENSAL_POSSUI"))
            //    usuario.DataValidade = dataValidadeBase.AddDays(30);
            //else
            //    usuario.DataValidade = usuario.DataRenovacao;

            //dataValidadeBase = dataValidadeBase.AddMonths(pedidoItem.Quantidade);
            //usuario.Validade = dataValidadeBase;

            var status = new PedidoItemStatus();

            status.Data = App.DateTimeZion;
            status.Mensagem = "";
            status.PedidoItemID = pedidoItem.ID;

            if (ConfiguracaoHelper.GetBoolean("LOJA_TEM_ENTREGA_PRODUTO"))
                status.Status = PedidoItemStatus.TodosStatus.AguardandoEnvio;
            else
                status.Status = PedidoItemStatus.TodosStatus.Entregue;

            pedidoItemStatusRepository.Save(status);

            var pedidoPagamento = pedidoItem.Pedido.PedidoPagamento.FirstOrDefault();
            var contaIDPontos = 2;
            if (pedidoPagamento.ContaID == contaIDPontos)
            {
                usuario.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Pontos;
            }
            else
            {
                usuario.TipoDeAtivacao = Entities.Usuario.TodosTiposAtivacao.Dinheiro;
            }

            if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
            {
                usuario.RecebeBonus = true;
            }

            usuarioRepository.Save(usuario);

            // === Gera registro de Ganho
            if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
            {
                UsuarioGanho usuarioGanho = usuarioGanhoRepository.GetAtual(usuario.ID);

                int indicador = usuarioGanho.Indicador;

                usuarioGanho.Atual = false;
                usuarioGanho.DataFim = pedidoPagamento.UltimoStatus.Data.AddSeconds(-10); //  App.DateTimeZion;
                usuarioGanhoRepository.Save(usuarioGanho);

                usuarioGanho = new UsuarioGanho();
                usuarioGanho.UsuarioID = usuario.ID;
                usuarioGanho.Atual = true;
                usuarioGanho.Indicador = indicador + 1;
                usuarioGanho.AcumuladoGanho = 0;
                usuarioGanho.DataInicio = pedidoPagamento.UltimoStatus.Data; // App.DateTimeZion;
                usuarioGanho.DataFim = (DateTime)usuario.DataValidade;
                usuarioGanho.DataCriacao = App.DateTimeZion;
                usuarioGanho.DataAtingiuLimite = null;
                usuarioGanhoRepository.Save(usuarioGanho);
            }

            //gera registro de Associacao
            foreach (var associacao in usuarioAssociacaoRepository.GetByExpression(a => a.UsuarioID == usuario.ID).ToList())
            {
                associacao.DataValidade = usuario.DataRenovacao;
                usuarioAssociacaoRepository.Save(associacao);
            };


            /*bonus de indicacao*/
            //if (usuario.TipoDeAtivacao == Entities.Usuario.TodosTiposAtivacao.Dinheiro)
            //{
            //   bonificacaoRepository.GerarBonusIndicacao(pedidoItem.Pedido.UsuarioID, pedidoItem.Pedido.Usuario.NivelAssociacao);
            //}

        }
    }
}
