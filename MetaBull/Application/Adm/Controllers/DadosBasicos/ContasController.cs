
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
    public class ContasController : Controller
    {
        #region Core

        public ContasController()
        {
            Localizacao();
        }

        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

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

        // GET: Contas
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "Contas");

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

            if (!(ProcuraNome != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;

            IQueryable<Conta> lista = null;

            lista = db.Contas.Include(f => f.Moeda);

            if (!String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
            }

            switch (SortOrder)
            {
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Nome";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Nome_desc";
                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Nome_desc";
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

        // GET: Contas/Details/5
        public ActionResult Details(int? id)
        {
            Localizacao();


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conta Conta = db.Contas.Find(id);
            if (Conta == null)
            {
                return HttpNotFound();
            }
            return View(Conta);
        }

        // GET: Contas/Create
        public ActionResult Create()
        {
            Localizacao();

            Conta conta = new Conta();
            conta.Ativo = true;
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Nome");

            return View(conta);
        }

        // POST: Contas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,MoedaID,Nome,Ativo,PermiteSaque")] Conta Conta)
        public ActionResult Create(FormCollection form)
        {
            Localizacao();

            Conta conta = new Conta();
            conta.Nome = form["Nome"];
            conta.MoedaID = int.Parse(form["MoedaID"]);
            conta.Ativo = form["Ativo"] == "on" ? true : false;
            conta.PermiteSaque = form["PermiteSaque"] == "on" ? true : false;


            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(conta.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CONTA"], erro, "err");
            }
            else
            {
                db.Contas.Add(conta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            //ViewBag.ContaID = new SelectList(db.Contas, "ID", "Nome", conta.Nome);
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Nome", conta.MoedaID);
            return View(conta);
        }

        // GET: contas/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conta Conta = db.Contas.Find(id);
            if (Conta == null)
            {
                return HttpNotFound();
            }
            //ViewBag.ContaID = new SelectList(db.Contas, "ID", "nome", Conta.Nome);
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Nome", Conta.MoedaID);
            return View(Conta);
        }

        // POST: Contas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ID,IdiomaID,nome,Ativo,PermiteSaque")] Conta Conta)
        public ActionResult Edit(FormCollection form)
        {
            Localizacao();

            Conta conta = new Conta();
            conta.ID = int.Parse(form["ID"]);
            conta.Nome = form["Nome"];
            conta.MoedaID = int.Parse(form["MoedaID"]);
            conta.Ativo = form["Ativo"] == "on" ? true : false;
            conta.PermiteSaque = form["PermiteSaque"] == "on" ? true : false;

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(conta.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CONTA"], erro, "err");
            }
            else
            {
                db.Entry(conta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            //ViewBag.ContaID = new SelectList(db.Contas, "ID", "nome", conta.Nome);
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Simbolo", conta.MoedaID);

            return View(conta);
        }

        // GET: Contas/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conta Conta = db.Contas.Find(id);
            if (Conta == null)
            {
                return HttpNotFound();
            }
            return View(Conta);
        }

        // POST: Contas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            Conta conta = db.Contas.Find(id);
            db.Contas.Remove(conta);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Reload()
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
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
        #endregion

    }
}
