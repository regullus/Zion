using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class InvestigacaoLog
    {
        public static void LogNivelAssociacao(int usuarioId, int etapa, string dados = null)
        {
            string file = "LogNivelAssociacao_ID_" + usuarioId;

            try
            {
                var nomeEtapa = string.Empty;

                if (etapa == 1)
                    nomeEtapa = "Entrou";
                else if (etapa == 2)
                    nomeEtapa = "Pegou pagamento";
                else if (etapa == 3)
                    nomeEtapa = "Definiu novo status";
                else if (etapa == 4)
                    nomeEtapa = "Salvou novo status";
                else if (etapa == 5)
                    nomeEtapa = "ID de pago";
                else if (etapa == 6)
                    nomeEtapa = "Ajustou valor";
                else if (etapa == 7)
                    nomeEtapa = "Erro no ajuste de valor";
                else if (etapa == 8)
                    nomeEtapa = "Gravou lançamento";
                else if (etapa == 9)
                    nomeEtapa = "Loop de status do Item";
                else if (etapa == 10)
                    nomeEtapa = "Entrou no liberar";
                else if (etapa == 11)
                    nomeEtapa = "Obteve o produto";
                else if (etapa == 12)
                    nomeEtapa = "Vai entrar no usuarioService.AssociarUsuario";
                else if (etapa == 13)
                    nomeEtapa = "Status atual do usuário";
                else if (etapa == 14)
                    nomeEtapa = "Salvou novo nível de associação";
                else if (etapa == 100)
                    nomeEtapa = "Entrou no LiberarPedido";

                string log = "Usuario: " + usuarioId + " " + etapa + " - " + nomeEtapa;

                if (dados != null)
                {
                    log += " - Dados: " + dados;
                }
         
                cpUtilities.LoggerHelper.WriteFile(log, file);
                
            }
            catch (Exception ex)
            {
                cpUtilities.LoggerHelper.WriteFile(ex.Message, file);
                
            }
        }
    }
}
