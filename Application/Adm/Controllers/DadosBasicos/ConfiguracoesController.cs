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
    public class ConfiguracoesController : Controller
    {
        #region Core

        public ConfiguracoesController()
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
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraChave, string ProcuraChave, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraChave,
                                    ref ProcuraChave, ref NumeroPaginas, ref Page, "Configuracoes");

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

            if (!(ProcuraNome != null || ProcuraChave != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraChave == null)
                {
                    ProcuraChave = CurrentProcuraChave;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraChave = ProcuraChave;

            IQueryable<Configuracao> lista = null;

            lista = db.Configuracoes.Include(c => c.ConfiguracaoCategoria).Include(c => c.ConfiguracaoTipo);

            if (!String.IsNullOrEmpty(ProcuraNome) && !String.IsNullOrEmpty(ProcuraChave))
            {
                lista = lista.Where(x => x.Nome.Contains(ProcuraNome) && x.Chave.Contains(ProcuraChave));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
                if (!String.IsNullOrEmpty(ProcuraChave))
                {
                    lista = lista.Where(x => x.Chave.Contains(ProcuraChave));
                }
            }

            switch (SortOrder)
            {
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Chave";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Chave":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Chave";
                    lista = lista.OrderBy(x => x.Chave);
                    break;
                case "Chave_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "Chave";

                    lista = lista.OrderByDescending(x => x.Chave);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Chave";

                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "Chave";
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
            Configuracao Configuracao = db.Configuracoes.Find(id);
            if (Configuracao == null)
            {
                return HttpNotFound();
            }
            return View(Configuracao);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            ViewBag.CategoriaID = new SelectList(db.ConfiguracaoCategoria, "ID", "Nome");
            ViewBag.TipoID = new SelectList(db.ConfiguracaoTipo, "ID", "Nome");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,CategoriaID,TipoID,Chave,Nome,Dados")] Configuracao Configuracao)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Configuracao.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(Configuracao.Chave))
            {
                msg.Add(traducaoHelper["CHAVE"]);
            }
            if (string.IsNullOrEmpty(Configuracao.Dados))
            {
                msg.Add(traducaoHelper["DADOS"]);
            }
            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CONFIGURACAO"], erro, "err");
            }
            else
            {
                db.Configuracoes.Add(Configuracao);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.CategoriaID = new SelectList(db.ConfiguracaoCategoria, "ID", "Nome", Configuracao.CategoriaID);
            ViewBag.TipoID = new SelectList(db.ConfiguracaoTipo, "ID", "Nome", Configuracao.TipoID);
            return View(Configuracao);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Configuracao Configuracao = db.Configuracoes.Find(id);
            if (Configuracao == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoriaID = new SelectList(db.ConfiguracaoCategoria, "ID", "Nome", Configuracao.CategoriaID);
            ViewBag.TipoID = new SelectList(db.ConfiguracaoTipo, "ID", "Nome", Configuracao.TipoID);
            return View(Configuracao);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,CategoriaID,TipoID,Chave,Nome,Dados")] Configuracao Configuracao)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(Configuracao.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(Configuracao.Chave))
            {
                msg.Add(traducaoHelper["CHAVE"]);
            }
            if (string.IsNullOrEmpty(Configuracao.Dados))
            {
                msg.Add(traducaoHelper["DADOS"]);
            }
            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CONFIGURACAO"], erro, "err");
            }
            else
            {
                db.Entry(Configuracao).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.CategoriaID = new SelectList(db.ConfiguracaoCategoria, "ID", "Nome", Configuracao.CategoriaID);
            ViewBag.TipoID = new SelectList(db.ConfiguracaoTipo, "ID", "Nome", Configuracao.TipoID);
            return View(Configuracao);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Configuracao Configuracao = db.Configuracoes.Find(id);
            if (Configuracao == null)
            {
                return HttpNotFound();
            }
            return View(Configuracao);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Configuracao Configuracao = db.Configuracoes.Find(id);
            db.Configuracoes.Remove(Configuracao);
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
