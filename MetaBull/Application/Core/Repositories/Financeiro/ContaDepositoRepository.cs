using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainExtension.Repositories;

namespace Core.Repositories.Financeiro
{
   public class ContaDepositoRepository : PersistentRepository<Entities.ContaDeposito>
   {

      public ContaDepositoRepository(DbContext context)
          : base(context)
      {
      }

      public Entities.ContaDeposito GetAtual()
      {
         var tipoConta = 1;
         return this.GetByExpression(c => c.IDTipoConta == tipoConta).OrderByDescending(o => o.DataCriacao).FirstOrDefault();
      }

      public Entities.ContaDeposito GetAtual(int tipoConta)
      {
         return this.GetByExpression(c => c.IDTipoConta == tipoConta).OrderByDescending(o => o.DataCriacao).FirstOrDefault();
      }

      public Entities.ContaDeposito GetByData(DateTime dataReferencia)
      {
         return this.GetByExpression(c => c.DataCriacao <= dataReferencia).OrderByDescending(o => o.DataCriacao).FirstOrDefault();
      }

      public IEnumerable<Entities.ContaDeposito> GetByUsuario(int usuarioID)
      {
         return base.GetByExpression(c => c.IDUsuario == usuarioID);
      }

   }
}
