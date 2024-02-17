using Coinpayments.Api;
using Core.Entities;
using Core.Helpers;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Core.Repositories.Globalizacao
{
    public class MoedaCotacaoRepository : PersistentRepository<Entities.MoedaCotacao>
    {

        public MoedaCotacaoRepository(DbContext context)
            : base(context)
        {
        }

        public MoedaCotacao GetByMoedas(string origem, string destino, MoedaCotacao.Tipos tipo)
        {
            return this.GetByExpression(c => ((c.Moeda.Sigla == origem && c.Moeda1.Sigla == destino) || (c.Moeda1.Sigla == origem && c.Moeda.Sigla == destino)) && c.TipoID == (int)tipo).OrderByDescending(c => c.Data).FirstOrDefault();
        }

        public void AddValorParaEntrada(double valor, Moeda.Moedas moeda)
        {
            if (!valor.Equals(0))
            {
                using (var transacao = new TransactionScope(TransactionScopeOption.Required))
                {
                    var moedaCotacao = MoedaCotacaoFactory(valor, MoedaCotacao.Tipos.Entrada, moeda);
                    Delete(MoedaCotacao.Tipos.Entrada, moeda);
                    this.Save(moedaCotacao);
                    transacao.Complete();
                }
            }
        }

        public void AddValorParaSaida(double valor, Moeda.Moedas moeda)
        {
            if (!valor.Equals(0))
            {
                using (var transacao = new TransactionScope(TransactionScopeOption.Required))
                {
                    var moedaCotacao = MoedaCotacaoFactory(valor, MoedaCotacao.Tipos.Saida, moeda);
                    Delete(MoedaCotacao.Tipos.Saida, moeda);
                    this.Save(moedaCotacao);
                    transacao.Complete();
                }
            }
        }

        private MoedaCotacao MoedaCotacaoFactory(double valor, MoedaCotacao.Tipos tipo, Moeda.Moedas moeda)
        {
            return new MoedaCotacao
            {
                MoedaOrigemID = (int)moeda,
                MoedaDestinoID = (int)Moeda.Moedas.USD,
                Valor = valor,
                TipoID = (int)tipo,
                Data = App.DateTimeZion
            };
        }

        private void Delete(MoedaCotacao.Tipos tipo, Moeda.Moedas moeda)
        {
            var cotacoes = this.GetByExpression(e => e.MoedaOrigemID == (int)moeda && e.MoedaDestinoID == (int)Moeda.Moedas.USD && e.TipoID == (int)tipo).ToList();
            foreach (var cotacao in cotacoes)
            {
                this.Delete(cotacao);
            }
        }

        public double GetValorParaEntradaBTCDolar()
        {
            return GetBTCDolar(MoedaCotacao.Tipos.Entrada);
        }

        public double GetValorParaSaidaBTCDolar()
        {
            return GetBTCDolar(MoedaCotacao.Tipos.Saida);
        }

        public double GetValorParaSaidaLTCDolar()
        {
            return GetLTCDolar(MoedaCotacao.Tipos.Saida);
        }

        public double GetValorParaEntradaLTCDolar()
        {
            return GetLTCDolar(MoedaCotacao.Tipos.Entrada);
        }

        private double GetBTCDolar(MoedaCotacao.Tipos tipo)
        {
            var bitcoinValue = ConfiguracaoHelper.GetDouble("COTACAO_BTC_USD_DEFAULT");

            var cotacao = this.GetByExpression(e => e.MoedaOrigemID == (int)Moeda.Moedas.BTC && e.MoedaDestinoID == (int)Moeda.Moedas.USD && e.TipoID == (int)tipo).FirstOrDefault();
            if(cotacao != null)
            {
                bitcoinValue = (double)cotacao.Valor;
            }

            return bitcoinValue;
        }

        private double GetLTCDolar(MoedaCotacao.Tipos tipo)
        {
            var valor = ConfiguracaoHelper.GetDouble("COTACAO_LTC_USD_DEFAULT");

            var cotacao = this.GetByExpression(e => e.MoedaOrigemID == (int)Moeda.Moedas.LTC && e.MoedaDestinoID == (int)Moeda.Moedas.USD && e.TipoID == (int)tipo).FirstOrDefault();
            if (cotacao != null)
            {
                valor = (double)cotacao.Valor;
            }

            return valor;
        }

        public async Task<double> GetBTCDolar1Async()
        {
            var bitcoinValue = ConfiguracaoHelper.GetDouble("COTACAO_BTC_USD_DEFAULT");

            try
            {
                var cotacao = await CoinpaymentsApiWrapper.ExchangeRatesAsHelper();
                return (double)cotacao.BtcUsd;
            }
            catch (Exception)
            {
                return bitcoinValue;
            }
        }

        public async Task<double> GetLTCDolar1Async()
        {
            var litecoinValue = ConfiguracaoHelper.GetDouble("COTACAO_BTC_USD_DEFAULT");

            try
            {
                var cotacao = await CoinpaymentsApiWrapper.ExchangeRatesAsHelper();
                return (double)cotacao.LtcUsd;
            }
            catch (Exception)
            {
                return litecoinValue;
            }
        }
        
        public async Task<double> GetTetherDolar1Async()
        {
            try
            {
                var cotacao = await CoinpaymentsApiWrapper.ExchangeRatesAsHelper();
                return (double)cotacao.UsdtUsd;
            }
            catch (Exception)
            {
                return 1;
            }
        }


    }
}
