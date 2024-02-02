using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class ProdutoTipoRepository : PersistentRepository<Entities.ProdutoTipo>
    {

        DbContext _context;

        public ProdutoTipoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }
    }
}