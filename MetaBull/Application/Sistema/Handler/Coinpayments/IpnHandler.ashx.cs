using Coinpayments.Api.Ipns;
using Core.Entities;
using Core.Services;
using Core.Services.Loja;
using Core.Services.Financeiro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Core.Repositories.Financeiro;
using System.Data.Entity;
using System.Text;

namespace Coinpayments.IPNHandler
{
    public class IpnHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                //var ipClient = GetUserIp(Request);
                //if (ipClient != "3.232.167.191")
                //{
                //    return Request.CreateResponse(HttpStatusCode.Forbidden, "IP: " +  ipClient + " sem permissão");
                //}
                string pathLog = "";

                string transacao = context.Request.Form["ipn_type"];
                var hmac = context.Request.Headers["HMAC"];
                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"INICIO DO PROCESSAMENTO {0}", DateTime.Now.ToString("yyyyMMdd hh:mm:ss")));
                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"Transacao: {0}", transacao));

                if (transacao == "deposit")
                {
                    sb.AppendLine(string.Format(@"   Entrou em: {0}", transacao));
                    var reqDeposito = IpnBase.Parse<IpnDeposit>(context.Request.Form);
                    if (hmac == null || !reqDeposito.SigIsValid(hmac))
                    {
                        sb.AppendLine(string.Format(string.Format(@"   SigIsValid retornou false para IpnType {0}", reqDeposito.IpnType)));
                        response(context, HttpStatusCode.BadRequest, "Invalid HMAC / MerchantId");
                        //Escrever log
                        Log(sb.ToString(), "_");
                        return;
                    }
                    sb.AppendLine(string.Format(@"   SigIsValid retornou true"));
                                        
                    var contextSec = new YLEVELEntities();
                    sb.AppendLine(string.Format(@"   Obtem Pedido"));
                    PedidoService pedidoService = new PedidoService(contextSec);

                    sb.AppendLine(string.Format(@"   Obtem Pedido por carteira"));
                    //Busca Pedido referente atualização do pagamento.
                    var pedido = pedidoService.ObterPedidoPorCarteira(reqDeposito.Address);

                    sb.AppendLine(string.Format(@"   Obtem Pedido pagamento"));
                    //Busca Pagamento referente ao Pedido.
                    var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                    sb.AppendLine(string.Format(@"   Pedido {0}", pedido.ID));
                    //Para log
                    pathLog = "pedidoID_" + pedido.ID.ToString();

                    if (pagamento != null)
                    {
                        sb.AppendLine(string.Format(@"   Pagamento NOT NULL"));
                        sb.AppendLine(string.Format(@"      Carteira {0} - Transaction ID: {1}  - Data: {2}  - StatusID: {3} - Status Descricao: {4}  - Valor: {5}", reqDeposito.Address, reqDeposito.TxnId, DateTime.Now, reqDeposito.Status.ToString(), reqDeposito.StatusText, reqDeposito.FiatAmount.ToString()));

                        if (checkForDuplicateOrValid(reqDeposito, pagamento))
                        {
                            sb.AppendLine(string.Format(@"   Duplicate transactions"));
                            response(context, HttpStatusCode.OK, "Duplicate transactions");
                            //Escrever LOG
                            Log(sb.ToString(), "_");
                            return;
                        }
                        sb.AppendLine(string.Format(@"   Obtem statusNovo"));
                        var statusNovo = PedidoPagamentoStatus.TodosStatus.AguardandoPagamento;
                        if (reqDeposito.Status >= 0 && reqDeposito.Status <= 99)
                        {
                            sb.AppendLine(string.Format(@"      reqDeposito.Status >= 0 && reqDeposito.Status <= 99"));
                            statusNovo = PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao;
                        }
                        if (reqDeposito.Status == -1)
                        {
                            sb.AppendLine(string.Format(@"      reqDeposito.Status == -1"));
                            statusNovo = PedidoPagamentoStatus.TodosStatus.Cancelado;
                        }
                        if (reqDeposito.Status >= 100 || reqDeposito.Status == 2 || reqDeposito.Status == 1)
                        {
                            sb.AppendLine(string.Format(@"      reqDeposito.Status >= 100 || reqDeposito.Status == 2 || reqDeposito.Status == 1"));
                            statusNovo = PedidoPagamentoStatus.TodosStatus.PagamentoParcial;
                        }
                        sb.AppendLine(string.Format(string.Format(@"   ProcessarPagamento", pagamento.ID)));
                        bool ret = pedidoService.ProcessarPagamento(pagamento.ID, statusNovo, 0, (double)reqDeposito.FiatAmount, reqDeposito.IpnId, reqDeposito.DepositId, reqDeposito.TxnId);

                        if (ret)
                        {
                            sb.AppendLine(string.Format(@"   ProcessarPagamento OK"));
                            response(context, HttpStatusCode.OK, "1");
                        }
                        else
                        {
                            sb.AppendLine(string.Format(@"   ProcessarPagamento NOOK"));
                            sb.AppendLine(string.Format(@"      Carteira {0} - Transaction ID: {1}  - Data: {2}  - StatusID: {3} - Status Descricao: {4}  - Valor: {5}", reqDeposito.Address, reqDeposito.TxnId, DateTime.Now, reqDeposito.Status.ToString(), reqDeposito.StatusText, reqDeposito.FiatAmount.ToString()));
                            response(context, HttpStatusCode.BadRequest, "Problema no processamento do pedido");
                        }
                    }
                    else
                    {
                        sb.AppendLine(string.Format(@"   Pagamento NULL"));
                        sb.AppendLine(string.Format(@"      Carteira {0} - Transaction ID: {1}  - Data: {2}  - StatusID: {3} - Status Descricao: {4}  - Valor: {5}", reqDeposito.Address, reqDeposito.TxnId, DateTime.Now, reqDeposito.Status.ToString(), reqDeposito.StatusText, reqDeposito.FiatAmount.ToString()));
                        response(context, HttpStatusCode.BadRequest, "Id do pagamento nao encontrado");
                    }
                }
                else
                {
                    if (transacao == "withdrawal")
                    {
                        sb.AppendLine(string.Format(@"   Entrou em: {0}", transacao));
                        var reqWithdraw = IpnBase.Parse<IpnWithdraw>(context.Request.Form);
                        if (hmac == null || !reqWithdraw.SigIsValid(hmac))
                        {
                            sb.AppendLine(string.Format(string.Format(@"   SigIsValid retornou false para IpnType {0}", reqWithdraw.IpnType)));
                            response(context, HttpStatusCode.BadRequest, "Invalid HMAC / MerchantId");
                            //Escrever log
                            Log(sb.ToString(), "_");
                            return;
                        }
                        sb.AppendLine(string.Format(@"   SigIsValid retornou true"));

                        var contextSec = new YLEVELEntities();
                        sb.AppendLine(string.Format(@"   Obtem Pedido"));
                        PedidoService pedidoService = new PedidoService(contextSec);
                        
                        sb.AppendLine(string.Format(@"   Obtem splitCriptoRepository"));
                        SplitCriptoRepository splitCriptoRepository = new SplitCriptoRepository(contextSec);

                        sb.AppendLine(string.Format(@"   Obtem splitCriptoService"));
                        SplitCriptoService splitCriptoService = new SplitCriptoService(contextSec);

                        //Busca Splits referentes ao lote (id da transacao da coinpayments.
                        sb.AppendLine(string.Format(string.Format(@"   Busca Splits referentes ao lote", reqWithdraw.Id)));
                        pathLog = "idGataway_" + reqWithdraw.Id;
                        var splits = splitCriptoService.ObterSplitCriptoPorIdTransacaoCoinpayments(reqWithdraw.Id);

                        if (splits != null || splits.Count > 0)
                        {
                            sb.AppendLine(@"   splits != null || splits.Count > 0");
                            
                            YLEVELEntities db = new YLEVELEntities();
                            foreach (SplitCripto split in splits)
                            {
                                if (split.Efetivado != 1)
                                {
                                    var lancamento = new Core.Entities.SplitCripto()
                                    {
                                        ID = split.ID,
                                        PedidoID = split.PedidoID,
                                        MoedaID = split.MoedaID,
                                        Valor = split.Valor,
                                        MoedaIDCripto = split.MoedaIDCripto,
                                        ValorCripto = split.ValorCripto,
                                        CotacaoCripto = split.CotacaoCripto,
                                        Plataforma = split.Plataforma,
                                        Carteira = split.Carteira,
                                        Percentual = split.Percentual,
                                        Efetivado = reqWithdraw.Status == 2 ? 1 : 0,
                                        IdGateway = split.IdGateway,
                                        DataSolicitacaoGateway = split.DataSolicitacaoGateway,
                                        IPNID = reqWithdraw.IpnId,
                                        WithdrawID = reqWithdraw.Id,
                                        TXNID = reqWithdraw.TxnId,
                                        DataEfetivacaoGateway = DateTime.Now,
                                        StatusGateway = reqWithdraw.Status,
                                        StatusGatewayDescricao = reqWithdraw.StatusText
                                    };
                                    db.Entry(lancamento).State = EntityState.Modified;
                                    db.SaveChanges();
                                    sb.AppendLine(string.Format(@"      lancamento para o slit.ID: {0}", split.ID));
                                    
                                }
                            };
                            db.Dispose();
                            response(context, HttpStatusCode.OK, "1");
                        }
                        else
                        {
                            sb.AppendLine(string.Format(@"      Carteira {0} - Transaction ID: {1}  - Data: {2}  - StatusID: {3} - Status Descricao: {4}  - Valor: {5}", reqWithdraw.Address, reqWithdraw.TxnId, DateTime.Now, reqWithdraw.Status.ToString(), reqWithdraw.StatusText, reqWithdraw.Amount.ToString()));
                            response(context, HttpStatusCode.BadRequest, "Id da transacao da coinpayments nao encontrado");
                        }
                    }
                }

                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"FIM Transacao: {0}", transacao));
                sb.AppendLine(@"==============================================================");
                Log(sb.ToString(), pathLog);
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format(@"Erro: {0}", ex.Message));
                //Escrever log
                Log(sb.ToString(), "_");
                response(context, HttpStatusCode.BadRequest, "Error: Invalid body");
            }
        }

        private bool checkForDuplicateOrValid(IpnDeposit req, PedidoPagamento pagamento)
        {
            if (pagamento.UltimoStatus.Status == PedidoPagamentoStatus.TodosStatus.Pago || pagamento.UltimoStatus.Status == PedidoPagamentoStatus.TodosStatus.Cancelado)
            {
                return true;
            }
            else
            {
                var status = pagamento.PedidoPagamentoStatus.Where(w => w.IPNID == req.IpnId && w.TXNID == req.TxnId && w.DepositID == req.DepositId);
                if (status != null && status.Count() > 0)
                {
                    return true;
                }

            }
            return false;
        }

        private void response(HttpContext context, HttpStatusCode statusCode, string text)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/plain";
            context.Response.Write(text);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public static void Log(string texto, string strPath)
        {
            if (strPath == "_")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"FIM DO PROCESSAMENTO {0}", DateTime.Now.ToString("yyyyMMdd hh:mm:ss")));
                sb.AppendLine(@"==============================================================");
                strPath = "_erro";
            }
            string path = @"d:\logs\" + DateTime.Now.ToString("yyyyMMdd") + "_ipnHandler" + strPath + ".txt";
            path = path.Replace("_.txt", ".txt").Replace("_.txt", ".txt");

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(texto);
            }
        }

        private string GetUserIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            return null;
        }
    }
}