using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class EstoqueMovimentoRepository : PersistentRepository<Entities.EstoqueMovimento>
    {

        private DbContext _context;

        public EstoqueMovimentoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

    }
}
