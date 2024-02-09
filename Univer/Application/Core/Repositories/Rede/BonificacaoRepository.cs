using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class BonificacaoRepository : PersistentRepository<Entities.Bonificacao>
    {
        private DbContext _context;
        public BonificacaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public IEnumerable<Entities.Bonificacao> GetByUsuario(int usuarioID, int? categoriaID = null, DateTime? dataInicial = null, DateTime? dataFinal = null)
        {
            var bonificacoes = base.GetByExpression(b => b.UsuarioID == usuarioID);

            if (categoriaID.HasValue)
            {
                bonificacoes = bonificacoes.Where(b => b.CategoriaID == categoriaID.Value);
            }

            if (dataInicial.HasValue)
            {
                bonificacoes = bonificacoes.Where(b => b.Data >= dataInicial.Value);
            }

            if (dataFinal.HasValue)
            {
                bonificacoes = bonificacoes.Where(b => b.Data <= dataFinal.Value);
            }

            return bonificacoes;
        }

        public decimal GetValorTotalBonusGlobal(DateTime dataReferencia)
        {
            string sql = "SELECT isnull(Total, 0) as Valor FROM [dbo].fn_GetValorTotalVendas('" + dataReferencia.ToString("yyyy-MM-dd") + "')";
            return _context.Database.SqlQuery<decimal>(sql).FirstOrDefault();
        }

        public IEnumerable<RelatorioBonificacaoGlobalAssociacao> GetQtdeAssociacoesBonusGlobal(DateTime dataReferencia)
        {
            string sql = "select NivelAssociacao, Total from [dbo].fn_GetAtivosData('" + dataReferencia.ToString("yyyy-MM-dd") + "')";
            return _context.Database.SqlQuery<RelatorioBonificacaoGlobalAssociacao>(sql).ToList();
        }

        public void GerarBonusGlobal(DateTime dataReferencia, float valorTotal, int qtdeNivel1, int qtdeNivel2, int qtdeNivel3, int qtdeNivel4, int qtdeNivel5)
        {
            _context.Database.ExecuteSqlCommand("EXEC sp_BonusGlobal @data, @valor, @qtde1, @qtde2, @qtde3, @qtde4, @qtde5",
                new SqlParameter("@data", dataReferencia),
                new SqlParameter("@valor", valorTotal),
                new SqlParameter("@qtde1", qtdeNivel1),
                new SqlParameter("@qtde2", qtdeNivel2),
                new SqlParameter("@qtde3", qtdeNivel3),
                new SqlParameter("@qtde4", qtdeNivel4),
                new SqlParameter("@qtde5", qtdeNivel5)
                );
        }

        public void GerarBonusAtivoMensal(int idUsuario, int NivelAssociacao)
        {
            _context.Database.ExecuteSqlCommand("EXEC sp_BonusAtivoMensal @idUsuarioOrigem, @NivelAssociacao",
                new SqlParameter("@idUsuarioOrigem", idUsuario),
                new SqlParameter("@NivelAssociacao", NivelAssociacao)
                );
        }

        public void AnalisaQualificacao(int idUsuario, int idPedido)
        {
            _context.Database.ExecuteSqlCommand("EXEC spDG_US_Qualificacao @UsuarioId, @PedidoId",
                new SqlParameter("@UsuarioId", idUsuario),
                new SqlParameter("@PedidoId", idPedido)
                );
        }

        public void GerarBonusEquipe(int idUsuario, int idPedido)
        {
            _context.Database.ExecuteSqlCommand("EXEC spDG_RE_GeraBonusEquipe @UsuarioId, @PedidoId",
                new SqlParameter("@UsuarioId", idUsuario),
                new SqlParameter("@PedidoId", idPedido)
                );
        }

        public void GerarBonusIndicacaoDireta(int idUsuario, int idPedido)
        {
            _context.Database.ExecuteSqlCommand("EXEC spDG_RE_GeraBonusIndicacao @UsuarioId, @PedidoId",
                new SqlParameter("@UsuarioId", idUsuario),
                new SqlParameter("@PedidoId", idPedido)
                );
        }
    }
}

