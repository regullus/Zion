using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Sistema.Integracao.Models.Cielo
{
    public class Transaction
    {
        public string MerchantOrderId { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
    }

    public class Payment
    {
        private static readonly Regex softDescriptorMatch = new Regex("^[a-zA-Z0-9]?", RegexOptions.Compiled);

        //private string urlReturn = "http://localhost:55095";
        //private string urlReturn = "http://www.cielo.com.br";

        private string softDescriptor;
        public Payment()
        {
        }

        public Payment(decimal amount, Currency currency, int installments, bool capture, string softDescriptor, CreditCard creditCard, string country = Cielo.Country.BRA)
        {
            this.Type = PaymentType.CreditCard;
            this.Amount = amount;
            this.Currency = currency;
            this.Installments = installments;
            this.Capture = capture;
            this.SoftDescriptor = softDescriptor;
            this.CreditCard = creditCard;
            this.Country = country;
        }

        public Payment(decimal amount, Currency currency, bool capture, DebitCard debitCard, string country = Cielo.Country.BRA, string urlReturn = null)
        {
            this.Type = PaymentType.DebitCard;
            this.Amount = amount;
            this.Currency = currency;
            this.Capture = capture;
            this.DebitCard = debitCard;
            this.Country = country;
            this.ReturnUrl = urlReturn;
        }
        public PaymentType? Type { get; set; }
        [JsonConverter(typeof(CieloDecimalToIntegerConverter))]
        public decimal? Amount { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Currency? Currency { get; set; }
        public CreditCard CreditCard { get; set; }
        public DebitCard DebitCard { get; set; }
        public bool? Capture { get; set; }
        public string ReturnUrl { get; set; }
        public string Country { get; set; }
        public string Tid { get; set; }
        public string ProofOfSale { get; set; }
        public string AuthorizationCode { get; set; }
        public int? Installments { get; set; }
        public string SoftDescriptor
        {
            get
            {
                return softDescriptor;
            }
            set
            {
                if (value != null && (
                    value.Length > 13 ||
                    !softDescriptorMatch.IsMatch(value)))
                {
                    throw new ArgumentException("SoftDescriptor: it has a limit of 13 characters (not special) and no spaces.");
                }

                softDescriptor = value;
            }
        }
        public Guid? PaymentId { get; set; }
        public decimal? CapturedAmount { get; set; }
        public DateTime? CapturedDate { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonMessage { get; set; }
        public string ProviderReturnCode { get; set; }
        public string ProviderReturnMessage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status? Status { get; set; }
    }

    public class Customer
    {
        public Customer()
        {
        }

        public Customer(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? Birthdate { get; set; }
    }

    public class CreditCard
    {
        public CreditCard()
        {
        }

        public CreditCard(Guid cardToken, string securityCode, CardBrand brand)
        {
            this.CardToken = cardToken;
            this.SecurityCode = securityCode;
            this.Brand = brand;
        }

        public CreditCard(string cardNumber, string holder, DateTime expirationDate, string securityCode, CardBrand brand, bool saveCard = false)
        {
            this.CardNumber = cardNumber;
            this.Holder = holder;
            this.ExpirationDate = expirationDate;
            this.SecurityCode = securityCode;
            this.Brand = brand;
            this.SaveCard = saveCard;
        }

        public string CardNumber { get; set; }
        public Guid? CardToken { get; set; }
        public string Holder { get; set; }
        [JsonConverter(typeof(ExpirationDateConverter))]
        public DateTime? ExpirationDate { get; set; }
        public string SecurityCode { get; set; }
        public bool? SaveCard { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CardBrand? Brand { get; set; }
    }

    public class DebitCard
    {
        public DebitCard()
        {
        }

        public DebitCard(Guid cardToken, string securityCode, CardBrand brand)
        {
            this.CardToken = cardToken;
            this.SecurityCode = securityCode;
            this.Brand = brand;
        }

        public DebitCard(string cardNumber, string holder, DateTime expirationDate, string securityCode, CardBrand brand, bool saveCard = false)
        {
            this.CardNumber = cardNumber;
            this.Holder = holder;
            this.ExpirationDate = expirationDate;
            this.SecurityCode = securityCode;
            this.Brand = brand;
            this.SaveCard = saveCard;
        }

        public string CardNumber { get; set; }
        public Guid? CardToken { get; set; }
        public string Holder { get; set; }
        [JsonConverter(typeof(ExpirationDateConverter))]
        public DateTime? ExpirationDate { get; set; }
        public string SecurityCode { get; set; }
        public bool? SaveCard { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CardBrand? Brand { get; set; }
    }
    
    #region Converters
    internal class ExpirationDateConverter : IsoDateTimeConverter
    {
        public ExpirationDateConverter()
        {
            base.DateTimeFormat = "MM/yyyy";
        }
    }
    internal class CieloDecimalToIntegerConverter : JsonConverter
    {
        public override bool CanRead { get; } = true;

        public override bool CanWrite { get; } = true;

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            return NumberHelper.IntegerToDecimal(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var newValue = NumberHelper.DecimalToInteger(value);

                JToken.FromObject(newValue).WriteTo(writer);
            }
        }
    }
    internal static class NumberHelper
    {
        public static decimal IntegerToDecimal(object value)
        {
            return Convert.ToDecimal(value) / 100;
        }

        public static object DecimalToInteger(object value)
        {
            return Convert.ToInt32(Convert.ToDecimal(value) * 100);
        }
    }
    #endregion
}