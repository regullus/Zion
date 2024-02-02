using Newtonsoft.Json;

namespace Sistema.Models.Envelope
{
    public class GoogleAuthenticatorRequest
    {
        public string Nome { get; set; }

        public string Token { get; set; }

        public string Secretkey { get; set; }

    }
}