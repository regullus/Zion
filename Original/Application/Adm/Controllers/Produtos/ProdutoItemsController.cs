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
using Core.Repositories.Sistema;
using Core.Repositories.Rede;

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

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ProdutoItemsController : Controller
    {
        #region Variaveis

        private string cstrFalha = "";
        //private Boolean cblnCadastroResunido;
        //private ArquivoSecaoRepository arquivoSecaoRepository;
        //private ProdutoTipoRepository produtoTipoRepository;

        private List<string> cstrValidacao = new List<string>();

        #endregion

        #region Core

        private AssociacaoRepository associacaoRepository;
        private ProdutoRepository produtoRepository;
        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

        public ProdutoItemsController(DbContext context)
        {
            Localizacao();

            associacaoRepository = new AssociacaoRepository(context);
            produtoRepository = new ProdutoRepository(context);
            //arquivoSecaoRepository = new ArquivoSecaoRepository(context);
            //produtoTipoRepository = new ProdutoTipoRepository(context);            
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

        public struct lstProduto
        {
            public int ID;
            public string Nome;

            public lstProduto(Core.Entities.Produto produto)
            {
                this.ID = produto.ID;
                this.Nome = produto.Nome;
            }
        }
        #endregion

        #region Actions
        // GET: Produtos
        public ActionResult Index(int? IdPai, string SortOrder, string CurrentProcuraSKU, string ProcuraSKU, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraSKU, ref ProcuraSKU, ref CurrentProcuraNome,
                                    ref ProcuraNome, ref NumeroPaginas, ref Page, "ProdutoItems");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "SKU";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            //if (!(ProcuraSKU != null || ProcuraNome != null))
            //{
            //    if (ProcuraSKU == null)
            //    {
            //        ProcuraSKU = CurrentProcuraSKU;
            //    }
            //    if (ProcuraNome == null)
            //    {
            //        ProcuraNome = CurrentProcuraNome;
            //    }
            //}

            ViewBag.CurrentProcuraSKU = ProcuraSKU;
            ViewBag.CurrentProcuraNome = ProcuraNome;

            IQueryable<ProdutoItem> lista = null;

            lista = db.ProdutoItem.Where(p => p.ProdutoID == IdPai);

            //if (!String.IsNullOrEmpty(ProcuraSKU) && !String.IsNullOrEmpty(ProcuraNome))
            //{
            //    lista = lista.Where(x => x.Produto1.SKU.Contains(ProcuraSKU) && x.Produto1.Nome.Contains(ProcuraNome));
            //}
            //else
            //{
            //    if (!String.IsNullOrEmpty(ProcuraSKU))
            //    {
            //        lista = lista.Where(x => x.Produto1.SKU.Contains(ProcuraSKU));
            //    }
            //    if (!String.IsNullOrEmpty(ProcuraNome))
            //    {
            //        lista = lista.Where(x => x.Produto1.Nome.Contains(ProcuraNome));
            //    }
            //}

            switch (SortOrder)
            {
                case "SKU_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderByDescending(x => x.Produto1.Nome);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome_desc";
                    lista = lista.OrderBy(x => x.Produto1.Nome);
                    break;
                case "Nome_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderByDescending(x => x.Produto1.Nome);
                    break;
                case "SKU":
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderBy(x => x.Produto1.SKU);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderBy(x => x.Produto1.SKU);
                    break;
            }

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

            Produto produtoPai = db.Produto.Find(IdPai);
            if (produtoPai == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProdutoPai = produtoPai;
            ViewBag.IdPai = IdPai;

            return View(lista.ToPagedList(PageNumber, PageSize));
        }

        // GET: Produtos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoItem produtoItem = db.ProdutoItem.Find(id);
            if (produtoItem == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProdutoPai = produtoItem.Produto;
            ViewBag.IdPai = produtoItem.Produto.ID;

            return View(produtoItem);
        }

        // GET: Produtos/Create
        public ActionResult Create(int? IdPai)
        {
            if (IdPai == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produto produtoPai = db.Produto.Find(IdPai);
            if (produtoPai == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProdutoPai = produtoPai;
            ViewBag.IdPai = IdPai;

            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            //ViewBag.ProdutoID = new SelectList(db.Produto.Where(p => p.Composto == false).OrderBy(p => p.Nome), "ID", "Nome");

            //ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");          
            //ViewBag.ProdutoEdit = false;

            return View();
        }
        // POST: Produtos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ProdutoID,ItemID,OpcaoID,Quantidade,Observacao")] ProdutoItem produtoItem)
        {
            if (ModelState.IsValid)
            {
                if (produtoItem.Observacao == null)
                    produtoItem.Observacao = "";

                db.ProdutoItem.Add(produtoItem);
                db.SaveChanges();

                Produto produtoPai = db.Produto.Find(produtoItem.ProdutoID);
                if (produtoPai != null && !produtoPai.Composto)
                {
                    produtoPai.Composto = true;
                    db.Entry(produtoPai).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Index", new { IdPai = produtoPai.ID });
            }

            // ViewBag.TipoID = new SelectList(db.ProdutoTipo, "ID", "Nome", Produto.TipoID);
            // ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria, "ID", "Nome", Produto.ProdutoCategoriaID);

            return View(produtoItem);
        }

        // GET: Produtos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoItem produtoItem = db.ProdutoItem.Find(id);
            if (produtoItem == null)
            {
                return HttpNotFound();
            }

            ///ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            //ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            //ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            //ViewBag.ProdutoItens = Produto.Itens();        

            return View(produtoItem);
        }
        // POST: Produtos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ProdutoID,ItemID,OpcaoID,Quantidade,Observacao")] ProdutoItem produtoItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(produtoItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(produtoItem);
        }

        // GET: Produtos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoItem produtoItem = db.ProdutoItem.Find(id);
            if (produtoItem == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProdutoPai = produtoItem.Produto;
            ViewBag.IdPai = produtoItem.Produto.ID;

            return View(produtoItem);
        }
        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProdutoItem produtoItem = db.ProdutoItem.Find(id);
            Produto produtoPai = produtoItem.Produto;

            db.ProdutoItem.Remove(produtoItem);
            db.SaveChanges();

            if (produtoPai.ProdutoItem.Count == 0)
            {
                produtoPai.Composto = false;
                db.Entry(produtoPai).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { IdPai = produtoPai.ID });
        }


        public ActionResult Excel(string ProcuraSKU, string ProcuraNome)
        {
            //*Lista para montar excel          
            IQueryable<Produto> lista = null;

            lista = db.Produto.Include(p => p.ProdutoTipo);

            if (!String.IsNullOrEmpty(ProcuraSKU) && !String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(x => x.SKU.Contains(ProcuraSKU) && x.Nome.Contains(ProcuraNome));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraSKU))
                {
                    lista = lista.Where(x => x.SKU.Contains(ProcuraSKU));
                }
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
            }


            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["PRODUTO"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 8;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["PRODUTO"]);

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
            objWorkSheet.Cell(4, 1).Value = traducaoHelper["SKU"];
            objWorkSheet.Cell(4, 2).Value = traducaoHelper["NOME"];
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["TIPO"];
            objWorkSheet.Cell(4, 4).Value = traducaoHelper["CHAMADAS"];
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["DESCRICAO"];
            objWorkSheet.Cell(4, 6).Value = traducaoHelper["PUBLICADO"];
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["DATA_CRIACAO"];
            objWorkSheet.Cell(4, 8).Value = traducaoHelper["NIVEL_ASSOCIACAO"];

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

            var Associacoes = associacaoRepository.GetAll().ToList();

            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                var associacao = Associacoes.FirstOrDefault(a => a.Nivel == objFor.NivelAssociacao);

                objWorkSheet.Cell(intLinha, 1).Value = objFor.SKU;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.Nome;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.ProdutoTipo.Nome;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.Chamada;
                objWorkSheet.Cell(intLinha, 5).Value = objFor.Descricao;
                objWorkSheet.Cell(intLinha, 6).Value = objFor.Publicado;
                objWorkSheet.Cell(intLinha, 7).Value = objFor.DataCriacao;
                objWorkSheet.Cell(intLinha, 8).Value = associacao.Nome;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalvarGeral(FormCollection form)
        {
            #region Variaveis
            //Produto
            string strID = form["ID"];
            string strSKU = form["SKU"];
            string strTipoID = form["ProdutoTipo"];
            string strNivelAssociacao = form["NivelAssociacao"];
            string strNome = form["Nome"];
            string strChamada = form["Chamada"];
            string strDescricao = form["Descricao"];
            string strPeso = form["Peso"];
            string strDestaque = form["Destaque"];
            string strLimitePorUsuario = form["LimitePorUsuario"];
            string strLimitePorPedido = form["LimitePorPedido"];
            string strControlaEstoque = form["ControlaEstoque"];
            string strEstoque = form["Estoque"];
            string strPublicado = form["Publicado"];
            string strVendaDireta = form["VendaDireta"];
            string strObservacao = form["Observacao"];
            string strProdutoCategoriaID = form["ProdutoCategoria"];
            string strDimensoes = form["Dimensoes"];
            string strAtivoMensal = form["AtivoMensal"];
            string strComposto = form["Composto"];
            string strDataPublicacao = form["DataPublicacao"];
            string strDataCriacao = form["DataCriacao"];
            string strCodigoExterno = form["CodigoExterno"];
            #endregion

            #region Salva Produto
            Produto produto = new Produto();

            if (strID != null)
                produto.ID = int.Parse(strID);

            produto.SKU = strSKU;
            produto.TipoID = int.Parse(strTipoID);
            produto.NivelAssociacao = int.Parse(strNivelAssociacao);
            produto.Nome = strNome;
            produto.Chamada = strChamada != null ? strChamada : "";
            produto.Descricao = strDescricao != null ? strDescricao : "";
            produto.Peso = strPeso != "" ? double.Parse(strPeso) : 0.0;
            produto.Destaque = strDestaque == "on" ? true : false;
            produto.LimitePorUsuario = strLimitePorUsuario != "" ? int.Parse(strLimitePorUsuario) : 9999;
            produto.LimitePorPedido = strLimitePorPedido != "" ? int.Parse(strLimitePorPedido) : 9999;
            produto.ControlaEstoque = strControlaEstoque == "on" ? true : false;
            produto.Estoque = strEstoque != "" ? int.Parse(strEstoque) : 9999;
            produto.Publicado = strPublicado == "on" ? true : false;
            produto.VendaDireta = strVendaDireta == "on" ? true : false;
            produto.Observacao = strObservacao != null ? strObservacao : "";
            produto.ProdutoCategoriaID = int.Parse(strProdutoCategoriaID);
            produto.Dimensoes = strDimensoes != null ? strDimensoes : "";
            produto.AtivoMensal = strAtivoMensal == "on" ? true : false;
            produto.Composto = strComposto == "on" ? true : false;
            produto.CodigoExterno = strCodigoExterno != null ? strCodigoExterno : "";


            //if (produto.Publicado && strDataPublicacao.Length == 0)
            produto.DataPublicacao = App.DateTimeZion;

            if (strDataCriacao.Length == 0)
                produto.DataCriacao = App.DateTimeZion;
            else
                produto.DataCriacao = DateTime.Parse(strDataCriacao);

            #endregion

            if (produto.ID > 0)
                db.Entry(produto).State = EntityState.Modified;
            else
                db.Produto.Add(produto);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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

        #region JsonResult

        public JsonResult Produtos(int produtoPaiId, int categoriaId)
        {
            var retorno = new List<lstProduto>();
            var produtos = produtoRepository.GetSemComposicaoByCategoria(produtoPaiId, categoriaId).ToList();

            produtos.ForEach(p => retorno.Add(new lstProduto(p)));

            return Json(retorno);
        }

        #endregion
    }
}
