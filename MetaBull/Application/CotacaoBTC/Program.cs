using Core.Entities;
using Core.Repositories.Globalizacao;
using CotacaoBTC.cotacao;
using CotacaoBTC.source;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace CotacaoBTC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cotação BTC");

            var context = new YLEVELEntities();

            var moedaCotacaoRepository = new MoedaCotacaoRepository(context);

            var client = new WebClient();

            var moedas = new List<Moeda.Moedas> { Moeda.Moedas.BTC, Moeda.Moedas.LTC };
            
            foreach (var moeda in moedas)
            {
                var sources = SourceFactory.Sources(moeda, client);

                var cotacoes = new CotacaoFactory(sources);

                var maxCotacao = cotacoes.Max();
                var minCotacao = cotacoes.Min();

                moedaCotacaoRepository.AddValorParaSaida(maxCotacao, moeda);
                moedaCotacaoRepository.AddValorParaEntrada(minCotacao, moeda);
            }
        }
    }
}
