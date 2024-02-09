using Core.Entities;
using Core.Helpers;
using Core.Repositories.Integracao;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Core.Repositories.Loja;

namespace Core.Services.Integracao
{
    public class CorreiosService
    {
        private PedidoRepository pedidoRepository;

        public static CorreioFrete Calcular(string cepOrigem, string cepDestino, float peso, decimal comprimento, decimal altura, decimal largura, double? valor = null)
        {
            var retorno = new CorreioFrete();

            //Valores padrão do serviço do Correio
            string maoPropria = "N"; //Não será entregue em mãos
            int formatoEmbalagem = 1; //Formato caixa/pacote
            decimal valorDeclarado = 0; //Não terá o valor declarado
            string avisoRecebimento = "N"; //Não será informado o recibemento do pedido

            if (!ConfiguracaoHelper.TemChave("FRETE_HABILITADO") || !ConfiguracaoHelper.GetBoolean("FRETE_HABILITADO"))
            {
                return retorno;
            }

            if (string.IsNullOrWhiteSpace(cepOrigem))
            {
                cepOrigem = ConfiguracaoHelper.GetString("FRETE_CEP_ORIGEM");
            }

            cepDestino = cepDestino.Replace("-", "");

            var ws = new br.com.correios.ws.CalcPrecoPrazoWS();
            ws.Url = "http://ws.correios.com.br/calculador/CalcPrecoPrazo.asmx";

            //Código da empresa junto ao Correio
            var codEmpresa = ConfiguracaoHelper.GetString("CORREIO_CODIGO_CLIENTE");
            //Senha da empresa junto ao Correio
            var senhaEmpresa = ConfiguracaoHelper.GetString("CORREIO_SENHA_CLIENTE");

            /* Tabela de Códigos de Serviços de Entrega vigentes do Correio
                04014 SEDEX à vista
                04065 SEDEX à vista pagamento na entrega
                04510 PAC à vista
                04707 PAC à vista pagamento na entrega
                40169 SEDEX 12 ( à vista e a faturar)
                40215 SEDEX 10 (à vista e a faturar)*
                40290 SEDEX Hoje Varejo*
            */
            var codServico = ConfiguracaoHelper.GetString("CORREIO_CODIGO_SERVICO");

            /* Tabela de Códigos de Formatos de Embalagem
                1 – Formato caixa/pacote
                2 – Formato rolo/prisma
                3 - Envelope             
            */
            if (ConfiguracaoHelper.TemChave("CORREIO_TIPO_EMBALAGEM"))
                formatoEmbalagem = ConfiguracaoHelper.GetInt("CORREIO_TIPO_EMBALAGEM");

            //Informa se a encomenda será entregue em mãos pelo Correio (S/N)
            if (ConfiguracaoHelper.TemChave("CORREIO_MAO_PROPRIA"))
                maoPropria = ConfiguracaoHelper.GetString("CORREIO_MAO_PROPRIA");

            //Informa se a encomenda terá o valor declarado pelo Correio (S/N)
            if (ConfiguracaoHelper.TemChave("CORREIO_VALOR_DECLARADO"))
                valorDeclarado = ConfiguracaoHelper.GetString("CORREIO_MAO_PROPRIA") == "N" ? 0 : (valor.HasValue ? Convert.ToDecimal(valor.Value) : 0);

            //Informa se o Correio informará o recebimento (S/N)
            if (ConfiguracaoHelper.TemChave("CORREIO_AVISO_RECEBIMENTO"))
                avisoRecebimento = ConfiguracaoHelper.GetString("CORREIO_AVISO_RECEBIMENTO");

            var retWS = ws.CalcPrecoPrazo(codEmpresa, senhaEmpresa, codServico, cepOrigem, cepDestino, peso.ToString(), formatoEmbalagem, comprimento,
                    altura, largura, 0, maoPropria, valorDeclarado, avisoRecebimento);

            if(retWS != null && retWS.Servicos.Count() > 0)
            {
                var frete = retWS.Servicos.FirstOrDefault();
                if(!string.IsNullOrWhiteSpace(frete.Erro))
                {
                    retorno.PrazoDias = int.Parse(frete.PrazoEntrega);
                    retorno.Valor = float.Parse(frete.Valor.ToString().Replace(",","."));
                }
            }

            if (retorno.Valor > 200)
            {
                retorno.Valor = retorno.Valor / 100;
            }

            return retorno;
        }
    }
}
