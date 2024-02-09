using Sistema.Models.Envelope;
using Sistema.Services.GoogleAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OtpSharp;
using Base32;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Sistema.Models;
using Microsoft.AspNet.Identity.Owin;

namespace Sistema.Controllers
{
    public class SegurancaController : SecurityController<Core.Entities.Usuario>
    {
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

        public SegurancaController(DbContext context) : base(context)
        {
        }

        // GET: Seguranca
        public ActionResult GoogleAuthenticator()
        {
            var user = UserManager.FindByIdAsync(usuario.IdAutenticacao).Result;

            var response = new GoogleAuthenticatorResponse { TwoFactorEnabled = user.TwoFactorEnabled };

            return View(response);
        }

        public JsonResult HabilitarGoogleAuthenticator()
        {
            var user = UserManager.FindByIdAsync(usuario.IdAutenticacao).Result;

            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string barcodeUrl = GetTotpUrl(secretKey, usuario.Login);

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
                var user = await UserManager.FindByIdAsync(usuario.IdAutenticacao);

                user.TwoFactorEnabled = true;
                user.IsGoogleAuthenticatorEnabled = true;
                user.GoogleAuthenticatorSecretKey = model.Secretkey;

                await UserManager.UpdateAsync(user);

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
            var response = new GoogleAuthenticatorResponse();

            var user = await UserManager.FindByIdAsync(usuario.IdAutenticacao);

            byte[] secretKey = Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey);

            string barcodeUrl = GetTotpUrl(secretKey, usuario.Login);

            long timeStepMatched = 0;
            var otp = new Totp(secretKey);
            if (otp.VerifyTotp(model.Token, out timeStepMatched, new VerificationWindow(10, 10)))
            {
                user.TwoFactorEnabled = false;
                user.IsGoogleAuthenticatorEnabled = false;
                user.GoogleAuthenticatorSecretKey = string.Empty;

                await UserManager.UpdateAsync(user);

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
            url = KeyUrl.GetTotpUrl(secretKey, user) + "&issuer=" + Helpers.Local.Cliente;

            return url;
        }
    }
}