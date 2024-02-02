using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class RedeMigracaoRepository : CachedRepository<Entities.RedeMigracao>
    {
        DbContext _context;

        public RedeMigracaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public List<int> GetListaPatrocinadores()
        {
            //var  redeMigracao = cachedRepository.GroupBy(x => x.PatrocinadorID);

            //List<int> list = redeMigracao.Select(x => x.Key).ToList();
            //list.Sort();

            string sql = " Select R.PatrocinadorID " +
                         " From  Rede.RedeMigracao R(nolock), Usuario.Usuario U(nolock) " +
                         " Where R.PatrocinadorID = U.ID " +
                         " Group by R.PatrocinadorID, U.DataCriacao " +
                         " Order by U.DataCriacao ";

            List<int>  list = _context.Database.SqlQuery<int>(sql).ToList();

            return list; 
        }

        public List<Entities.RedeMigracao>  GetByPatricinador(int patrocinadorID)
        {
            return cachedRepository.Where(x => x.PatrocinadorID == patrocinadorID).ToList();
        }

    }
}
