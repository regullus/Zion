using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class ContaOperacaoRepository : PersistentRepository<Entities.ContaOperacao>
    {

        public ContaOperacaoRepository(DbContext context)
            : base(context)
        {
        }

        public override void Delete(int id)
        {
            base.Delete(id);
            ContaRepository.ClearCache();
        }

        public override void Delete(Entities.ContaOperacao entity)
        {
            base.Delete(entity);
            ContaRepository.ClearCache();
        }

        public override void Save(Entities.ContaOperacao entity)
        {
            base.Save(entity);
            ContaRepository.ClearCache();
        }

    }
}
