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
using System.Data.Entity.Core.Objects;
using Core.Models;

namespace Core.Repositories.Rede
{
    public class TabuleiroRepository : PersistentRepository<Entities.Posicao>
    {

        private DbContext _context;

        public TabuleiroRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }


        public IEnumerable<TabuleiroNivelModel> ObtemTabuleiro(int idUsuario, int? statusID)
        {
            string sql = "";

            if (statusID.HasValue)
            {
                sql = "Exec spC_TabuleiroNivel @UsuarioID=" + idUsuario + ", @StatusID=" + statusID;
            }
            else
            {
                sql = "Exec spC_TabuleiroNivel @UsuarioID=" + idUsuario + ", @StatusID=null";
            }
            
            var retorno = _context.Database.SqlQuery<TabuleiroNivelModel>(sql).ToList();

            return retorno;
        }


    }
}
