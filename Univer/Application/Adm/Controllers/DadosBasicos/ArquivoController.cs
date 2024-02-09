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
using Core.Repositories.Sistema;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;

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

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ArquivoController : Controller
    {
        #region Variaveis

        private YLEVELEntities db = new YLEVELEntities();
        private string cstrFalha = "";

        #endregion

        #region Core

        //private CartaoCreditoRepository cartaoCreditoRepository;

        private ArquivoRepository arquivoRepository;

        private Core.Helpers.TraducaoHelper traducaoHelper;

        public ArquivoController(DbContext context)
        {

            //paisRepository = new PaisRepository(context);

            arquivoRepository = new ArquivoRepository(context);

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

            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            ViewBag.TraducaoHelper = traducaoHelper;

            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Actions

        // GET: Arquivos
        public ActionResult Index(string SortOrder, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraSecao, string ProcuraSecao, int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraNome, ref ProcuraNome, ref CurrentProcuraSecao, ref ProcuraSecao, ref NumeroPaginas, ref Page, "NOME");
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

            if (!(ProcuraNome != null || ProcuraSecao != null))
            {
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraSecao == null)
                {
                    ProcuraSecao = CurrentProcuraSecao;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraEndereco = ProcuraSecao;

            IQueryable<Arquivo> lista = null;
            lista = db.Arquivo;
            if (!String.IsNullOrEmpty(ProcuraNome))
            {
                lista = lista.Where(s => s.Nome.Contains(ProcuraNome));
            }

            switch (SortOrder)
            {
                case "name_desc":
                    ViewBag.NameSortParm = "name";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderByDescending(s => s.Nome);
                    break;
                case "name":
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";

                    lista = lista.OrderBy(s => s.Nome);
                    break;
                default:  // Name ascending 
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderBy(s => s.Nome);
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

        // GET: Arquivos/Details/5
        public ActionResult Details(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arquivo arquivo = db.Arquivo.Find(id);
            if (arquivo == null)
            {
                return HttpNotFound();
            }
            return View(arquivo);
        }

        // GET: Arquivos/Create
        public ActionResult Create()
        {
            Localizacao();

            ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
            ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");

            return View();
        }

        //// POST: Arquivos/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,Nome,Descricao,SecaoID,TipoID,Ativo,Atualizacao,Imagem,Observacao")] Arquivo arquivo)
        //{
        //    Localizacao();

        //    if (ModelState.IsValid)
        //    {
        //        arquivo.Atualizacao = Core.Helpers.App.DateTimeZion;

        //        db.Arquivo.Add(arquivo);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
        //    ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");

        //    return View(arquivo);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Salvar(FormCollection form)
        //public ActionResult Salvar(FormCollection form)
        {
            #region Variaveis

            string[] strMensagem;

            int secaoID;
            string nome;
            string descricao;
            string observacao;
            Boolean ativo;
            int tipoID;
            //string caminho;

            try
            {
                secaoID = int.Parse(form["SecaoID"]);
                nome = form["Nome"];
                descricao = form["Descricao"];
                ativo = form["Ativo"] == "on" ? true : false;
                tipoID = int.Parse(form["TipoID"]);
                observacao = form["Observacao"];
                //caminho = form["Arquivo"];

            }
            catch (Exception ex)
            {
                //Caso haja erro nos paramentros basicos volta a tela inicial
                strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], "Cod 3427", ex.Message };
                Mensagem(traducaoHelper["MENSAGEM_ERRO"], strMensagem, "err");
                obtemMensagem();
                return View("Index");
            }

            //Inconsistencia
            List<string> lstMensagem = new List<string>();

            #endregion

            #region ViewBags

            //Caso retorne ao form
            ViewBag.secaoID = secaoID;
            ViewBag.Nome = nome;
            ViewBag.Descricao = descricao;
            ViewBag.Ativo = ativo;
            ViewBag.TipoID = tipoID;
            ViewBag.Observacao = observacao;
            //ViewBag.Caminho = caminho;

            ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
            ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");

            #endregion

            #region Consistencia

            //Seção
            if (secaoID == 0)
            {
                lstMensagem.Add(traducaoHelper["SECAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            //Nome
            //if (String.IsNullOrEmpty(nome))
            //{
            //    lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}
            //else
            //{
            //    if (nome.Length < 3)
            //    {
            //        lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
            //    }
            //}
            //Descricao
            if (string.IsNullOrEmpty(descricao))
            {
                lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (descricao.Length < 3)
                {
                    lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
                }
            }
            //Tipo
            if (tipoID == 0)
            {
                lstMensagem.Add(traducaoHelper["TIPO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            if (string.IsNullOrEmpty(observacao))
            {
                lstMensagem.Add(traducaoHelper["OBSERVACAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            //else

            //Descricao
            //if (String.IsNullOrEmpty(caminho))
            //{
            //   lstMensagem.Add(traducaoHelper["CAMINHO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}

            if (lstMensagem.Count > 0)
            {
                strMensagem = lstMensagem.ToArray();
                Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                obtemMensagem();
                return View("Create");
            }
            #endregion

            #region Criar Arquivo

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (Request.Files[0].ContentLength > 0)
                {
                    Arquivo arquivo = new Arquivo();

                    arquivo.SecaoID = secaoID;
                    arquivo.Nome = file.FileName;
                    arquivo.Descricao = descricao;
                    arquivo.Ativo = ativo;
                    arquivo.TipoID = tipoID;
                    arquivo.Atualizacao = Core.Helpers.App.DateTimeZion;
                    arquivo.Observacao = observacao;

                    //caminho = form["Caminho"];


                    var info = new FileInfo(Request.Files[0].FileName);
                    string strExtension = info.Extension;

                    if (strExtension.ToLower() == ".jpg" || strExtension.ToLower() == ".png" ||
                        strExtension.ToLower() == ".doc" || strExtension.ToLower() == ".pdf")
                    {
                        arquivoRepository.Save(arquivo);

                        var strCaminhoFisico = "c:/files/zion/ativabox/"; //Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                        //if (strCaminhoFisico == "~")
                        //{
                        //    strCaminhoFisico = Server.MapPath("~");
                        //}
                        var strDiretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS");
                        var strCaminho = strCaminhoFisico + "/" + strDiretorio + "/" + arquivo.Nome + info.Extension.ToLower();
                        Request.Files[0].SaveAs(strCaminho);



                        strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        strMensagem = new string[] { traducaoHelper["TIPO_ARQUIVO_UPLOAD"], traducaoHelper["TIPO"] + ": [" + strExtension.ToLower() + "]" + traducaoHelper["NAO_VALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagem, "ale");
                        obtemMensagem();
                        return View("Create");

                    }
                }
                else
                {
                    strMensagem = new string[] { traducaoHelper["ARQUIVO"] + traducaoHelper["NAO_VALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagem, "ale");
                    obtemMensagem();
                    return View("Create");

                }
            }

            #endregion

            //PopulaViewBags();

            obtemMensagem();

            return RedirectToAction("Index");
        }

        // GET: Arquivos/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arquivo arquivo = db.Arquivo.Find(id);
            if (arquivo == null)
            {
                return HttpNotFound();
            }
            ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
            ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");
            return View(arquivo);
        }

        // POST: Arquivos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nome,Descricao,SecaoID,TipoID,Ativo,Atualizacao,Imagem,Observacao")] Arquivo arquivo)
        {
            Localizacao();

            if (ModelState.IsValid)
            {
                db.Entry(arquivo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(arquivo);
        }

        // GET: Arquivos/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arquivo arquivo = db.Arquivo.Find(id);
            if (arquivo == null)
            {
                return HttpNotFound();
            }
            return View(arquivo);
        }

        // POST: Arquivos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            Arquivo arquivo = db.Arquivo.Find(id);
            db.Arquivo.Remove(arquivo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Reload()
        {
            Localizacao();

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

        public async Task<ActionResult> SalvarEditar(FormCollection form, int? NumeroPaginas, int? Page)
        //public ActionResult SalvarEditar(FormCollection form)
        {
            #region Variaveis

            string[] strMensagem;

            int id;
            int secaoID;
            string nome;
            string descricao;
            string observacao;
            Boolean ativo;
            int tipoID;

            try
            {
                id = int.Parse(form["ID"]);
                secaoID = int.Parse(form["SecaoID"]);
                nome = form["Nome"];
                descricao = form["Descricao"];
                ativo = form["Ativo"] == "on" ? true : false;
                tipoID = int.Parse(form["TipoID"]);
                observacao = form["Observacao"];

            }
            catch (Exception ex)
            {
                //Caso haja erro nos paramentros basicos volta a tela inicial
                strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], "Cod 3427", ex.Message };
                Mensagem(traducaoHelper["MENSAGEM_ERRO"], strMensagem, "err");
                obtemMensagem();
                return View("Index");
            }

            //Inconsistencia
            List<string> lstMensagem = new List<string>();

            #endregion

            #region ViewBags

            //Caso retorne ao form
            ViewBag.id = id;
            ViewBag.secaoID = secaoID;
            ViewBag.Nome = nome;
            ViewBag.Descricao = descricao;
            ViewBag.Ativo = ativo;
            ViewBag.TipoID = tipoID;
            ViewBag.Observacao = observacao;

            ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
            ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");

            IQueryable<Arquivo> lista = null;
            lista = db.Arquivo;

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

            #endregion

            #region Consistencia

            //Seção
            if (secaoID == 0)
            {
                lstMensagem.Add(traducaoHelper["SECAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }

            //Descricao
            if (String.IsNullOrEmpty(descricao))
            {
                lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (descricao.Length < 3)
                {
                    lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
                }
            }
            //Tipo
            if (tipoID == 0)
            {
                lstMensagem.Add(traducaoHelper["TIPO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            if (String.IsNullOrEmpty(observacao))
            {
                lstMensagem.Add(traducaoHelper["OBSERVACAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else


            if (lstMensagem.Count() > 0)
            {
                strMensagem = lstMensagem.ToArray();
                Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                obtemMensagem();
                return View("Edit");
            }
            #endregion

            #region Criar Arquivo

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                Arquivo arquivo = new Arquivo();
                arquivo.ID = id;
                arquivo.SecaoID = secaoID;
                arquivo.Nome = nome;
                arquivo.Descricao = descricao;
                arquivo.Ativo = ativo;
                arquivo.TipoID = tipoID;
                arquivo.Atualizacao = Core.Helpers.App.DateTimeZion;
                arquivo.Observacao = observacao;

                if (arquivo.ID > 0)
                    db.Entry(arquivo).State = EntityState.Modified;
                else
                    db.Arquivo.Add(arquivo);

                db.SaveChanges();

                if (Request.Files[0].ContentLength > 0)
                {
                                      
                    arquivo.Nome = file.FileName;
                    
                    var info = new FileInfo(Request.Files[0].FileName);
                    string strExtension = info.Extension;

                    if (strExtension.ToLower() == ".jpg" || strExtension.ToLower() == ".png" ||
                        strExtension.ToLower() == ".doc" || strExtension.ToLower() == ".pdf")
                    {
                        arquivoRepository.Save(arquivo);

                        var strCaminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                                                                          if (strCaminhoFisico == "~")
                                                                          {
                                                                              strCaminhoFisico = Server.MapPath("~");
                                                                          }
                        var strDiretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS");
                        var strCaminho = strCaminhoFisico + "/" + strDiretorio + "/" + arquivo.Nome + info.Extension.ToLower();
                        Request.Files[0].SaveAs(strCaminho);
                        if (arquivo.ID > 0)
                            db.Entry(arquivo).State = EntityState.Modified;
                        else
                            db.Arquivo.Add(arquivo);

                        db.SaveChanges();



                        strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        strMensagem = new string[] { traducaoHelper["TIPO_ARQUIVO_UPLOAD"], traducaoHelper["TIPO"] + ": [" + strExtension.ToLower() + "]" + traducaoHelper["NAO_VALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagem, "ale");
                    }
                }
                else
                {
                    strMensagem = new string[] { traducaoHelper["ARQUIVO"] + traducaoHelper["NAO_VALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagem, "ale");
                }
            }

            #endregion
            //PopulaViewBags();

            obtemMensagem();

            return RedirectToAction("Index");
        }



    }


    #endregion
}