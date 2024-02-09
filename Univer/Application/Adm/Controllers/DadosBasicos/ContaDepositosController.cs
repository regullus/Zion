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
    public class ContaDepositosController : Controller
    {
        #region Core

        public ContaDepositosController()
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
        // GET: ContaDepositos
        public ActionResult Index(string SortOrder, string CurrentProcuraConta, string ProcuraConta, int? NumeroPaginas, int? Page)
        {
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraConta, ref ProcuraConta, ref NumeroPaginas, ref Page, "Traducoes");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Conta";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (ProcuraConta == null)
            {
                ProcuraConta = CurrentProcuraConta;
            }

            ViewBag.CurrentProcuraConat = ProcuraConta;

            IQueryable<ContaDeposito> lista = null;

            lista = db.ContaDeposito.Include(c => c.Usuario).Include(c => c.Instituicao).Include(c => c.TipoConta).Include(c => c.MeioPagamento);

            switch (SortOrder)
            {
                case "Conta_desc":
                    ViewBag.FirstSortParm = "Conta";
                    lista = lista.OrderByDescending(x => x.Conta);
                    break;
                case "Conta":
                    ViewBag.FirstSortParm = "Conta_desc";
                    lista = lista.OrderBy(x => x.Conta);
                    break;
                default:
                    ViewBag.FirstSortParm = "Conta_desc";
                    lista = lista.OrderBy(x => x.Conta);
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

        // GET: ContaDepositos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContaDeposito contaDeposito = db.ContaDeposito.Find(id);
            if (contaDeposito == null)
            {
                return HttpNotFound();
            }
            return View(contaDeposito);
        }

        // GET: ContaDepositos/Create
        public ActionResult Create()
        {
            ViewBag.IDUsuario = new SelectList(db.Usuarios, "ID", "Nome");
            ViewBag.IDInstituicao = new SelectList(db.Instituicao, "ID", "Descricao");
            ViewBag.IDTipoConta = new SelectList(db.TipoConta, "ID", "Descricao");
            ViewBag.IDMeioPagamento = new SelectList(db.MeioPagamento, "ID", "Descricao");
            return View();
        }

        // POST: ContaDepositos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,IDUsuario,IDTipoConta,IDInstituicao,Agencia,Conta,DigitoConta,ProprietarioConta,IdentificacaoProprietario,DataCriacao,CPF,CNPJ,IDMeioPagamento,Litecoin,Bitcoin")] ContaDeposito contaDeposito)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            contaDeposito.DataCriacao = DateTime.Today;

            if (string.IsNullOrEmpty(contaDeposito.Agencia))
            {
                msg.Add(traducaoHelper["AGENCIA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.Conta))
            {
                msg.Add(traducaoHelper["CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.DigitoConta))
            {
                msg.Add(traducaoHelper["DIGITO_CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.ProprietarioConta))
            {
                msg.Add(traducaoHelper["PROPRIETARIO_CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.IdentificacaoProprietario))
            {
                msg.Add(traducaoHelper["IDENTIFICACAO_PROPRIETARIO"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.CPF) && string.IsNullOrEmpty(contaDeposito.CNPJ))
            {
                msg.Add(traducaoHelper["CPF"] + " ou " + traducaoHelper["CNPJ"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem("ContaDeposito", erro, "err");
            }
            else
            {
                db.ContaDeposito.Add(contaDeposito);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.IDUsuario = new SelectList(db.Usuarios, "ID", "Nome", contaDeposito.IDUsuario);
            ViewBag.IDInstituicao = new SelectList(db.Instituicao, "ID", "Descricao", contaDeposito.IDInstituicao);
            ViewBag.IDTipoConta = new SelectList(db.TipoConta, "ID", "Descricao", contaDeposito.IDTipoConta);
            ViewBag.IDMeioPagamento = new SelectList(db.MeioPagamento, "ID", "Descricao", contaDeposito.IDMeioPagamento);
            return View(contaDeposito);
        }

        // GET: ContaDepositos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContaDeposito contaDeposito = db.ContaDeposito.Find(id);
            if (contaDeposito == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDUsuario = new SelectList(db.Usuarios, "ID", "Nome", contaDeposito.IDUsuario);
            ViewBag.IDInstituicao = new SelectList(db.Instituicao, "ID", "Descricao", contaDeposito.IDInstituicao);
            ViewBag.IDTipoConta = new SelectList(db.TipoConta, "ID", "Descricao", contaDeposito.IDTipoConta);
            ViewBag.IDMeioPagamento = new SelectList(db.MeioPagamento, "ID", "Descricao", contaDeposito.IDMeioPagamento);
            return View(contaDeposito);
        }

        // POST: ContaDepositos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,IDUsuario,IDTipoConta,IDInstituicao,Agencia,Conta,DigitoConta,ProprietarioConta,IdentificacaoProprietario,DataCriacao,CPF,CNPJ,IDMeioPagamento,Litecoin,Bitcoin")] ContaDeposito contaDeposito)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(contaDeposito.Agencia))
            {
                msg.Add(traducaoHelper["AGENCIA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.Conta))
            {
                msg.Add(traducaoHelper["CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.DigitoConta))
            {
                msg.Add(traducaoHelper["DIGITO_CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.ProprietarioConta))
            {
                msg.Add(traducaoHelper["PROPRIETARIO_CONTA"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.IdentificacaoProprietario))
            {
                msg.Add(traducaoHelper["IDENTIFICACAO_PROPRIETARIO"]);
            }

            if (string.IsNullOrEmpty(contaDeposito.CPF) && string.IsNullOrEmpty(contaDeposito.CNPJ))
            {
                msg.Add(traducaoHelper["CPF"] + " ou " + traducaoHelper["CNPJ"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem("ContaDeposito", erro, "err");
            }
            else
            {
                db.Entry(contaDeposito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.IDUsuario = new SelectList(db.Usuarios, "ID", "Nome", contaDeposito.IDUsuario);
            ViewBag.IDInstituicao = new SelectList(db.Instituicao, "ID", "Descricao", contaDeposito.IDInstituicao);
            ViewBag.IDTipoConta = new SelectList(db.TipoConta, "ID", "Descricao", contaDeposito.IDTipoConta);
            ViewBag.IDMeioPagamento = new SelectList(db.MeioPagamento, "ID", "Descricao", contaDeposito.IDMeioPagamento);
            return View(contaDeposito);
        }

        // GET: ContaDepositos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContaDeposito contaDeposito = db.ContaDeposito.Find(id);
            if (contaDeposito == null)
            {
                return HttpNotFound();
            }
            return View(contaDeposito);
        }

        // POST: ContaDepositos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ContaDeposito contaDeposito = db.ContaDeposito.Find(id);
            db.ContaDeposito.Remove(contaDeposito);
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
