using DomainExtension.Repositories;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Core.Repositories.Usuario
{
    public class LogAPIRepository : PersistentRepository<Entities.LogAPI>
    {
        DbContext _context;

        public LogAPIRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public bool VerificaUsuarioLogAPI(int? UsuarioID, 
            int? ExternoID, 
            string ActionName, 
            string ControllerName, 
            string Mensagem, 
            string Objeto)
        {
            return _context.Database.SqlQuery<int>("EXEC sp_VerificaUsuarioLogAPI @UsuarioID, @ExternoID, @ActionName, @ControllerName, @Mensagem, @Objeto",
                    UsuarioID.HasValue ?
                        new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = UsuarioID }
                        : new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = DBNull.Value },
                    ExternoID.HasValue ?
                        new SqlParameter("@ExternoID", SqlDbType.Int) { Value = ExternoID }
                        : new SqlParameter("@ExternoID", SqlDbType.Int) { Value = DBNull.Value },
                    new SqlParameter("@ActionName", SqlDbType.NVarChar) { Value = ActionName },
                    new SqlParameter("@ControllerName", SqlDbType.NVarChar) { Value = ControllerName },
                    new SqlParameter("@Mensagem", SqlDbType.NVarChar) { Value = Mensagem },
                    new SqlParameter("@Objeto", SqlDbType.NVarChar) { Value = Objeto }
                    ).FirstOrDefault() == 0;
        }
    }
}