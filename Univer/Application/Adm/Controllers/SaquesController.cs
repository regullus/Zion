
#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;
using Core.Repositories.Loja;
using Core.Repositories.Financeiro;
using Core.Services.Loja;

//Models Local
using Sistema.Models;

//Lista
using PagedList;
using Helpers;

//Excel
using ClosedXML.Excel;
using cpUtilities;
using System.Threading;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using Core.Services.MeioPagamento;
using System.Globalization;
using Core.Models.Financeiro;
using Core.Helpers;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class SaquesController : Controller
    {
        #region Variaveis
        private SaqueRepository saqueRepository;
        private PedidoRepository pedidoRepository;
        private PedidoService pedidoService;
        private FlowBtcService flowBtcService;
        private SaqueStatusRepository saqueStatusRepository;

        #endregion

        #region Core
        public SaquesController(DbContext context)
        {
            saqueRepository = new SaqueRepository(context);
            pedidoRepository = new PedidoRepository(context);
            pedidoService = new PedidoService(context);
            flowBtcService = new FlowBtcService(context);
            saqueStatusRepository = new SaqueStatusRepository(context);

            Localizacao();
        }
        private YLEVELEntities db = new YLEVELEntities();
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

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Actions
        public ActionResult Index(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, int? ProcuraStatus, int? NumeroPaginas, int? Page, string de, string ate, int? quantidade)
        {
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraLogin, ref ProcuraLogin,
                                    ref NumeroPaginas, ref Page, "Pedidos");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "login";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (ProcuraLogin == null)
            {
                ProcuraLogin = CurrentProcuraLogin;
            }

            ViewBag.CurrentProcuraStatus = ProcuraStatus;
            ViewBag.CurrentProcuraLogin = ProcuraLogin;

            IQueryable<SolicitacaoSaqueModel> saques = null;

            if (string.IsNullOrEmpty(de) || string.IsNullOrEmpty(ate))
            {
                de = App.DateTimeZion.ToString("dd/MM/yyyy");
                ate = App.DateTimeZion.ToString("dd/MM/yyyy");
            }

            ViewBag.De = de;
            ViewBag.Ate = ate;
            ViewBag.ProcuraStatus = ProcuraStatus;

            quantidade = 10000;
            var status = ProcuraStatus.HasValue ? ProcuraStatus.Value : 0;
            saques = saqueRepository.BuscarSaques(de, ate, ProcuraLogin, status, quantidade).AsQueryable();

            var count = saques.Count();

            switch (SortOrder)
            {
                case "login_desc":
                    ViewBag.FirstSortParm = "login";
                    saques = saques?.OrderByDescending(x => x.Login);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "login_desc";
                    saques = saques?.OrderBy(x => x.Login);
                    break;
            }

            ViewBag.Total = saques.Sum(s => s.Liquido);
            ViewBag.TotalBTC = saques.Sum(s => s.LiquidoBTC);
            ViewBag.TotalLTC = saques.Sum(s => s.LiquidoLTC);

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 5);

            //Caso seja selecionada toda a lista (-1), pega na verdade 1000
            if (PageSize == -1)
            {
                PageSize = 1000;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.CurrentNumeroPaginas = NumeroPaginas;

            //Pagina corrente
            int PageNumber = (Page ?? 1);

            //DropDown de paginação
            int intNumeroPaginas = (NumeroPaginas ?? 5);
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            return View(saques.ToPagedList(PageNumber, PageSize));
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var busca = form["loginBusca"];

            ViewBag.Busca = busca;

            var pedidos = pedidoRepository.GetByExpression(p => p.Usuario.Nome.Contains(busca) || p.Usuario.Login.Contains(busca) || p.Usuario.NomeFantasia.Contains(busca)).ToList();
            return View(pedidos);
        }

        public ActionResult Aprovar(int id)
        {
            var saque = saqueRepository.Get(id);

            //saque.StatusAtual.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Aprovado;
            //saqueRepository.Save(saque);

            SaqueStatus saqueStatus = new SaqueStatus();
            saqueStatus.Data = App.DateTimeZion;
            saqueStatus.SaqueID = id;
            saqueStatus.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Aprovado;
            saqueStatus.Ultimo = true;

            saqueStatusRepository.GravaSaqueStatus(saqueStatus);

            return RedirectToAction("Index");
        }

        public ActionResult Reprovar(int id)
        {
            //var saque = saqueRepository.Get(id);

            //saque.StatusAtual.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Reprovado;
            //saqueRepository.Save(saque);

            SaqueStatus saqueStatus = new SaqueStatus();
            saqueStatus.Data = App.DateTimeZion;
            saqueStatus.SaqueID = id;
            saqueStatus.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Reprovado;
            saqueStatus.Ultimo = true;

            saqueStatusRepository.GravaSaqueStatus(saqueStatus);

            return RedirectToAction("Index");
        }

        public ActionResult Cancelar(int id)
        {
            //var saque = saqueRepository.Get(id);

            //saque.StatusAtual.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Solicitado;
            //saqueRepository.Save(saque);

            SaqueStatus saqueStatus = new SaqueStatus();
            saqueStatus.Data = App.DateTimeZion;
            saqueStatus.SaqueID = id;
            saqueStatus.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Solicitado;
            saqueStatus.Ultimo = true;

            saqueStatusRepository.GravaSaqueStatus(saqueStatus);

            return RedirectToAction("Index");
        }

        public ActionResult Reload()
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("Index");
        }
        #endregion

        #region Json
        [HttpPost]
        public JsonResult PagarSaques(List<int> SaquesID)
        {
            var saques = CarregarSaques(SaquesID);
            var error = false;

            if (Core.Helpers.ConfiguracaoHelper.GetString("MEIO_PAGAMENTO").ToUpper().Equals("BLOCKCHAIN"))
            {
                foreach (var saque in saques)
                {
                    if (Pagar(saque))
                        error = true;
                }
            }

            return Json(!error ? "OK" : "ERROR");
        }

        [HttpGet]
        public JsonResult Resumo(int[] SaquesID)
        {
            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            if (Core.Helpers.ConfiguracaoHelper.GetString("MEIO_PAGAMENTO").ToUpper().Equals("BLOCKCHAIN"))
            {
                var saques = CarregarSaques(SaquesID.ToList());
                var account = flowBtcService.Saldo();

                var resumo = new
                {
                    Pagamentos = saques.Sum(s => s.Liquido.Value).ToString(moedaPadrao.MascaraOut),
                    Saldo = account != null ? account.currencies.ToList().FirstOrDefault(c => c.name.Equals("BTC")).balance.ToString(moedaPadrao.MascaraOut) : 0.ToString(moedaPadrao.MascaraOut)
                };

                return Json(resumo, JsonRequestBehavior.AllowGet);
            }
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Metodos Privados
        private List<Saque> CarregarSaques(List<int> SaquesID)
        {
            var saques = new List<Saque>();

            SaquesID.ForEach(i =>
            {
                var saque = this.saqueRepository.Get(i);
                if (saque != null)
                    saques.Add(saque);
            });

            return saques;
        }

        private bool Pagar(Saque saque)
        {
            var error = false;
            FlowBtcResponseWithdrawResponse response = null;
            if (saque.StatusAtual.Status == SaqueStatus.TodosStatus.Solicitado || saque.StatusAtual.Status == SaqueStatus.TodosStatus.Reprocessando)
            {
                var para = saque.Carteira;
                response = flowBtcService.Pagar((float)saque.Liquido, para);

                var saqueStatus = new SaqueStatus
                {
                    AdministradorID = 1,
                    Data = App.DateTimeZion,
                    SaqueID = saque.ID
                };

                if (response != null && response.isAccepted)
                {
                    saqueStatus.StatusID = (int)SaqueStatus.TodosStatus.Pago;
                    saqueStatus.Mensagem = "Transferencia feita via FLOWBTC no valor de " + saque.Liquido + " para a carteira " + para + " em " + App.DateTimeZion.ToString();
                    saqueStatusRepository.Save(saqueStatus);

                }
                else
                {
                    saqueStatus.StatusID = (int)SaqueStatus.TodosStatus.Aviso;
                    saqueStatus.Mensagem = "Erro ao processar via FLOWBTC no valor de " + saque.Liquido + " para a carteira " + para + " em " + App.DateTimeZion.ToString() + " | " + response.rejectReason;
                    saqueStatusRepository.Save(saqueStatus);

                    //cria novo status para o pagamento para aparecer no proximo load da página.
                    var statusReprocessando = new SaqueStatus
                    {
                        AdministradorID = 1,
                        Data = App.DateTimeZion,
                        SaqueID = saque.ID,
                        StatusID = (int)SaqueStatus.TodosStatus.Reprocessando
                    };
                    saqueStatusRepository.Save(statusReprocessando);
                    error = true;
                }
            }

            return error;


        }
        #endregion
    }
}
