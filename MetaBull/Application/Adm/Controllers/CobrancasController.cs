#region Bibliotecas

using Core.Entities;
using Core.Repositories.Globalizacao;
using Core.Repositories.Sistema;
using Core.Repositories.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Repositories.Financeiro;
using Core.Services.MeioPagamento;
using DomainExtension.Repositories;
using Helpers;
using System.Configuration;
using System.Net;
using cpUtilities;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Models Local
using Sistema.Models;
using System.Threading;
using System.Threading.Tasks;
using Core.Helpers;
#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class CobrancasController : Controller
    {

        #region Variaveis

        #endregion

        #region Core

        //private CategoriaRepository categoriaRepository;
        //private PedidoPagamentoRepository pedidoPagamentoRepository;
        //private PedidoRepository pedidoRepository;
        //private BonificacaoRepository bonificacaoRepository;
        private Core.Helpers.TraducaoHelper traducaoHelper;

        public CobrancasController(DbContext context)
        {
            Localizacao();
            //categoriaRepository = new CategoriaRepository(context);
            //pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            //pedidoRepository = new PedidoRepository(context);
            //bonificacaoRepository = new BonificacaoRepository(context);
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
            //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            var culture = new System.Globalization.CultureInfo("en-US");
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Actions

        public ActionResult Retorno()
        {
            //ViewBag.Categorias = categoriaRepository.GetByTipo(Core.Entities.Lancamento.Tipos.Bonificacao);
            //ViewBag.Usuarios = this.repository.GetAll();
            //ViewBag.Pedidos = pedidoRepository.GetAll();
            //ViewBag.bonificacoesAll = bonificacaoRepository.GetAll();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(FormCollection form)
        {
            #region Variaveis

            string[] strMensagem;

            //int secaoID;
            //string nome;
            //string descricao;
            //Boolean ativo;
            //int tipoID;

            //try
            //{
                //secaoID = int.Parse(form["SecaoID"]);
                //nome = form["Nome"];
                //descricao = form["Descricao"];
                //ativo = form["Ativo"] == "on" ? true : false;
                //tipoID = int.Parse(form["TipoID"]);
            //}
            //catch (Exception ex)
            //{
                //Caso haja erro nos paramentros basicos volta a tela inicial
                //strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], "Cod 3427", ex.Message };
                //Mensagem(traducaoHelper["MENSAGEM_ERRO"], strMensagem, "err");
                //obtemMensagem();
                //return View("Index");
            //}

            //Inconsistencia
            //List<string> lstMensagem = new List<string>();

            #endregion

            #region ViewBags

            //Caso retorne ao form
            //ViewBag.secaoID = secaoID;
            //ViewBag.Nome = nome;
            //ViewBag.Descricao = descricao;
            //ViewBag.Ativo = ativo;
            //ViewBag.TipoID = tipoID;

            //ViewBag.SecaoID = new SelectList(db.ArquivoSecao, "ID", "Nome");
            //ViewBag.TipoID = new SelectList(db.ArquivoTipo, "ID", "Nome");

            #endregion

            #region Consistencia

            //Seção
            //if (secaoID == 0)
            //{
            //    lstMensagem.Add(traducaoHelper["SECAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}
            //Nome
            //if (String.IsNullOrEmpty(nome))
            //{
            //lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}
            //else
            //{
            //    if (nome.Length < 3)
            //    {
            //        lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
            //   }
            //}
            //Descricao
            //if (String.IsNullOrEmpty(descricao))
            //{
            //    lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}
            //else
            //{
            //    if (descricao.Length < 3)
            //    {
            //        lstMensagem.Add(traducaoHelper["DESCRICAO"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
            //    }
            //}
            //Tipo
            //if (tipoID == 0)
            //{
            //    lstMensagem.Add(traducaoHelper["TIPO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}

            //Descricao
            //if (String.IsNullOrEmpty(caminho))
            //{
            //   lstMensagem.Add(traducaoHelper["CAMINHO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}

            //if (lstMensagem.Count() > 0)
            //{
            //    strMensagem = lstMensagem.ToArray();
            //    Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
            //    obtemMensagem();
            //    return View("Create");
            //}
            #endregion

            #region Criar Arquivo

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (Request.Files[0].ContentLength > 0)
                {                   
                    var info = new FileInfo(Request.Files[0].FileName);
                    string strExtension = info.Extension;

                    if (strExtension.ToLower() == ".ret")
                    {                  
                        var caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                        if (caminhoFisico == "~")
                        {
                            caminhoFisico = Server.MapPath("~");
                        }
                        var diretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS_USUARIOS");
                        var caminho = caminhoFisico + diretorio + "/" + info.Name;
                        Request.Files[0].SaveAs(caminho);

                        strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");
                    }
                    else
                    {
                        strMensagem = new string[] { traducaoHelper["TIPO_ARQUIVO_UPLOAD"], traducaoHelper["TIPO"] + ": [" + strExtension.ToLower() + "]" + traducaoHelper["NAO_VALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagem, "ale");
                    }
                }
            }

            #endregion

            //PopulaViewBags();

            obtemMensagem();

            return View("Retorno");
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