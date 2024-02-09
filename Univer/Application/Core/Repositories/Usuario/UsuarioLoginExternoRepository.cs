using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioLoginExternoRepository : PersistentRepository<Entities.LoginExterno>
    {
        DbContext _context;

        public UsuarioLoginExternoRepository(DbContext context)
           : base(context)
        {
            _context = context;
        }

        public LoginExterno Obtem(Guid guid)
        {
            return this.GetByExpression(e => e.Guid == guid).FirstOrDefault();
        }

        public void Salvar(LoginExterno login)
        {
            this.Save(login);
        }


        public void MarcaUsado(LoginExterno login)
        {
            login.Usado = true;
            this.Save(login);
        }

    }
}
