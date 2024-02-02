#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Sistema.Controllers;
using System.Web.Configuration;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;

//Models Local
using Sistema.Models;

//Lista
using PagedList;

//Excel
using System.Security.Claims;
using ClosedXML.Excel;
using System.IO;

//Utilities
using System.Drawing;
using System.Configuration;
using Core.Repositories.Usuario;
using Core.Repositories.Sistema;
using Core.Helpers;

#endregion

namespace Helpers
{
    public class Local : Controller
    {

        #region Sistema

        /// <summary>
        /// Retorna o Nome do Sistema
        /// </summary>
        public static int idEmpresaWebConfig
        {
            get
            {
                int idintEmpresa = 0;
                string strRetorno = WebConfigurationManager.AppSettings["Empresa"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "1";
                }
                idintEmpresa = Convert.ToInt32(strRetorno);
                return idintEmpresa;
            }
        }

        #endregion

        #region Autenticacao

        /// <summary>
        /// Retorna o id da Empresa do Administrador Logado
        /// </summary>
        public static int idEmpresa
        {
            get
            {
                int intRetorno = 0;
                YLEVELEntities context;
                AdministradorRepository administradorRepository;
                try
                {
                    context = new YLEVELEntities();
                    administradorRepository = new AdministradorRepository(context);
                    var usuario = administradorRepository.GetByAutenticacao(idAutenticacao);
                    intRetorno = usuario.EmpresaID;
                }
                catch (Exception)
                {
                    intRetorno = 0;
                }
                finally
                {
                    administradorRepository = null;
                    context = null;
                }

                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o id do Usuario Logado no identity
        /// </summary>
        public static int idAutenticacao
        {
            get
            {
                string strId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                int intRetorno = Convert.ToInt16(strId);
                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o id do Administrador Logado
        /// </summary>
        public static int idUsuario
        {
            get
            {
                int intRetorno = 0;
                YLEVELEntities context;
                AdministradorRepository administradorRepository;
                try
                {
                    context = new YLEVELEntities();
                    administradorRepository = new AdministradorRepository(context);
                    var usuario = administradorRepository.GetByAutenticacao(idAutenticacao);
                    if (idAutenticacao > 0)
                    {
                        intRetorno = usuario.ID;
                    }
                }
                catch (Exception)
                {
                    intRetorno = 0;
                }
                finally
                {
                    administradorRepository = null;
                    context = null;
                }

                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o id do Administrador Logado
        /// </summary>
        public static Administrador usuario
        {
            get
            {
                Administrador intRetorno;
                YLEVELEntities context;
                AdministradorRepository administradorRepository;
                try
                {
                    context = new YLEVELEntities();
                    administradorRepository = new AdministradorRepository(context);
                    var usuario = administradorRepository.GetByAutenticacao(idAutenticacao);
                    intRetorno = usuario;
                }
                catch (Exception)
                {
                    intRetorno = null;
                }
                finally
                {
                    administradorRepository = null;
                    context = null;
                }

                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o id do Usuario Logado
        /// </summary>
        public static string nomeUsuario
        {
            get
            {
                string strNome = System.Web.HttpContext.Current.User.Identity.GetUserName();
                return strNome;
            }
        }

        /// <summary>
        /// Retorna o pais do Administrador
        /// </summary>
        public static Core.Entities.Idioma UsuarioIdioma
        {
            get
            {
                Core.Entities.Idioma retorno = null;
                YLEVELEntities context;
                AdministradorRepository administradorRepository;
                try
                {
                    context = new YLEVELEntities();
                    administradorRepository = new AdministradorRepository(context);

                    int idAut = 1;
                    if (idAutenticacao > 0)
                    {
                        idAut = idAutenticacao;
                    }
                    Administrador administrador = administradorRepository.GetByAutenticacao(idAut);
                    if (administrador == null)
                    {
                        idAut = 1;
                        administrador = administradorRepository.GetByAutenticacao(idAut);
                    }
                    retorno = administrador.Pais.Idioma;
                }
                catch (Exception ex)
                {
                    retorno = null;
                }
                finally
                {
                    administradorRepository = null;
                    context = null;
                }

                return retorno;
            }
        }

        /// <summary>
        /// Retorna as regras do Usuario Logado
        /// </summary>
        public static string Regra
        {
            get
            {
                string strRetorno = "";
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var query = db.Roles.Where(x => x.Users.Select(y => y.UserId).Contains(idAutenticacao));
                    foreach (var item in query)
                    {
                        strRetorno = item.Name;
                    }
                }
                return strRetorno;
            }
        }

        #endregion

        #region Geral

        /// <summary>
        /// Retorna o Nome do Sistema
        /// </summary>
        public static string Sistema
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["Sistema"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "Default";
                }
                return strRetorno;
            }
        }

        /// <summary>
        /// Retorna o Versão do Sistema
        /// </summary>
        public static string Versao
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["Versao"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "1.0";
                }
                return strRetorno;
            }
        }

        /// <summary>
        /// Retorna o no do loadbalance que a aplicacao esta rodando
        /// </summary>
        public static string no
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["no"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "0";
                }
                return strRetorno;
            }
        }

        /// <summary>
        /// Retorna o Cliente do Sistema
        /// </summary>
        public static string Cliente
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["Cliente"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "Sistema";
                }
                return strRetorno;
            }
        }

        /// <summary>
        /// Verifica se a senha é forte
        /// </summary>
        /// <param name="senha"></param>
        /// <returns></returns>
        public static bool verificaSenhaForte(string senha)
        {
            //entre 6 e 12 caracteres?
            if (senha.Length < 6 || senha.Length > 12)
                return false;
            //Há numero?
            if (!senha.Any(c => char.IsDigit(c)))
                return false;
            //Há letra Maiuscula
            if (!senha.Any(c => char.IsUpper(c)))
                return false;
            //Há letra Minuscula
            if (!senha.Any(c => char.IsLower(c)))
                return false;

            //Há simbolo
            if (!senha.Any(c => char.IsSymbol(c)))
                return false;

            //há mais de 2 caracteres repetidos?
            var contadorRepetido = 0;
            var ultimoCaracter = '\0';
            foreach (var c in senha)
            {
                if (c == ultimoCaracter)
                    contadorRepetido++;
                else
                    contadorRepetido = 0;
                if (contadorRepetido == 2)
                    return false;
                ultimoCaracter = c;
            }

            return true;
        }

        public static bool ChecarImagem(HttpPostedFileBase imagem, ref int intX, ref int intY, ref float floTamanho, bool blnExatamanete)
        {
            bool blnValido = true;
            int intXImg = 0;
            int intYImg = 0;
            float floTamanhoImg = 0;

            //imagem
            if (imagem != null)
            {
                floTamanhoImg = imagem.ContentLength;
                //Imagem somente menor que Somente até 500M
                if (floTamanhoImg > (floTamanho * (1024 * 1024)))
                {
                    blnValido = false;
                }
                if (!(imagem.ContentType == "image/jpeg" || (imagem.ContentType == "image/png")))
                {
                    blnValido = false;
                }

                //Obtem dimensoes da imagem
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(imagem.InputStream, true, true))
                {
                    intXImg = image.Width;
                    intYImg = image.Height;
                }

                if (blnExatamanete)
                {
                    //Exatamente o tamanho
                    if (intXImg != intX && intYImg != intY)
                    {
                        blnValido = false;
                    }
                }
                else
                {
                    //Caso seja maior do que o permitido
                    if (intXImg > intX || intYImg > intY)
                    {
                        blnValido = false;
                    }
                }
            }
            intX = intXImg;
            intY = intYImg;
            floTamanho = (floTamanhoImg / (1024));

            return blnValido;
        }

        public static string Ambiente
        {
            get
            {
                string strRetorno = ConfigurationManager.AppSettings["Ambiente"];
                return strRetorno;
            }
        }

        public static string removerAcentos(string input)
        {

            if (string.IsNullOrEmpty(input))
                return "";
            else
            {
                byte[] bytes = System.Text.Encoding.GetEncoding("iso-8859-8").GetBytes(input);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        /// <summary>
        /// Loga acessos indevidos
        /// </summary>
        /// <param name="chave"></param>
        /// <returns></returns>
        public static void LogAcesso(string nome, string senha, string ip)
        {

            #region Variaveis

            YLEVELEntities context;
            LogAcessoRepository logAcessoRepository;

            #endregion

            try
            {
                context = new YLEVELEntities();
                logAcessoRepository = new LogAcessoRepository(context);

                var logAcesso = new LogAcesso()
                {
                    DATA = App.DateTimeZion,
                    IP = ip,
                    USUARIO = nome,
                    SENHA = cpUtilities.Gerais.Morpho(senha, "morpheus", cpUtilities.TipoCriptografia.Criptografa)
                };

                logAcessoRepository.Save(logAcesso);
            }
            catch (Exception)
            {
            }
            finally
            {
                logAcessoRepository = null;
                context = null;
            }
        }

        /// <summary>
        /// 
        /// Verifica se há conexao com o banco de dados
        /// </summary>
        public static bool BancoConexao
        {
            get
            {
                bool blnRetorno = false;

                GenericRepository<Paginacao> objTab;
                IEnumerable<Paginacao> objReg;
                try
                {
                    objTab = new GenericRepository<Paginacao>();
                    objReg = objTab.FindBy(x => x.id == 1);
                    if (objReg != null)
                    {
                        foreach (Paginacao objFor in objReg)
                        {
                            blnRetorno = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string strErro = ex.Message;
                    blnRetorno = false;
                }
                finally
                {
                    objReg = null;
                    objTab = null;
                }

                return blnRetorno;
            }
        }

        /// <summary>
        /// Log de eventos
        /// </summary>
        /// <param name="local"></param>
        /// <param name="descricao"></param>
        public static void Log(string local, string descricao)
        {

            #region Variaveis

            YLEVELEntities context;
            LogRepository logRepository;

            #endregion

            try
            {
                context = new YLEVELEntities();
                logRepository = new LogRepository(context);

                var log = new Log()
                {
                    Data = App.DateTimeZion,
                    Local = local,
                    Descricao = descricao
                };

                logRepository.Save(log);
            }
            catch (Exception)
            {
            }
            finally
            {
                logRepository = null;
                context = null;
            }
        }

        #endregion

    }

    public class Funcoes
    {
        #region Context

        private HttpContextBase Context { get; set; }
        private Dictionary<string, object> parametros { get; set; }

        public Funcoes(HttpContextBase context)
        {
            this.Context = context;
            this.parametros = new Dictionary<string, object>();
        }

        #endregion

        #region Percistencia

        public void GerenciaParametro(string key, ref string value, string strPagina)
        {
            var sessionName = string.Format("{0}-{1}", strPagina, key);

            if (value == null)
            {
                if (Context.Session[sessionName] != null)
                {
                    value = Context.Session[sessionName].ToString();
                }
            }
            else
            {
                Context.Session[sessionName] = value;
            }
        }

        public void GerenciaParametro(string key, ref int? value, string strPagina)
        {
            var sessionName = string.Format("{0}-{1}", strPagina, key);

            if (!value.HasValue)
            {
                if (Context.Session[sessionName] != null)
                {
                    value = (int)Context.Session[sessionName];
                }
            }
            else
            {
                Context.Session[sessionName] = value.Value;
            }
        }

        public void GerenciaPaginacao(ref string SortOrder, ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            #region Pagina

            if (NumeroPaginas == null)
            {
                if (Context.Session["NumeroPaginas"] != null)
                {
                    NumeroPaginas = Convert.ToInt32(Context.Session["NumeroPaginas"]);
                }
                else
                {
                    NumeroPaginas = 40;
                    Context.Session["NumeroPaginas"] = 40;
                }
            }
            else
            {
                //Caso se altere a qtde de listagem por pagina, deve voltar para a pagina 1
                if (Context.Session["NumeroPaginas"] != null)
                {
                    if (Convert.ToInt32(Context.Session["NumeroPaginas"]) != NumeroPaginas)
                    {
                        Page = 1;
                    }
                }
                Context.Session["NumeroPaginas"] = NumeroPaginas;
            }

            if (Page == null)
            {
                if (Context.Session["Page"] != null)
                {
                    Page = Convert.ToInt32(Context.Session["Page"]);
                }
            }
            else
            {
                Context.Session["Page"] = Page;
            }

            if (String.IsNullOrEmpty(SortOrder))
            {
                if (Context.Session["SortOrder"] != null)
                {
                    if (!String.IsNullOrEmpty(Context.Session["SortOrder"].ToString()))
                    {
                        SortOrder = Context.Session["SortOrder"].ToString();
                    }
                }
            }
            else
            {
                Context.Session["SortOrder"] = SortOrder;
            }

            #endregion
        }

        public void LimparPersistencia()
        {
            Context.Session["Limpar"] = "true";
        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 1
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            //Inicia Tela trabalhada pelo usuario
            if (Context.Session["Pagina"] == null)
            {
                Context.Session["Pagina"] = strPagina;
            }

            ResetaItem(1, ref CurrentProcuraItem1, ref ProcuraItem1, strPagina);

            //Caso tenha trocado de tela ou caso tenha clicado no botao refresh da tela
            if (Context.Session["Pagina"] != null && Context.Session["Pagina"].ToString() != strPagina || Context.Session["Limpar"] != null)
            {
                SortOrder = null;
                NumeroPaginas = null;
                Page = null;

                Context.Session["SortOrder"] = null;
                Context.Session["Page"] = null;
                Context.Session["Pagina"] = strPagina;
                Context.Session["Limpar"] = null;
            }

            ControlaItem(1, ref CurrentProcuraItem1, ref ProcuraItem1);

            GerenciaPaginacao(ref SortOrder, ref NumeroPaginas, ref Page, strPagina);

        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 2
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            ResetaItem(2, ref CurrentProcuraItem2, ref ProcuraItem2, strPagina);


            Persistencia(ref SortOrder,
                ref CurrentProcuraItem1, ref ProcuraItem1,
                ref NumeroPaginas, ref Page, strPagina);

            ControlaItem(2, ref CurrentProcuraItem2, ref ProcuraItem2);
        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 3
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {

            ResetaItem(3, ref CurrentProcuraItem3, ref ProcuraItem3, strPagina);


            Persistencia(ref SortOrder,
                     ref CurrentProcuraItem1, ref ProcuraItem1,
                     ref CurrentProcuraItem2, ref ProcuraItem2,
                     ref NumeroPaginas, ref Page, strPagina);


            ControlaItem(3, ref CurrentProcuraItem3, ref ProcuraItem3);

        }


        /// <summary>
        /// Percistencia dos parametros da pagina - 4
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="CurrentProcuraItem4"></param>
        /// <param name="ProcuraItem4"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            ResetaItem(4, ref CurrentProcuraItem4, ref ProcuraItem4, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref NumeroPaginas, ref Page, strPagina);

            ControlaItem(4, ref CurrentProcuraItem4, ref ProcuraItem4);

        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 5
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="CurrentProcuraItem4"></param>
        /// <param name="ProcuraItem4"></param>
        /// <param name="CurrentProcuraItem5"></param>
        /// <param name="ProcuraItem5"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref string CurrentProcuraItem5, ref string ProcuraItem5,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            ResetaItem(5, ref CurrentProcuraItem5, ref ProcuraItem5, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref CurrentProcuraItem4, ref ProcuraItem4,
                                 ref NumeroPaginas, ref Page, strPagina);

            ControlaItem(5, ref CurrentProcuraItem5, ref ProcuraItem5);

        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 6
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="CurrentProcuraItem4"></param>
        /// <param name="ProcuraItem4"></param>
        /// <param name="CurrentProcuraItem5"></param>
        /// <param name="ProcuraItem5"></param>
        /// <param name="CurrentProcuraItem6"></param>
        /// <param name="ProcuraItem6"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref string CurrentProcuraItem5, ref string ProcuraItem5,
                                 ref string CurrentProcuraItem6, ref string ProcuraItem6,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {

            ResetaItem(6, ref CurrentProcuraItem6, ref ProcuraItem6, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref CurrentProcuraItem4, ref ProcuraItem4,
                                 ref CurrentProcuraItem5, ref ProcuraItem5,
                                 ref NumeroPaginas, ref Page, strPagina);


            ControlaItem(6, ref CurrentProcuraItem6, ref ProcuraItem6);

        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 7
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="CurrentProcuraItem4"></param>
        /// <param name="ProcuraItem4"></param>
        /// <param name="CurrentProcuraItem5"></param>
        /// <param name="ProcuraItem5"></param>
        /// <param name="CurrentProcuraItem6"></param>
        /// <param name="ProcuraItem6"></param>
        /// <param name="CurrentProcuraItem7"></param>
        /// <param name="ProcuraItem7"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref string CurrentProcuraItem5, ref string ProcuraItem5,
                                 ref string CurrentProcuraItem6, ref string ProcuraItem6,
                                 ref string CurrentProcuraItem7, ref string ProcuraItem7,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            ResetaItem(7, ref CurrentProcuraItem7, ref ProcuraItem7, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref CurrentProcuraItem4, ref ProcuraItem4,
                                 ref CurrentProcuraItem5, ref ProcuraItem5,
                                 ref CurrentProcuraItem6, ref ProcuraItem6,
                                 ref NumeroPaginas, ref Page, strPagina);


            ControlaItem(7, ref CurrentProcuraItem7, ref ProcuraItem7);
        }

        /// <summary>
        /// Percistencia dos parametros da pagina - 7
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="CurrentProcuraItem3"></param>
        /// <param name="ProcuraItem3"></param>
        /// <param name="CurrentProcuraItem4"></param>
        /// <param name="ProcuraItem4"></param>
        /// <param name="CurrentProcuraItem5"></param>
        /// <param name="ProcuraItem5"></param>
        /// <param name="CurrentProcuraItem6"></param>
        /// <param name="ProcuraItem6"></param>
        /// <param name="CurrentProcuraItem7"></param>
        /// <param name="ProcuraItem7"></param>
        /// <param name="CurrentProcuraItem8"></param>
        /// <param name="ProcuraItem8"></param>
        /// 
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref string CurrentProcuraItem5, ref string ProcuraItem5,
                                 ref string CurrentProcuraItem6, ref string ProcuraItem6,
                                 ref string CurrentProcuraItem7, ref string ProcuraItem7,
                                 ref string CurrentProcuraItem8, ref string ProcuraItem8,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {

            ResetaItem(8, ref CurrentProcuraItem8, ref ProcuraItem8, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref CurrentProcuraItem4, ref ProcuraItem4,
                                 ref CurrentProcuraItem5, ref ProcuraItem5,
                                 ref CurrentProcuraItem6, ref ProcuraItem6,
                                 ref CurrentProcuraItem7, ref ProcuraItem7,
                                 ref NumeroPaginas, ref Page, strPagina);


            ControlaItem(8, ref CurrentProcuraItem8, ref ProcuraItem8);

        }

        public void Persistencia(ref string SortOrder,
                                 ref string CurrentProcuraItem1, ref string ProcuraItem1,
                                 ref string CurrentProcuraItem2, ref string ProcuraItem2,
                                 ref string CurrentProcuraItem3, ref string ProcuraItem3,
                                 ref string CurrentProcuraItem4, ref string ProcuraItem4,
                                 ref string CurrentProcuraItem5, ref string ProcuraItem5,
                                 ref string CurrentProcuraItem6, ref string ProcuraItem6,
                                 ref string CurrentProcuraItem7, ref string ProcuraItem7,
                                 ref string CurrentProcuraItem8, ref string ProcuraItem8,
                                 ref string CurrentProcuraItem9, ref string ProcuraItem9,
                                 ref int? NumeroPaginas, ref int? Page, string strPagina)
        {

            ResetaItem(9, ref CurrentProcuraItem9, ref ProcuraItem9, strPagina);

            Persistencia(ref SortOrder,
                                 ref CurrentProcuraItem1, ref ProcuraItem1,
                                 ref CurrentProcuraItem2, ref ProcuraItem2,
                                 ref CurrentProcuraItem3, ref ProcuraItem3,
                                 ref CurrentProcuraItem4, ref ProcuraItem4,
                                 ref CurrentProcuraItem5, ref ProcuraItem5,
                                 ref CurrentProcuraItem6, ref ProcuraItem6,
                                 ref CurrentProcuraItem7, ref ProcuraItem7,
                                 ref CurrentProcuraItem8, ref ProcuraItem8,
                                 ref NumeroPaginas, ref Page, strPagina);

            ControlaItem(9, ref CurrentProcuraItem9, ref ProcuraItem9);
        }

        private void ControlaItem(int index, ref string CurrentProcuraItem, ref string ProcuraItem)
        {
            var nomeSessionCurrent = CriaNomeSessionCurrent(index);
            var nomeSessionProcura = CriaNomeSessionProcura(index);

            if (String.IsNullOrEmpty(CurrentProcuraItem))
            {
                if (CurrentProcuraItem != "")
                {
                    if (Context.Session[nomeSessionCurrent] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session[nomeSessionCurrent].ToString()))
                        {
                            CurrentProcuraItem = Context.Session[nomeSessionCurrent].ToString();
                        }
                    }
                }
                else
                {
                    CurrentProcuraItem = null;
                    Context.Session[nomeSessionCurrent] = null;
                }
            }
            else
            {
                Context.Session[nomeSessionCurrent] = CurrentProcuraItem;
            }

            if (String.IsNullOrEmpty(ProcuraItem))
            {
                if (ProcuraItem != "")
                {
                    if (Context.Session[nomeSessionProcura] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session[nomeSessionProcura].ToString()))
                        {
                            ProcuraItem = Context.Session[nomeSessionProcura].ToString();
                        }
                    }
                }
                else
                {
                    ProcuraItem = null;
                    Context.Session[nomeSessionProcura] = null;
                }
            }
            else
            {
                Context.Session[nomeSessionProcura] = ProcuraItem;
            }
        }

        private void ResetaItem(int index, ref string CurrentProcuraItem, ref string ProcuraItem, string strPagina)
        {
            //Caso tenha trocado de tela ou caso tenha clicado no botao refresh da tela
            if (Context.Session["Pagina"] != null && Context.Session["Pagina"].ToString() != strPagina || Context.Session["Limpar"] != null)
            {
                CurrentProcuraItem = null;
                ProcuraItem = null;
                Context.Session[CriaNomeSessionCurrent(index)] = null;
                Context.Session[CriaNomeSessionProcura(index)] = null;
            }
        }

        private string CriaNomeSessionCurrent(int index)
        {
            return string.Format("CurrentProcuraItem{0}", index);
        }

        private string CriaNomeSessionProcura(int index)
        {
            return string.Format("ProcuraItem{0}", index);
        }

        #endregion

    }
}