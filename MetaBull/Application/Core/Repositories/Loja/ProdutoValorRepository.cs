using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class ProdutoValorRepository : PersistentRepository<Entities.ProdutoValor>
    {

        public ProdutoValorRepository(DbContext context)
            : base(context)
        {
        }

    }
}
