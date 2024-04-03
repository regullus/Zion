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
        private AvisoRepository avisoRepository;
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
            avisoRepository = new AvisoRepository(context);
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

                if (!ConfiguracaoHelper.GetBoolean("REDE_TABULEIRO"))
                {
                    ViewBag.RedeTabuleiro = false;

                    #region Ecommerce

                    //strErro = "Ecommerce";
                    ////Verifica se chamada vem do Ecommerce
                    //if (Session["ecPedido"] != null)
                    //{

                    //    #region Obtem Dados 
                    //    strErro = "Ecommerce - Obtem Dados";
                    //    blnEcommerce = true;
                    //    bool blnValido = true;
                    //    string strTokenRetorno = "";

                    //    //Obtem dados passados pelo ecommerce
                    //    string strPedido = Session["ecPedido"].ToString();
                    //    string strEmail = "";
                    //    string strUrlRetorno = "";
                    //    string strUrlRetornoLog = "";
                    //    string strValor = "";

                    //    Dictionary<string, object> objData = new Dictionary<string, object>();

                    //    if (Session["ecEmail"] != null)
                    //    {
                    //        strEmail = Session["ecEmail"].ToString();
                    //        //Verifica se email é valido
                    //        if (!cpUtilities.Gerais.ValidarEmail(strEmail))
                    //        {
                    //            //Email ok
                    //            //email invalido!
                    //            blnValido = false;
                    //            Local.Log("Home", "Ecommerce:    Email Invalido, pedido: " + strPedido);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //Sem email
                    //        Local.Log("Home", "Ecommerce:    Sem email, pedido: " + strPedido);
                    //        blnValido = false;
                    //    }

                    //    if (Session["ecUrlRetorno"] != null)
                    //    {
                    //        //URL OK
                    //        strUrlRetorno = Session["ecUrlRetorno"].ToString();
                    //        strUrlRetornoLog = strUrlRetorno;
                    //    }
                    //    else
                    //    {
                    //        //URL de retorno não foi passado!!!
                    //        Local.Log("Home", "Ecommerce:    Sem URL de Retorno, pedido: " + strPedido);
                    //        blnValido = false;
                    //    }

                    //    if (Session["ecValor"] != null)
                    //    {
                    //        strValor = Session["ecValor"].ToString();
                    //    }
                    //    else
                    //    {
                    //        //Valor não foi passado!!!
                    //        Local.Log("Home", "Ecommerce:    Sem valor, pedido: " + strPedido);
                    //        blnValido = false;
                    //    }

                    //    #endregion

                    //    #region Zera Variaveis
                    //    strErro = "Ecommerce -  Variaveis";
                    //    //Zera Sessions
                    //    Session["ecPedido"] = null;
                    //    Session["ecUrlRetorno"] = null;
                    //    Session["ecPedido"] = null;
                    //    Session["ecValor"] = null;
                    //    Session["ecEmail"] = null;
                    //    #endregion

                    //    if (blnValido)
                    //    {
                    //        strErro = "Ecommerce - TokenRetorno";
                    //        strTokenRetorno = cpUtilities.Encryption.CreateToken("ecChr@Wil_" + DateTime.UtcNow.ToString("yyyyMMdd") + "_" + strPedido + "_" + strEmail);

                    //        #region Verificacao
                    //        strErro = "Ecommerce -  Verificacao";
                    //        //Verifica se o usuario esta ativo
                    //        if (usuario.Status != Core.Entities.Usuario.TodosStatus.Associado)
                    //        {
                    //            Local.Log("Home", "Ecommerce:    Usuario não associado");
                    //            strUrlRetornoLog += "?ret=notassociated&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "notassociated");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }

                    //        var valor = double.Parse(Regex.Replace(strValor, @"[^\d]", ""));

                    //        if (valor <= 0)
                    //        {
                    //            Local.Log("Home", "Ecommerce:    Valor invalido, pedido: " + strPedido);
                    //            strUrlRetornoLog += "?ret=notvalue&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "notvalue");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }

                    //        //Verifica se o usuario possui saldo
                    //        if (usuarioRepository.Saldo(usuario.ID) < (valor / 100))
                    //        {
                    //            Local.Log("Home", "Ecommerce:    Sem Saldo, pedido: " + strPedido);
                    //            strUrlRetornoLog += "?ret=notfunds&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "notfunds");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //return Redirect(strUrlRetorno); 

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }

                    //        #endregion

                    //        #region Efetua Lancamento
                    //        strErro = "Ecommerce - Efetua Lancamento";
                    //        try
                    //        {
                    //            //Verifica se lancamento desse pedido já existe
                    //            string strDescricao = "eMail: " + strEmail + " - " + traducaoHelper["PEDIDO"] + " #" + strPedido;

                    //            var lancamentoExiste = lancamentoRepository.GetByDescricao(usuario.ID, strDescricao);
                    //            if (lancamentoExiste.Count() > 0)
                    //            {
                    //                //Pedido já existe no sistema
                    //                Local.Log("Home", "Ecommerce:    Sem Saldo, pedido: " + strPedido);
                    //                strUrlRetornoLog += "?ret=alreadyexists&id=" + strPedido + "&token=" + strTokenRetorno;
                    //                Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //                objData.Add("ret", "alreadyexists");
                    //                objData.Add("id", strPedido);
                    //                objData.Add("token", strTokenRetorno);

                    //                //return Redirect(strUrlRetorno); 

                    //                //Agora vai por post
                    //                return this.RedirectAndPost(strUrlRetorno, objData);
                    //            }

                    //            //ok dados corretos - efetua lancamento
                    //            var lancamento = new Core.Entities.Lancamento()
                    //            {
                    //                CategoriaID = 20, //CatagoraiID = 20 é eCommerce - Tabela Finaceiro.Categoria
                    //                ContaID = 1,
                    //                DataCriacao = App.DateTimeZion,
                    //                DataLancamento = App.DateTimeZion,
                    //                Descricao = strDescricao,
                    //                ReferenciaID = usuario.ID,
                    //                Tipo = Core.Entities.Lancamento.Tipos.Debito,
                    //                UsuarioID = usuario.ID,
                    //                Valor = -1 * (valor / 100),
                    //                MoedaIDCripto = (int)Core.Entities.Moeda.Moedas.NEN, //Nenhum
                    //            };
                    //            lancamentoRepository.Save(lancamento);
                    //            Local.Log("Home", "Ecommerce:    Lancado Pedido: [" + strPedido + "]");
                    //            strUrlRetornoLog += "?ret=ok&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "ok");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //return Redirect(strUrlRetorno); 

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Local.Log("Home", "Ecommerce:    Pedido: " + strPedido + " Erro: [" + ex.Message + "]");
                    //            strUrlRetornoLog += "?ret=error&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "error");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //return Redirect(strUrlRetorno); 

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }

                    //        #endregion

                    //    }
                    //    else
                    //    {
                    //        #region Dados invalidos
                    //        strErro = "Ecommerce - Dados invalidos";
                    //        //Retorno a url do ecommerce o erro ocorrido
                    //        if (String.IsNullOrEmpty(strUrlRetorno))
                    //        {
                    //            //Sem url de retorno
                    //            Local.Log("Home", "Ecommerce:    Sem URL de Retorno, pedido: " + strPedido);
                    //            return RedirectToAction("BadRequest", "Error");
                    //        }
                    //        else
                    //        {
                    //            if (String.IsNullOrEmpty(strPedido))
                    //            {
                    //                Local.Log("Home", "Ecommerce:    Dados invalidos");
                    //                strUrlRetornoLog += "?ret=invaliddata";
                    //                strPedido = "";
                    //            }
                    //            else
                    //            {
                    //                Local.Log("Home", "Ecommerce:    Dados invalidos, pedido: " + strPedido);
                    //                strUrlRetornoLog += "?ret=invaliddata&id=" + strPedido + "&token=" + strTokenRetorno;
                    //            }
                    //            Local.Log("Home", "Ecommerce:    Retorno: [" + strUrlRetornoLog + "]");

                    //            objData.Add("ret", "invaliddata");
                    //            objData.Add("id", strPedido);
                    //            objData.Add("token", strTokenRetorno);

                    //            //return Redirect(strUrlRetorno); 

                    //            //Agora vai por post
                    //            return this.RedirectAndPost(strUrlRetorno, objData);
                    //        }

                    //        #endregion
                    //    }
                    //}

                    //if (blnEcommerce)
                    //{
                    //    strErro = "Ecommerce - Erro inesperado";
                    //    //Não deve passar por aqui caso tenha dado tudo certo no ecommerce
                    //    Local.Log("Home", "Ecommerce:    Erro inesperado");
                    //    return RedirectToAction("BadRequest", "Error");
                    //}

                    #endregion

                    #region ViewBags

                    //Exibe ou não os dados do associado acima do menu
                    ViewBag.ExibeDadosAssociado = false;

                    strErro = "ViewBags";

                    strErro = "ViewBags - ExibeBanners";
                    ViewBag.ExibeBanners = ConfiguracaoHelper.GetBoolean("REDE_EXIBE_BANNER");
                    if (ViewBag.ExibeBanners)
                    {
                        strErro = "ViewBags - Banners";
                        ViewBag.Banners = arquivoRepository.GetBySecao((int)cpUtilities.TipoArquivoSecao.Banner).ToList();
                    }

                    strErro = "ViewBags - MENU_OFFICE_AVISO";
                    if (ConfiguracaoHelper.GetString("MENU_OFFICE_AVISO").ToLower() == "true")
                    {
                        strErro = "ViewBags - Avisos";
                        ViewBag.Avisos = avisoRepository.GetNaoLidosByUsuario(usuario.ID);
                    }

                    strErro = "ViewBags - REDE_CONTROLA_GANHO_ASSOCIACAO";
                    var usuarioGanho = usuarioGanhoRepository.GetAtual(usuario.ID);
                    ViewBag.AtingiuLimiteGanhoBonusArbitragem = false;
                    if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
                    {
                        if (usuarioGanho != null)
                        {
                            ViewBag.AtingiuLimiteGanhoBonusArbitragem = usuarioGanho.DataAtingiuLimite.HasValue;
                        }
                    }

                    strErro = "ViewBags - RedeBinaria";
                    ViewBag.RedeBinaria = (Core.Helpers.ConfiguracaoHelper.GetString("REDE_BINARIA") == "true");

                    strErro = "ViewBags - Dashboard";
                    ViewBag.DashboardDiretos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_DIRETOS") == "true");
                    ViewBag.DashboardAssociacao = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_ASSOCIACAO") == "true");
                    ViewBag.DashboardBonificacao = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_BONIFICACAO") == "true");
                    ViewBag.DashboardAtividadeMensal = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_ATIVIDADEMENSAL") == "true");
                    ViewBag.DashboardPontos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PONTOS") == "true");
                    ViewBag.DashboardPontosCiclo = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PONTOS_CICLO") == "true");
                    ViewBag.DashboardPontosCicloAnterior = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PONTOS_CICLO_ANTERIOR") == "true");
                    ViewBag.DashboardPontosMaximos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PONTOS_MAXIMOS") == "true");
                    ViewBag.DashboardSaldo = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_SALDO") == "true");
                    ViewBag.DashboardContatos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_CONTATOS_PENDENTES") == "true");
                    ViewBag.DashboardUsuarios = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_USUARIOS") == "true");
                    ViewBag.DashboardAtivos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_ATIVOS") == "true");
                    ViewBag.DashboardBinAcumulado = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_BINARIO_ACUMULADO") == "true");
                    ViewBag.DashboardBinDiario = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_BINARIO_DIARIO") == "true");
                    ViewBag.DashboardPedidosPendentesPgto = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PEDIDOS_PENDENTES_PGTO") == "true");
                    ViewBag.DashboardLayout_V2 = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_LAYOUT_V2") == "true");
                    ViewBag.DashboardTetoGanhosTotais = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_TETO_GANHOS_TOTAIS") == "true");
                    ViewBag.DashboardTetoGanhosAlavancagem = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_TETO_GANHOS_ALAVANCAGEM") == "true");
                    ViewBag.DashboardTetoGanhosBonusEquipeBinario = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_TETO_GANHOS_BONUS_EQUIPE_BINARIO") == "true");
                    ViewBag.DashboardGanhosTotaisBonificacao = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_GANHOS_TOTAIS_BONIFICACAO") == "true");

                    ViewBag.ExibeLadoDerramamento = (Core.Helpers.ConfiguracaoHelper.GetString("EXIBE_OU_NAO_LADO_DERRAMAMENTO") == "true");

                    strErro = "ViewBags - Associacoes";
                    ViewBag.Associacoes = associacaoRepository.GetAll();

                    strErro = "ViewBags - Derramamento";
                    if (ViewBag.ExibeLadoDerramamento)
                    {
                        //Exibe lado de derramamento seguindo a regra de que se o usuario possui filhos ativos, exibe se não, não exibe
                        ViewBag.ListaDerramamento = usuarioRepository.GetListaDerramamento(usuario, traducaoHelper);
                        bool blnExibeLadoDerramamento = usuarioRepository.VerificaDiretoAssociado(usuario.ID); ;
                        ViewBag.ExibeLadoDerramamento = blnExibeLadoDerramamento;
                        ViewBag.ListaDerramamento = usuarioRepository.GetListaDerramamento(usuario, traducaoHelper);
                    }

                    strErro = "ViewBags - DataMigracao";
                    ViewBag.MigrouNovaRede = usuario.DataMigracao.HasValue;
                    if (!ConfiguracaoHelper.GetBoolean("EXIGI_MIGRACAO"))
                    {
                        ViewBag.MigrouNovaRede = true;
                    }

                    #endregion

                    #region Saldo

                    strErro = "Obtem Saldo";

                    Double saldo = 0;
                    Double saldoGanhosDiarios = 0;
                    Double saldoGanhosComissoes = 0;
                    Double acumuladoGanhosDiarios = 0;
                    Double acumuladoGanhosUnilevel = 0;
                    Double acumuladoGanhosGlobais = 0;

                    if (usuario.Lancamento.Count > 0)
                    {
                        //Entram no saldo: 1 - Bonus de Rendimento; 2 - Bonus de Equipe; 
                        //Não entram: 9 - BitCoin; 10 - LiteCoin, 14 - Saldo e 7 - Transferencias; 8 - Valor Investido 
                        saldo = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade || l.ContaID == (int)Conta.Contas.Bonus).Sum(l => l.Valor) ?? 0;
                        saldoGanhosDiarios = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade).Sum(l => l.Valor) ?? 0;
                        saldoGanhosComissoes = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Bonus).Sum(l => l.Valor) ?? 0;
                    }

                    if (usuario.UsuarioGanho.Count > 0)
                    {
                        if (usuario.UsuarioGanho.Count(u => u.Indicador == 11) > 0)
                        {
                            acumuladoGanhosDiarios = usuario.UsuarioGanho.First(u => u.Indicador == 11).AcumuladoGanho ?? 0;
                        }
                        if (usuario.UsuarioGanho.Count(u => u.Indicador == 12) > 0)
                        {
                            acumuladoGanhosUnilevel = usuario.UsuarioGanho.First(u => u.Indicador == 12).AcumuladoGanho ?? 0;
                        }
                        if (usuario.UsuarioGanho.Count(u => u.Indicador == 13) > 0)
                        {
                            acumuladoGanhosGlobais = usuario.UsuarioGanho.First(u => u.Indicador == 13).AcumuladoGanho ?? 0;
                        }
                    }

                    ViewBag.SaldoTotalDisponivel = saldo;
                    ViewBag.SaldoGanhosDiarios = saldoGanhosDiarios;
                    ViewBag.SaldoGanhosComissoes = saldoGanhosComissoes;

                    ViewBag.AcumuladoGanhosDiarios = acumuladoGanhosDiarios;
                    ViewBag.AcumuladoGanhosUnilevel = acumuladoGanhosUnilevel;
                    ViewBag.AcumuladoGanhosGlobais = acumuladoGanhosGlobais;
                    ViewBag.AcumuladoTotal = acumuladoGanhosDiarios + acumuladoGanhosUnilevel + acumuladoGanhosGlobais;

                    #endregion

                    #region Usuario

                    strErro = "Usuario";
                    strErro = "Usuario - ViewBags Classificacoes";
                    ViewBag.Classificacoes = classificacaoRepository.GetAll();
                    strErro = "Usuario - ViewBags Categorias";
                    ViewBag.Categorias = categoriaRepository.GetByTipo(Core.Entities.Lancamento.Tipos.Bonificacao);
                    strErro = "Usuario - ViewBags Filiais";
                    ViewBag.Filiais = filialRepository.GetAll();
                    strErro = "Usuario - ViewBags AtivoMensal";
                    ViewBag.AtivoMensal = usuarioRepository.GetAtivoMensal(usuario.ID);
                    strErro = "Usuario - ViewBags DataValidade";
                    ViewBag.DataValidade = (usuario.DataValidade.HasValue ? usuario.DataValidade.Value.Day.ToString("00") + "/" + usuario.DataValidade.Value.Month.ToString("00") + "/" + usuario.DataValidade.Value.Year.ToString("00") : " - ");
                    double pontosAcumulados = Convert.ToDouble(usuarioRepository.GetPontos(usuario.ID));
                    string classificacaoUsuario = "";
                    double porcentagemClassificacao = 0;
                    string classificacaoValor = "";
                    if (pontosAcumulados > 0 && pontosAcumulados <= (int)Classificacao.Tipo.Broker)
                    {
                        //string classificacaoUsuario = Enumerations.GetEnumDescription((Classificacao.Tipo)50);
                        classificacaoUsuario = "Broker";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.Broker * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.Broker).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.Broker && pontosAcumulados <= (int)Classificacao.Tipo.DiretorBronze)
                    {
                        classificacaoUsuario = "Diretor Bronze";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.DiretorBronze * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorBronze).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.DiretorBronze && pontosAcumulados <= (int)Classificacao.Tipo.DiretorSilver)
                    {
                        classificacaoUsuario = "Diretor Silver";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.DiretorSilver * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorSilver).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.DiretorSilver && pontosAcumulados <= (int)Classificacao.Tipo.DiretorGold)
                    {
                        classificacaoUsuario = "Diretor Gold";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.DiretorGold * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorGold).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.DiretorGold && pontosAcumulados <= (int)Classificacao.Tipo.DiretorPlatinum)
                    {
                        classificacaoUsuario = "Diretor Platinum";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.DiretorPlatinum * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorPlatinum).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.DiretorPlatinum && pontosAcumulados <= (int)Classificacao.Tipo.DiretorDiamante)
                    {
                        classificacaoUsuario = "Diretor Diamante";
                        porcentagemClassificacao = pontosAcumulados / (int)Classificacao.Tipo.DiretorDiamante * 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorDiamante).ToString();
                    }
                    else if (pontosAcumulados > (int)Classificacao.Tipo.DiretorDiamante)
                    {
                        classificacaoUsuario = "Presidente";
                        porcentagemClassificacao = 100;
                        classificacaoValor = ((int)Classificacao.Tipo.DiretorDiamante).ToString();
                    }

                    ViewBag.pontosAcumulados = pontosAcumulados;
                    ViewBag.classificacaoUsuario = classificacaoUsuario;
                    ViewBag.porcentagemClassificacao = porcentagemClassificacao + "%";
                    ViewBag.classificacaoValor = classificacaoValor;
                    strErro = "Usuario - Variaveis";
                    var statusAssociado = Core.Entities.Usuario.TodosStatus.Associado.GetHashCode();
                    //var usuarioEsquerda = usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura + "0").FirstOrDefault();
                    //var usuarioDireta = usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura + "1").FirstOrDefault();
                    //var idUsuarioEsquerda = usuarioEsquerda != null ? usuarioEsquerda.ID : 0;
                    //var idUsuarioDireita = usuarioDireta != null ? usuarioDireta.ID : 0;

                    int qtdeEsquerdo = 0;
                    int qtdeDireito = 0;
                    int qtdeTotal = 0;
                    if (ViewBag.RedeBinaria)
                    {
                        var usuarioIDsEsquerda = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
                        var usuarioIDsDireita = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
                        qtdeEsquerdo = usuarioIDsEsquerda.Count();
                        qtdeDireito = usuarioIDsDireita.Count();
                        qtdeTotal = qtdeEsquerdo + qtdeDireito;

                        strErro = "Usuario - ViewBags Rede Binaria";
                        ViewBag.RedeEsquerdo = qtdeEsquerdo;
                        ViewBag.RedeDireito = qtdeDireito;
                        ViewBag.RedeTotal = qtdeTotal;
                    }
                    else
                    {
                        var usuarioIDsColuna0 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
                        var usuarioIDsColuna1 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
                        var usuarioIDsColuna2 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "2") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
                        var qtdeColuna0 = usuarioIDsColuna0.Count();
                        var qtdeColuna1 = usuarioIDsColuna1.Count();
                        var qtdeColuna2 = usuarioIDsColuna2.Count();
                        qtdeTotal = qtdeColuna0 + qtdeColuna1 + qtdeColuna2;

                        strErro = "Usuario - ViewBags Rede";
                        ViewBag.RedeColuna0 = qtdeColuna0;
                        ViewBag.RedeColuna1 = qtdeColuna1;
                        ViewBag.RedeColuna2 = qtdeColuna2;
                        ViewBag.RedeTotal = qtdeTotal;
                    }

                    strErro = "Plano Atual - ViewBag Icone Produto";
                    var produtoUsuario = produtoRepository.GetByExpression(x => x.NivelAssociacao == usuario.NivelAssociacao).FirstOrDefault();

                    if (produtoUsuario != null)
                        ViewBag.IconeProduto = "~/Arquivos/produtos/" + produtoUsuario.SKU + "/" + produtoUsuario.SKU + ".png";
                    else
                        ViewBag.IconeProduto = string.Empty;

                    #endregion

                    #region Quantidades

                    strErro = "Quantidades";
                    ViewBag.QtdeDiretos = 0;
                    if (ViewBag.DashboardDiretos)
                    {
                        if (!string.IsNullOrEmpty(usuario.Assinatura))
                        {
                            var qtdeDiretos = usuarioRepository.GetByExpression(r => r.Assinatura.Contains(usuario.Assinatura) && r.StatusID == 2).OrderBy(r => r.Assinatura.Length).Count();
                            ViewBag.QtdeDiretos = qtdeDiretos;
                        }
                    }

                    //strErro = "Quantidades - Pontuacao";
                    //double _PONTUACAO_NIVEL_1 = produtoRepository.Bonificacao("Kit Promocional");
                    //double _PONTUACAO_NIVEL_2 = produtoRepository.Bonificacao("Kit Adesão 1");
                    //double _PONTUACAO_NIVEL_3 = produtoRepository.Bonificacao("Kit Adesão 2");
                    //double _PONTUACAO_NIVEL_4 = produtoRepository.Bonificacao("Kit Adesão 3");
                    //double _PONTUACAO_NIVEL_5 = produtoRepository.Bonificacao("");

                    //strErro = "Quantidades - Categorias";
                    //var hoje = new DateTime(App.DateTimeZion.Year, App.DateTimeZion.Month, App.DateTimeZion.Day, 0, 0, 0, 0);
                    //var bronzeEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 2 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
                    //var bronzeDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 2 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
                    //var silverEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 3 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
                    //var silverDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 3 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
                    //var goldEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 4 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
                    //var goldDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 4 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
                    //var goldPlusEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 5 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
                    //var goldPlusDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 5 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();

                    //strErro = "Quantidades - Pontuacao Lado";
                    //ViewBag.PontuacaoEsquerdoHoje = (bronzeEsquerdaHoje * _PONTUACAO_NIVEL_2) + (silverEsquerdaHoje * _PONTUACAO_NIVEL_3) + (goldEsquerdaHoje * _PONTUACAO_NIVEL_4) + (goldPlusEsquerdaHoje * _PONTUACAO_NIVEL_5);
                    //ViewBag.PontuacaoDireitoHoje = (bronzeDireitaHoje * _PONTUACAO_NIVEL_2) + (silverDireitaHoje * _PONTUACAO_NIVEL_3) + (goldDireitaHoje * _PONTUACAO_NIVEL_4) + (goldPlusDireitaHoje * _PONTUACAO_NIVEL_5);

                    strErro = "Quantidades - Pagamentos";
                    var statusPago = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago;
                    var pagamentos = pedidoPagamentoRepository.GetByExpression(p => p.Pedido.Usuario.GeraBonus == true && p.Pedido.Usuario.RecebeBonus == true && p.PedidoPagamentoStatus.FirstOrDefault(s => s.StatusID == statusPago) != null && p.Pedido.Usuario.Assinatura.StartsWith(usuario.Assinatura));
                    double acumuladoBonusEsquerda = 0;

                    #endregion

                    #region Atividade Mensal

                    strErro = "Atividade Mensal";
                    if (usuario.NivelAssociacao == 0)
                    {
                        strErro = "Atividade Mensal - ViewBag";
                        ViewBag.ValorPlano = 0;
                        ViewBag.AtividadeMensalFalta = 0;
                        ViewBag.AtividadeMensalAtivo = false;
                    }
                    else
                    {
                        strErro = "Atividade Mensal - Produto";
                        var produto = produtoRepository.GetByExpression(prd => prd.NivelAssociacao == usuario.NivelAssociacao && prd.TipoID == 3 && prd.Composto).FirstOrDefault();
                        if (produto != null)
                        {
                            strErro = "Atividade Mensal - Produto not null";
                            var produtoValor = produtoValorRepository.GetByExpression(PrdVal => PrdVal.ProdutoID == produto.ID).FirstOrDefault();
                            ViewBag.ValorPlano = (produtoValor.Valor.HasValue ? produtoValor.Valor.Value.ToString("#.##").Replace(",", ".") : "0");
                            ViewBag.AtividadeMensalFalta = ViewBag.ValorPlano;
                            ViewBag.AtividadeMensalAtivo = (usuario.DataValidade.HasValue ? (usuario.DataValidade.Value >= App.DateTimeZion ? true : false) : false);
                        }
                        else
                        {
                            if (usuario.DataValidade != null && usuario.DataValidade >= App.DateTimeZion)
                            {
                                ViewBag.ValorPlano = 0;
                                ViewBag.AtividadeMensalFalta = 0;
                                ViewBag.AtividadeMensalAtivo = true;
                            }
                            else
                            {
                                strErro = "Atividade Mensal - Produto null";
                                ViewBag.ValorPlano = 0;
                                ViewBag.AtividadeMensalFalta = 0;
                                ViewBag.AtividadeMensalAtivo = false;
                            }
                        }
                    }

                    #endregion

                    #region Pontuacao

                    ViewBag.Pontos = 0.0;
                    ViewBag.PontosMax = 0.0;
                    ViewBag.PontosTotal = 0.0;
                    if (Core.Helpers.ConfiguracaoHelper.GetBoolean("REDE_BINARIA"))
                    {
                        var qtdeEsquerdo1 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 1).Count();
                        var qtdeDireito1 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 1).Count();

                        var qtdeEsquerdo2 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 2).Count();
                        var qtdeDireito2 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 2).Count();

                        var qtdeEsquerdo3 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 3).Count();
                        var qtdeDireito3 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 3).Count();

                        var qtdeEsquerdo4 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 4).Count();
                        var qtdeDireito4 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 4).Count();

                        var qtdeEsquerdo5 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 5).Count();
                        var qtdeDireito5 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 5).Count();

                        strErro = "Quantidades - try";
                        try //Para caso de null - valor fica zero
                        {
                            //Deixou de calcular para pegar campos [AcumuladoEsquerda] e [AcumuladoDireita] da tabela [Rede].[Posicao]
                            //acumuladoBonusEsquerda = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "0")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                            acumuladoBonusEsquerda = posicaoRepository.AcumuladoEsquerda(usuario.ID) + pontosBinarioRepository.AcumuladoEsquerda(usuario.ID);

                        }
                        catch (Exception)
                        {
                            acumuladoBonusEsquerda = 0;
                        }

                        strErro = "Quantidades - Acumulado";
                        double acumuladoBonusDireita = 0;
                        strErro = "Quantidades - Acumulado try";
                        try //Para caso de null - valor fica zero
                        {
                            //Deixou de calcular para pegar campos [AcumuladoEsquerda] e [AcumuladoDireita] da tabela [Rede].[Posicao]
                            //acumuladoBonusDireita = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "1")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                            acumuladoBonusDireita = posicaoRepository.AcumuladoDireita(usuario.ID) + pontosBinarioRepository.AcumuladoDireita(usuario.ID);
                        }
                        catch (Exception)
                        {
                            acumuladoBonusDireita = 0;
                        }

                        strErro = "Pontuacao -  ViewBag";
                        ViewBag.Pontos = posicaoRepository.ObtemPontuacao(usuario.ID);
                        if (ViewBag.Pontos < 0)
                            ViewBag.PontosLado = traducaoHelper["ESQUERDA"];
                        else
                            ViewBag.PontosLado = traducaoHelper["DIREITA"];

                        strErro = "Pontuacao -  ViewBag Pontos";
                        ViewBag.Pontos = Math.Abs(ViewBag.Pontos);

                        ViewBag.PontuacaoEsquerdo = acumuladoBonusEsquerda;
                        ViewBag.PontuacaoDireito = acumuladoBonusDireita;

                        strErro = "Pontuacao -  ViewBag Rede";
                        ViewBag.RedeEsquerdo = qtdeEsquerdo;
                        ViewBag.RedeDireito = qtdeDireito;
                        ViewBag.RedeTotal = qtdeTotal;

                        ViewBag.RedeEsquerdo1 = qtdeEsquerdo1;
                        ViewBag.RedeDireito1 = qtdeDireito1;
                        ViewBag.RedeTotal1 = qtdeEsquerdo1 + qtdeDireito1;

                        ViewBag.RedeEsquerdo2 = qtdeEsquerdo2;
                        ViewBag.RedeDireito2 = qtdeDireito2;
                        ViewBag.RedeTotal2 = qtdeEsquerdo2 + qtdeDireito2;

                        ViewBag.RedeEsquerdo3 = qtdeEsquerdo3;
                        ViewBag.RedeDireito3 = qtdeDireito3;
                        ViewBag.RedeTotal3 = qtdeEsquerdo3 + qtdeDireito3;

                        ViewBag.RedeEsquerdo4 = qtdeEsquerdo4;
                        ViewBag.RedeDireito4 = qtdeDireito4;
                        ViewBag.RedeTotal4 = qtdeEsquerdo4 + qtdeDireito4;

                        ViewBag.RedeEsquerdo5 = qtdeEsquerdo5;
                        ViewBag.RedeDireito5 = qtdeDireito5;
                        ViewBag.RedeTotal5 = qtdeEsquerdo5 + qtdeDireito5;
                    }
            
                    #endregion

                    #region Pedidos Pendente de Pagamento
                    strErro = "Pedidos Pendente -  ViewBag";
                    if (ViewBag.DashboardPedidosPendentesPgto)
                    {
                        ViewBag.QtdePedidosPendentesPgto = pedidoPagamentoRepository.ObtemPedidosPendentesPgto(usuario.ID);
                    }

                    #endregion

                    #region PlanoCarreira

                    strErro = "PlanoCarreira -  ViewBag";
                    if (ViewBag.DashboardPontos || ViewBag.DashboardPontosCiclo)
                    {
                        strErro = "PlanoCarreira -  ViewBag Dashboard";
                        ViewBag.PlanoCarreiraNome = usuarioService.PlanoCarreira(usuario.ID, ViewBag.Pontos);
                        decimal percentagem = usuarioService.PlanoCarreiraPercentagem(usuario.ID, ViewBag.Pontos);
                        ViewBag.PlanoCarreiraPercentagem = Math.Round(percentagem * 100, 2);
                        ViewBag.PlanoCarreiraNivel = usuarioService.PlanoCarreiraNivel(usuario.ID, ViewBag.Pontos);
                    }

                    if (ViewBag.DashboardPontosCicloAnterior)
                    {
                        strErro = "PlanoCarreiraAnter -  ViewBag Dashboard";
                        ViewBag.PlanoCarreiraNomeAnter = usuarioService.PlanoCarreira(usuario.ID, ViewBag.PontosMax);
                        decimal percentagemAnter = usuarioService.PlanoCarreiraPercentagem(usuario.ID, ViewBag.PontosMax);
                        ViewBag.PlanoCarreiraPercentagemAnter = Math.Round(percentagemAnter * 100, 2);
                        ViewBag.PlanoCarreiraNivelAnter = usuarioService.PlanoCarreiraNivel(usuario.ID, ViewBag.PontosMax);
                    }
                    #endregion

                    #region Diario/Arbitragem
                    strErro = "Diario/Arbitragem -  ViewBag";

                    ViewBag.BolqueioRenovacao = "false";
                    double somaBonificacoes = 0;
                    double? somaLancamentos = 0;
                    if (usuarioGanho != null)
                    {
                        somaBonificacoes = usuarioGanho.AcumuladoGanho.Value;
                        somaLancamentos = lancamentoRepository.GetByExpression(x => x.UsuarioID == usuario.ID &&
                                                                                        x.Categoria.TipoID == 4 &&
                                                                                        x.DataLancamento >= usuarioGanho.DataInicio &&
                                                                                        x.DataLancamento <= usuarioGanho.DataFim).Sum(s => s.ValorCripto);
                        somaLancamentos = somaLancamentos != null ? somaLancamentos : 0; // (Math.Truncate(somaLancamentos.Value * 1000) / 1000) : 0;

                        ViewBag.BolqueioRenovacao = usuarioGanho.DataAtingiuLimite.HasValue ? "true" : "false";
                    }

                    double valorMigracao = 0;
                    if (usuario.Complemento != null && usuarioGanho != null && usuarioGanho.Indicador == 0)
                    {
                        valorMigracao = usuario.Complemento.MaximoGanhos.HasValue ? double.Parse(usuario.Complemento.MaximoGanhos.Value.ToString()) : 0;

                        //somaBonificacoes = somaBonificacoes + (usuario.Complemento.GanhoAtual.HasValue ? double.Parse(usuario.Complemento.GanhoAtual.Value.ToString()) : 0);
                    }

                    var teto = somaLancamentos * ConfiguracaoHelper.GetInt("FATOR_MULTIPLICADOR_TETO") + valorMigracao;
                    teto = (Math.Truncate(teto.Value * 10000) / 10000); // -- Trunca para resolver o problemas dos pedidos com 5 casas decimais

                    if (teto > 0)
                    {
                        ViewBag.PercentualBonusDiarioArbitragem = Math.Round((somaBonificacoes * 100) / teto.Value, 2);
                        ViewBag.PercentualBarra = Math.Round((somaBonificacoes * 100) / teto.Value, 0);
                        ViewBag.SomaBonificacoes = Math.Round(somaBonificacoes, 4, MidpointRounding.ToEven);
                        teto = teto.Value / 2;
                        var tetoStr = teto.Value.ToString();
                        if (tetoStr.Length >= 6)
                            ViewBag.Ganhos = tetoStr.Substring(0, 6);
                        else
                            ViewBag.Ganhos = tetoStr;

                        //ViewBag.Ganhos = Math.Round((teto.Value / 2), 4, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        ViewBag.PercentualBonusDiarioArbitragem = 0;
                        ViewBag.PercentualBarra = 0;
                        ViewBag.SomaBonificacoes = somaBonificacoes;
                        ViewBag.Ganhos = 0;
                    }

                    #endregion

                    #region Info Usuario 

                    strErro = "Info Usuario -  ViewBag";

                    var pontosUsuarios = posicaoRepository.ObtemPontuacao(usuario.ID);
                    double pontosMax = 0;
                    //Obtem Ponto maximo da classificacao atual do usuario
                    var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos >= pontosUsuarios).OrderBy(x => x.Pontos).FirstOrDefault();

                    if (classificacao != null)
                    {
                        pontosMax = (double)classificacao.Pontos;
                    }

                    ViewBag.PontosRestantesInfoUsuario = pontosMax - pontosUsuarios;

                    var qualificacaoUsuario = qualificacaoRepository.GetByExpression(x => x.UsuarioID == usuario.ID);

                    if (qualificacaoUsuario.Count() > 0)
                    {
                        var isQualifyBinary = qualificacaoUsuario.OrderBy(x => x.DataQualificacao).FirstOrDefault().DataQualificacao;
                        ViewBag.isQualificacaoBinario = isQualifyBinary != null ? true : false;
                    }

                    #endregion

                    #region BonusBinario

                    strErro = "Bonus -  ViewBag Dashboard";

                    if (ViewBag.DashboardBinDiario)
                    {
                        var posicao = posicaoRepository.GetByExpression(x => x.UsuarioID == usuario.ID).OrderByDescending(x => x.DataInicio).Take(5);
                        ViewBag.BinarioData = posicao;
                    }

                    #endregion

                    #region Bonificacoes

                    strErro = "Bonificacoes";

                    if (ViewBag.DashboardBonificacao)
                    {
                        strErro = "Bonificacoes - ViewBag";
                        List<Core.Entities.Categoria> listBonificacoes = categoriaRepository.GetByTipo(Core.Entities.Lancamento.Tipos.Bonificacao).ToList();
                        ViewBag.Bonificacoes = listBonificacoes;
                        strErro = "Bonificacoes - ViewBag Lista";
                        if (listBonificacoes.Count() > 0)
                        {
                            int ID_Bonus_01 = ViewBag.Bonificacoes[0].ID;
                            var bonus_01 = lancamentoRepository.GetByExpression(lan => lan.UsuarioID == usuario.ID && lan.CategoriaID == ID_Bonus_01).Sum(lan => lan.Valor);
                            ViewBag.Bonus_01 = (bonus_01.HasValue ? bonus_01.Value.ToString("#.##").Replace(",", ".") : "0");
                        }
                        strErro = "Bonificacoes - ViewBag > 1";
                        if (listBonificacoes.Count() > 1)
                        {
                            int ID_Bonus_02 = ViewBag.Bonificacoes[1].ID;
                            var bonus_02 = lancamentoRepository.GetByExpression(lan => lan.UsuarioID == usuario.ID && lan.CategoriaID == ID_Bonus_02).Sum(lan => lan.Valor);
                            ViewBag.Bonus_02 = (bonus_02.HasValue ? bonus_02.Value.ToString("#.##").Replace(",", ".") : "0");
                        }
                    }
                    else
                    {
                        ViewBag.Bonus_01 = "0";
                        ViewBag.Bonus_02 = "0";
                    }
                    #endregion

                    #region Titulo

                    strErro = "Titulo";
                    if (Session["TituloMensagem"] != null)
                    {
                        strErro = "TituloMensagem";
                        ViewBag.TituloMensagem = Session["TituloMensagem"];
                        ViewBag.Mensagem = Session["Mensagem"];
                        Session["TituloMensagem"] = null;
                        Session["Mensagem"] = null;
                    }

                    #endregion

                    #region Teto Ganhos

                    ViewBag.GanhosTotaisBonificacao = lancamentoRepository.GetByExpression(lan => lan.UsuarioID == usuario.ID && lan.TipoID == 6).Sum(lan => lan.Valor);
                    if (ViewBag.GanhosTotaisBonificacao == null)
                    {
                        ViewBag.GanhosTotaisBonificacao = 0d;
                    }

                    if (ViewBag.DashboardTetoGanhosTotais)
                        ViewBag.TetoGanhosTotais = Math.Round(posicaoRepository.ObtemBonusTetoGanhoTotal(usuario.ID), 2);

                    if (ViewBag.DashboardTetoGanhosAlavancagem)
                        ViewBag.TetoGanhosAlavancagem = Math.Round(posicaoRepository.ObtemBonusTetoGanhoAlavancagem(usuario.ID), 2);

                    if (ViewBag.DashboardTetoGanhosBonusEquipeBinario)
                    {
                        var categoriaBonusBinarioID = categoriaRepository.GetByExpression(cat => cat.Nome.Equals("Bonus Equipe")).First().ID;
                        var dataMenor = App.DateTimeZion.Date;
                        var dataMaior = App.DateTimeZion.AddDays(1).Date;
                        var bonusBinario = lancamentoRepository.GetByExpression(lan => lan.UsuarioID == usuario.ID && lan.CategoriaID == categoriaBonusBinarioID && (lan.DataLancamento >= dataMenor && lan.DataLancamento <= dataMaior)).Sum(lst => lst.Valor);
                        ViewBag.GanhosBonusEquipeBinario = bonusBinario ?? 0;

                        ViewBag.TetoGanhosBonusEquipeBinario = Math.Round(posicaoRepository.ObtemBonusTetoGanhoBinario(usuario.ID).Value, 2);
                    }

                    #endregion

                    #region Liderança

                    ViewBag.IsLideranca = usuario.Complemento != null ? usuario.Complemento.IsLideranca : false;

                    #endregion
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("login", "Account", new { strPopupTitle = "Erro", strPopupMessage = "Erro em: " + strErro + " | " + ex.Message, Sair = "true" });
            }
            return View();
        }

        public ActionResult Dashboar()
        {
            #region Funcoes
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();
            #endregion

            ViewBag.RedeBinaria = (Core.Helpers.ConfiguracaoHelper.GetString("REDE_BINARIA") == "true");

            ViewBag.DashboardPontos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_PONTOS") == "true");
            ViewBag.DashboardSaldo = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_SALDO") == "true");
            ViewBag.DashboardContatos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_CONTATOS_PENDENTES") == "true");
            ViewBag.DashboardUsuarios = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_USUARIOS") == "true");
            ViewBag.DashboardAtivos = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_ATIVOS") == "true");
            ViewBag.DashboardBinAcumulado = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_BINARIO_ACUMULADO") == "true");
            ViewBag.DashboardBinDiario = (Core.Helpers.ConfiguracaoHelper.GetString("HOME_DASHBOARD_BINARIO_DIARIO") == "true");

            //ViewBag.Associacoes = associacaoRepository.GetAll();
            ViewBag.Classificacoes = classificacaoRepository.GetAll();
            ViewBag.Categorias = categoriaRepository.GetByTipo(Core.Entities.Lancamento.Tipos.Bonificacao);
            // AdamastorVer  ViewBag.Filiais = filialRepository.GetAll();
            ViewBag.AtivoMensal = usuarioRepository.GetAtivoMensal(usuario.ID);

            #region Usuario

            var statusAssociado = Core.Entities.Usuario.TodosStatus.Associado.GetHashCode();
            var usuarioEsquerda = usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura + "0").FirstOrDefault();
            var usuarioDireta = usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura + "1").FirstOrDefault();
            var idUsuarioEsquerda = usuarioEsquerda != null ? usuarioEsquerda.ID : 0;
            var idUsuarioDireita = usuarioDireta != null ? usuarioDireta.ID : 0;

            var usuarioIDsEsquerda = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
            var usuarioIDsDireita = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado).Select(u => u.ID);
            var qtdeEsquerdo = usuarioIDsEsquerda.Count();
            var qtdeDireito = usuarioIDsDireita.Count();
            var qtdeTotal = qtdeEsquerdo + qtdeDireito;
            ViewBag.RedeEsquerdo = qtdeEsquerdo;
            ViewBag.RedeDireito = qtdeDireito;
            ViewBag.RedeTotal = qtdeTotal;

            #endregion

            #region Quantidades

            // AdamastorVer double _PONTUACAO_NIVEL_1 = produtoRepository.Bonificacao("Kit Promocional");
            // AdamastorVer double _PONTUACAO_NIVEL_2 = produtoRepository.Bonificacao("Kit Adesão 1");
            // AdamastorVer double _PONTUACAO_NIVEL_3 = produtoRepository.Bonificacao("Kit Adesão 2");
            // AdamastorVer double _PONTUACAO_NIVEL_4 = produtoRepository.Bonificacao("Kit Adesão 3");
            // AdamastorVer double _PONTUACAO_NIVEL_5 = produtoRepository.Bonificacao("");

            // var hoje = new DateTime(App.DateTimeZion.Year, App.DateTimeZion.Month, App.DateTimeZion.Day, 0, 0, 0, 0);
            // AdamastorVer var bronzeEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 2 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
            // AdamastorVer var bronzeDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 2 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
            // AdamastorVer var silverEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 3 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
            // AdamastorVer var silverDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 3 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
            // AdamastorVer var goldEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 4 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
            // AdamastorVer var goldDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 4 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();
            // AdamastorVer var goldPlusEsquerdaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 5 && usuarioIDsEsquerda.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioEsquerda).Count();
            // AdamastorVer var goldPlusDireitaHoje = usuarioAssociacaoRepository.GetByExpression(a => a.Data >= hoje && a.NivelAssociacao == 5 && usuarioIDsDireita.Contains(a.UsuarioID) && a.UsuarioID != idUsuarioDireita).Count();

            // AdamastorVer ViewBag.PontuacaoEsquerdoHoje = (bronzeEsquerdaHoje * _PONTUACAO_NIVEL_2) + (silverEsquerdaHoje * _PONTUACAO_NIVEL_3) + (goldEsquerdaHoje * _PONTUACAO_NIVEL_4) + (goldPlusEsquerdaHoje * _PONTUACAO_NIVEL_5);
            // AdamastorVer ViewBag.PontuacaoDireitoHoje = (bronzeDireitaHoje * _PONTUACAO_NIVEL_2) + (silverDireitaHoje * _PONTUACAO_NIVEL_3) + (goldDireitaHoje * _PONTUACAO_NIVEL_4) + (goldPlusDireitaHoje * _PONTUACAO_NIVEL_5);

            /*MELHORAR ISSO!*/
            //var AdamastorVer qtdeEsquerdo1 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 1).Count();
            //var AdamastorVer qtdeDireito1 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 1).Count();

            //var AdamastorVer qtdeEsquerdo2 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 2).Count();
            //var AdamastorVer qtdeDireito2 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 2).Count();

            //var AdamastorVer qtdeEsquerdo3 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 3).Count();
            //var AdamastorVer qtdeDireito3 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 3).Count();

            //var AdamastorVer qtdeEsquerdo4 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 4).Count();
            //var AdamastorVer qtdeDireito4 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 4).Count();

            //var AdamastorVer qtdeEsquerdo5 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "0") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 5).Count();
            //var AdamastorVer qtdeDireito5 = usuarioRepository.GetByExpression(u => u.Assinatura.StartsWith(usuario.Assinatura + "1") && u.GeraBonus == true && u.RecebeBonus == true && u.StatusID == statusAssociado && u.NivelAssociacao == 5).Count();

            var statusPago = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago;
            var pagamentos = pedidoPagamentoRepository.GetByExpression(p => p.Pedido.Usuario.GeraBonus == true && p.Pedido.Usuario.RecebeBonus == true && p.PedidoPagamentoStatus.FirstOrDefault(s => s.StatusID == statusPago) != null && p.Pedido.Usuario.Assinatura.StartsWith(usuario.Assinatura));
            double acumuladoBonusEsquerda = 0;

            try //Para caso de null - valor fica zero
            {
                //Deixou de calcular para pegar campos [AcumuladoEsquerda] e [AcumuladoDireita] da tabela [Rede].[Posicao]
                //acumuladoBonusEsquerda = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "0")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                acumuladoBonusEsquerda = posicaoRepository.AcumuladoEsquerda(usuario.ID);

            }
            catch (Exception)
            {
                acumuladoBonusEsquerda = 0;
            }

            double acumuladoBonusDireita = 0;
            try //Para caso de null - valor fica zero
            {
                //Deixou de calcular para pegar campos [AcumuladoEsquerda] e [AcumuladoDireita] da tabela [Rede].[Posicao]
                //acumuladoBonusDireita = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "1")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                acumuladoBonusDireita = posicaoRepository.AcumuladoDireita(usuario.ID);
            }
            catch (Exception)
            {
                acumuladoBonusDireita = 0;
            }

            #endregion

            #region Pontuacao

            ViewBag.Pontos = posicaoRepository.ObtemPontuacao(usuario.ID);
            if (ViewBag.Pontos < 0)
                ViewBag.PontosLado = traducaoHelper["ESQUERDA"];
            else
                ViewBag.PontosLado = traducaoHelper["DIREITA"];

            ViewBag.Pontos = Math.Abs(ViewBag.Pontos);


            ViewBag.PontuacaoEsquerdo = acumuladoBonusEsquerda;
            ViewBag.PontuacaoDireito = acumuladoBonusDireita;

            ViewBag.RedeEsquerdo = qtdeEsquerdo;
            ViewBag.RedeDireito = qtdeDireito;
            ViewBag.RedeTotal = qtdeTotal;

            //ViewBag.RedeEsquerdo1 = qtdeEsquerdo1;
            //ViewBag.RedeDireito1 = qtdeDireito1;
            //ViewBag.RedeTotal1 = qtdeEsquerdo1 + qtdeDireito1;

            //ViewBag.RedeEsquerdo2 = qtdeEsquerdo2;
            //ViewBag.RedeDireito2 = qtdeDireito2;
            //ViewBag.RedeTotal2 = qtdeEsquerdo2 + qtdeDireito2;

            //ViewBag.RedeEsquerdo3 = qtdeEsquerdo3;
            //ViewBag.RedeDireito3 = qtdeDireito3;
            //ViewBag.RedeTotal3 = qtdeEsquerdo3 + qtdeDireito3;

            //ViewBag.RedeEsquerdo4 = qtdeEsquerdo4;
            //ViewBag.RedeDireito4 = qtdeDireito4;
            //ViewBag.RedeTotal4 = qtdeEsquerdo4 + qtdeDireito4;

            //ViewBag.RedeEsquerdo5 = qtdeEsquerdo5;
            //ViewBag.RedeDireito5 = qtdeDireito5;
            //ViewBag.RedeTotal5 = qtdeEsquerdo5 + qtdeDireito5;

            #endregion

            #region PlanoCarreira

            if (ViewBag.DashboardPontos)
            {
                ViewBag.PlanoCarreiraNome = usuarioService.PlanoCarreira(usuario.ID, ViewBag.Pontos);
                decimal percentagem = usuarioService.PlanoCarreiraPercentagem(usuario.ID, ViewBag.Pontos);
                ViewBag.PlanoCarreiraPercentagem = Math.Round(percentagem * 100, 2) + "%";
                ViewBag.PlanoCarreiraNivel = usuarioService.PlanoCarreiraNivel(usuario.ID, ViewBag.Pontos);
            }

            #endregion

            #region BonusBinario

            if (ViewBag.DashboardBinDiario)
            {
                var posicao = posicaoRepository.GetByExpression(x => x.UsuarioID == usuario.ID).OrderByDescending(x => x.DataFim).Take(4);
                ViewBag.BinarioData = posicao;
            }

            #endregion

            #region Titulo

            if (Session["TituloMensagem"] != null)
            {
                ViewBag.TituloMensagem = Session["TituloMensagem"];
                ViewBag.Mensagem = Session["Mensagem"];
                Session["TituloMensagem"] = null;
                Session["Mensagem"] = null;
            }

            #endregion

            return View();
        }

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

        public ContentResult Derramamento(int id)
        {
            usuario.DerramamentoID = id;
            this.repository.Save(usuario);

            Core.Entities.UsuarioDerramamentoLog log = new Core.Entities.UsuarioDerramamentoLog();
            log.UsuarioID = usuario.ID;
            log.Lado = id;
            log.DataCriacao = Core.Helpers.App.DateTimeZion;
            usuarioDerramamentoLogRepository.Save(log);

            return Content("OK");
        }

        public ActionResult SalvarFilial(int idFilial)
        {
            string strRetorno = "";
            try
            {
                usuario.FilialID = idFilial;
                repository.Save(usuario);
                strRetorno = traducaoHelper["FILIAL_SALVAR_SUCESSO"];
            }
            catch (Exception ex)
            {
                strRetorno = "err: " + ex.Message;
            }

            Session["TituloMensagem"] = traducaoHelper["FILIAL"];
            Session["Mensagem"] = strRetorno;

            return RedirectToAction("index", "home");

        }

        //public ActionResult MediaTron()
        //{
        //    //id da empresa Prospera (4) no MediaTron 
        //    string strIdEmpresa = "4";
        //    //URL do sistema mediatron
        //    string strURL = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_ADM_URL");
        //    //URL de Retorno, url desse sistema
        //    string strURLRetorno = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_REDIRECT");
        //    //Esse sistema utiliza o mediatron? (true, false)
        //    string strMediaTronAtivo = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_EM_USO");
        //    //Token para comunucacao entre os sistemas
        //    string strMediaTronToken = Core.Helpers.ConfiguracaoHelper.GetString("MEDIATRON_TOKEN");
        //    //Determina se o processamento deve continuar
        //    bool blnContinua = false;
        //    //Se o sistema utiliza o mediatron continua o processamento
        //    if (!String.IsNullOrEmpty(strMediaTronAtivo))
        //    {
        //        if (strMediaTronAtivo == "true")
        //        {
        //            blnContinua = true;
        //        }
        //    }

        //    if (blnContinua)
        //    {
        //        //Criptografa dados para envio ao mediatron
        //        string strIdEmpresaGet = CriptografiaHelper.Morpho(strIdEmpresa, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strNome = CriptografiaHelper.Morpho(usuario.Nome, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strTelefone = CriptografiaHelper.Morpho(usuario.Telefone, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strCelular = CriptografiaHelper.Morpho(usuario.Celular, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strEmail = CriptografiaHelper.Morpho(usuario.Email, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strEndereco = CriptografiaHelper.Morpho("---", CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strBairro = CriptografiaHelper.Morpho("---", CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strCEP = CriptografiaHelper.Morpho("00000000", CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strLogin = CriptografiaHelper.Morpho(usuario.Login, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strSenha = CriptografiaHelper.Descriptografar(usuario.Senha);
        //        strSenha = CriptografiaHelper.Morpho(strSenha, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strKey = CriptografiaHelper.Morpho(usuario.ID.ToString(), CriptografiaHelper.TipoCriptografia.Criptografa);
        //        string strRedirect = CriptografiaHelper.Morpho(strURLRetorno, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        strMediaTronToken = CriptografiaHelper.Morpho(strMediaTronToken + strIdEmpresa, CriptografiaHelper.TipoCriptografia.Criptografa);
        //        //Monta parametros para serem passados por get
        //        strURL += "?IdEmpresa=" + strIdEmpresaGet + "&" +
        //           "Nome=" + strNome + "&" +
        //           "Telefone=" + strTelefone + "&" +
        //           "Celular=" + strCelular + "&" +
        //           "Email=" + strEmail + "&" +
        //           "Endereco=" + strEndereco + "&" +
        //           "Bairro=" + strBairro + "&" +
        //           "CEP=" + strCEP + "&" +
        //           "Login=" + strLogin + "&" +
        //           "Senha=" + strSenha + "&" +
        //           "Key=" + strKey + "&" +
        //           "Redirect=" + strRedirect + "&" +
        //           "Token=" + strMediaTronToken;
        //        //envia dados ao mediatron
        //        return Redirect(strURL);

        //    }

        //    return View();
        //}

        //public ActionResult MediaTronRetorno(string retorno)
        //{
        //    //Mediatron retorna caso ocorra problemas

        //    //Exibe mensagem de retorno do MediaTron
        //    Session["TituloMensagem"] = "Mediatron";
        //    Session["Mensagem"] = retorno;

        //    return RedirectToAction("index", "home");
        //}

        #endregion

    }
}