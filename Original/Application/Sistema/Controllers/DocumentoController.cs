#region Biblioteca

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Core.Repositories.Sistema;

#endregion

namespace Sistema.Controllers
{
   [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
   public class DocumentoController : SecurityController<Core.Entities.Documento>
   {

      #region Variaveis

      #endregion

      #region Core

      private ArquivoRepository arquivoRepository;

      public DocumentoController(DbContext context)
          : base(context)
      {
         arquivoRepository = new ArquivoRepository(context);
      }
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
      #endregion

      #region Actions

      public ActionResult Index()
      {
         ViewBag.Documentos = arquivoRepository.GetBySecao((int)cpUtilities.TipoArquivoSecao.Documento); 

         return View();
      }

      #endregion

   }
}
