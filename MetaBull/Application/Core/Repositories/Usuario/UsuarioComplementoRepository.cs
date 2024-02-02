using Core.Helpers;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioComplementoRepository : PersistentRepository<Entities.Complemento>
    {
        DbContext _context;

        public UsuarioComplementoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public void SetLideranca(Core.Entities.Usuario usuario, bool isLideranca)
        {
            var complemento = usuario.Complemento;

            if (complemento == null)
            {
                complemento = new Core.Entities.Complemento()
                {
                    ID = usuario.ID,
                    Login = usuario.Login
                };
            }

            complemento.dtInicioLideranca = App.DateTimeZion;
            complemento.IsLideranca = isLideranca;

            if(usuario.Complemento == null)
            {
                _context.Entry(complemento).State = EntityState.Added;
            }
            else
            {
                _context.Entry(complemento).State = EntityState.Modified;
            }

            _context.SaveChanges();
        }



    }

}
