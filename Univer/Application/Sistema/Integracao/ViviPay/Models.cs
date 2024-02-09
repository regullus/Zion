using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Sistema.Integracao.Models.ViviPay
{
    public class transaction
    {
        public string conta_token { get; set; }
        public checkout_data checkout_data { get; set; }
    }

    public class payment
    {
        public payment() { }

        public card card { get; set; }
        public decimal total_value { get; set; }
        public string method_payment { get; set; }
        public int installments { get; set; }
    }

    public class checkout_data
    {
        public checkout_data() { }

        public payment payment { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string cpf { get; set; }
        public string area_code { get; set; }
        public string phone { get; set; }
        public string description { get; set; }
    }

    public class card
    {
        public card() { }

        public string number { get; set; }
        public string brand { get; set; }
        public string cvv { get; set; }
        public string month { get; set; }
        public string year { get; set; }
    }

    public class installmentsTransaction
    {
        public bool error { get; set; }
        public string message { get; set; }
        public installments installments { get; set; }
    }

    public class installments
    {
        public List<brandCard> mastercard { get; set; }
        public List<brandCard> visa { get; set; }
        public List<brandCard> amex { get; set; }
        public List<brandCard> elo { get; set; }
        public List<brandCard> diners { get; set; }
    }

    public class brandCard
    {
        public int quantity { get; set; }
        public decimal installmentAmount { get; set; }
        public decimal totalAmount { get; set; }
        public bool interestFree { get; set; }
    }
}