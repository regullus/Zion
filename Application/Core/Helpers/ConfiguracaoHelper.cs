using Core.Repositories.Sistema;
using Core.Repositories.Globalizacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cpUtilities;
using System.Configuration;

namespace Core.Helpers
{
    public class ConfiguracaoHelper
    {
        private static ConfiguracaoRepository _configuracaoRepository;
        private static ConfiguracaoRepository configuracaoRepository
        {
            get
            {
                if (_configuracaoRepository == null)
                {
                    _configuracaoRepository = new ConfiguracaoRepository(new Entities.YLEVELEntities());
                }
                return _configuracaoRepository;
            }
        }

        private static MoedaRepository _moedaRepository;
        private static MoedaRepository moedaRepository
        {
            get
            {
                if (_moedaRepository == null)
                {
                    _moedaRepository = new MoedaRepository(new Entities.YLEVELEntities());
                }
                return _moedaRepository;
            }
        }

        public static bool TemChave(string chave)
        {
            return configuracaoRepository.GetByChave(chave) != null;
        }

        public static string GetString(string chave)
        {
            var configuracao = configuracaoRepository.GetByChave(chave);
            return configuracao != null ? configuracao.Dados : "";
        }

        public static int GetInt(string chave)
        {
            var dados = GetString(chave);
            var inteiro = 0;
            int.TryParse(dados, out inteiro);
            return inteiro;
        }

        public static double GetDouble(string chave)
        {
            var dados = GetString(chave);
            var value = 0.0;
            double.TryParse(dados, out value);
            return value;
        }

        public static decimal GetDecimal(string chave)
        {
            var dados = GetString(chave);
            decimal value = 0;
            decimal.TryParse(dados, out value);
            return value;
        }

        public static bool GetBoolean(string chave)
        {
            var dados = GetString(chave);
            var boolean = false;
            Boolean.TryParse(dados, out boolean);
            return boolean;
        }

        public static void Replace(ref string texto)
        {
            var regex = new Regex(@"\[[A-Za-z0-9_]+\]");
            var matches = regex.Matches(texto);
            for (var i = 0; i < matches.Count; i++)
            {
                var chave = matches[i].Value.Replace("[", "").Replace("]", "");
                if (TemChave(chave))
                {
                    texto = texto.Replace(matches[i].Value, GetString(chave));
                }
            }
        }

        public static Entities.Moeda GetMoedaPadrao()
        {
            var moeda = moedaRepository.GetByID(GetInt("MOEDA_PADRAO_ID"));
            if (moeda == null)
                moeda = new Entities.Moeda();

            return moeda;
        }

        public static string GetCarteira(string moeda)
        {
            string dados = "";
            string dados1 = "dados1";
            string dados2 = "dados2";
            string dados3 = "";
            string carteira = "";

            switch (moeda)
            {
                case "BTC":
                    carteira = GetString("CARTEIRA_FREE_BTC");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "qGKz1GpogCHL", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["btc"], "CHLqGKz1Gpog", TipoCriptografia.Descriptografa);
                        dados1 = cpUtilities.Criptografia.Descriptografar(dados1);
                        dados2 = cpUtilities.Criptografia.Descriptografar(dados2);
                    }
                    if (dados1 != dados2)
                    {
                        //Não deve passar por aqui - é só uma contingencia
                        dados3 = "005024042071035044085115029069007049038120006034020061063093023095046000104038021037049031003123003069008002005036120027020024046000055091041003011031034000044027035030115038008013117014058057";
                        dados = cpUtilities.Gerais.Morpho(dados3, "CHLqGKz1Gpog", TipoCriptografia.Descriptografa);
                        dados = cpUtilities.Criptografia.Descriptografar(dados);
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                case "LTC":
                    carteira = GetString("CARTEIRA_FREE_LTC");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "qGKz1GpogCHL", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["ltc"], "CHLqGKz1Gpog", TipoCriptografia.Descriptografa);
                        dados1 = cpUtilities.Criptografia.Descriptografar(dados1);
                        dados2 = cpUtilities.Criptografia.Descriptografar(dados2);
                    }
                    if (dados1 != dados2)
                    {
                        //Não deve passar por aqui - é só uma contingencia
                        dados3 = "002126051023082041039055014113009032052048096034105045023013022027103031011038096048093031002044012009126024052053030072091001020093045108113045023020027076127005020057004122032034055061063027";
                        dados = cpUtilities.Gerais.Morpho(dados3, "qGKz1GpogCHL", TipoCriptografia.Descriptografa);
                        dados = cpUtilities.Criptografia.Descriptografar(dados);
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                case "USDT":
                    carteira = GetString("CARTEIRA_FREE_USDT");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "qGKz1GpogCHL", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["usdt"], "CHLqGKz1Gpog", TipoCriptografia.Descriptografa);
                        //verificar porque nao tratou em branco
                        dados1 = cpUtilities.Criptografia.Descriptografar(dados1);
                        dados2 = cpUtilities.Criptografia.Descriptografar(dados2);
                    }
                    if (dados1 != dados2)
                    {
                        //Não deve passar por aqui - é só uma contingencia
                        dados3 = "008104042077008049066032063002031127052017038076089119023039053119043103033032062024117003091054083048005040020018015021122049008008014041126059070010019010093022077082";
                        dados = cpUtilities.Gerais.Morpho(dados3, "qGKz1GpogCHL", TipoCriptografia.Descriptografa);
                        dados = cpUtilities.Criptografia.Descriptografar(dados);
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                default:
                    break;
            }

            return dados;
        }

        public static string GetCarteiraCliente(string moeda)
        {
            //IMPORTANTE - CONFIGURAR
            string dados = "";
            string dados1 = "dados1";
            string dados2 = "dados2";
            string carteira = "";

            switch (moeda)
            {
                case "BTC":
                    carteira = GetString("CARTEIRA_CLIENTE_BTC");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "BpCOdInt2uxx", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["btccli"], "2uxxBpCOdInt", TipoCriptografia.Descriptografa);
                    }
                    if (dados1 != dados2)
                    {
                        dados = "";
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                case "LTC":
                    carteira = GetString("CARTEIRA_CLIENTE_LTC");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "BpCOdInt2uxx", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["ltccli"], "2uxxBpCOdInt", TipoCriptografia.Descriptografa);
                    }
                    if (dados1 != dados2)
                    {
                        dados = "";
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                case "USDT":
                    
                    carteira = GetString("CARTEIRA_CLIENTE_USDT");
                    if (!String.IsNullOrEmpty(carteira))
                    {
                        dados1 = cpUtilities.Gerais.Morpho(carteira, "BpCOdInt2uxx", TipoCriptografia.Descriptografa);
                        dados2 = cpUtilities.Gerais.Morpho(ConfigurationManager.AppSettings["usdtcli"], "2uxxBpCOdInt", TipoCriptografia.Descriptografa);
                    }

                    if (dados1 != dados2)
                    {
                        dados = "";
                    }
                    else
                    {
                        dados = dados1;
                    }
                    break;
                default:
                    break;
            }
            if (!String.IsNullOrEmpty(dados))
            {
                dados = cpUtilities.Criptografia.Descriptografar(dados);
            }
            return dados;
        }

        public static string CriptoCarteira(string carteira)
        {

            string dados = cpUtilities.Criptografia.Criptografar(carteira);
            dados = cpUtilities.Gerais.Morpho(dados, "qGKz1GpogCHL", TipoCriptografia.Criptografa);

            string dados2 = cpUtilities.Criptografia.Criptografar(carteira);
            dados2 = cpUtilities.Gerais.Morpho(dados2, "CHLqGKz1Gpog", TipoCriptografia.Criptografa);

            return dados;
        }

        public static string CriptoCarteiraCliente(string carteira)
        {
            string dados = cpUtilities.Criptografia.Criptografar(carteira);
            dados = cpUtilities.Gerais.Morpho(dados, "BpCOdInt2uxx", TipoCriptografia.Criptografa);

            string dados2 = cpUtilities.Criptografia.Criptografar(carteira);
            dados2 = cpUtilities.Gerais.Morpho(dados2, "2uxxBpCOdInt", TipoCriptografia.Criptografa);

            return dados;
        }

    }
}
