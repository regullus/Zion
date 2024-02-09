using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class BoletoRepository : PersistentRepository<Entities.Boleto>
    {

        public BoletoRepository(DbContext context)
            : base(context)
        {
        }

        public int GetMaxID()
        {
            return this.GetAll().Any() ? this.GetAll().Max(b => b.ID) : 0;
        }

        public Entities.Boleto GetByNossoNumerio(string nossoNumero)
        {
            return this.GetByExpression(s => s.NossoNumero == nossoNumero).FirstOrDefault();
        }

        public Entities.Boleto GetByNumeroDocumento(int numeroDocumento)
        {
            return this.GetByExpression(s => s.NumeroDocumento == numeroDocumento).FirstOrDefault();
        }
    }
}
