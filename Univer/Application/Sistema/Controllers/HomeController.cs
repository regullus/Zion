namespace Sistema.Controllers
{

    #region Bibliotecas

    using Boilerplate.Web.Mvc;
    using Boilerplate.Web.Mvc.Filters;
    using Core.Helpers;
    using Core.Repositories.Financeiro;
    using Core.Repositories.Globalizacao;
    using Core.Repositories.Loja;
    using Core.Repositories.Rede;
    using Core.Repositories.Sistema;
    using Core.Repositories.Usuario;
    using Core.Services.Usuario;
    using cpUtilities;
    using Fluentx.Mvc;
    using Helpers;
    using Newtonsoft.Json;
    using Sistema.Constants;
    using Sistema.Services;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core.Entities;

    #endregion

    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class HomeController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis

        private double cdblLatitude = 0;
        private double cdblLongitude = 0;
        private readonly IBrowserConfigService browserConfigService;
        private readonly IFeedService feedService;
        private readonly IManifestService manifestService;
        private readonly IOpenSearchService openSearchService;
        private readonly IRobotsService robotsService;
        private readonly ISitemapService sitemapService;

        #endregion

        #region Core

        private AssociacaoRepository associacaoRepository;
        private ClassificacaoRepository classificacaoRepository;
        private CategoriaRepository categoriaRepository;
        private UsuarioRepository usuarioRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;
        private SincronizacaoRepository sincronizacaoRepository;
        private ProdutoRepository produtoRepository;
        private ProdutoValorRepository produtoValorRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private FilialRepository filialRepository;
        private PosicaoRepository posicaoRepository;
        private PontosBinarioRepository pontosBinarioRepository;
        private LancamentoRepository lancamentoRepository;
        private UsuarioService usuarioService;
        private QualificacaoRepository qualificacaoRepository;
        private BonificacaoRepository bonificacaoRepository;
        private ArquivoRepository arquivoRepository;
        private UsuarioDerramamentoLogRepository usuarioDerramamentoLogRepository;
        private UsuarioGanhoRepository usuarioGanhoRepository;
        private TabuleiroRepository tabuleiroRepository;

        public HomeController(DbContext context)
                 : base(context)
        {

            associacaoRepository = new AssociacaoRepository(context);
            classificacaoRepository = new ClassificacaoRepository(context);
            categoriaRepository = new CategoriaRepository(context);
            usuarioRepository = new UsuarioRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            sincronizacaoRepository = new SincronizacaoRepository(context);
            produtoRepository = new ProdutoRepository(context);
            produtoValorRepository = new ProdutoValorRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            filialRepository = new FilialRepository(context);
            posicaoRepository = new PosicaoRepository(context);
            pontosBinarioRepository = new PontosBinarioRepository(context);
            lancamentoRepository = new LancamentoRepository(context);
            usuarioService = new UsuarioService(context);
            arquivoRepository = new ArquivoRepository(context);
            qualificacaoRepository = new QualificacaoRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            usuarioDerramamentoLogRepository = new UsuarioDerramamentoLogRepository(context);
            usuarioGanhoRepository = new UsuarioGanhoRepository(context);
            tabuleiroRepository = new TabuleiroRepository(context);
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

        #endregion

        #region Actions

        [Route("", Name = HomeControllerRoute.GetIndex)]
        public ActionResult Index()
        {
            //bool blnEcommerce = false;
            string strErro = "";
            ViewBag.Background = null;

            try
            {
                #region Logado

                strErro = "IsAuthenticated";
                var redirectLogin = false;
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    Local.Log("Home", "Usuario logado: zero ou null");
                    redirectLogin = true;
                }

                //Administrador nao pode se logar no office
                if (usuario.ID < 1000)
                {
                    Local.Log("Home", "Usuario logado: Administrador");
                    redirectLogin = true;
                }

                if (!(User.IsInRole("perfilAdministrador") || User.IsInRole("Master") || User.IsInRole("perfilUsuario")))
                {
                    Local.Log("Home", "Usuario logado > 1000");
                    redirectLogin = true;
                }

                if (redirectLogin)
                {
                    return RedirectToAction("login", "Account", new
                    {
                        strPopupTitle = traducaoHelper["LOGIN_SESSAO_EXPIRADA_TITULO"],
                        strPopupMessage = traducaoHelper["LOGIN_SESSAO_EXPIRADA"],
                        Sair = "true"
                    });
                }

                #endregion

                #region Funcoes

                strErro = "Funcoes";
                //Verifica se a msg em popup para ser exibido na view
                obtemMensagem();

                #endregion

                #region ViewBags

                #endregion

            }
            catch (Exception ex)
            {
                return RedirectToAction("login", "Account", new { strPopupTitle = "Erro", strPopupMessage = "Erro em: " + strErro + " | " + ex.Message, Sair = "true" });
            }
            return View();
        }

        public ActionResult MediaTron()
        {
            //id da empresa Prospera (4) no MediaTron 
            string strIdEmpresa = "4";
            //URL do sistema mediatron
            string strURL = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_ADM_URL");
            //URL de Retorno, url desse sistema
            string strURLRetorno = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_REDIRECT");
            //Esse sistema utiliza o mediatron? (true, false)
            string strMediaTronAtivo = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_EM_USO");
            //Token para comunucacao entre os sistemas
            string strMediaTronToken = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_TOKEN");
            //Determina se o processamento deve continuar
            bool blnContinua = false;
            //Se o sistema utiliza o mediatron continua o processamento
            if (!String.IsNullOrEmpty(strMediaTronAtivo))
            {
                if (strMediaTronAtivo == "true")
                {
                    blnContinua = true;
                }
            }

            if (blnContinua)
            {
                //Criptografa dados para envio ao mediatron
                string strIdEmpresaGet = CriptografiaHelper.Morpho(strIdEmpresa, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strNome = CriptografiaHelper.Morpho(usuario.Nome, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strTelefone = CriptografiaHelper.Morpho(usuario.Telefone, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strCelular = CriptografiaHelper.Morpho(usuario.Celular, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strEmail = CriptografiaHelper.Morpho(usuario.Email, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strEndereco = CriptografiaHelper.Morpho("---", CriptografiaHelper.TipoCriptografia.Criptografa);
                string strBairro = CriptografiaHelper.Morpho("---", CriptografiaHelper.TipoCriptografia.Criptografa);
                string strCEP = CriptografiaHelper.Morpho("00000000", CriptografiaHelper.TipoCriptografia.Criptografa);
                string strLogin = CriptografiaHelper.Morpho(usuario.Login, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strSenha = CriptografiaHelper.Descriptografar(usuario.Senha);
                strSenha = CriptografiaHelper.Morpho(strSenha, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strKey = CriptografiaHelper.Morpho(usuario.ID.ToString(), CriptografiaHelper.TipoCriptografia.Criptografa);
                string strRedirect = CriptografiaHelper.Morpho(strURLRetorno, CriptografiaHelper.TipoCriptografia.Criptografa);
                strMediaTronToken = CriptografiaHelper.Morpho(strMediaTronToken + strIdEmpresa, CriptografiaHelper.TipoCriptografia.Criptografa);
                //Monta parametros para serem passados por get
                strURL += "?IdEmpresa=" + strIdEmpresaGet + "&" +
                   "Nome=" + strNome + "&" +
                   "Telefone=" + strTelefone + "&" +
                   "Celular=" + strCelular + "&" +
                   "Email=" + strEmail + "&" +
                   "Endereco=" + strEndereco + "&" +
                   "Bairro=" + strBairro + "&" +
                   "CEP=" + strCEP + "&" +
                   "Login=" + strLogin + "&" +
                   "Senha=" + strSenha + "&" +
                   "Key=" + strKey + "&" +
                   "Redirect=" + strRedirect + "&" +
                   "Token=" + strMediaTronToken;
                //envia dados ao mediatron
                return Redirect(strURL);

            }

            return View();
        }

        public ActionResult MediaTronRetorno(string retorno)
        {
            //Mediatron retorna caso ocorra problemas

            //Exibe mensagem de retorno do MediaTron
            Session["TituloMensagem"] = "Mediatron";
            Session["Mensagem"] = retorno;

            return RedirectToAction("index", "home");
        }

        public ActionResult EnviarValidacaoEmail()
        {
            try
            {
                usuarioService.EnviarValidacaoEmail(usuario, Helpers.Local.Sistema);
                string[] strMensagemParam1 = new string[] { traducaoHelper["EMAIL_ENVIADO_PARA"], usuario.Email };
                Mensagem(traducaoHelper["SUCESSO"], strMensagemParam1, "msg");

                //    Session["TituloMensagem"] = traducaoHelper["SUCESSO"];
                //    Session["Mensagem"] = traducaoHelper["EMAIL_ENVIADO_PARA"] + ": " + usuario.Email;
            }
            catch (Exception ex)
            {
                cpUtilities.LoggerHelper.WriteFile("erro envio de email " + usuario.Email + " : " + ex.Message, "HomeEnvioEmail");
                string[] strMensagemParam2 = new string[] { traducaoHelper["LOGIN_NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");

                //Session["TituloMensagem"] = traducaoHelper["ALERTA"];
                //Session["Mensagem"] = traducaoHelper["LOGIN_NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"] + ": " + usuario.Email;
            }

            return RedirectToAction("index", "home");
        }

        #endregion

        #region AboutContact

        [Route("about", Name = HomeControllerRoute.GetAbout)]
        public ActionResult About()
        {
            return this.View(HomeControllerAction.About);
        }

        [Route("contact", Name = HomeControllerRoute.GetContact)]
        public ActionResult Contact()
        {
            return this.View(HomeControllerAction.Contact);
        }

        #endregion

        #region security

        /// <summary>
        /// Gets the Atom 1.0 feed for the current site. Note that Atom 1.0 is used over RSS 2.0 because Atom 1.0 is a 
        /// newer and more well defined format. Atom 1.0 is a standard and RSS is not. See
        /// http://rehansaeed.com/building-rssatom-feeds-for-asp-net-mvc/
        /// </summary>
        /// <returns>The Atom 1.0 feed for the current site.</returns>
        [OutputCache(CacheProfile = CacheProfileName.Feed)]
        [Route("feed", Name = HomeControllerRoute.GetFeed)]
        public async Task<ActionResult> Feed()
        {
            // A CancellationToken signifying if the request is cancelled. See
            // http://www.davepaquette.com/archive/2015/07/19/cancelling-long-running-queries-in-asp-net-mvc-and-web-api.aspx
            CancellationToken cancellationToken = this.Response.ClientDisconnectedToken;
            return new AtomActionResult(await this.feedService.GetFeed(cancellationToken));
        }

        [Route("search", Name = HomeControllerRoute.GetSearch)]
        public ActionResult Search(string query)
        {
            // You can implement a proper search function here and add a Search.cshtml page.
            // return this.View(HomeControllerAction.Search);

            // Or you could use Google Custom Search (https://cse.google.co.uk/cse) to index your site and display your 
            // search results in your own page.

            // For simplicity we are just assuming your site is indexed on Google and redirecting to it.
            return this.Redirect(string.Format(
                "https://www.google.co.uk/?q=site:{0} {1}",
                this.Url.AbsoluteRouteUrl(HomeControllerRoute.GetIndex),
                query));
        }

        /// <summary>
        /// Gets the browserconfig XML for the current site. This allows you to customize the tile, when a user pins 
        /// the site to their Windows 8/10 start screen. See http://www.buildmypinnedsite.com and 
        /// https://msdn.microsoft.com/en-us/library/dn320426%28v=vs.85%29.aspx
        /// </summary>
        /// <returns>The browserconfig XML for the current site.</returns>
        [NoTrailingSlash]
        [OutputCache(CacheProfile = CacheProfileName.BrowserConfigXml)]
        [Route("browserconfig.xml", Name = HomeControllerRoute.GetBrowserConfigXml)]
        public ContentResult BrowserConfigXml()
        {
            Trace.WriteLine(string.Format(
                "browserconfig.xml requested. User Agent:<{0}>.",
                this.Request.Headers.Get("User-Agent")));
            string content = this.browserConfigService.GetBrowserConfigXml();
            return this.Content(content, ContentType.Xml, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the manifest JSON for the current site. This allows you to customize the icon and other browser 
        /// settings for Chrome/Android and FireFox (FireFox support is coming). See https://w3c.github.io/manifest/
        /// for the official W3C specification. See http://html5doctor.com/web-manifest-specification/ for more 
        /// information. See https://developer.chrome.com/multidevice/android/installtohomescreen for Chrome's 
        /// implementation.
        /// </summary>
        /// <returns>The manifest JSON for the current site.</returns>
        [NoTrailingSlash]
        [OutputCache(CacheProfile = CacheProfileName.ManifestJson)]
        [Route("manifest.json", Name = HomeControllerRoute.GetManifestJson)]
        public ContentResult ManifestJson()
        {
            Trace.WriteLine(string.Format(
                "manifest.jsonrequested. User Agent:<{0}>.",
                this.Request.Headers.Get("User-Agent")));
            string content = this.manifestService.GetManifestJson();
            return this.Content(content, ContentType.Json, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the Open Search XML for the current site. You can customize the contents of this XML here. The open 
        /// search action is cached for one day, adjust this time to whatever you require. See
        /// http://www.hanselman.com/blog/CommentView.aspx?guid=50cc95b1-c043-451f-9bc2-696dc564766d
        /// http://www.opensearch.org
        /// </summary>
        /// <returns>The Open Search XML for the current site.</returns>
        [NoTrailingSlash]
        [OutputCache(CacheProfile = CacheProfileName.OpenSearchXml)]
        [Route("opensearch.xml", Name = HomeControllerRoute.GetOpenSearchXml)]
        public ContentResult OpenSearchXml()
        {
            try
            {
                Trace.WriteLine(string.Format(
                "opensearch.xml requested. User Agent:<{0}>.",
                this.Request.Headers.Get("User-Agent")));
                string content = this.openSearchService.GetOpenSearchXml();
                return this.Content(content, ContentType.Xml, Encoding.UTF8);
            }
            catch (Exception)
            {
                return this.Content(null, ContentType.Xml, Encoding.UTF8);
            }

        }

        /// <summary>
        /// Tells search engines (or robots) how to index your site. 
        /// The reason for dynamically generating this code is to enable generation of the full absolute sitemap URL
        /// and also to give you added flexibility in case you want to disallow search engines from certain paths. The 
        /// sitemap is cached for one day, adjust this time to whatever you require. See
        /// http://rehansaeed.com/dynamically-generating-robots-txt-using-asp-net-mvc/
        /// </summary>
        /// <returns>The robots text for the current site.</returns>
        [NoTrailingSlash]
        [OutputCache(CacheProfile = CacheProfileName.RobotsText)]
        [Route("robots.txt", Name = HomeControllerRoute.GetRobotsText)]
        public ContentResult RobotsText()
        {
            Trace.WriteLine(string.Format(
                "robots.txt requested. User Agent:<{0}>.",
                this.Request.Headers.Get("User-Agent")));
            string content = this.robotsService.GetRobotsText();
            return this.Content(content, ContentType.Text, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the sitemap XML for the current site. You can customize the contents of this XML from the 
        /// <see cref="SitemapService"/>. The sitemap is cached for one day, adjust this time to whatever you require.
        /// http://www.sitemaps.org/protocol.html
        /// </summary>
        /// <param name="index">The index of the sitemap to retrieve. <c>null</c> if you want to retrieve the root 
        /// sitemap file, which may be a sitemap index file.</param>
        /// <returns>The sitemap XML for the current site.</returns>
        [NoTrailingSlash]
        [Route("sitemap.xml", Name = HomeControllerRoute.GetSitemapXml)]
        public ActionResult SitemapXml(int? index = null)
        {
            string content = this.sitemapService.GetSitemapXml(index);

            if (content == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sitemap index is out of range.");
            }

            return this.Content(content, ContentType.Xml, Encoding.UTF8);
        }

        #endregion

    }
}