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
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ProdutoCategoriasController : Controller
    {
        #region Core

        public ProdutoCategoriasController()
        {
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
        // GET: Filiais
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
        {

            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "ProdutoCategoria");
            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "name";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraNome != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;

            IQueryable<ProdutoCategoria> lista = null;
            lista = db.ProdutoCategoria;
            if (!String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(s => s.Nome.Contains(ProcuraNome));
            }

            switch (SortOrder)
            {
                case "name_desc":
                    ViewBag.NameSortParm = "name";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderByDescending(s => s.Nome);
                    break;
                case "name":
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";

                    lista = lista.OrderBy(s => s.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderBy(s => s.Nome);
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

        }


        // GET: Filiais/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoCategoria ProdutoCategoria = db.ProdutoCategoria.Find(id);
            if (ProdutoCategoria == null)
            {
                return HttpNotFound();
            }
            return View(ProdutoCategoria);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            ViewBag.ProdutoCategoria = new SelectList(db.ProdutoCategoria, "ID", "Nome");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nome")] ProdutoCategoria ProdutoCategoria)
        {
            if (ModelState.IsValid)
            {
                db.ProdutoCategoria.Add(ProdutoCategoria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProdutoCategoria = new SelectList(db.ProdutoCategoria, "ID", "Nome", ProdutoCategoria.Nome);
            return View(ProdutoCategoria);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoCategoria ProdutoCategoria = db.ProdutoCategoria.Find(id);
            if (ProdutoCategoria == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProdutoCategoria = new SelectList(db.ProdutoCategoria, "ID", "Nome", ProdutoCategoria.Nome);
            return View(ProdutoCategoria);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nome")] ProdutoCategoria ProdutoCategoria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ProdutoCategoria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProdutoCategoria = new SelectList(db.ProdutoCategoria, "ID", "Nome", ProdutoCategoria.Nome);
            return View(ProdutoCategoria);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProdutoCategoria ProdutoCategoria = db.ProdutoCategoria.Find(id);
            if (ProdutoCategoria == null)
            {
                return HttpNotFound();
            }
            return View(ProdutoCategoria);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProdutoCategoria ProdutoCategoria = db.ProdutoCategoria.Find(id);
            db.ProdutoCategoria.Remove(ProdutoCategoria);
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
