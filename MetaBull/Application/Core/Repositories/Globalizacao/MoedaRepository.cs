using Core.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class MoedaRepository : CachedRepository<Entities.Moeda>
    {

        public MoedaRepository(DbContext context)
            : base(context)
        {
        }

        public Moeda GetByID(int ID)
        {
            return this.GetByExpression(m => m.ID == ID).FirstOrDefault();
        }

        public Moeda GetBySigla(string sigla)
        {
            return this.GetByExpression(m => m.Sigla == sigla).FirstOrDefault();
        }

    }
}
