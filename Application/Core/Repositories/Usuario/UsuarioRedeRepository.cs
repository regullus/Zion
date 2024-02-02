using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Repositories.Usuario
{
    public class UsuarioRedeRepository : PersistentRepository<Entities.Rede>
    {
        public UsuarioRedeRepository(DbContext context)
            : base(context)
        {
        }
    }
}
