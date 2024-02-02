namespace Sistema.Controllers
{

    #region Bibliotecas

    using Core.Entities;
    using Core.Helpers;
    using Core.Repositories.Usuario;
    using Helpers;
    using Sistema.Constants;
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Threading;
    using System.Web.Mvc;

    #endregion

    [Authorize(Roles = "Master, perfilAdministrador")]
    public class HomeController : Controller
    {

        #region Variaveis

        private double cdblLatitude = 0;
        private double cdblLongitude = 0;
        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

        #endregion

        #region Core

        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;

        public HomeController(DbContext context)
        {
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);

            Localizacao();
        }

        #endregion

        #region Mensagem

        public void Mensagem(string titulo, string[] mensagem, string tipo)
        {
            Session["strPoppup"] = tipo;
            Session["strMensagem"] = mensagem;
            Session["strTitulo"] = titulo;
        }

        public void obtemMensagem()
        {
            string strErr = "NOOK";
            string strAle = "NOOK";
            string strMsg = "NOOK";
            string strTipo = "";

            if (Session["strPoppup"] != null)
            {
                strTipo = Session["strPoppup"].ToString();
            }

            switch (strTipo)
            {
                case "err":
                    strErr = "OK";
                    break;
                case "ale":
                    strAle = "OK";
                    break;
                case "msg":
                    strMsg = "OK";
                    break;
            }

            ViewBag.PopupErr = strErr;
            ViewBag.PopupMsg = strMsg;
            ViewBag.PopupAlert = strAle;

            ViewBag.PopupTitle = Session["strTitulo"];
            ViewBag.PopupMessage = Session["strMensagem"];

            Session["strPoppup"] = "NOOK";
            Session["strTitulo"] = "";
            Session["strMensagem"] = "";
        }

        #endregion

        #region Helpers

        private string GetGeo()
        {
            string strIPAddress = string.Empty;
            string strVisitorCountry = string.Empty;

            try
            {
                strIPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (strIPAddress == "" || strIPAddress == null)
                {
                    strIPAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                //strIPAddress = "191.180.224.107";

                Tools.GetLocation.IVisitorsGeographicalLocation objLocation;
                objLocation = new Tools.GetLocation.ClsVisitorsGeographicalLocation();

                DataTable objDataTable = objLocation.GetLocation(strIPAddress);
                if (objDataTable != null)
                {
                    if (objDataTable.Rows.Count > 0)
                    {
                        string strLat = objDataTable.Rows[0]["Latitude"].ToString();
                        string strLon = objDataTable.Rows[0]["Longitude"].ToString();

                        cdblLatitude = Convert.ToDouble(strLat);
                        cdblLongitude = Convert.ToDouble(strLon);
                        strVisitorCountry = Convert.ToString(objDataTable.Rows[0]["Latitude"]) + "," + Convert.ToString(objDataTable.Rows[0]["Longitude"]);
                    }
                    else
                    {
                        strVisitorCountry = null;
                    }
                }
            }
            catch (Exception)
            {
                strVisitorCountry = "0.0.0.0";
            }
            return strVisitorCountry;
        }

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;
            if (idioma != null)
            {
                traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
                //string teste = traducaoHelper["ACESSO_AREA_RESTRITA"] + " - " + traducaoHelper["NOME_SITE"];
                ViewBag.TraducaoHelper = traducaoHelper;
                var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        #endregion

        #region Actions

        [Route("", Name = HomeControllerRoute.GetIndex)]
        [AllowAnonymous]
        public ActionResult Index(string login)
        {

            #region Funcoes
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            string strRegras = Local.Regra;
            #endregion

            DateTime Data = App.DateTimeZion;
            var redirectLogin = false;
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                Local.Log("Home", "Usuario logado: zero ou null");
                redirectLogin = true;
            }

            if (Local.idUsuario >= 1000)
            {
                Local.Log("Home", "Usuario logado > 1000");
                redirectLogin = true;
            }

            if (!User.IsInRole("perfilAdministrador") && !User.IsInRole("perfilMaster"))
            {
                Local.Log("Home", "Usuario logado > 1000");
                redirectLogin = true;
            }

            if (redirectLogin)
            {
                return RedirectToAction("login", "Account", new
                {
                    //strPopupTitle = traducaoHelper["LOGIN_SESSAO_EXPIRADA_TITULO"],
                    //strPopupMessage = traducaoHelper["LOGIN_SESSAO_EXPIRADA"],
                    Sair = "true"
                });
            }

            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("ADM_HOME_DASHBOARD_USUARIOS_ASSOCIACAO"))
            {
                var usuariosNivel = usuarioAssociacaoRepository.ObtemListaTotalUsuarioAssociacao();
                ViewBag.UsuariosNivel = Json(usuariosNivel);
            }
            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("ADM_HOME_DASHBOARD_EVOLUCAO_USUARIOS_ASSOCIADOS"))
            {
                var usuariosAssocDia = usuarioAssociacaoRepository.ObtemListaTotalUsuarioAssociadosDia(App.DateTimeZion.ToString("yyyyMMdd"));
                ViewBag.UsuariosAssocDia = Json(usuariosAssocDia);
                ViewBag.UsuariosAssocDiaPer = App.DateTimeZion.ToString("MMM/yyyy");
            }

            return View();
        }

        #endregion

    }
}