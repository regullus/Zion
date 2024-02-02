using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Produto : IPersistentEntity
    {
        public enum Tipos
        {
            [Description("Associação")]
            Associacao = 1,
            [Description("Upgrade")]
            Upgrade = 2,
            [Description("Produto Físico")]
            ProdutoFisico = 3,
            [Description("Paquete Complementario")]
            ProdutoVirtual = 4,
            [Description("Ativo Mensal")]
            AtivoMensal = 5,
            [Description("Renovacao Assinatura")]
            RenovacaoAssinatura = 6
        }

        public Tipos Tipo
        {
            get { return (Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

        public IEnumerable<ProdutoValor> Valores(Entities.Usuario usuario)
        {
            var valores = this.ProdutoValor.Where(p =>
                (!p.AssociacaoID.Any() || p.AssociacaoID.Contains(usuario.NivelAssociacao)) &&
                (!p.BlocoID.Any() || p.BlocoID.Contains(usuario.Pais.BlocoID)) &&
                (!p.ClassificacaoID.Any() || p.ClassificacaoID.Contains(usuario.NivelClassificacao)) &&
                (!p.PaisID.Any() || p.PaisID.Contains(usuario.PaisID))
            );
            return valores;
        }

        public IEnumerable<ProdutoItem> Itens()
        {
            var itens = this.ProdutoItem.Where(p => p.ProdutoID == this.ID);
            return itens;
        }

        public ProdutoValor ValorMinimo(Entities.Usuario usuario)
        {
            //var valores = Valores(usuario).OrderBy(v => v.Valor);
            var preco = Valores(usuario).OrderBy(v => v.Valor).FirstOrDefault();

            if (usuario != null && this.TipoID == (int)Tipos.RenovacaoAssinatura)
            {
                double fator = Convert.ToDouble(Helpers.ConfiguracaoHelper.GetString("FATOR_RENOVACAO_ASSOCIACAO").Replace(".", ","));
                if (fator > 0)
                {
                    var ganho = usuario.UsuarioGanho.OrderByDescending(g => g.ID).FirstOrDefault();
                    var valor = ganho.AcumuladoGanho * fator;

                    if (valor > preco.Valor)
                        preco.Valor = valor;
                }
            }

            return preco; /// valores.FirstOrDefault();    
        }

        public ProdutoValor ValorMaximo(Entities.Usuario usuario)
        {
            var valores = Valores(usuario).OrderByDescending(v => v.Valor);
            return valores.FirstOrDefault();
        }

        public IEnumerable<string> Fotos
        {
            get
            {
                //string caminhoFisico  = Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                string caminhoFisico  = System.Web.HttpContext.Current.Server.MapPath("~/");

                string caminhoVirtual = Helpers.ConfiguracaoHelper.GetString("DOMINIO") + Helpers.ConfiguracaoHelper.GetString("URL_CDN");               
                string diretorio      = Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS") + this.SKU + "/";

                List<string> fotos = new List<string>();
                fotos.AddRange(Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, "*.jpg"));
                fotos.AddRange(Repositories.Sistema.ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, "*.png"));

                return fotos;
            }
        }

    }
}