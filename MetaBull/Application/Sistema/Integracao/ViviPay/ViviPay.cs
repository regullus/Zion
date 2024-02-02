using System;
using Helpers;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Core.Helpers;
using Sistema.Integracao.Models.ViviPay;
using System.Collections.Generic;

namespace Sistema.Integracao
{
    /// <summary>
    /// Classe da API ViviPay
    /// </summary>
    public class ViviPay
    {
        /// <summary>
        /// Construtor da classe
        /// </summary>
        public ViviPay()
        {
            Autenticacao();
        }

        /// <summary>
        /// Retorna o email de usuário utilizado na autenticação e requisições da API da ViviPay
        /// </summary>
        public static string UserEmail
        {
            get
            {
                return ConfiguracaoHelper.GetString("LOGIN_EMAIL_API_VIVIPAY");
            }
        }

        /// <summary>
        /// Retorna o token de usuário utilizado nas requisições da API da ViviPay
        /// </summary>
        public static string UserToken { get; set; }

        /// <summary>
        /// Retorna o token da conta utilizado nas requisições da API da ViviPay
        /// </summary>
        public static string AccountToken
        {
            get
            {
                return ConfiguracaoHelper.GetString("ACCOUNT_TOKEN_API_VIVIPAY");
            }
        }

        /// <summary>
        /// Retorna a URL de autenticação da API da ViviPay
        /// </summary>
        public static string AuthenticationURL
        {
            get
            {
                return ConfiguracaoHelper.GetString("URL_AUTHENTICATION_API_VIVIPAY");
            }
        }

        /// <summary>
        /// Retorna a URL de requisição da API da ViviPay
        /// </summary>
        public static string TransactionURL
        {
            get
            {
                return ConfiguracaoHelper.GetString("URL_TRANSACTION_API_VIVIPAY");
            }
        }

        /// <summary>
        /// Realiza autenticação junto a API da ViviPay
        /// </summary>
        /// <returns>Retorna o token de acesso a API da ViviPay</returns>
        private static void Autenticacao()
        {
            try
            {
                var psw_API = ConfiguracaoHelper.GetString("PASSWORD_API_VIVIPAY");

                if (string.IsNullOrEmpty(AuthenticationURL) && string.IsNullOrEmpty(UserEmail) && string.IsNullOrEmpty(psw_API))
                {
                    throw new Exception("Faltam parâmetros para se autenticar na ViviPay.");
                }

                var client = new RestClient(AuthenticationURL);
                var request = new RestRequest("/users/authentication_token", Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var json = "{\"user\":{\"email\":\"" + UserEmail + "\",\"password\":\"" + psw_API + "\"}}";
                var json1 = "{\n    \"user\": {\n        \"email\": \"" + UserEmail + "\",\n        \"password\": \"" + psw_API + "\"\n    }\n}\n";
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    dynamic retorno = JsonConvert.DeserializeObject(response.Content);
                    UserToken = retorno.authentication_token;
                }
                else
                {
                    throw new Exception("Não foi possível se autenticar na ViviPay.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

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
        /// <param name="cpf">CPF registrado no cartão</param>
        /// <param name="ddd">DDD registrado no cartão</param>
        /// <param name="telefone">Telefone registrado no cartão</param>
        /// <param name="email">Email do usuário logado</param>
        /// <returns>Objeto com dados da transação realizada</returns>
        public string PagamentoCredito(string strBandeira, string nome, string numero, string codSeguranca, string mes, string ano, string codigoPedido, decimal valor, string cpf, string ddd, string telefone, string email, string parcelamento)
        {
            #region Consistência
            if (string.IsNullOrEmpty(AccountToken))
                throw new Exception("Não é possível realizar a transação sem o token da conta.");

            if (string.IsNullOrEmpty(TransactionURL))
                throw new Exception("Não é possível realizar a requisição sem a URL de transação.");

            if (string.IsNullOrEmpty(UserToken))
                throw new Exception("Não é possível realizar a requisição sem o token de usuário.");
            #endregion

            #region Cria objeto da Transação
            //Monta objeto de crédito da transação.
            card credito = new card()
            {
                brand = strBandeira.ToLower(),
                number = numero,
                cvv = codSeguranca,
                month = mes,
                year = ano
            };

            //Monta objeto do Pagamento.
            payment pagamento = new payment()
            {
                installments = string.IsNullOrEmpty(parcelamento) ? 1 : Convert.ToInt16(parcelamento),
                total_value = valor / 100,
                method_payment = "creditcard",
                card = credito
            };

            //Monta objeto de CheckOut.
            checkout_data checkout_Data = new checkout_data()
            {
                name = nome,
                cpf = cpf,
                area_code = ddd,
                phone = telefone,
                email = email,
                description = codigoPedido,
                payment = pagamento
            };

            //Monta objeto da transação.
            transaction transacao = new transaction
            {
                conta_token = AccountToken,
                checkout_data = checkout_Data
            };
            #endregion

            #region Cria Transação
            var client = new RestClient(TransactionURL);
            var request = new RestRequest("/new_checkout", Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("X-User-Token", UserToken);
            request.AddHeader("X-User-Email", UserEmail);
            var json = request.JsonSerializer.Serialize(transacao);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            var response = client.Execute(request);
            #endregion

            #region Tratamento Resposta
            //Converte resposta com sucesso.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic retorno = JsonConvert.DeserializeObject(response.Content);
                if (retorno.success == "true")
                    return retorno.transaction_code;
                else
                    throw new Exception(retorno.message);
            }
            //Converte resposta com erro.
            else
            {
                dynamic retorno = JsonConvert.DeserializeObject(response.Content);
                throw new Exception("Falha ao tentar realizar pagamento com a ViviPay. Mensagem: " + retorno.message);
            }
            #endregion
        }

        /// <summary>
        /// Retorna a lista com valores parcelados e os respectivos juros
        /// </summary>
        /// <param name="bandeira">Bandeira do cartão</param>
        /// <param name="valor">Valor da transação</param>
        /// <returns>Objeto com dados do parcelamento</returns>
        public List<brandCard> ParcelamentosDisponiveis(string strBandeira, string valor)
        {
            #region Consistência
            if (string.IsNullOrEmpty(TransactionURL))
                throw new Exception("Não é possível realizar a requisição sem a URL de transação.");

            if (string.IsNullOrEmpty(UserToken))
                throw new Exception("Não é possível realizar a requisição sem o token de usuário.");
            #endregion

            #region Cria Transação
            var client = new RestClient(TransactionURL);
            var request = new RestRequest("/card/installments", Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("X-User-Token", UserToken);
            request.AddHeader("X-User-Email", UserEmail);
            request.AddQueryParameter("brand", strBandeira);
            request.AddQueryParameter("valor", valor.Replace(".", "").Replace(",", "."));
            request.AddQueryParameter("installments", "1");
            var response = client.Execute(request);
            #endregion

            #region Tratamento Resposta
            //Converte resposta com sucesso.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var retorno = JsonConvert.DeserializeObject<installmentsTransaction>(response.Content);
                if (!retorno.error)
                {
                    if (retorno.installments.mastercard != null)
                    {
                        retorno.installments.mastercard.RemoveRange(3, retorno.installments.mastercard.Count - 3);
                        return retorno.installments.mastercard;
                    }
                    else if (retorno.installments.visa != null)
                    {
                        retorno.installments.visa.RemoveRange(3, retorno.installments.visa.Count - 3);
                        return retorno.installments.visa;
                    }
                    else if (retorno.installments.amex != null)
                    {
                        retorno.installments.amex.RemoveRange(3, retorno.installments.amex.Count - 3);
                        return retorno.installments.amex;
                    }
                    else if (retorno.installments.diners != null)
                    {
                        retorno.installments.diners.RemoveRange(3, retorno.installments.diners.Count - 3);
                        return retorno.installments.diners;
                    }
                    else if (retorno.installments.elo != null)
                    {
                        retorno.installments.elo.RemoveRange(3, retorno.installments.elo.Count - 3);
                        return retorno.installments.elo;
                    }
                    else
                    {
                        throw new Exception(retorno.message);
                    }
                }
                else
                    throw new Exception(retorno.message);
            }
            //Converte resposta com erro.
            else
            {
                dynamic retorno = JsonConvert.DeserializeObject(response.Content);
                throw new Exception("Falha ao tentar buscar informações de parcelamento com a ViviPay. Mensagem: " + retorno.message);
            }
            #endregion
        }
    }
}