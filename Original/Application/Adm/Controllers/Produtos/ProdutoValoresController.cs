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
using System.Text.RegularExpressions;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ProdutoValoresController : Controller
    {
        #region Variaveis
        #endregion

        #region Core

        private ProdutoRepository produtoRepository;

        public ProdutoValoresController(DbContext context)
        {

            produtoRepository = new ProdutoRepository(context);

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

        public static double RemoverMascara(string chave)
        {

            if (chave.Length == 0)
                return 0;

            double valor = 0;
            double divisorCasas = 1;

            // Remove a mascara 
            int intPosicao = chave.IndexOf(',') + 1;
            if (intPosicao > 0)
                divisorCasas = Math.Pow(10, (chave.Length - intPosicao));
            else
                divisorCasas = 1;

            valor = double.Parse(Regex.Replace(chave.Replace("_", "0"), @"[^\d]", "")) / divisorCasas;

            return valor;
        }

        public static double ColocaMascara(string chave)
        {

            if (chave.Length == 0)
                return 0;

            double valor = 0;
            double divisorCasas = 1;

            // Remove a mascara 
            int intPosicao = chave.IndexOf(',') + 1;
            if (intPosicao > 0)
                divisorCasas = Math.Pow(10, (chave.Length - intPosicao));
            else
                divisorCasas = 1;

            valor = double.Parse(Regex.Replace(chave.Replace("_", "0"), @"[^\d]", "")) / divisorCasas;

            return valor;
        }

        #endregion

        #region Actions
        // GET: ProdutoValores
        public ActionResult Index(string SortOrder, string CurrentProcuraSKU, string ProcuraSKU, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
        {
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraSKU, ref ProcuraSKU, ref CurrentProcuraNome,
                                    ref ProcuraNome, ref NumeroPaginas, ref Page, "Produtos");

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

            if (!(ProcuraSKU != null || ProcuraNome != null))
            {
                if (ProcuraSKU == null)
                {
                    ProcuraSKU = CurrentProcuraSKU;
                }
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
            }

            ViewBag.CurrentProcuraSKU = ProcuraSKU;
            ViewBag.CurrentProcuraNome = ProcuraNome;

            IQueryable<ProdutoValor> lista = null;

            lista = db.ProdutoValor.Include(p => p.Produto); //.Include(p => p.ProdutoOpcao);

            if (!String.IsNullOrEmpty(ProcuraSKU) && !String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(x => x.Produto.SKU.Contains(ProcuraSKU) && x.Produto.Nome.Contains(ProcuraNome));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraSKU))
                {
                    lista = lista.Where(x => x.Produto.SKU.Contains(ProcuraSKU));
                }
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Produto.Nome.Contains(ProcuraNome));
                }
            }

            switch (SortOrder)
            {
                case "SKU_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderByDescending(x => x.Produto.Nome);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome_desc";
                    lista = lista.OrderBy(x => x.Produto.Nome);
                    break;
                case "Nome_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderByDescending(x => x.Produto.Nome);
                    break;
                case "SKU":
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderBy(x => x.Produto.SKU);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderBy(x => x.Produto.SKU);
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
            return View(lista.ToPagedList(PageNumber, PageSize));

            //return View(produtoValor.ToList());
        }

        // GET: ProdutoValores/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoValor produtoValor = db.ProdutoValor.Find(id);
            if (produtoValor == null)
            {
                return HttpNotFound();
            }
            return View(produtoValor);
        }

        // GET: ProdutoValores/Create
        public ActionResult Create()
        {
            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            ViewBag.MascaraEntrada = moedaPadrao.MascaraIn;
            ViewBag.Moeda = moedaPadrao.Simbolo;
            ViewBag.ProdutoID = new SelectList(produtoRepository.GetProdutoSemValor(Core.Helpers.ConfiguracaoHelper.GetString("LOJA_PRODUTO_NIVE_ATRIBUIR_PRECO")), "ID", "Nome");
            ViewBag.OpcaoID = new SelectList(db.ProdutoOpcao.OrderBy(o => o.Nome), "ID", "Nome");

            return View();
        }

        // POST: ProdutoValores/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form)
        {
            #region Variaveis
            string strID = form["ProdutoID"];
            string strOpcao = form["OpcaoID"];
            string strContas = form["ContasIDs"];
            string strAssociacoes = form["AssociacaoIDs"];
            string strClassificacoes = form["ClassificacaoIDs"];
            string strBlocos = form["BlocosIDs"];
            string strPaises = form["PaisIDs"];
            string strValor = form["Valor"];
            string strBonificacao = form["Bonificacao"];
            string strBonusVenda = form["VlrBonusVenda"];
            string strBonusVendaAdicinal = form["VlrBonusAdicionalVenda"];

            #endregion
            #region Cria ProdutoValor
            ProdutoValor produtovalor = new ProdutoValor();

            #endregion

            if (strID != null)

                produtovalor.ProdutoID = int.Parse(strID);
            produtovalor.Valor = RemoverMascara(strValor);
            produtovalor.Bonificacao = RemoverMascara(strBonificacao);
            produtovalor.VlrBonusVenda = RemoverMascara(strBonusVenda);
            produtovalor.VlrBonusAdicionalVenda = RemoverMascara(strBonusVendaAdicinal);
            produtovalor.ContaIDs = string.Empty;
            produtovalor.AssociacaoIDs = string.Empty;
            produtovalor.BlocoIDs = string.Empty;
            produtovalor.PaisIDs = string.Empty;
            produtovalor.ClassificacaoIDs = string.Empty;



            if (ModelState.IsValid)
            {
                db.ProdutoValor.Add(produtovalor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.ProdutoID = new SelectList(db.Produto.OrderBy(o => o.Nome), "ID", "Nome", produtovalor.ProdutoID);


            ViewBag.OpcaoID = new SelectList(db.ProdutoOpcao.OrderBy(o => o.Nome), "ID", "Nome", produtovalor.OpcaoID);


            return View(produtovalor);
        }

        // GET: ProdutoValores/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoValor produtovalor = db.ProdutoValor.Find(id);
            if (produtovalor == null)
            {
                return HttpNotFound();
            }


            ViewBag.ProdutoID = new SelectList(db.Produto, "ID", "Nome", produtovalor.ProdutoID);
            ViewBag.OpcaoID = new SelectList(db.ProdutoOpcao, "ID", "Nome", produtovalor.OpcaoID);

            return View(produtovalor);
        }

        // POST: ProdutoValores/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection form)
        {
            #region Variaveis
            string strID = form["ID"];
            string strProdutoID = form["ProdutoID"];
            string strOpcao = form["OpcaoID"];
            string strContas = form["ContasIDs"];
            string strAssociacoes = form["AssociacaoIDs"];
            string strClassificacoes = form["ClassificacaoIDs"];
            string strBlocos = form["BlocosIDs"];
            string strPaises = form["PaisIDs"];
            string strValor = form["Valor"];
            string strBonificacao = form["Bonificacao"];
            string strBonusVenda = form["VlrBonusVenda"];
            string strBonusVendaAdicinal = form["VlrBonusAdicionalVenda"];

            #endregion
            #region Cria ProdutoValor
            ProdutoValor produtovalor = new ProdutoValor();

            #endregion

            produtovalor.ID = int.Parse(strID);
            produtovalor.ProdutoID = int.Parse(strProdutoID);
            produtovalor.Valor = ColocaMascara(strValor);
            produtovalor.Bonificacao = ColocaMascara(strBonificacao);
            produtovalor.VlrBonusVenda = ColocaMascara(strBonusVenda);
            produtovalor.VlrBonusAdicionalVenda = ColocaMascara(strBonusVendaAdicinal);
            produtovalor.ContaIDs = string.Empty;
            produtovalor.AssociacaoIDs = string.Empty;
            produtovalor.BlocoIDs = string.Empty;
            produtovalor.PaisIDs = string.Empty;
            produtovalor.ClassificacaoIDs = string.Empty;

            db.Entry(produtovalor).State = EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("Index");

            ViewBag.OpcaoID = new SelectList(db.ProdutoOpcao, "ID", "Nome", produtovalor.OpcaoID);

            return View(produtovalor);
        }

        // GET: ProdutoValores/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoValor produtoValor = db.ProdutoValor.Find(id);
            if (produtoValor == null)
            {
                return HttpNotFound();
            }
            return View(produtoValor);
        }

        // POST: ProdutoValores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProdutoValor produtoValor = db.ProdutoValor.Find(id);
            db.ProdutoValor.Remove(produtoValor);
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
    }
}
