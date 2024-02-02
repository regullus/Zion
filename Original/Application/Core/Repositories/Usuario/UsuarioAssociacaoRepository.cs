using DomainExtension.Repositories;
//using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioAssociacaoRepository : PersistentRepository<Entities.UsuarioAssociacao>
    {
        private DbContext _context;

        public UsuarioAssociacaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public List<Models.Geral.Data> ObtemListaTotalUsuarioAssociacao()
        {
            string sql = "EXEC spOC_US_ObtemTotalUsuarioAssoc ";

            return _context.Database.SqlQuery<Models.Geral.Data>(sql).ToList();
        }

        public List<Models.Geral.Data> ObtemListaTotalUsuarioAssociadosDia(string data)
        {
            string sql = "EXEC spOC_US_ObtemTotalUsuarioAssociadosDia '" + data + "' ";

            return _context.Database.SqlQuery<Models.Geral.Data>(sql).ToList();
        }

    }
}
