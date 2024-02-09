using ServiceStack.Text;
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
    public class IpnDeposit : IpnBase
    {
        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "status_text")]
        public string StatusText { get; set; }
        
        [DataMember(Name = "txn_id")]
        public string TxnId { get; set; }
        
        [DataMember(Name = "currency")]
        public string Currency { get; set; }
        
        [DataMember(Name = "address")]
        public string Address { get; set; }
        
        [DataMember(Name = "deposit_id")]
        public string DepositId { get; set; }
        
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }

        [DataMember(Name = "amounti")]
        public string Amounti { get; set; }

        [DataMember(Name = "fee")]
        public string Fee { get; set; }

        [DataMember(Name = "feei")]
        public string Feei { get; set; }

        [DataMember(Name = "confirms")]
        public string Confirms { get; set; }

        [DataMember(Name = "fiat_coin")]
        public string Fiat_coin { get; set; }

        [DataMember(Name = "fiat_amount")]
        public decimal FiatAmount { get; set; }

        [DataMember(Name = "fiat_amounti")]
        public string FiatAmounti { get; set; }

        [DataMember(Name = "fiat_fee")]
        public decimal fiat_fee { get; set; }

        [DataMember(Name = "fiat_feei")]
        public string fiat_feei { get; set; }
    }
}
