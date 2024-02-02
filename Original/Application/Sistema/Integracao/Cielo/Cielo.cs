using System;
using Helpers;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Sistema.Integracao.Models.Cielo;

namespace Sistema.Integracao
{
    public class Cielo
    {
        /// <summary>
        /// Realiza uma transação com cartão de crédito 
        /// </summary>
        /// <param name="bandeira">Bandeira do cartão</param>
        /// <param name="nome">Nome impresso no cartão</param>
        /// <param name="numero">Número do cartão</param>
        /// <param name="codSeguranca">Código de Segurança do cartão</param>
        /// <param name="mes">Mês de Validade</param>
        /// <param name="ano">Ano de Validade</param>
        /// <param name="codigoPedido">ID do Pedido no sistema</param>
        /// <param name="valor">Valor da transação</param>
        /// <param name="token">Token de Autenticação para acessar API da Cielo</param>
        /// <returns>Objeto com dados da transação realizada</returns>
        public static Transaction PagamentoCredito(string strBandeira, string nome, string numero, string codSeguranca, string mes, string ano, string codigoPedido, decimal valor, string token = null)
        {
            string tokenAuthentication = null;

            //Realiza Autenticação com a API.
            if (string.IsNullOrEmpty(token))
                tokenAuthentication = Autenticacao();
            else
                tokenAuthentication = token;

            //Monta objeto de crédito da  transação.
            CreditCard credito = new CreditCard()
            {
                Brand = CardType(strBandeira),
                CardNumber = numero,
                SecurityCode = codSeguranca,
                Holder = nome,
                ExpirationDate = Convert.ToDateTime("01/" + mes + "/" + ano)
            };
            //Monta objeto da transação.
            Transaction transacao = new Transaction
            {
                Payment = new Payment(valor, Currency.BRL, 1, true, null, credito),
                Customer = new Customer(nome),
                MerchantOrderId = codigoPedido
            };

            //Cria a transação.
            var client = new RestClient(Local.CieloURL);
            var request = new RestRequest("/createpayment", Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("Authorization", tokenAuthentication);
            var json = request.JsonSerializer.Serialize(transacao);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            var response = client.Execute(request);

            //Converte resposta com sucesso.
            if (response.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Transaction>(response.Content);

            //Trata a resposta com erro e retorna mensagens.
            RetornaErro(response);

            return new Transaction();
        }

        /// <summary>
        /// Realiza uma transação com cartão de débito 
        /// </summary>
        /// <param name="bandeira">Bandeira do cartão</param>
        /// <param name="nome">Nome impresso no cartão</param>
        /// <param name="numero">Número do cartão</param>
        /// <param name="codSeguranca">Código de Segurança do cartão</param>
        /// <param name="mes">Mês de Validade</param>
        /// <param name="ano">Ano de Validade</param>
        /// <param name="pedidoID">ID do Pedido no sistema</param>
        /// <param name="valor">Valor da transação</param>
        /// <param name="token">Token de Autenticação para acessar API da Cielo</param>
        /// <returns>Objeto com dados da transação realizada</returns>
        public static Transaction PagamentoDebito(CardBrand bandeira, string nome, string numero, string codSeguranca, string mes, string ano, int pedidoID, decimal valor, string token = null)
        {
            string tokenAuthentication = null;

            //Realiza Autenticação com a API.
            if (string.IsNullOrEmpty(token))
                tokenAuthentication = Autenticacao();
            else
                tokenAuthentication = token;

            //Cria a transação.
            var client = new RestClient(Local.CieloURL);
            var request = new RestRequest("/createpayment", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", tokenAuthentication);

            var response = client.Execute(request);

            //Converte resposta com sucesso.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Transaction>(response.Content);
            }

            //Trata a resposta com erro e retorna mensagens.
            RetornaErro(response);

            return new Transaction();
        }

        /// <summary>
        /// Verifica status da transação realizada
        /// </summary>
        /// <param name="idPagamento">Id do Pagamento junto a Cielo</param>
        /// <param name="token">Token de Autenticação para acessar API da Cielo</param>
        /// <returns>Objeto com dados da transação realizada</returns>
        public static Transaction VerificaPagamento (string idPagamento, string token = null)
        {
            string tokenAuthentication = null;

            //Realiza Autenticação com a API.
            if (string.IsNullOrEmpty(token))
                tokenAuthentication = Autenticacao();
            else
                tokenAuthentication = token;

                //Cria a chamada de checagem de pagamento.
                var client = new RestClient(Local.CieloURL);
                var request = new RestRequest("/checkpayment", Method.GET);
                request.AddParameter("Authorization", tokenAuthentication);
                var response = client.Execute(request);

                //Converte resposta com sucesso.
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Transaction>(response.Content);
                }

                //Trata a resposta com erro e retorna mensagens.
                RetornaErro(response);

                return new Transaction();
            }

        /// <summary>
        /// Cancela uma transação realizada
        /// </summary>
        /// <param name="idPagamento">Id do Pagamento junto a Cielo</param>
        /// <param name="token">Token de Autenticação para acessar API da Cielo</param>
        /// <returns>Objeto com dados da transação cancelada</returns>
        public Transaction CancelarTransacao(string idPagamento, string token = null)
        {
            string tokenAuthentication = null;

            //Realiza Autenticação com a API.
            if (string.IsNullOrEmpty(token))
                tokenAuthentication = Autenticacao();
            else
                tokenAuthentication = token;

            //Cria a chamada de checagem de pagamento.
            var client = new RestClient(Local.CieloURL);
            var request = new RestRequest("/cancelpayment", Method.PUT);
            request.AddParameter("Authorization", tokenAuthentication);
            var response = client.Execute(request);

            //Converte resposta com sucesso.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Transaction>(response.Content);
            }

            //Trata a resposta com erro e retorna mensagens.
            RetornaErro(response);

            return new Transaction();

        }

        /// <summary>
        /// Realiza autenticação junto a API da Cielo
        /// </summary>
        /// <returns>Retorna o token de acesso a API da Cielo</returns>
        private static string Autenticacao()
        {
            var client = new RestClient(Local.CieloURL);
            var request = new RestRequest("/token", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("grant_type", "password");
            request.AddParameter("userName", Local.Sistema);
            request.AddParameter("password", Local.CieloPassword);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic retorno = JsonConvert.DeserializeObject(response.Content);
                return retorno.access_token;
            }
            else
            {
                dynamic erro =  JsonConvert.DeserializeObject(response.Content);

                if (erro.error_description.ToString() == "Usuário não encontrado ou senha incorreta.")
                    throw new Exception("Erro ao autenticar serviço CIELO.");
                else
                    throw new Exception("Erro desconhecido no acesso ao serviço CIELO.");
            }
        }

        /// <summary>
        /// Trata os status HTTP de retorno da API da Cielo
        /// </summary>
        /// <param name="response">Retorno da chamada na API da Cielo</param>
        private static void RetornaErro(IRestResponse response)
        {
            //dynamic erro = JsonConvert.DeserializeObject(response.Content);
            //string retorno = erro.errorCode.ToString() + ", " + erro.error_description.ToString();
            //throw new Exception(retorno);

            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError)
            {
                dynamic erro = JsonConvert.DeserializeObject(response.Content);
                throw new Exception(erro.errorCode.ToString() + ", " + erro.error_description.ToString());
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                dynamic erros = JsonConvert.DeserializeObject(response.Content);
                throw new Exception(erros[0].errorCode.ToString() + ", " + erros[0].error_description.ToString());
            }
            else
            {
                dynamic erro = JsonConvert.DeserializeObject(response.Content);
                throw new Exception(erro.errorCode.ToString() + ", " + erro.error_description.ToString());
            }
        }

        private static CardBrand? CardType(string cardBrand)
        {
            switch (cardBrand)
            {
                case "visa":
                    return CardBrand.Visa;
                case "mastercard":
                    return CardBrand.Master;
                case "amex":
                    return CardBrand.Amex;
                case "aura":
                    return CardBrand.Aura;
                case "diners":
                    return CardBrand.Diners;
                case "discover":
                    return CardBrand.Discover;
                case "elo":
                    return CardBrand.Elo;
                case "jcb":
                    return CardBrand.JCB;
                default:
                    return null;
            }
        }
    }

}