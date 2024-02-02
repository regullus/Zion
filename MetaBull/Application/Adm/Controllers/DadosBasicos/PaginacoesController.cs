
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
   public class PaginacoesController : Controller
   {
      #region Variaveis

      private YLEVELEntities db = new YLEVELEntities();

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
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }

      #endregion

      #region Actions
      // GET: Paginacao
      public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
      {
         Localizacao();

         //Verifica se a msg em popup para ser exibido na view
         obtemMensagem();

         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "Paginacao");
         objFuncoes = null;

         //List
         if (String.IsNullOrEmpty(SortOrder))
         {
            ViewBag.CurrentSort = "nome";
         }
         else
         {
            ViewBag.CurrentSort = SortOrder;
         }

         if (!(ProcuraNome != null))
         {
            if (ProcuraNome == null)
            {
               ProcuraNome = CurrentProcuraNome;
            }
         }

         ViewBag.CurrentProcuraNome = ProcuraNome;

         IQueryable<Paginacao> lista = null;
         lista = db.Paginacao;
         if (!String.IsNullOrEmpty(ProcuraNome))
         {
            lista = lista.Where(s => s.nome.Contains(ProcuraNome));
         }

         switch (SortOrder)
         {
            case "nome_desc":
               ViewBag.FirstSortParm = "nome";
               ViewBag.SecondSortParm = "nome";
               lista = lista.OrderByDescending(x => x.nome);
               break;
            case "nome":
               ViewBag.FirstSortParm = "nome";
               ViewBag.SecondSortParm = "Nome";
               lista = lista.OrderBy(x => x.nome);
               break;
            default:  // Name ascending 
               ViewBag.FirstSortParm = "nome_desc";
               ViewBag.SecondSortParm = "nome";
               lista = lista.OrderBy(x => x.nome);
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

      // GET: Paginacao/Details/5
      public ActionResult Details(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Paginacao Paginacao = db.Paginacao.Find(id);
         if (Paginacao == null)
         {
            return HttpNotFound();
         }
         return View(Paginacao);
      }

      // GET: Bloco/Create
      public ActionResult Create()
      {
         Localizacao();

         return View();
      }

      // POST: Bloco/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create([Bind(Include = "ID,atualizacao, nome, valor")] Paginacao Paginacao)
      {
         Localizacao();

         if (ModelState.IsValid)
         {
            db.Paginacao.Add(Paginacao);
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         return View(Paginacao);
      }

      // GET: Bloco/Edit/5
      public ActionResult Edit(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Paginacao Paginacao = db.Paginacao.Find(id);
         if (Paginacao == null)
         {
            return HttpNotFound();
         }
         return View(Paginacao);
      }

      // POST: Bloco/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit([Bind(Include = "ID,atualizacao, nome, valor")] Paginacao Paginacao)
      {
         Localizacao();

         if (ModelState.IsValid)
         {
            db.Entry(Paginacao).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }
         return View(Paginacao);
      }

      // GET: Bloco/Delete/5
      public ActionResult Delete(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Paginacao Paginacao = db.Paginacao.Find(id);
         if (Paginacao == null)
         {
            return HttpNotFound();
         }
         return View(Paginacao);
      }

      // POST: Bloco/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         Localizacao();

         Paginacao Paginacao = db.Paginacao.Find(id);
         db.Paginacao.Remove(Paginacao);
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
