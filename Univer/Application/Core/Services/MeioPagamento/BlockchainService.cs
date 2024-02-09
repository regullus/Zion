using Core.Entities;
using Core.Helpers;
using Core.Repositories.Financeiro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services.MeioPagamento
{
    public class BlockchainService
    {
        private SaqueRepository saqueRepository;
        private SaqueStatusRepository saqueStatusRepository;
        private BlockchainRepository blockchainLogRepository;

        private string walletId = string.Empty;
        private string walletPassword1 = string.Empty;
        private string walletPassword2 = string.Empty;
        private string carteira = string.Empty;
        private string apikey = string.Empty;

        public BlockchainService(DbContext context)
        {
            this.saqueRepository = new SaqueRepository(context);
            this.saqueStatusRepository = new SaqueStatusRepository(context);
            this.blockchainLogRepository = new BlockchainRepository(context);

            //walletId = Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_WALLET_ID");
            //walletPassword1 = Core.Helpers.CriptografiaHelper.Cripto(Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_WALLET_PASSWORD1"), Core.Helpers.CriptografiaHelper.TipoCriptografia.Descriptografa);
            //walletPassword2 = Core.Helpers.CriptografiaHelper.Cripto(Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_WALLET_PASSWORD2"), Core.Helpers.CriptografiaHelper.TipoCriptografia.Descriptografa);
            //carteira = Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_FIRST_ADDRESS");
            //apikey = Core.Helpers.ConfiguracaoHelper.GetString("BLOCKCHAIN_KEY");
        }

        public BlockChainPaymentResponse Pagar(string carteira, decimal valor, string moeda, decimal feePersonalizado, int transacaoID)
        {
            var intFee = ConfigurarFee(feePersonalizado);
            try
            {
                return Transferir(carteira, valor, moeda, transacaoID, intFee);
            }
            catch (Exception ex)
            {
                Log(carteira, valor, moeda, transacaoID, intFee, ex.Message);
                return null;
            }
        }

        public BlockChainPaymentResponse Pagar(string carteira, decimal valor, decimal feePersonalizado, int transacaoID)
        {
            var intFee = ConfigurarFee(feePersonalizado);
            try
            {
                return Transferir(carteira, valor, transacaoID, intFee);
            }
            catch (Exception ex)
            {
                Log(carteira, valor, transacaoID, intFee, ex.Message);
                return null;
            }
        }

        #region Métodos Públicos Estáticos 

        public static bool ValidarCarteiraBitcoin(string carteira)
        {
            try
            {
                //Todo checar se isso esta certo
                var regex = new Regex(@"^(bc1|[13])[a-zA-HJ-NP-Z0-9]{25,39}$");
                var matches = regex.Matches(carteira);
                if (matches.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ValidarCarteiraLitecoin(string carteira)
        {
            try
            {
                //Todo checar se isso esta certo
                var regex = new Regex(@"^ltc1[a-zA-HJ-NP-Z0-9]{25,40}$");
                var matches = regex.Matches(carteira);
                if (matches.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ValidarCarteiraTether(string carteira)
        {
            try
            {
                //Todo checar se isso esta certo
                var regex = new Regex(@"^(T|[13])[A-Za-z1-9]{33}$");
                var matches = regex.Matches(carteira);
                if (matches.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool ValidarTransacao(string hash, string carteira)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var url = "https://blockchain.info/pt/q/txtotalbtcinput/{0}/{1}";
                    url = string.Format(url, hash, carteira);

                    var retorno = client.DownloadString(url);

                    return retorno.Equals("Transaction Not Found") ? false : true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string Saldo(string carteira)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var url = string.Format("https://blockchain.info/pt/address/{0}", carteira);
                    return client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar a cotação do Bitcoin", ex);
            }
        }
        #endregion

        #region Métodos Privados

        private BlockChainPaymentResponse Transferir(string carteira, decimal valor, string moeda, int transacaoID, int intFee)
        {
            using (var client = new System.Net.WebClient())
            {
                var bitcoin = CotacaoBitcoin(valor, moeda);
                var satoshi = ConverterBitcoinParaSatoshi(bitcoin);
                var fee = CotacaoBitcoin(intFee, moeda);
                var satoshiFee = ConverterBitcoinParaSatoshi(fee);
                var url = string.Empty;

                url = string.Format("http://localhost:3000/merchant/{0}/payment?password={1}&second_password={2}&to={3}&amount={4}&from={5}&fee={6}", walletId, walletPassword1, walletPassword2, carteira, satoshi, carteira, satoshiFee);

                var retornoBlockchain = client.DownloadString(url);
                var retorno = JsonConvert.DeserializeObject<BlockChainPaymentResponse>(retornoBlockchain);

                Log(carteira, valor, moeda, transacaoID, intFee, retorno.ToString());

                Thread.Sleep(3000);
                return retorno;
            }
        }

        private BlockChainPaymentResponse Transferir(string carteira, decimal valor, int transacaoID, int fee)
        {
            using (var client = new System.Net.WebClient())
            {
                var satoshi = ConverterBitcoinParaSatoshi(valor.ToString());
                var satoshiFee = ConverterBitcoinParaSatoshi(fee.ToString());
                var url = string.Empty;

                url = string.Format("http://localhost:3000/merchant/{0}/payment?password={1}&second_password={2}&to={3}&amount={4}&from={5}&fee={6}", walletId, walletPassword1, walletPassword2, carteira, satoshi, carteira, satoshiFee);

                var retornoBlockchain = client.DownloadString(url);
                var retorno = JsonConvert.DeserializeObject<BlockChainPaymentResponse>(retornoBlockchain);

                Log(carteira, valor, transacaoID, fee, retorno.ToString());

                Thread.Sleep(3000);
                return retorno;
            }
        }

        private static decimal ConverterBitcoinParaSatoshi(string bitcoin)
        {
            return decimal.Parse(bitcoin, new CultureInfo("en")) * 100000000;
        }

        private void Log(string carteira, decimal valor, string moeda, int transacaoID, int intFee, string retorno)
        {
            var blockchainLog = new BlockchainLog
            {
                TransactionID = transacaoID,
                Carteira = carteira,
                Valor = Convert.ToDouble(valor),
                Moeda = moeda,
                Fee = intFee,
                Data = App.DateTimeZion,
                retorno = "Err: " + retorno
            };
            blockchainLogRepository.Save(blockchainLog);
        }

        private void Log(string carteira, decimal valor, int transacaoID, int intFee, string retorno)
        {
            var blockchainLog = new BlockchainLog
            {
                TransactionID = transacaoID,
                Carteira = carteira,
                Valor = Convert.ToDouble(valor),
                Moeda = "BTC",
                Fee = intFee,
                Data = App.DateTimeZion,
                retorno = "Err: " + retorno
            };
            blockchainLogRepository.Save(blockchainLog);
        }

        private static int ConfigurarFee(decimal feePersonalizado)
        {
            var fee = (int)Math.Abs(feePersonalizado);

            if (fee.Equals(0))
                fee = Core.Helpers.ConfiguracaoHelper.GetInt("BLOCKCHAIN_FEE");

            //Caso fee não tenha sido informado assume o default 2
            if (fee == 0)
                fee = 2;
            else if (fee > 50) //Limita fee em 10, caso a configuração ultrapasse esse valor
                fee = 50;

            return fee;
        }

        private static string CotacaoBitcoin(decimal valor, string moeda)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var url = string.Format("https://blockchain.info/tobtc?currency={0}&value={1}", moeda, valor.ToString(new CultureInfo("en")));
                    return client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar a cotação do Bitcoin", ex);
            }
        }

        #endregion
    }

    public class BlockChainPaymentResponse
    {

        public string message { get; set; }
        public string tx_hash { get; set; }
        public string notice { get; set; }

        public override string ToString()
        {
            return string.Format("tx_hash: {0} | message: {1} | notice: {2}", tx_hash, message, notice);
        }
    }

    public class JsonBlockchainReturnObject
    {
        public List<string> to { get; set; }
        public List<string> amounts { get; set; }
        public List<string> from { get; set; }
        public string fee { get; set; }

        public string txid { get; set; }
        public string tx_hash { get; set; }
        public string message { get; set; }
        public bool success { get; set; }

        public override string ToString()
        {
            return string.Format("tx_hash: {0} | message: {1} | success: {2}", tx_hash, message, success);
        }
    }

}
