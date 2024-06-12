using Coinpayments.Api.Helpers;
using Coinpayments.Api.Models;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api
{
    public class CoinpaymentsApi
    {
        public static Task<GetCallbackAddressResponse> GetCallbackAddress(string currency)
        {
            var req = new HttpUrlRequest(new {
                cmd = "get_callback_address",
                currency 
            });

            return process<GetCallbackAddressResponse>(req);
        }

        public static Task<CreateTransactionResponse> CreateTransaction(
            decimal amount, string currency1, string currency2, string buyerEmail, 
            string custom = null, string itemNumber = null)
        {
            var req = new CreateTransactionRequest
            {
                Amount = amount,
                Currency1 = currency1,
                Currency2 = currency2,
                BuyerEmail = buyerEmail,
                Custom = custom,
                ItemNumber = itemNumber
            };

            return CreateTransaction(req);
        }

        public static Task<ConvertLimitsResponse> ConversionLimits(string from, string to)
        {
            var req = new ConvertLimitsRequest
            {
                From = from,
                To = to
            };

            return ConversionLimits(req);
        }

        public static Task<ConvertLimitsResponse> ConversionLimits(ConvertLimitsRequest request)
        {
            var req = new HttpUrlRequest(request);
            return process<ConvertLimitsResponse>(req);
        }

        public static Task<ConvertCoinsResponse> ConvertCoin(decimal amount, string from, string to)
        {
            var req = new ConvertCoinsRequest
            {
                Amount = amount,
                Currency = from,
                Currency2 = to
            };

            return ConvertCoin(req);
        }

        public static Task<ConvertCoinsResponse> ConvertCoin(ConvertCoinsRequest request)
        {
            var req = new HttpUrlRequest(request);
            return process<ConvertCoinsResponse>(req);
        }

        public static Task<CreateTransactionResponse> CreateTransaction(CreateTransactionRequest request)
        {
            var req = new HttpUrlRequest(request);
            return process<CreateTransactionResponse>(req);
        }

        public static Task<CreateWithdrawalResponse> CreateWithdrawal(decimal amount, string add_tx_fee, string currency, string address, string ipn_url, string auto_confirm, string note)
        {
            var req = new CreateWithdrawalRequest
            {
                Amount = amount,
                Add_Tx_Fee = add_tx_fee,
                Currency = currency,
                Address = address,
                IpnUrl = ipn_url,
                AutoConfirm = auto_confirm,
                Note = note
            };

            return CreateWithdrawal(req);
        }

        internal static Task<CreateWithdrawalResponse> CreateWithdrawal(CreateWithdrawalRequest request)
        {
            var req = new HttpUrlRequest(request);
            return process<CreateWithdrawalResponse>(req);
        }

        public static Task<CreateMassWithdrawalResponse> CreateMassWithdrawal(CreateMassWithdrawalRequest request)
        {
            var req = new HttpUrlRequest(request);
            return process<CreateMassWithdrawalResponse>(req);
        }

        public static Task<ExchangeRatesResponse> ExchangeRates(int isshort = 0, int accepted = 2)
        {
            var request = new ExchangeRatesRequest
            {
                Short = isshort,
                Accepted = accepted
            };

            var req = new HttpUrlRequest(request);
            return process<ExchangeRatesResponse>(req);
        }

        public static Task<GetWithdrawalInfoResponse> GetWithdrawalInfo(string txnId)
        {
            var request = new GetWithdrawalInfoRequest { Id = txnId };
            var req = new HttpUrlRequest(request);
            return process<GetWithdrawalInfoResponse>(req);
        }

        public static Task<CoinBalancesResponse> CoinBalances(int all = 0)
        {
            var request = new CoinBalancesRequest
            {
                All = all
            };

            var req = new HttpUrlRequest(request);
            return process<CoinBalancesResponse>(req);
        }

        private static async Task<T1> process<T1>(HttpUrlRequest request)
            where T1 : ResponseModel, new()
        {
            var response = await HttpUrlCaller.GetResponse(request);

            var result = new T1();
            result.HttpResponse = response;
            result.ProcessJson();

            return result;
        }
    }
}
