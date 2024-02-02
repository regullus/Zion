using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class LogAcessoRepository : PersistentRepository<Entities.LogAcesso>
    {

        public LogAcessoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
