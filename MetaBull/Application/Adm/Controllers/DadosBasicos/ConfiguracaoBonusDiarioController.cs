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

namespace Sistema.Controllers.DadosBasicos
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ConfiguracaoBonusDiarioController : Controller
    {
        #region Variaveis

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
        // GET: ConfiguracaoBonusDiario
        public ActionResult Index(string SortOrder, string CurrentProcuraData, string ProcuraData, string CurrentProcuraValor, string ProcuraValor, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraData, ref ProcuraData, ref CurrentProcuraValor, ref ProcuraValor, ref NumeroPaginas, ref Page, "NOME");
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

            if (!(ProcuraData != null || ProcuraValor != null))
            {
                if (ProcuraData == null)
                {
                    ProcuraData = CurrentProcuraData;
                }
                if (ProcuraValor == null)
                {
                    ProcuraValor = CurrentProcuraValor;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraData;
            ViewBag.CurrentProcuraEndereco = ProcuraValor;

            IQueryable<ConfiguracaoBonusDiario> lista = null;
            lista = db.ConfiguracaoBonusDiario;
            if (!String.IsNullOrEmpty(ProcuraData))
            {
                var data = Convert.ToDateTime(ProcuraData);
                lista = lista.Where(s => s.DataReferencia.Equals(data));
            }

            switch (SortOrder)
            {
                case "name_desc":
                    ViewBag.NameSortParm = "name";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderByDescending(s => s.DataReferencia);
                    break;
                case "name":
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";

                    lista = lista.OrderBy(s => s.DataReferencia);
                    break;
                default:  // Name ascending 
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderBy(s => s.DataReferencia);
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

        // GET: ConfiguracaoBonusDiario/Details/5
        public ActionResult Details(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracaoBonusDiario config = db.ConfiguracaoBonusDiario.Find(id);
            if (config == null)
            {
                return HttpNotFound();
            }
            return View(config);
        }

        // GET: ConfiguracaoBonusDiario/Create
        public ActionResult Create()
        {
            ViewBag.Associacoes = db.Associacao;

            Localizacao();
            return View();
        }

        // POST: ConfiguracaoBonusDiario/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind] ConfiguracaoBonusDiario configuracaoBonusDiario)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (configuracaoBonusDiario.DataReferencia == DateTime.MinValue)
            {
                msg.Add(traducaoHelper["DATA_REFERENCIA"]);
            }

            if (configuracaoBonusDiario.Valor == 0)
            {
                msg.Add(traducaoHelper["VALOR"] + " BTC");
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["BONUS_DIARIO"], erro, "err");
            }
            else
            {
                if (configuracaoBonusDiario.AssociacaoID.Equals(0))
                {
                    configuracaoBonusDiario.AssociacaoID = null;
                }

                db.ConfiguracaoBonusDiario.Add(configuracaoBonusDiario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.Associacoes = db.Associacao;

            return View(configuracaoBonusDiario);
        }

        // GET: ConfiguracaoBonusDiario/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Associacoes = db.Associacao;

            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracaoBonusDiario configuracaoBonusDiario = db.ConfiguracaoBonusDiario.Find(id);
            if (configuracaoBonusDiario == null)
            {
                return HttpNotFound();
            }

            return View(configuracaoBonusDiario);
        }

        // POST: ConfiguracaoBonusDiario/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind] ConfiguracaoBonusDiario configuracaoBonusDiario)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (configuracaoBonusDiario.DataReferencia == DateTime.MinValue)
            {
                msg.Add(traducaoHelper["DATA_REFERENCIA"]);
            }

            if (configuracaoBonusDiario.Valor == 0)
            {
                msg.Add(traducaoHelper["VALOR"] + " BTC");
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["BONUS_DIARIO"], erro, "err");
            }
            else
            {
                db.Entry(configuracaoBonusDiario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();


            ViewBag.TipoID = new SelectList(db.ConfiguracaoBonusDiario, "ID", "DataReferencia", configuracaoBonusDiario.DataReferencia);
            return View(configuracaoBonusDiario);
        }

        // GET: ConfiguracaoBonusDiario/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracaoBonusDiario configuracaoBonusDiario = db.ConfiguracaoBonusDiario.Find(id);
            if (configuracaoBonusDiario == null)
            {
                return HttpNotFound();
            }
            return View(configuracaoBonusDiario);
        }

        // POST: ConfiguracaoBonusDiario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            ConfiguracaoBonusDiario configuracaoBonusDiario = db.ConfiguracaoBonusDiario.Find(id);
            db.ConfiguracaoBonusDiario.Remove(configuracaoBonusDiario);
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
            Localizacao();

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}

