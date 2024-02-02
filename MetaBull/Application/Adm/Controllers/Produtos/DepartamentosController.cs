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
    public class DepartamentosController : Controller
    {
        #region Core

        public DepartamentosController()
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
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraOrdem, string ProcuraOrdem, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraOrdem,
                                    ref ProcuraOrdem, ref NumeroPaginas, ref Page, "Departamento");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Nome";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraNome != null || ProcuraOrdem != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraOrdem == null)
                {
                    ProcuraOrdem = CurrentProcuraOrdem;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraOrdem = ProcuraOrdem;

            IQueryable<Departamento> lista = null;
            lista = db.Departamento;

            if (!String.IsNullOrEmpty(ProcuraNome) && !String.IsNullOrEmpty(ProcuraOrdem))
            {
                lista = lista.Where(x => x.Nome.Contains(ProcuraNome) && x.Nome.Contains(ProcuraOrdem));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
                if (!String.IsNullOrEmpty(ProcuraOrdem))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraOrdem));
                }
            }

            switch (SortOrder)
            {
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Ordem";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Ordem":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Ordem";
                    lista = lista.OrderBy(x => x.Ordem);
                    break;
                case "Ordem_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Ordem";

                    lista = lista.OrderByDescending(x => x.Ordem);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Ordem";

                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Ordem";
                    lista = lista.OrderBy(x => x.Nome);
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
            Departamento Departamento = db.Departamento.Find(id);
            if (Departamento == null)
            {
                return HttpNotFound();
            }
            return View(Departamento);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            ViewBag.DepartamentoID = new SelectList(db.Departamento, "ID", "Nome");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nome,DEpartamentoID,Ordem,Disponivel")] Departamento departamento)
        {
            if (ModelState.IsValid)
            {
                db.Departamento.Add(departamento);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartamentoID = new SelectList(db.Departamento, "ID", "Nome", departamento.DepartamentoID);
            return View(departamento);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Departamento Departamento = db.Departamento.Find(id);
            if (Departamento == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartamentoID = new SelectList(db.Departamento, "ID", "Nome");
            return View(Departamento);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nome,DEpartamentoID,Ordem,Disponivel")] Departamento departamento)
        {
            if (ModelState.IsValid)
            {
                db.Entry(departamento).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartamentoID = new SelectList(db.Departamento, "ID", "Nome");
            return View(departamento);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Departamento Departamento = db.Departamento.Find(id);
            if (Departamento == null)
            {
                return HttpNotFound();
            }
            return View(Departamento);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Departamento Departamento = db.Departamento.Find(id);
            db.Departamento.Remove(Departamento);
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
