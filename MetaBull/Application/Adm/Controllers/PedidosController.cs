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
using Core.Services.Loja;
using Core.Repositories.Financeiro;
using Core.Repositories.Usuario;
using Core.Services.Usuario;

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
using Core.Helpers;
//Rede
using Core.Repositories.Rede;
using System.Transactions;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class PedidosController : Controller
    {
        #region Variaveis

        private PedidoRepository pedidoRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private PedidoService pedidoService;
        private BoletoRepository boletoRepository;
        private PedidoItemRepository pedidoItemRepository;
        private UsuarioService usuarioService;
        private UsuarioRepository usuarioRepository;
        private ProdutoRepository produtoRepository;
        private UsuarioComplementoRepository usuarioComplementoRepository;


        private Core.Helpers.TraducaoHelper traducaoHelper;

        #endregion

        #region Core
        private CicloRepository cicloRepository;
        public PedidosController(DbContext context)
        {
            Localizacao();

            pedidoRepository = new PedidoRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoService = new PedidoService(context);
            boletoRepository = new BoletoRepository(context);
            pedidoItemRepository = new PedidoItemRepository(context);
            usuarioRepository = new UsuarioRepository(context);
            usuarioService = new UsuarioService(context);
            produtoRepository = new ProdutoRepository(context);
            usuarioComplementoRepository = new UsuarioComplementoRepository(context);
            cicloRepository = new CicloRepository(context);
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
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        public ActionResult Index(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, string CurrentProcuraBoleto, string ProcuraBoleto, int? NumeroPaginas, int? Page)
        {
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraLogin, ref ProcuraLogin, ref CurrentProcuraBoleto, ref ProcuraBoleto,
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

            if (ProcuraLogin == null || ProcuraBoleto == null)
            {
                if (ProcuraLogin == null)
                {
                    ProcuraLogin = CurrentProcuraLogin;
                }
                if (ProcuraBoleto == null)
                {
                    ProcuraBoleto = CurrentProcuraBoleto;
                }

            }

            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcurarBoleto = ProcuraBoleto;

            IQueryable<Pedido> pedidos = null;

            pedidos = pedidoRepository.GetAll().OrderByDescending(p => p.DataCriacao);

            if (!String.IsNullOrEmpty(ProcuraLogin))
            {
                pedidos = pedidos.Where(x => x.Usuario.Login.ToLower().Contains(ProcuraLogin.ToLower()));

            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraBoleto))
                {
                    //var zeros = "00000000000".Substring(0, 11 - ProcuraBoleto.Length);
                    Boleto boleto = boletoRepository.GetByNumeroDocumento(int.Parse(ProcuraBoleto));

                    //var zeros = "00000000000".Substring(0 , 11 - ProcuraBoleto.Length);
                    //Boleto boleto = boletoRepository.GetByNossoNumerio(zeros + ProcuraBoleto);

                    if (boleto != null)
                    {
                        Pedido pedido = boleto.PedidoPagamento.Pedido;
                        pedidos = pedidos.Where(x => x.ID == pedido.ID);
                    }
                }
            }


            switch (SortOrder)
            {
                case "login_desc":
                    ViewBag.FirstSortParm = "login";
                    pedidos = pedidos.OrderByDescending(x => x.Usuario.Login);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "login_desc";
                    //pedidos = pedidos.OrderByDescending(x => x.Usuario.ID).ToList();
                    break;
            }

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 40);

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
            int intNumeroPaginas = (NumeroPaginas ?? 40);
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            return View(pedidos.ToPagedList(PageNumber, PageSize));
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var busca = form["loginBusca"];

            ViewBag.Busca = busca;

            int idPedidoBusca = 0;
            if (int.TryParse(busca, out idPedidoBusca))
            {
                var pedidosID = pedidoRepository.GetByExpression(p => p.ID == idPedidoBusca).ToList();
                if (pedidosID != null)
                {
                    return View(pedidosID);
                }
            }

            var pedidos = pedidoRepository.GetByExpression(p => p.Usuario.Nome.Contains(busca) || p.Usuario.Login.Contains(busca) || p.Usuario.NomeFantasia.Contains(busca)).ToList();
            return View(pedidos);
        }

        public ActionResult Lideranca(int id)
        {
            return Pagar(id, true);
        }

        public ActionResult Pagar(int id, bool lideranca = false)
        {
            var pedido = pedidoRepository.Get(id);
            if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                bool validacao = true;

                if (pagamento != null)
                {
                    using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
                    {
                        try
                        {
                            pagamento.MeioPagamento = PedidoPagamento.MeiosPagamento.Manual;
                            pedidoPagamentoRepository.Save(pagamento);
                            validacao = pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago, Local.idUsuario, pagamento.ValorCripto);
                            if (lideranca)
                            {
                                usuarioComplementoRepository.SetLideranca(pedido.Usuario, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            validacao = false;
                            LoggerHelper.WriteFile("ProcessarPagamento : " + ex.Message, "AdmPedidosControllerPagar");
                        }
                        try
                        {
                            if (validacao)
                            {
                                transacao.Complete();
                            }
                            else
                            {
                                transacao.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.WriteFile("ProcessarPagamento : " + ex.Message, "PedidoService");
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Cancelar(int id)
        {
            var pedido = pedidoRepository.Get(id);
            if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                if (pagamento != null)
                {
                    pedidoService.Cancelar(pagamento.ID, Local.idUsuario);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Gratuito(int id)
        {
            var pedido = pedidoRepository.Get(id);
            if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                bool validacao = true;

                if (pagamento != null)
                {
                    using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
                    {
                        try
                        {
                            pagamento.MeioPagamento = PedidoPagamento.MeiosPagamento.Gratis;
                            pedidoPagamentoRepository.Save(pagamento);
                            validacao = pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago, Local.idUsuario, pagamento.ValorCripto);
                        }
                        catch (Exception ex)
                        {
                            validacao = false;
                            LoggerHelper.WriteFile("ProcessarPagamento (Gratis): " + ex.Message, "PedidoController");
                        }
                        try
                        {
                            if (validacao)
                            {
                                transacao.Complete();
                            }
                            else
                            {
                                transacao.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.WriteFile("ProcessarPagamento (Gratis): " + ex.Message, "PedidoController");
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Rastreamento(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido Pedido = db.Pedido.Find(id);
            if (Pedido == null)
            {
                return HttpNotFound();
            }

            return View(Pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalvarRastreamento(FormCollection form)
        {
            Localizacao();

            #region Variaveis
            //Produto
            string strID = form["ID"];
            string strPedidoItemID = form["PedidoItemID"];
            string strTransportadora = form["Transportadora"];
            string strCodigoR = form["CodigoR"];
            string strObservacao = form["Observacao"];
            int intAdministrador = Local.idUsuario;
            string strMnsagem = strTransportadora + " | " + strCodigoR + " | " + strObservacao;
            #endregion

            #region Salva Produto

            PedidoItemStatus pedidoItemStatus = new PedidoItemStatus();
            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CATEGORIA"], erro, "err");
            }
            else
            {
                if (strPedidoItemID == "Todos")
                {
                    var itens = pedidoItemRepository.GetByPedido(int.Parse(strID));

                    //for (int i = 0; i < itens.Count(); i++)
                    //{
                    foreach (var i in itens)
                    {
                        pedidoItemStatus.PedidoItemID = (int)i.ID;
                        pedidoItemStatus.AdministradorID = intAdministrador;
                        pedidoItemStatus.Mensagem = strMnsagem;
                        pedidoItemStatus.StatusID = (int)Core.Entities.PedidoItemStatus.TodosStatus.Enviado;
                        pedidoItemStatus.Data = App.DateTimeZion;

                        db.PedidoItemStatus.Add(pedidoItemStatus);
                        db.SaveChanges();
                    }
                }
                else
                {
                    pedidoItemStatus.PedidoItemID = int.Parse(strPedidoItemID);
                    pedidoItemStatus.AdministradorID = intAdministrador;
                    pedidoItemStatus.Mensagem = strMnsagem;
                    pedidoItemStatus.StatusID = 3;  //verificar
                    pedidoItemStatus.Data = App.DateTimeZion;

                    db.PedidoItemStatus.Add(pedidoItemStatus);
                    db.SaveChanges();
                }
            }

            #endregion

            return RedirectToAction("Index");
        }

        public ActionResult Excel(string CurrentProcuraLogin)
        {
            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
            //*Lista para montar excel
            IQueryable<Pedido> lista = null;

            lista = pedidoRepository.GetAll().OrderByDescending(p => p.DataCriacao);

            if (!String.IsNullOrEmpty(CurrentProcuraLogin))
            {
                lista = lista.Where(x => x.Usuario.Login.Contains(CurrentProcuraLogin));
            }

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["PEDIDOS"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 7;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["PEDIDOS"]);

            //Range do Cabeçalho da planilha
            var objHeadLine = objWorkSheet.Range(objWorkSheet.Cell(2, 1).Address, objWorkSheet.Cell(2, intTotColumns).Address);

            //Formata cabeçalho do relatorio
            objHeadLine.Style.Font.Bold = true;
            objHeadLine.Style.Font.FontSize = 14;
            objHeadLine.Style.Font.FontColor = XLColor.White;
            objHeadLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            objHeadLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            objHeadLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
            objHeadLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;
            objHeadLine.Merge();
            objHeadLine.Value = strReportHeader;

            //Nome das Colunas 
            objWorkSheet.Cell(4, 1).Value = traducaoHelper["LOGIN"];
            objWorkSheet.Cell(4, 2).Value = traducaoHelper["CODIGO"];
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["IDENTIFICADOR"];
            objWorkSheet.Cell(4, 4).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["MEIO_PAGAMENTO"];
            objWorkSheet.Cell(4, 6).Value = traducaoHelper["VALOR"];
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["STATUS"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(4, 1).Address, objWorkSheet.Cell(4, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            //* var Associacoes = associacaoRepository.GetAll().ToList();
            //*var Classificacoes = classificacaoRepository.GetAll().ToList();

            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                //*  var associacao = Associacoes.FirstOrDefault(a => a.Nivel == objFor.NivelAssociacao);
                //* var classificacao = Classificacoes.FirstOrDefault(c => c.Nivel == objFor.NivelClassificacao);
                var pagamento = objFor.PedidoPagamento.FirstOrDefault();

                objWorkSheet.Cell(intLinha, 1).Value = objFor.Usuario.Login;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.ID;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.Codigo;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.DataCriacao;
                objWorkSheet.Cell(intLinha, 5).Value = (pagamento != null ? pagamento.MeioPagamento.ToString() : "");
                objWorkSheet.Cell(intLinha, 6).Value = moedaPadrao.Simbolo + objFor.Total.Value.ToString(moedaPadrao.MascaraOut);
                objWorkSheet.Cell(intLinha, 7).Value = objFor.StatusAtual;

                if (intLinha % 2 == 0)
                {
                    //coloca cor nas linhas impares
                    var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                    dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(5, 1).Address, objWorkSheet.Cell(intTotRows + 4, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            objWorkSheet.Columns().AdjustToContents();

            // Preparação para o response
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"" + strReportHeader + ".xlsx\"");

            // Planilha vai para memoria
            using (MemoryStream memoryStream = new MemoryStream())
            {
                objWorkBook.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                memoryStream.Close();
            }
            //Planilha vai para download
            Response.End();

            //Retorna a lista
            return RedirectToAction("Index");

        }

        public ActionResult Details(int id)
        {
            Pedido pedido = db.Pedido.Find(id);
            if (pedido != null)
            {
                return View(pedido);
            }
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
    }
}
