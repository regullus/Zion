using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api.Models
{
    public class ConvertLimitsResponse : ResponseModelFoundation<ConvertLimitsResponse.Item>
    {
        [DataContract]
        public class Item
        {
            [DataMember(Name = "cmd")]
            public string Cmd { get; private set; }
            [DataMember(Name = "min")]
            public decimal Min { get; set; }
            [DataMember(Name = "max")]
            public decimal Max { get; set; }
            [DataMember(Name = "rate")]
            public decimal Rate { get; set; }
        }
    }
}
