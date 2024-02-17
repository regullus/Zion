using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class ProdutoRepository : PersistentRepository<Entities.Produto>
    {
        DbContext _context;

        public ProdutoRepository(DbContext context)
           : base(context)
        {
            _context = context;
        }

        public Entities.Produto GetBySKU(string sku)
        {
            return GetDisponiveis().Where(p => p.SKU == sku).FirstOrDefault();
        }

        public IEnumerable<Entities.Produto> GetDisponiveis()
        {
            return this.GetByExpression(p => ((p.ControlaEstoque && p.Estoque > 0) || !p.ControlaEstoque) && p.Publicado && p.VendaDireta);
        }
        public IEnumerable<Entities.Produto> GetAtivacao()
        {
            return this.GetByExpression(p => ((p.ControlaEstoque && p.Estoque > 0) || !p.ControlaEstoque) && p.Publicado && p.VendaDireta && p.TipoID == (int)Entities.Produto.Tipos.Associacao);
        }
        public IEnumerable<Entities.Produto> GetUpgrade(int pNivel)
        {
            return this.GetByExpression(p => ((p.ControlaEstoque && p.Estoque > 0) || !p.ControlaEstoque) &&
                                               p.Publicado &&
                                               p.VendaDireta &&
                                               p.TipoID == (int)Entities.Produto.Tipos.Upgrade &&
                                               p.NivelAssociacao > pNivel);
        }

        public IEnumerable<Entities.Produto> GetPacotesComplmentares(int pNivel)
        {
            return GetDisponiveis().Where(p => p.TipoID == (int)Entities.Produto.Tipos.ProdutoVirtual && p.Peso <= 1000 && p.NivelAssociacao < pNivel);
        }

        public IEnumerable<Entities.Produto> GetRenovacao(int pNivel)
        {
            return this.GetByExpression(p => ((p.ControlaEstoque && p.Estoque > 0) || !p.ControlaEstoque) &&
                                               p.Publicado &&
                                               p.VendaDireta &&
                                               p.TipoID == (int)Entities.Produto.Tipos.RenovacaoAssinatura &&
                                               p.NivelAssociacao == pNivel);
        }

        public IEnumerable<Entities.Produto> GetByTipo(Entities.Produto.Tipos tipo)
        {
            return GetDisponiveis().Where(p => p.TipoID == (int)tipo);
        }

        public IEnumerable<Entities.Produto> GetProdutos()
        {
            return GetDisponiveis().Where(p => p.TipoID == (int)Entities.Produto.Tipos.ProdutoFisico || p.TipoID == (int)Entities.Produto.Tipos.ProdutoVirtual);
        }

        public IEnumerable<Entities.Produto> GetByTipoCadastrado(Entities.Produto.Tipos tipo)
        {
            return this.GetByExpression(p => p.TipoID == (int)tipo);
        }

        /// <summary>
        /// Retorna o valor de Bonificação de um dado nivel
        /// </summary>
        /// <param name="strNivel">Nível para obter a bonificacao</param>
        /// <returns>valor da bonificação</returns>
        public double Bonificacao(string strNivel)
        {
            double? valor = 0;
            if (!String.IsNullOrEmpty(strNivel))
            {
                try
                {
                    Entities.Produto produto = new Entities.Produto();
                    produto = base.GetByExpression(p => p.Nome == strNivel).FirstOrDefault();
                    Core.Entities.ProdutoValor produtoValor = produto.ProdutoValor.FirstOrDefault();
                    valor = produtoValor.Bonificacao;
                }
                catch (Exception ex)
                {
                    //sem dados
                    cpUtilities.LoggerHelper.WriteFile("Bonificacao: " + ex.Message, "CoreProdutoRepository");
                }
            }
            return (double)valor;
        }

        public double Valor(int nivelAssociacao)
        {
            double? valor = 0;
            if (!nivelAssociacao.Equals(0))
            {
                try
                {
                    var produto = base.GetByExpression(p => p.NivelAssociacao == nivelAssociacao).FirstOrDefault();
                    var produtoValor = produto.ProdutoValor.FirstOrDefault();
                    valor = produtoValor.Valor;
                }
                catch (Exception)
                {
                    //sem dados
                }
            }
            return (double)valor;
        }

        public IEnumerable<Entities.Produto> GetProdutoSemValor(string nivelProduto)
        {
            string sql =
              "Exec spOC_LO_ObtemProdutoSemValor " + nivelProduto;

            return _context.Database.SqlQuery<Entities.Produto>(sql).ToList();
        }

        public IEnumerable<Entities.Produto> GetSemComposicaoByCategoria(int pCategoriaId)
        {
            return this.GetByExpression(p => p.ProdutoCategoriaID == pCategoriaId && p.Composto == false).OrderBy(p => p.Nome);
        }

        public IEnumerable<Entities.Produto> GetSemComposicaoByCategoria(int pProdutoPaiId, int pCategoriaId)
        {
            string sql =
                "Select * " +
                "from Loja.Produto P (nolock) " +
                "where P.ProdutoCategoriaID = " + pCategoriaId +
                "  and P.Composto = 0 " +
                "  and P.ID not in (Select I.ItemID from Loja.ProdutoItem I (nolock) where I.ProdutoID = " + pProdutoPaiId + ")" +
                "Order by P.Nome";
 
            return _context.Database.SqlQuery<Entities.Produto>(sql).ToList();
        }
    }
}
