using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class MovimentoContaRepository : PersistentRepository<Entities.MovimentoConta>
    {

        public MovimentoContaRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.MovimentoConta> GetByUsuario(int idUsuario)
        {
            return base.GetByExpression(i => i.UsuarioID == idUsuario).ToList();
        }

        public void DeleteByDate(DateTime date)
        {
            var movimentos = base.GetByExpression(i => i.Data.Year == date.Year && i.Data.Month == date.Month && i.Data.Day == date.Day).ToList();

            for (int i = movimentos.Count; i > 0; i--)
            {
                base.Delete(movimentos[i].ID);
            }
        }

    }
}
