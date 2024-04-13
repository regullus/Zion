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

        public string IncluiTabuleiro(int idUsuario, int idPai, int idBoard, string Chamada)
        {
            //idUsuario usuario a ser incluido no tabuleiro
            //idPai patrocinador do usuario acima
            //idBoard board a ser inserido o usuario
            //chamada deve ser principal para incluir novo usuario
            if(String.IsNullOrEmpty(Chamada))
            {
                Chamada = "Principal";
            }

            string sql = "Exec spG_Tabuleiro @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idPai + ",@BoardID=" + idBoard + ",@Chamada='" + Chamada + "'";

            TabuleiroInclusao retorno = _context.Database.SqlQuery<TabuleiroInclusao>(sql).FirstOrDefault();

            string ret = "OK";
            
            if (retorno !=  null )
            {
                if (retorno.Retorno != "OK")
                {
                    switch (retorno.Historico.Substring(0, 2))
                    {
                        case "01":
                            ret = "USUARIO_JA_POSICIONADO";
                            break;
                        case "02":
                            ret = "CONVIDADO_INCLUIR";
                            break;
                        case "03":
                            ret = "ALVO_USUARIO";
                            break;
                        case "04":
                            ret = "TABULEIRO_SEM_POSICAO";
                            break;
                        case "05":
                            ret = "MASTER_NAO_EXISTE";
                            break;
                        case "06":
                            ret = "USUARIO_JA_POSICIONADO";
                            break;
                        case "07":
                            //Retorno para tipo de chamda = "COMPLETA"
                            //07 indica que o tabuleiro ainda não esta completo
                            ret = "OK";
                            break;
                        case "08":
                            //Retorno para tipo de chamda = "COMPLETA"
                            //08 indica que o Tabuleiro esta completo
                            ret = "COMPLETO";
                            break;
                        default:
                            ret = "OK";
                            break;
                    }
                }
            } else
            {
                ret = "SEM_DADOS";
            }

            return ret;
        }

        public string InformarPagamento(int idUsuario, int idTabuleiro, int idUsuarioPag)
        {
            string sql = "Exec spC_TabuleiroInformarPagto @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro + ",@UsuarioIDPag=" + idUsuarioPag;
            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarPagtoSistema(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInformarPagtoSistema @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarRecebimento(int idUsuario, int idUsuarioPai, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInformarRecebimento @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idUsuarioPai + ",@TabuleiroID =" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public TabuleiroInfoUsuarioModel ObtemInfoUsuario(int idTarget, int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroInfoUsuario @idTarget=" + idTarget + ", @idUsuario=" + idUsuario + ", @idTabuleiro=" + idTabuleiro;
            var retorno = _context.Database.SqlQuery<TabuleiroInfoUsuarioModel>(sql).FirstOrDefault(); 

            return retorno;
        }

        public TabuleiroInfoUsuarioModel ObtemInfoSystem()
        {
            string sql = "Exec spC_TabuleiroInfoSystem";
            var retorno = _context.Database.SqlQuery<TabuleiroInfoUsuarioModel>(sql).FirstOrDefault();

            return retorno;
        }

        public TabuleiroInfoUsuarioModel ObtemInfoSysPag()
        {
            string sql = "Exec spC_TabuleiroInfoSysPag";
            var retorno = _context.Database.SqlQuery<TabuleiroInfoUsuarioModel>(sql).FirstOrDefault();

            return retorno;
        }

        public bool MasterRuleOK(int idUsuario, int idTabuleiro)
        {
            bool retorno = true;
            string sql = "Exec spC_TabuleiroMasterRule @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;
            string retornoSP = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
            if (retornoSP != "OK")
            {
                retorno = false;
            }
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

        public string TabuleiroSair(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spG_TabuleiroSair @UsuarioID=" + idUsuario + ", @TabuleiroID=" + idTabuleiro;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

    }
}
