using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
   public partial class Documento : IPersistentEntity
   {

      public enum Tipos
      {
         [Description("Documento de Identificação")]
         DocumentoIdentificacao = 1,
         Passaporte = 2,
         [Description("Comprovante de Residência")]
         ComprovanteResidencia = 3,
         [Description("Comprovante de Pagamento")]
         ComprovantePagamento = 4,
         [Description("Nota Fiscal")]
         NotaFiscal = 5,
         [Description("Documentos empresa")]
         DocumentosEmpresa = 6
      }

      public Tipos Tipo
      {
         get { return (Tipos)this.TipoID; }
         set { this.TipoID = (int)value; }
      }

      public string Arquivo
      {
         get
         {
            string arquivos = null;
            var caminhoFisico = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
            var diretorio = Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS_USUARIOS");
            string url = Helpers.ConfiguracaoHelper.GetString("URL_DOCUMENTOS");
            var retorno = Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, diretorio, this.ID + ".*");
            arquivos = retorno.Any() ? retorno.FirstOrDefault() : null;

            if (!String.IsNullOrEmpty(url))
            {
               arquivos = url + arquivos;
            }
            return arquivos;
         }
      }

        public string CaminhoVirtual
        {
            get
            {
                string caminhoFisico  = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                string caminhoVirtual = Helpers.ConfiguracaoHelper.GetString("DOMINIO") + Helpers.ConfiguracaoHelper.GetString("URL_CDN");
                string diretorio      = Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS_USUARIOS");
           
                var retorno = Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, this.ID + ".*");

                return  retorno.Any() ? retorno.FirstOrDefault() : null;           
            }
        }

    }
}
