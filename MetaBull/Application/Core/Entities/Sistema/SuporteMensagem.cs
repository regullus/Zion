using DomainExtension.Entities.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Core.Entities
{
    public partial class SuporteMensagem : IPersistentEntity
    {
        public List<string> Imagens { get; set; }

        private string caminhoFisico { get; set; }
        private string caminhoVirtual { get; set; }

        public SuporteMensagem()
        {
            Imagens = new List<string>();

            caminhoFisico = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
            caminhoVirtual = Helpers.ConfiguracaoHelper.GetString("DOMINIO") + Helpers.ConfiguracaoHelper.GetString("URL_CDN");
        }

        public void ObtemImagens()
        {
            //string caminhoFisico  = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");

            string diretorio = Path.Combine(Helpers.ConfiguracaoHelper.GetString("PASTA_SUPORTE_ANEXOS"), this.Guid.ToString());

            Imagens.AddRange(Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, "*.jpg"));
            Imagens.AddRange(Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, "*.png"));
            Imagens.AddRange(Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, "*.jpeg"));
        }

    }

}
