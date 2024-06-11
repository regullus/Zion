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
    // extension of CoinpaymentsApi... addition of useful methods
    public class CoinpaymentsApiWrapper
    {
        public static async Task<ExchangeRateHelper> ExchangeRatesAsHelper()
        {
            try
            {
                var exchangeRates = await CoinpaymentsApi.ExchangeRates(accepted: 2);
                if (!exchangeRates.IsSuccess)
                    throw new Exception("Coinpayments error: " + exchangeRates.Error);

                var helper = new ExchangeRateHelper(exchangeRates.Result);
                return helper;
            }
            catch (Exception ex)
            {
                String erro = ex.Message;
                var erro2 = ex.InnerException;
                return null;
            }
        }
    }
}
