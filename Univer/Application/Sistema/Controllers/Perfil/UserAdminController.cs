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
   public class UsersAdminController : Controller
   {
  
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

      #region Controlers

      public UsersAdminController()
      {
      }

      public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
      {
         UserManager = userManager;
         RoleManager = roleManager;
      }

      private ApplicationUserManager _userManager;

      public ApplicationUserManager UserManager
      {
         get
         {
            return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
         }
         private set
         {
            _userManager = value;
         }
      }

      // Add the Group Manager (NOTE: only access through the public
      // Property, not by the instance variable!)
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
            return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
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
         objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref NumeroPaginas, ref Page, "UserAdmin");
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

         var lista = UserManager.Users;

         if (!String.IsNullOrEmpty(ProcuraNome))
         {
            lista = lista.Where(s => s.UserName.Contains(ProcuraNome));
         }

         switch (SortOrder)
         {
            case "name_desc":
               ViewBag.NameSortParm = "name";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderByDescending(s => s.UserName);
               break;
            case "name":
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";

               lista = lista.OrderBy(s => s.UserName);
               break;
            default:  // Name ascending 
               ViewBag.NameSortParm = "name_desc";
               ViewBag.DateSortParm = "date";
               lista = lista.OrderBy(s => s.UserName);
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
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         var user = await UserManager.FindByIdAsync(id);

         // Show the groups the user belongs to:
         var userGroups = await this.GroupManager.GetUserGroupsAsync(id);
         ViewBag.GroupNames = userGroups.Select(u => u.Name).ToList();
         return View(user);
      }
      
      public ActionResult Create()
      {
         // Show a list of available groups:
         ViewBag.GroupsList = new SelectList(this.GroupManager.Groups, "Id", "Name");
         return View();
      }
      
      [HttpPost]
      public async Task<ActionResult> Create(RegisterViewModel userViewModel, params int[] selectedGroups)
      {
         if (ModelState.IsValid)
         {
            var user = new ApplicationUser
            {
               UserName = userViewModel.Login,
               Email = userViewModel.Email
            };
            var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

            //Add User to the selected Groups 
            if (adminresult.Succeeded)
            {
               if (selectedGroups != null)
               {
                  selectedGroups = selectedGroups ?? new int[] { };
                  await this.GroupManager
                      .SetUserGroupsAsync(user.Id, selectedGroups);
               }
               return RedirectToAction("Index");
            }
         }
         ViewBag.Groups = new SelectList(await RoleManager.Roles.ToListAsync(), "Id", "Name");

         return View();
      }
      
      public async Task<ActionResult> Edit(int id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         var user = await UserManager.FindByIdAsync(id);
         if (user == null)
         {
            return HttpNotFound();
         }

         // Display a list of available Groups:
         var allGroups = this.GroupManager.Groups;
         var userGroups = await this.GroupManager.GetUserGroupsAsync(id);

         var model = new EditUserViewModel()
         {
            Id = user.Id,
            Email = user.Email
         };

         foreach (var group in allGroups)
         {
            var listItem = new SelectListItem()
            {
               Text = group.Name,
               Value = group.Id.ToString(),
               Selected = userGroups.Any(g => g.Id == group.Id)
            };
            model.GroupsList.Add(listItem);
         }
         return View(model);
      }
      
      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Edit(
          [Bind(Include = "Email,Id")] EditUserViewModel editUser,
          params int[] selectedGroups)
      {
         if (ModelState.IsValid)
         {
            var user = await UserManager.FindByIdAsync(editUser.Id);
            if (user == null)
            {
               return HttpNotFound();
            }

            // Update the User:
            user.UserName = editUser.Email;
            user.Email = editUser.Email;
            await this.UserManager.UpdateAsync(user);

            // Update the Groups:
            selectedGroups = selectedGroups ?? new int[] { };
            await this.GroupManager.SetUserGroupsAsync(user.Id, selectedGroups);
            return RedirectToAction("Index");
         }
         ModelState.AddModelError("", "Something failed.");
         return View();
      }
      
      public async Task<ActionResult> Delete(int id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         var user = await UserManager.FindByIdAsync(id);
         if (user == null)
         {
            return HttpNotFound();
         }
         return View(user);
      }
      
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> DeleteConfirmed(int id)
      {
         if (ModelState.IsValid)
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
               return HttpNotFound();
            }

            // Remove all the User Group references:
            await this.GroupManager.ClearUserGroupsAsync(id);

            // Then Delete the User:
            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
               ModelState.AddModelError("", result.Errors.First());
               return View();
            }
            return RedirectToAction("Index");
         }
         return View();
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
         var lista = UserManager.Users;

         if (lista == null)
         {
            return HttpNotFound();
         }

         //*Titulo do relatorio
         string strReportHeader = "Usuário";

         //Total de linhas
         int intTotRows = lista.Count();

         //*Total de colunas - Verificar quantas colunas a planilha terá
         int intTotColumns = 2;

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
            objWorkSheet.Cell(intLinha, 2).Value = objFor.UserName;

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
