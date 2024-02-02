using Base32;
using Core.Repositories.Usuario;
using Core.Services.Usuario;
using Helpers;
using Microsoft.AspNet.Identity.Owin;
using OtpSharp;
using Sistema.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Sistema.Controllers
{
    public class SegurancaController : Controller
    {
        // GET: Seguranca
        private ApplicationUserManager _userManager;
        private Core.Helpers.TraducaoHelper traducaoHelper;
        private UsuarioRepository usuarioRepository;

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

        public SegurancaController(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
        }

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        // GET: Seguranca
        public ActionResult GoogleAuthenticator()
        {
            Localizacao();

            var user = UserManager.FindByIdAsync(Local.idAutenticacao).Result;

            var response = new GoogleAuthenticatorResponse { TwoFactorEnabled = user.TwoFactorEnabled };

            return View(response);
        }

        public JsonResult HabilitarGoogleAuthenticator()
        {
            var user = UserManager.FindByIdAsync(Local.idAutenticacao).Result;

            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string barcodeUrl = GetTotpUrl(secretKey, Local.usuario.Nome);

            var response = new GoogleAuthenticatorResponse()
            {
                TwoFactorEnabled = user.TwoFactorEnabled,
                Ok = true,
                Secretkey = Base32Encoder.Encode(secretKey),
                QrCodeImage = HttpUtility.UrlEncode(barcodeUrl)
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> Validar(GoogleAuthenticatorRequest model)
        {
            var response = new GoogleAuthenticatorResponse();

            byte[] secretKey = Base32Encoder.Decode(model.Secretkey);

            long timeStepMatched = 0;
            var otp = new Totp(secretKey);
            if (otp.VerifyTotp(model.Token, out timeStepMatched, new VerificationWindow(10, 10)))
            {
                var user = await UserManager.FindByIdAsync(Local.idAutenticacao);

                //user.TwoFactorEnabled = true;
                //user.IsGoogleAuthenticatorEnabled = true;
                //user.GoogleAuthenticatorSecretKey = model.Secretkey;

                //await UserManager.UpdateAsync(user);
                
                usuarioRepository.AdmTwoFactorEnabled(Local.idAutenticacao, model.Secretkey, true);

                response.Ok = true;
            }
            else
            {
                response.Mensagem = traducaoHelper["TOKEN_INVALIDO"];
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DesabilitarGoogleAuthenticator(GoogleAuthenticatorRequest model)
        {
            Localizacao();
            var response = new GoogleAuthenticatorResponse();
            var user = await UserManager.FindByIdAsync(Local.idUsuario);
            byte[] secretKey = Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey);
            string barcodeUrl = GetTotpUrl(secretKey, Local.usuario.Nome);
            long timeStepMatched = 0;
            var otp = new Totp(secretKey);
            if (otp.VerifyTotp(model.Token, out timeStepMatched, new VerificationWindow(10, 10)))
            {
                //user.TwoFactorEnabled = false;
                //user.IsGoogleAuthenticatorEnabled = false;
                //user.GoogleAuthenticatorSecretKey = string.Empty;
                //await UserManager.UpdateAsync(user);
                usuarioRepository.AdmTwoFactorEnabled(Local.idAutenticacao, model.Secretkey, false);

                response.Ok = true;
            }
            else
            {
                response.Mensagem = traducaoHelper["TOKEN_INVALIDO"];
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private string GetTotpUrl(byte[] secretKey, string user)
        {
            var url = string.Empty;
            url = KeyUrl.GetTotpUrl(secretKey, user) + "&issuer=" + Helpers.Local.Cliente + "ADMIN";
            return url;
        }
    }
}