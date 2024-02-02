using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Repositories.Loja
{
    public class EstoqueSaldoRepository : PersistentRepository<Entities.EstoqueSaldo>
    {

        private DbContext _context;

        public EstoqueSaldoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public IEnumerable<EstoqueSaldo> GetPosicaoAtual(int idArmazem)
        {
            return this.GetByExpression(e => e.ArmazemID == idArmazem).OrderBy(o => o.Quantidade).ToList();
        }

    }
}
