
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
   public class BlocosController : Controller
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
         var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
         traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }

      #endregion

      #region Actions
      // GET: Blocos
      public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraEndereco, string ProcuraEndereco, int? NumeroPaginas, int? Page)
      {
         Localizacao();

         //Verifica se a msg em popup para ser exibido na view
         obtemMensagem();

         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraEndereco, ref ProcuraEndereco, ref NumeroPaginas, ref Page, "Bloco");
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

         if (!(ProcuraNome != null || ProcuraEndereco != null))
         {
            if (ProcuraNome == null)
            {
               ProcuraNome = CurrentProcuraNome;
            }
            if (ProcuraEndereco == null)
            {
               ProcuraEndereco = CurrentProcuraEndereco;
            }
         }

         ViewBag.CurrentProcuraNome = ProcuraNome;
         ViewBag.CurrentProcuraEndereco = ProcuraEndereco;

         IQueryable<Bloco> lista = null;
         lista = db.Blocos;
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

      // GET: Blocos/Details/5
      public ActionResult Details(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Bloco Bloco = db.Blocos.Find(id);
         if (Bloco == null)
         {
            return HttpNotFound();
         }
         return View(Bloco);
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
      public ActionResult Create([Bind(Include = "ID,Nome")] Bloco Bloco)
      {
        Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(Bloco.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            

         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["BLOCO"], erro, "err");
         }
         else
         {
            db.Blocos.Add(Bloco);
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(Bloco);
      }

      // GET: Bloco/Edit/5
      public ActionResult Edit(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Bloco Bloco = db.Blocos.Find(id);
         if (Bloco == null)
         {
            return HttpNotFound();
         }
         return View(Bloco);
      }

      // POST: Bloco/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit([Bind(Include = "ID,Nome")] Bloco Bloco)
      {
         Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(Bloco.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            

         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["BLOCO"], erro, "err");
         }
         else
         {
            db.Entry(Bloco).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(Bloco);
      }

      // GET: Bloco/Delete/5
      public ActionResult Delete(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Bloco Bloco = db.Blocos.Find(id);
         if (Bloco == null)
         {
            return HttpNotFound();
         }
         return View(Bloco);
      }

      // POST: Bloco/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         Localizacao();

         Bloco Bloco = db.Blocos.Find(id);
         db.Blocos.Remove(Bloco);
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
