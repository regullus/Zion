using ClosedXML.Excel;
using Core.Entities;
using Core.Helpers;
using Core.Models.Financeiro;
using Core.Models.Relatorios;
using Core.Repositories.Financeiro;
using Core.Repositories.Relatorios;
using Helpers;
using PagedList;
using Sistema.Models.Relatorio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Repositories.Usuario;
using Sistema.Models;
using System.Web.Script.Serialization;

using System.Web.Mvc;
using Core.Repositories.Loja;

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class RelatoriosController : BaseController
    {
        #region Variaveis

        private RelatorioRepository relatorioRepository;
        private Core.Helpers.TraducaoHelper traducaoHelper;
        private SaqueRepository saqueRepository;

        #endregion

        #region Core

        private UsuarioRepository usuarioRepository;
        private ProdutoRepository produtoRepository;


        public RelatoriosController(DbContext context)
        {
            Localizacao();
            relatorioRepository = new RelatorioRepository(context);
            saqueRepository = new SaqueRepository(context);
            usuarioRepository = new UsuarioRepository(context);
            produtoRepository = new ProdutoRepository(context);
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

        private string DescricaoPedidoPagamentoStatus(int statusID)
        {
            switch (statusID)
            {
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Indefinido:
                    return traducaoHelper["INDEFINIDO"];
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento:
                    return traducaoHelper["AGUARDANDO_PAGAMENTO"];
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao:
                    return traducaoHelper["AGUARDANDO_CONFIRMACAO"];
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago:
                    return traducaoHelper["PAGO"];
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Cancelado:
                    return traducaoHelper["CANCELADO"];
                case (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Expirado:
                    return traducaoHelper["ESPIRADO"];
                default:
                    return traducaoHelper["INDEFINIDO"];
            }

        }


        private string DescricaoCategoria(int categoriaID)
        {
            ProdutoCategoria categoria = db.ProdutoCategoria.Where(x => x.ID == categoriaID).FirstOrDefault();

            if (categoria == null)
                return "";
            else
                return categoria.Nome;
        }

        private string DescricaoMeioPagamento(int meioPagamentoID)
        {
            MeioPagamento meioPagamento = db.MeioPagamento.Where(x => x.ID == meioPagamentoID).FirstOrDefault();

            if (meioPagamento == null)
                return "";
            else
                return meioPagamento.Descricao;
        }

        #endregion

        public ActionResult Saldo(string SortOrder, string de, string ate, string login, int? status, int? porAssinatura, int? NumeroPaginas, int? Page)
        {
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);

            objFuncoes.GerenciaParametro("de", ref de, "relatorioSaldo");
            objFuncoes.GerenciaParametro("ate", ref ate, "relatorioSaldo");
            objFuncoes.GerenciaParametro("login", ref login, "relatorioSaldo");
            objFuncoes.GerenciaParametro("status", ref status, "relatorioSaldo");
            objFuncoes.GerenciaParametro("porAssinatura", ref porAssinatura, "relatorioSaldo");

            objFuncoes.GerenciaPaginacao(ref SortOrder, ref NumeroPaginas, ref Page, "Usuarios");

            var culture = new CultureInfo("pt-BR");

            var dataInicio = DateTime.MinValue;
            var dataFim = DateTime.MinValue;

            var relatorio = new RelatorioSaldo();

            if (DateTime.TryParse(de, culture, DateTimeStyles.None, out dataInicio) &&
                DateTime.TryParse(ate, culture, DateTimeStyles.None, out dataFim) && status.HasValue)
            {
                relatorio = this.relatorioRepository.RelatorioSaldo(dataInicio.ToString("yyyy/MM/dd"), dataFim.ToString("yyyy/MM/dd"), status.Value, login, porAssinatura.Equals(1));
            }
            else
            {
                ViewBag.Mensagem = "Dados da busca inválidos";
            }

            ViewBag.De = de;
            ViewBag.Ate = ate;
            ViewBag.Login = login;
            ViewBag.PorAssinatura = porAssinatura;
            ViewBag.Status = status;

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? base.PageSizeDefault);

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

            var relatorioSaldoModel = new RelatorioSaldoModel(PageSize);

            if (relatorio.Itens.Any())
            {
                relatorioSaldoModel.Items = relatorio.Itens.ToPagedList(PageNumber, PageSize);
                relatorioSaldoModel.Resumo = relatorio.Resumo;
            }

            return View(relatorioSaldoModel);
        }

        public ActionResult Pagamento(string SortOrder, string DataInicio, string DataFim, int? NumeroPaginas, int? Page)
        {
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.GerenciaParametro("DataInicio", ref DataInicio, "relatorioPagamento");
            objFuncoes.GerenciaParametro("DataFim", ref DataFim, "relatorioPagamento");
            objFuncoes.GerenciaPaginacao(ref SortOrder, ref NumeroPaginas, ref Page, "relatorioPagamento");

            var dataInicio = App.DateTimeZion;
            var dataFim = App.DateTimeZion;
            var date = App.DateTimeZion;

            if (DateTime.TryParse(DataInicio, out date))
            {
                dataInicio = date;
            }

            if (DateTime.TryParse(DataFim, out date))
            {
                dataFim = date;
            }


            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;

            var relatorio = this.relatorioRepository.RelatorioPagamento(dataInicio, dataFim);

            relatorio.ForEach(delegate (RelatorioPagamentoModel item)
            {
                item.EnderecoPrincipal = string.IsNullOrEmpty(item.Logradouro) ? string.Empty :
                    string.Format("{0}, {1}{2}, CEP: {3}, {4} - {5}",
                        item.Logradouro,
                        item.Numero,
                        (string.IsNullOrEmpty(item.Complemento) ? string.Empty : string.Concat(" - ", item.Complemento)),
                        item.CodigoPostal,
                        item.Cidade,
                        item.Estado);
            });

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? base.PageSizeDefault);

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

            var relatorioSaldoPgamento = new RelatorioPagamentoPaginacaoModel(PageSize);

            if (relatorio.Any())
            {
                relatorioSaldoPgamento.Itens = relatorio.ToPagedList(PageNumber, PageSize);
            }

            return View(relatorioSaldoPgamento);
        }

        public ActionResult ReloadPagamento()
        {
            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("Pagamento");
        }

        public ActionResult ExcelPagamento(string DataInicio, string DataFim)
        {
            var dataInicio = DateTime.MinValue;
            var dataFim = DateTime.MinValue;

            DateTime.TryParse(DataInicio, out dataInicio);
            DateTime.TryParse(DataFim, out dataFim);

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            //*Lista para montar excel          
            var lista = this.relatorioRepository.RelatorioPagamento(
                (dataInicio.Equals(DateTime.MinValue) ? (DateTime?)null : dataInicio),
                (dataFim.Equals(DateTime.MinValue) ? (DateTime?)null : dataFim));

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            var strReportHeader = traducaoHelper["PAGAMENTO"];

            //Total de linhas
            var intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            var intTotColumns = 20;

            var objWorkBook = new XLWorkbook();

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["PAGAMENTO"]);

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
            objWorkSheet.Cell(4, 2).Value = traducaoHelper["NOME"];
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["CELULAR"];
            objWorkSheet.Cell(4, 4).Value = traducaoHelper["SKU"];
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["PRODUTO"];
            objWorkSheet.Cell(4, 6).Value = traducaoHelper["ENDERECO_PRINCIPAL"];
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["NUMERO"];
            objWorkSheet.Cell(4, 8).Value = traducaoHelper["COMPLEMENTO"];
            objWorkSheet.Cell(4, 9).Value = traducaoHelper["CODIGO_POSTAL"];
            objWorkSheet.Cell(4, 10).Value = traducaoHelper["CIDADE"];
            objWorkSheet.Cell(4, 11).Value = traducaoHelper["ESTADO"];
            objWorkSheet.Cell(4, 12).Value = traducaoHelper["ENDERECO_ENTREGA_ALTERNATIVO"];
            objWorkSheet.Cell(4, 13).Value = traducaoHelper["NUMERO"];
            objWorkSheet.Cell(4, 14).Value = traducaoHelper["COMPLEMENTO"];
            objWorkSheet.Cell(4, 15).Value = traducaoHelper["CODIGO_POSTAL"];
            objWorkSheet.Cell(4, 16).Value = traducaoHelper["CIDADE"];
            objWorkSheet.Cell(4, 17).Value = traducaoHelper["ESTADO"];
            objWorkSheet.Cell(4, 18).Value = traducaoHelper["DATA_PAGAMENTO"];
            objWorkSheet.Cell(4, 19).Value = traducaoHelper["CODIGO"];
            objWorkSheet.Cell(4, 20).Value = traducaoHelper["TOTAL"];

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

            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var item in lista)
            {
                intLinha++;

                objWorkSheet.Cell(intLinha, 1).Value = item.Login;
                objWorkSheet.Cell(intLinha, 2).Value = item.Nome;
                objWorkSheet.Cell(intLinha, 3).Value = item.Celular;
                objWorkSheet.Cell(intLinha, 4).Value = item.SKU;
                objWorkSheet.Cell(intLinha, 5).Value = item.Produto;
                objWorkSheet.Cell(intLinha, 6).Value = item.Logradouro;
                objWorkSheet.Cell(intLinha, 7).Value = item.Numero;
                objWorkSheet.Cell(intLinha, 8).Value = item.Complemento;
                objWorkSheet.Cell(intLinha, 9).Value = item.CodigoPostal;
                objWorkSheet.Cell(intLinha, 10).Value = item.Cidade;
                objWorkSheet.Cell(intLinha, 11).Value = item.Estado;
                objWorkSheet.Cell(intLinha, 12).Value = item.LogradouroAlt;
                objWorkSheet.Cell(intLinha, 13).Value = item.NumeroAlt;
                objWorkSheet.Cell(intLinha, 14).Value = item.ComplementoAlt;
                objWorkSheet.Cell(intLinha, 15).Value = item.CodigoPostalAlt;
                objWorkSheet.Cell(intLinha, 16).Value = item.CidadeAlt;
                objWorkSheet.Cell(intLinha, 17).Value = item.EstadoAlt;
                objWorkSheet.Cell(intLinha, 18).Value = item.DataPagamento;
                objWorkSheet.Cell(intLinha, 19).Value = item.Codigo;
                objWorkSheet.Cell(intLinha, 20).Value = item.Total.ToString(moedaPadrao.MascaraOut);

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
            Response.AddHeader("content-disposition", string.Format("attachment;filename=\"{0}.xlsx\"", strReportHeader));

            // Planilha vai para memoria
            using (var memoryStream = new MemoryStream())
            {
                objWorkBook.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                memoryStream.Close();
            }
            //Planilha vai para download
            Response.End();

            //Retorna a lista
            return RedirectToAction("Pagamento");

        }

        #region PedidosAdesaoUpgrade
        public ActionResult PedidosAdesaoUpgrade(string SortOrder,
                                                 string ProcuraDtIni,
                                                 string ProcuraDtFim,
                                                 string ProcuraLogin,
                                                 int? ProcuraStatus,
                                                 int? NumeroPaginas,
                                                 int? Page)
        {
            Localizacao();


            //Persistencia dos paramentros da tela
            string strProcuraStatus = ProcuraStatus.HasValue ? ProcuraStatus.Value.ToString() : "0";

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref ProcuraDtIni,
                                    ref ProcuraDtFim,
                                    ref ProcuraLogin,
                                    ref strProcuraStatus,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioPedidosAdesaoUpgrade");
            objFuncoes = null;

            if (ProcuraDtIni == null)
                ProcuraDtIni = Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraDtFim == null)
                ProcuraDtFim = Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraStatus == null)
                ProcuraStatus = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago;

            IList<RelatorioPedidosModel> lista = new List<RelatorioPedidosModel>();

            int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
            if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
            {
                List<string> msg = new List<string>();
                //msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                msg.Add(string.Format(traducaoHelper["PERIODO_INFERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["ADESAO_UPGRADE"], erro, "err");
                obtemMensagem();
            }
            else
            {
                lista = relatorioRepository.RelatorioPedidosAdesaoUpgrade(ProcuraDtIni,
                                                                          ProcuraDtFim,
                                                                          ProcuraLogin,
                                                                          ProcuraStatus);
            }
            #region ViewBag 

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;
            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcuraStatus = ProcuraStatus;

            List<Object> procuraStatus = new List<object>();
            foreach (var item in Enum.GetValues(typeof(Core.Entities.PedidoPagamentoStatus.TodosStatus)))
            {
                procuraStatus.Add(new { id = (int)item, nome = item });
            }
            ViewBag.ProcuraStatus = new SelectList(procuraStatus, "ID", "Nome", ProcuraStatus);

            #endregion

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
            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        public ActionResult PedidosAdesaoUpgradeExcel(string ProcuraDtIni,
                                                      string ProcuraDtFim,
                                                      string ProcuraLogin,
                                                      int? ProcuraStatus)
        {
            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioPedidosAdesaoUpgrade(ProcuraDtIni,
                                                                          ProcuraDtFim,
                                                                          ProcuraLogin,
                                                                          ProcuraStatus);

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["ADESAO_UPGRADE"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 11;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["ADESAO_UPGRADE"]);

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

            //Parametros de seleçâo           
            var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 7).Address, objWorkSheet.Cell(6, 11).Address);
            paramRange.Style.Font.Bold = true;
            paramRange.Style.Font.FontSize = 10;
            paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["PERIODO"] + ":";
            objWorkSheet.Cell(5, 7).Value = traducaoHelper["STATUS"] + ":";
            objWorkSheet.Cell(6, 7).Value = traducaoHelper["LOGIN"] + ":";
            objWorkSheet.Cell(4, 8).Value = ProcuraDtIni + " " + traducaoHelper["ATE"] + " " + ProcuraDtFim;
            objWorkSheet.Cell(5, 8).Value = ProcuraStatus == 0 ? traducaoHelper["TODAS"] : DescricaoPedidoPagamentoStatus((int)ProcuraStatus);
            objWorkSheet.Cell(6, 8).Value = string.IsNullOrEmpty(ProcuraLogin) ? "" : ProcuraLogin;

            //Nome das Colunas 
            objWorkSheet.Cell(8, 1).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(8, 2).Value = traducaoHelper["USUARIO"];
            objWorkSheet.Cell(8, 3).Value = traducaoHelper["PEDIDO"];
            objWorkSheet.Cell(8, 4).Value = traducaoHelper["PRODUTO"];
            objWorkSheet.Cell(8, 5).Value = traducaoHelper["MEIO_PAGAMENTO"];
            objWorkSheet.Cell(8, 6).Value = traducaoHelper["STATUS"];
            objWorkSheet.Cell(8, 7).Value = traducaoHelper["DATA_STATUS"];
            objWorkSheet.Cell(8, 8).Value = traducaoHelper["VALOR"];
            objWorkSheet.Cell(8, 9).Value = traducaoHelper["JUROS"];
            objWorkSheet.Cell(8, 10).Value = traducaoHelper["FRETE"];
            objWorkSheet.Cell(8, 11).Value = traducaoHelper["TOTAL"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(8, 1).Address, objWorkSheet.Cell(8, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            int intLinha = 8;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                objWorkSheet.Cell(intLinha, 1).Value = objFor.dataPedido;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.login;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.pedido;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.produto;
                objWorkSheet.Cell(intLinha, 5).Value = objFor.meioPagamento;
                objWorkSheet.Cell(intLinha, 6).Value = DescricaoPedidoPagamentoStatus(objFor.pgtoStatusID);
                objWorkSheet.Cell(intLinha, 7).Value = objFor.dataStatus;
                objWorkSheet.Cell(intLinha, 8).Value = objFor.valor;
                objWorkSheet.Cell(intLinha, 9).Value = objFor.juros;
                objWorkSheet.Cell(intLinha, 10).Value = objFor.frete;
                objWorkSheet.Cell(intLinha, 11).Value = objFor.total;

                if (intLinha % 2 == 0)
                {
                    //coloca cor nas linhas impares
                    var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                    dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(9, 1).Address, objWorkSheet.Cell(intTotRows + 8, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            dataRowRange = objWorkSheet.Range("H9", "K" + intTotRows + 8);
            dataRowRange.Style.NumberFormat.SetFormat("R$ #,###,###.00");
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            objWorkSheet.Columns().AdjustToContents();

            objWorkSheet.SetShowGridLines(false);

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
        #endregion

        #region Vendas
        public ActionResult Vendas(string SortOrder,
                                   string CurrentProcuraDtIni,
                                   string ProcuraDtIni,
                                   string CurrentProcuraDtFim,
                                   string ProcuraDtFim,
                                   int? CurrentProcuraTipo,
                                   int? ProcuraTipo,
                                   string CurrentProcuraLogin,
                                   string ProcuraLogin,
                                   int? CurrentProcuraStatus,
                                   int? ProcuraStatus,
                                   int? CurrentProcuraCategoria,
                                   int? ProcuraCategoria,
                                   int? CurrentProcuraProdutoTipo,
                                   int? ProcuraProdutoTipo,
                                   string CurrentProcuraProduto,
                                   string ProcuraProduto,
                                   int? CurrentProcuraMeioPagamento,
                                   int? ProcuraMeioPagamento,
                                   int? NumeroPaginas,
                                   int? Page)
        {
            Localizacao();
            string strCurrentProcuraTipo = CurrentProcuraTipo.HasValue ? CurrentProcuraTipo.Value.ToString() : "1";
            string strProcuraTipo = ProcuraTipo.HasValue ? ProcuraTipo.Value.ToString() : "1";
            string strCurrentProcuraStatus = CurrentProcuraStatus.HasValue ? CurrentProcuraStatus.Value.ToString() : "0";
            string strProcuraStatus = ProcuraStatus.HasValue ? ProcuraStatus.Value.ToString() : "0";
            string strCurrentProcuraCategoria = CurrentProcuraCategoria.HasValue ? CurrentProcuraCategoria.Value.ToString() : "0";
            string strProcuraCategoria = ProcuraCategoria.HasValue ? ProcuraCategoria.Value.ToString() : "0";

            string strCurrentProcuraProdutoTipo = CurrentProcuraProdutoTipo.HasValue ? CurrentProcuraProdutoTipo.Value.ToString() : "0";
            string strProcuraProdutoTipo = ProcuraProdutoTipo.HasValue ? ProcuraProdutoTipo.Value.ToString() : "0";

            string strCurrentProcuraMeioPagamento = CurrentProcuraMeioPagamento.HasValue ? CurrentProcuraMeioPagamento.Value.ToString() : "0";
            string strProcuraMeioPagamento = ProcuraMeioPagamento.HasValue ? ProcuraMeioPagamento.Value.ToString() : "0";

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref strCurrentProcuraTipo,
                                    ref strProcuraTipo,
                                    ref CurrentProcuraDtIni,
                                    ref ProcuraDtIni,
                                    ref CurrentProcuraDtFim,
                                    ref ProcuraDtFim,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref strCurrentProcuraStatus,
                                    ref strProcuraStatus,
                                    ref strCurrentProcuraCategoria,
                                    ref strProcuraCategoria,
                                    ref CurrentProcuraProduto,
                                    ref ProcuraProduto,
                                    ref strCurrentProcuraProdutoTipo,
                                    ref strProcuraProdutoTipo,
                                    ref strCurrentProcuraMeioPagamento,
                                    ref strProcuraMeioPagamento,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioVendas");
            objFuncoes = null;

            if (ProcuraTipo == null)
                ProcuraTipo = strCurrentProcuraTipo != null ? int.Parse(strCurrentProcuraTipo) : 1;
            if (ProcuraDtIni == null)
                ProcuraDtIni = CurrentProcuraDtIni != null ? CurrentProcuraDtIni : Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraDtFim == null)
                ProcuraDtFim = CurrentProcuraDtFim != null ? CurrentProcuraDtFim : Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraStatus == null)
                ProcuraStatus = strCurrentProcuraStatus != null ? int.Parse(strCurrentProcuraStatus) : 0;
            if (ProcuraCategoria == null)
                ProcuraCategoria = strCurrentProcuraCategoria != null ? int.Parse(strCurrentProcuraCategoria) : 0;
            if (ProcuraLogin == null)
                ProcuraLogin = CurrentProcuraLogin != null ? CurrentProcuraLogin : "";
            if (ProcuraProduto == null)
                ProcuraProduto = CurrentProcuraProduto != null ? CurrentProcuraProduto : "";
            if (ProcuraProdutoTipo == null)
                ProcuraProdutoTipo = CurrentProcuraProdutoTipo != null ? CurrentProcuraProdutoTipo : 0;
            if (ProcuraMeioPagamento == null)
                ProcuraMeioPagamento = strCurrentProcuraMeioPagamento != null ? int.Parse(strCurrentProcuraMeioPagamento) : 0;


            IList<RelatorioPedidosModel> lista = new List<RelatorioPedidosModel>();

            int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
            if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
            {
                List<string> msg = new List<string>();
                //msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                msg.Add(string.Format(traducaoHelper["PERIODO_SUPERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["VENDAS"], erro, "err");
                obtemMensagem();
            }
            else
            {
                lista = relatorioRepository.RelatorioVendas(ProcuraTipo,
                                                            ProcuraDtIni,
                                                            ProcuraDtFim,
                                                            ProcuraLogin,
                                                            ProcuraProduto,
                                                            ProcuraStatus,
                                                            ProcuraCategoria,
                                                            ProcuraProdutoTipo,
                                                            ProcuraMeioPagamento);
            }
            #region ViewBag 

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;
            ViewBag.CurrentProcuraTipo = ProcuraTipo;
            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcuraStatus = ProcuraStatus;
            ViewBag.CurrentProcuraCategoria = ProcuraCategoria;
            ViewBag.CurrentProcuraProduto = ProcuraProduto;
            ViewBag.CurrentProcuraProdutoTipo = ProcuraProdutoTipo;
            ViewBag.CurrentProcuraMeioPagamento = ProcuraMeioPagamento;

            List<Object> lstStatus = new List<object>();
            lstStatus.Add(new { id = 0, nome = traducaoHelper["TODOS"] });
            lstStatus.Add(new { id = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento, nome = traducaoHelper["AGUARDANDO_PAGAMENTO"] });
            lstStatus.Add(new { id = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao, nome = traducaoHelper["AGUARDANDO_CONFIRMACAO"] });
            lstStatus.Add(new { id = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Cancelado, nome = traducaoHelper["CANCELADO"] });
            lstStatus.Add(new { id = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Expirado, nome = traducaoHelper["ESPIRADO"] });
            lstStatus.Add(new { id = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago, nome = traducaoHelper["PAGO"] });
            ViewBag.ProcuraStatus = new SelectList(lstStatus, "ID", "Nome", ProcuraStatus);

            List<Object> lstTipo = new List<object>();
            lstTipo.Add(new { id = 1, nome = traducaoHelper["ANALITICO"] });
            lstTipo.Add(new { id = 2, nome = traducaoHelper["SINTETICO"] });
            ViewBag.ProcuraTipo = new SelectList(lstTipo, "ID", "Nome", ProcuraTipo);

            var categorias = db.ProdutoCategoria.OrderBy(x => x.Nome);
            List<Object> lstCategoria = new List<object>();
            lstCategoria.Add(new { id = 0, nome = traducaoHelper["TODAS"] });
            foreach (var item in categorias)
            {
                lstCategoria.Add(new { id = item.ID, nome = item.Nome });
            }
            ViewBag.ProcuraCategoria = new SelectList(lstCategoria, "ID", "Nome", ProcuraCategoria);


            var produtoTipos = db.ProdutoTipo.OrderBy(x => x.Nome);
            List<Object> lstProdutoTipo = new List<object>();
            lstProdutoTipo.Add(new { id = 0, nome = traducaoHelper["TODAS"] });
            foreach (var item in produtoTipos)
            {
                lstProdutoTipo.Add(new { id = item.ID, nome = item.Nome });
            }
            ViewBag.ProcuraProdutoTipo = new SelectList(lstProdutoTipo, "ID", "Nome", ProcuraProdutoTipo);


            var lstMeioPagamento = new List<object>
            {

                new { id = 0 , nome = traducaoHelper["TODAS"] },
                new { id = (int)PedidoPagamento.MeiosPagamento.CryptoPayments , nome = "CryptoPayments" },
                new { id = (int)PedidoPagamento.MeiosPagamento.Manual , nome = "Manual" }
            };

            ViewBag.ProcuraMeioPagamento = new SelectList(lstMeioPagamento, "ID", "Nome", ProcuraMeioPagamento);


            #endregion

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
            if (lista != null)
            {
                return View(lista.ToPagedList(PageNumber, PageSize));
            } else
            {
                return View();
            }
        }

        public ActionResult VendasExcel(string CurrentProcuraDtIni,
                                        string CurrentProcuraDtFim,
                                        int? CurrentProcuraTipo,
                                        string CurrentProcuraLogin,
                                        int? CurrentProcuraStatus,
                                        int? CurrentProcuraCategoria,
                                        string CurrentProcuraProduto,
                                        int? CurrentProcuraProdutoTipo,
                                        int? CurrentProcuraMeioPagamento)
        {

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioVendas(CurrentProcuraTipo,
                                                            CurrentProcuraDtIni,
                                                            CurrentProcuraDtFim,
                                                            CurrentProcuraLogin,
                                                            CurrentProcuraProduto,
                                                            CurrentProcuraStatus,
                                                            CurrentProcuraCategoria,
                                                            CurrentProcuraProdutoTipo,
                                                            CurrentProcuraMeioPagamento);

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["VENDAS"] + " - " + (CurrentProcuraTipo == 1 ? traducaoHelper["ANALITICO"] : traducaoHelper["SINTETICO"]);

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = CurrentProcuraTipo == 1 ? 10 : 4;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(strReportHeader);

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

            if (CurrentProcuraTipo == 1)
            {
                #region Analitido               
                //Parametros de seleçâo           
                var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 7).Address, objWorkSheet.Cell(8, 10).Address);
                paramRange.Style.Font.Bold = true;
                paramRange.Style.Font.FontSize = 10;
                paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                objWorkSheet.Cell(4, 7).Value = traducaoHelper["PERIODO"] + ":";
                objWorkSheet.Cell(5, 7).Value = traducaoHelper["STATUS"] + ":";
                objWorkSheet.Cell(6, 7).Value = traducaoHelper["LOGIN"] + ":";
                objWorkSheet.Cell(7, 7).Value = traducaoHelper["CATEGORIA"] + ":";
                objWorkSheet.Cell(8, 7).Value = traducaoHelper["PRODUTO"] + ":";
                objWorkSheet.Cell(9, 7).Value = traducaoHelper["MEIO_PAGAMENTO"] + ":";
                objWorkSheet.Cell(4, 8).Value = CurrentProcuraDtIni + " " + traducaoHelper["ATE"] + " " + CurrentProcuraDtFim;
                objWorkSheet.Cell(5, 8).Value = CurrentProcuraStatus == 0 ? traducaoHelper["TODOS"] : DescricaoPedidoPagamentoStatus((int)CurrentProcuraStatus);
                objWorkSheet.Cell(6, 8).Value = string.IsNullOrEmpty(CurrentProcuraLogin) ? "" : CurrentProcuraLogin;
                objWorkSheet.Cell(7, 8).Value = CurrentProcuraCategoria == 0 ? traducaoHelper["TODAS"] : DescricaoCategoria((int)CurrentProcuraCategoria);
                objWorkSheet.Cell(8, 8).Value = string.IsNullOrEmpty(CurrentProcuraProduto) ? "" : CurrentProcuraProduto;
                objWorkSheet.Cell(9, 8).Value = CurrentProcuraMeioPagamento == 0 ? traducaoHelper["TODAS"] : DescricaoMeioPagamento((int)CurrentProcuraMeioPagamento);

                //Nome das Colunas 
                objWorkSheet.Cell(10, 1).Value = traducaoHelper["USUARIO"];
                objWorkSheet.Cell(10, 2).Value = traducaoHelper["PEDIDO"];
                objWorkSheet.Cell(10, 3).Value = traducaoHelper["DATA"];
                objWorkSheet.Cell(10, 4).Value = traducaoHelper["PRODUTO"];
                objWorkSheet.Cell(10, 5).Value = traducaoHelper["MEIO_PAGAMENTO"];
                objWorkSheet.Cell(10, 6).Value = traducaoHelper["STATUS"];
                objWorkSheet.Cell(10, 7).Value = traducaoHelper["DATA_STATUS"];
                objWorkSheet.Cell(10, 8).Value = traducaoHelper["QUANTIDADE"];
                objWorkSheet.Cell(10, 9).Value = traducaoHelper["VALOR_UNITARIO"];
                objWorkSheet.Cell(10, 10).Value = traducaoHelper["TOTAL"]; objWorkSheet.Cell(8, 1).Value = traducaoHelper["DATA"];

                //formata na linha dos nomes dos campos
                var columnRange = objWorkSheet.Range(objWorkSheet.Cell(10, 1).Address, objWorkSheet.Cell(10, intTotColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                int intLinha = 11;
                //*Preenche planilha com os valores
                foreach (var objFor in lista)
                {
                    intLinha++;

                    if (objFor.tipoRg == 1)
                    {
                        objWorkSheet.Cell(intLinha, 1).Value = objFor.login;
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.pedido;
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.dataPedido;
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.produto;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.meioPagamento;
                        objWorkSheet.Cell(intLinha, 6).Value = DescricaoPedidoPagamentoStatus(objFor.pgtoStatusID);
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.dataStatus;
                        objWorkSheet.Cell(intLinha, 8).Value = objFor.quantidade;
                        objWorkSheet.Cell(intLinha, 9).Value = objFor.valor;
                        objWorkSheet.Cell(intLinha, 10).Value = objFor.total;
                    }
                    else
                    {
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.tipoRg == 2 ? traducaoHelper["TOTAL"] + " " + objFor.login : traducaoHelper["TOTAL_GERAL"];
                        objWorkSheet.Cell(intLinha, 8).Value = objFor.quantidade;
                        objWorkSheet.Cell(intLinha, 9).Value = objFor.valor;
                        objWorkSheet.Cell(intLinha, 10).Value = objFor.total;
                    }

                    if (objFor.tipoRg > 1)
                    {
                        //coloca cor nas linhas de totais
                        var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                    }
                }

                //Formata range dos valores preenchidos
                var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(10, 1).Address, objWorkSheet.Cell(intTotRows + 10, intTotColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                dataRowRange = objWorkSheet.Range("H11", "H" + intTotRows + 10);
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                dataRowRange = objWorkSheet.Range("I11", "J" + intTotRows + 10);
                dataRowRange.Style.NumberFormat.SetFormat(moedaPadrao.MascaraOut);
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                #endregion
            }
            else
            {
                #region Sintetico           
                //Parametros de seleçâo           
                var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 3).Address, objWorkSheet.Cell(8, 4).Address);
                paramRange.Style.Font.Bold = true;
                paramRange.Style.Font.FontSize = 10;
                paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                objWorkSheet.Cell(4, 3).Value = traducaoHelper["PERIODO"] + ":";
                objWorkSheet.Cell(5, 3).Value = traducaoHelper["STATUS"] + ":";
                objWorkSheet.Cell(6, 3).Value = traducaoHelper["LOGIN"] + ":";
                objWorkSheet.Cell(7, 3).Value = traducaoHelper["CATEGORIA"] + ":";
                objWorkSheet.Cell(8, 3).Value = traducaoHelper["PRODUTO"] + ":";
                objWorkSheet.Cell(4, 4).Value = CurrentProcuraDtIni + " " + traducaoHelper["ATE"] + " " + CurrentProcuraDtFim;
                objWorkSheet.Cell(5, 4).Value = CurrentProcuraStatus == 0 ? traducaoHelper["TODOS"] : DescricaoPedidoPagamentoStatus((int)CurrentProcuraStatus);
                objWorkSheet.Cell(6, 4).Value = string.IsNullOrEmpty(CurrentProcuraLogin) ? "" : CurrentProcuraLogin;
                objWorkSheet.Cell(7, 4).Value = CurrentProcuraCategoria == 0 ? traducaoHelper["TODAS"] : DescricaoCategoria((int)CurrentProcuraCategoria);
                objWorkSheet.Cell(8, 4).Value = string.IsNullOrEmpty(CurrentProcuraProduto) ? "" : CurrentProcuraProduto;

                //Nome das Colunas 
                objWorkSheet.Cell(10, 1).Value = traducaoHelper["CATEGORIA"];
                objWorkSheet.Cell(10, 2).Value = traducaoHelper["QUANTIDADE"];
                objWorkSheet.Cell(10, 3).Value = traducaoHelper["VALOR"];
                objWorkSheet.Cell(10, 4).Value = traducaoHelper["STATUS"];

                //formata na linha dos nomes dos campos
                var columnRange = objWorkSheet.Range(objWorkSheet.Cell(10, 1).Address, objWorkSheet.Cell(10, intTotColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                int intLinha = 10;
                //*Preenche planilha com os valores
                foreach (var objFor in lista)
                {
                    intLinha++;

                    if (objFor.tipoRg == 2)
                    {
                        objWorkSheet.Cell(intLinha, 1).Value = objFor.categoria;
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.quantidade;
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.total;
                        objWorkSheet.Cell(intLinha, 4).Value = DescricaoPedidoPagamentoStatus(objFor.pgtoStatusID);
                    }
                    else
                    {
                        objWorkSheet.Cell(intLinha, 1).Value = objFor.tipoRg == 9 ? traducaoHelper["TOTAL_GERAL"] : traducaoHelper["TOTAL"] + " " + objFor.categoria;
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.quantidade;
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.total;
                    }

                    if (objFor.tipoRg > 2)
                    {
                        //coloca cor nas linhas de totais
                        var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                    }
                }

                //Formata range dos valores preenchidos
                var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(10, 1).Address, objWorkSheet.Cell(intTotRows + 10, intTotColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                dataRowRange = objWorkSheet.Range("C11", "C" + intTotRows + 10);
                dataRowRange.Style.NumberFormat.SetFormat("R$ #,###,###.00");
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                #endregion
            }

            objWorkSheet.Columns().AdjustToContents();

            objWorkSheet.SetShowGridLines(false);

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
        #endregion

        #region VendasComposicao
        public ActionResult VendasComposicao(string SortOrder,
                                   string CurrentProcuraDtIni,
                                   string ProcuraDtIni,
                                   string CurrentProcuraDtFim,
                                   string ProcuraDtFim,
                                   string CurrentProcuraLogin,
                                   string ProcuraLogin,
                                   int? CurrentProcuraProdutoTipo,
                                   int? ProcuraProdutoTipo,
                                   string CurrentProcuraProduto,
                                   string ProcuraProduto,
                                   int? CurrentProcuraMeioPagamento,
                                   int? ProcuraMeioPagamento,
                                   int? NumeroPaginas,
                                   int? Page)
        {
            Localizacao();

            string strCurrentProcuraProdutoTipo = CurrentProcuraProdutoTipo.HasValue ? CurrentProcuraProdutoTipo.Value.ToString() : "0";
            string strProcuraProdutoTipo = ProcuraProdutoTipo.HasValue ? ProcuraProdutoTipo.Value.ToString() : "0";

            string strCurrentProcuraMeioPagamento = CurrentProcuraMeioPagamento.HasValue ? CurrentProcuraMeioPagamento.Value.ToString() : "0";
            string strProcuraMeioPagamento = ProcuraMeioPagamento.HasValue ? ProcuraMeioPagamento.Value.ToString() : "0";

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraDtIni,
                                    ref ProcuraDtIni,
                                    ref CurrentProcuraDtFim,
                                    ref ProcuraDtFim,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref CurrentProcuraProduto,
                                    ref ProcuraProduto,
                                    ref strCurrentProcuraProdutoTipo,
                                    ref strProcuraProdutoTipo,
                                    ref strCurrentProcuraMeioPagamento,
                                    ref strProcuraMeioPagamento,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioVendasComposicao");
            objFuncoes = null;

            if (ProcuraDtIni == null)
                ProcuraDtIni = CurrentProcuraDtIni != null ? CurrentProcuraDtIni : Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraDtFim == null)
                ProcuraDtFim = CurrentProcuraDtFim != null ? CurrentProcuraDtFim : Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraLogin == null)
                ProcuraLogin = CurrentProcuraLogin != null ? CurrentProcuraLogin : "";
            if (ProcuraProduto == null)
                ProcuraProduto = CurrentProcuraProduto != null ? CurrentProcuraProduto : "";
            if (ProcuraProdutoTipo == null)
                ProcuraProdutoTipo = CurrentProcuraProdutoTipo != null ? CurrentProcuraProdutoTipo : 0;
            if (ProcuraMeioPagamento == null)
                ProcuraMeioPagamento = strCurrentProcuraMeioPagamento != null ? int.Parse(strCurrentProcuraMeioPagamento) : 0;


            IList<RelatorioPedidosModel> lista = new List<RelatorioPedidosModel>();

            int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
            if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
            {
                List<string> msg = new List<string>();
                //msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                msg.Add(string.Format(traducaoHelper["PERIODO_SUPERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["VENDAS"], erro, "err");
                obtemMensagem();
            }
            else
            {
                lista = relatorioRepository.RelatorioVendasComposicao(ProcuraDtIni,
                                                            ProcuraDtFim,
                                                            ProcuraLogin,
                                                            ProcuraProduto,
                                                            (int)PedidoPagamentoStatus.TodosStatus.Pago,
                                                            0,
                                                            ProcuraProdutoTipo,
                                                            ProcuraMeioPagamento);

            }
            #region ViewBag 

            ViewBag.Produtos = new List<Produto>();

            if (lista != null && lista.Any())
            {
                ViewBag.Produtos = produtoRepository.GetByExpression(e => e.Composto).ToList();
            }

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;
            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcuraProduto = ProcuraProduto;
            ViewBag.CurrentProcuraProdutoTipo = ProcuraProdutoTipo;
            ViewBag.CurrentProcuraMeioPagamento = ProcuraMeioPagamento;

            var produtoTipos = db.ProdutoTipo.OrderBy(x => x.Nome);
            List<Object> lstProdutoTipo = new List<object>();
            lstProdutoTipo.Add(new { id = 0, nome = traducaoHelper["TODAS"] });
            foreach (var item in produtoTipos)
            {
                lstProdutoTipo.Add(new { id = item.ID, nome = item.Nome });
            }
            ViewBag.ProcuraProdutoTipo = new SelectList(lstProdutoTipo, "ID", "Nome", ProcuraProdutoTipo);


            var lstMeioPagamento = new List<object>
            {

                new { id = 0 , nome = traducaoHelper["TODAS"] },
                new { id = (int)PedidoPagamento.MeiosPagamento.CryptoPayments , nome = "CryptoPayments" },
                new { id = (int)PedidoPagamento.MeiosPagamento.Manual , nome = "Manual" }
            };

            ViewBag.ProcuraMeioPagamento = new SelectList(lstMeioPagamento, "ID", "Nome", ProcuraMeioPagamento);
            
            #endregion

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
            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        public ActionResult VendasComposicaoExcel(string CurrentProcuraDtIni,
                                        string CurrentProcuraDtFim,
                                        string CurrentProcuraLogin,
                                        string CurrentProcuraProduto,
                                        int? CurrentProcuraProdutoTipo,
                                        int? CurrentProcuraMeioPagamento)
        {

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioVendasComposicao(CurrentProcuraDtIni,
                                                            CurrentProcuraDtFim,
                                                            CurrentProcuraLogin,
                                                            CurrentProcuraProduto,
                                                           (int)PedidoPagamentoStatus.TodosStatus.Pago,
                                                            0,
                                                            CurrentProcuraProdutoTipo,
                                                            CurrentProcuraMeioPagamento);

            if (lista == null)
            {
                return HttpNotFound();
            }

            var produtos = new List<Produto>();

            if (lista.Any())
            {
                produtos = produtoRepository.GetByExpression(e => e.Composto).ToList();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["VENDAS_COMPOSICAO"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 6;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(strReportHeader);

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

            #region Analitido               
            //Parametros de seleçâo           
            var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 5).Address, objWorkSheet.Cell(7, 6).Address);
            paramRange.Style.Font.Bold = true;
            paramRange.Style.Font.FontSize = 10;
            paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["PERIODO"] + ":";
            objWorkSheet.Cell(5, 5).Value = traducaoHelper["LOGIN"] + ":";
            objWorkSheet.Cell(6, 5).Value = traducaoHelper["PRODUTO"] + ":";
            objWorkSheet.Cell(7, 5).Value = traducaoHelper["MEIO_PAGAMENTO"] + ":";
            objWorkSheet.Cell(4, 6).Value = CurrentProcuraDtIni + " " + traducaoHelper["ATE"] + " " + CurrentProcuraDtFim;
            objWorkSheet.Cell(5, 6).Value = string.IsNullOrEmpty(CurrentProcuraLogin) ? "" : CurrentProcuraLogin;
            objWorkSheet.Cell(6, 6).Value = string.IsNullOrEmpty(CurrentProcuraProduto) ? "" : CurrentProcuraProduto;
            objWorkSheet.Cell(7, 6).Value = CurrentProcuraMeioPagamento == 0 ? traducaoHelper["TODAS"] : DescricaoMeioPagamento((int)CurrentProcuraMeioPagamento);

            //Nome das Colunas 
            objWorkSheet.Cell(9, 1).Value = traducaoHelper["USUARIO"];
            objWorkSheet.Cell(9, 2).Value = traducaoHelper["PEDIDO"];
            objWorkSheet.Cell(9, 3).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(9, 4).Value = traducaoHelper["PRODUTO"];
            objWorkSheet.Cell(9, 5).Value = traducaoHelper["STATUS"];
            objWorkSheet.Cell(9, 6).Value = traducaoHelper["COMPOSICAO"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(9, 1).Address, objWorkSheet.Cell(9, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            int intLinha = 9;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                objWorkSheet.Cell(intLinha, 1).Value = objFor.login;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.pedido;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.dataPedido;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.produto;
                objWorkSheet.Cell(intLinha, 5).Value = DescricaoPedidoPagamentoStatus(objFor.pgtoStatusID);

                var produtoComposicao = produtos.FirstOrDefault(p => p.ID == objFor.produtoID);

                if (produtoComposicao != null)
                {
                    var composicaoDescricao = string.Empty;

                    foreach (var produtoItem in produtoComposicao.ProdutoItem)
                    {
                        composicaoDescricao += string.Format("{0} {1} {2}" , produtoItem.Quantidade, produtoItem.Produto1.Descricao, Environment.NewLine);
                    }

                    objWorkSheet.Cell(intLinha, 6).Value = composicaoDescricao;
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(10, 1).Address, objWorkSheet.Cell(intTotRows + 9, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

             #endregion

            objWorkSheet.Columns().AdjustToContents();

            objWorkSheet.SetShowGridLines(false);

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
        #endregion

        #region BonusPagos        
        public ActionResult BonusPagos(string SortOrder,
                                       string ProcuraDtIni,
                                       string ProcuraDtFim,
                                       string ProcuraUsuario,
                                       int? ProcuraCategoria,
                                       int? NumeroPaginas,
                                       int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            //obtemMensagem();

            //Persistencia dos paramentros da tela
            string strProcuraCategoria = ProcuraCategoria.HasValue ? ProcuraCategoria.Value.ToString() : "0";

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref ProcuraDtIni,
                                    ref ProcuraDtFim,
                                    ref ProcuraUsuario,
                                    ref strProcuraCategoria,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioBonusPagos");
            objFuncoes = null;

            if (ProcuraDtIni == null)
                ProcuraDtIni = Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraDtFim == null)
                ProcuraDtFim = Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraCategoria == null)
                ProcuraCategoria = 0;

            IList<RelatorioBonusPagosModel> lista = new List<RelatorioBonusPagosModel>();

            int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
            if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
            {
                List<string> msg = new List<string>();
                //msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                msg.Add(string.Format(traducaoHelper["PERIODO_SUPERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["BONIFICACOES_PAGAS"], erro, "err");
                obtemMensagem();
            }
            else
            {
                Usuario usuario = new Usuario();
                if (!string.IsNullOrEmpty(ProcuraUsuario))
                {
                    usuario = usuarioRepository.Get(int.Parse(ProcuraUsuario));

                    var usuariosIDs = new List<ItemViewModel>();
                    usuariosIDs.Add(new ItemViewModel { id = usuario.ID, Name = usuario.Login });
                    ViewBag.UsuariosIDs = usuariosIDs;
                    ViewBag.UsuariosJson = new JavaScriptSerializer().Serialize(usuariosIDs);
                }

                lista = relatorioRepository.RelatorioBonusPagos(ProcuraDtIni,
                                                               ProcuraDtFim,
                                                               usuario.Login,
                                                               ProcuraCategoria);
            }
            #region ViewBag 

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;
            ViewBag.CurrentProcuraLogin = ProcuraUsuario;
            ViewBag.CurrentProcuraCategoria = ProcuraCategoria;

            List<Object> procuraStatus = new List<object>();
            //Dictionary<int, string> status = new Dictionary<int, string>();

            procuraStatus.Add(new { id = 0, nome = traducaoHelper["TODAS"] });

            var categorias = db.Categorias.Where(s => s.TipoID == 6).OrderBy(s => s.Nome);
            foreach (var item in categorias)
            {
                procuraStatus.Add(new { id = item.ID, nome = item.Nome });
            }
            ViewBag.ProcuraCategoria = new SelectList(procuraStatus, "ID", "Nome", ProcuraCategoria);

            #endregion

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
            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        public ActionResult BonusPagosExcel(string ProcuraDtIni,
                                            string ProcuraDtFim,
                                            string ProcuraUsuario,
                                            int? ProcuraCategoria)
        {
            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioBonusPagos(ProcuraDtIni,
                                                                ProcuraDtFim,
                                                                ProcuraUsuario,
                                                                ProcuraCategoria);

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["BONIFICACOES_PAGAS"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 6;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["BONIFICACOES_PAGAS"]);

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

            //Parametros de seleçâo           
            var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 3).Address, objWorkSheet.Cell(6, 6).Address);
            paramRange.Style.Font.Bold = true;
            paramRange.Style.Font.FontSize = 10;
            paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["PERIODO"] + ":";
            objWorkSheet.Cell(5, 3).Value = traducaoHelper["BONIFICACAO"] + ":";
            objWorkSheet.Cell(6, 3).Value = traducaoHelper["LOGIN"] + ":";
            objWorkSheet.Cell(4, 4).Value = ProcuraDtIni + " " + traducaoHelper["ATE"] + " " + ProcuraDtFim;
            objWorkSheet.Cell(5, 4).Value = ProcuraCategoria == 0 ? traducaoHelper["TODAS"] : db.Categorias.Where(s => s.ID == ProcuraCategoria).FirstOrDefault().Nome;
            objWorkSheet.Cell(6, 4).Value = string.IsNullOrEmpty(ProcuraUsuario) ? "" : ProcuraUsuario;

            //Nome das Colunas 
            objWorkSheet.Cell(8, 1).Value = traducaoHelper["USUARIO"];
            objWorkSheet.Cell(8, 2).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(8, 3).Value = traducaoHelper["BONIFICACAO"];
            objWorkSheet.Cell(8, 4).Value = traducaoHelper["VALOR"];
            objWorkSheet.Cell(8, 5).Value = traducaoHelper["ATIVO"];
            objWorkSheet.Cell(8, 6).Value = traducaoHelper["ASSOCIACAO"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(8, 1).Address, objWorkSheet.Cell(8, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            int intLinha = 8;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                switch (objFor.tipoRg)
                {
                    case 1:
                        objWorkSheet.Cell(intLinha, 1).Value = objFor.login;
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.data;
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.categoriaNome;
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.valor;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.ativo == 0 ? traducaoHelper["NAO"] : traducaoHelper["SIM"];
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.nivelAssociacao;
                        break;
                    case 2:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.data;
                        objWorkSheet.Cell(intLinha, 3).Value = traducaoHelper["TOTAL"] + " " + objFor.login;
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.valor;
                        objWorkSheet.Cell(intLinha, 5).Value = "";
                        objWorkSheet.Cell(intLinha, 6).Value = "";

                        var dataRowRangeImpTo = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImpTo.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                        break;
                    case 9:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.data;
                        objWorkSheet.Cell(intLinha, 3).Value = traducaoHelper["TOTAL_GERAL"];
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.valor;
                        objWorkSheet.Cell(intLinha, 5).Value = "";
                        objWorkSheet.Cell(intLinha, 6).Value = "";

                        var dataRowRangeImpTg = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImpTg.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                        break;
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(9, 1).Address, objWorkSheet.Cell(intTotRows + 8, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            dataRowRange = objWorkSheet.Range("D9", "D" + intTotRows + 8);
            dataRowRange.Style.NumberFormat.SetFormat("R$ #,###,###.00");
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            objWorkSheet.Columns().AdjustToContents();

            objWorkSheet.SetShowGridLines(false);

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

        #endregion

        #region PontosBinario        
        public ActionResult PontosBinario(string SortOrder,
                                          string CurrentProcuraUsuario,
                                          string ProcuraUsuario,
                                          string CurrentProcuraDtIni,
                                          string ProcuraDtIni,
                                          string CurrentProcuraDtFim,
                                          string ProcuraDtFim,
                                          int? NumeroPaginas,
                                          int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            //obtemMensagem();

            //Persistencia dos paramentros da tela

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela   
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraUsuario,
                                    ref ProcuraUsuario,
                                    ref CurrentProcuraDtIni,
                                    ref ProcuraDtIni,
                                    ref CurrentProcuraDtFim,
                                    ref ProcuraDtFim,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioPontosBinario");
            objFuncoes = null;

            var primeiraVez = false;

            if (string.IsNullOrEmpty(ProcuraDtIni))
            {
                ProcuraDtIni = Core.Helpers.App.DateTimeZion.ToShortDateString();
                primeiraVez = true;
            }
            if (string.IsNullOrEmpty(ProcuraDtFim))
            {
                ProcuraDtFim = Core.Helpers.App.DateTimeZion.ToShortDateString();
            }

            IList<RelatorioPontosBinarioModel> lista = new List<RelatorioPontosBinarioModel>();

            var usuario = new Usuario();
            if (string.IsNullOrEmpty(ProcuraUsuario))
            {
                if (!primeiraVez)
                {
                    List<string> msg = new List<string>();
                    msg.Add(traducaoHelper["SELECIONE_USUARIO"]);
                    string[] erro = msg.ToArray();
                    Mensagem(traducaoHelper["PONTOS_BINARIO"], erro, "err");
                    obtemMensagem();
                }
            }
            else
            {
                usuario = usuarioRepository.Get(int.Parse(ProcuraUsuario));

                int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
                if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
                {
                    List<string> msg = new List<string>();
                    msg.Add(string.Format(traducaoHelper["PERIODO_SUPERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
                    string[] erro = msg.ToArray();
                    Mensagem(traducaoHelper["PONTOS_BINARIO"], erro, "err");
                    obtemMensagem();
                }
                else
                {
                    lista = relatorioRepository.RelatorioPontosBinario(usuario.ID,
                                                                       ProcuraDtIni,
                                                                       ProcuraDtFim);

                    var usuariosIDs = new List<ItemViewModel>();
                    usuariosIDs.Add(new ItemViewModel { id = usuario.ID, Name = usuario.Login });
                    ViewBag.UsuariosIDs = usuariosIDs;
                    ViewBag.UsuariosJson = new JavaScriptSerializer().Serialize(usuariosIDs);

                    ViewBag.Usuarios = "," + usuario.ID;
                }
            }
            #region ViewBag 

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;
            ViewBag.CurrentProcuraUsuario = ProcuraUsuario;

            #endregion

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
            return View(lista.ToPagedList(PageNumber, PageSize));
        }

        public ActionResult PontosBinarioExcel(string ProcuraUsuario,
                                               string ProcuraDtIni,
                                               string ProcuraDtFim)
        {

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            var usuario = new Usuario();
            IList<RelatorioPontosBinarioModel> lista = new List<RelatorioPontosBinarioModel>();
            //*Lista para montar excel      
            if (!string.IsNullOrEmpty(ProcuraUsuario))
            {
                usuario = usuarioRepository.Get(int.Parse(ProcuraUsuario));
                lista = relatorioRepository.RelatorioPontosBinario(usuario.ID,
                                                                   ProcuraDtIni,
                                                                   ProcuraDtFim);
            }

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["PONTOS_BINARIO"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 8;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["PONTOS_BINARIO"]);

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

            //Parametros de seleçâo           
            var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 7).Address, objWorkSheet.Cell(5, 8).Address);
            paramRange.Style.Font.Bold = true;
            paramRange.Style.Font.FontSize = 12;
            paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["PERIODO"] + ":";
            objWorkSheet.Cell(5, 7).Value = traducaoHelper["LOGIN"] + ":";
            objWorkSheet.Cell(4, 8).Value = ProcuraDtIni + " " + traducaoHelper["ATE"] + " " + ProcuraDtFim;
            objWorkSheet.Cell(5, 8).Value = usuario.Login + " - " + usuario.Nome;

            //Nome das Colunas 
            objWorkSheet.Cell(7, 1).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(7, 2).Value = traducaoHelper["USUARIO"];
            objWorkSheet.Cell(7, 3).Value = traducaoHelper["PATROCINADOR"];
            objWorkSheet.Cell(7, 4).Value = traducaoHelper["PRODUTO"];
            objWorkSheet.Cell(7, 5).Value = traducaoHelper["PONTOS_DIREITA"];
            objWorkSheet.Cell(7, 6).Value = traducaoHelper["PONTOS_ESQUERDA"];
            objWorkSheet.Cell(7, 7).Value = traducaoHelper["VALOR_DIREITA"];
            objWorkSheet.Cell(7, 8).Value = traducaoHelper["VALOR_ESQUERDA"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(7, 1).Address, objWorkSheet.Cell(7, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            int intLinha = 7;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                switch (objFor.tipoRg)
                {
                    case 1:
                        objWorkSheet.Cell(intLinha, 1).Value = objFor.dataCriacao;
                        objWorkSheet.Cell(intLinha, 2).Value = objFor.login;
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.loginPatrocinador;
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.produtoNome;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 8).Value = objFor.valorDireita;
                        break;
                    case 5:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = "";
                        objWorkSheet.Cell(intLinha, 3).Value = "";
                        objWorkSheet.Cell(intLinha, 4).Value = traducaoHelper["TOTAL"];
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 8).Value = objFor.valorDireita;

                        var dataRowRangeImpTo = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImpTo.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                        break;
                    case 9:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = "";
                        objWorkSheet.Cell(intLinha, 3).Value = "";
                        objWorkSheet.Cell(intLinha, 4).Value = traducaoHelper["TOTAL_GERAL"];
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 8).Value = objFor.valorDireita;

                        var dataRowRangeImpTg = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImpTg.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
                        break;
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(8, 1).Address, objWorkSheet.Cell(intTotRows + 7, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            dataRowRange = objWorkSheet.Range("E8", "F" + intTotRows + 7);
            dataRowRange.Style.NumberFormat.SetFormat("##,###,###,##0");
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            dataRowRange = objWorkSheet.Range("G8", "H" + intTotRows + 7);
            dataRowRange.Style.NumberFormat.SetFormat(moedaPadrao.MascaraOut);
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            objWorkSheet.Columns().AdjustToContents();

            objWorkSheet.SetShowGridLines(false);

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
        #endregion

        public ActionResult Reload(string acao)
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction(acao);
        }

        #region JsonResult
        public JsonResult GetUsuarios(string search)
        {
            IQueryable<Usuario> usuarios = usuarioRepository.GetByExpression(x => x.Login.Contains(search));
            return Json(usuarios.Select(s => new { id = s.ID, text = s.Login }).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}