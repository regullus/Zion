namespace Sistema.Controllers
{

    #region Bibliotecas

    using Core.Entities;
    using Core.Helpers;
    using Core.Repositories.Financeiro;
    using Core.Repositories.Usuario;
    //using Fluentx;
    using Helpers;
    //using System.Collections;
    using System.Linq;

    using static Core.Entities.Conta;

    using Sistema.Constants;
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Threading;
    using System.Web.Mvc;
    using System.Collections;

    #endregion

    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ExtratoController : Controller
    {

        #region Variaveis

        private double cdblLatitude = 0;
        private double cdblLongitude = 0;
        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

        #endregion

        #region Core

        private UsuarioRepository usuarioRepository;
        //private Core.Repositories.Financeiro.LancamentoRepository lancamentoRepository;
        private CategoriaRepository categoriaRepository;
        private ContaRepository contaRepository;

        public ExtratoController(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            categoriaRepository = new CategoriaRepository(context);
            contaRepository = new ContaRepository(context);
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

        public ActionResult Index(int? idUsuario)
        {
            //2000    system
            //2001    syspag

            if(idUsuario == 2000)
            {
                ViewBag.Taxa = "Sistema";
            }
            if (idUsuario == 2001)
            {
                ViewBag.Taxa = "Taxa";
            }

            int usuarioID = idUsuario ?? 0;

            if (usuarioID == 0)
            {
                ViewBag.Contas = null;
                ViewBag.Contaslancamentos = null;
            }
            else
            {
                var contas = contaRepository.GetByAtiva();
                Usuario usuario = usuarioRepository.Get(usuarioID);

                ArrayList contasLancamentos = new ArrayList();
                var lancamentos = usuario.Lancamento.Where(l => l.ContaID == 7); //Transferencia
                contasLancamentos.Add(lancamentos);

                ViewBag.Contas = contas;
                ViewBag.Contaslancamentos = contasLancamentos;
            }
            return View();
        }

        #endregion
    }
}