using CotacaoBTC.source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CotacaoBTC.cotacao
{
    public class CotacaoFactory
    {
        List<double> Cotacoes = new List<double>();

        public CotacaoFactory(List<ISource> sources)
        {
            ObtemCotacoes(sources);
        }

        private void ObtemCotacoes(List<ISource> sources)
        {
            foreach (var source in sources)
            {
                try
                {
                    Console.WriteLine("Buscando cotação em {0}", source.Name);

                    var cotacao = source.ObtemCotacao();

                    Console.WriteLine("- Valor {0}", cotacao.ToString());

                    if (!cotacao.Equals(0))
                    {
                        Cotacoes.Add(cotacao);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Erro no source {0}", source.Name) ;
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public double Max()
        {
            if (Cotacoes.Any())
            {
                return Cotacoes.Max();
            }
            else
            {
                return 0;
            }
        }

        public double Min()
        {
            if (Cotacoes.Any())
            {
                return Cotacoes.Min();
            }
            else
            {
                return 0;
            }
        }


    }
}
