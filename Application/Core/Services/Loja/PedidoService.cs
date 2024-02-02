using Core.Proxies;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Entities;
using System.Net.Http;
using Core.Repositories.Financeiro;
using Core.Repositories.Usuario;
using Core.Repositories.Rede;
using static Core.Entities.Lancamento;
using static Core.Entities.Categoria;
using static Core.Entities.Conta;

namespace Core.Services.Loja
{
    public class PedidoService
    {

        private PedidoRepository pedidoRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private PedidoPagamentoStatusRepository pedidoPagamentoStatusRepository;
        private PedidoItemStatusRepository pedidoItemStatusRepository;
        private LancamentoRepository lancamentoRepository;
        private UsuarioComplementoRepository complementoRepository;
        private BitCoinHelper bitCoinHelper;
        private PontosBinarioRepository pontosBinarioRepository;
        private CicloRepository cicloRepository;
        private ProdutoProxy produtoProxy;
        private SplitCriptoRepository splitCriptoRepository;

        public PedidoService(DbContext context)
        {
            pedidoRepository = new PedidoRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoPagamentoStatusRepository = new PedidoPagamentoStatusRepository(context);
            pedidoItemStatusRepository = new PedidoItemStatusRepository(context);
            bitCoinHelper = new BitCoinHelper(context);
            lancamentoRepository = new LancamentoRepository(context);
            complementoRepository = new UsuarioComplementoRepository(context);
            produtoProxy = new ProdutoProxy(context);
            pontosBinarioRepository = new PontosBinarioRepository(context);
            cicloRepository = new CicloRepository(context);
            splitCriptoRepository = new SplitCriptoRepository(context);
        }

        public List<Pedido> ObterPedidos()
        {
            return this.pedidoRepository.GetByExpression(p => p.DataIntegracao == null && p.PedidoPagamento.OrderByDescending(pp => pp.ID).FirstOrDefault().PedidoPagamentoStatus.OrderByDescending(pps => pps.Data).FirstOrDefault().StatusID.Equals(3)).OrderBy(p => p.DataCriacao).ToList();
        }

        public bool ProcessarPagamento(int pagamentoID, Entities.PedidoPagamentoStatus.TodosStatus status, int AdministradorId = 0, double? valorPago = null, string IPNID = null, string DepositID = null, string TXNID = null)
        {
            bool retorno = true;
            int usuarioID = 0;
            using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
            {
                var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
                bool validacao = true;
                if (pagamento != null)
                {
                    usuarioID = pagamento.Pedido.UsuarioID;
                    InvestigacaoLog.LogNivelAssociacao(usuarioID, 999, "________________________________________");
                    InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 2,"INICIO");

                    var statusConsiderar = new List<PedidoPagamentoStatus.TodosStatus>
                    {
                        PedidoPagamentoStatus.TodosStatus.Pago,
                        PedidoPagamentoStatus.TodosStatus.PagamentoParcial
                    };

                    var totalPago = pagamento.PedidoPagamentoStatus.Where(w => w.ValorPago.HasValue && statusConsiderar.Contains(w.Status)).Sum(s => s.ValorPago.Value);
                    var novoTotalPago = totalPago;

                    if (valorPago.HasValue)
                    {
                        novoTotalPago += valorPago.Value;
                    }

                    if (novoTotalPago >= pagamento.Valor)
                    {
                        status = PedidoPagamentoStatus.TodosStatus.Pago;
                    }

                    InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 3, "Status atual: " + pagamento.UltimoStatus.Status + ", novo Status: " + status);

                    if (pagamento.UltimoStatus.Status != status || pagamento.UltimoStatus.Status == PedidoPagamentoStatus.TodosStatus.PagamentoParcial)
                    {
                        var novoStatus = new Entities.PedidoPagamentoStatus();
                        novoStatus.Data = App.DateTimeZion;
                        novoStatus.PedidoPagamentoID = pagamento.ID;
                        novoStatus.Status = status;
                        novoStatus.ValorPago = valorPago.HasValue ? valorPago.Value : 0;
                        if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
                        {
                            novoStatus.IPNID = IPNID;
                            novoStatus.DepositID = DepositID;
                            novoStatus.TXNID = TXNID;
                        }
                        if (!AdministradorId.Equals(0))
                        {
                            novoStatus.AdministradorID = AdministradorId;
                        }
                        pedidoPagamentoStatusRepository.Save(novoStatus);

                        InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 4);

                        if (status == Entities.PedidoPagamentoStatus.TodosStatus.Pago)
                        {
                            InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 5);

                            try
                            {
                                //Salva diferença de BTC
                                if (valorPago.HasValue)
                                {
                                    bitCoinHelper.AjustarValor(valorPago.Value, pagamento.PedidoID);
                                }
                                InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 6);
                            }
                            catch (Exception ex)
                            {
                                InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 7);
                                throw ex;
                            }
                            int contaID = (int)Contas.Rentabilidade;
                            int tipoID = (int)Tipos.Compra;

                            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
                            {
                                contaID = (int)Contas.Investimento;
                                tipoID = (int)Tipos.Investimento;
                            }

                            var lancamento = new Entities.Lancamento()
                            {
                                CategoriaID = (int)Categorias.Investimento,
                                TipoID = tipoID,
                                ContaID = contaID,
                                DataCriacao = App.DateTimeZion,
                                DataLancamento = App.DateTimeZion,
                                PedidoID = pagamento.PedidoID,
                                ReferenciaID = pagamento.ReferenciaID,
                                Descricao = "Package Purchase",
                                UsuarioID = pagamento.UsuarioID,
                                Valor = pagamento.Valor,
                                MoedaIDCripto = pagamento.MoedaIDCripto,
                                ValorCripto = pagamento.ValorCripto
                            };
                            lancamentoRepository.Save(lancamento);

                            InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 8);

                            LiberarPedido(pagamento.PedidoID, pagamento.Pedido.UsuarioID);

                            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
                            {
                                if (status == PedidoPagamentoStatus.TodosStatus.Pago && pagamento.MeioPagamentoID != (int)PedidoPagamento.MeiosPagamento.Gratis) // && pagamento.MeioPagamentoID == (int)Core.Entities.PedidoPagamento.MeiosPagamento.CryptoPayments)
                                {
                                    //Grava dados base para split de cripto | percentual fixo no codigo para seguranca

                                    //plataforma
                                    string chave1Plataforma = Core.Helpers.ConfiguracaoHelper.CriptoCarteira(Core.Helpers.ConfiguracaoHelper.GetCarteira("USDT"));
                                    var splitCriptoPlataforma = new Entities.SplitCripto()
                                    {
                                        PedidoID = pagamento.PedidoID,
                                        MoedaID = pagamento.MoedaID,
                                        Valor = pagamento.Valor,
                                        MoedaIDCripto = pagamento.MoedaIDCripto,
                                        ValorCripto = pagamento.ValorCripto,
                                        CotacaoCripto = pagamento.CotacaoCripto,
                                        Plataforma = 1,
                                        Carteira = chave1Plataforma,
                                        Percentual = 3.0,
                                        Efetivado = 0,
                                        IdGateway = null,
                                        DataSolicitacaoGateway = null,
                                        IPNID = null,
                                        WithdrawID = null,
                                        TXNID = null,
                                        DataEfetivacaoGateway = null,
                                        StatusGateway = null,
                                        StatusGatewayDescricao = null
                                    };
                                    splitCriptoRepository.Save(splitCriptoPlataforma);

                                    string chave1Cliente = "";
                                    
                                    //cliente caso haja carteira
                                    if (!String.IsNullOrEmpty(Core.Helpers.ConfiguracaoHelper.GetCarteiraCliente("USDT")))
                                    {
                                        chave1Cliente = Core.Helpers.ConfiguracaoHelper.CriptoCarteira(Core.Helpers.ConfiguracaoHelper.GetCarteiraCliente("USDT"));
                                        chave1Cliente = cpUtilities.Gerais.Morpho(chave1Cliente, "qGKz1GpogCHL", cpUtilities.TipoCriptografia.Criptografa);
                                    }
                                    
                                    var splitCriptoCliente = new Entities.SplitCripto()
                                    {
                                        PedidoID = pagamento.PedidoID,
                                        MoedaID = pagamento.MoedaID,
                                        Valor = pagamento.Valor,
                                        MoedaIDCripto = pagamento.MoedaIDCripto,
                                        ValorCripto = pagamento.ValorCripto,
                                        CotacaoCripto = pagamento.CotacaoCripto,
                                        Plataforma = 0,
                                        Carteira = chave1Cliente,
                                        Percentual = 97.0,
                                        Efetivado = 0,
                                        IdGateway = null,
                                        DataSolicitacaoGateway = null,
                                        IPNID = null,
                                        WithdrawID = null,
                                        TXNID = null,
                                        DataEfetivacaoGateway = null,
                                        StatusGateway = null,
                                        StatusGatewayDescricao = null
                                    };
                                    splitCriptoRepository.Save(splitCriptoCliente);

                                }

                                //Chama sp EXEC spDG_RE_GeraPontos
                                bool retPVV = cicloRepository.GeraPontosValorVariavel(pagamento.Pedido.UsuarioID, pagamento.PedidoID);
                                //Chama sp EXEC spOC_US_RedeUpline_Ciclo
                                bool retRUC = cicloRepository.RedeUplineCiclo(pagamento.Pedido.UsuarioID);
                                //chama sp GeraBonusVenda_V2
                                bool retGBV = cicloRepository.GeraBonusVenda(pagamento.Pedido.UsuarioID, pagamento.PedidoID);
                                validacao = retPVV && retRUC && retGBV;
                            }
                        }
                    }
                }
                
                try
                {
                    if (validacao)
                    {
                        transacao.Complete();
                    }
                    else
                    {
                        retorno = false;
                        transacao.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    retorno = false;
                    InvestigacaoLog.LogNivelAssociacao(pagamento.Pedido.UsuarioID, 9);
                    cpUtilities.LoggerHelper.WriteFile("ProcessarPagamento : " + ex.Message, "PedidoService");
                }
            }
            InvestigacaoLog.LogNivelAssociacao(usuarioID, 999, "FIM");
            InvestigacaoLog.LogNivelAssociacao(usuarioID, 999, "________________________________________");
            return retorno;
        }

        public void ExpirarAguardandoPagamento(int pedidoId)
        {
            var pedido = this.pedidoRepository.GetByExpression(p => p.ID == pedidoId).FirstOrDefault();

            if (pedido != null && pedido.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();

                if (pagamento != null)
                {
                    var novoStatus = new Entities.PedidoPagamentoStatus()
                    {
                        Data = App.DateTimeZion,
                        PedidoPagamentoID = pagamento.ID,
                        Status = Core.Entities.PedidoPagamentoStatus.TodosStatus.Expirado,
                    };
                    pedidoPagamentoStatusRepository.Save(novoStatus);
                }

                var item = pedido.PedidoItem.FirstOrDefault();

                if (item != null)
                {
                    var novoStatus = new PedidoItemStatus()
                    {
                        PedidoItemID = item.ID,
                        Mensagem = "Expired",
                        Status = Core.Entities.PedidoItemStatus.TodosStatus.Cancelado,
                        Data = App.DateTimeZion,
                    };

                    pedidoItemStatusRepository.Save(novoStatus);
                }
            }
        }

        public void Cancelar(int pagamentoID, int? administrador)
        {
            using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
            {
                var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
                if (pagamento != null)
                {
                    if (pagamento.UltimoStatus.Status != Core.Entities.PedidoPagamentoStatus.TodosStatus.Cancelado)
                    {
                        var novoStatus = new Entities.PedidoPagamentoStatus()
                        {
                            Data = App.DateTimeZion,
                            PedidoPagamentoID = pagamento.ID,
                            Status = Core.Entities.PedidoPagamentoStatus.TodosStatus.Cancelado,
                            AdministradorID = administrador
                        };
                        pedidoPagamentoStatusRepository.Save(novoStatus);
                    }
                }
                transacao.Complete();
            }
        }

        private void LiberarPedido(int pedidoID, int UsuarioID)
        {

            InvestigacaoLog.LogNivelAssociacao(UsuarioID, 100);

            var pedido = pedidoRepository.Get(pedidoID);

            InvestigacaoLog.LogNivelAssociacao(UsuarioID, 101, pedido != null ? "Achou Pedido: pedidoID: " + pedidoID + ", StatusAtual: " + pedido.StatusAtual : "Não achou pedido");

            if (pedido.StatusAtual == Entities.PedidoPagamentoStatus.TodosStatus.Pago)
            {
                foreach (var item in pedido.PedidoItem)
                {
                    InvestigacaoLog.LogNivelAssociacao(pedido.UsuarioID, 9, "Item Status: " + item.UltimoStatus.Status);

                    if (item.UltimoStatus.Status == Entities.PedidoItemStatus.TodosStatus.AguardandoPagamento)
                    {
                        produtoProxy.Liberar(item);
                    }
                }
            }
        }

        public void PedidoEnviado(List<Pedido> pedidos)
        {
            pedidos.ForEach(p =>
            {
                p.DataIntegracao = App.DateTimeZion;
                this.pedidoRepository.Save(p);
            });
        }

        public void PedidoEnviado(Pedido pedido)
        {
            pedido.DataIntegracao = App.DateTimeZion;
            this.pedidoRepository.Save(pedido);
        }

        public void PedidoCancelarEnvio(Pedido pedido)
        {
            pedido.DataIntegracao = null;
            this.pedidoRepository.Save(pedido);
        }

        public Pedido ObterPedidoPorId(int id)
        {
            return this.pedidoRepository.Get(id);
        }

        public Pedido ObterPedidoPorCodigo(string codigo)
        {
            return this.pedidoRepository.GetByExpression(p => p.Codigo == codigo).FirstOrDefault();
        }

        public Pedido ObterPedidoPorPedidoPagamentoID(int pedidoPagamentoID)
        {
            Pedido pedido = null;
            var pedidoPagamento = pedidoPagamentoRepository.Get(pedidoPagamentoID);

            if (pedidoPagamento != null)
                return pedidoRepository.Get(pedidoPagamento.PedidoID);
            else
                return pedido;
        }

        public Pedido ObterPedidoPorCarteira(string carteira)
        {
            Pedido pedido = null;

            var pedidoPagamento = this.pedidoPagamentoRepository.GetByExpression(p => p.Numero == carteira).First();

            if (pedidoPagamento != null)
                return pedidoRepository.Get(pedidoPagamento.PedidoID);
            else
                return pedido;
        }
    }
}
