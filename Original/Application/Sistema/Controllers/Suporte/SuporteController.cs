using Core.Repositories.Usuario;
using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Mvc;
using Helpers;
using System.Threading;
using Core.Repositories.Sistema;
using System.Linq;
using System.Threading.Tasks;
using Sistema.Models;
using PagedList;
using Core.Entities;
using System.IO;
using System.Web;

namespace Sistema.Controllers
{
    public class SuporteController : SecurityController<Core.Entities.Usuario>
    {
        #region Variaveis

        private UsuarioRepository usuarioRepository;
        private SuporteRepository suporteRepository;
        private SuporteMensagemRepository suporteMensagemRepository;

        private Core.Helpers.TraducaoHelper traducaoHelper;

        private YLEVELEntities db = new YLEVELEntities();
        
        string strIdiomaSigla = "pt-BR";
        public string PathImages { get; set; }


        #endregion

        #region Core

        public SuporteController(DbContext context) : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
            suporteRepository = new SuporteRepository(context);
            suporteMensagemRepository = new SuporteMensagemRepository(context);

            usuario = usuarioRepository.Get(Helpers.Local.idUsuario);
            if (usuario != null)
            {
                strIdiomaSigla = usuario.Pais.Idioma.Sigla;
            }


            PathImages = Path.Combine(ConfiguracaoHelper.GetString("CAMINHO_FISICO"), ConfiguracaoHelper.GetString("PASTA_SUPORTE_ANEXOS"));
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

        public async Task<ActionResult> Index(string SortOrder, string CurrentProcuraTitulo, string ProcuraTitulo, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            var objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraTitulo, ref ProcuraTitulo, ref NumeroPaginas, ref Page, "Aviso");
            objFuncoes = null;

            if (!(ProcuraTitulo != null))
            {
                if (ProcuraTitulo == null)
                {
                    ProcuraTitulo = CurrentProcuraTitulo;
                }
            }


            var lista = await suporteRepository.MeusSuportes(Local.idUsuario);

            var viewModel = lista.Select(s =>
            new SuporteViewModel
            {
                Assunto = s.Assunto,
                Respondido = s.Respondido,
                Data = s.SuporteMensagem.OrderByDescending(o => o.ID).FirstOrDefault().Data,
                ID = s.ID
            }).OrderByDescending(o => o.ID)
                .ToList();


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

            return View(viewModel.ToPagedList(PageNumber, PageSize));

        }

        public ActionResult Enviar()
        {
            ViewBag.Guid = Guid.NewGuid().ToString();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enviar(FormCollection form)
        {
            Localizacao();

            IList<string> strMsg = new List<string>();
            string[] strMensagem;


            try
            {
                var lstMensagem = new List<string>();

                var assunto = form["assunto"].ToString().Replace("'", "&#39;").Trim();
                var conteudo = form["conteudo"].ToString().Replace("'", "&#39;").Trim();

                var guid = Guid.Parse(Request.Form["guid"].ToString());
                ViewBag.Guid = guid;

                ViewBag.Assunto = form["assunto"];
                ViewBag.Conteudo = form["conteudo"];

                if (String.IsNullOrEmpty(assunto))
                {
                    lstMensagem.Add(traducaoHelper["ASSUNTO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }

                if (String.IsNullOrEmpty(conteudo))
                {
                    lstMensagem.Add(traducaoHelper["TEXTO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }

                if (lstMensagem.Count() > 0)
                {
                    Mensagem(traducaoHelper["INCONSISTENCIA"], lstMensagem.ToArray(), "msg");
                    obtemMensagem();
                    return View();
                }

                //var suporte = new Core.Entities.Suporte();
                //suporte.Data = App.DateTimeZion;
                //suporte.UsuarioID = Local.idUsuario;
                //suporte.Assunto = assunto;

                //var msg = new Core.Entities.SuporteMensagem();
                //msg.Texto = conteudo;
                //msg.Data = App.DateTimeZion;
                //suporte.SuporteMensagem.Add(msg);

                suporteRepository.CriarChamado(Local.idUsuario, guid, assunto, conteudo);

                strMensagem = new string[] { traducaoHelper["MENSAGEM_ENVIADA_COM_SUCESSO"] };
                Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");

            }
            catch (Exception ex)
            {
                strMensagem = new string[] { ex.Message };
                Mensagem(traducaoHelper["ERRO"], strMensagem, "err");
            }

            obtemMensagem();

            return RedirectToAction("Index");
        }

        public ActionResult Ler(int? id)
        {
            ViewBag.Guid = Guid.NewGuid().ToString();

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            var suporte = suporteRepository.Get(id.Value);
            suporte.ObtemImagens();

            if (suporte == null)
            {
                return RedirectToAction("Index");
            }

            return View(suporte);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Ler(int id, FormCollection form)
        {
            Localizacao();

            IList<string> strMsg = new List<string>();
            string[] strMensagem;

            try
            {
                var lstMensagem = new List<string>();

                var conteudo = form["conteudo"];
                var guid = Guid.Parse(Request.Form["guid"].ToString());
                ViewBag.Guid = guid;

                ViewBag.Conteudo = form["conteudo"].ToString().Trim();

                if (String.IsNullOrEmpty(conteudo))
                {
                    lstMensagem.Add(traducaoHelper["TEXTO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }

                if (lstMensagem.Count() > 0)
                {
                    Mensagem(traducaoHelper["INCONSISTENCIA"], lstMensagem.ToArray(), "msg");
                    obtemMensagem();

                    var suporte = suporteRepository.Get(id);
                    suporte.ObtemImagens();
                    return View(suporte);
                }

                suporteRepository.NovaInteracao(id, guid, conteudo);

                strMensagem = new string[] { traducaoHelper["MENSAGEM_ENVIADA_COM_SUCESSO"] };
                Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");

            }
            catch (Exception ex)
            {
                strMensagem = new string[] { ex.Message };
                Mensagem(traducaoHelper["ERRO"], strMensagem, "err");
            }

            obtemMensagem();

            return RedirectToAction("ler", new { id });
        }


        #endregion

        #region UPLOAD 

        [HttpPost]
        public ActionResult uploaddelete(ImagemUploadSuporteMensagem image)
        {
            var pathAndFile = PathAndFileImage(image);

            if (System.IO.File.Exists(pathAndFile))
            {
                System.IO.File.Delete(pathAndFile);
            }

            var path = PathImage(image.Guid);

            var dic = new System.IO.DirectoryInfo(path);

            if (dic.GetFiles().Count() == 0)
            {
                System.IO.Directory.Delete(path);
            }

            return Json(null);
        }

        private string PathAndFileImage(ImagemUploadSuporteMensagem image)
        {
            return PathAndFileImage(image.Guid, image.FileName);
        }

        private string PathAndFileImage(Guid guid, string fileName)
        {
            var path = PathImage(guid);

            return Path.Combine(path, fileName);
        }

        private string PathImage(Guid guid)
        {
            return Path.Combine(PathImages, guid.ToString());
        }

        [HttpPost]
        public ActionResult Upload()
        {
            var fileName = string.Empty;
            var guid = Guid.Empty;

            try
            {
                guid = Guid.Parse(Request.Form["Guid"].ToString());

                var path = PathImage(guid);

                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                                                                //Use the following properties to get file's name, size and MIMEType
                    int fileSize = file.ContentLength;
                    fileName = file.FileName;
                    string mimeType = file.ContentType;
                    System.IO.Stream fileContent = file.InputStream;
                    //To save file, use SaveAs method

                    string pathAndFile = Path.Combine(path, fileName);

                    file.SaveAs(pathAndFile); //File will be saved in application root
                }

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return null;
            }

            return Json(new ImagemUploadSuporteMensagem(fileName, guid));
        }

        #endregion


    }
}
