using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class CupomRepository : PersistentRepository<Entities.Cupom>
    {

        public CupomRepository(DbContext context)
            : base(context)
        {
        }

        public Entities.Cupom GetByCodigo(string codigo)
        {
            return base.GetByExpression(c => c.Codigo == codigo).FirstOrDefault();
        }

    }
}
