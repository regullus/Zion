﻿#region Bibliotecas

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
    public class FiliaisController : Controller
    {
        #region Core

        public FiliaisController()
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
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraNomeFantasia, string ProcuraNomeFantasia, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraNomeFantasia,
                                    ref ProcuraNomeFantasia, ref NumeroPaginas, ref Page, "Filiais");

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

            if (!(ProcuraNome != null || ProcuraNomeFantasia != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraNomeFantasia == null)
                {
                    ProcuraNomeFantasia = CurrentProcuraNomeFantasia;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraNomeFantasia = ProcuraNomeFantasia;

            IQueryable<Filial> lista = null;

            lista = db.Filial.Include(f => f.Cidade).Include(f => f.Estado);

            if (!String.IsNullOrEmpty(ProcuraNome) && !String.IsNullOrEmpty(ProcuraNomeFantasia))
            {
                lista = lista.Where(x => x.Nome.Contains(ProcuraNome) && x.NomeFantasia.Contains(ProcuraNomeFantasia));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
                if (!String.IsNullOrEmpty(ProcuraNomeFantasia))
                {
                    lista = lista.Where(x => x.NomeFantasia.Contains(ProcuraNomeFantasia));
                }
            }

            switch (SortOrder)
            {
                case "Nome_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "NomeFantasia";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "NomeFantasia":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "NomeFantasia_desc";
                    lista = lista.OrderBy(x => x.NomeFantasia);
                    break;
                case "NomeFantasia_desc":
                    ViewBag.FirstSortParm = "Nome";
                    ViewBag.SecondSortParm = "NomeFantasia";

                    lista = lista.OrderByDescending(x => x.NomeFantasia);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "NomeFantasia";

                    lista = lista.OrderBy(x => x.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "Nome_desc";
                    ViewBag.SecondSortParm = "NomeFantasia";
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
            Filial filial = db.Filial.Find(id);
            if (filial == null)
            {
                return HttpNotFound();
            }
            return View(filial);
        }

        // GET: Filiais/Create
        public ActionResult Create()
        {
            ViewBag.CidadeID = new SelectList(db.Cidades, "ID", "Nome");
            ViewBag.EstadoID = new SelectList(db.Estados, "ID", "Sigla");
            return View();
        }

        // POST: Filiais/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nome,NomeFantasia,EstadoID,CidadeID,Logradouro,Numero,Complemento,Distrito,CodigoPostal,Observacoes")] Filial filial)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(filial.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(filial.NomeFantasia))
            {
                msg.Add(traducaoHelper["NOME_FANTASIA"]);
            }
            if (string.IsNullOrEmpty(filial.Logradouro))
            {
                msg.Add(traducaoHelper["LOGRADOURO"]);
            }
            if (string.IsNullOrEmpty(filial.Numero))
            {
                msg.Add(traducaoHelper["NUMERO"]);
            }
            if (string.IsNullOrEmpty(filial.Complemento))
            {
                msg.Add(traducaoHelper["COMPLEMENTO"]);
            }
            if (string.IsNullOrEmpty(filial.Distrito))
            {
                msg.Add(traducaoHelper["DISTRITO"]);
            }
            if (string.IsNullOrEmpty(filial.CodigoPostal))
            {
                msg.Add(traducaoHelper["CODIGO_POSTAL"]);
            }
            if (string.IsNullOrEmpty(filial.Observacoes))
            {
                msg.Add(traducaoHelper["OBSERVACOES"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CATEGORIA"], erro, "err");
            }
            else
            {
                db.Filial.Add(filial);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.CidadeID = new SelectList(db.Cidades, "ID", "Nome", filial.CidadeID);
            ViewBag.EstadoID = new SelectList(db.Estados, "ID", "Sigla", filial.EstadoID);
            return View(filial);
        }

        // GET: Filiais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Filial filial = db.Filial.Find(id);
            if (filial == null)
            {
                return HttpNotFound();
            }
            ViewBag.CidadeID = new SelectList(db.Cidades, "ID", "Nome", filial.CidadeID);
            ViewBag.EstadoID = new SelectList(db.Estados, "ID", "Sigla", filial.EstadoID);
            return View(filial);
        }

        // POST: Filiais/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nome,NomeFantasia,EstadoID,CidadeID,Logradouro,Numero,Complemento,Distrito,CodigoPostal,Observacoes")] Filial filial)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(filial.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(filial.NomeFantasia))
            {
                msg.Add(traducaoHelper["NOME_FANTASIA"]);
            }
            if (string.IsNullOrEmpty(filial.Logradouro))
            {
                msg.Add(traducaoHelper["LOGRADOURO"]);
            }
            if (string.IsNullOrEmpty(filial.Numero))
            {
                msg.Add(traducaoHelper["NUMERO"]);
            }
            if (string.IsNullOrEmpty(filial.Complemento))
            {
                msg.Add(traducaoHelper["COMPLEMENTO"]);
            }
            if (string.IsNullOrEmpty(filial.Distrito))
            {
                msg.Add(traducaoHelper["DISTRITO"]);
            }
            if (string.IsNullOrEmpty(filial.CodigoPostal))
            {
                msg.Add(traducaoHelper["CODIGO_POSTAL"]);
            }
            if (string.IsNullOrEmpty(filial.Observacoes))
            {
                msg.Add(traducaoHelper["OBSERVACOES"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem(traducaoHelper["CATEGORIA"], erro, "err");
            }
            else
            {
                db.Entry(filial).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.CidadeID = new SelectList(db.Cidades, "ID", "Nome", filial.CidadeID);
            ViewBag.EstadoID = new SelectList(db.Estados, "ID", "Sigla", filial.EstadoID);
            return View(filial);
        }

        // GET: Filiais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Filial filial = db.Filial.Find(id);
            if (filial == null)
            {
                return HttpNotFound();
            }
            return View(filial);
        }

        // POST: Filiais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Filial filial = db.Filial.Find(id);
            db.Filial.Remove(filial);
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
