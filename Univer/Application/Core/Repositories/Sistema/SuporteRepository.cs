using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Repositories.Sistema
{
    public class SuporteRepository : PersistentRepository<Entities.Suporte>
    {
        private DbContext _context { get; set; }
        public SuporteRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public Task<List<Core.Entities.Suporte>> MeusSuportes(int idUsuario)
        {
            return base.GetByExpression(c => c.UsuarioID == idUsuario && c.SuporteMensagem.Any()).ToListAsync();
        }

        public void CriarChamado(int UsuarioId, Guid guid, string assunto, string texto)
        {
            var procedure = string.Format("EXEC sp_I_SuporteNovoChamado '{0}', '{1}', N'{2}', N'{3}'", UsuarioId, guid, assunto, texto);

            this._context.Database.SqlQuery<decimal>(procedure).FirstOrDefault();
        }

        public void NovaInteracao(int SuporteId, Guid guid, string texto)
        {
            var procedure = string.Format("EXEC sp_I_SuporteNovaInteracao '{0}', '{1}', N'{2}'", SuporteId, guid, texto);

            this._context.Database.SqlQuery<decimal>(procedure).FirstOrDefault();
        }

        public int Resposta(int SuporteId, int AdministradorId, Guid guid, string texto)
        {
            var procedure = string.Format("EXEC sp_I_SuporteResposta {0}, {1}, '{2}', N'{3}'", SuporteId, AdministradorId, guid, texto);

            var id = (int)this._context.Database.SqlQuery<decimal>(procedure).FirstOrDefault();

            return id;
        }
    }
}
