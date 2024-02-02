#region Bibliotecas

using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Security.Claims;
using System.Linq;
using System.Globalization;
using System.Data.Entity;
using System.Data;
using System.Collections.Generic;
using Sistema.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Helpers;

//core
using Core.Entities;
using Core.Models.Loja;
using Core.Repositories.Globalizacao;
using Core.Repositories.Sistema;
using Core.Services.Globalizacao;
using Core.Services.Usuario;

//utilities
using cpUtilities;
using Core.Repositories.Usuario;
using Fluentx.Mvc;

#endregion

namespace Sistema.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        #region Core

        private Core.Helpers.TraducaoHelper traducaoHelper;
        private PaisRepository paisRepository;
        private GeolocalizacaoService geolocalizacaoService;
        private UsuarioService usuarioService;
        private TraducaoRepository traducaoRepository;
        private UsuarioRepository usuarioRepository;
        private AdministradorRepository administradorRepository;

        #endregion

        #region Variaveis

        private double cdblLatitude = 0;
        private double cdblLongitude = 0;

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

        #region Controlers

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

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

        #region Actions

        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }

        [AllowAnonymous]
        public ActionResult Login(string strPopupTitle, string strPopupMessage, string Sair)
        {
            Localizacao();
            Fundos();

            if (!string.IsNullOrEmpty(Sair))
            {
                //Remove autenticação caso ela exista.
                AuthenticationManager.SignOut();
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    //Caso ja tenha se logado vai para home
                    return RedirectToAction("Index", "Home");
                }
            }

            //Exibe popup com mensagem passada como paramentor
            if (!string.IsNullOrEmpty(strPopupTitle))
            {
                ViewBag.PopupShow = "OK";
                ViewBag.PopupTitle = strPopupTitle;
                ViewBag.PopupMessage = strPopupMessage;
            }
            else
            {
                ViewBag.PopupShow = "NOOK";
            }

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            Localizacao();
            Fundos();

            if (!Local.BancoConexao)
            {
                string[] strMensagem = new string[] { traducaoHelper["LOGIN_NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"], traducaoHelper["POR_FAVOR_TENTE_MAIS_TARDE"] };
                Mensagem("LOGIN", strMensagem, "msg");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            #region Variaveis 

            bool blnAtivo = true;

            //Log de tentativa de acesso a paginas não autorizadas pelo identity
            string strIPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (strIPAddress == "" || strIPAddress == null)
            {
                strIPAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            if (strIPAddress == "" || strIPAddress == "::1" || strIPAddress == null)
            {
                strIPAddress = "0.0.0.0";
            }

            #endregion

            #region ReCaptha

            bool blnContinua = true;

            if (WebConfigurationManager.AppSettings["Ambiente"] != "dev" && WebConfigurationManager.AppSettings["Ambiente"] != "hom")
            {
                //Obtem valor do reCaptha do google
                string recaptcha = Request.Form["g-recaptcha-response"];
                string postData = "secret=" + Core.Helpers.ConfiguracaoHelper.GetString("RECAPTCHA_SECRET_KEY") + "&response=" + recaptcha;
                //string postData = "secret=6LfKeScTAAAAAEKvJWHN8fRVkKhsy1L0zwGsWpN6&response=" + recaptcha;
                string strSenhaConfirmar = Request.Form["senhaConfirmar"];
                blnContinua = false;

                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        var json2 = webClient.DownloadString("https://www.google.com/recaptcha/api/siteverify?" + postData);
                        dynamic data = JObject.Parse(json2);
                        if (data != null)
                        {
                            if (data["success"].ToString() == "True")
                            {
                                blnContinua = true;
                            }
                            else
                            {
                                blnContinua = false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    blnContinua = false;
                }
            }

            #endregion

            if (blnContinua)
            {
                if (blnAtivo)
                {

                    #region Usuario

                    //Checa se usuario existe
                    var context = new YLEVELEntities();
                    //usuarioRepository = new UsuarioRepository(context);
                    administradorRepository = new AdministradorRepository(context);

                    Administrador user = administradorRepository.GetByEmail(model.Email);
                    if (user == null)
                    {
                        Local.LogAcesso("(adm1) " + model.Email, model.Password, strIPAddress);
                        return RedirectToAction("Login", "Account", new
                        {
                            strPopupTitle = "Login",
                            strPopupMessage = "Login incorreto!"
                        });
                    }

                    //Somente para verificar a senha do usuario
                    string strSenha = Core.Helpers.CriptografiaHelper.Descriptografar(user.Senha);

                    if (strSenha != model.Password)
                    {
                        Local.LogAcesso("(adm2) " + model.Email, model.Password, strIPAddress);
                        return RedirectToAction("Login", "Account", new
                        {
                            strPopupTitle = "Login",
                            strPopupMessage = "Login incorreto!!"
                        });
                    }

                    #endregion

                    #region Autenticacao

                    // This doen't count login failures towards lockout only two factor authentication
                    // To enable password failures to trigger lockout, change to shouldLockout: true
                    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                    switch (result)
                    {
                        case SignInStatus.Success:
                            return RedirectToLocal(returnUrl);
                        case SignInStatus.LockedOut:
                            return View("Lockout");
                        case SignInStatus.RequiresVerification:
                            return RedirectToAction("VerifyCode", new { ReturnUrl = returnUrl });
                        case SignInStatus.Failure:
                            //Caso o login não seja valido retorna a pagina de login informando o ocorrido.
                            Local.LogAcesso(model.Email, model.Password, strIPAddress);
                            return RedirectToAction("Login", "Account", new
                            {
                                strPopupTitle = "Login",
                                strPopupMessage = "Falha no login."
                            });
                        default:
                            //Caso o login não seja valido retorna a pagina de login informando o ocorrido.
                            Local.LogAcesso(model.Email, model.Password, strIPAddress);
                            return RedirectToAction("Login", "Account", new
                            {
                                strPopupTitle = "Login",
                                strPopupMessage = "Login incorreto."
                            });
                    }

                    #endregion

                }
                else
                {
                    #region FalhaAutenticacao

                    return RedirectToAction("Login", "Account", new
                    {
                        strPopupTitle = "Login",
                        strPopupMessage = "login não esta ativo."
                    });

                    #endregion
                }
            }
            else
            {
                #region Falha reCAPTCHA

                return RedirectToAction("Login", "Account", new
                {
                    strPopupTitle = "Login",
                    strPopupMessage = "reCAPTCHA não esta correto!"
                });

                #endregion
            }
        }

        [AllowAnonymous]
        //[HttpPost]
        public ActionResult eCom()
        {
            #region Chamada pelo eccomerce

            string strToken = Request["token"];
            string strEmail = Request["email"];
            string strPedido = Request["pedido"];
            string strUrlRetorno = Request["urlRetorno"];
            string strUrlRetornoLog = strUrlRetorno;
            string strValor = Request["valor"];

            //Teste
            strToken = "Gx1gtTtjY5rTK29Lrt7aXQRmbF0tn6CanOaOIX0dyJ8=";
            strEmail = "XXXXXX@123456qwert.com.ss"; //ToDo Trocar por email valido
            strPedido = "100000562";
            strUrlRetorno = "http://www.supermarcasstore.com.br/atualizapagto.php";
            strValor = "21.10";

            if (!String.IsNullOrEmpty(strToken))
            {
                Dictionary<string, object> objData = new Dictionary<string, object>();

                string strTokenLocal = "ecChr@Wil_" + DateTime.UtcNow.ToString("yyyyMMdd") + "_" + strPedido + "_" + strEmail;

                strTokenLocal = cpUtilities.Encryption.CreateToken(strTokenLocal);
                if (strTokenLocal != strToken)
                {
                    if (String.IsNullOrEmpty(strPedido))
                    {
                        strUrlRetornoLog += "?ret=invalidtoken";
                        objData.Add("ret", "invalidtoken");
                    }
                    else
                    {
                        string strTokenRetorno = cpUtilities.Encryption.CreateToken("ecChr@Wil_" + DateTime.UtcNow.ToString("yyyyMMdd") + "_" + strPedido + "_" + strEmail);
                        objData.Add("id", strPedido);
                        objData.Add("token", strTokenRetorno);
                        strUrlRetornoLog += "?ret=invalidtoken&id=" + strPedido + "&token=" + strTokenRetorno;
                    }
                    Local.Log("Account", "Ecommerce:" + strUrlRetorno);
                    //return Redirect(strUrlRetorno);
                    return this.RedirectAndPost(strUrlRetorno, objData);
                }

                Session["ecEmail"] = strEmail;
                Session["ecPedido"] = strPedido;
                Session["ecUrlRetorno"] = strUrlRetorno;
                Session["ecValor"] = strValor;

                return RedirectToAction("Login", new { strPopupTitle = "", strPopupMessage = "", Sair = "" });

            }
            Local.Log("Home", "Ecommerce:    Falha na chamada");
            return RedirectToAction("BadRequest", "Error");

            #endregion
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string returnUrl)
        {

            Localizacao();

            var provider = "GoogleAuthenticator";

            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());
            if (user != null)
            {
                ViewBag.Status = "For DEMO purposes the current " + " code is: " + await UserManager.GenerateTwoFactorTokenAsync(user.Id, provider);
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            Localizacao();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync("GoogleAuthenticator", model.Code, isPersistent: false, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    ViewBag.Link = callbackUrl;
                    return View("DisplayEmail");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId > 0 || code == null)
            {
                var result = await UserManager.ConfirmEmailAsync(userId, code);
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
            return View("Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            Localizacao();
            Fundos();
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            Localizacao();
            Fundos();
            bool blnEnvioEmail = false;

            #region ReCaptha

            bool blnContinua = true;

            if (WebConfigurationManager.AppSettings["Ambiente"] != "dev")
            {
                //Obtem valor do reCaptha do google
                string recaptcha = Request.Form["g-recaptcha-response"];
                //string postData = "secret=6LfKeScTAAAAAEKvJWHN8fRVkKhsy1L0zwGsWpN6&response=" + recaptcha;
                string postData = "secret=" + Core.Helpers.ConfiguracaoHelper.GetString("RECAPTCHA_SECRET_KEY") + "&response=" + recaptcha;
                string strSenhaConfirmar = Request.Form["senhaConfirmar"];
                blnContinua = false;

                try
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        var json2 = webClient.DownloadString("https://www.google.com/recaptcha/api/siteverify?" + postData);
                        dynamic data = JObject.Parse(json2);
                        if (data != null)
                        {
                            if (data["success"].ToString() == "True")
                            {
                                blnContinua = true;
                            }
                            else
                            {
                                blnContinua = false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    blnContinua = false;
                }
            }

            #endregion

            if (blnContinua)
            {
                #region Email

                if (ModelState.IsValid)
                {
                    var context = new YLEVELEntities();
                    usuarioRepository = new UsuarioRepository(context);
                    Usuario user = usuarioRepository.GetByEmail(model.Email);

                    if (user != null)
                    {
                        blnEnvioEmail = EnviaEmail(user.Email);
                    }
                    else
                    {
                        blnEnvioEmail = false;
                    }

                    //Informa usuario se o envio foi efetuado ou não.
                    if (blnEnvioEmail)
                    {
                        //Caso o envio de email tenha ocorrido.
                        return RedirectToAction("Login", "Account", new
                        {
                            strPopupTitle = "Login",
                            strPopupMessage = "Email enviado com sucesso."
                        });
                    }
                    else
                    {
                        //Caso o envio de email não tenha ocorrido.
                        return RedirectToAction("Login", "Account", new
                        {
                            strPopupTitle = "Login",
                            strPopupMessage = "Não foi possível enviar o email. Por favor entre em contato com o suporte."
                        });
                    }
                }
                else
                {
                    //Caso o envio de email não tenha ocorrido.
                    return RedirectToAction("Login", "Account", new
                    {
                        strPopupTitle = "Login",
                        strPopupMessage = "Email inválido."
                    });
                }

                #endregion
            }
            else
            {
                #region Falha reCAPTCHA

                return RedirectToAction("Login", "Account", new
                {
                    strPopupTitle = "Login",
                    strPopupMessage = "reCAPTCHA não esta correto!"
                });

                #endregion
            }

        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            Localizacao();

            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId > 0)
            {
                var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
                var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
                return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
            }
            return View("Error");
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //[AllowAnonymous]
        //public async Task<ActionResult> LoginExterno(string login, string senha)
        //{

        //    #region variaveis

        //    bool blnContinua = false;
        //    string strLogin = "";
        //    string strSenha = "";

        //    #endregion

        //    //Descriptografa entrada de dados
        //    if (!String.IsNullOrEmpty(login))
        //    {
        //        strLogin = cpUtilities.Gerais.Morpho(login, TipoCriptografia.Descriptografa);
        //        blnContinua = !String.IsNullOrEmpty(senha);
        //        if (blnContinua)
        //        {
        //            strSenha = cpUtilities.Gerais.Morpho(senha, TipoCriptografia.Descriptografa);
        //        }
        //    }

        //    //Monta model para autenticacao, como se fosse um post
        //    LoginViewModel model = new LoginViewModel();
        //    model.Email = strLogin;
        //    model.Password = strSenha;

        //    //Verifica conexão com banco de dados
        //    if (!Local.BancoConexao)
        //    {
        //        string[] strMensagem = new string[] { "Não foi possível completar usa requisição.", "Por favor tente mais tarde." };
        //        Mensagem("LOGIN", strMensagem, "msg");
        //        return RedirectToAction("Login", "Account");
        //    }

        //    #region Variaveis 

        //    bool blnAtivo = false;
        //    GenericRepository<Usuario> objTabUsuario;
        //    IEnumerable<Usuario> objRegUsuario;

        //    //Log de tentativa de acesso a paginas não autorizadas pelo identity
        //    string strIPAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        //    if (strIPAddress == "" || strIPAddress == null)
        //    {
        //        strIPAddress = Request.ServerVariables["REMOTE_ADDR"];
        //    }
        //    if (strIPAddress == "" || strIPAddress == "::1" || strIPAddress == null)
        //    {
        //        strIPAddress = "0.0.0.0";
        //    }

        //    #endregion

        //    #region Usuario esta ativo?

        //    //Checa se usuario esta ativo para login
        //    var context = new YLEVELEntities();
        //    usuarioService = new UsuarioService(context);
        //    Usuario usuario = usuarioService.Autenticar(strLogin, strSenha);

        //    if (usuario != null)
        //    {
        //        blnAtivo = true;
        //    }

        //    #endregion

        //    if (blnContinua)
        //    {
        //        if (blnAtivo)
        //        {
        //            #region Autenticacao

        //            string returnUrl = ".";
        //            // This doen't count login failures towards lockout only two factor authentication
        //            // To enable password failures to trigger lockout, change to shouldLockout: true
        //            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
        //            switch (result)
        //            {
        //                case SignInStatus.Success:
        //                    return RedirectToLocal(returnUrl);
        //                case SignInStatus.LockedOut:
        //                    return View("Lockout");
        //                case SignInStatus.RequiresVerification:
        //                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
        //                case SignInStatus.Failure:
        //                    Local.LogAcesso(model.Email, model.Password, strIPAddress);
        //                    string[] strMensagem = new string[] { "Falha no login!" };
        //                    Mensagem("LOGIN", strMensagem, "msg");
        //                    return RedirectToAction("Login", "Account");
        //                default:
        //                    //Caso o login não seja valido retorna a pagina de login informando o ocorrido.
        //                    string[] strMensagem2 = new string[] { "Login incorreto!" };
        //                    Mensagem("LOGIN", strMensagem2, "msg");
        //                    return RedirectToAction("Login", "Account");
        //            }

        //            #endregion
        //        }
        //        else
        //        {
        //            #region FalhaAutenticacao

        //            Local.LogAcesso(model.Email, model.Password, strIPAddress);
        //            string[] strMensagem = new string[] { "Login não é valido!" };
        //            Mensagem("LOGIN", strMensagem, "msg");
        //            return RedirectToAction("Login", "Account");

        //            #endregion
        //        }
        //    }
        //    else
        //    {
        //        #region Falha Login

        //        Local.LogAcesso(model.Email, model.Password, strIPAddress);
        //        string[] strMensagem = new string[] { "Login incorreto!" };
        //        Mensagem("LOGIN", strMensagem, "msg");
        //        return RedirectToAction("Login", "Account");

        //        #endregion
        //    }
        //}

        public ActionResult ClearCache()
        {
            traducaoRepository.LimparCacheTraducoes();
            ConfiguracaoRepository.ClearCache();
            return RedirectToAction("Login");
        }

        #endregion

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            //ToDo - Alterar para exibir erro em portugues.
            //foreach (var error in result.Errors)
            //{
            //   if (error.StartsWith("Name"))
            //   {
            //      var NameToEmail = Regex.Replace(error, "Name", "Email");
            //      ModelState.AddModelError("", NameToEmail);
            //   }
            //   else
            //   {
            //      ModelState.AddModelError("", error);
            //   }
            //}

        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { login = "ok" });
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private bool EnviaEmail(string strEmail)
        {
            bool blnEnvioEmail = false;
            try
            {
                var context = new YLEVELEntities();
                usuarioService = new UsuarioService(context);
                blnEnvioEmail = usuarioService.EnviarSenha(strEmail);
                usuarioService = null;
                context = null;
            }
            catch (Exception)
            {
                blnEnvioEmail = false;
            }

            return blnEnvioEmail;
        }

        private void Localizacao()
        {
            var context = new YLEVELEntities();
            paisRepository = new PaisRepository(context);
            geolocalizacaoService = new GeolocalizacaoService(context);
            //usuarioService = new UsuarioService(context);
            traducaoRepository = new TraducaoRepository(context);

            Pais pais = null;

            if (Request.UserLanguages.Any())
            {
                pais = paisRepository.GetBySigla(Request.UserLanguages[0]);
            }

            if (pais == null)
            {
                pais = geolocalizacaoService.GetByIP();
                if (pais == null)
                {
                    pais = paisRepository.GetPadrao();
                    if (pais == null)
                    {
                        //Default
                        pais = paisRepository.GetBySigla("pt-BR");
                    }
                }
            }

            if (pais != null && pais.Idioma != null)
            {
                traducaoHelper = new Core.Helpers.TraducaoHelper(pais.Idioma);
                ViewBag.TraducaoHelper = traducaoHelper;
                var culture = new System.Globalization.CultureInfo(pais.Idioma.Sigla);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        private void Fundos()
        {
            ViewBag.Fundos = ArquivoRepository.BuscarArquivos(Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO"), Core.Helpers.ConfiguracaoHelper.GetString("PASTA_FUNDOS"), "*.jpg");
        }

        #endregion

    }
}