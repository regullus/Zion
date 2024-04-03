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

        public IEnumerable<TabuleiroNivelModel> ObtemNivelTabuleiro(int idUsuario, int? statusID)
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

        public TabuleiroModel ObtemTabuleiro(int id, int usuarioID)
        {
            string sql = "Exec spC_Tabuleiro @id=" + id + ", @UsuarioID = " + usuarioID;

            var retorno = _context.Database.SqlQuery<TabuleiroModel>(sql).FirstOrDefault();;

            return retorno;
        }

        public IEnumerable<TabuleiroInclusao> IncluiTabuleiro(int idUsuario, int idPai, int idBoard)
        {
            //idUsuario usuario a ser incluido no tabuleiro
            //idPai patrocinador do usuario acima
            //idBoard board a ser inserido o usuario
            //chamada deve ser principal para incluir novo usuario
            string sql = "Exec spG_Tabuleiro @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idPai + ",@BoardID=" + idBoard + ",@Chamada='Principal'";

            var retorno = _context.Database.SqlQuery<TabuleiroInclusao>(sql).ToList();

            return retorno;
        }

        public string InformarPagamento(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInformarPagto @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarPagtoSistema(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInformarPagtoSistema @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarRecebimento(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInformarRecebimento @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }


        public TabuleiroInfoUsuarioModel ObtemInfoUsuario(int idTarget, int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInfoUsuario @idTarget=" + idTarget + ", @idUsuario=" + idUsuario + ", @idTabuleiro=" + idTabuleiro;
            var retorno = _context.Database.SqlQuery<TabuleiroInfoUsuarioModel>(sql).FirstOrDefault(); 

            return retorno;
        }

        public TabuleiroUsuarioModel ObtemTabuleiroUsuario(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroUsuario @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;
            var retorno = _context.Database.SqlQuery<TabuleiroUsuarioModel>(sql).FirstOrDefault();

            return retorno;
        }

        public TabuleiroBoardModel ObtemTabuleiroBoard(int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroBoard @TabuleiroID=" + idTabuleiro;
            var retorno = _context.Database.SqlQuery<TabuleiroBoardModel>(sql).FirstOrDefault();

            return retorno;
        }

    }
}
