using PayPal.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AreaRestrita.Helpers
{
    public class PayPalHelper
    {

        public static LocaleCode GetLocaleCodeByString(string code)
        {
            var localeCode = LocaleCode.PORTUGAL;
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

        public static CurrencyCode GetCurrencyCodeByString(string code)
        {
            var currencyCode = CurrencyCode.EURO;
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