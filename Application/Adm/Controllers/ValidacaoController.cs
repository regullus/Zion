using Core.Repositories.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema.Controllers
{
   [AllowAnonymous]
   [RequireHttps]
   public class ValidacaoController : Controller
   {
      public UsuarioRepository usuarioRepository;

      public ValidacaoController(DbContext context)
      {
         usuarioRepository = new UsuarioRepository(context);
      }

      public ContentResult Data(string idioma, string dataCheck)
      {
         DateTime _data;
         var retorno = DateTime.TryParse(dataCheck, new CultureInfo(idioma), DateTimeStyles.None, out _data) ? "true" : "false";
         return Content(retorno);
      }

      public ContentResult Data(string dataCheck)
      {
         DateTime _data;
         var retorno = DateTime.TryParse(dataCheck, new CultureInfo("pt-br"), DateTimeStyles.None, out _data) ? "true" : "false";
         return Content(retorno);
      }


      public ContentResult Login(string login)
      {
         login = login.ToLower();
         var retorno = usuarioRepository.GetByLogin(login) != null ? "false" : "true";
         return Content(retorno);
      }

      public ContentResult Email(string email)
      {
         var retorno = usuarioRepository.GetByEmail(email) != null ? "false" : "true";
         return Content(retorno);
      }


   }
}
