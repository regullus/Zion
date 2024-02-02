using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
   public class LancamentoRepository : PersistentRepository<Entities.Lancamento>
   {

      public LancamentoRepository(DbContext context)
          : base(context)
      {
      }

      public IEnumerable<Entities.Lancamento> GetByUsuario(int usuarioID)
      {
         return base.GetByExpression(l => l.UsuarioID == usuarioID);
      }

      public IEnumerable<Entities.Lancamento> GetByUsuarioConta(int usuarioID, int contaID, Entities.Lancamento.Tipos? tipo = null, int? categoriaID = null, DateTime? dataInicial = null, DateTime? dataFinal = null)
      {
         var lancamentos = base.GetByExpression(l => l.UsuarioID == usuarioID && l.ContaID == contaID);

         if (tipo.HasValue)
         {
            lancamentos = lancamentos.Where(l => l.TipoID == (int)tipo.Value);
         }

         if (categoriaID.HasValue)
         {
            lancamentos = lancamentos.Where(l => l.CategoriaID == categoriaID.Value);
         }

         if (dataInicial.HasValue)
         {
            lancamentos = lancamentos.Where(l => l.DataLancamento >= dataInicial.Value);
         }

         if (dataFinal.HasValue)
         {
            lancamentos = lancamentos.Where(l => l.DataLancamento <= dataFinal.Value);
         }

         return lancamentos;
      }
      
      public IEnumerable<Entities.Lancamento> GetByDescricao(int usuarioID, string descricao)
      {
         var lancamentos = base.GetByExpression(l => l.UsuarioID == usuarioID && l.Descricao.Contains(descricao));
         return lancamentos;
      }
      
   }
}
