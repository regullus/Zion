using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Models.Envelope
{
    public class GoogleAuthenticatorResponse
    {
        public bool Ok { get; set; }
        public string QrCodeImage { get; set; }
        public string Mensagem { get; set; }
        public string Secretkey { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}