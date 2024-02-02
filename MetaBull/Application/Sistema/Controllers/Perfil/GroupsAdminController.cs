
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

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity


//Models Local
using Sistema.Models;

//Lista
using PagedList;

//Excel
using System.Security.Claims;
using ClosedXML.Excel;
using System.IO;

//Utilities

using Helpers;

#endregion

namespace Sistema.Controllers
{
   [Authorize(Roles = "Master")]
   public class GroupsAdminController : Controller
   {

      #region Variaveis

      private ApplicationDbContext db = new ApplicationDbContext();

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

      #region Controllers
      private AutenticacaoGrupoManager _groupManager;
      public AutenticacaoGrupoManager GroupManager
      {
         get
         {
            return _groupManager ?? new AutenticacaoGrupoManager();
         }
         private set
         {
            _groupManager = value;
         }
      }

      private ApplicationRoleManager _roleManager;
      public ApplicationRoleManager RoleManager
      {
         get
         {
            return _roleManager ?? HttpContext.GetOwinContext()
                .Get<ApplicationRoleManager>();
         }
         private set
         {
            _roleManager = value;
         }
      }

      #endregion

      #region Actions

      public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, int? NumeroPaginas, int? Page)
      {
         //Verifica se a msg em popup para ser exibido na view
         obtemMensagem();

         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "GroupsAdmin");
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

         if (ProcuraNome == null)
         {
            if (ProcuraNome == null)
            {
               ProcuraNome = CurrentProcuraNome;
            }
         }

         ViewBag.CurrentProcuraNome = ProcuraNome;

         var lista = this.GroupManager.Groups;

         if (!String.IsNullOrEmpty(ProcuraNome))
         {
            lista = lista.Where(s => s.Name.Contains(ProcuraNome));
         }

         switch (SortOrder)
         {
            case "name_desc":
               ViewBag.NameSortParm = "name";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderByDescending(s => s.Name);
               break;
            case "name":
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";

               lista = lista.OrderBy(s => s.Name);
               break;
            default:  // Name ascending 
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderBy(s => s.Name);
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
         ViewBag.NumeroPaginas = new SelectList(new[]
                                 {
                                              new{valor=5,nome="5"},
                                              new{valor=10,nome="10"},
                                              new{valor=15,nome="15"},
                                              new{valor=20,nome="20"},
                                              new{valor=30,nome="30"},
                                              new{valor=40,nome="40"},
                                              new{valor=-1,nome="Todos"},
                                          },
                   "valor", "nome", intNumeroPaginas);

         return View(lista.ToPagedList(PageNumber, PageSize));

      }
      
      public async Task<ActionResult> Details(int id)
      {
         //if (id == null)
         //{
         //   return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         //}

         Sistema.Models.AutenticacaoGrupo AutenticacaoGrupo =
              await this.GroupManager.Groups.FirstOrDefaultAsync(g => g.Id == id);
         if (AutenticacaoGrupo == null)
         {
            return HttpNotFound();
         }
         var groupRoles = this.GroupManager.GetGroupRoles(AutenticacaoGrupo.Id);
         string[] RoleNames = groupRoles.Select(p => p.Name).ToArray();
         ViewBag.RolesList = RoleNames;
         ViewBag.RolesCount = RoleNames.Count();
         return View(AutenticacaoGrupo);
      }

      public ActionResult Create()
      {
         //Get a SelectList of Roles to choose from in the View:
         ViewBag.RolesList = new SelectList(
             this.RoleManager.Roles.ToList(), "Id", "Name");
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Create(
          [Bind(Include = "Name,Description")] Sistema.Models.AutenticacaoGrupo AutenticacaoGrupo, params string[] selectedRoles)
      {
         if (ModelState.IsValid)
         {
            // Create the new Group:
            var result = await this.GroupManager.CreateGroupAsync(AutenticacaoGrupo);
            if (result.Succeeded)
            {
               selectedRoles = selectedRoles ?? new string[] { };

               // Add the roles selected:
               await this.GroupManager.SetGroupRolesAsync(AutenticacaoGrupo.Id, selectedRoles);
            }
            return RedirectToAction("Index");
         }

         // Otherwise, start over:
         ViewBag.RoleId = new SelectList(
             this.RoleManager.Roles.ToList(), "Id", "Name");
         return View(AutenticacaoGrupo);
      }

      public async Task<ActionResult> Edit(int id)
      {
         if (id < 1)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Sistema.Models.AutenticacaoGrupo AutenticacaoGrupo = await this.GroupManager.FindByIdAsync(id);
         if (AutenticacaoGrupo == null)
         {
            return HttpNotFound();
         }

         // Get a list, not a DbSet or queryable:
         var allRoles = await this.RoleManager.Roles.ToListAsync();
         var groupRoles = await this.GroupManager.GetGroupRolesAsync(id);

         var model = new GroupViewModel()
         {
            Id = AutenticacaoGrupo.Id,
            Name = AutenticacaoGrupo.Name,
            Description = AutenticacaoGrupo.Description
         };

         // load the roles/Roles for selection in the form:
         foreach (var Role in allRoles)
         {
            var listItem = new SelectListItem()
            {
               Text = Role.Name,
               Value = Role.Id.ToString(),
               Selected = groupRoles.Any(g => g.Id == Role.Id)
            };
            model.RolesList.Add(listItem);
         }
         return View(model);
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Edit(
          [Bind(Include = "Id,Name,Description")] GroupViewModel model, params string[] selectedRoles)
      {
         var group = await this.GroupManager.FindByIdAsync(model.Id);
         if (group == null)
         {
            return HttpNotFound();
         }
         if (ModelState.IsValid)
         {
            group.Name = model.Name;
            group.Description = model.Description;
            await this.GroupManager.UpdateGroupAsync(group);

            selectedRoles = selectedRoles ?? new string[] { };
            await this.GroupManager.SetGroupRolesAsync(group.Id, selectedRoles);
            return RedirectToAction("Index");
         }
         return View(model);
      }

      public async Task<ActionResult> Delete(int id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Sistema.Models.AutenticacaoGrupo AutenticacaoGrupo = await this.GroupManager.FindByIdAsync(id);
         if (AutenticacaoGrupo == null)
         {
            return HttpNotFound();
         }
         return View(AutenticacaoGrupo);
      }

      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> DeleteConfirmed(int id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Sistema.Models.AutenticacaoGrupo AutenticacaoGrupo = await this.GroupManager.FindByIdAsync(id);
         await this.GroupManager.DeleteGroupAsync(id);
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
         //Persistencia dos paramentros da tela
         Funcoes objFuncoes = new Funcoes(this.HttpContext);
         objFuncoes.LimparPersistencia();
         objFuncoes = null;
         return RedirectToAction("Index");
      }

      public ActionResult Excel()
      {

         //*Lista para montar excel
         var lista = this.GroupManager.Groups;

         if (lista == null)
         {
            return HttpNotFound();
         }

         //*Titulo do relatorio
         string strReportHeader = "Grupo";

         //Total de linhas
         int intTotRows = lista.Count();

         //*Total de colunas - Verificar quantas colunas a planilha terá
         int intTotColumns = 3;

         XLWorkbook objWorkBook = new XLWorkbook();
         //XLWorkbook objWorkBook = new XLWorkbook(filepath);

         //*Nome da aba da planilha
         var objWorkSheet = objWorkBook.Worksheets.Add("Planos");

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
         objWorkSheet.Cell(4, 1).Value = "Código";
         objWorkSheet.Cell(4, 2).Value = "Nome";
         objWorkSheet.Cell(4, 3).Value = "Descrição";

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
            objWorkSheet.Cell(intLinha, 1).Value = objFor.Id;
            objWorkSheet.Cell(intLinha, 2).Value = objFor.Name;
            objWorkSheet.Cell(intLinha, 3).Value = objFor.Description;

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

      #endregion

   }
}
