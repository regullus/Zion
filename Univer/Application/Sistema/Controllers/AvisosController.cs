#region Bibliotecas

using Core.Entities;
using Core.Repositories.Usuario;
using Sistema.Containers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using PagedList;
using Helpers;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class AvisosController : SecurityController<Core.Entities.Aviso>
    {

        #region Variaveis

        #endregion

        #region Core

        private AvisoRepository avisoRepository;
        private UsuarioRepository usuarioRepository;

        public AvisosController(DbContext context)
            : base(context)
        {
            avisoRepository = new AvisoRepository(context);
            usuarioRepository = new UsuarioRepository(context);
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

        #endregion

        #region Actions

        public ActionResult Index(string SortOrder, string CurrentProcuraTitulo, string ProcuraTitulo, int? NumeroPaginas, int? Page)
        {
            //Localizacao();

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

            IList<Core.Models.StoredProcedures.spOC_US_ObtemAvisos> lista = null;
            lista = avisoRepository.GetByUsuario(Local.idUsuario);

            if (lista != null)
            {
                if (!String.IsNullOrEmpty(ProcuraTitulo))
                {
                    lista = lista.Cast<Core.Models.StoredProcedures.spOC_US_ObtemAvisos>().Where(s => s.Titulo.Contains(ProcuraTitulo)).ToList();
                }

                switch (SortOrder)
                {
                    case "Titulo_desc":
                        ViewBag.NameSortParm = "Titulo";
                        lista = lista.Cast<Core.Models.StoredProcedures.spOC_US_ObtemAvisos>().OrderByDescending(s => s.Titulo).ToList();
                        break;
                    case "Titulo":
                        ViewBag.NameSortParm = "Titulo_desc";

                        lista = lista.Cast<Core.Models.StoredProcedures.spOC_US_ObtemAvisos>().OrderBy(s => s.Titulo).ToList();
                        break;
                    default:  // Name ascending 
                        ViewBag.NameSortParm = "Titulo_desc";
                        lista = lista.Cast<Core.Models.StoredProcedures.spOC_US_ObtemAvisos>().OrderBy(s => s.Titulo).ToList();
                        break;
                }
            }

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 5);

            //Caso seja selecionada toda a lista (-1), pega na verdade 1000
            if (PageSize == -1)
            {
                PageSize = 1000;
            }
            ViewBag.PageSize = PageSize;
            //ViewBag.CurrentNumeroPaginas = NumeroPaginas;

            //Pagina corrente
            int PageNumber = (Page ?? 1);

            //DropDown de paginação
            int intNumeroPaginas = 5;  // (NumeroPaginas ?? 5);
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);

            return View(lista.ToPagedList(PageNumber, PageSize));
        }

        public ActionResult Ler(int? id)
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
            List<Core.Models.StoredProcedures.spOC_US_ObtemAvisoNaoLidos> listaAvisoLido = avisoRepository.GetLidosByUsuario(usuario.ID, id ?? 0);
            if (listaAvisoLido != null)
            {
                ViewBag.AvisoLido = true;
            }
            else
            {
                ViewBag.AvisoLido = false;
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

        public ActionResult Excluir(string id)
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
                avisoLido.AvisoExcluido = true;
                avisoLido.UsuarioID = Helpers.Local.idUsuario;                           

                db.AvisoLido.Add(avisoLido);
            }
            else
            {
                avisoLido.AvisoExcluido = true;
                     
                db.Entry(avisoLido).State = EntityState.Modified;
            }

            db.SaveChanges();
            
            return Json("OK");
        }

        public ActionResult Reload()
        {       
            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
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

        #endregion
    }
}
