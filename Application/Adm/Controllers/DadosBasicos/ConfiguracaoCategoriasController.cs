
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
   public class ConfiguracaoCategoriasController : Controller
   {
        #region Variaveis

      public ConfiguracaoCategoriasController()
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
      // GET: ConfiguracaoCategoria
      public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraEndereco, string ProcuraEndereco, int? NumeroPaginas, int? Page)
      {
         Localizacao();

         //Verifica se a msg em popup para ser exibido na view
         obtemMensagem();

         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraEndereco, ref ProcuraEndereco, ref NumeroPaginas, ref Page, "CategoriaConfiguração");
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

         IQueryable<ConfiguracaoCategoria> lista = null;
         lista = db.ConfiguracaoCategoria;
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

      // GET: ConfiguracaoCategoria/Details/5
      public ActionResult Details(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         ConfiguracaoCategoria configuracaoCategoria = db.ConfiguracaoCategoria.Find(id);
         if (configuracaoCategoria == null)
         {
            return HttpNotFound();
         }
         return View(configuracaoCategoria);
      }

      // GET: ConfiguracaoCategoria/Create
      public ActionResult Create()
      {
         Localizacao();

         return View();
      }

      // POST: ConfiguracaoCategoria/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create([Bind(Include = "ID,Nome")] ConfiguracaoCategoria configuracaoCategoria)
      {
         Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(configuracaoCategoria.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            

         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["CONFIGURACAO_CATEGORIA"], erro, "err");
         }
         else
         {
            db.ConfiguracaoCategoria.Add(configuracaoCategoria);
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(configuracaoCategoria);
      }

      // GET: ConfiguracaoTipos/Edit/5
      public ActionResult Edit(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         ConfiguracaoCategoria configuracaoCategoria = db.ConfiguracaoCategoria.Find(id);
         if (configuracaoCategoria == null)
         {
            return HttpNotFound();
         }
         return View(configuracaoCategoria);
      }

      // POST: ConfiguracaoCategoria/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit([Bind(Include = "ID,Nome")] ConfiguracaoCategoria configuracaoCategoria)
      {
         Localizacao();

         List<string> msg = new List<string>();
         msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);
                        
         if (string.IsNullOrEmpty(configuracaoCategoria.Nome))
         {
            msg.Add(traducaoHelper["NOME"]);
         }            

         if (msg.Count > 1)
         {
             string[] erro = msg.ToArray();
             Mensagem(traducaoHelper["CONFIGURACAO_CATEGORIA"], erro, "err");
         }
         else
         {
            db.Entry(configuracaoCategoria).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         obtemMensagem();

         return View(configuracaoCategoria);
      }

      // GET: ConfiguracaoCategoria/Delete/5
      public ActionResult Delete(int? id)
      {
         Localizacao();

         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         ConfiguracaoCategoria configuracaoCategoria = db.ConfiguracaoCategoria.Find(id);
         if (configuracaoCategoria == null)
         {
            return HttpNotFound();
         }
         return View(configuracaoCategoria);
      }

      // POST: ConfiguracaoCategoria/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         Localizacao();

         ConfiguracaoCategoria configuracaoCategoria = db.ConfiguracaoCategoria.Find(id);
         db.ConfiguracaoCategoria.Remove(configuracaoCategoria);
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

        public ActionResult Excel(string ProcuraNome)
        {
            //*Lista para montar excel          
            IQueryable<ConfiguracaoCategoria> lista = null;

            lista = db.ConfiguracaoCategoria.Include(p => p.Configuracao);

            if (!String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }

            }


            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["CONFIGURACAOCATEGORIA"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 1;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["CONFIGURACAO"]);

            //Range do Cabeçalho da planilha
            var objHeadLine = objWorkSheet.Range(objWorkSheet.Cell(2, 1).Address, objWorkSheet.Cell(2, intTotColumns).Address);

            //Formata cabeçalho do relatorio
            objHeadLine.Style.Font.Bold = true;
            objHeadLine.Style.Font.FontSize = 14;
            objHeadLine.Style.Font.FontColor = XLColor.White;
            objHeadLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            objHeadLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            objHeadLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
            objHeadLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;
            objHeadLine.Merge();
            objHeadLine.Value = strReportHeader;

            //Nome das Colunas 
            objWorkSheet.Cell(4, 1).Value = traducaoHelper["NOME"];
        

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(4, 1).Address, objWorkSheet.Cell(4, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;


            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                objWorkSheet.Cell(intLinha, 1).Value = objFor.Nome;
                

                if (intLinha % 2 == 0)
                {
                    //coloca cor nas linhas impares
                    var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                    dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(5, 1).Address, objWorkSheet.Cell(intTotRows + 4, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            objWorkSheet.Columns().AdjustToContents();

            // Preparação para o response
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"" + strReportHeader + ".xlsx\"");

            // Planilha vai para memoria
            using (MemoryStream memoryStream = new MemoryStream())
            {
                objWorkBook.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                memoryStream.Close();
            }
            //Planilha vai para download
            Response.End();

            //Retorna a lista
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
