using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
   public class AdministradorRepository : PersistentRepository<Entities.Administrador>
   {

      public AdministradorRepository(DbContext context)
          : base(context)
      {
      }

      public Entities.Administrador GetByEmailSenha(string email, string senha)
      {
         return base.GetByExpression(a => a.Email == email && a.Senha == senha).FirstOrDefault();
      }

      public Entities.Administrador GetByEmail(string email)
      {
         email = email.ToLower();
         return base.GetByExpression(a => a.Email.ToLower() == email).FirstOrDefault();
      }
      
      public Entities.Administrador GetByAutenticacao(int intIdAutenticacao)
      {
         return base.GetByExpression(a => a.IdAutenticacao == intIdAutenticacao).FirstOrDefault();
      }

   }
}
