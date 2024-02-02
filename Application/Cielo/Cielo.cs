using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Cielo.Request;
using Cielo.Request.Element;

namespace Cielo
{
   /// <summary>
   /// Integração com o Webservice 1.5 da Cielo; esse participante faz um
   /// papel de facilitador para a construção de todos os participantes
   /// importantes para a integração. Através de factory methods, é possível
   /// criar as instâncias pré-configuradas com os parâmetros mínimos necessários
   /// para a execução das operações.
   /// </summary>
   public class Cielo
   {
      /// <summary>
      /// Versão interna do webservice
      /// </summary>
      public const String VERSION = "1.3.0";

      /// <summary>
      /// Endpoint de produção
      /// </summary>
      public const String PRODUCTION = "https://ecommerce.cielo.com.br/servicos/ecommwsec.do";
      /// <summary>
      /// Endpoint de testes
      /// </summary>
      public const String TEST = "https://qasecommerce.cielo.com.br/servicos/ecommwsec.do";

      private Merchant merchant;

      private String endpoint = PRODUCTION;

      /// <summary>
      /// Constroi a instância de Cielo definindo o Merchant ID, Merchant Key e
      /// o endpoint para onde serão enviadas as requisições
      /// </summary>
      /// <param name="id">Identifier.</param>
      /// <param name="key">Key.</param>
      /// <param name="endpoint">Endpoint.</param>
      public Cielo(String id, String key, String endpoint = PRODUCTION)
      {
         this.merchant = new Merchant(id, key);
         this.endpoint = endpoint;
      }

      /// <summary>
      /// Cria uma instância de Cielo.Holder, que representa o portador de um cartão,
      /// apenas definindo o token previamente gerado.
      /// </summary>
      /// <param name="token">Token.</param>
      public Holder holder(String token)
      {
         return new Holder(token);
      }

      /// <summary>
      /// Cria uma instância de Cielo.Holder, que representa o portador de um cartão,
      /// apenas definindo o número do cartão, ano e mês de expiração, código de segurança e
      /// o indicador do código de segurança.
      /// </summary>
      /// <param name="number">Número do cartão</param>
      /// <param name="expirationYear">Ano de expiração</param>
      /// <param name="expirationMonth">Mês de expiração</param>
      /// <param name="cvv">CVV - Código de segurança do verso do cartão</param>
      /// <param name="indicator">Indicador de segurança; veja Cielo.Holder.CVV</param>
      public Holder holder(
         String number,
         String expirationYear,
         String expirationMonth,
         String cvv,
         Holder.CVV indicator = Holder.CVV.INFORMED)
      {
         return new Holder(number, expirationYear, expirationMonth, cvv, indicator);
      }

      /// <summary>
      /// Cria uma instância de Cielo.Order, que representa os dados do pedido,
      /// definindo o número do pedido, valor total do pedido, data do pedido e moeda.
      /// </summary>
      /// <param name="number">Número do pedido</param>
      /// <param name="total">Total do pedido</param>
      /// <param name="dateTime">Data e hora do pedido</param>
      /// <param name="currency">Moeda do pedido - 986 para BRL</param>
      public Order order(String number, int total, String dateTime, int currency)
      {
         return new Order(number, total, dateTime, currency);
      }

      /// <summary>
      /// Cria uma instância de Cielo.Order, que representa os dados do pedido,
      /// definindo o número do pedido, valor total do pedido, data do pedido. A moeda será
      /// configurada por padrâo para 986 - BRL.
      /// </summary>
      /// <param name="number">Número do pedido</param>
      /// <param name="total">Total do pedido</param>
      /// <param name="dateTime">Data e hora do pedido</param>
      public Order order(String number, int total, String dateTime)
      {
         return new Order(number, total, dateTime);
      }

      /// <summary>
      /// Cria uma instância de Cielo.Order, que representa os dados do pedido,
      /// definindo o número do pedido e valor total. A data será configurada como a data
      /// atual e a moeda será configurada por padrâo para 986 - BRL.
      /// </summary>
      /// <param name="number">Número do pedido</param>
      /// <param name="total">Total do pedido</param>
      public Order order(String number, int total)
      {
         return new Order(number, total);
      }

      /// <summary>
      /// Cria uma instância de Cielo.PaymentMethod, que representa a forma de pagamento,
      /// definindo o emissor do cartão.
      /// </summary>
      /// <param name="issuer">O emissor do cartão - VISA, MASTERCARD, AMEX, etc</param>
      public PaymentMethod paymentMethod(String issuer)
      {
         return new PaymentMethod(issuer);
      }

      /// <summary>
      /// Cria uma instância de Cielo.PaymentMethod, que representa a forma de pagamento,
      /// definindo o emissor do cartão e o produto Cielo utilizado.
      /// </summary>
      /// <param name="issuer">O emissor do cartão - VISA, MASTERCARD, AMEX, etc</param>
      /// <param name="product">O produto utilizado na transação - crédito, débito, parcelado, etc</param>
      public PaymentMethod paymentMethod(String issuer, String product)
      {
         return new PaymentMethod(issuer, product);
      }

      /// <summary>
      /// Cria uma instância de Cielo.PaymentMethod, que representa a forma de pagamento,
      /// definindo o emissor do cartão, o produto Cielo utilizado e o número de parcelas.
      /// </summary>
      /// <param name="issuer">O emissor do cartão - VISA, MASTERCARD, AMEX, etc</param>
      /// <param name="product">O produto utilizado na transação - crédito, débito, parcelado, etc</param>
      /// <param name="installments">Quantidade de parcelas</param>
      public PaymentMethod paymentMethod(String issuer, String product, int installments)
      {
         return new PaymentMethod(issuer, product, installments);
      }

      /// <summary>
      /// Cria uma instância de Transaction pré-configurada
      /// </summary>
      /// <param name="holder">Detalhes do portador do cartão</param>
      /// <param name="order">Detalhes do pedido</param>
      /// <param name="paymentMethod">Forma de pagamento</param>
      /// <param name="returnURL">URL de retorno</param>
      /// <param name="authorize">Método de autorização</param>
      /// <param name="capture">Determina se a transação deverá ser capturada automaticamente</param>
      /// <returns>>Uma instância de Transaction</returns>
      public Transaction transaction(
         Holder holder,
         Order order,
         PaymentMethod paymentMethod,
         String returnURL,
         Transaction.AuthorizationMethod authorize,
         bool capture)
      {
         return new Transaction(merchant, holder, order, paymentMethod, returnURL, authorize, capture);
      }

      String sendHttpRequest(String message)
      {
         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);

         request.Method = "POST";
         request.ContentType = "application/x-www-form-urlencoded";

         using (Stream stream = request.GetRequestStream())
         {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes("mensagem=" + message);

            stream.Write(bytes, 0, bytes.Length);
         }

         HttpWebResponse response = (HttpWebResponse)request.GetResponse();
         string result;

         using (Stream stream = response.GetResponseStream())
         {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
               result = reader.ReadToEnd();
            }
         }

         return result.ToString();
      }

      String serialize<T>(T request)
      {
         XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
         StringWriter stringWriter = new StringWriter();

         XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
         {
            Indent = true,
            OmitXmlDeclaration = true,
            Encoding = Encoding.GetEncoding("ISO-8859-1")
         };

         XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);

         xmlserializer.Serialize(xmlWriter, request);

         return stringWriter.ToString();
      }

      /// <summary>
      /// Cria uma transação com os dados informados e envia uma requisição-autorizacao-tid para
      /// o webservice Cielo.
      /// </summary>
      /// <returns>A transação retornada pela Cielo e seu respectivo status</returns>
      /// <param name="holder">Portador do cartão</param>
      /// <param name="order">Dados do pedido</param>
      /// <param name="paymentMethod">Método de pagamento</param>
      /// <param name="returnURL">URL de retorno</param>
      /// <param name="authorize">Método de autorização</param>
      /// <param name="capture">Define se a transação deve ser capturada automaticamente</param>
      public Transaction authorizationRequest(
         Holder holder,
         Order order,
         PaymentMethod paymentMethod,
         String returnURL,
         Transaction.AuthorizationMethod authorize,
         bool capture)
      {
         return authorizationRequest(transaction(holder, order, paymentMethod, returnURL, authorize, capture));
      }

      /// <summary>
      /// Envia uma requisição-autorizacao-tid para o webservice Cielo para que a transação
      /// seja autorizada segundo as configurações previamente feitas.
      /// </summary>
      /// <returns>A transação retornada pela Cielo e seu respectivo status</returns>
      /// <param name="transaction">A transação previamente configurada</param>
      public Transaction authorizationRequest(Transaction transaction)
      {
         AuthorizationRequest request = AuthorizationRequest.create(transaction);
         return TransacaoElement.unserialize(transaction, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-cancelamento para o webservice Cielo para cancelar uma transação
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="holder">Portador do cartão</param>
      /// <param name="order">Dados do pedido</param>
      /// <param name="paymentMethod">Método de pagamento</param>
      /// <param name="returnURL">URL de retorno</param>
      /// <param name="authorize">Método de autorização</param>
      /// <param name="capture">Se a transação foi capturada</param>
      /// <param name="total">Total do cancelamento</param>
      public Transaction cancellationRequest(
         Holder holder,
         Order order,
         PaymentMethod paymentMethod,
         String returnURL,
         Transaction.AuthorizationMethod authorize,
         bool capture,
         int total)
      {
         return cancellationRequest(transaction(holder, order, paymentMethod, returnURL, authorize, capture), total);
      }

      /// <summary>
      /// Envia uma requisição-cancelamento para o webservice Cielo para cancelar uma transação
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="transaction">A transação que será cancelada</param>
      public Transaction cancellationRequest(Transaction transaction)
      {
         return this.cancellationRequest(transaction, transaction.order.total);
      }

      /// <summary>
      /// Envia uma requisição-cancelamento para o webservice Cielo para cancelar uma transação
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="transaction">A transação que será cancelada</param>
      /// <param name="total">Total do cancelamento</param>
      public Transaction cancellationRequest(Transaction transaction, int total)
      {
         CancellationRequest request = CancellationRequest.create(transaction, total);
         return TransacaoElement.unserialize(transaction, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-cancelamento para o webservice Cielo para cancelar uma transação
      /// </summary>
      /// <param name="transaction">A transação que será cancelada</param>
      /// <param name="total">Total do cancelamento</param>
      /// <param name="tid">todo: describe tid parameter on cancellationRequest</param>
      /// <param name="merchant">todo: describe merchant parameter on cancellationRequest</param>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      public Transaction cancellationRequest(string tid, int total, Merchant merchant = null)
      {
         CancellationRequest request = CancellationRequest.create(tid, merchant ?? this.merchant, total);
         return TransacaoElement.unserialize(null, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-captura para o webservice Cielo para capturar uma transação
      /// previamente autorizada
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="holder">Portador do cartão</param>
      /// <param name="order">Dados do pedido</param>
      /// <param name="paymentMethod">Método de pagamento</param>
      /// <param name="returnURL">URL de retorno</param>
      /// <param name="authorize">Método de autorização</param>
      /// <param name="capture">Se a transação foi capturada</param>
      /// <param name="total">Total da captura</param>
      public Transaction captureRequest(
         Holder holder,
         Order order,
         PaymentMethod paymentMethod,
         String returnURL,
         Transaction.AuthorizationMethod authorize,
         bool capture,
         int total)
      {
         return captureRequest(transaction(holder, order, paymentMethod, returnURL, authorize, capture), total);
      }

      /// <summary>
      /// Envia uma requisição-captura para o webservice Cielo para capturar uma transação
      /// previamente autorizada
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="transaction">A transação que deverá ser capturada</param>
      public Transaction captureRequest(Transaction transaction)
      {
         return this.captureRequest(transaction, transaction.order.total);
      }

      /// <summary>
      /// Envia uma requisição-captura para o webservice Cielo para capturar uma transação
      /// previamente autorizada
      /// </summary>
      /// <returns>A transação com o respectivo status retornada pela Cielo</returns>
      /// <param name="transaction">A transação que deverá ser capturada</param>
      /// <param name="total">O valor que deverá ser capturado</param>
      public Transaction captureRequest(Transaction transaction, int total)
      {
         CaptureRequest request = CaptureRequest.create(transaction, total);
         return TransacaoElement.unserialize(transaction, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-token para gerar um token para um cartão de crédito.
      /// </summary>
      /// <returns>O Token retornado pela Cielo</returns>
      /// <param name="transaction">A transação que contém os dados do portador</param>
      public Token tokenRequest(Transaction transaction)
      {
         TokenRequest request = TokenRequest.create(transaction);
         return RetornoTokenElement.unserialize(transaction, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-token para gerar um token para um cartão de crédito.
      /// </summary>
      /// <param name="transaction">A transação que contém os dados do portador</param>
      /// <param name="holder">todo: describe holder parameter on tokenRequest</param>
      /// <param name="merchant">todo: describe merchant parameter on tokenRequest</param>
      /// <returns>O Token retornado pela Cielo</returns>
      public Token tokenRequest(Holder holder, Merchant merchant = null)
      {
         TokenRequest request = TokenRequest.create(merchant ?? this.merchant, holder);
         return RetornoTokenElement.unserialize(sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição-transacao com os dados especificados
      /// </summary>
      /// <param name="holder">Detalhes do portador do cartão</param>
      /// <param name="order">Detalhes do pedido</param>
      /// <param name="paymentMethod">Forma de pagamento</param>
      /// <param name="returnURL">URL de retorno</param>
      /// <param name="authorize">Método de autorização</param>
      /// <param name="capture">Determina se a transação deverá ser capturada automaticamente</param>
      /// <returns>>Uma instância de Transaction com a resposta da requisição</returns>
      public Transaction transactionRequest(
         Holder holder,
         Order order,
         PaymentMethod paymentMethod,
         String returnURL,
         Transaction.AuthorizationMethod authorize,
         bool capture)
      {
         return transactionRequest(transaction(holder, order, paymentMethod, returnURL, authorize, capture));
      }

      /// <summary>
      /// Envia uma requisição-transacao com os dados especificados
      /// </summary>
      /// <param name="transaction">Detalhes da transação</param>
      /// <returns>>Uma instância de Transaction com a resposta da requisição</returns>
      public Transaction transactionRequest(Transaction transaction)
      {
         TransactionRequest request = TransactionRequest.create(transaction);
         return TransacaoElement.unserialize(transaction, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Envia uma requisição de consulta
      /// </summary>
      /// <param name="tid">TID da operação</param>        
      /// <returns>Uma instância de Transaction com a resposta da requisição</returns>
      public Transaction consultationRequest(String tid)
      {
         ConsultationRequest request = ConsultationRequest.create(tid, merchant);
         return TransacaoElement.unserialize(null, sendHttpRequest(serialize(request)));
      }

      /// <summary>
      /// Significado do codigo de retorno da transação
      /// </summary>
      /// <param name="strCodigo">Codigo de retorno</param>
      /// <returns>Descrição do código de retorno</returns>
      public static string Retorno(string strCodigo)
      {
         string strRetorno = "";

         switch (strCodigo)
         {
            case "00": strRetorno = "Transação autorizada"; break;
            case "01": strRetorno = "Transação referida pelo banco emissor"; break;
            case "04": strRetorno = "Transação não autorizada"; break;
            case "05": strRetorno = "Transação não autorizada"; break;
            case "06": strRetorno = "Tente novamente"; break;
            case "07": strRetorno = "Cartão com restrição"; break;
            case "08": strRetorno = "Código de segurança inválido"; break;
            case "11": strRetorno = "Transação autorizada"; break;
            case "13": strRetorno = "Valor inválido"; break;
            case "14": strRetorno = "Cartão inválido"; break;
            case "15": strRetorno = "Banco emissor indisponível"; break;
            case "21": strRetorno = "Cancelamento não efetuado"; break;
            case "41": strRetorno = "Cartão com restrição"; break;
            case "51": strRetorno = "Saldo insuficiente"; break;
            case "54": strRetorno = "Cartão vencido"; break;
            case "57": strRetorno = "Transação não permitida"; break;
            case "60": strRetorno = "Transação não autorizada"; break;
            case "62": strRetorno = "Transação não autorizada"; break;
            case "78": strRetorno = "Cartão não foi desbloqueado pelo portador"; break;
            case "82": strRetorno = "Erro no cartão"; break;
            case "91": strRetorno = "Banco fora do ar"; break;
            case "96": strRetorno = "Tente novamente"; break;
            case "AA": strRetorno = "Tempo excedido"; break;
            case "AC": strRetorno = "Use função débito"; break;
            case "GA": strRetorno = "Transação referida pela Cielo"; break;
            default: strRetorno = "Sem Informação"; break;
         }
         return strRetorno;
      }

      public static string Orientacao(string strCodigo)
      {
         string strRetorno = "";

         switch(strCodigo)
         {
            case "00": strRetorno = ""; break;
            case "01": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "04": strRetorno = "Por favor, refazer a transação"; break;
            case "05": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "06": strRetorno = "Por favor, refazer a transação"; break;
            case "07": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "08": strRetorno = "Por favor, refazer a transação digitando o código de segurança corretamente"; break;
            case "11": strRetorno = ""; break;
            case "13": strRetorno = "Por favor, refazer a transação digitando o valor correto"; break;
            case "14": strRetorno = "Por favor, verificar o número do cartão e digitar novamente"; break;
            case "15": strRetorno = "Por favor, aguardar alguns instantes e tentar novamente"; break;
            case "21": strRetorno = "O estabelecimento deve entrar em contato com a Central de Relacionamento Cielo"; break;
            case "41": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "51": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "54": strRetorno = "Por favor, verificar o vencimento do cartão e digitar novamente"; break;
            case "57": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "60": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "62": strRetorno = "Por favor, contatar o banco emissor do cartão"; break;
            case "78": strRetorno = "Por favor, desbloquear o cartão junto ao emissor do cartão"; break;
            case "82": strRetorno = "Por favor, verificar o número do cartão e digitar novamente"; break;
            case "91": strRetorno = "Por favor, aguardar alguns instantes e tentar novamente"; break;
            case "96": strRetorno = "Por favor, aguardar alguns instantes e tentar novamente"; break;
            case "AA": strRetorno = "Por favor, aguardar alguns instantes e tentar novamente"; break;
            case "AC": strRetorno = "Por favor, utilizar o cartão de débito (Visa ou MasterCard)"; break;
            case "GA": strRetorno = "Por favor, aguardar alguns instantes e tentar novamente"; break;
            default: strRetorno = "Sem Informação"; break;
         }
         return strRetorno;
      }

      public static string Erro(int intCodigo)
      {
         string strRetorno = "";

         switch(intCodigo)
         {
            case 1: strRetorno = "Mensagem inválida"; break;
            case 2: strRetorno = "Credenciais inválidas"; break;
            case 3: strRetorno = "Transação inexistente"; break;
            case 8: strRetorno = "Código de Segurança Inválido"; break;
            case 10: strRetorno = "Inconsistência no envio do cartão"; break;
            case 11: strRetorno = "Modalidade não habilitada"; break;
            case 12: strRetorno = "Número de parcelas inválido"; break;
            case 13: strRetorno = "Flag de autorização automática"; break;
            case 14: strRetorno = "Autorização Direta inválida"; break;
            case 15: strRetorno = "Autorização Direta sem Cartão"; break;
            case 16: strRetorno = "Identificador, TID, inválido"; break;
            case 17: strRetorno = "Código de segurança ausente"; break;
            case 18: strRetorno = "Indicador de código de segurança inconsistente"; break;
            case 19: strRetorno = "URL de Retorno não fornecida"; break;
            case 20: strRetorno = "Status não permite autorização"; break;
            case 21: strRetorno = "Prazo de autorização vencido"; break;
            case 22: strRetorno = "Número de parcelas inválido"; break;
            case 25: strRetorno = "Encaminhamento a autorização não permitido"; break;
            case 30: strRetorno = "Status inválido para captura"; break;
            case 31: strRetorno = "Prazo de captura vencido"; break;
            case 32: strRetorno = "Valor de captura inválido"; break;
            case 33: strRetorno = "Falha ao capturar"; break;
            case 34: strRetorno = "Valor da taxa de embarque obrigatório"; break;
            case 35: strRetorno = "Bandeira inválida para utilização da Taxa de Embarque"; break;
            case 36: strRetorno = "Produto inválido para utilização da Taxa de Embarque"; break;
            case 40: strRetorno = "Prazo de cancelamento vencido"; break;
            case 42: strRetorno = "Falha ao cancelar"; break;
            case 43: strRetorno = "Valor de cancelamento é maior que valor autorizado."; break;
            case 51: strRetorno = "Recorrência Inválida"; break;
            case 52: strRetorno = "Token Inválido"; break;
            case 53: strRetorno = "Recorrência não habilitada"; break;
            case 54: strRetorno = "Transação com Token inválida"; break;
            case 55: strRetorno = "Número do cartão não fornecido"; break;
            case 56: strRetorno = "Validade do cartão não fornecido"; break;
            case 57: strRetorno = "Erro inesperado gerando Token"; break;
            case 61: strRetorno = "Transação Recorrente Inválida"; break;
            case 77: strRetorno = "XID não fornecido"; break;
            case 78: strRetorno = "CAVV não fornecido"; break;
            case 86: strRetorno = "XID e CAVV não fornecidos"; break;
            case 87: strRetorno = "CAVV com tamanho divergente"; break;
            case 88: strRetorno = "XID com tamanho divergente"; break;
            case 89: strRetorno = "ECI com tamanho divergente"; break;
            case 90: strRetorno = "ECI inválido"; break;
            case 95: strRetorno = "Erro interno de autenticação"; break;
            case 97: strRetorno = "Sistema indisponível"; break;
            case 98: strRetorno = "Timeout"; break;
            case 99: strRetorno = "Erro inesperado"; break;
            default: strRetorno = "Erro"; break;
         }

         return strRetorno;

      }

   }

}