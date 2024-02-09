using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Coinpayments.Api.Models
{
    [DataContract]
    public class ConvertLimitsRequest
    {
        public ConvertLimitsRequest()
        {
            Cmd = "convert_limits";
        }

        [DataMember(Name = "cmd")]
        public string Cmd { get; private set; }
        [DataMember(Name = "from")]
        public string From { get; set; }
        [DataMember(Name = "to")]
        public string To { get; set; }

    }
}




