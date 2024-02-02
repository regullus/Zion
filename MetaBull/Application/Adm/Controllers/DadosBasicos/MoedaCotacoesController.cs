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
    public class MoedaCotacoesController : Controller
    {
        #region Core

        public MoedaCotacoesController()
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
        public ActionResult Index(string SortOrder, string CurrentProcuraMoedaDestino, string ProcuraMoedaDestino, string CurrentProcuraMoedaOrigem, string ProcuraMoedaOrigem, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraMoedaDestino, ref ProcuraMoedaDestino, ref CurrentProcuraMoedaOrigem,
                                    ref ProcuraMoedaOrigem, ref NumeroPaginas, ref Page, "MoedaCotacoes");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "MoedaCotacoes";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraMoedaDestino != null || ProcuraMoedaOrigem != null))
            {
                if (ProcuraMoedaDestino == null)
                {
                    ProcuraMoedaDestino = CurrentProcuraMoedaDestino;
                }
                if (ProcuraMoedaOrigem == null)
                {
                    ProcuraMoedaOrigem = CurrentProcuraMoedaOrigem;
                }
            }

            ViewBag.CurrentProcuraMoedaDestino = ProcuraMoedaDestino;
            ViewBag.CurrentProcuraMoedaOrigem = ProcuraMoedaOrigem;

            IQueryable<MoedaCotacao> lista = null;

            lista = db.MoedaCotacao.Include(m => m.Moeda).Include(m => m.Moeda1).Include(m => m.MoedaCotacaoTipo);

            if (!String.IsNullOrEmpty(ProcuraMoedaDestino) && !String.IsNullOrEmpty(ProcuraMoedaOrigem))
            {
                lista = lista.Where(x => x.Moeda.Nome.Contains(ProcuraMoedaDestino) && x.Moeda1.Nome.Contains(ProcuraMoedaOrigem));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraMoedaDestino))
                {
                    lista = lista.Where(x => x.Moeda.Nome.Contains(ProcuraMoedaDestino));
                }
                if (!String.IsNullOrEmpty(ProcuraMoedaOrigem))
                {
                    lista = lista.Where(x => x.Moeda1.Nome.Contains(ProcuraMoedaOrigem));
                }
            }

            switch (SortOrder)
            {
                case "MoedaDestino_desc":
                    ViewBag.FirstSortParm = "MoedaDestino";
                    ViewBag.SecondSortParm = "MoedaOrigem";
                    lista = lista.OrderByDescending(x => x.Moeda.Nome);
                    break;
                case "MoedaOrigem":
                    ViewBag.FirstSortParm = "MoedaDestino";
                    ViewBag.SecondSortParm = "MoedaOrigem_desc";
                    lista = lista.OrderBy(x => x.Moeda1.Nome);
                    break;
                case "MoedaOrigem_desc":
                    ViewBag.FirstSortParm = "MoedaDestino";
                    ViewBag.SecondSortParm = "MoedaOrigem";

                    lista = lista.OrderByDescending(x => x.Moeda1.Nome);
                    break;
                case "MoedaDestino":
                    ViewBag.FirstSortParm = "MoedaDestino_desc";
                    ViewBag.SecondSortParm = "MoedaOrigem";

                    lista = lista.OrderBy(x => x.Moeda.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "MoedaDestino_desc";
                    ViewBag.SecondSortParm = "MoedaOrigem";
                    lista = lista.OrderBy(x => x.Moeda.Nome);
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
            MoedaCotacao MoedaCotacao = db.MoedaCotacao.Find(id);
            if (MoedaCotacao == null)
            {
                return HttpNotFound();
            }
            return View(MoedaCotacao);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            ViewBag.MoedaOrigemID = new SelectList(db.Moedas, "ID", "Nome");
            ViewBag.MoedaDestinoID = new SelectList(db.Moedas, "ID", "Simbolo");
            ViewBag.TipoID = new SelectList(db.MoedaCotacaoTipo, "ID", "Descricao");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID, MoedaOrigemID, MoedaDestinoID, Valor, Data ,TipoID")] MoedaCotacao MoedaCotacao)
        {
            Localizacao();

            MoedaCotacao.Data = DateTime.Today;

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (MoedaCotacao.Valor == 0)
            {
                msg.Add(traducaoHelper["VALOR"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA_COTACAO"], erro, "err");
            }
            else
            {
                db.MoedaCotacao.Add(MoedaCotacao);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.MoedaOrigemID = new SelectList(db.Moedas, "ID", "Nome");
            ViewBag.MoedaDestinoID = new SelectList(db.Moedas, "ID", "Simbolo");
            ViewBag.TipoID = new SelectList(db.MoedaCotacaoTipo, "ID", "Descricao");
            return View(MoedaCotacao);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoedaCotacao MoedaCotacao = db.MoedaCotacao.Find(id);
            if (MoedaCotacao == null)
            {
                return HttpNotFound();
            }
            ViewBag.MoedaOrigemID = new SelectList(db.Moedas, "ID", "Nome");
            ViewBag.MoedaDestinoID = new SelectList(db.Moedas, "ID", "Simbolo");
            ViewBag.TipoID = new SelectList(db.MoedaCotacaoTipo, "ID", "Descricao");
            return View(MoedaCotacao);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, MoedaOrigemID, MoedaDestinoID, Valor, Data ,TipoID")] MoedaCotacao MoedaCotacao)
        {
            Localizacao();

            MoedaCotacao.Data = DateTime.Today;

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (MoedaCotacao.Valor == 0)
            {
                msg.Add(traducaoHelper["VALOR"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["MOEDA_COTACAO"], erro, "err");
            }
            else
            {
                db.Entry(MoedaCotacao).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.MoedaOrigemID = new SelectList(db.Moedas, "ID", "Nome");
            ViewBag.MoedaDestinoID = new SelectList(db.Moedas, "ID", "Simbolo");
            ViewBag.TipoID = new SelectList(db.MoedaCotacaoTipo, "ID", "Descricao");
            return View(MoedaCotacao);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MoedaCotacao MoedaCotacao = db.MoedaCotacao.Find(id);
            if (MoedaCotacao == null)
            {
                return HttpNotFound();
            }
            return View(MoedaCotacao);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MoedaCotacao MoedaCotacao = db.MoedaCotacao.Find(id);
            db.MoedaCotacao.Remove(MoedaCotacao);
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
