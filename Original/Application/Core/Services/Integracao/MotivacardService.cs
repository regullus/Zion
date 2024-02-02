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

namespace Core.Services.Integracao
{
    public class MotivacardService
    {

        private MotivacardRepository motivacardRepository;

        public MotivacardService(DbContext context)
        {
            motivacardRepository = new MotivacardRepository(context);
        }

        public bool Solicitar(Motivacard cartao)
        {
            var chave = ConfiguracaoHelper.GetString("MOTIVACARD_CHAVE");
            //var chave = "fbk-2015-amc";
            var serializer = new JavaScriptSerializer();
            var client = new WebClient();

            var cpf = cartao.CPF.Length == 11 ? String.Format("{0}.{1}.{2}-{3}", cartao.CPF.Substring(0, 3), cartao.CPF.Substring(3, 3), cartao.CPF.Substring(6, 3), cartao.CPF.Substring(9, 2)) : cartao.CPF;
            var cep = cartao.Endereco.CodigoPostal.Length == 8 ? String.Format("{0}-{1}", cartao.Endereco.CodigoPostal.Substring(0, 5), cartao.CPF.Substring(5, 3)) : cartao.Endereco.CodigoPostal;

            var values = new NameValueCollection(){
                { "chave", chave },
                { "nome", cartao.Nome },
                { "cpf", cpf },
                { "nome_da_mae", cartao.NomeMae },
                { "email", cartao.Email },
                { "telefone", cartao.Telefone },
                { "celular", cartao.Celular },
                { "endereco_cep", cep },
                { "endereco_logradouro", cartao.Endereco.Logradouro },
                { "endereco_numero", cartao.Endereco.Numero },
                { "endereco_complemento", cartao.Endereco.Complemento },
                { "endereco_bairro", cartao.Endereco.Distrito },
                { "endereco_cidade", cartao.Endereco.Cidade.Nome },
                { "endereco_uf", cartao.Endereco.Estado.Sigla },
                { "id_usuario", cartao.ID.ToString() },
            };
            var response = client.UploadValues("http://motivacard.ganhamais.com.br/api/v1/cartao/solicitacao", values);
            var dados = Encoding.Default.GetString(response);
            dynamic objeto = serializer.DeserializeObject(dados);
            if (objeto["sucesso"] != 1)
            {
                var mensagem = new StringBuilder();
                mensagem.Append(objeto["mensagem"]);
                var extras = (Dictionary<string, object>)objeto["extras"];
                foreach (var extra in extras)
                {
                    mensagem.AppendFormat(" {0}", ((object[])extra.Value).First());
                }
                throw new Exception(mensagem.ToString());
            }
            return true;
        }

        public bool Desbloquear(Motivacard cartao)
        {
            var chave = ConfiguracaoHelper.GetString("MOTIVACARD_CHAVE");
            //var chave = "fbk-2015-amc";
            var serializer = new JavaScriptSerializer();
            var client = new WebClient();
            var values = new NameValueCollection(){
                { "chave", chave },
                { "nsu", cartao.NSU },
                { "id_usuario", cartao.ID.ToString() },
            };
            var response = client.UploadValues("http://motivacard.ganhamais.com.br/api/v1/cartao/desbloqueio", values);
            var dados = Encoding.Default.GetString(response);
            dynamic objeto = serializer.DeserializeObject(dados);
            if (objeto["sucesso"] != 1)
            {
                throw new Exception(objeto["mensagem"]);
            }
            return true;
        }

    }
}
