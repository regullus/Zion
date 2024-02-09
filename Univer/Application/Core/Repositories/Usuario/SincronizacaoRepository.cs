using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class SincronizacaoRepository : PersistentRepository<Entities.Sincronizacao>
    {

        public SincronizacaoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
