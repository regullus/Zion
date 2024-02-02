using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class UsuarioGanhoRepository : PersistentRepository<Entities.UsuarioGanho>
    {
        DbContext _context;

        public UsuarioGanhoRepository(DbContext context)
           : base(context)
        {
            _context = context;
        }

        public Entities.UsuarioGanho GetByUsuario(int usuarioID)
        {
            return base.GetByExpression(u => u.UsuarioID == usuarioID).FirstOrDefault();
        }

        public Entities.UsuarioGanho GetUltimo(int usuarioID)
        {
            return base.GetByExpression(u => u.UsuarioID == usuarioID).OrderBy(u => u.ID).FirstOrDefault();
        }

        public Entities.UsuarioGanho GetAtual(int usuarioID)
        {
            return base.GetByExpression(u => u.UsuarioID == usuarioID && u.Atual == true).OrderBy(u => u.ID).FirstOrDefault();
        }

    }
}
