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
    public class MoedasController : Controller
    {
        #region Core

        public MoedasController()
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
        // GET: INstituição
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraSigla, string ProcuraSigla, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraSigla, ref ProcuraSigla, ref NumeroPaginas, ref Page, "Moedas");

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

            if (!(ProcuraNome != null || ProcuraSigla != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraSigla == null)
                {
                    ProcuraSigla = CurrentProcuraSigla;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraSigla = ProcuraSigla;

            IQueryable<Moeda> lista = null;
            lista = db.Moedas;
            if (!String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(s => s.Nome.Contains(ProcuraNome));
                lista = lista.Where(s => s.Sigla.Contains(ProcuraSigla));
            }

            switch (SortOrder)
            {
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Sigla";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Sigla":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Sigla_desc";
                    lista = lista.OrderBy(x => x.Sigla);
                    break;
                case "Sigla_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Sigla";

                    lista = lista.OrderByDescending(x => x.Sigla);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Sigla";

                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Sigla";
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

        // GET: Instituição/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Moeda Moeda = db.Moedas.Find(id);
            if (Moeda == null)
            {
                return HttpNotFound();
            }
            return View(Moeda);
        }

        // GET: Instituicao/Create
        public ActionResult Create()
        {

            Moeda moeda = new Moeda();
            moeda.MascaraIn = "99999999,99";
            moeda.MascaraOut = "##,###,##0.00";

            return View(moeda);
        }

        // POST: Instituicao/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Sigla,Simbolo,Nome,MascaraIn,MascaraOut")] Moeda Moeda)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Moeda.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(Moeda.Sigla))
            {
                msg.Add(traducaoHelper["SIGLA"]);
            }
            if (string.IsNullOrEmpty(Moeda.Simbolo))
            {
                msg.Add(traducaoHelper["SIMBOLO"]);
            }
            if (string.IsNullOrEmpty(Moeda.MascaraIn))
            {
                msg.Add(traducaoHelper["MASCARA_IN"]);
            }
            if (string.IsNullOrEmpty(Moeda.MascaraOut))
            {
                msg.Add(traducaoHelper["MASCARA_OUT"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA"], erro, "err");
            }
            else
            {
                db.Moedas.Add(Moeda);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            return View(Moeda);
        }

        // GET: Instituição/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Moeda Moeda = db.Moedas.Find(id);
            if (Moeda == null)
            {
                return HttpNotFound();
            }
            return View(Moeda);
        }

        // POST: Instituição/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Sigla,Simbolo,Nome,MascaraIn,MascaraOut")] Moeda Moeda)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Moeda.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(Moeda.Sigla))
            {
                msg.Add(traducaoHelper["SIGLA"]);
            }
            if (string.IsNullOrEmpty(Moeda.Simbolo))
            {
                msg.Add(traducaoHelper["SIMBOLO"]);
            }
            if (string.IsNullOrEmpty(Moeda.MascaraIn))
            {
                msg.Add(traducaoHelper["MASCARA_IN"]);
            }
            if (string.IsNullOrEmpty(Moeda.MascaraOut))
            {
                msg.Add(traducaoHelper["MASCARA_OUT"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA"], erro, "err");
            }
            else
            {
                db.Entry(Moeda).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            return View(Moeda);
        }

        // GET: Instituição/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Moeda Moeda = db.Moedas.Find(id);
            if (Moeda == null)
            {
                return HttpNotFound();
            }
            return View(Moeda);
        }

        // POST: Instituicâo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Moeda Moeda = db.Moedas.Find(id);
            db.Moedas.Remove(Moeda);
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
