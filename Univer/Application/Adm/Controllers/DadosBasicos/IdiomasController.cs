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
   public class IdiomasController : Controller
   {
      #region Core

      public IdiomasController()
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

      #region Actions
      // GET: Idiomas
      public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraSigla, string ProcuraSigla, int? NumeroPaginas, int? Page)
      {

         //Verifica se a msg em popup para ser exibido na view
         obtemMensagem();

         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraSigla, ref ProcuraSigla, ref NumeroPaginas, ref Page, "NOME");
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
         ViewBag.CurrentProcuraEndereco = ProcuraSigla;

         IQueryable<Idioma> lista = null;
         lista = db.Idiomas;
         if (!String.IsNullOrEmpty(ProcuraNome))
         {
            lista = lista.Where(s => s.Nome.Contains(ProcuraNome));
         }

         switch (SortOrder)
         {
            case "name_desc":
               ViewBag.NameSortParm = "name";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderByDescending(s => s.Nome);
               break;
            case "name":
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";

               lista = lista.OrderBy(s => s.Nome);
               break;
            default:  // Name ascending 
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderBy(s => s.Nome);
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

      // GET: Idiomas/Details/5
      public ActionResult Details(int? id)
      {

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Idioma Idioma = db.Idiomas.Find(id);
         if (Idioma == null)
         {
            return HttpNotFound();
         }
         return View(Idioma);
      }

      // GET: Idiomas/Create
      public ActionResult Create()
      {

         return View();
      }

      // POST: Idiomas/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create([Bind(Include = "ID,Sigla,Nome")] Idioma Idioma)
      {
         Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(Idioma.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            
         if (string.IsNullOrEmpty(Idioma.Sigla))
         {
            msg.Add(traducaoHelper["SIGLA"]);
         }  
         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["IDIOMA"], erro, "err");
         }
         else
         {
            db.Idiomas.Add(Idioma);
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(Idioma);
      }

      // GET: Idioma/Edit/5
      public ActionResult Edit(int? id)
      {

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Idioma Idioma = db.Idiomas.Find(id);
         if (Idioma == null)
         {
            return HttpNotFound();
         }
         return View(Idioma);
      }

      // POST: Idiomas/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit([Bind(Include = "ID,Sigla,Nome")] Idioma Idioma)
      {
         Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(Idioma.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            
         if (string.IsNullOrEmpty(Idioma.Sigla))
         {
            msg.Add(traducaoHelper["SIGLA"]);
         }  
         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["IDIOMA"], erro, "err");
         }
         else
         {
            db.Entry(Idioma).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(Idioma);
      }

      // GET: Idioma/Delete/5
      public ActionResult Delete(int? id)
      {

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Idioma Idioma = db.Idiomas.Find(id);
         if (Idioma == null)
         {
            return HttpNotFound();
         }
         return View(Idioma);
      }

      // POST: Idioma/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {

         Idioma Idioma = db.Idiomas.Find(id);
         db.Idiomas.Remove(Idioma);
         db.SaveChanges();
         return RedirectToAction("Index");
      }

      public ActionResult Reload()
      {

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

   }
}
