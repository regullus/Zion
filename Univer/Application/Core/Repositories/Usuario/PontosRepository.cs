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
    public class PontosRepository : PersistentRepository<Entities.Pontos>
    {
        public PontosRepository(DbContext context)
            : base(context)
        {
        }
    }
}
