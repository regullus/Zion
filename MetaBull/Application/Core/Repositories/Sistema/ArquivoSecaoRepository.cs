using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
    public class ArquivoSecaoRepository : CachedRepository<Entities.ArquivoSecao>
    {
        DbContext _context;

        public ArquivoSecaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public Entities.ArquivoSecao GetById(int id)
        {
            return base.GetByExpression(s => s.ID == id).FirstOrDefault();
        }
    }
}

