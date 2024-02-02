using Core.Repositories.Loja;
using Core.Services.Loja;
using PayPal;
using PayPal.Enum;
using PayPal.ExpressCheckout;
using PayPal.ExpressCheckout.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.MeioPagamento
{
    public class PayPalService
    {

        private PedidoPagamentoRepository pedidoPagamentoRepository;
        
        private PedidoService pedidoService;

        public PayPalService(DbContext context)
        {
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);

            pedidoService = new PedidoService(context);
        }

        public string Pagar(Entities.PedidoPagamento pagamento)
        {
            ExpressCheckoutApi api = PayPalApiFactory.instance.ExpressCheckout(Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_USERNAME"), Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_PASSWORD"), Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_SIGNATURE"));
            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PAYPAL_SANDBOX"))
            {
                api = api.sandbox();
            }
            
            SetExpressCheckoutOperation operation = api.SetExpressCheckout(Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO") + "/paypal/finalizar?sigla=" + pagamento.Pedido.Usuario.Pais.Idioma.Sigla, Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO") + "/paypal/erro");
            PaymentRequest request = operation.PaymentRequest(0);

            request.Action = PaymentAction.SALE;
            request.Custom = pagamento.ID.ToString();

            var item = request.addItem(pagamento.Pedido.Codigo, 1, (double)pagamento.Valor);
            item.Category = new ItemCategory("Digital", 0);

            operation.LocaleCode = GetLocaleCodeByString(pagamento.Usuario.Pais.Sigla);
            operation.CurrencyCode = GetCurrencyCodeByString(pagamento.Moeda.Sigla);

            var retorno = operation.execute();

            if (retorno != null && retorno.ResponseNVP != null && retorno.ResponseNVP.Ack != null && retorno.ResponseNVP.Ack.ToString().ToUpper() == "SUCCESS")
            {
                return operation.RedirectUrl;
            }

            return null;
        }

        public Entities.PedidoPagamentoStatus.TodosStatus Processar(string token, string payerID)
        {
            ExpressCheckoutApi api = PayPalApiFactory.instance.ExpressCheckout(Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_USERNAME"), Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_PASSWORD"), Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_SIGNATURE"));
            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PAYPAL_SANDBOX"))
            {
                api = api.sandbox();
            }

            GetExpressCheckoutDetailsOperation detailsOperation = api.GetExpressCheckoutDetails(token);
            detailsOperation.execute();

            var custom = detailsOperation.ResponseNVP.Get("CUSTOM");            
            var pagamentoID = 0;
            var novoStatus = Entities.PedidoPagamentoStatus.TodosStatus.Indefinido;

            if (int.TryParse(custom, out pagamentoID))
            {
                var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
                var valorPago = detailsOperation.ResponseNVP.GetDouble("PAYMENTREQUEST_0_AMT");

                DoExpressCheckoutPaymentOperation operation = api.DoExpressCheckoutPayment(token, payerID, PaymentAction.SALE);
                operation.CurrencyCode = GetCurrencyCodeByString(pagamento.Moeda.Sigla);
                operation.PaymentRequest(0).Amount = valorPago;
                operation.execute();

                if (operation.ResponseNVP.Get("ACK") == "Success")
                {
                    string status = operation.ResponseNVP.Get("PAYMENTINFO_0_PAYMENTSTATUS");
                    switch (status)
                    {
                        //Falha
                        case "Canceled":
                        case "Denied":
                        case "Failed":
                        case "Refused":
                        case "Returned":
                        case "Reversed":
                        case "Unclaimed":
                            novoStatus = Entities.PedidoPagamentoStatus.TodosStatus.Cancelado;
                            break;

                        //Aguardando
                        case "Held":
                        case "In progress":
                        case "On hold":
                        case "Paid":
                        case "Pending verification":
                        case "Placed":
                        case "Removed":
                        case "Temporary hold":
                        case "Processing":
                            novoStatus = Entities.PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao;
                            break;

                        //Sucesso
                        case "Cleared":
                        case "Cleared by payment review":
                        case "Completed":
                            novoStatus = Entities.PedidoPagamentoStatus.TodosStatus.Pago;
                            break;

                        //Outros Casos
                        case "Partially refunded":
                        case "Refunded":
                            break;
                    }

                    if (novoStatus != Entities.PedidoPagamentoStatus.TodosStatus.Indefinido)
                    {
                        bool ret = pedidoService.ProcessarPagamento(pagamentoID, novoStatus, 0);
                    }
                }

            }

            return novoStatus;
        }

        private LocaleCode GetLocaleCodeByString(string code)
        {
            var localeCode = LocaleCode.DEFAULT;
            var fields = typeof(LocaleCode).GetFields();
            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(null);
                if (fieldValue.ToString() == code)
                {
                    return (LocaleCode)fieldValue;
                }
            }
            return localeCode;
        }

        private CurrencyCode GetCurrencyCodeByString(string code)
        {
            var currencyCode = CurrencyCode.DEFAULT;
            var fields = typeof(CurrencyCode).GetFields();
            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(null);
                if (fieldValue.ToString() == code)
                {
                    return (CurrencyCode)fieldValue;
                }
            }
            return currencyCode;
        }
    }
}
