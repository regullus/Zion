using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class ProdutoOpcao : IPersistentEntity
    {

        public IEnumerable<string> Fotos
        {
            get
            {
                var caminhoFisico = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                var diretorio = Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS") + "/" + this.SKU;

                var fotos = Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, diretorio, "*.jpg");
                return fotos;
            }
        }

    }
}
