using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Integracao
{
    public class MotivacardRepository : PersistentRepository<Entities.Motivacard>
    {

        public MotivacardRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Motivacard> GetByUsuario(int idUsuario)
        {
            return base.GetByExpression(i => i.UsuarioID == idUsuario).ToList();
        }

    }
}
