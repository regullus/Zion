#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Core.Helpers;
using Helpers;
using System.Threading;
using Core.Repositories.Sistema;
using System.Data.Entity;
using System.Linq;
using Sistema.Models;
using System.Threading.Tasks;
using PagedList;
using Core.Entities;
using Core.Repositories.Globalizacao;
using System.IO;
using System.Web;
using System.Web.Configuration;

#endregion

namespace Sistema.Controllers
{
    public class SuporteController : Controller
    {
        #region Variaveis

        private SuporteRepository suporteRepository { get; set; }
        private Core.Helpers.TraducaoHelper traducaoHelper;
        private IdiomaRepository idiomaRepository { get; set; }

        private YLEVELEntities db = new YLEVELEntities();

        string strIdiomaSigla = "pt-BR";
        public string PathImages { get; set; }

        #endregion

        #region Core

        public SuporteController(DbContext context)
        {
            suporteRepository = new SuporteRepository(context);
            idiomaRepository = new IdiomaRepository(context);

            if (WebConfigurationManager.AppSettings["Ambiente"] == "dev")
            {
                PathImages = Path.Combine(ConfiguracaoHelper.GetString("CAMINHO_FISICO"), "d\\");
            }
            else if (WebConfigurationManager.AppSettings["Ambiente"] == "homol")
            {
                PathImages = Path.Combine(ConfiguracaoHelper.GetString("CAMINHO_FISICO"), "h\\");
            }
            else
            {
                PathImages = Path.Combine(ConfiguracaoHelper.GetString("CAMINHO_FISICO"), "p\\");
            }

            Localizacao();
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

        public ActionResult Index(string SortOrder, string CurrentProcuraTexto, string ProcuraTexto, string ProcuraLogin, int? ProcuraIdioma, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            var lista = suporteRepository.GetByExpression(w =>
                            !w.Respondido
                            && w.SuporteMensagem.Any()
                            && (string.IsNullOrEmpty(ProcuraTexto) || w.Assunto.Contains(ProcuraTexto) || w.SuporteMensagem.Any(ws => ws.Texto.Contains(ProcuraTexto)))
                            && (string.IsNullOrEmpty(ProcuraLogin) || w.Usuario.Login.Equals(ProcuraLogin))
                            && (!ProcuraIdioma.HasValue || ProcuraIdioma.Value.Equals(0) || w.Usuario.Pais.IdiomaID.Equals(ProcuraIdioma.Value)))
            .Select(s =>
            new SuporteViewModel
            {
                ID = s.ID,
                Assunto = s.Assunto,
                Login = s.Usuario.Login,
                Nome = s.Usuario.Nome,
                Email = s.Usuario.Email,
                Data = s.SuporteMensagem.OrderByDescending(o => o.ID).FirstOrDefault().Data,
                Idioma = s.Usuario.Pais.Idioma.Nome
            }).OrderBy(o => o.ID)
            .ToList();

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 40);

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

            ViewBag.Idiomas = idiomaRepository.GetAll();
            ViewBag.ProcuraIdioma = ProcuraIdioma.HasValue ? ProcuraIdioma : 0;
            ViewBag.ProcuraLogin = ProcuraLogin;
            ViewBag.ProcuraTexto = ProcuraTexto;

            return View(lista.ToPagedList(PageNumber, PageSize));

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

                var conteudo = form["conteudo"].ToString().Replace("'", "&#39;").Trim();
                var guid = Guid.Parse(Request.Form["guid"].ToString());
                ViewBag.Guid = guid;

                ViewBag.Conteudo = conteudo;

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

                var mensagemId = suporteRepository.Resposta(id, Local.idUsuario, guid, conteudo);

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
            Localizacao();

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
                    fileName = file.FileName.ToLower();
                    if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
                    {
                        string mimeType = file.ContentType;
                        System.IO.Stream fileContent = file.InputStream;
                        //To save file, use SaveAs method

                        string pathAndFile = Path.Combine(path, fileName);

                        file.SaveAs(pathAndFile); //File will be saved in application root
                    }
                    else
                    {
                        Response.ClearHeaders();
                        Response.ClearContent();
                        Response.Status = "503 ServiceUnavailable";
                        Response.StatusCode = 503;
                        Response.StatusDescription = traducaoHelper["IMAGENS_PERMITIDAS"];
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return null;
            }

            return Json(new ImagemUploadSuporteMensagem(fileName, guid));
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
    }
}
