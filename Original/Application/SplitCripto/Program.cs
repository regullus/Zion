using Core.Entities;
using Core.Repositories.Loja;
using Core.Repositories.Financeiro;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coinpayments.Api;
using Coinpayments.Api.Models;
using System.Configuration;

namespace SplitCript
{
    class Program
    {
        
        public static string SBLog { get; set; }

        public static string SBLogId { get; set; }

        static async Task Main(string[] args)
        {
            string ambiente = "prod";
            bool exibeConsole = false;

            if (ConfigurationManager.AppSettings["exibeConsole"] == "true")
            {
                exibeConsole = true;
            }

            StringBuilder sb = new StringBuilder();

            decimal saldoTRX = 0;
            decimal saldoBTC = 0;
            decimal saldoLTC = 0;
            decimal saldoUSDT = 0;
            string strIdPedidoPagamento = "";

            try
            {
                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"INICIO DO PROCESSAMENTO {0}", DateTime.Now.ToString("yyyyMMdd hh:mm:ss")));
                sb.AppendLine(@"==============================================================");

                if (exibeConsole) Console.WriteLine("==============================================================");
                if (exibeConsole) Console.WriteLine("Split de valores de criptomoedas (Plataforma e Cliente)");
                if (exibeConsole) Console.WriteLine("==============================================================");

                sb.AppendLine(@"Obter saldos das moedas");
                sb.AppendLine(@"==============================================================");

                if (exibeConsole) Console.WriteLine("Obter saldos das moedas");
                if (exibeConsole) Console.WriteLine("==============================================================");

                var balances = await CoinpaymentsApi.CoinBalances(0);

                foreach (var balance in balances.Result)
                {
                    if (balance.Value.Status == "available" && balance.Value.CoinStatus == "online")
                    {
                        switch (balance.Key.ToUpper())
                        {
                            case "BTC":
                                saldoBTC = balance.Value.BalanceFloat;
                                break;
                            case "LTC":
                                saldoLTC = balance.Value.BalanceFloat;
                                break;
                            case "USDT.TRC20":
                                saldoUSDT = balance.Value.BalanceFloat;
                                break;
                            case "TRX":
                                saldoTRX = balance.Value.BalanceFloat;
                                break;
                        }
                        sb.AppendLine(string.Format(@"Moeda: {0} Saldo: {1}", balance.Key, balance.Value.BalanceFloat.ToString()));
                        if (exibeConsole) Console.WriteLine("Moeda: {0} Saldo: {1}", balance.Key, balance.Value.BalanceFloat.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format(@"Erro ao chamar api da coinpaymnets para obter os saldos das moedas: {0}", ex.Message));
                if (exibeConsole) Console.WriteLine("Erro ao chamar api da coinpaymnets para obter os saldos das moedas: {0}", ex);
            }

            decimal minimoBTC = 0;
            decimal minimoLTC = 0;
            decimal minimoTRX = 0;
            try
            {
                sb.AppendLine(@"Verificar se tem valor minimo para conversao");
                sb.AppendLine(@"==============================================================");
                if (exibeConsole) Console.WriteLine("Verificar se tem valor minimo para conversao");
                if (exibeConsole) Console.WriteLine("==============================================================");

                var limitsBTC = await CoinpaymentsApi.ConversionLimits("BTC", "USDT.TRC20");
                minimoBTC = limitsBTC.Result.Min;
                sb.AppendLine(string.Format(@"Moeda: {0} Minimo: {1}", "BTC", minimoBTC.ToString()));
                if (exibeConsole) Console.WriteLine("Moeda: {0} Minimo: {1}", "BTC", minimoBTC.ToString());

                var limitsLTC = await CoinpaymentsApi.ConversionLimits("LTC", "USDT.TRC20");
                minimoLTC = limitsLTC.Result.Min;
                sb.AppendLine(string.Format(@"Moeda: {0} Minimo: {1}", "LTC", minimoLTC.ToString()));
                if (exibeConsole) Console.WriteLine("Moeda: {0} Minimo: {1}", "LTC", minimoLTC.ToString());

                var limitsTRX = await CoinpaymentsApi.ConversionLimits("TRX", "USDT.TRC20");
                minimoTRX = limitsTRX.Result.Min;
                sb.AppendLine(string.Format(@"Moeda: {0} Minimo: {1}", "TRX", minimoTRX.ToString()));
                if (exibeConsole) Console.WriteLine("Moeda: {0} Minimo: {1}", "TRX", minimoTRX.ToString());

            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format(@"Erro ao chamar api da coinpaymnets para obter os valores minimos para conversao: {0}", ex.Message));
                if (exibeConsole) Console.WriteLine("Erro ao chamar api da coinpaymnets para obter os valores minimos para conversao: {0}", ex);
            }

            try
            {
                sb.AppendLine(@"Converter 50 USDT.TRC20 para TRX se houver saldo minimo e se o valor estiver com saldo baixo");
                sb.AppendLine(@"==============================================================");
                if (exibeConsole) Console.WriteLine("Converter 50 USDT.TRC20 para TRX se houver saldo minimo e se o valor estiver com saldo baixo");
                if (exibeConsole) Console.WriteLine("==============================================================");

                decimal valorMinimoCarteiraTRX = 1000;
                decimal valorUsdtParaConversao = 50;

                if (saldoTRX < valorMinimoCarteiraTRX && saldoUSDT >= valorUsdtParaConversao)
                {
                    var retornoConvert = await CoinpaymentsApi.ConvertCoin(valorUsdtParaConversao, "USDT.TRC20", "TRX");
                    sb.AppendLine(string.Format(@"Solicitacao de conversao realizada de USDT.TRC20 {0} para TRX --> ID Coinpayments: {1}", valorUsdtParaConversao, retornoConvert));
                    if (exibeConsole) Console.WriteLine("Solicitacao de conversao realizada de USDT.TRC20 {0} para TRX --> ID Coinpayments: {1}", valorUsdtParaConversao, retornoConvert);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(@"");
                sb.AppendLine(string.Format(@"Erro ao chamar api da coinpaymnets para conversao de USDT.TRC20 para TRX: {0}", ex.Message));
                if (exibeConsole) Console.WriteLine("Erro ao chamar api da coinpaymnets para conversao de USDT.TRC20 para TRX: {0}", ex);
            }

            try
            {
                sb.AppendLine(@"Converter para USDT se houver saldo minimo");
                sb.AppendLine(@"==============================================================");
                if (exibeConsole) Console.WriteLine("Converter para USDT se houver saldo minimo");
                if (exibeConsole) Console.WriteLine("==============================================================");

                if (saldoBTC >= minimoBTC)
                {
                    var retornoConvert = await CoinpaymentsApi.ConvertCoin(saldoBTC, "BTC", "USDT.TRC20");
                    sb.AppendLine(string.Format(@"Solicitacao de consersao realizada de BTC {0} para USDT.TRC20 --> ID Coinpayments: {1}", saldoBTC.ToString(), retornoConvert));
                    if (exibeConsole) Console.WriteLine("Solicitacao de consersao realizada de BTC {0} para USDT.TRC20 --> ID Coinpayments: {1}", saldoBTC.ToString(), retornoConvert);
                }
                if (saldoLTC >= minimoLTC)
                {
                    var retornoConvert = await CoinpaymentsApi.ConvertCoin(saldoLTC, "LTC", "USDT.TRC20");
                    sb.AppendLine(string.Format(@"Solicitacao de conversao realizada de LTC {0} para USDT.TRC20 --> ID Coinpayments: {1}", saldoBTC.ToString(), retornoConvert));
                    if (exibeConsole) Console.WriteLine("Solicitacao de conversao realizada de LTC {0} para USDT.TRC20 --> ID Coinpayments: {1}", saldoBTC.ToString(), retornoConvert);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format(@"Erro ao chamar api da coinpaymnets para conversao das moedas para USDT.TRC20: {0}", ex.Message));
                if (exibeConsole) Console.WriteLine("Erro ao chamar api da coinpaymnets para conversao das moedas para USDT.TRC20: {0}", ex);
            }

            sb.AppendLine(string.Format(@"Saldo USDT.TRC20: {0}", saldoUSDT));
            if (exibeConsole) Console.WriteLine("Saldo USDT.TRC20: {0}", saldoUSDT);

            saldoUSDT = await RetiradaAsync(saldoUSDT, "Plataforma", ambiente);
            sb.AppendLine(SBLog);
            strIdPedidoPagamento = SBLogId;
            sb.AppendLine(string.Format(@"Saldo apos Plataforma USDT.TRC20: {0}", saldoUSDT));
            if (exibeConsole) Console.WriteLine("Saldo USDT.TRC20: {0}", saldoUSDT);

            saldoUSDT = await RetiradaAsync(saldoUSDT, "Cliente", ambiente);
            sb.AppendLine(SBLog);
            strIdPedidoPagamento += "_" + SBLogId;
            sb.AppendLine(string.Format(@"Saldo apos Cliente USDT.TRC20: {0}", saldoUSDT));
            if (exibeConsole) Console.WriteLine("Saldo USDT.TRC20: {0}", saldoUSDT);

            sb.AppendLine(@"==============================================================");
            sb.AppendLine(string.Format(@"FIM DO PROCESSAMENTO {0}", DateTime.Now.ToString("yyyyMMdd hh:mm:ss")));
            sb.AppendLine(@"==============================================================");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");
            if (exibeConsole) Console.WriteLine("==============================================================");
            if (exibeConsole) Console.WriteLine("FIM DO PROCESSAMENTO");
            if (exibeConsole) Console.WriteLine("==============================================================");

            Log(sb.ToString(), strIdPedidoPagamento);

            if (ambiente == "dev")
            {
                if (exibeConsole) Console.ReadKey();
            }
        }

        private static async Task<decimal> RetiradaAsync(decimal saldoUSDT, string tipo, string ambiente)
        {
            SBLog = "";
            SBLogId = "";
            string moedaCarteiraRetirada = "USDT";
            string moedaRetirada = "USDT.TRC20";
            string urlRetornoAPN = "";    //prod enviar sem endereco para pegar a cadastrada na coinpayments
            StringBuilder sb = new StringBuilder();
            bool exibeConsole = false;

            if (ConfigurationManager.AppSettings["exibeConsole"] == "true")
            {
                exibeConsole = true;
            }

            if (ambiente == "dev")
            {
                urlRetornoAPN = "http://ec2-18-209-14-7.compute-1.amazonaws.com/Handler/Coinpayments/IpnHandler.ashx";
            }

            try
            {
                sb.AppendLine(@"==============================================================");
                sb.AppendLine(string.Format(@"****** Inicio Retirada {0} ******",tipo));
                if (exibeConsole) Console.WriteLine("==============================================================");
                if (exibeConsole) Console.WriteLine(string.Format(@"****** Inicio Retirada {0} ******", tipo));
                
                string carteira = "";
                //saque somente se tiver carteira cadastrada
                if (tipo == "Plataforma")
                {
                    carteira = Core.Helpers.ConfiguracaoHelper.GetCarteira(moedaCarteiraRetirada);
                }
                else
                {
                    carteira = Core.Helpers.ConfiguracaoHelper.GetCarteiraCliente(moedaCarteiraRetirada);
                }
                if (carteira == "")
                {
                    sb.AppendLine(string.Format(@"Sem cartera configurada para {0} . Retirada nao realizada para {1}.",tipo,tipo));
                    if (exibeConsole) Console.WriteLine("Sem cartera configurada para " + tipo + ". Retirada nao realizada para " + tipo + " .");
                    return saldoUSDT;
                }
                else
                {
                    if (saldoUSDT > 0)
                    {
                        sb.AppendLine(string.Format(@"Obtem pedidos e carteira {0}",tipo));
                        if (exibeConsole) Console.WriteLine("Obtem pedidos e carteira " + tipo);
                        var context = new YLEVELEntities();
                        SplitCriptoRepository splitCriptoRepository = new SplitCriptoRepository(context);
                        //status withdrawal(-1 = Cancelled, 0 = Waiting for email confirmation, 1 = Pending, 2 = Complete).
                        var pedidosParaSplit = splitCriptoRepository.GetAll().Where(x => x.Plataforma == (tipo == "Plataforma" ? 1 : 0) && x.Efetivado == 0 && x.IdGateway == null && (x.StatusGateway == null || x.StatusGateway < 0) && x.ValorCripto != null && x.ValorCripto > 0).ToList();
            
                        decimal valorRetirada = 0;
                        List<int> idsSplitCriptoLote = new List<int>();
                        string idsSplitCriptoLoteString = "_Pedidos_";
                        foreach (SplitCripto pedido in pedidosParaSplit)
                        {
                            if (valorRetirada <= saldoUSDT)
                            {
                                idsSplitCriptoLote.Add(pedido.ID);

                                if (pedido.Percentual == null || pedido.Percentual == 0)
                                {
                                    pedido.Percentual = 3;
                                }
                                valorRetirada += (decimal)(pedido.Valor * (pedido.Percentual / 100));
                                idsSplitCriptoLoteString += pedido.ID.ToString() + "_";
                            }
                        };

                        if (valorRetirada > 0)
                        {
                            SBLogId = tipo + "_" + idsSplitCriptoLoteString;

                            string pedidosRetirada = "Retirada_" + (tipo == "Plataforma" ? "comissao" : "") + "_da_" + tipo + "_" + idsSplitCriptoLoteString;
                            //status = 0 or 1. 0 = Withdrawal created, waiting for email confirmation. 1 = Withdrawal created with no email confirmation needed.
                            var retornoConvert = await CoinpaymentsApi.CreateWithdrawal(valorRetirada, "1", moedaRetirada, carteira, urlRetornoAPN, "1", pedidosRetirada);
                         

                            if (retornoConvert.Result != null)
                            {
                                sb.AppendLine(string.Format(@"Retirada solicitada: USDT.TRC20 {0} | Status retornado {1}", valorRetirada.ToString(), retornoConvert.Result.Id));
                                if (exibeConsole) Console.WriteLine("Retirada solicitada: USDT.TRC20 {0} | Status retornado {1}", valorRetirada.ToString(), retornoConvert.Result.Id);
                                if (retornoConvert.Result.Status > 0)
                                {
                                    YLEVELEntities db = new YLEVELEntities();
                                    sb.AppendLine(@"Gravar retorno na tabela SplitCripto");
                                    if (exibeConsole) Console.WriteLine("Gravar retorno na tabela SplitCripto");

                                    foreach (int pedido in idsSplitCriptoLote)
                                    {
                                        var registro = splitCriptoRepository.Get(pedido);

                                        var lancamento = new Core.Entities.SplitCripto()
                                        {
                                            ID = pedido,
                                            PedidoID = registro.PedidoID,
                                            MoedaID = registro.MoedaID,
                                            Valor = registro.Valor,
                                            MoedaIDCripto = registro.MoedaIDCripto,
                                            ValorCripto = registro.ValorCripto,
                                            CotacaoCripto = registro.CotacaoCripto,
                                            Plataforma = registro.Plataforma,
                                            Carteira = registro.Carteira,
                                            Percentual = registro.Percentual,
                                            Efetivado = registro.Efetivado,
                                            IdGateway = retornoConvert.Result.Id,
                                            DataSolicitacaoGateway = DateTime.Now,
                                            IPNID = null,
                                            WithdrawID = null,
                                            TXNID = null,
                                            DataEfetivacaoGateway = null,
                                            StatusGateway = null,
                                            StatusGatewayDescricao = null
                                        };
                                        db.Entry(lancamento).State = EntityState.Modified;
                                        db.SaveChanges();
                                        sb.AppendLine(string.Format(@"   Gravado pedido: {0}", pedido));
                                        if (exibeConsole) Console.WriteLine(string.Format(@"   Gravado pedido: {0}", pedido));
                                    };
                                    db.Dispose();
                                    saldoUSDT -= valorRetirada;
                                }
                            }
                        }
                    }
                    sb.AppendLine(@"==============================================================");
                    sb.AppendLine(string.Format(@"****** FIM Retirada {0} ******", tipo));
                    if (exibeConsole) Console.WriteLine("==============================================================");
                    if (exibeConsole) Console.WriteLine(string.Format(@"****** FIM Retirada {0} ******", tipo));

                    SBLog = sb.ToString();

                    return saldoUSDT;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("");
                if (exibeConsole) Console.WriteLine("Erro ao chamar api da coinpaymnets solicitando retirada para " + tipo + ": {0}", ex);
                return saldoUSDT;
            }
        }
        
        public static void Log(string texto, string strPath)
        {
            if (strPath == "_")
            {
                strPath = "_semTransacao";
            }
            string path = @"d:\logs\"+ DateTime.Now.ToString("yyyyMMdd") + "_robo" + strPath + ".txt";
            path = path.Replace("_.txt", ".txt").Replace("_.txt", ".txt");

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(texto);
            }
        }
    
    }
}
