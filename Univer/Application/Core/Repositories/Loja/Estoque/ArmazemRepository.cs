using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class ArmazemRepository : PersistentRepository<Entities.Armazem>
    {

        private DbContext _context;

        public ArmazemRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public List<Entities.Estado> GetDistinctEstados()
        {
            string sql = "SELECT  DISTINCT Globalizacao.Estado.* from Globalizacao.Estado ";
            sql += " INNER JOIN Loja.Armazem on Globalizacao.Estado.ID = Loja.Armazem.EstadoID WHERE Loja.Armazem.Publicado = 1  ORDER BY Sigla";

            return _context.Database.SqlQuery<Core.Entities.Estado>(sql).ToList();
        }

        public List<Entities.Cidade> GetDistinctCidades(int idEstado)
        {
            string sql = "select DISTINCT Globalizacao.Cidade.* from Globalizacao.Cidade ";
            sql += " INNER JOIN Loja.Armazem on Globalizacao.Cidade.ID = Loja.Armazem.CidadeID ";
            sql += " WHERE Globalizacao.Cidade.EstadoID = " + idEstado + " AND Loja.Armazem.Publicado = 1 ORDER BY Nome";

            return _context.Database.SqlQuery<Core.Entities.Cidade>(sql).ToList();
        }

       public IEnumerable<Entities.Armazem> GetArmazemByCidade(int idCidade)
        {
            return this.GetByExpression(a => a.CidadeID == idCidade && a.Publicado == true).ToList();
        }


    }
}
