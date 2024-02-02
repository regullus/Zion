using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class TraducaoSecaoRepository : PersistentRepository<Entities.MoedaCotacaoTipo>
    {

        public TraducaoSecaoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
