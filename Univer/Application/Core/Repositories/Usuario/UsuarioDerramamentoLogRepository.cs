using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioDerramamentoLogRepository : PersistentRepository<Entities.UsuarioDerramamentoLog>
    {

      DbContext _context;

      public UsuarioDerramamentoLogRepository(DbContext context)
            : base(context)
        {
         _context = context;
        }

   }
}
