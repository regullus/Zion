using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class AvisoRepository : PersistentRepository<Entities.Aviso>
    {
        DbContext _context;

        public AvisoRepository(DbContext context)
         : base(context)
        {
            _context = context;
        }

        public List<Models.StoredProcedures.spOC_US_ObtemAvisoNaoLidos> GetNaoLidosByUsuario(int usuarioID)
        {
            try
            {
                var sql = string.Format("Exec spC_ObtemAvisoNaoLidos {0}", usuarioID);
                return _context.Database.SqlQuery<Models.StoredProcedures.spOC_US_ObtemAvisoNaoLidos>(sql).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Models.StoredProcedures.spOC_US_ObtemAvisoNaoLidos> GetLidosByUsuario(int usuarioID, int avisoID)
        {
            try
            {
                var sql = string.Format("Exec spC_ObtemAvisoLido @UsuarioID={0}, @AvisoID={1}", usuarioID, avisoID);
                return _context.Database.SqlQuery<Models.StoredProcedures.spOC_US_ObtemAvisoNaoLidos>(sql).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Models.StoredProcedures.spOC_US_ObtemAvisos> GetByUsuario(int usuarioID)
        {
            try
            {
                var sql = string.Format("Exec spC_ObtemAvisos {0}", usuarioID);
                return _context.Database.SqlQuery<Models.StoredProcedures.spOC_US_ObtemAvisos>(sql).ToList();
            }
            catch (Exception ex)
            {
                var teste = ex.Message;
                return null;
            }
        }
    }
}
