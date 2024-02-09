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
    public class PaisesController : Controller
    {
        #region Core

        public PaisesController()
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
        // GET: Paises
        public ActionResult Index(string SortOrder, string CurrentProcuraSigla, string ProcuraSigla, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraSigla, ref ProcuraSigla, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "Paises");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Sigla";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraSigla != null || ProcuraNome != null))
            {
                if (ProcuraSigla == null)
                {
                    ProcuraSigla = CurrentProcuraSigla;
                }
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
            }

            ViewBag.CurrentProcuraSigla = ProcuraSigla;
            ViewBag.CurrentProcuraNome = ProcuraNome;

            IQueryable<Pais> lista = null;

            lista = db.Paises.Include(f => f.Bloco).Include(f => f.Idioma).Include(f => f.Moeda);

            if (!String.IsNullOrEmpty(ProcuraSigla) && !String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(x => x.Sigla.Contains(ProcuraSigla) && x.Nome.Contains(ProcuraNome));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraSigla))
                {
                    lista = lista.Where(x => x.Sigla.Contains(ProcuraSigla));
                }
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
            }

            switch (SortOrder)
            {
                case "Sigla_desc":
                    ViewBag.FirstSortParm = "Sigla";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderByDescending(x => x.Sigla);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Sigla";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderBy(x => x.Nome);
                    break;
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Sigla";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Sigla":
                    ViewBag.FirstSortParm = "Sigla_desc";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Sigla_desc";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderBy(x => x.Sigla);
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

        // GET: Paises/Details/5
        public ActionResult Details(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pais Pais = db.Paises.Find(id);
            if (Pais == null)
            {
                return HttpNotFound();
            }
            return View(Pais);
        }

        // GET: Paises/Create
        public ActionResult Create()
        {
            Localizacao();

            ViewBag.BlocoID = new SelectList(db.Blocos, "ID", "Nome");
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla");
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Simbolo");
            return View();
        }

        // POST: Paises/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,BlocoID,IdiomaID,MoedaID,Sigla,Nome,Padrao,Disponivel,MascaraTel,MascaraCel,MascaraCep")] Pais Pais)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Pais.Sigla))
            {
                msg.Add(traducaoHelper["SIGLA"]);
            }
            if (string.IsNullOrEmpty(Pais.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["PAIS"], erro, "err");
            }
            else
            {
                db.Paises.Add(Pais);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.BlocoID = new SelectList(db.Blocos, "ID", "Nome");
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla");
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Simbolo");
            return View(Pais);
        }

        // GET: Paises/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pais Pais = db.Paises.Find(id);
            if (Pais == null)
            {
                return HttpNotFound();
            }
            ViewBag.BlocoID = new SelectList(db.Blocos, "ID", "Nome", Pais.BlocoID);
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", Pais.IdiomaID);
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Simbolo", Pais.MoedaID);
            return View(Pais);
        }

        // POST: Paises/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,BlocoID,IdiomaID,MoedaID,Sigla,Nome,Padrao,Disponivel,MascaraTel,MascaraCel,MascaraCep")] Pais Pais)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Pais.Sigla))
            {
                msg.Add(traducaoHelper["SIGLA"]);
            }
            if (string.IsNullOrEmpty(Pais.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["PAIS"], erro, "err");
            }
            else
            {
                db.Entry(Pais).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.BlocoID = new SelectList(db.Blocos, "ID", "Nome", Pais.BlocoID);
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", Pais.IdiomaID);
            ViewBag.MoedaID = new SelectList(db.Moedas, "ID", "Simbolo", Pais.MoedaID);
            return View(Pais);
        }

        // GET: Paises/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pais Pais = db.Paises.Find(id);
            if (Pais == null)
            {
                return HttpNotFound();
            }
            return View(Pais);
        }

        // POST: Paises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            Pais Pais = db.Paises.Find(id);
            db.Paises.Remove(Pais);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            Localizacao();

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
        public JsonResult GetbyID(int paisID)
        {
            Object resultado = new object();

            Pais pais = db.Paises.Find(paisID);
            if (pais == null)
            {
                resultado = new {
                    codRet = "0"                
                };
            }

            resultado= new {
                codRet = "1",
                MascaraTel = pais.MascaraTel,
                MascaraCel = pais.MascaraCel,
                MascaraCep = pais.MascaraCep,              
            };

            return Json(resultado);
        }

        #endregion
    }
}
