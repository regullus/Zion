using Core.Entities;
using Core.Repositories.Financeiro;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class BitCoinHelper
    {
        BitCoinSafeRepository bitCoinSafeRepository;
        public BitCoinHelper(DbContext context)
        {
            this.bitCoinSafeRepository = new BitCoinSafeRepository(context);
        }

        public double AjustarValor(double valorPago, int? pedidoID = null, int? bonusID = null)
        {
            double valorLiquido = valorPago;

            try
            {
                var valorPagoStr = valorPago.ToString("R");
                var valorSplit = valorPagoStr.Split(',');

                if (valorSplit[1].Length > 4)
                    valorLiquido = double.Parse(valorSplit[0] + "," + valorSplit[1].Substring(0, 4));
                else
                    valorLiquido = valorPago;

                BitCoinSafe bitCoinSafe = new BitCoinSafe()
                {
                    PedidoID = pedidoID,
                    BonusID = bonusID,
                    ValorBruto = valorPago,
                    ValorLiquido = valorLiquido,
                    Diferenca = valorPago - valorLiquido,
                    Data = App.DateTimeZion
                };

                this.bitCoinSafeRepository.Save(bitCoinSafe);

                return bitCoinSafe.ValorLiquido;
            }
            catch (Exception)
            {
                return valorLiquido;
            }
        }

        public static double ConverterLiteCoinParaBitCoin(double valorLTC, double cotacaoBTC, double cotacaoLTC)
        {
            if (cotacaoBTC.Equals(0))
            {
                throw new ArgumentException("Cotação BTC inválida");
            }

            if (cotacaoLTC.Equals(0))
            {
                throw new ArgumentException("Cotação LTC inválida");
            }

            var fator = cotacaoBTC / cotacaoLTC;

            return valorLTC / fator;
        }

    }
}
