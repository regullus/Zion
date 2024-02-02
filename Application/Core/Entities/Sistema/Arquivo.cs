using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Arquivo : IPersistentEntity
    {

        public string CaminhoVirtual
        {
            get
            {
                string caminhoFisico = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                string caminhoVirtual = Helpers.ConfiguracaoHelper.GetString("DOMINIO");
                string diretorio = this.ArquivoSecao.Caminho;
                
                var retorno = Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, this.Nome);

                return retorno.Any() ? retorno.FirstOrDefault() : " ";
            }
        }

    }
}
