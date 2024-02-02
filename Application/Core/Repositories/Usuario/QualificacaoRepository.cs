using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using System.Data.SqlClient;

namespace Core.Repositories.Usuario
{
    public class QualificacaoRepository : PersistentRepository<Entities.Qualificacao>
    {
        private DbContext _context;
        public QualificacaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }
        /// <summary>
        /// com base em um usuarioid, atualiza a qualificacao de seu patrocinador
        /// </summary>
        /// <param name="idUsuarioReferencia"></param>
        public void AtualizaQualificacao(int idUsuarioReferencia)
        {
            _context.Database.ExecuteSqlCommand("EXEC sp_JB_AtualizaQualificacaoBinario @UsuarioId",
                new SqlParameter("@UsuarioId", idUsuarioReferencia)
                );
        }

        /// <summary>
        /// com base em um usuarioid, atualiza a qualificacao para bonus PLUS
        /// </summary>
        /// <param name="idUsuarioReferencia"></param>
        public void AtualizaQualificacaoPlus(int idUsuarioReferencia, int nivelAssociacao)
        {
            _context.Database.ExecuteSqlCommand("EXEC sp_JB_AtualizaQualificacaoPlus @UsuarioId, @nivelAssociacao",
                new SqlParameter("@UsuarioId", idUsuarioReferencia),
                new SqlParameter("@nivelAssociacao", nivelAssociacao)
                );
        }
    }
}
