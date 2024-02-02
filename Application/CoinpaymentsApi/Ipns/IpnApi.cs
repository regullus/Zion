﻿using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api.Ipns
{
    [DataContract]
    public class IpnApi : IpnBase
    {
        [DataMember(Name = "status")]
        public int Status { get; set; }
        [DataMember(Name = "status_text")]
        public string StatusText { get; set; }
        [DataMember(Name = "txn_id")]
        public string TxnId { get; set; }
        [DataMember(Name = "currency1")]
        public string Currency1 { get; set; }
        [DataMember(Name = "currency2")]
        public string Currency2 { get; set; }
        [DataMember(Name = "amount1")]
        public decimal Amount1 { get; set; }
        [DataMember(Name = "amount2")]
        public decimal Amount2 { get; set; }
        [DataMember(Name = "fee")]
        public string Fee { get; set; } // comes in shitty exponential format like 2.0E-5
        [DataMember(Name = "net")]
        public decimal Net { get; set; }
        [DataMember(Name = "buyer_name")]
        public string BuyerName { get; set; }
        [DataMember(Name = "item_name")]
        public string ItemName { get; set; }
        [DataMember(Name = "item_number")]
        public string ItemNumber { get; set; }
        [DataMember(Name = "invoice")]
        public string Invoice { get; set; }
        [DataMember(Name = "custom")]
        public string Custom { get; set; }
        [DataMember(Name = "send_tx")]
        public string SendTx { get; set; }
        [DataMember(Name = "received_amount")]
        public decimal ReceivedAmount { get; set; }
        [DataMember(Name = "received_confirms")]
        public int ReceivedConfirms { get; set; }

        public bool SuccessStatus()
        {
            return Status >= 100 || Status == 2;
        }

        public bool SuccessStatusLax()
        {
            
            /*
             * Payment Statuses
             * 
            Payments will post with a 'status' field, here are the currently defined values:
                -2 = PayPal Refund or Reversal
                -1 = Cancelled / Timed Out
                0 = Waiting for buyer funds
                1 = We have confirmed coin reception from the buyer
                2 = Queued for nightly payout (if you have the Payout Mode for this coin set to Nightly)
                3 = PayPal Pending(eChecks or other types of holds)
                100 = Payment Complete.We have sent your coins to your payment address or 3rd party payment system reports the payment complete
            For future-proofing your IPN handler you can use the following rules:
                < 0 = Failures / Errors
                0 - 99 = Payment is Pending in some way
                >= 100 = Payment completed successfully
            IMPORTANT: You should never ship/ release your product until the status is >= 100 OR == 2(Queued for nightly payout) !
            
             */

            // we trust Coinpayment so as soon as they receive funds (status == 1)
            // and email customer ont their end that funds are received - we release product
            // otherwise Coinpayment would email customer that transaction is complete
            // and there would be 10 minute delay until those funds are forwarded to our wallets
            // bad customer experience
            return Status >= 100 || Status == 2 || Status == 1;
        }
    }
}
