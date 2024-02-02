using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class DepartamentoRepository : PersistentRepository<Entities.Departamento>
    {

        public DepartamentoRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Departamento> GetByPaiID(int departamentoID)
        {
            return base.GetByExpression(d => d.DepartamentoID == departamentoID);
        }

    }
}
