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
    public class MoedaCotacaoTiposController : Controller
    {
        #region Core

        public MoedaCotacaoTiposController()
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
        public ActionResult Index(string SortOrder, string CurrentProcuraDescricao, string ProcuraDescricao, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraDescricao, ref ProcuraDescricao, ref NumeroPaginas, ref Page, "MoedaCotacaoTipos");
            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Descricao";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraDescricao != null))
            {
                if (ProcuraDescricao == null)
                {
                    ProcuraDescricao = CurrentProcuraDescricao;
                }
            }

            ViewBag.CurrentProcuraDescricao = ProcuraDescricao;

            IQueryable<MoedaCotacaoTipo> lista = null;
            lista = db.MoedaCotacaoTipo;
            if (!String.IsNullOrEmpty(ProcuraDescricao))
            {
                lista = lista.Where(s => s.Descricao.Contains(ProcuraDescricao));
            }

            switch (SortOrder)
            {
                case "Descricao_desc":
                    ViewBag.NameSortParm = "Descricao";
                    lista = lista.OrderByDescending(s => s.Descricao);
                    break;
                case "Descricao":
                    ViewBag.NameSortParm = "Descricao_desc";

                    lista = lista.OrderBy(s => s.Descricao);
                    break;
                default:  // Name ascending 
                    ViewBag.NameSortParm = "Descricao_desc";
                    lista = lista.OrderBy(s => s.Descricao);
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
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoedaCotacaoTipo MoedaCotacaoTipo = db.MoedaCotacaoTipo.Find(id);
            if (MoedaCotacaoTipo == null)
            {
                return HttpNotFound();
            }
            return View(MoedaCotacaoTipo);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            Localizacao();

            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Descricao")] MoedaCotacaoTipo MoedaCotacaoTipo)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(MoedaCotacaoTipo.Descricao))
            {
                msg.Add(traducaoHelper["DESCRICAO"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA_COTACAO_TIPO"], erro, "err");
            }
            else
            {
                db.MoedaCotacaoTipo.Add(MoedaCotacaoTipo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            return View(MoedaCotacaoTipo);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoedaCotacaoTipo MoedaCotacaoTipo = db.MoedaCotacaoTipo.Find(id);
            if (MoedaCotacaoTipo == null)
            {
                return HttpNotFound();
            }
            return View(MoedaCotacaoTipo);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Descricao")] MoedaCotacaoTipo MoedaCotacaoTipo)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(MoedaCotacaoTipo.Descricao))
            {
                msg.Add(traducaoHelper["DESCRICAO"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA_COTACAO_TIPO"], erro, "err");
            }
            else
            {
                db.Entry(MoedaCotacaoTipo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            return View(MoedaCotacaoTipo);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoedaCotacaoTipo MoedaCotacaoTipo = db.MoedaCotacaoTipo.Find(id);
            if (MoedaCotacaoTipo == null)
            {
                return HttpNotFound();
            }
            return View(MoedaCotacaoTipo);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            MoedaCotacaoTipo MoedaCotacaoTipo = db.MoedaCotacaoTipo.Find(id);
            db.MoedaCotacaoTipo.Remove(MoedaCotacaoTipo);
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
    }
}
