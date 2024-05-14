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
using System.Collections;

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

        public IEnumerable<TabuleiroUsuarioModel> ObtemTabuleirosUsuario(int? idUsuario)
        {
            string sql = "";

            if (idUsuario == null)
            {
                sql = "Exec spC_TabuleiroUsuario @UsuarioID=null";
            }
            else
            {
                sql = "Exec spC_TabuleiroUsuario @UsuarioID=" + idUsuario;
            }

            var retorno = _context.Database.SqlQuery<TabuleiroUsuarioModel>(sql).ToList();

            return retorno;
        }

        public TabuleiroUsuarioModel ObtemTabuleiroUsuario(int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroUsuarioID @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;
            var retorno = _context.Database.SqlQuery<TabuleiroUsuarioModel>(sql).FirstOrDefault();
            return retorno;
        }

        public TabuleiroModel ObtemTabuleiro(int id, int usuarioID)
        {
            string sql = "Exec spC_Tabuleiro @id=" + id + ", @UsuarioID = " + usuarioID;

            TabuleiroModel retorno = _context.Database.SqlQuery<TabuleiroModel>(sql).FirstOrDefault(); ;
            if (retorno != null)
            {
                retorno.ApelidoMaster = retorno.ApelidoMaster.ToLower();

                retorno.ApelidoCoordinatorDir = retorno.ApelidoCoordinatorDir.ToLower();
                retorno.ApelidoCoordinatorEsq = retorno.ApelidoCoordinatorEsq.ToLower();

                retorno.ApelidoIndicatorDirSup = retorno.ApelidoIndicatorDirSup.ToLower();
                retorno.ApelidoIndicatorDirInf = retorno.ApelidoIndicatorDirInf.ToLower();
                retorno.ApelidoIndicatorEsqSup = retorno.ApelidoIndicatorEsqSup.ToLower();
                retorno.ApelidoIndicatorEsqInf = retorno.ApelidoIndicatorEsqInf.ToLower();

                retorno.ApelidoDonatorDirSup1 = retorno.ApelidoDonatorDirSup1.ToLower();
                retorno.ApelidoDonatorDirInf1 = retorno.ApelidoDonatorDirInf1.ToLower();
                retorno.ApelidoDonatorDirSup2 = retorno.ApelidoDonatorDirSup2.ToLower();
                retorno.ApelidoDonatorDirInf2 = retorno.ApelidoDonatorDirInf2.ToLower();

                retorno.ApelidoDonatorEsqSup1 = retorno.ApelidoDonatorEsqSup1.ToLower();
                retorno.ApelidoDonatorEsqInf1 = retorno.ApelidoDonatorEsqInf1.ToLower();
                retorno.ApelidoDonatorEsqSup2 = retorno.ApelidoDonatorEsqSup2.ToLower();
                retorno.ApelidoDonatorEsqInf2 = retorno.ApelidoDonatorEsqInf2.ToLower();

            }
            return retorno;
        }

        public string IncluiTabuleiro(int idUsuario, int idPai, int idBoard, string Chamada)
        {
            //idUsuario usuario a ser incluido no tabuleiro
            //idPai patrocinador do usuario acima
            //idBoard board a ser inserido o usuario
            //chamada deve ser principal para incluir novo usuario
            if (String.IsNullOrEmpty(Chamada))
            {
                Chamada = "Principal";
            }

            string sql = "";

            if (Chamada == "Convite") //Força a entradanum tabuleiro mais antigo
            {
                sql = "Exec spG_Tabuleiro @UsuarioID=" + idUsuario + ",@UsuarioPaiID=null,@BoardID=" + idBoard + ",@Chamada='" + Chamada + "'";
            }
            else
            {
                sql = "Exec spG_Tabuleiro @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idPai + ",@BoardID=" + idBoard + ",@Chamada='" + Chamada + "'";
            }

            TabuleiroInclusao retorno = _context.Database.SqlQuery<TabuleiroInclusao>(sql).FirstOrDefault();

            string ret = "OK";

            if (retorno != null && retorno.Retorno != null)
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
                        case "09":
                            //Recursiva
                            ret = "OK";
                            break;
                        default:
                            ret = "OK";
                            break;
                    }
                }
                else
                {
                    ret = retorno.Retorno;
                }
            }
            else
            {
                ret = "SEM_DADOS";
            }

            return ret;
        }

        public string IncluiTabuleiroNew(int idUsuario, int idPai)
        {
            //idUsuario usuario a ser incluido no tabuleiro
            //idPai patrocinador do usuario acima
            //idBoard board a ser inserido o usuario
            //chamada deve ser principal para incluir novo usuario
            string sql = "Exec spI_TabuleiroUsuario @UsuarioID=" + idUsuario + ", @MasterID=" + idPai;
            string ret = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            if (ret == "OK")
            {
                sql = "Exec spG_Tabuleiro @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idPai + ",@BoardID=1,@Chamada='ConviteNew'";

                TabuleiroInclusao retorno = _context.Database.SqlQuery<TabuleiroInclusao>(sql).FirstOrDefault();

                if (retorno != null && retorno.Retorno != null)
                {
                    ret = "OK";
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
                            case "09":
                                //Recursiva
                                ret = "OK";
                                break;
                            default:
                                ret = "OK";
                                break;
                        }
                    }
                }
                else
                {
                    if (ret.Substring(0, 4) == "NOOK")
                    {
                        //panda - Criar log
                    }
                    ret = "SEM_DADOS";
                }
            }
            else
            {
                ret = "SEM_DADOS";
            }

            return ret;
        }

        public string InformarPagamento(int idUsuario, int idBoard, int idUsuarioPag)
        {
            string sql = "Exec spC_TabuleiroInformarPagto @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard + ",@UsuarioIDPag=" + idUsuarioPag;
            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarPagtoSistema(int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroInformarPagtoSistema @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string InformarRecebimento(int idUsuario, int idUsuarioPai, int idBoard)
        {
            string sql = "Exec spC_TabuleiroInformarRecebimento @UsuarioID=" + idUsuario + ",@UsuarioPaiID=" + idUsuarioPai + ",@BoardID =" + idBoard;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public string RemoverUsuario(int idUsuario, int idMaster, int idBoard)
        {
            string sql = "Exec spD_TabuleiroExcluirUsuario @UsuarioID=" + idUsuario + ", @MasterID=" + idMaster + ", @BoardID =" + idBoard;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        public TabuleiroInfoUsuarioModel ObtemInfoUsuario(int idTarget, int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroInfoUsuario @TargetID=" + idTarget + ", @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;
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

        public int ObtemBoardIDByTabuleiroID(int idUsuario, int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroObtemBoardIDByTabuleiroID @UsuarioID=" + idUsuario + ",@TabuleiroId=" + idTabuleiro;
            var ret = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
            int retorno = int.Parse(ret);
            return retorno;
        }

        public int ObtemUsuarioIDPag(int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroObtemUsuarioIDPag @UsuarioID=" + idUsuario + ",@BoardID=" + idBoard;
            var ret = _context.Database.SqlQuery<int>(sql).FirstOrDefault();
            int retorno = ret;
            return retorno;
        }

        public string MasterRuleOK(int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroMasterRule @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;
            string retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
            return retorno;
        }

        public string MasterIndicadosOK(int idUsuario, int idBoard)
        {
            string sql = "Exec spC_TabuleiroIndicadosValidos @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;
            string retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
            return retorno;
        }

        public string UsuarioRuleOK(int idUsuario)
        {
            string sql = "Exec spC_TabuleiroUsuarioRule @UsuarioID=" + idUsuario;
            string retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
            return retorno;
        }

        public TabuleiroBoardModel ObtemTabuleiroBoard(int idTabuleiro)
        {
            string sql = "Exec spC_TabuleiroBoard @TabuleiroID=" + idTabuleiro;
            var retorno = _context.Database.SqlQuery<TabuleiroBoardModel>(sql).FirstOrDefault();

            return retorno;
        }

        public TabuleiroBoardModel ObtemTabuleiroBoardID(int idBoard)
        {
            string sql = "Exec spC_TabuleiroBoardID @BoardID=" + idBoard;
            var retorno = _context.Database.SqlQuery<TabuleiroBoardModel>(sql).FirstOrDefault();

            return retorno;
        }
        public string TabuleiroSair(int idUsuario, int idBoard)
        {

            string sql = "Exec spG_TabuleiroSair @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard;

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }

        /// <summary>
        /// Lista dos que informaram o pagamento ao sistema
        /// Usado no Admin
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TabuleiroInformaramPagtoModel> ObtemTabuleirosInformaramPgto()
        {
            string sql = "Exec spC_TabuleiroInformaramPagto";

            var retorno = _context.Database.SqlQuery<TabuleiroInformaramPagtoModel>(sql).ToList();

            return retorno;
        }

        public IEnumerable<TabuleiroInformaramPagtoModel> ObtemTabuleirosPagos()
        {
            string sql = "Exec spC_TabuleiroPagos";

            var retorno = _context.Database.SqlQuery<TabuleiroInformaramPagtoModel>(sql).ToList();

            return retorno;
        }

        public IEnumerable<TabuleiroUsuarioModel> ObtemTabuleirosNaoPagaramSistema()
        {
            string sql = "Exec spC_TabuleiroNaoPagaramSistema";

            var retorno = _context.Database.SqlQuery<TabuleiroUsuarioModel>(sql).ToList();

            return retorno;
        }

        public string ConfirmarPagtoSistema(int idUsuario, int idBoard, bool confirmar)
        {
            string sql = "";
            if (confirmar)
            {
                sql = "Exec spC_TabuleiroConfirmarPagtoSistema @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard + ", @confirmar='true'";
            }
            else
            {
                sql = "Exec spC_TabuleiroConfirmarPagtoSistema @UsuarioID=" + idUsuario + ", @BoardID=" + idBoard + ", @confirmar='false'";
            }

            var retorno = _context.Database.SqlQuery<string>(sql).FirstOrDefault();

            return retorno;
        }
        public bool TabuleiroFechado(int idTabuleiro)
        {
            bool retorno = false;
            if (idTabuleiro > 0)
            {
                string sql = "Exec spC_TabuleiroFechado @TabuleiroID=" + idTabuleiro;
                var ret = _context.Database.SqlQuery<string>(sql).FirstOrDefault();
                if (ret == "sim")
                {
                    retorno = true;
                }
            }
            return retorno;
        }

        public IEnumerable<TabuleiroIndicados> ObtemTabuleirosIndicados(int idUsuario)
        {
            string sql = "Exec spC_TabuleiroIndicados @UsuarioID=" + idUsuario;

            IEnumerable<TabuleiroIndicados> tabuleiroIndicados = _context.Database.SqlQuery<TabuleiroIndicados>(sql).ToList();

            foreach (TabuleiroIndicados item in tabuleiroIndicados)
            {
                item.Galaxia = item.BoardNome.Substring(0, 3).ToUpper() + "-" + (item.TabuleiroID.HasValue ? item.TabuleiroID.Value : 0).ToString("000000");
            }
            
            return tabuleiroIndicados;
        }

        public DateTime TabuleiroGetDate()
        {
            string sql = "Exec spC_TabuleiroGetDate";
            var ret = _context.Database.SqlQuery<DateTime>(sql).FirstOrDefault();
            
            return ret;
        }

    }
}
