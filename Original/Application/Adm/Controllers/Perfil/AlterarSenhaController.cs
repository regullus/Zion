using Core.Repositories.Globalizacao;
using Core.Repositories.Sistema;
using Core.Services.Globalizacao;
using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Sistema.Models;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Entities;
using Helpers;
using System.Threading;
using System.Data.Entity.Validation;
using System.Data.Entity;

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class AlterarSenhaController : Controller
    {

        #region Variaveis

        private TraducaoHelper traducaoHelper;
        private AdministradorRepository administradorRepository;
        
        #endregion

        #region Core

        public AlterarSenhaController(DbContext context)
        {
            Localizacao();

            administradorRepository = new AdministradorRepository(context);
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

        #region Actions

        public ActionResult Index()
        {
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Alterar(FormCollection frmAltera)
        {
            #region Variaveis

            //Verifica se a troca foi efetuada com sucesso
            bool blnValid = false;

            //Obter dados digitados:
            string strSenhaAtual = frmAltera["txtSenhaAtual"];
            string strSenhaNova = frmAltera["txtSenhaNova"];
            string strSenhaConfirma = frmAltera["txtSenhaConfima"];

            #endregion

            #region Verificar se senha confirmação são identicas.

            if (strSenhaNova == strSenhaConfirma)
            {
                blnValid = true;
            }
            else
            {
                string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["NOVA_SENHA_DIFERENTE_CONFIRMACAO"] };
                Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                blnValid = false;
            }

            #endregion

            try
            {
                if (blnValid)
                {
                    Administrador adm = administradorRepository.Get(Local.idUsuario);

                    #region Validação

                    if (blnValid)
                    {
                        //Verifica se senha atual esta correta.
                        var senhaCripto = CriptografiaHelper.Descriptografar(adm.Senha);
                        if (strSenhaAtual == senhaCripto)
                        {
                            //Verifica se senhaNova é identica a senhaConfima
                            if (strSenhaNova == strSenhaConfirma)
                            {
                                if (strSenhaNova != strSenhaAtual)
                                {
                                    blnValid = true;
                                }
                                else
                                {
                                    string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["NOVA_SENHA_DIFERENTE_SENHA_ATUAL"] };
                                    Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                                    blnValid = false;
                                }
                            }
                            else
                            {
                                string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["NOVA_SENHA_DIFERENTE_CONFIRMACAO"] };
                                Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                                blnValid = false;
                            }
                        }
                        else
                        {
                            string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["SENHA_ATUAL_NAO_CONFERE"] };
                            Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                            blnValid = false;
                        }
                    }

                    #endregion

                    #region Consistência
                    if (blnValid)
                    {
                        //Verifica se senha é forte
                        if (!Local.verificaSenhaForte(strSenhaNova))
                        {
                            string[] strMensagem = new string[] { traducaoHelper["CONSISTENCIA_SENHA_1"], traducaoHelper["CONSISTENCIA_SENHA_2"], traducaoHelper["CONSISTENCIA_SENHA_3"] };
                            Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "ale");
                            blnValid = false;
                            return RedirectToAction("Index");
                        }
                    }
                    #endregion

                    #region Alteração

                    if (blnValid)
                    {
                        // var result = await UserManager.ChangePasswordAsync(Helpers.Local.idAutenticacao, strSenhaAtual, strSenhaNova);
                        var user = await UserManager.FindByIdAsync(adm.IdAutenticacao);
                        if (user == null)
                        {
                            string[] strMensagem = new string[] { " " + traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"] + " ", traducaoHelper["MOTIVO_USUARIO_NAO_ENCONTRADO"] };
                            Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                            blnValid = false;
                        }
                        if (blnValid)
                        {

                            #region Identity
                            var code = await UserManager.GeneratePasswordResetTokenAsync(adm.IdAutenticacao);
                            var result = await UserManager.ResetPasswordAsync(adm.IdAutenticacao, code, strSenhaNova);
                            #endregion

                            if (result.Succeeded)
                            {
                                #region Sistema

                                blnValid = true;

                                //Senha do Usuario deve ser criptografada
                                adm.Senha = CriptografiaHelper.Criptografar(strSenhaNova);
                                adm.Liberado = true;

                                //Somente apos o identity trocar a senha é mque esta é trocada no usuario
                                administradorRepository.Save(adm);

                                #endregion

                            }
                            else
                            {
                                #region Falha

                                string strErro = "";
                                //Msg não foi possivel alterar senha
                                foreach (var error in result.Errors)
                                {
                                    strErro += error + ";";
                                }
                                string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["MOTIVO"] + strErro };
                                Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "ale");
                                blnValid = false;
                                return RedirectToAction("Index", "AlterarSenha");
                                #endregion
                            }
                        }
                    }

                    if (blnValid)
                    {
                        #region Sucesso

                        string[] strMensagem = new string[] { traducaoHelper["SENHA_ALTERADA_SUCESSO"] };
                        Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + " ", strMensagem, "msg");
                        //Redireciona a home apos troca da senha
                        return RedirectToAction("Index", "AlterarSenha");

                        #endregion
                    }

                    #endregion
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                #region Erro

                string strErro = "";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        strErro = "Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage;
                    }

                    string[] strErros = new string[] { " " + traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"] + " ", strErro };
                    Mensagem(" " + traducaoHelper["FALHA"] + " ", strErros, "ale");
                }

                #endregion
            }
            catch (Exception ex)
            {
                #region Erro

                string[] strMensagem = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], "Um erro ocorreu:" + ex.Message };
                Mensagem(" " + traducaoHelper["ALTERAR_SENHA"] + "  ", strMensagem, "msg");

                #endregion
            }

            return RedirectToAction("Index", "AlterarSenha");
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


        // Add the Group Manager (NOTE: only access through the public
        // Property, not by the instance variable!)
        private AutenticacaoGrupoManager _groupManager;

        public AutenticacaoGrupoManager GroupManager
        {
            get
            {
                return _groupManager ?? new AutenticacaoGrupoManager();
            }
            private set
            {
                _groupManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        #endregion

        #region Helpers

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion
    }
}