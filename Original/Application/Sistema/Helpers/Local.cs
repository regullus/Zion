#region Bibliotecas

//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;
//using System.Web.UI;
using Core.Helpers;
using Core.Repositories.Globalizacao;
using Core.Repositories.Sistema;
using Core.Repositories.Usuario;
//Identyty
using Microsoft.AspNet.Identity;
//Models Local
using Sistema.Models;
using System;
using System.Collections.Generic;
//Lista
//using PagedList;

//Excel
//using System.Security.Claims;
//using ClosedXML.Excel;
//using System.IO;

//Utilities
//using System.Drawing;
using System.Configuration;
using System.Data;
using System.IO;
//using System.Data.Entity;
using System.Linq;
using System.Text;
//using System.Net;
using System.Web;
//using System.Threading.Tasks;
//using System.Threading;
//using Sistema.Controllers;
using System.Web.Configuration;
using System.Web.Mvc;

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
        /// Retorna o id da Empresa do Usuario Logado
        /// </summary>
        public static int idEmpresa
        {
            get
            {
                int intRetorno = 0;
                YLEVELEntities context;
                UsuarioRepository usuarioRepository;
                try
                {
                    context = new YLEVELEntities();
                    usuarioRepository = new UsuarioRepository(context);
                    var usuario = usuarioRepository.GetByAutenticacao(idAutenticacao);
                    intRetorno = usuario.EmpresaID;
                }
                catch (Exception)
                {
                    intRetorno = 0;
                }
                finally
                {
                    usuarioRepository = null;
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
                int intRetorno = Convert.ToInt32(strId);
                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o id do Usuario Logado
        /// </summary>
        public static int idUsuario
        {
            get
            {
                int intRetorno = 0;
                YLEVELEntities context;
                UsuarioRepository usuarioRepository;
                try
                {
                    context = new YLEVELEntities();
                    usuarioRepository = new UsuarioRepository(context);
                    var usuario = usuarioRepository.GetByAutenticacao(idAutenticacao);
                    if (usuario != null)
                    {
                        intRetorno = usuario.ID;
                    }
                    else
                    {
                        intRetorno = 0;
                    }
                }
                catch (Exception)
                {
                    intRetorno = 0;
                }
                finally
                {
                    usuarioRepository = null;
                    context = null;
                }

                return intRetorno;
            }
        }

        /// <summary>
        /// Retorna o pais do usuario
        /// </summary>
        public static Core.Entities.Idioma UsuarioIdioma
        {
            get
            {
                Core.Entities.Idioma retorno = null;
                YLEVELEntities context;
                UsuarioRepository usuarioRepository;
                try
                {
                    context = new YLEVELEntities();
                    usuarioRepository = new UsuarioRepository(context);
                    var usuario = usuarioRepository.GetByAutenticacao(idAutenticacao);
                    if (usuario != null)
                    {
                        retorno = usuario.Pais.Idioma;
                    }
                    else
                    {
                        retorno = null;
                    }
                }
                catch (Exception)
                {
                    retorno = null;
                }
                finally
                {
                    usuarioRepository = null;
                    context = null;
                }

                return retorno;
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

        public static TraducaoHelper TraducaoHelper
        {
            get
            {
                var httpContext = System.Web.HttpContext.Current;
                var idioma = (httpContext.Session["Idioma"] != null) ?
                    (Idioma)httpContext.Session["Idioma"] :
                    UsuarioIdioma;

                if (idioma == null)
                {
                    Pais pais = new Pais();

                    if (httpContext.Session["pais"] != null)
                    {
                        pais = (Pais)httpContext.Session["pais"];
                    }
                    else
                    {
                        //pais = new PaisRepository(new YLEVELEntities()).GetBySigla("es-ES");
                        pais = new PaisRepository(new YLEVELEntities()).GetPadrao();
                    }

                    //var pais = (httpContext.Session["pais"] != null) 
                    //? (Pais)httpContext.Session["pais"] 
                    //: new PaisRepository(new YLEVELEntities()).GetBySigla("ES");
                    if (pais != null)
                        idioma = pais.Idioma;
                    else
                        idioma = new IdiomaRepository(new YLEVELEntities()).GetBySigla("pt-BR");

                }

                return new Core.Helpers.TraducaoHelper(idioma);
            }
        }


        #endregion

        #region Geral

        /// <summary>
        /// Retorna a senha para autenticação na API Cielo
        /// </summary>
        public static string CieloPassword
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["CieloPassword"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "0";
                }
                return strRetorno;
            }
        }

        /// <summary>
        /// Retorna a URL para acesso da API Cielo
        /// </summary>
        public static string CieloURL
        {
            get
            {
                string strRetorno = WebConfigurationManager.AppSettings["CieloURL"];
                if (String.IsNullOrEmpty(strRetorno))
                {
                    strRetorno = "";
                }
                return strRetorno;
            }
        }

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
                    strRetorno = "MediaTron";
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
                LogErro(descricao, "Log");
            }
            catch (Exception ex)
            {
                LogErro(ex.Message, "LogErro");
            }
            finally
            {
                logRepository = null;
                context = null;
            }
        }

        private static void LogErro(string log, string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = "LogErro";
            } else
            {
                fileName = fileName;
            }
            
            fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + fileName;

            cpUtilities.LoggerHelper.WriteFile(log, fileName);

        }

        #endregion

        #region PageTour

        /// <summary>
        /// Gera um JavaScript para a presentação do Help por pagina
        /// </summary>
        /// <param name="titulo"></param>
        /// <param name="textos"></param>
        /// <param name="vidios"></param>
        /// <returns>string</returns>
        public static string GerarJsPageTourPagina(string titulo,
                                                   Dictionary<string, string> textos,
                                                   Dictionary<string, string> videos = null)
        {
            int largura = ConfiguracaoHelper.GetInt("PAGE_TOUR_WINDOW_WIDTH");
            int altura = (largura - 40) * 5625 / 10000;
            string strVirgula = "";
            string strUrl = "";
            StringBuilder strbJson = new StringBuilder();

            strbJson.Append(@"<script type = ""text/javascript"">");

            strbJson.Append(@"$(function() {");
            strbJson.Append(@"$('.StartPtPagina').on('click', function() {");
            strbJson.Append(@"$.ptJs({");

            strbJson.Append(@"autoStart: true,");
            strbJson.Append(GetAnimation());
            strbJson.Append(@"continueEnable: true,");
            strbJson.Append(@"size: {width: " + largura + "},");

            strbJson.Append(@"templateData:{");
            strbJson.Append(@"title: '" + @titulo + "',");
            strbJson.Append(@"description: '" + "_mensagemPopUpContinue" + "',");
            strbJson.Append(@"'button-end': '" + "_fim" + "',");
            strbJson.Append(@"'button-next': '" + "_proximo" + "',");
            strbJson.Append(@"'button-previous': '" + "_anterior" + "',");
            strbJson.Append(@"'button-start': '" + "_inicio" + "',");
            strbJson.Append(@"'button-restart': '" + "_reiniciar" + "',");
            strbJson.Append(@"'button-continue': '" + "_continuar" + "'");
            strbJson.Append(@"},");
            strbJson.Append(@"templateViewContinue: ");
            strbJson.Append(@"'<div class=""step-container"" ptjs-id=""ptjs-step-container""> ");
            strbJson.Append(@"<span class=""step-button-close"" ptjs-id=""button-close"" ptjs-data=""button-close""></span>");
            strbJson.Append(@"<div class=""step-header"" ptjs-id=""header"">");
            strbJson.Append(@"</div>");
            strbJson.Append(@"<div class=""step-body"" ptjs-id=""body"">");
            strbJson.Append(@"<p ptjs-data=""description""></p> ");
            strbJson.Append(@"</div>");
            strbJson.Append(@"<div class=""step-footer"" ptjs-id=""footer"">");
            strbJson.Append(@"<div class=""step-buttons"">");
            strbJson.Append(@"<span ptjs-id=""button-restart"" ptjs-data=""button-restart""></span> ");
            strbJson.Append(@"<span ptjs-id=""button-continue"" ptjs-data=""button-continue""></span> ");
            strbJson.Append(@"</div>");
            strbJson.Append(@"</div>");
            strbJson.Append(@"</div>',");

            strbJson.Append(@"steps: [");
            foreach (var txt in textos)
            {
                strbJson.Append(strVirgula + @"{");
                strbJson.Append(@"el: '#" + txt.Key + "',");
                strbJson.Append(@"templateData:{content: '" + @txt.Value + "'");

                if (videos != null && videos.ContainsKey(txt.Key))
                {
                    strUrl = @videos[txt.Key];
                    if (strUrl.Length > 0 && (strUrl.Substring(0, 1) == "~" || strUrl.Substring(0, 1) == "/"))
                        strbJson.Append(@" },");
                    else
                        strbJson.Append(@", videoUrl: '" + @videos[txt.Key] + "?rel=0&amp;showinfo=0'},");

                    strbJson.Append(@"templateViewWindow: '<div class=""step-container"" ptjs-id=""ptjs-step-container""> ");
                    strbJson.Append(@"<span class=""step-button-close"" ptjs-id=""button-close"" ptjs-data=""button-close""></span>");
                    strbJson.Append(@"<div class=""step-header"" ptjs-id=""header"">");
                    strbJson.Append(@"<div class=""title"" ptjs-data=""title""></div> </div>");
                    strbJson.Append(@"<div class=""step-body"" ptjs-id=""body"">");
                    strbJson.Append(@"<div class=""content"" ptjs-data=""content""></div><br>");

                    if (strUrl.Length > 0 && strUrl.Substring(0, 1) == "~")
                    {
                        strbJson.Append(@"<div class=""content"">");
                        strbJson.Append(@"<video controls controlsList=""nodownload"" type=""video/mp4"" preload=""metadata"" oncontextmenu=""return false;"" src=""" + ConfiguracaoHelper.GetString("URL_ADMINISTRACAO") + strUrl.Substring(1, strUrl.Length - 1) + @""" style=""width:100%; height:100%; border:0;""></video>");
                        strbJson.Append(@"</div>");
                    }
                    else
                    {
                        if (strUrl.Length > 0 && strUrl.Substring(0, 1) == "/")
                        {
                            strbJson.Append(@"<div class=""content"">");
                            strbJson.Append(@"<video controls controlsList=""nodownload"" type=""video/mp4"" preload=""metadata"" oncontextmenu=""return false;"" src=""" + ConfiguracaoHelper.GetString("URL_CDN") + strUrl.Substring(1, strUrl.Length - 1) + @""" style=""width:100%; height:100%; border:0;""></video>");
                            strbJson.Append(@"</div>");
                        }
                        else
                            strbJson.Append(@"<iframe width=""100%"" " + @"height=""" + altura + @""" ptjs-src=""videoUrl"" frameborder=""0"" allownetworking = ""internal"" allowfullscreen sandbox=""allow-forms allow-scripts allow-pointer-lock allow-same-origin allow-top-navigation""></iframe> </div>");
                    }

                    strbJson.Append(@"<div class=""step-footer"" ptjs-id=""footer"">");
                    strbJson.Append(@"<span class=""step-pagination"" ptjs-id=""pagination"" ptjs-data=""pagination""></span>");
                    strbJson.Append(@"<div class=""step-buttons"">");
                    strbJson.Append(@"<span ptjs-id=""button-start"" ptjs-data=""button-start""></span>");
                    strbJson.Append(@"<span ptjs-id=""button-previous"" ptjs-data=""button-previous""></span>");
                    strbJson.Append(@"<span ptjs-id=""button-next"" ptjs-data=""button-next""></span>");
                    strbJson.Append(@"<span ptjs-id=""button-end"" ptjs-data=""button-end""></span>");
                    strbJson.Append(@"</div> </div> </div>'");
                }
                else
                {
                    strbJson.Append(@"}");
                }

                strbJson.Append(@"}");
                strVirgula = ",";
            }
            strbJson.Append(@"]");

            strbJson.Append(@"});});});");

            strbJson.Append(@"</script>");

            return strbJson.ToString();
        }

        /// <summary>
        /// Gera um JavaScript para a presentação do Help por macação
        /// </summary>
        /// <param name="titulo"></param>
        /// <param name="identificador"></param>
        /// <param name="textos"></param>
        /// <returns>string</returns>
        public static string GerarJsPageTourMarcador(string titulo,
                                                     Dictionary<string, string> textos,
                                                     int identificador = 0)
        {
            int largura = ConfiguracaoHelper.GetInt("PAGE_TOUR_WINDOW_WIDTH");
            //int altura = (largura - 40) * 5625 / 10000;

            string strVirgula = "";
            StringBuilder strbJson = new StringBuilder();

            strbJson.Append(@"<script type = ""text/javascript"">");


            strbJson.Append(@"  var blnIniciado = false; ");

            strbJson.Append(@"$(function() {");
            strbJson.Append(@"$('.StartPtMarcador" + identificador + @"').on('click', function() {");

            strbJson.Append(@"$.ptJs({");
            strbJson.Append(@"autoDestroy: true,");
            strbJson.Append(@"autoStart: true,");
            strbJson.Append(@"position: {location: 'tl-m'},");
            strbJson.Append(@"display: {mode: 'marker'},");
            strbJson.Append(@"instanceName: true,");
            strbJson.Append(@"size: {width: " + largura + "},");
            strbJson.Append(GetAnimation());
            strbJson.Append(@"templateData:{");
            strbJson.Append(@"title: '" + titulo + "',");
            strbJson.Append(@"},");

            strbJson.Append(@"steps: [");
            foreach (var txt in textos)
            {
                strbJson.Append(strVirgula + @"{");
                strbJson.Append(@"el: '#" + txt.Key + "',");
                strbJson.Append(@"templateData:{content: '" + txt.Value + "'}");

                strbJson.Append(@"}");
                strVirgula = ",";
            }
            strbJson.Append(@"]");

            strbJson.Append(@"});");

            strbJson.Append(@"});});");

            strbJson.Append(@"</script>");

            return strbJson.ToString();
        }

        private static string GetAnimation()
        {
            // Tipos de animação: ptJs_bounce, ptJs_bounceDirection, ptJs_fade,ptJs_fadeDirection, ptJs_flipDirection,
            //                    ptJs_lightSpeed, ptJs_roll, ptJs_rotate, ptJs_slideDirection, ptJs_zoom,ptJs_zoomDirection

            return @"animation: {name: ['ptJs_zoomDirection','ptJs_zoom']},";
        }

        #endregion

    }

    public class Funcoes
    {
        #region Context

        private HttpContextBase Context { get; set; }

        public Funcoes(HttpContextBase context)
        {
            this.Context = context;
        }

        #endregion

        #region Persistencia

        /// <summary>
        /// Percistencia dos parametros da pagina
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder, ref string CurrentProcuraItem1, ref string ProcuraItem1, ref string CurrentProcuraItem2, ref string ProcuraItem2, ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            #region Inicio

            //Inicia Tela trabalhada pelo usuario
            if (Context.Session["Pagina"] == null)
            {
                Context.Session["Pagina"] = strPagina;
            }

            //Caso tenha trocado de tela ou caso tenha clicado no botao refresh da tela
            if (Context.Session["Pagina"].ToString() != strPagina || Context.Session["Limpar"] != null)
            {
                SortOrder = null;
                CurrentProcuraItem1 = null;
                ProcuraItem1 = null;
                CurrentProcuraItem2 = null;
                ProcuraItem2 = null;
                NumeroPaginas = null;
                Page = null;
                Context.Session["SortOrder"] = null;
                Context.Session["CurrentProcuraItem1"] = null;
                Context.Session["ProcuraItem1"] = null;
                Context.Session["CurrentProcuraItem2"] = null;
                Context.Session["ProcuraItem2"] = null;
                //Context.Session["NumeroPaginas"] = null;
                Context.Session["Page"] = null;
                Context.Session["Pagina"] = strPagina;
                Context.Session["Limpar"] = null;
            }

            #endregion

            #region Procura 1

            if (String.IsNullOrEmpty(CurrentProcuraItem1))
            {
                if (CurrentProcuraItem1 != "")
                {
                    if (Context.Session["CurrentProcuraItem1"] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session["CurrentProcuraItem1"].ToString()))
                        {
                            CurrentProcuraItem1 = Context.Session["CurrentProcuraItem1"].ToString();
                        }
                    }
                }
                else
                {
                    CurrentProcuraItem1 = null;
                    Context.Session["CurrentProcuraItem1"] = null;
                }
            }
            else
            {
                Context.Session["CurrentProcuraItem1"] = CurrentProcuraItem1;
            }

            if (String.IsNullOrEmpty(ProcuraItem1))
            {
                if (ProcuraItem1 != "")
                {
                    if (Context.Session["ProcuraItem1"] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session["ProcuraItem1"].ToString()))
                        {
                            ProcuraItem1 = Context.Session["ProcuraItem1"].ToString();
                        }
                    }
                }
                else
                {
                    ProcuraItem1 = null;
                    Context.Session["ProcuraItem1"] = null;
                }
            }
            else
            {
                Context.Session["ProcuraItem1"] = ProcuraItem1;
                if (Page != 1)
                {
                    Page = 1;
                }
            }

            #endregion

            #region Procura 2

            if (String.IsNullOrEmpty(CurrentProcuraItem2))
            {
                if (CurrentProcuraItem2 != "")
                {
                    if (Context.Session["CurrentProcuraItem2"] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session["CurrentProcuraItem2"].ToString()))
                        {
                            CurrentProcuraItem2 = Context.Session["CurrentProcuraItem2"].ToString();
                        }
                    }
                }
                else
                {
                    CurrentProcuraItem2 = null;
                    Context.Session["CurrentProcuraItem2"] = null;
                }
            }
            else
            {
                Context.Session["CurrentProcuraItem2"] = CurrentProcuraItem2;
            }

            if (String.IsNullOrEmpty(ProcuraItem2))
            {
                if (ProcuraItem2 != "")
                {
                    if (Context.Session["ProcuraItem2"] != null)
                    {
                        if (!String.IsNullOrEmpty(Context.Session["ProcuraItem2"].ToString()))
                        {
                            ProcuraItem2 = Context.Session["ProcuraItem2"].ToString();
                        }
                    }
                }
                else
                {
                    ProcuraItem2 = null;
                    Context.Session["ProcuraItem2"] = null;
                }
            }
            else
            {
                Context.Session["ProcuraItem2"] = ProcuraItem2;
            }

            #endregion

            #region Pagina

            if (NumeroPaginas == null)
            {
                if (Context.Session["NumeroPaginas"] != null)
                {
                    NumeroPaginas = Convert.ToInt32(Context.Session["NumeroPaginas"]);
                }
                else
                {
                    NumeroPaginas = 5;
                    Context.Session["NumeroPaginas"] = 5;
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

        /// <summary>
        /// Percistencia dos parametros da pagina
        /// </summary>
        /// <param name="SortOrder"></param>
        /// <param name="CurrentProcuraItem1"></param>
        /// <param name="ProcuraItem1"></param>
        /// <param name="CurrentProcuraItem2"></param>
        /// <param name="ProcuraItem2"></param>
        /// <param name="NumeroPaginas"></param>
        /// <param name="Page"></param>
        public void Persistencia(ref string SortOrder, ref string CurrentProcuraItem1, ref string ProcuraItem1, ref int? NumeroPaginas, ref int? Page, string strPagina)
        {
            string CurrentProcuraItem2 = "";
            string ProcuraItem2 = "";
            Persistencia(ref SortOrder, ref CurrentProcuraItem1, ref ProcuraItem1, ref CurrentProcuraItem2, ref ProcuraItem2, ref NumeroPaginas, ref Page, strPagina);
        }

        public void LimparPersistencia()
        {
            Context.Session["Limpar"] = "true";
        }

        #endregion

    }

}