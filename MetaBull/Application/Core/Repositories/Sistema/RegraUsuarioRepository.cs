using DomainExtension.Repositories;
using System.Data.Entity;

namespace Core.Repositories.Sistema
{
    public class RegraUsuarioRepository : PersistentRepository<Entities.RegraUsuario>
    {
        public RegraUsuarioRepository(DbContext context)
            : base(context)
        {
        }
    }
}