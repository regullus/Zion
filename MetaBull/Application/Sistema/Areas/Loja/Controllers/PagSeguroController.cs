using Core.Entities;
using Core.Repositories.Financeiro;
using Core.Services.Loja;
using Core.Services.MeioPagamento;
using System.Data.Entity;
using System.Web.Mvc;

namespace Sistema.Areas.Loja.Controllers
{
    public class PagSeguroController : Controller
    {
        private PedidoService pedidoService;
        private CartaoCreditoRepository cartaoCreditoRepository;
        private CartaoCreditoService cartaoCreditoService;

        public PagSeguroController(DbContext context) 
        {
            pedidoService = new PedidoService(context);
            cartaoCreditoService = new CartaoCreditoService(context);
            cartaoCreditoRepository = new CartaoCreditoRepository(context);
        }

        [AllowAnonymous]
        public void Notificacao(string notificationCode, string notificationType)
        {
            if (!string.IsNullOrEmpty(notificationCode) && !string.IsNullOrEmpty(notificationType))
            {
                if (notificationType == "transaction")
                {
                    //Busca dados da transação na API da PagSeguro
                    var retorno = Integracao.PagSeguro.CheckNotificationTransaction(notificationCode);

                    if (retorno != null)
                    {
                        //Busca registro do pagamento de cartão de crédito
                        var pagtoCartao = cartaoCreditoService.ObterPagamentoPorTransacaoID(retorno.Code);

                        if (pagtoCartao != null)
                        {
                            //Busca dados do Pedido
                            var pedido = pedidoService.ObterPedidoPorPedidoPagamentoID(pagtoCartao.PedidoPagamentoID.Value);

                            switch (retorno.TransactionStatus)
                            {
                                //Aguardando Pagamento
                                case 1:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.AguardandoPagamento);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Aguardando Pagamento";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Em análise
                                case 2:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.AguardandoPagamento);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Em análise";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Paga
                                case 3:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.Pago);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Pagamento OK";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Disponível
                                case 4:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.Pago);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Disponível";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Em disputa
                                case 5:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.AguardandoPagamento);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Em disputa";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Devolvida
                                case 6:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.Cancelado);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Devolvida";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Cancelada
                                case 7:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.Cancelado);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Cancelada";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                                //Debitado e Retenção temporária
                                default:
                                    //Altera status de pagamento do pedido
                                    pedidoService.ProcessarPagamento(pagtoCartao.PedidoPagamentoID.Value, PedidoPagamentoStatus.TodosStatus.AguardandoPagamento);
                                    //Altera dados do registro de cartão de crédito
                                    pagtoCartao.MensagemRetorno = "PagSeguro - Debitado ou Retenção temporária";
                                    cartaoCreditoRepository.Save(pagtoCartao);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
