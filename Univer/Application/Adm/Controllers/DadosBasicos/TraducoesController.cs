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
    public class TraducoesController : Controller
    {
        #region Core

        public TraducoesController()
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
        // GET: Filiais
        public ActionResult Index(string SortOrder, string CurrentProcuraChave, string ProcuraChave, string CurrentProcuraTexto, string ProcuraTexto, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraChave, ref ProcuraChave, ref CurrentProcuraTexto,
                                    ref ProcuraTexto, ref NumeroPaginas, ref Page, "Traducoes");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Chave";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraChave != null || ProcuraTexto != null))
            {
                if (ProcuraChave == null)
                {
                    ProcuraChave = CurrentProcuraChave;
                }
                if (ProcuraTexto == null)
                {
                    ProcuraTexto = CurrentProcuraTexto;
                }
            }

            ViewBag.CurrentProcuraChave = ProcuraChave;
            ViewBag.CurrentProcuraTexto = ProcuraTexto;

            IQueryable<Traducao> lista = null;

            lista = db.Traducoes.Include(t => t.Idioma).Include(t => t.TraducaoSecao);

            if (!String.IsNullOrEmpty(ProcuraChave) && !String.IsNullOrEmpty(ProcuraTexto))
            {
                lista = lista.Where(x => x.Chave.Contains(ProcuraChave) && x.Texto.Contains(ProcuraTexto));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraChave))
                {
                    lista = lista.Where(x => x.Chave.Contains(ProcuraChave));
                }
                if (!String.IsNullOrEmpty(ProcuraTexto))
                {
                    lista = lista.Where(x => x.Texto.Contains(ProcuraTexto));
                }
            }

            switch (SortOrder)
            {
                case "Chave_desc":
                    ViewBag.FirstSortParm = "Chave";
                    ViewBag.SecondSortParm = "Texto";
                    lista = lista.OrderByDescending(x => x.Chave);
                    break;
                case "Texto":
                    ViewBag.FirstSortParm = "Chave";
                    ViewBag.SecondSortParm = "Texto_desc";
                    lista = lista.OrderBy(x => x.Texto);
                    break;
                case "Texto_desc":
                    ViewBag.FirstSortParm = "Chave";
                    ViewBag.SecondSortParm = "Texto";

                    lista = lista.OrderByDescending(x => x.Texto);
                    break;
                case "Chave":
                    ViewBag.FirstSortParm = "Chave_desc";
                    ViewBag.SecondSortParm = "Texto";

                    lista = lista.OrderBy(x => x.Chave);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Chave_desc";
                    ViewBag.SecondSortParm = "Texto";
                    lista = lista.OrderBy(x => x.Chave);
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
            Traducao Traducao = db.Traducoes.Find(id);
            if (Traducao == null)
            {
                return HttpNotFound();
            }
            return View(Traducao);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            Localizacao();

            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", "Nome");
            ViewBag.SecaoID = new SelectList(db.TraducaoSecao, "ID", "Descricao");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID, IdiomaID, SecaoID, Chave, Texto, Descricao")] Traducao Traducao)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Traducao.Chave))
            {
                msg.Add(traducaoHelper["CHAVE"]);
            }
            if (string.IsNullOrEmpty(Traducao.Texto))
            {
                msg.Add(traducaoHelper["TEXTO"]);
            }
            if (string.IsNullOrEmpty(Traducao.Descricao))
            {
                msg.Add(traducaoHelper["DESCRICAO"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["TRADUCAO"], erro, "err");
            }
            else
            {
                db.Traducoes.Add(Traducao);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", Traducao.IdiomaID);
            ViewBag.SecaoID = new SelectList(db.TraducaoSecao, "ID", "Descricao", Traducao.TraducaoSecao);
            return View(Traducao);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Traducao Traducao = db.Traducoes.Find(id);
            if (Traducao == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", Traducao.IdiomaID);
            ViewBag.SecaoID = new SelectList(db.TraducaoSecao, "ID", "Descricao", Traducao.TraducaoSecao);
            return View(Traducao);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, IdiomaID, SecaoID, Chave, Texto, Descricao")] Traducao Traducao)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Traducao.Chave))
            {
                msg.Add(traducaoHelper["CHAVE"]);
            }
            if (string.IsNullOrEmpty(Traducao.Texto))
            {
                msg.Add(traducaoHelper["TEXTO"]);
            }
            if (string.IsNullOrEmpty(Traducao.Descricao))
            {
                msg.Add(traducaoHelper["DESCRICAO"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["TRADUCAO"], erro, "err");
            }
            else
            {
                db.Entry(Traducao).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Sigla", Traducao.IdiomaID);
            ViewBag.SecaoID = new SelectList(db.TraducaoSecao, "ID", "Descricao", Traducao.TraducaoSecao);
            return View(Traducao);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Traducao Traducao = db.Traducoes.Find(id);
            if (Traducao == null)
            {
                return HttpNotFound();
            }
            return View(Traducao);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Traducao Traducao = db.Traducoes.Find(id);
            db.Traducoes.Remove(Traducao);
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
