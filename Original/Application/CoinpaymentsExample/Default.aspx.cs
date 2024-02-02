using Coinpayments.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Coinpayments.IPNHandler
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected async void btnBuy_Click(object sender, EventArgs e)
        {
            // var purchase = await CoinpaymentsApi.CreateTransaction(12, "USD", "LTC", "um@dois.com");
            //Response.Redirect(purchase.Result.StatusUrl);

            //var exchangeRatesResponse = await CoinpaymentsApi.ExchangeRates();
            //Response.Redirect(purchase.HttpResponse.ToString());

            var ret = await CoinpaymentsApiWrapper.ExchangeRatesAsHelper();
            var cotacao = ret.BtcUsd;
            //Response.Redirect(exchangeRatesResponse.HttpResponse.ToString());

        }
    }
}