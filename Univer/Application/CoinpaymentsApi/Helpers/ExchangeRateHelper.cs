using Coinpayments.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api.Helpers
{
    public class ExchangeRateHelper
    {
        public ExchangeRateHelper(Dictionary<string, ExchangeRatesResponse.Item> result)
        {
            if (result == null)
                return;

            UsdBtc = result["USD"].RateBtc;
            BtcUsd = 1 / UsdBtc;

            var ltcBtc = result["LTC"].RateBtc;
            LtcUsd = ltcBtc / UsdBtc;
            UsdLtc = 1 / LtcUsd;

            var usdtBtc = result["USDT.TRC20"].RateBtc;
            UsdtUsd = usdtBtc / UsdBtc;
            UsdUsdt = 1 / UsdtUsd;
        }

        public decimal BtcUsd { get; set; }
        public decimal UsdBtc { get; set; }
        
        public decimal LtcUsd { get; set; }
        public decimal UsdLtc { get; set; }

        public decimal UsdtUsd { get; set; }
        public decimal UsdUsdt { get; set; }

    }
}
