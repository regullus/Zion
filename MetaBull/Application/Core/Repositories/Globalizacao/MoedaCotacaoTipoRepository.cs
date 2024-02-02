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
    public class MoedaCotacaoTipoRepository : PersistentRepository<Entities.MoedaCotacaoTipo>
    {

        public MoedaCotacaoTipoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
