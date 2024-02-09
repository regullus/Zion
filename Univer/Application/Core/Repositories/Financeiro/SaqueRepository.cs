using Core.Helpers;
using Core.Models.Financeiro;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class SaqueRepository : PersistentRepository<Entities.Saque>
    {
        private DbContext _context;
        public SaqueRepository(DbContext context)
            : base(context)
        {
            this._context = context;
        }

        public IEnumerable<Entities.Saque> GetByUsuario(int usuarioID)
        {
            return base.GetByExpression(s => s.UsuarioID == usuarioID);
        }

        public IEnumerable<Entities.Saque> GetByStatus(Entities.SaqueStatus.TodosStatus status)
        {
            return base.GetByExpression(s => s.SaqueStatus.OrderByDescending(sta => sta.Data).FirstOrDefault().StatusID == (int)status);
        }

        public List<Entities.Saque> GetByUltimoStatus(Entities.SaqueStatus.TodosStatus status)
        {
            string sql =
               "Select top 1 s.* " +
               "from Financeiro.Saque (nolock) s " +
               "   inner join Financeiro.SaqueStatus (nolock) ss on s.ID = ss.SaqueID " +
               "where ss.StatusID != " + (int)status +
               "  and ss.Ultimo = 1 ";

            return _context.Database.SqlQuery<Entities.Saque>(sql).ToList();
        }

        public List<Entities.Saque> GetForToday(int usuarioID, int ContaId)
        {
            string sql =
               "Select top 1 s.* " +
               "from Financeiro.Saque (nolock) s " +
               "   inner join Financeiro.SaqueStatus (nolock) ss on s.ID = ss.SaqueID " +
               "   inner join Financeiro.Lancamento (nolock) l on s.ID = l.ReferenciaID " +
               "where s.UsuarioID = " + usuarioID +
               "  and ss.StatusID not in (4)" +
               "  and ss.Ultimo = 1 " +
               "  and l.ContaID = " + ContaId + 
               "  and s.Data >= '" + App.DateTimeZion.ToString("yyyy-MM-dd") + " 00:00:00' " +
               "  and s.Data <= '" + App.DateTimeZion.ToString("yyyy-MM-dd") + " 23:59:59' ";

            return _context.Database.SqlQuery<Entities.Saque>(sql).ToList();
        }

        public List<Entities.Saque> BuscaSaquePorConta(int usuarioID, int contaID)
        {
            return (from s in  _context.Set<Entities.Saque>()
                    join ss in _context.Set<Entities.SaqueStatus>() on s.ID equals ss.SaqueID
                    join l in _context.Set<Entities.Lancamento>() on s.ID equals l.ReferenciaID
                    where s.UsuarioID == usuarioID
                    && l.ContaID == contaID
                    && l.TipoID == 7
                    && ss.StatusID != 4
                    && ss.Ultimo.Value
                    select s).ToList();
        }

        public IEnumerable<SolicitacaoSaqueModel> BuscarSaques(string de, string ate, string login, int status, int? quantidade = null)
        {
            string strDe = Core.Helpers.ProcedureHelper.ConverterDataInicio(de);
            string strAte = Core.Helpers.ProcedureHelper.ConverterDataFim(ate);
            login = string.IsNullOrEmpty(login) ? "null" : $"'{login}'";
            string strQuantidade = quantidade.HasValue ? quantidade.Value.ToString() : "null";

            var procedure = string.Format("EXEC spOC_FI_BuscarSolicitacoesSaque '{0}', '{1}', {2}, {3}, {4}", strDe, strAte, login, status, strQuantidade);
            
            return this._context.Database.SqlQuery<SolicitacaoSaqueModel>(procedure);
        }
    }
}
