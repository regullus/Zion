using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
    public class IntegracaoRepository : PersistentRepository<Entities.Integracao>
    {

        public IntegracaoRepository(DbContext context)
            : base(context)
        {
        }

        public bool GetByApiCode(string api_code)
        {
            return base.GetByExpression(c => c.ApiCode == api_code).Any();
        }
    }
}
