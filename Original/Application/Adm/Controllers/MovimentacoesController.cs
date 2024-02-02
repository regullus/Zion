#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;
using Core.Repositories.Loja;
using Core.Repositories.Financeiro;
using Core.Repositories.Usuario;
using Core.Services.Loja;

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
using Core.Helpers;

#endregion

namespace Sistema.Controllers
{
    public class MovimentacoesController : Controller
    {
        #region Variaveis
        private LancamentoRepository lancamentoRepository;
        private PedidoRepository pedidoRepository;
        private PedidoService pedidoService;
        private UsuarioRepository usuarioRepository;
        private CategoriaRepository categoriaRepository;
        private Core.Helpers.TraducaoHelper traducaoHelper;

        private YLEVELEntities db = new YLEVELEntities();
        #endregion

        #region Core
        public MovimentacoesController(DbContext context)
        {
            Localizacao();

            lancamentoRepository = new LancamentoRepository(context);
            pedidoRepository = new PedidoRepository(context);
            pedidoService = new PedidoService(context);
            usuarioRepository = new UsuarioRepository(context);
            categoriaRepository = new CategoriaRepository(context);
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
            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Actions
        public ActionResult Index()
        {
            var lancamentos = lancamentoRepository.GetByExpression(l => l.TipoID == (int)Lancamento.Tipos.Credito || l.TipoID == (int)Lancamento.Tipos.Debito).OrderByDescending(o => o.DataLancamento).ThenBy(o => o.Usuario.Login).ToList();
            return View(lancamentos);
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            var busca = form["loginBusca"];

            ViewBag.Busca = busca;

            var pedidos = pedidoRepository.GetByExpression(p => p.Usuario.Nome.Contains(busca) || p.Usuario.Login.Contains(busca) || p.Usuario.NomeFantasia.Contains(busca)).ToList();
            return View(pedidos);
        }

        public ActionResult Incluir(int id)
        {
            var usuario = usuarioRepository.Get(id);
            if (usuario.Status != Usuario.TodosStatus.Associado)
            {
                return RedirectToAction("Index", "Usuarios");
            }

            ViewBag.MensagemAlerta = TempData["MensagemAlerta"];
            ViewBag.Categorias = categoriaRepository.GetAll().ToList();
            return View(usuario);
        }

        [HttpPost]
        public ActionResult Incluir(FormCollection form)
        {
            var idUsuario = form["Codigo"];
            string pwd = Core.Helpers.ConfiguracaoHelper.TemChave("ADMIN_FINANCEIRO_PWD") ? Core.Helpers.ConfiguracaoHelper.GetString("ADMIN_FINANCEIRO_PWD") : "Não há Senha";
            
            pwd = cpUtilities.Gerais.Morpho(pwd, TipoCriptografia.Descriptografa);

            if (form["password"] != pwd)
            {
                TempData["MensagemAlerta"] = traducaoHelper["SENHA_FINANCEIRA_INCORRETA"];
                return RedirectToAction("Incluir", new { id = int.Parse(idUsuario) });
            }

            double divisorCasas = 1;
            int intQtdeCasas = 0;
            int intPosicao = form["Valor"].IndexOf('.') + 1;
            if (intPosicao > 0)
            {
                intQtdeCasas = form["Valor"].Length - intPosicao;
                divisorCasas = Math.Pow(10, intQtdeCasas);
            }

            var tipo = form["Tipo"];
            var categoriaID = int.Parse(form["Categoria"]);
            var descricao = form["Descricao"];
            var categoria = categoriaRepository.Get(categoriaID);
            int contaID = 1;

            switch (categoriaID)
            {
                case 7:
                    contaID = 7;  //Transferencia
                    break;
                case 12:
                    contaID = 2; //Bonus de equipe
                    break;
                case 20:
                    contaID = 8; //Investimento
                    break;
                default:
                    break;
            }
           
            var lancamento = new Lancamento();
            lancamento.UsuarioID = int.Parse(idUsuario);
            lancamento.Tipo = (tipo == "D" ? Lancamento.Tipos.Debito : Lancamento.Tipos.Credito);
            lancamento.ReferenciaID = lancamento.UsuarioID;
            lancamento.Descricao = String.Format("{0}{1}{2}", categoria.Nome, String.IsNullOrEmpty(descricao) ? "" : " - ", descricao);
            lancamento.DataLancamento = App.DateTimeZion;
            lancamento.DataCriacao = App.DateTimeZion;
            lancamento.ContaID = contaID;
            lancamento.CategoriaID = categoriaID;
            lancamento.MoedaIDCripto = (int)Moeda.Moedas.NEN; //Nenhum

            var strvalor = form["Valor"].Replace("_", "0");
            var valor = double.Parse(Regex.Replace(strvalor, @"[^\d]", ""));
            if (lancamento.Tipo == Lancamento.Tipos.Debito && valor > 0)
                valor *= -1;

            lancamento.Valor = valor / divisorCasas;

            lancamentoRepository.Save(lancamento);

            var usuario = usuarioRepository.Get(lancamento.UsuarioID);
            if (usuario.Bloqueado && lancamento.Valor > 0)
            {
                var saldo = lancamentoRepository.GetByUsuarioConta(usuario.ID, 1).Sum(l => l.Valor);
                if (saldo >= 0)
                {
                    usuario.Bloqueado = false;
                    usuarioRepository.Save(usuario);
                }
            }

            return RedirectToAction("Details", "Usuarios", new { id = idUsuario });
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
