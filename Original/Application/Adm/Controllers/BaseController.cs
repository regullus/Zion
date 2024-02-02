using Core.Entities;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Sistema.Controllers
{
    public class BaseController : Controller
    {
        protected int PageSizeDefault = 5;

        protected YLEVELEntities db;

        public BaseController()
        {
            db = new YLEVELEntities();

            Localizacao();
        }

        private void Localizacao()
        {
            var idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            var culture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        protected void ConfiguraPaginacaoDefaultViewBag()
        {
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", PageSizeDefault);
            ViewBag.PageSize = PageSizeDefault;
        }

    }
}