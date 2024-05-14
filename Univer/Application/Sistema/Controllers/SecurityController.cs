using Core.Entities;
using Core.Repositories;
using DomainExtension.Entities;
using DomainExtension.Repositories;
using DomainExtension.Repositories.Interfaces;
using MvcExtension.Security.Filters;
using MvcExtension.Security.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Core.Repositories.Usuario;
using DomainExtension.Entities.Interfaces;
using Core.Repositories.Globalizacao;
using Helpers;
using Core.Helpers;

namespace Sistema.Controllers
{
    public abstract class SecurityController<T> : Controller where T : class, IPersistentEntity
    {
        protected IPersistentRepository<T> repository;
        protected Core.Helpers.TraducaoHelper traducaoHelper;

        private UsuarioRepository usuarioRepository;
        private IdiomaRepository idiomaRepository;
        public Usuario usuario;
        public List<Idioma> idiomas;
        public Containers.UsuarioContainer usuarioContainer;

        public SecurityController(DbContext context)
        {
            repository = new PersistentRepository<T>(context);
            usuarioRepository = new UsuarioRepository(context);
            idiomaRepository = new IdiomaRepository(context);
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if (idiomas != null)
            {
                idiomas = idiomaRepository.GetAll().ToList();
                ViewBag.Idiomas = idiomas;
            }
            
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                try
                {
                    usuario = usuarioRepository.Get(Local.idUsuario);
                    if (usuario != null)
                    {
                        var idioma = usuario.Pais.Idioma;
                        if (Session["Idioma"] != null)
                        {
                            idioma = (Idioma)Session["Idioma"];
                        }
                        traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
                        ViewBag.Idioma = idioma;
                        ViewBag.TraducaoHelper = traducaoHelper;

                        //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
                        var culture = new System.Globalization.CultureInfo("en-US");
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                    }
                    usuarioContainer = new Containers.UsuarioContainer(usuario);
                    ViewBag.Usuario = usuario;
                    ViewBag.UsuarioContainer = usuarioContainer;
                }
                catch (Exception)
                {
                    ViewBag.Usuario = null;
                    ViewBag.UsuarioContainer = null;
                }
            }
          
            return base.BeginExecuteCore(callback, state);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (usuario != null && usuario.Bloqueado)
            {
                filterContext.Result = RedirectToAction("Login", "Account", new
                {
                    strPopupTitle = traducaoHelper["LOGIN_BLOQUEADO_TITULO"],
                    strPopupMessage = traducaoHelper["LOGIN_BLOQUEADO"],
                    Sair = "true"
                });
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                if (traducaoHelper == null)
                {
                    traducaoHelper = Local.TraducaoHelper;
                }

                filterContext.Result = RedirectToAction("Login", "Account", new
                {
                    Sair = "true"
                });

            }
        }
    }
}