using Core.Repositories.Globalizacao;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Globalizacao
{
    public class MoedaService
    {

        private MoedaRepository moedaRepository;
        private MoedaCotacaoRepository moedaCotacaoRepository;

        public MoedaService(DbContext context)
        {
            moedaRepository = new MoedaRepository(context);
            moedaCotacaoRepository = new MoedaCotacaoRepository(context);
        }

        public double Converter(string origem, string destino, Entities.MoedaCotacao.Tipos tipo, double valor)
        {
            var cotacao = moedaCotacaoRepository.GetByMoedas(origem, destino, tipo);
            if (cotacao != null)
            {
                if (cotacao.Moeda1.Sigla == origem)
                {
                    return (valor * cotacao.Valor);
                }
                else
                {
                    return (valor / cotacao.Valor);
                }
            }
            return valor;
        }

    }
}
