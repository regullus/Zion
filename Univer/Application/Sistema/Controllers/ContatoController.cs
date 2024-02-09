#region Bibliotecas

using System;
using System.Data.Entity;
using System.Web.Mvc;

#endregion

namespace Sistema.Controllers
{
    public class ContatoController : Controller
    {

      #region Variaveis

      #endregion

      #region Core

      public ContatoController(DbContext context)
        {
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

      [HttpPost]
        public ActionResult Enviar(string nome, string email, string telefone, string mensagem)
        {
            //ToDo - Refazer
            //var corpo = String.Format("Nome: {0}\nEmail: {1}\nTelefone: {2}\nMensagem:\n{3}\n", nome, email, telefone, mensagem);
            //var emailService = new Core.Services.Sistema.EmailService();
            //emailService.Send(email, "suporte@XXXXXX.com.br", "Contato Site", corpo, false);
            return Content("OK");
        }

      #endregion

   }
}
