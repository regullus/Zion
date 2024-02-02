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
    public class IpnWithdraw : IpnBase
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
        
        [DataMember(Name = "id")]
        public string Id { get; set; }
        
        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }

        [DataMember(Name = "amounti")]
        public string Amounti { get; set; }
        [DataMember(Name = "note")]
        public string Note { get; set; }


    }
}
