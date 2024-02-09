using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioClassificacaoRepository : PersistentRepository<Entities.UsuarioClassificacao>
    {

        public UsuarioClassificacaoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
