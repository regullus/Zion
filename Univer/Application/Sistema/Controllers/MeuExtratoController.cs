#region Bibliotecas

using Core.Entities;
using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Threading.Tasks;

using Base32;
using Core.Repositories.Usuario;
using Core.Services.MeioPagamento;
using cpUtilities;
using Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using OtpSharp;
using Sistema.Models;
using System.Text.RegularExpressions;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class MeuExtratoController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis
        #endregion

        #region Core

        private CategoriaRepository categoriaRepository;
        private ContaRepository contaRepository;

        public MeuExtratoController(DbContext context)
            : base(context)
        {
            categoriaRepository = new CategoriaRepository(context);
            contaRepository = new ContaRepository(context);
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

        public bool VerificaAutenticacao2FA(string token)
        {
            var user = UserManager.FindById(usuario.IdAutenticacao);

            if (string.IsNullOrEmpty(user.GoogleAuthenticatorSecretKey))
            {
                return false;
            }

            byte[] secretKey = Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey);

            var otp = new Totp(secretKey);
            if (otp.VerifyTotp(token, out _, new VerificationWindow(10, 10)))
                return true;
            else
                return false;
        }

        #endregion

        #region Actions

        public ActionResult Index()
        {
            var contas = contaRepository.GetByAtiva();

            ArrayList contasLancamentos = new ArrayList();
            foreach (var conta in contas)
            {
                var lancamentos = usuario.Lancamento.Where(l => l.ContaID == conta.ID);
                contasLancamentos.Add(lancamentos);
            }
            ViewBag.Contas = contas;
            ViewBag.Contaslancamentos = contasLancamentos;

            return View();
        }

        [HttpPost]
        public JsonResult solicitarRenovacao(string valor = null, string token = null)
        {
            string strSenha = CriptografiaHelper.Descriptografar(usuario.Senha);

            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                if (!VerificaAutenticacao2FA(token))
                    return Json(traducaoHelper["TOKEN_INVALIDO"]);
            }
            var contas = contaRepository.CallSpRenovacao(usuario.ID);

            return Json("OK");
        }

        #endregion

        #region Autorizacao

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #endregion

    }
}
