using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class PontosRecompraRepository : PersistentRepository<Entities.PontoBinario>
    {
        private DbContext _context;
        public PontosRecompraRepository(DbContext context) : base(context)
        {
            _context = context;
        }
    }
}
