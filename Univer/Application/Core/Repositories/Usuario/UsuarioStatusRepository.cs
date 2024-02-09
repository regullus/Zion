using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioStatusRepository : PersistentRepository<Entities.UsuarioStatus>
    {

      DbContext _context;

      public UsuarioStatusRepository(DbContext context)
            : base(context)
        {
         _context = context;
        }

   }
}
