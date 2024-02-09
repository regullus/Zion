using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.Loja;
using Core.Helpers;
//using System.Data.Objects;

namespace Core.Repositories.Loja
{
    public class PedidoRepository : PersistentRepository<Entities.Pedido>
    {

        private DbContext _context;

        public PedidoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public override IQueryable<Entities.Pedido> GetAll()
        {
            return base.GetByExpression(p => p.Usuario.Oculto == false);
        }

        public Entities.Pedido GetByCodigo(string codigo)
        {
            return this.GetByExpression(p => p.Codigo == codigo).FirstOrDefault();
        }

        public Entities.Pedido GetByID(int ID)
        {
            return this.GetByExpression(p => p.ID == ID).FirstOrDefault();
        }

        public IEnumerable<Entities.Pedido> GetByArmazem(int idArmazem)
        {
            return this.GetByExpression(p => p.ArmazemEntregaID == idArmazem).ToList();
        }


        public IEnumerable<PedidoRelatorioData> GetAllDatas(PedidoPagamentoStatus.TodosStatus status)
        {
            string sql = "SELECT TOP 90 CAST(PPS.Data as date) as DataPedido , CAST( SUM(P.Total) as Float) as Total, COUNT(P.ID) as QtdePedidos FROM Loja.Pedido P ";
            sql += " INNER JOIN Usuario.Usuario U ON U.ID = P.UsuarioID";
            sql += " INNER JOIN Loja.PedidoPagamento PP ON P.ID = PP.PedidoID";
            sql += " INNER JOIN Loja.PedidoPagamentoStatus PPS ON PP.ID = PPS.PedidoPagamentoID";
            sql += " WHERE PPS.StatusID = " + status.GetHashCode();
            sql += " GROUP BY CAST(PPS.Data as date)";
            sql += " ORDER BY CAST(PPS.Data as date) DESC";

            return _context.Database.SqlQuery<PedidoRelatorioData>(sql).ToList();
        }

        public IEnumerable<PedidoRelatorioProduto> GetAllProdutosByData(PedidoPagamentoStatus.TodosStatus status, DateTime dataReferencia)
        {
            string sql = "SELECT '" + dataReferencia.ToString("yyyy-MM-dd") + "' as DataPedido, PRD.ID as idProduto, PRD.Nome as Produto, PP.MeioPagamentoID as FormaPagamento,";
            sql += " CAST(SUM(PEI.ValorUnitario * PEI.Quantidade) as Float) as Total, COUNT(PEI.ID) as QtdePedidos ";
            sql += " FROM Loja.Pedido P ";
            sql += " INNER JOIN Usuario.Usuario U ON U.ID = P.UsuarioID ";
            sql += " INNER JOIN Loja.PedidoPagamento PP ON P.ID = PP.PedidoID ";
            sql += " INNER JOIN Loja.PedidoItem PEI on P.ID = PEI.PedidoID ";
            sql += " INNER JOIN Loja.Produto PRD on PEI.ProdutoID = PRD.ID ";
            sql += " INNER JOIN Loja.PedidoPagamentoStatus PPS ON PP.ID = PPS.PedidoPagamentoID ";
            sql += " WHERE PPS.StatusID = " + status.GetHashCode();
            sql += " AND PPS.Data BETWEEN '" + dataReferencia.ToString("yyyy-MM-dd") + " 00:00:00' AND '" + dataReferencia.ToString("yyyy-MM-dd") + " 23:59:59' ";
            sql += " GROUP BY PRD.ID, ";
            sql += " PRD.Nome, ";
            sql += " PP.MeioPagamentoID  ";

            return _context.Database.SqlQuery<PedidoRelatorioProduto>(sql).ToList();
        }

        public IEnumerable<PedidoRelatorioFranqueado> GetAllFranqueadosByProdutoData(PedidoPagamentoStatus.TodosStatus status, DateTime dataReferencia, string produto, PedidoPagamento.MeiosPagamento formaPgto)
        {
            string sql = "SELECT U.Login AS Login, U.Nome as Nome, CAST(SUM(PEI.ValorUnitario * PEI.Quantidade) as Float) as Total, COUNT(PEI.ID) as QtdePedidos ";
            sql += " FROM Loja.Pedido P ";
            sql += " INNER JOIN Usuario.Usuario U ON U.ID = P.UsuarioID ";
            sql += " INNER JOIN Loja.PedidoPagamento PP ON P.ID = PP.PedidoID ";
            sql += " INNER JOIN Loja.PedidoItem PEI on P.ID = PEI.PedidoID ";
            sql += " INNER JOIN Loja.Produto PRD on PEI.ProdutoID = PRD.ID ";
            sql += " INNER JOIN Loja.PedidoPagamentoStatus PPS ON PP.ID = PPS.PedidoPagamentoID ";
            sql += " WHERE PPS.StatusID = " + status.GetHashCode();
            sql += " AND PEI.ProdutoID = '" + produto + "'";
            sql += " AND PP.MeioPagamentoID = " + formaPgto.GetHashCode();
            sql += " AND PPS.Data BETWEEN '" + dataReferencia.ToString("yyyy-MM-dd") + " 00:00:00' AND '" + dataReferencia.ToString("yyyy-MM-dd") + " 23:59:59' ";
            sql += " GROUP BY U.Login, U.Nome ORDER BY U.Login ";

            return _context.Database.SqlQuery<PedidoRelatorioFranqueado>(sql).ToList();
        }

        public PedidoUltimoStatus GetPedidoUltimoStatus(int usuarioId, int produtoId)
        {
            //string sql = "SELECT TOP 1 p.ID as PedidoId, pps.StatusID StatusId, p.DataCriacao Data ";
            //sql += " FROM Loja.Pedido p ";
            //sql += " INNER JOIN loja.PedidoItem i ON p.ID = i.PedidoID ";
            //sql += " OUTER APPLY(SELECT TOP 1 * FROM loja.PedidoPagamento WHERE PedidoID = p.ID  ORDER BY ID DESC) pp ";
            //sql += " OUTER APPLY(SELECT TOP 1 * FROM  Loja.PedidoPagamentoStatus WHERE PedidoPagamentoID = pp.ID  ORDER BY ID DESC) pps ";
            //sql += " WHERE p.usuarioid = {0} AND i.produtoID = {1} ORDER BY p.ID DESC";

            string sql = "select top 1 p.ID, sta.StatusID, sta.Data from Loja.Pedido p ";
            sql += " join Loja.PedidoItem item on item.PedidoID = p.ID ";
            sql += " join Loja.PedidoPagamento pag on pag.PedidoID = p.ID ";
            sql += " join Loja.PedidoPagamentoStatus sta on sta.PedidoPagamentoID = pag.ID ";
            sql += " WHERE p.usuarioid = {0} AND item.produtoID = {1} order by p.ID DESC, sta.DATA DESC";

            sql = string.Format(sql, usuarioId, produtoId);

            return _context.Database.SqlQuery<PedidoUltimoStatus>(sql).FirstOrDefault();
        }

        public List<int> GetPedidosVencidos()
        {
            string sql = "SELECT p.ID as PedidoId ";
            sql += " FROM Loja.Pedido p ";
            sql += " INNER JOIN loja.PedidoItem i ON p.ID = i.PedidoID ";
            sql += " OUTER APPLY(SELECT TOP 1 * FROM loja.PedidoPagamento WHERE PedidoID = p.ID ORDER BY ID DESC) pp ";
            sql += " OUTER APPLY(SELECT TOP 1 * FROM  Loja.PedidoPagamentoStatus WHERE PedidoPagamentoID = pp.ID ORDER BY ID  DESC) pps ";
            sql += " WHERE pps.StatusID = {0} AND DATEDIFF(hour, p.DataCriacao, dbo.getDateZion()) >= {1} ";
            sql += " ORDER BY p.DataCriacao DESC ";

            sql = string.Format(sql, (int)PedidoPagamentoStatus.TodosStatus.AguardandoPagamento, ConfiguracaoHelper.GetString("PEDIDO_EXPIRADOS_HORAS"));

            return _context.Database.SqlQuery<int>(sql).ToList();
        }


    }
}
