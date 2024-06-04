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
using Core.Repositories.Usuario;

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
using System.Web.Script.Serialization;

#endregion

namespace Sistema.Controllers
{
    public class AvisosController : Controller
    {
        #region Variaveis

        #endregion

        #region Core

        private Core.Helpers.TraducaoHelper traducaoHelper;

        private UsuarioRepository usuarioRepository;

        public AvisosController(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            Localizacao();
        }

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
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Actions

        public ActionResult Index(string SortOrder, string CurrentProcuraTitulo, string ProcuraTitulo, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraTitulo, ref ProcuraTitulo, ref NumeroPaginas, ref Page, "Aviso");
            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "Titulo";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraTitulo != null))
            {
                if (ProcuraTitulo == null)
                {
                    ProcuraTitulo = CurrentProcuraTitulo;
                }
            }

            ViewBag.CurrentProcuraTitulo = ProcuraTitulo;

            IQueryable<Aviso> lista = null;
            lista = db.Aviso;
            if (!String.IsNullOrEmpty(ProcuraTitulo))
            {
                lista = lista.Where(s => s.Titulo.Contains(ProcuraTitulo));
            }

            switch (SortOrder)
            {
                case "Titulo_desc":
                    ViewBag.NameSortParm = "Titulo";
                    lista = lista.OrderByDescending(s => s.Titulo);
                    break;
                case "Titulo":
                    ViewBag.NameSortParm = "Titulo_desc";

                    lista = lista.OrderBy(s => s.Titulo);
                    break;
                default:  // Name ascending 
                    ViewBag.NameSortParm = "Titulo_desc";
                    lista = lista.OrderBy(s => s.Titulo);
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

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aviso aviso = db.Aviso.Find(id.Value);
            if (aviso == null)
            {
                return HttpNotFound();
            }

            SetViewBagComUsuariosJson(aviso);


            return View(aviso);
        }

        private void SetViewBag()
        {
            ViewBag.TipoID = new SelectList(db.AvisoTipo.Where(x => x.ID > 1), "ID", "Nome");
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Nome");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind] Aviso aviso)
        {
            // (Include = "ID,Titulo,Texto,TipoID,UsuarioID,IdiomaID,Login,Nome,Email,Atualizacao,DataDivulgacao,Urgente")
            if (ModelState.IsValid)
            {
                try
                {
                    aviso.Atualizacao = Core.Helpers.App.DateTimeZion;

                    if (aviso.ID.Equals(0))
                    {
                        db.Aviso.Add(aviso);
                    }
                    else
                    {
                        db.Entry(aviso).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException dbEx)
                {
                    string strRetErro = "";
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            strRetErro += validationError.PropertyName + ": " + validationError.ErrorMessage + " ";
                        }
                    }
                    string[] strMensagem = new string[] { strRetErro };
                    Mensagem(" " + traducaoHelper["ERRO"] + " ", strMensagem, "err");
                }
                catch (Exception ex)
                {
                    string[] strMensagem = new string[] { ex.Message };
                    Mensagem(" " + traducaoHelper["ERRO"] + " ", strMensagem, "err");
                }
            }

            obtemMensagem();

            ViewBag.TipoID = new SelectList(db.AvisoTipo.Where(x => x.ID > 1), "ID", "Nome");
            ViewBag.IdiomaID = new SelectList(db.Idiomas, "ID", "Nome");

            return View(aviso);
        }

        public ActionResult Create(int? id)
        {
            this.SetViewBag();

            Aviso aviso = new Aviso();

            if (id.HasValue)
            {
                aviso = db.Aviso.FirstOrDefault(f => f.ID == id.Value);

                if (aviso == null)
                {
                    return HttpNotFound();
                }

                SetViewBagComUsuariosJson(aviso);

            }

            return View("create", aviso);
        }

        private void SetViewBagComUsuariosJson(Core.Entities.Aviso aviso)
        {
            var usuariosIDs = new List<ItemViewModel>();

            if (aviso.UsuarioIDs != null)
            {
                var ids = aviso.UsuarioIDs.Split(',');

                foreach (var item in ids.Where(w => !string.IsNullOrEmpty(w)))
                {
                    var usuario = db.Usuarios.Find(int.Parse(item));

                    usuariosIDs.Add(new ItemViewModel { id = usuario.ID, Name = usuario.Login });
                }
            }

            if (usuariosIDs.Any())
            {
                ViewBag.UsuariosIDs = usuariosIDs;
                ViewBag.UsuariosJson = new JavaScriptSerializer().Serialize(usuariosIDs);
            }
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aviso aviso = db.Aviso.Find(id);
            if (aviso == null)
            {
                return HttpNotFound();
            }
            return View(aviso);
        }

        public ActionResult Alterar(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int avisoID = int.Parse(id);

            AvisoLido avisoLido = db.AvisoLido.Where(x => x.AvisoID == avisoID).FirstOrDefault();
            if (avisoLido == null)
            {
                avisoLido = new AvisoLido();
                avisoLido.AvisoID = avisoID;
                avisoLido.DataLeitura = Core.Helpers.App.DateTimeZion;
                avisoLido.AvisoExcluido = false;
                avisoLido.UsuarioID = Helpers.Local.idUsuario;

                db.AvisoLido.Add(avisoLido);
            }
            else
            {
                db.AvisoLido.Remove(avisoLido);
                db.SaveChanges();
            }

            db.SaveChanges();

            return Json("OK");
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Aviso aviso = db.Aviso.Find(id);
            db.Aviso.Remove(aviso);
            db.SaveChanges();
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
            Localizacao();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("Index");
        }
        #endregion

        #region JsonResult
        public JsonResult GetUsuarios(string search)
        {
            IQueryable<Usuario> usuarios = usuarioRepository.GetByExpression(x => x.Login.Contains(search));
            return Json(usuarios.Select(s => new { id = s.ID, text = s.Login }).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
