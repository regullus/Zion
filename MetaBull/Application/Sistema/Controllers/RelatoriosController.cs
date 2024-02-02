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
using System.IO;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Rede;
using Core.Repositories.Relatorios;
using Core.Models.Relatorios;

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

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class RelatoriosController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis

        #endregion

        #region Core

        private YLEVELEntities db = new YLEVELEntities();

        private CategoriaRepository categoriaRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private PedidoRepository pedidoRepository;
        private BonificacaoRepository bonificacaoRepository;
        private RelatorioRepository relatorioRepository;

        public RelatoriosController(DbContext context)
            : base(context)
        {
            categoriaRepository = new CategoriaRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoRepository = new PedidoRepository(context);
            bonificacaoRepository = new BonificacaoRepository(context);
            relatorioRepository = new RelatorioRepository(context);
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

        #region Actions

        public ActionResult Vendas()
        {
            var statusPago = (int)Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago;

            //=== Verifica o Tipo de Rede (Binaria ou Linear) 
            bool blnRedeBinaria = (Core.Helpers.ConfiguracaoHelper.GetString("REDE_BINARIA") == "true");
            ViewBag.RedeBinaria = blnRedeBinaria;

            if (blnRedeBinaria)
            {
                var pagamentos = pedidoPagamentoRepository.GetByExpression(p => p.Pedido.Usuario.GeraBonus == true && p.Pedido.Usuario.RecebeBonus == true && p.PedidoPagamentoStatus.FirstOrDefault(s => s.StatusID == statusPago) != null && p.Pedido.Usuario.Assinatura.StartsWith(usuario.Assinatura));
                try
                {
                    ViewBag.AcumuladoValorEsquerda = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "0")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.ValorUnitario));
                    ViewBag.AcumuladoBonusEsquerda = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "0")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                    ViewBag.AcumuladoValorDireita = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "1")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.ValorUnitario));
                    ViewBag.AcumuladoBonusDireita = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura + "1")).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                    ViewBag.SemDados = "false";
                }
                catch (Exception)
                {
                    //Em caso de erro nas consultas, na query ocorre erro caso algum campo na conta seja null
                    ViewBag.AcumuladoValorEsquerda = 0;
                    ViewBag.AcumuladoBonusEsquerda = 0;
                    ViewBag.AcumuladoValorDireita = 0;
                    ViewBag.AcumuladoBonusDireita = 0;
                    ViewBag.SemDados = "true";
                }
                ViewBag.pagamentos = pagamentos;
            }
            else
            {
                var pagamentos = pedidoPagamentoRepository.GetByExpression(p => p.Pedido.Usuario.GeraBonus == true &&
                                                                                p.Pedido.Usuario.RecebeBonus == true &&
                                                                                p.PedidoPagamentoStatus.FirstOrDefault(s => s.StatusID == statusPago) != null &&
                                                                                p.Pedido.Usuario.Assinatura.StartsWith(usuario.Assinatura) &&
                                                                                p.Usuario.Assinatura != usuario.Assinatura);
                try
                {
                    ViewBag.AcumuladoValor = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura) && p.Usuario.Assinatura != usuario.Assinatura).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.ValorUnitario));
                    ViewBag.AcumuladoBonus = pagamentos.Where(p => p.Usuario.Assinatura.StartsWith(usuario.Assinatura) && p.Usuario.Assinatura != usuario.Assinatura).Sum(p => p.Pedido.PedidoItem.Sum(i => i.Quantidade * i.BonificacaoUnitaria));
                    ViewBag.SemDados = "false";
                }
                catch (Exception)
                {
                    //Em caso de erro nas consultas, na query ocorre erro caso algum campo na conta seja null
                    ViewBag.AcumuladoValor = 0;
                    ViewBag.AcumuladoBonus = 0;
                    ViewBag.SemDados = "true";
                }
                ViewBag.pagamentos = pagamentos;
            }

            return View(usuario);
        }

        public ActionResult Ganhos()
        {
            ViewBag.Categorias = categoriaRepository.GetByTipo(Core.Entities.Lancamento.Tipos.Bonificacao);
            ViewBag.Usuarios = this.repository.GetAll();
            ViewBag.Pedidos = pedidoRepository.GetAll();
            ViewBag.bonificacoesAll = bonificacaoRepository.GetAll();
            return View(usuario);
        }

        public ActionResult Pontos(string SortOrder, int? Ciclos, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            string strCiclo;
            if (Ciclos.HasValue)
                strCiclo = Ciclos.Value.ToString();
            else
                strCiclo = db.Ciclo.Where(x => x.Ativo == true).FirstOrDefault().ID.ToString();

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas :  20;

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, 
                                    ref strCiclo,
                                    ref strCiclo,
                                    ref NumeroPaginas, 
                                    ref Page, 
                                    "RelatorioPontos");
            objFuncoes = null;
          
            var lista = relatorioRepository.RelatorioPontos(usuario.ID, Int32.Parse(strCiclo));
                      
            ViewBag.Ciclos = new SelectList(db.Ciclo.OrderBy(s => s.Nome), "ID", "Nome", db.Ciclo.Where(x=>x.Ativo==true).FirstOrDefault().ID );

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 20);

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
            int intNumeroPaginas = (NumeroPaginas ?? 20);
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        #region PontosBinario        
        public ActionResult PontosBinario(string SortOrder,                         
                                          string ProcuraDtIni,
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
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref ProcuraDtIni,
                                    ref ProcuraDtFim,                                                  
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioPontosBinario");
            objFuncoes = null;

            if (ProcuraDtIni == null)
                ProcuraDtIni = Core.Helpers.App.DateTimeZion.ToShortDateString();
            if (ProcuraDtFim == null)
                ProcuraDtFim = Core.Helpers.App.DateTimeZion.ToShortDateString();
          
            IList<RelatorioPontosBinarioModel> lista = new List<RelatorioPontosBinarioModel>();

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
            }
            #region ViewBag 

            ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            ViewBag.CurrentProcuraDtFim = ProcuraDtFim;

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

        public ActionResult PontosBinarioExcel(string ProcuraDtIni,
                                               string ProcuraDtFim)
        {

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioPontosBinario(usuario.ID,
                                                                   ProcuraDtIni,
                                                                   ProcuraDtFim);

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
            dataRowRange.Style.NumberFormat. SetFormat("##,###,###,##0");
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

        #region PontosBinario        
        public ActionResult AcumuladoBinario(string SortOrder,                                           
                                             int? NumeroPaginas,
                                             int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            //obtemMensagem();

            //Persistencia dos paramentros da tela
            string ProcuraDtIni="";
            string ProcuraDtFim="";

            NumeroPaginas = NumeroPaginas.HasValue ? NumeroPaginas : 20;

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref ProcuraDtIni,
                                    ref ProcuraDtFim,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "RelatorioAcumuladoBinario");
            objFuncoes = null;

            //if (ProcuraDtIni == null)
            //    ProcuraDtIni = Core.Helpers.App.DateTimeZion.ToShortDateString();
            //if (ProcuraDtFim == null)
            //    ProcuraDtFim = Core.Helpers.App.DateTimeZion.ToShortDateString();

            IList<RelatorioPontosBinarioModel> lista = new List<RelatorioPontosBinarioModel>();

            //int totalDias = (DateTime.Parse(ProcuraDtFim).Subtract(DateTime.Parse(ProcuraDtIni))).Days;
            //if (totalDias > Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA"))
            //{
            //    List<string> msg = new List<string>();
            //    //msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
            //    msg.Add(string.Format(traducaoHelper["PERIODO_SUPERIOR_{0}_DIAS"] + "!", Core.Helpers.ConfiguracaoHelper.GetInt("QUANTIDADE_MAXIMA_DIAS_PESQUISA")));
            //    string[] erro = msg.ToArray();
            //    Mensagem(traducaoHelper["PONTOS_BINARIO"], erro, "err");
            //    obtemMensagem();
            //}
            //else
            //{
                lista = relatorioRepository.RelatorioAcumuladoBinario(usuario.ID);
            //}
            #region ViewBag 

            //ViewBag.CurrentProcuraDtIni = ProcuraDtIni;
            //ViewBag.CurrentProcuraDtFim = ProcuraDtFim;

            //List<Object> procuraStatus = new List<object>();
            //Dictionary<int, string> status = new Dictionary<int, string>();

            //procuraStatus.Add(new { id = 0, nome = traducaoHelper["TODAS"] });

            //var categorias = db.Categorias.Where(s => s.TipoID == 6).OrderBy(s => s.Nome);
            //foreach (var item in categorias)
            //{
            //    procuraStatus.Add(new { id = item.ID, nome = item.Nome });
            //}

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

        public ActionResult AcumuladoBinarioExcel(string ProcuraDtIni,
                                                  string ProcuraDtFim)
        {
            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
        
            //*Lista para montar excel          
            var lista = relatorioRepository.RelatorioPontosBinario(usuario.ID,
                                                                   ProcuraDtIni,
                                                                   ProcuraDtFim);

            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["PONTOS_BINARIO"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 7;

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
            var paramRange = objWorkSheet.Range(objWorkSheet.Cell(4, 5).Address, objWorkSheet.Cell(5, 7).Address);
            paramRange.Style.Font.Bold = true;
            paramRange.Style.Font.FontSize = 12;
            paramRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paramRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paramRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["PERIODO"] + ":";
            objWorkSheet.Cell(5, 5).Value = traducaoHelper["LOGIN"] + ":";
            objWorkSheet.Cell(4, 6).Value = ProcuraDtIni + " " + traducaoHelper["ATE"] + " " + ProcuraDtFim;
            objWorkSheet.Cell(5, 6).Value = usuario.Login + " - " + usuario.Nome;

            //Nome das Colunas 
            objWorkSheet.Cell(7, 1).Value = traducaoHelper["DATA"];
            objWorkSheet.Cell(7, 2).Value = traducaoHelper["USUARIO"];
            objWorkSheet.Cell(7, 3).Value = traducaoHelper["PRODUTO"];
            objWorkSheet.Cell(7, 4).Value = traducaoHelper["PONTOS_DIREITA"];
            objWorkSheet.Cell(7, 5).Value = traducaoHelper["PONTOS_ESQUERDA"];
            objWorkSheet.Cell(7, 6).Value = traducaoHelper["VALOR_DIREITA"];
            objWorkSheet.Cell(7, 7).Value = traducaoHelper["VALOR_ESQUERDA"];

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
                        objWorkSheet.Cell(intLinha, 3).Value = objFor.produtoNome;
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorDireita;
                        break;
                    case 5:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = "";
                        objWorkSheet.Cell(intLinha, 3).Value = traducaoHelper["TOTAL"];
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorDireita;

                        var dataRowRangeImpTo = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                        dataRowRangeImpTo.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                        break;
                    case 9:
                        objWorkSheet.Cell(intLinha, 1).Value = "";
                        objWorkSheet.Cell(intLinha, 2).Value = "";
                        objWorkSheet.Cell(intLinha, 3).Value = traducaoHelper["TOTAL_GERAL"];
                        objWorkSheet.Cell(intLinha, 4).Value = objFor.pontosEsqueda;
                        objWorkSheet.Cell(intLinha, 5).Value = objFor.pontosDireita;
                        objWorkSheet.Cell(intLinha, 6).Value = objFor.valorEsqueda;
                        objWorkSheet.Cell(intLinha, 7).Value = objFor.valorDireita;

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

            dataRowRange = objWorkSheet.Range("D8", "E" + intTotRows + 7);
            dataRowRange.Style.NumberFormat.SetFormat("##,###,###,##0");
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            dataRowRange = objWorkSheet.Range("F8", "G" + intTotRows + 7);
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


        #endregion

    }
}
