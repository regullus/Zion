using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Core.Repositories.Financeiro
{
    public class ContaRepository : CachedRepository<Entities.Conta>
    {
        DbContext _context;

        public ContaRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public IEnumerable<Entities.Conta> GetByOperacao(Entities.ContaOperacao.Operacoes operacao)
        {
            return cachedRepository.Where(c => c.ContaOperacoes.Any(o => o.Operacao == operacao));
        }

        public List<Entities.Conta> GetByAtiva()
        {
            string sql = "" +
               " select * " +
               " From Financeiro.Conta (nolock) " +
               " where Ativo = 1";

            return _context.Database.SqlQuery<Entities.Conta>(sql).ToList();
        }

        public bool CallSpRenovacao(int idUsuario)
        {
            bool ret = true;
            //Chama sp renovacao : spOC_US_Renovacao 
            try
            {
                _context.Database.ExecuteSqlCommand("EXEC spOC_US_Renovacao @UsuarioId=" + idUsuario);
            }
            catch (Exception ex)
            {
                ret = false;
                string strErro = ex.Message;
                cpUtilities.LoggerHelper.WriteFile("ERROR spOC_US_Renovacao : " + strErro, "CoreRepositoriesFinanceiroContaRepository");
            }
            return ret;
        }
        public bool CallSpRenovacaoAutomatica(int idUsuario, bool renova)
        {
            bool ret = true;
            //Chama sp renovacao : spOC_US_Renovacao 
            try
            {
                _context.Database.ExecuteSqlCommand("EXEC spOC_US_RenovacaoAutomatica @UsuarioId=" + idUsuario + ", @renova=" + renova);
            }
            catch (Exception ex)
            {
                ret = false;
                string strErro = ex.Message;
                cpUtilities.LoggerHelper.WriteFile("ERROR spOC_US_RenovacaoAutomatica : " + strErro, "CoreRepositoriesFinanceiroContaRepository");
            }
            return ret;
        }


    }
}
