using System;
using Uol.PagSeguro.Constants;
using Uol.PagSeguro.Domain;
using Uol.PagSeguro.Exception;
using Uol.PagSeguro.Resources;
using Core.Models.Loja;
using System.IO;
using Uol.PagSeguro.Service;
using Helpers;
using Core.Helpers;

namespace Sistema.Integracao
{
    public class PagSeguro
    {
        public static string CreatePayment(CarrinhoModel carrinho, string tipoJanela)
        {
            //Use global configuration
            PagSeguroConfiguration.UrlXmlConfiguration = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\PagSeguroConfig.xml";

            bool isSandbox = true;

            if (Local.Ambiente.ToUpper().Equals("PRD"))
                isSandbox = false;
            
            EnvironmentConfiguration.ChangeEnvironment(isSandbox);

            // Instantiate a new payment request
            PaymentRequest payment = new PaymentRequest();

            // Sets the currency
            payment.Currency = Uol.PagSeguro.Constants.Currency.Brl;

            // Sets a reference code for this payment request, it is useful to identify this payment in future notifications.
            payment.Reference = carrinho.CodigoPedido;

            // Sets the url used by PagSeguro for redirect user after ends checkout process
            //payment.RedirectUri = new Uri("");

            #region Produtos
            // Add an item for this payment request
            foreach (var item in carrinho.Itens)
            {
                payment.Items.Add(new Item(item.Produto.ID.ToString(), item.Produto.Nome, item.Quantidade, (decimal)item.Valor.Valor));

            }
            #endregion

            #region Frete
            // Sets shipping information for this payment request
            payment.Shipping = new Shipping();
            payment.Shipping.ShippingType = ShippingType.NotSpecified;

            //Passando valor para ShippingCost
            payment.Shipping.Cost = carrinho.Frete != null ? (decimal)carrinho.Frete.Valor : 0.00m;
            #endregion

            #region Endereço - TODO
            //payment.Shipping.Address = new Address(
            //    carrinho.EnderecoEntrega.Estado.Pais.Nome,
            //    carrinho.EnderecoEntrega.Estado.Sigla,
            //    carrinho.EnderecoEntrega.CidadeNome,
            //    carrinho.EnderecoEntrega.Distrito,
            //    carrinho.EnderecoEntrega.CodigoPostal,
            //    carrinho.EnderecoEntrega.Logradouro,
            //    carrinho.EnderecoEntrega.Numero,
            //    carrinho.EnderecoEntrega.Complemento
            //);
            #endregion

            #region Dados do Cliente
            // Sets your customer information.
            //payment.Sender = new Sender(
            //    carrinho.Usuario.Nome,
            //    carrinho.Usuario.Email,
            //    new Phone("", carrinho.Usuario.Telefone)
            //);

            //SenderDocument document = new SenderDocument(Documents.GetDocumentByType("CPF"), carrinho.Usuario.Documento);
            //payment.Sender.Documents.Add(document);

            // Add checkout metadata information
            //payment.AddMetaData(MetaDataItemKeys.GetItemKeyByDescription("CPF do passageiro"), "123.456.789-09", 1);
            //payment.AddMetaData("PASSENGER_PASSPORT", "23456", 1);
            #endregion

            #region Dados do Produto
            // Another way to set checkout parameters
            //payment.AddParameter("senderBirthday", "07/05/1980");
            //payment.AddIndexedParameter("itemColor", "verde", 1);
            //payment.AddIndexedParameter("itemId", "0003", 3);
            //payment.AddIndexedParameter("itemDescription", "Mouse", 3);
            //payment.AddIndexedParameter("itemQuantity", "1", 3);
            //payment.AddIndexedParameter("itemAmount", "200.00", 3);
            #endregion

            #region Métodos de Pagamento
            // Add discount per payment method
            //payment.AddPaymentMethodConfig(PaymentMethodConfigKeys.DiscountPercent, 50.00, PaymentMethodGroup.CreditCard);

            // Add installment without addition per payment method
            //payment.AddPaymentMethodConfig(PaymentMethodConfigKeys.MaxInstallmentsNoInterest, 6, PaymentMethodGroup.CreditCard);

            // Add installment limit per payment method
            //payment.AddPaymentMethodConfig(PaymentMethodConfigKeys.MaxInstallmentsLimit, 6, PaymentMethodGroup.CreditCard);
            #endregion

            #region Meios e Grupos de Pagamento
            // Add and remove groups and payment methods
            //List<string> accept = new List<string>();
            //payment.AcceptPaymentMethodConfig(ListPaymentMethodGroups.CreditCard, accept);

            //List<string> exclude = new List<string>();
            //exclude.Add(ListPaymentMethodNames.Boleto);
            //exclude.Add(ListPaymentMethodGroups.ETF);
            //exclude.Add(ListPaymentMethodGroups.Balance);
            //payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.Boleto, exclude);

            //List<string> exclude1 = new List<string>();
            //payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.ETF, exclude1);

            payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.ETF, null);
            payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.Boleto, null);
            payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.Balance, null);
            payment.ExcludePaymentMethodConfig(ListPaymentMethodGroups.Deposit, null);
            #endregion

            try
            {
                AccountCredentials credentials = new AccountCredentials(ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_LOGIN"), ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_TOKEN"));

                if (tipoJanela == "lightbox")
                {
                    Uri paymentRedirectUri = payment.Register(credentials);
                    return paymentRedirectUri.Query;
                }
                else
                {
                    Uri paymentRedirectUri = payment.Register(credentials);
                    return paymentRedirectUri.AbsoluteUri;
                }

            }
            catch (PagSeguroServiceException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public static Transaction VerifyPTransactionStatus(string code)
        {
            PagSeguroConfiguration.UrlXmlConfiguration = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\PagSeguroConfig.xml";

            bool isSandbox = true;

            if (Local.Ambiente.ToUpper().Equals("PRD"))
                isSandbox = false;

            EnvironmentConfiguration.ChangeEnvironment(isSandbox);

            try
            {
                AccountCredentials credentials = new AccountCredentials(ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_LOGIN"), ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_TOKEN"));

                Uol.PagSeguro.Domain.Transaction transaction = TransactionSearchService.SearchByCode(credentials, code);

                return transaction;

            }
            catch (PagSeguroServiceException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public static Transaction CheckNotificationTransaction(string codeNotification)
        {
            PagSeguroConfiguration.UrlXmlConfiguration = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\PagSeguroConfig.xml";

            bool isSandbox = true;

            if (Local.Ambiente.ToUpper().Equals("PRD"))
                isSandbox = false;

            EnvironmentConfiguration.ChangeEnvironment(isSandbox);

            try
            {
                AccountCredentials credentials = new AccountCredentials(ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_LOGIN"), ConfiguracaoHelper.GetString("PAGSEGURO_EMAIL_TOKEN"));

                Uol.PagSeguro.Domain.Transaction transaction = NotificationService.CheckTransaction(credentials, codeNotification);

                return transaction;

            }
            catch (PagSeguroServiceException exception)
            {
                throw new Exception(exception.Message);
            }
        }
    }
}