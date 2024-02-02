using Core.Entities;
using Core.Helpers;
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
    public class CicloRepository : PersistentRepository<Entities.Ciclo>
    {
        private DbContext _context;
        public CicloRepository(DbContext context) : base(context)
        {
            _context = context;
        }

        public Entities.Ciclo GetAtivo()
        {
            return this.GetByExpression(a => a.Ativo).OrderByDescending(a => a.DataFinal).FirstOrDefault();
        }

        public bool GeraPontosValorVariavel(int idUsuario, int idPedido)
        {
            bool ret = true;
            try
            {
                _context.Database.ExecuteSqlCommand("EXEC spDG_RE_GeraPontos @PedidoId=" + idPedido);
            }
            catch (Exception ex)
            {
                ret = false;
                string strErro = ex.Message;
                cpUtilities.LoggerHelper.WriteFile("ERROR spDG_RE_GeraPontos : " + strErro, "CoreRepositoriesRede");
            }
            return ret;
        }

        public bool RedeUplineCiclo(int idUsuario)
        {
            bool ret = true;
            try
            {
                _context.Database.ExecuteSqlCommand("EXEC spOC_US_RedeUpline_Ciclo @CicloID, @UsuarioID",
                new SqlParameter("@CicloID", 5),
                new SqlParameter("@UsuarioID", idUsuario)
                );
            }
            catch (Exception ex)
            {
                ret = false;
                string strErro = ex.Message;
                cpUtilities.LoggerHelper.WriteFile("ERROR RedeUplineCiclo : " + strErro, "CoreRepositoriesRede");
            }
            return ret;
        }

        public bool GeraBonusVenda(int idUsuario, int idPedido)
        {
            bool ret = true;
            try
            {
                _context.Database.ExecuteSqlCommand("EXEC spDG_RE_GeraBonusVenda_v2 @UsuarioID, @PedidoID",
                new SqlParameter("@UsuarioID", idUsuario),
                new SqlParameter("@PedidoID", idPedido)
                );
            }
            catch (Exception ex)
            {
                ret = false;
                string strErro = ex.Message;
                cpUtilities.LoggerHelper.WriteFile("ERROR GeraBonusVenda : " + strErro, "CoreRepositoriesRede");
            }
            return ret;
        }
    }
}
