#region Bibliotecas

using Core.Entities;
using Core.Factories;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using Core.Repositories.Sistema;
using Core.Repositories.Usuario;
using Core.Services.Globalizacao;
using Core.Services.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Core.Repositories.Financeiro;
using Core.Repositories.Rede;
using Core.Helpers;
using System.Data.Entity.Validation;
using System.Web.Configuration;

using System.IO;
using System.Data;
using System.Net;
using System.Web;
using System.Threading.Tasks;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Models Local
using Sistema.Models;
using System.Text.RegularExpressions;
using Helpers;
using System.Text;
using Newtonsoft.Json.Linq;


#endregion

namespace Sistema.Controllers
{
    [AllowAnonymous]
    public class CadastroController : Controller
    {

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

        #region Variaveis

        #endregion

        #region Core

        private PaisRepository paisRepository;
        private EstadoRepository estadoRepository;
        private CidadeRepository cidadeRepository;
        private GeolocalizacaoService geolocalizacaoService;

        private UsuarioRepository usuarioRepository;
        private UsuarioStatusRepository usuarioStatusRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;
        private PosicaoRepository posicaoRedeRepository;
        private UsuarioFactory usuarioFactory;
        private UsuarioService usuarioService;

        private ProdutoRepository produtoRepository;
        private ContaDepositoRepository contaDepositoRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;

        private PedidoFactory pedidoFactory;
        private Core.Services.Loja.PedidoService pedidoService;
        private CartaoCreditoRepository cartaoCreditoRepository;

        private Core.Helpers.TraducaoHelper traducaoHelper;
        private CicloRepository cicloRepository;

        public CadastroController(DbContext context)
        {
            paisRepository = new PaisRepository(context);
            estadoRepository = new EstadoRepository(context);
            cidadeRepository = new CidadeRepository(context);
            geolocalizacaoService = new GeolocalizacaoService(context);
            usuarioRepository = new UsuarioRepository(context);
            usuarioStatusRepository = new UsuarioStatusRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            posicaoRedeRepository = new PosicaoRepository(context);
            usuarioFactory = new UsuarioFactory(context);
            usuarioService = new UsuarioService(context);
            produtoRepository = new ProdutoRepository(context);
            contaDepositoRepository = new ContaDepositoRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoFactory = new PedidoFactory(context);
            pedidoService = new Core.Services.Loja.PedidoService(context);
            cartaoCreditoRepository = new CartaoCreditoRepository(context);
            cicloRepository = new CicloRepository(context);
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


        private void Localizacao(int paisID)
        {
            var pais = paisRepository.Get(paisID);

            Localizacao(pais);
        }

        private void Localizacao(string sigla = null)
        {
            Pais pais = null;
            if (!String.IsNullOrEmpty(sigla))
            {
                pais = paisRepository.GetBySigla(sigla);
            }
            if (pais == null)
            {
                pais = geolocalizacaoService.GetByIP();
            }
            if (pais == null && Request.UserLanguages.Any())
            {
                pais = paisRepository.GetBySigla(Request.UserLanguages[0]);
            }
            if (pais == null)
            {
                pais = paisRepository.GetPadrao();
            }

            Localizacao(pais);
        }

        private void Localizacao(Pais pais = null)
        {
            ViewBag.Paises = paisRepository.GetDisponiveis();
            ViewBag.Pais = pais;
            Session["pais"] = pais;

            ViewBag.Estados = pais != null ? estadoRepository.GetByPais(pais.ID) : null;

            if (pais != null)
            {
                ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(pais.Idioma);
                traducaoHelper = new TraducaoHelper(pais.Idioma);

                var culture = new System.Globalization.CultureInfo(pais.Idioma.Sigla);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                Session["PaisID"] = pais.ID;
            }
        }

        private void Fundos()
        {
            ViewBag.Fundos = ArquivoRepository.BuscarArquivos(Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO"), Core.Helpers.ConfiguracaoHelper.GetString("PASTA_FUNDOS"), "*.jpg");
        }

        public struct lstCidade
        {
            public int ID;
            public string Nome;

            public lstCidade(Core.Entities.Cidade cidade)
            {
                this.ID = cidade.ID;
                this.Nome = cidade.Nome;
            }
        }

        #endregion

        #region Validacao

        public JsonResult DataValidar(string DataNascimento)
        {
            DateTime _data;
            string idioma = "pt-BR";

            Pais pais = (Pais)Session["pais"];

            if (pais != null)
            {
                idioma = pais.Idioma.Sigla;
            }

            bool blnChecar = DateTime.TryParse(DataNascimento, new CultureInfo(idioma), DateTimeStyles.None, out _data) ? true : false;

            return Json(blnChecar, JsonRequestBehavior.AllowGet);

        }

        public JsonResult DocumentoValidar(string Documento)
        {
            string idioma = "pt-BR";
            bool retorno = true;

            Pais pais = (Pais)Session["pais"];

            if (pais != null)
            {
                idioma = pais.Idioma.Sigla;
            }

            //if (idioma == "pt-BR")
            //{
            //    //se brasil valida cpf
            //    retorno = cpUtilities.Validacoes.ValidaCPF(Documento);

            //    if (!retorno)
            //    {
            //        return Json("CPF inválido", JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_DOCUMENTO_EM_USO") &&
            //            ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_DOCUMENTO_EM_USO"))
            //        {
            //            retorno = usuarioRepository.GetByDocumento(Documento) == null ? true : false;
            //            if (!retorno)
            //            {
            //                return Json("CPF já cadastrado", JsonRequestBehavior.AllowGet);
            //            }
            //        }
            //    }
            //}

            return Json(retorno, JsonRequestBehavior.AllowGet);

        }

        public JsonResult EmailValidar(string Email)
        {
            bool retorno = true;
            if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_EMAIL_EM_USO") &&
                ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_EMAIL_EM_USO"))
            {
                retorno = usuarioRepository.GetByEmail(Email) == null ? true : false;
            }

            return Json(retorno, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoginValidar(string Login)
        {
            bool retorno = false;

            Regex rg = new Regex(@"^[a-zA-Z0-9]+$");

            if (rg.IsMatch(Login))
            {
                retorno = usuarioRepository.GetByLogin(Login) == null ? true : false;
            }

            return Json(retorno, JsonRequestBehavior.AllowGet);
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

        public ActionResult Index(string sigla = null)
        {
            return null;

            //id do patrocinador
            string strUser = Request["id"];

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();
            if (String.IsNullOrEmpty(sigla))
            {
                if (Session["sigla"] != null)
                {
                    sigla = Session["sigla"].ToString();
                }
            }
            else
            {
                Session["sigla"] = sigla;
            }

            ViewBag.SenhaComplexa = ConfiguracaoHelper.GetBoolean("SENHA_COMPLEXA");
            ViewBag.TipoPessoa = "F";
            Localizacao(sigla);
            Fundos();
            if (!String.IsNullOrEmpty(strUser))
            {
                //caso o id do Usuario seja passado ja entra no patrocinador
                return RedirectToAction("BuscarPatrocinador", new { login = strUser });
            }

            return View();
        }

        public async Task<ActionResult> Pais_Changed(FormCollection form)
        {
            var sigla = form["sigla"];

            Localizacao(sigla);

            return View("Index");
        }

        public async Task<ActionResult> Pais_MeusDados_Changed(FormCollection form)
        {
            var paisID = int.Parse(form["pais"]);

            Localizacao(paisID);

            var patrocinadorID = int.Parse(form["PatrocinadorID"]);
            ViewBag.patrocinadorID = patrocinadorID;
            ViewBag.patrocinador = usuarioRepository.Get(patrocinadorID);

            return View("Index");
        }

        public async Task<ActionResult> Salvar(FormCollection form)
        {
            #region localizacao

            string getIdioma = "pt-BR";

            Pais getPais = (Pais)Session["pais"];

            if (getPais != null)
            {
                getIdioma = getPais.Idioma.Sigla;
            }

            Localizacao(getIdioma);

            #endregion

            #region Variaveis

            Usuario patrocinador;
            string login;
            int estadoID = 0;
            int cidadeID = 0;
            int paisID = 0;
            int estadoID2 = 0;
            int cidadeID2 = 0;
            string strErro = "";

            try
            {
                int result;

                //Patrocinador    
                patrocinador = usuarioRepository.Get(int.Parse(form["PatrocinadorID"]));
                login = form["Login"];
                paisID = int.Parse(form["PaisID"]);
                if (form["EstadoID"] != null && int.TryParse(form["EstadoID"], out result))
                    estadoID = result;
                if (form["CidadeID"] != null && int.TryParse(form["CidadeID"], out result))
                    cidadeID = result;
                if (form["EstadoID2"] != null && int.TryParse(form["EstadoID2"], out result))
                    estadoID2 = result;
                if (form["CidadeID2"] != null && int.TryParse(form["CidadeID2"], out result))
                    cidadeID2 = result;

            }
            catch (Exception ex)
            {
                //Caso haja erro nos paramentros basicos volta a tela inicial
                string[] strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], "Cod 3427", ex.Message };
                Mensagem(traducaoHelper["MENSAGEM_ERRO"], strMensagem, "err");
                obtemMensagem();
                return View("Index");
            }

            //Usuario
            string strApelido = "";
            string strAssinatura = "";
            string strCelular = string.IsNullOrWhiteSpace(form["Celular"]) ? "0" : form["Celular"];
            //strCelular = Regex.Replace(strCelular, @"[^\d]", ""); // == so numeros
            string strDataNascimento = form["DataNascimento"];
            //string strDocumento = form["Documento"];

            string strTpPessoa = form["TipoPessoa"];
            string strDocPF = form["DocPF"];
            string strDocPJ = form["DocPJ"];

            string strEmail = form["Email"];
            string strConfirmarEmail = form["ConfirmarEmail"];
            string strLogin = form["Login"];
            string strNome = form["Nome"];
            string strNomeFantasia = string.IsNullOrWhiteSpace(form["NomeFantasia"]) ? form["Nome"] : form["NomeFantasia"];
            string strSenha = form["Senha"];
            string strConfirmarSenha = form["ConfirmarSenha"];
            string strSexo = form["Sexo"];
            string strTelefone = string.IsNullOrWhiteSpace(form["Telefone"]) ? "0" : form["Telefone"];
            //strTelefone = Regex.Replace(strTelefone, @"[^\d]", "");  // == so numeros
            string strIdioma = form["Idioma"];

            //Endereço Principal
            string strCodigoPostal = string.IsNullOrWhiteSpace(form["CodigoPostal"]) ? "" : form["CodigoPostal"].Remove(5, 1);
            string strComplemento = string.IsNullOrWhiteSpace(form["Complemento"]) ? "" : form["Complemento"];
            string strDistrito = string.IsNullOrWhiteSpace(form["Bairro"]) ? "" : form["Bairro"];
            string strLogradouro = string.IsNullOrWhiteSpace(form["Logradouro"]) ? "" : form["Logradouro"];
            string strNumero = string.IsNullOrWhiteSpace(form["Numero"]) ? "" : form["Numero"];

            //Endereço Alternativo
            string strCodigoPostal2 = string.IsNullOrWhiteSpace(form["CodigoPostal2"]) ? "" : form["CodigoPostal2"].Remove(5, 1);
            string strComplemento2 = string.IsNullOrWhiteSpace(form["Complemento2"]) ? "" : form["Complemento2"];
            string strDistrito2 = string.IsNullOrWhiteSpace(form["Bairro2"]) ? "" : form["Bairro2"];
            string strLogradouro2 = string.IsNullOrWhiteSpace(form["Logradouro2"]) ? "" : form["Logradouro2"];
            string strNumero2 = string.IsNullOrWhiteSpace(form["Numero2"]) ? "" : form["Numero2"];
            string strObservacao2 = string.IsNullOrWhiteSpace(form["Observacoes2"]) ? "" : form["Observacoes2"];

            //Termo de Aceite
            Boolean blnTermoAceite = string.IsNullOrWhiteSpace(form["chkTermos"]) ? false : true;

            //Inconsistencia
            List<string> lstMensagem = new List<string>();

            #endregion

            #region ViewBags

            //Caso retorne ao form
            ViewBag.Patrocinador = patrocinador;
            ViewBag.patrocinadorID = int.Parse(form["PatrocinadorID"]);
            ViewBag.Login = login;
            ViewBag.EstadoID = form["EstadoID"];
            ViewBag.CidadeID = form["CidadeID"];
            ViewBag.PaisID = paisID;
            ViewBag.EstadoID2 = form["EstadoID2"];
            ViewBag.CidadeID2 = form["CidadeID2"];

            ViewBag.Celular = strCelular;
            ViewBag.DataNascimento = strDataNascimento;
            //ViewBag.Documento = strDocumento;

            ViewBag.TipoPessoa = strTpPessoa;
            ViewBag.DocPF = strDocPF;
            ViewBag.DocPJ = strDocPJ;

            ViewBag.Email = strEmail;
            ViewBag.ConfirmarEmail = strConfirmarEmail;
            ViewBag.Login = strLogin;
            ViewBag.Nome = strNome;
            ViewBag.NomeFantasia = strNomeFantasia;
            ViewBag.Senha = strSenha;
            ViewBag.ConfirmarSenha = strConfirmarSenha;
            ViewBag.Sexo = strSexo;
            ViewBag.Telefone = strTelefone;
            ViewBag.Idioma = strIdioma;

            ViewBag.CodigoPostal = strCodigoPostal;
            ViewBag.Complemento = strComplemento;
            ViewBag.Bairro = strDistrito;
            ViewBag.Logradouro = strLogradouro;
            ViewBag.Numero = strNumero;

            ViewBag.CodigoPostal2 = strCodigoPostal2;
            ViewBag.Complemento2 = strComplemento2;
            ViewBag.Bairro2 = strDistrito2;
            ViewBag.Logradouro2 = strLogradouro2;
            ViewBag.Numero2 = strNumero2;
            ViewBag.Observacoes2 = strObservacao2;

            #endregion

            #region ReCaptha

            strErro = "";
            if (WebConfigurationManager.AppSettings["Ambiente"] != "dev" && Core.Helpers.ConfiguracaoHelper.GetBoolean("RECAPTCHA_CADASTRO_ATIVO"))
            {
                strErro = "reCaptha";
                //Obtem valor do reCaptha do google
                string recaptcha = Request.Form["g-recaptcha-response"];
                string postData = "secret=" + Core.Helpers.ConfiguracaoHelper.GetString("RECAPTCHA_SECRET_KEY") + "&response=" + recaptcha;
                string strSenhaConfirmar = Request.Form["senhaConfirmar"];

                try
                {
                    strErro = "webClient";
                    using (var webClient = new System.Net.WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        var json2 = webClient.DownloadString("https://www.google.com/recaptcha/api/siteverify?" + postData);
                        dynamic data = JObject.Parse(json2);
                        if (data != null)
                        {
                            if (data["success"].ToString() != "True")
                            {
                                lstMensagem.Add(traducaoHelper["RECAPTCHA_INCORRETO"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lstMensagem.Add(traducaoHelper["RECAPTCHA_INCORRETO"]);
                }
            }

            #endregion

            #region Consistencia

            //Nome
            if (String.IsNullOrEmpty(strNome))
            {
                lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (strNome.Length < 3)
                {
                    lstMensagem.Add(traducaoHelper["NOME"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
                }
            }

            //Login
            if (String.IsNullOrEmpty(strLogin))
            {
                lstMensagem.Add(traducaoHelper["LOGIN"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (strLogin.Length < 3)
                {
                    lstMensagem.Add(traducaoHelper["LOGIN"] + ": " + traducaoHelper["MINIMO_3_CARACTERES"]);
                }
            }

            //Data
            if (String.IsNullOrEmpty(strDataNascimento))
            {
                lstMensagem.Add(traducaoHelper["DATA"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                DateTime _data;
                string idioma = "pt-BR";

                Pais pais = (Pais)Session["pais"];

                if (pais != null)
                {
                    idioma = pais.Idioma.Sigla;
                }
                if (!(DateTime.TryParse(strDataNascimento, new CultureInfo(idioma), DateTimeStyles.None, out _data) ? true : false))
                {
                    lstMensagem.Add(traducaoHelper["DATA"] + ": " + traducaoHelper["DATA_INVALIDA"]);
                }
            }

            //Documento
            //string strDocumento = string.Empty;
            //if (String.IsNullOrEmpty(strDocPF) && String.IsNullOrEmpty(strDocPJ))
            //{
            //    lstMensagem.Add(traducaoHelper["DOCUMENTO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            //}
            //else
            //{
            //    string idioma = "pt-BR";
            //    bool retorno = true;

            //    if (strTpPessoa == "F")               
            //        strDocumento = strDocPF;              
            //    else               
            //        strDocumento = strDocPJ;

            //    Pais pais = (Pais)Session["pais"];

            //    if (pais != null)
            //    {
            //        idioma = pais.Idioma.Sigla;
            //    }

            //    if (idioma == "pt-BR")
            //    {
            //        //se brasil valida CPF/CNPJ
            //        if (strTpPessoa == "F")
            //            retorno = cpUtilities.Validacoes.ValidaCPF(strDocumento);                    
            //        else                    
            //            retorno = cpUtilities.Validacoes.ValidaCNPJ(strDocumento);                  

            //        if (!retorno)
            //        {
            //            lstMensagem.Add(traducaoHelper["DOCUMENTO"] + ": " + traducaoHelper["DOCUMENTO_INVALIDO"]);
            //        }
            //        else
            //        {
            //            if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_DOCUMENTO_EM_USO") &&
            //                ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_DOCUMENTO_EM_USO"))
            //            {
            //                retorno = usuarioRepository.GetByDocumento(strDocumento) == null ? true : false;
            //                if (!retorno)
            //                {
            //                    lstMensagem.Add(traducaoHelper["DOCUMENTO"] + ": " + traducaoHelper["DOCUMENTO_JA_CADASTRADO"]);
            //                }
            //            }
            //            else
            //            {
            //                int QtdeRepeticoes = ConfiguracaoHelper.GetInt("CADASTRO_DOCUMENTO_QTDE_REPETICOES");
            //                if (QtdeRepeticoes > 0)
            //                {
            //                    int TotalCadastros = usuarioRepository.CountByDocumento(strDocumento);
            //                    if (TotalCadastros>= QtdeRepeticoes)
            //                    {
            //                        lstMensagem.Add(traducaoHelper["DOCUMENTO"] + ": " + traducaoHelper["DOCUMENTO_JA_ATINGIU_LIMITE_CADASTROS"]);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            if (ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO") == "true")
            {
                int cep;

                //CEP
                if (String.IsNullOrEmpty(strCodigoPostal))
                {
                    lstMensagem.Add(traducaoHelper["CODIGO_POSTAL"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                if (!int.TryParse(strCodigoPostal, out cep))
                {
                    lstMensagem.Add(traducaoHelper["CODIGO_POSTAL"] + ": Formato de CEP principal inválido");
                }
                //Numero
                if (String.IsNullOrEmpty(strNumero))
                {
                    lstMensagem.Add(traducaoHelper["NUMERO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                //Estado
                if (estadoID == 0)
                {
                    lstMensagem.Add(traducaoHelper["ESTADO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                //Cidade
                if (cidadeID == 0)
                {
                    lstMensagem.Add(traducaoHelper["CIDADE"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                //Logradouro
                if (String.IsNullOrEmpty(strLogradouro))
                {
                    lstMensagem.Add(traducaoHelper["LOGRADOURO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                //Celular
                if (String.IsNullOrEmpty(strCelular))
                {
                    lstMensagem.Add(traducaoHelper["CELULAR"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                }
                else
                {
                    if (strCelular.Length < 6)
                    {
                        lstMensagem.Add(traducaoHelper["CELULAR"] + ": " + traducaoHelper["MINIMO_6_CARACTERES"]);
                    }
                }

                Int64 celular;
                if (!Int64.TryParse(strCelular, out celular))
                {
                    lstMensagem.Add(traducaoHelper["CELULAR"] + ": Formato inválido");
                }

                //Endereço Alternativo
                if (!String.IsNullOrEmpty(strLogradouro2) || !String.IsNullOrEmpty(strCodigoPostal2) || cidadeID2 > 0 || estadoID2 > 0)
                {
                    //CEP
                    if (String.IsNullOrEmpty(strCodigoPostal2))
                    {
                        lstMensagem.Add(traducaoHelper["CODIGO_POSTAL"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                    }
                    if (!int.TryParse(strCodigoPostal2, out cep))
                    {
                        lstMensagem.Add(traducaoHelper["CODIGO_POSTAL"] + ": Formato de CEP alternativo inválido");
                    }
                    //Numero
                    if (String.IsNullOrEmpty(strNumero2))
                    {
                        lstMensagem.Add(traducaoHelper["NUMERO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                    }
                    //Estado
                    if (estadoID2 == 0)
                    {
                        lstMensagem.Add(traducaoHelper["ESTADO"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                    }
                    //Cidade
                    if (cidadeID2 == 0)
                    {
                        lstMensagem.Add(traducaoHelper["CIDADE"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                    }
                    //Observacao
                    if (String.IsNullOrEmpty(strObservacao2))
                    {
                        lstMensagem.Add(traducaoHelper["OBSERVACOES"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
                    }
                }

            }

            //Email
            if (String.IsNullOrEmpty(strEmail))
            {
                lstMensagem.Add(traducaoHelper["EMAIL"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (!cpUtilities.Validacoes.ValidaEmail(strEmail))
                {
                    lstMensagem.Add(traducaoHelper["EMAIL"] + ": " + traducaoHelper["DIGITE_EMAIL_VALIDO"]);
                }
                else
                {
                    if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_EMAIL_EM_USO") &&
                        ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_EMAIL_EM_USO"))
                    {
                        if (!(usuarioRepository.GetByEmail(strEmail) == null ? true : false))
                        {
                            lstMensagem.Add(traducaoHelper["EMAIL"] + ": " + traducaoHelper["EMAIL_EM_USO"]);
                        }
                    }
                }
            }

            //Confirmar Email
            if (strEmail != strConfirmarEmail)
            {
                lstMensagem.Add(traducaoHelper["EMAIL"] + ": " + traducaoHelper["DIGITE_MESMO_EMAIL"]);
            }

            //Senha
            if (String.IsNullOrEmpty(strSenha))
            {
                lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["CAMPO_REQUERIDO"]);
            }
            else
            {
                if (strSenha.Length < 6)
                {
                    lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["MINIMO_6_CARACTERES"]);
                }
                else
                {
                    if (ConfiguracaoHelper.GetBoolean("SENHA_COMPLEXA"))
                    {
                        if (strSenha.Length < 6)
                        {
                            lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["MINIMO_6_CARACTERES"]);
                        }
                        else
                        {
                            if (!(strSenha.Any(x => char.IsDigit(x))))
                            {
                                lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"]);
                            }
                            else
                            {
                                if (!(strSenha.Any(x => char.IsLetter(x))))
                                {
                                    lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"]);
                                }
                                else
                                {
                                    if (!(strSenha.Any(x => char.IsLower(x))))
                                    {
                                        lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"]);
                                    }
                                    else
                                    {
                                        if (!(strSenha.Any(x => char.IsUpper(x))))
                                        {
                                            lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"]);
                                        }
                                        else
                                        {
                                            char[] SpecialChars = @"!@#$%&*()-+=|\/:.,_;".ToCharArray();
                                            int indexOf = strSenha.IndexOfAny(SpecialChars);
                                            if (indexOf == -1)
                                            {
                                                lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Confirmar Senha
            if (strSenha != strConfirmarSenha)
            {
                lstMensagem.Add(traducaoHelper["SENHA"] + ": " + traducaoHelper["DIGITE_MESMA_SENHA"]);
            }

            //Termo de Aceite
            if (!blnTermoAceite)
            {
                lstMensagem.Add(traducaoHelper["CONFIRMA_LEITURA_TERMO_ACEITE"]);
            }

            if (lstMensagem.Count() > 0)
            {
                string[] strMensagem = lstMensagem.ToArray();
                Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                obtemMensagem();
                return View("Index");
            }

            #endregion

            #region identity

            strErro = "";
            string strNovaSenha = cpUtilities.Gerais.GerarChave() + strEmail;
            var user = new ApplicationUser
            {
                UserName = strLogin,
                Email = strNovaSenha
            };
            var adminresult = await UserManager.CreateAsync(user, strSenha);

            //Add User to the selected Groups
            if (adminresult.Succeeded)
            {
                int[] selectedGroups = new int[] { 3 };
                if (selectedGroups != null)
                {
                    selectedGroups = selectedGroups ?? new int[] { };
                    await this.GroupManager.SetUserGroupsAsync(user.Id, selectedGroups);
                }
            }
            else
            {
                string strValor = "";
                //adminresult.Errors strErro = new adminresult.Errors();
                foreach (string item in adminresult.Errors)
                {
                    strValor = item.Replace("Name", "Nome");
                    strValor = strValor.Replace("is already taken", "já existe");
                    strValor = strValor.Replace("is invalid", "não é valido");
                    strErro += strValor + "; ";
                }
                strErro += ".";
                strErro = strErro.Replace("; .", "");

                string[] strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], strErro };
                Mensagem(traducaoHelper["ERRO"], strMensagem, "err");
                obtemMensagem();
                return View("Index");

            }

            #endregion

            #region Criar Usuario

            DateTime? dtMigracao = null;
            if (ConfiguracaoHelper.GetBoolean("EXIGI_MIGRACAO"))
            {
                dtMigracao = App.DateTimeZion;
            }

            Usuario usuario = new Usuario()
            {
                Apelido = strApelido,
                Assinatura = strAssinatura,
                Celular = strCelular,
                DataNascimento = DateTime.Parse(strDataNascimento, new CultureInfo(strIdioma)),
                //Derramamento = Usuario.Derramamentos.Indefinido,
                //Entrada = patrocinador.Derramamento,
                Documento = string.Empty, //strDocumento,
                Email = strEmail,
                GeraBonus = true,
                Login = strLogin,
                NivelAssociacao = 0,
                NivelClassificacao = 0,
                Nome = form["Nome"],
                NomeFantasia = strNomeFantasia,
                PaisID = paisID,
                PatrocinadorDiretoID = patrocinador.ID,
                ProfundidadeRede = 0,
                RecebeBonus = true,
                Senha = strSenha,
                Sexo = strSexo,
                Status = Usuario.TodosStatus.NaoAssociado,
                StatusCelular = Usuario.TodosStatusCelular.NaoValidado,
                StatusEmail = Usuario.TodosStatusEmail.NaoValidado,
                Telefone = strTelefone,
                Tipo = Usuario.Tipos.Indefinido,
                EmpresaID = patrocinador.EmpresaID,
                FilialID = 1,
                IdAutenticacao = user.Id,
                TermoAceite = true,
                DataMigracao = dtMigracao,
                ExibeSaque = 1
            };

            var lstEnderecos = new List<Endereco>();

            var endereco = new Endereco();
            if (estadoID == 0)
            {
                endereco.CidadeID = 1;
                endereco.CodigoPostal = "";
                endereco.Complemento = "";
                endereco.Distrito = "";
                endereco.EstadoID = 1;
                endereco.Logradouro = "";
                endereco.Numero = "";
                endereco.Principal = true;
            }
            else
            {
                endereco.CidadeID = cidadeID;
                endereco.CodigoPostal = strCodigoPostal;
                endereco.Complemento = strComplemento;
                endereco.Distrito = strDistrito;
                endereco.EstadoID = estadoID;
                endereco.Logradouro = strLogradouro;
                endereco.Numero = strNumero;
                endereco.Principal = true;
            }

            lstEnderecos.Add(endereco);

            var endereco2 = new Endereco();
            if (estadoID2 > 0)
            {
                endereco2.CidadeID = cidadeID2;
                endereco2.CodigoPostal = strCodigoPostal2;
                endereco2.Complemento = strComplemento2;
                endereco2.Distrito = strDistrito2;
                endereco2.EstadoID = estadoID2;
                endereco2.Logradouro = strLogradouro2;
                endereco2.Numero = strNumero2;
                endereco2.Principal = false;
                endereco2.Observacoes = strObservacao2;

                lstEnderecos.Add(endereco2);
            }

            usuario = usuarioFactory.Criar(usuario, lstEnderecos);

            try
            {
                if (ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
                {
                    //Chama sp EXEC spOC_US_RedeUpline_Ciclo
                    cicloRepository.RedeUplineCiclo(usuario.ID);
                }
            }
            catch (Exception ex)
            {
                //logar erro
            }

            #endregion

            #region Enviar email

            EnviarValidacaoEmail(login);

            #endregion

            //#region Posiciona Rede Sem Pagamento
            if (ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
            {
                Boolean blnArvoreBinaria = false;
                if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
                    blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

                if (blnArvoreBinaria)
                {
                    usuarioService.Associar(usuario.ID, 0);
                }
                else
                {
                    if (ConfiguracaoHelper.GetBoolean("REDE_PREENCHIMENTO_SEQUENCIAL"))
                        usuarioService.AssociarRedeSequencia(usuario.ID, 0);
                    else
                        usuarioService.AssociarRedeHierarquia(usuario.ID, 0);
                }
            }
            //#endregion

            #region Pagamento

            if (Core.Helpers.ConfiguracaoHelper.GetString("CADASTRO_PAGAMENTO_NA_HORA").ToLower() == "true")
            {
                string codigo = CriptografiaHelper.Criptografar(usuario.Login);
                return RedirectToAction("Ativar", new { codigo = codigo });
            }

            #endregion

            return RedirectToAction("sucesso", new { login = usuario.Login });
        }

        public ActionResult BuscarPatrocinador(string login)
        {
            //limpa carrinho
            Session["_carrinho"] = null;

            //Remove autenticação caso ela exista.
            HttpContext.GetOwinContext().Authentication.SignOut();
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            var patrocinador = usuarioRepository.GetByLogin(login);
            if (patrocinador != null && patrocinador.Status == Usuario.TodosStatus.Associado &&
                !patrocinador.Bloqueado && !patrocinador.Oculto)
            {
                Containers.UsuarioContainer patrocinadorContainer = new Containers.UsuarioContainer(patrocinador);
                // if(!Core.Helpers//.ConfiguracaoHelper.GetBoolean("EXIGI_MIGRACAO"))
                if (!patrocinadorContainer.PendenteMigracao)
                {
                    ViewBag.Patrocinador = patrocinador;

                    string param = null;
                    Localizacao(param);
                    Fundos();

                    if (Session["LoginExiste"] == null)
                    {
                        Session["LoginExiste"] = false;
                    }

                    ViewBag.Erro = Session["Erro"];
                    ViewBag.LoginExiste = Session["LoginExiste"];

                    return View("Index");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EnviarValidacaoEmail(string login)
        {
            var usuario = usuarioRepository.GetByLogin(login);
            if (usuario != null)
            {
                try
                {
                    usuarioService.EnviarValidacaoEmail(usuario, Helpers.Local.Sistema);
                }
                catch (Exception ex)
                {
                    cpUtilities.LoggerHelper.WriteFile("erro envio de email " + usuario.Email + " : " + ex.Message, "CadastroEnvioEmail");
                }
            }
            return RedirectToAction("sucesso", new { login = login });
        }

        public ActionResult Sucesso(string login)
        {
            var usuario = usuarioRepository.GetByLogin(login);
            if (usuario != null)
            {
                //if (ConfiguracaoHelper.GetString("ASSOCIACAO_PAGTO_VIA_LOJA") == "true")
                //{
                //    Autentica(usuario.Login);
                //    return RedirectToAction("Index", "Loja", new { novoAssociado = true });
                //}
                //else
                //{
                Localizacao(usuario.Pais);
                Fundos();
                return View(usuario);
                //}
            }
            return RedirectToAction("index");
        }

        //public ActionResult SucessoCartao(string login)
        //{
        //    var usuario = usuarioRepository.GetByLogin(login);
        //    if (usuario != null)
        //    {
        //        Localizacao(usuario.Pais);
        //        Fundos();
        //        return View(usuario);
        //    }
        //    return RedirectToAction("index");
        //}

        public ActionResult Ativar(string codigo)
        {
            var usuario = usuarioService.ValidarEmail(codigo);
            if (usuario != null)
            {
                Localizacao(usuario.PaisID);
                return RedirectToAction("Login", "Account", new
                {
                    strPopupTitle = traducaoHelper["SUCESSO"],
                    strPopupMessage = traducaoHelper["EMAIL"] + " " + traducaoHelper["VALIDADO"]
                });
            }
            return RedirectToAction("index");
        }

        //public ActionResult Associacao(string login)
        //{

        //    if (String.IsNullOrEmpty(login))
        //    {
        //        return RedirectToAction("index");
        //    }

        //    #region Associacao

        //    var usuario = usuarioRepository.GetByLogin(login);

        //    if (usuario.StatusEmail != Usuario.TodosStatusEmail.Validado)
        //    {
        //        return RedirectToAction("sucesso", new { login = usuario.Login });
        //    }
        //    if (usuario.Status == Usuario.TodosStatus.NaoAssociado || usuario.NivelAssociacao.Equals(0))
        //    {
        //        var pedido = usuario.Pedido.Where(p => p.PedidoItem.Any(i => i.Produto.TipoID == (int)Core.Entities.Produto.Tipos.Associacao)).OrderByDescending(p => p.DataCriacao).FirstOrDefault();
        //        if (pedido != null)
        //        {
        //            if (pedido.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao || pedido.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
        //            {
        //                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
        //                return RedirectToAction("pagar", "deposito", new { pagamentoID = pagamento.ID });
        //            }
        //        }

        //        var listaProdutos = produtoRepository.GetByTipo(Produto.Tipos.Associacao).OrderBy(Prd => Prd.SKU).ToList();

        //        ViewBag.Produtos = listaProdutos;
        //        Localizacao(usuario.Pais);
        //        Fundos();

        //        ViewBag.CartaoNome = Session["CartaoNome"];
        //        ViewBag.CartaoBandeira = Session["CartaoBandeira"];
        //        ViewBag.CartaoNumero = Session["CartaoNumero"];
        //        ViewBag.CartaoCodSeguranca = Session["CartaoCodSeguranca"];
        //        ViewBag.CartaoMes = Session["CartaoMes"];
        //        ViewBag.CartaoAno = Session["CartaoAno"];
        //        ViewBag.CartaoLogin = Session["CartaoLogin"];
        //        ViewBag.CartaoProduto = Session["CartaoProduto"];

        //        #region Bandeira

        //        List<Object> bandeira = new List<object>();
        //        bandeira.Add(new { nome = "Visa", id = "visa" });
        //        bandeira.Add(new { nome = "Mastercard", id = "mastercard" });
        //        bandeira.Add(new { nome = "Maestro", id = "maestro" });
        //        bandeira.Add(new { nome = "American Express", id = "americanexpress" });
        //        bandeira.Add(new { nome = "Elo", id = "elo" });
        //        bandeira.Add(new { nome = "Diners Club", id = "diners" });
        //        bandeira.Add(new { nome = "JCB", id = "jcb" });
        //        bandeira.Add(new { nome = "Aura", id = "aura" });

        //        if (ViewBag.CartaoBandeira != null)
        //        {
        //            ViewBag.ccBandeira = new SelectList(bandeira, "id", "nome", ViewBag.CartaoBandeira);
        //        }
        //        else
        //        {
        //            ViewBag.ccBandeira = new SelectList(bandeira, "id", "nome");
        //        }

        //        #endregion

        //        #region Mes

        //        List<Object> mes = new List<object>();
        //        mes.Add(new { nome = traducaoHelper["JANEIRO"], id = "01" });
        //        mes.Add(new { nome = traducaoHelper["FEVEREIRO"], id = "02" });
        //        mes.Add(new { nome = traducaoHelper["MARCO"], id = "03" });
        //        mes.Add(new { nome = traducaoHelper["ABRIL"], id = "04" });
        //        mes.Add(new { nome = traducaoHelper["MAIO"], id = "05" });
        //        mes.Add(new { nome = traducaoHelper["JUNHO"], id = "06" });
        //        mes.Add(new { nome = traducaoHelper["JULHO"], id = "07" });
        //        mes.Add(new { nome = traducaoHelper["AGOSTO"], id = "08" });
        //        mes.Add(new { nome = traducaoHelper["SETEMBRO"], id = "09" });
        //        mes.Add(new { nome = traducaoHelper["OUTUBRO"], id = "10" });
        //        mes.Add(new { nome = traducaoHelper["NOVEMBRO"], id = "11" });
        //        mes.Add(new { nome = traducaoHelper["DEZEMBRO"], id = "12" });

        //        if (ViewBag.CartaoMes != null)
        //        {
        //            ViewBag.ccMes = new SelectList(mes, "id", "nome", ViewBag.CartaoMes);
        //        }
        //        else
        //        {
        //            ViewBag.ccMes = new SelectList(mes, "id", "nome");
        //        }

        //        #endregion

        //        #region Ano

        //        List<Object> ano = new List<object>();
        //        ano.Add(new { nome = "2016", id = "2016" });
        //        ano.Add(new { nome = "2017", id = "2017" });
        //        ano.Add(new { nome = "2018", id = "2018" });
        //        ano.Add(new { nome = "2019", id = "2019" });
        //        ano.Add(new { nome = "2020", id = "2020" });
        //        ano.Add(new { nome = "2021", id = "2021" });
        //        ano.Add(new { nome = "2022", id = "2022" });
        //        ano.Add(new { nome = "2023", id = "2023" });
        //        ano.Add(new { nome = "2024", id = "2024" });
        //        ano.Add(new { nome = "2025", id = "2025" });
        //        ano.Add(new { nome = "2026", id = "2026" });
        //        ano.Add(new { nome = "2027", id = "2027" });

        //        if (ViewBag.CartaoAno != null)
        //        {
        //            ViewBag.ccAno = new SelectList(ano, "id", "nome", ViewBag.CartaoAno);
        //        }
        //        else
        //        {
        //            ViewBag.ccAno = new SelectList(ano, "id", "nome");
        //        }

        //        #endregion

        //        #region Alertas

        //        if (Session["Erro"] != null)
        //        {
        //            ViewBag.AlertErroTitulo = Session["ErroTitulo"];
        //            ViewBag.AlertErro = Session["Erro"];
        //            Session["Erro"] = null;
        //            Session["ErroTitulo"] = null;
        //        }

        //        if (Session["Sucesso"] != null)
        //        {
        //            ViewBag.AlertSucessoTitulo = Session["SucessoTitulo"];
        //            ViewBag.AlertSucesso = Session["Sucesso"];
        //            Session["Sucesso"] = null;
        //            Session["SucessoTitulo"] = null;
        //        }

        //        if (Session["Info"] != null)
        //        {
        //            ViewBag.AlertInfoTitulo = Session["InfoTitulo"];
        //            ViewBag.AlertInfo = Session["Info"];
        //            Session["Info"] = null;
        //            Session["InfoTitulo"] = null;
        //        }


        //        if (Session["ShowCartao"] != null)
        //        {
        //            ViewBag.ShowCartao = Session["ShowCartao"];
        //            Session["ShowCartao"] = null;
        //        }

        //        #endregion

        //        Session["CartaoNome"] = null;
        //        Session["CartaoBandeira"] = null;
        //        Session["CartaoNumero"] = null;
        //        Session["CartaoCodSeguranca"] = null;
        //        Session["CartaoMes"] = null;
        //        Session["CartaoAno"] = null;
        //        Session["CartaoLogin"] = null;
        //        Session["CartaoProduto"] = null;

        //        return View(usuario);
        //    }

        //    #endregion

        //    return RedirectToAction("index");
        //}

        //public ActionResult Associar(string login, int produtoID, int meioPagamento)
        //{

        //    PedidoPagamento pagamento;

        //    var usuario = usuarioRepository.GetByLogin(login);
        //    var pedido = usuario.Pedido.Where(p => p.PedidoItem.Any(i => i.Produto.TipoID == (int)Core.Entities.Produto.Tipos.Associacao)).OrderByDescending(p => p.DataCriacao).FirstOrDefault();
        //    if (pedido != null && produtoID != 1)
        //    {
        //        if (pedido.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao || pedido.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
        //        {
        //            pagamento = pedido.PedidoPagamento.FirstOrDefault();
        //            return RedirectToAction("pagar", "deposito", new { pagamentoID = pagamento.ID });
        //        }
        //    }

        //    var carrinho = new Core.Models.Loja.CarrinhoModel(usuario);
        //    var produto = produtoRepository.Get(produtoID);
        //    var valor = produto.ValorMinimo(usuario);

        //    /*
        //    if (produto.SKU != "ADE0001")
        //    {
        //        var adesao = produtoRepository.GetBySKU("ADE0001");
        //        var valorAdesao = adesao.ValorMinimo(usuario);
        //        carrinho.Adicionar(adesao, valorAdesao);
        //    }
        //     */

        //    carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;

        //    if (usuario.EnderecoAlternativo != null && usuario.EnderecoAlternativo.ID > 0)
        //        carrinho.EnderecoFaturamento = usuario.EnderecoAlternativo;
        //    else
        //        carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;

        //    carrinho.Adicionar(produto, valor);

        //    var meioPagto = PedidoPagamento.MeiosPagamento.Deposito;

        //    if (meioPagamento == PedidoPagamento.MeiosPagamento.Boleto.GetHashCode())
        //    {
        //        meioPagto = PedidoPagamento.MeiosPagamento.Boleto;
        //    }

        //    carrinho.Adicionar(meioPagto, PedidoPagamento.FormasPagamento.Padrao);

        //    pedido = pedidoFactory.Criar(carrinho);
        //    pagamento = pedido.PedidoPagamento.FirstOrDefault();

        //    if (meioPagto == PedidoPagamento.MeiosPagamento.Deposito && pagamento != null)
        //    {
        //        try
        //        {
        //            //Verifica se os sitema só ira trabalhar com bitcoin, se sim a conta a ser associada é a de bitcoin na tabela contaDeposito
        //            if (Core.Helpers.ConfiguracaoHelper.GetString("BITCOIN_SISTEMA") == "true")
        //            {
        //                var contaDeposito = contaDepositoRepository.GetAtual(2); //tipodeconta 2 é bitcoin na tabela de contadeposito
        //                pagamento.ReferenciaID = contaDeposito.ID;
        //            }
        //            else
        //            {
        //                var contaDeposito = contaDepositoRepository.GetAtual(); //tipodeconta 1 (default) é deposito em conta na tabela de contadeposito
        //                pagamento.ReferenciaID = contaDeposito.ID;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string[] strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], "Cod 3428", traducaoHelper["CONTA_DEPOSITO_NAO_EXISTE"] };
        //            Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");

        //            return View("Index");
        //        }
        //        pedidoPagamentoRepository.Save(pagamento);
        //    }

        //    //Libera acesso ao Backoffice e gera registros básicos de associação
        //    /*
        //    var nivel = 0;
        //    usuario.NivelAssociacao = nivel;
        //    usuario.Status = Core.Entities.Usuario.TodosStatus.Associado;
        //    usuario.DataAtivacao = App.DateTimeZion;
        //    usuarioRepository.Save(usuario);

        //    var status = new Core.Entities.UsuarioStatus()
        //    {
        //        AdministradorID = null,
        //        Data = App.DateTimeZion,
        //        Status = Core.Entities.Usuario.TodosStatus.Associado,
        //        UsuarioID = usuario.ID
        //    };
        //    usuarioStatusRepository.Save(status);

        //    var existePosicao = posicaoRedeRepository.GetByExpression(p => p.UsuarioID == usuario.ID).Any();
        //    if (!existePosicao)
        //    {
        //        Posicao posicaoRede = new Posicao();
        //        posicaoRede.AcumuladoDireita = 0;
        //        posicaoRede.AcumuladoEsquerda = 0;
        //        posicaoRede.DataInicio = App.DateTimeZion.AddDays(-1);
        //        posicaoRede.DataFim = App.DateTimeZion.AddDays(-1);
        //        posicaoRede.DataCriacao = App.DateTimeZion;
        //        posicaoRede.Usuario = usuario;
        //        posicaoRede.UsuarioID = usuario.ID;
        //        posicaoRede.ValorPernaDireita = 0;
        //        posicaoRede.ValorPernaEsquerda = 0;
        //        posicaoRede.ReferenciaID = 0;
        //        posicaoRedeRepository.Save(posicaoRede);
        //    }

        //    var existeAssociacao = usuarioAssociacaoRepository.GetByExpression(u => u.UsuarioID == usuario.ID && u.NivelAssociacao == nivel).Count() > 0;
        //    if (!existeAssociacao)
        //    {
        //        var usuarioAssociacao = new Core.Entities.UsuarioAssociacao()
        //        {
        //            Data = App.DateTimeZion,
        //            NivelAssociacao = nivel,
        //            UsuarioID = usuario.ID
        //        };
        //        usuarioAssociacaoRepository.Save(usuarioAssociacao);
        //    }
        //    */


        //    #region Posiciona Rede Sem Pagamento
        //    if (ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
        //    {
        //        Boolean blnArvoreBinaria = false;
        //        if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
        //            blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

        //        if (blnArvoreBinaria)
        //        {
        //            usuarioService.Associar(usuario.ID, 0);
        //        }
        //        else
        //        {
        //            if (ConfiguracaoHelper.GetBoolean("REDE_PREENCHIMENTO_SEQUENCIAL"))
        //                usuarioService.AssociarRedeSequencia(usuario.ID, 0);
        //            else
        //                usuarioService.AssociarRedeHierarquia(usuario.ID, 0);
        //        }

        //        usuarioService.GeraUsuarioAssociacao(usuario, usuario.NivelAssociacao, false);
        //    }
        //    #endregion

        //    Autentica(usuario.Login);

        //    if (pagamento.MeioPagamento == PedidoPagamento.MeiosPagamento.Boleto)
        //    {
        //        return RedirectToAction("pagar", "boleto", new { pagamentoID = pagamento.ID });
        //    }
        //    else
        //    {
        //        return RedirectToAction("pagar", "deposito", new { pagamentoID = pagamento.ID });
        //    }
        //}

        public ActionResult Buscar(string termo, string sigla)
        {
            Session["sigla"] = sigla;
            Localizacao(sigla);
            IQueryable<Usuario> patrocinadores = null;

            #region ReCaptha

            bool blnContinua = true;

            //if (WebConfigurationManager.AppSettings["Ambiente"] != "dev" && Core.Helpers.ConfiguracaoHelper.GetBoolean("RECAPTCHA_CADASTRO_ATIVO"))
            //{
            //    //Obtem valor do reCaptha do google
            //    string recaptcha = Request.Form["g-recaptcha-response"];
            //    string postData = "secret=" + Core.Helpers.ConfiguracaoHelper.GetString("RECAPTCHA_SECRET_KEY") + "&response=" + recaptcha;

            //    blnContinua = false;

            //    try
            //    {
            //        using (var webClient = new System.Net.WebClient())
            //        {
            //            webClient.Encoding = Encoding.UTF8;
            //            var json2 = webClient.DownloadString("https://www.google.com/recaptcha/api/siteverify?" + postData);
            //            dynamic data = JObject.Parse(json2);
            //            if (data != null)
            //            {
            //                if (data["success"].ToString() == "True")
            //                {
            //                    blnContinua = true;
            //                }
            //                else
            //                {
            //                    blnContinua = false;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        blnContinua = false;
            //    }
            //}
            //if (!blnContinua)
            //{

            //    return View(patrocinadores);
            //}
            #endregion

            try
            {
                termo = termo.ToLower();
                var statusAssociado = Usuario.TodosStatus.Associado.GetHashCode();

                if (Core.Helpers.ConfiguracaoHelper.GetString("PATROCINADOR_BUSCA_EXATA") != "false") //Default caso não haja a chave no sistema
                {
                    //Busca pelo nome exato do patrocinador
                    patrocinadores = usuarioRepository.GetByExpression(u => u.Login.ToLower() == termo && u.Oculto == false && u.Bloqueado == false && u.StatusID == statusAssociado);
                }
                else
                {
                    // Busca por parte do nome do patrocinador Ex. car traz: car1, car123, 34carOP, etc
                    patrocinadores = usuarioRepository.GetByExpression(u => u.Login.ToLower().Contains(termo) && u.Oculto == false && u.Bloqueado == false && u.StatusID == statusAssociado);
                }

                return View(patrocinadores);
            }
            catch (Exception)
            {
                //Fazer tratamento de erro
            }

            return RedirectToAction("Index");

        }

        public ActionResult ReturnAfiliado()
        {
            return RedirectToAction("Index", "Home", new { login = "ok" });
        }

        //private void Autentica(string login)
        //{
        //    Usuario user = usuarioRepository.GetByLogin(login);

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

        //    if (user == null)
        //    {
        //        Local.LogAcesso(ViewBag.Login, ViewBag.Senha, strIPAddress);
        //        RedirectToAction("Login", "Account", new
        //        {
        //            strPopupTitle = "Login",
        //            strPopupMessage = "Login incorreto!"
        //        });
        //    }
        //    //Somente para verificar a senha do usuario
        //    string strSenha = Core.Helpers.CriptografiaHelper.Descriptografar(user.Senha);

        //    Usuario usuario = usuarioService.Autenticar(user.Login, strSenha);

        //    if (usuario != null)
        //    {
        //        var result = SignInManager.PasswordSignIn(user.Login, strSenha, true, shouldLockout: false);
        //    }
        //    else
        //    {
        //        #region FalhaAutenticacao

        //        Local.LogAcesso(user.Login, user.Senha, strIPAddress);
        //        string[] strMensagem = new string[] { "Login não é valido!" };
        //        Mensagem("LOGIN", strMensagem, "msg");
        //        RedirectToAction("Login", "Account");

        //        #endregion
        //    }
        //}

        //[HttpPost]
        //public ActionResult PagamentoCartao(FormCollection form)
        //{

        //    #region Dados via form

        //    string strNome = form["ccNome"];
        //    string strBandeira = form["ccBandeira"];
        //    string strNumero = form["ccNumero"];
        //    string strCodSeguranca = form["ccCodSeguranca"];
        //    string strMes = form["ccMes"];
        //    string strAno = form["ccAno"];
        //    string strLogin = form["ccLogin"];
        //    string strProduto = form["ccProduto"];
        //    string strRetCartao = "";

        //    Session["CartaoNome"] = strNome;
        //    Session["CartaoBandeira"] = strBandeira;
        //    Session["CartaoNumero"] = strNumero;
        //    Session["CartaoCodSeguranca"] = strCodSeguranca;
        //    Session["CartaoMes"] = strMes;
        //    Session["CartaoAno"] = strAno;
        //    Session["CartaoLogin"] = strLogin;
        //    Session["CartaoProduto"] = strProduto;

        //    #endregion

        //    #region Variaveis

        //    PedidoPagamento pagamento;
        //    Session["ShowCartao"] = "true";
        //    Usuario usuario = usuarioRepository.GetByLogin(strLogin);

        //    Localizacao(usuario.Pais);

        //    #endregion

        //    #region  Consistencias

        //    bool blnContinua = true;
        //    string strMensagem = "";

        //    if (string.IsNullOrEmpty(strLogin))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_LOGIN"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strProduto))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_PRODUTO"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strNome))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_NOME"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strBandeira))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_BANDEIRA"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strNumero))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_NUMERO"] + " ";
        //    }
        //    else
        //    {
        //        //Remove espaçõs em branco na digitação
        //        strNumero = strNumero.Replace(" ", "");

        //        //Cartão deve possuir 16 digitos
        //        if (strNumero.Length < 12)
        //        {
        //            blnContinua = false;
        //            strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_NUMERO"] + " ";
        //        }
        //    }
        //    if (string.IsNullOrEmpty(strCodSeguranca))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_CODSEGURANCA"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strMes))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_MES"] + " ";
        //    }

        //    if (string.IsNullOrEmpty(strAno))
        //    {
        //        blnContinua = false;
        //        strMensagem += traducaoHelper["CARTAO_CONSISTENCIA_ANO"] + " ";
        //    }

        //    if (!blnContinua)
        //    {
        //        #region Inconsistencia

        //        //Inconsitencia
        //        Session["ErroTitulo"] = traducaoHelper["CARTAO"];
        //        Session["Erro"] = strMensagem;
        //        //Vota para a tela 
        //        return RedirectToAction("associacao", "cadastro", new { login = strLogin });


        //        #endregion
        //    }

        //    #endregion

        //    #region Carrinho

        //    int produtoID = Convert.ToInt32(strProduto);
        //    Pedido pedido = usuario.Pedido.Where(p => p.PedidoItem.Any(i => i.Produto.TipoID == (int)Core.Entities.Produto.Tipos.Associacao)).OrderByDescending(p => p.DataCriacao).FirstOrDefault();

        //    var carrinho = new Core.Models.Loja.CarrinhoModel(usuario);
        //    var produto = produtoRepository.Get(produtoID);
        //    var valor = produto.ValorMinimo(usuario);

        //    carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
        //    carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
        //    carrinho.Adicionar(produto, valor);

        //    var meioPagto = PedidoPagamento.MeiosPagamento.Cartao;

        //    carrinho.Adicionar(meioPagto, PedidoPagamento.FormasPagamento.Padrao);

        //    pedido = pedidoFactory.Criar(carrinho);
        //    pagamento = pedido.PedidoPagamento.FirstOrDefault();

        //    #endregion

        //    try
        //    {
        //        #region Cielo

        //        //Chama serviço de pagamento por cartão de credito
        //        int total = Convert.ToInt32(pedido.Total * 100);
        //        //strRetCartao = Integracao.Cartao.Transacao(strBandeira, strNome, strNumero, strCodSeguranca, strMes, strAno, pedido.ID, total);

        //        //Retorno da cielo, 00 deu certo
        //        if (strRetCartao == "00")
        //        {
        //            #region Sucesso

        //            Session["SucessoTitulo"] = traducaoHelper["CARTAO"];
        //            Session["Sucesso"] = traducaoHelper["OPERACAO_SUCESSO"];
        //            //Armazenar em banco
        //            pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);

        //            //Salvar transação com cartao
        //            CartaoCredito cartaoCredito = new CartaoCredito()
        //            {
        //                UsuarioID = usuario.ID,
        //                PedidoPagamentoID = pagamento.ID,
        //                Bandeira = strBandeira,
        //                FinalCartao = strNumero.Substring(strNumero.Length - 4),
        //                Token = "",
        //                Valor = (total / 100),
        //                Descricao = strNome,
        //                DataCriacao = App.DateTimeZion,
        //                DataPagamento = App.DateTimeZion
        //            };
        //            cartaoCreditoRepository.Save(cartaoCredito);

        //            #region Liberar acesso

        //            //Libera acesso ao Backoffice e gera registros básicos de associação
        //            var nivel = produto.NivelAssociacao;
        //            usuario.NivelAssociacao = nivel;
        //            usuario.Status = Core.Entities.Usuario.TodosStatus.Associado;
        //            usuario.DataAtivacao = App.DateTimeZion;
        //            usuarioRepository.Save(usuario);

        //            var status = new Core.Entities.UsuarioStatus()
        //            {
        //                AdministradorID = null,
        //                Data = App.DateTimeZion,
        //                Status = Core.Entities.Usuario.TodosStatus.Associado,
        //                UsuarioID = usuario.ID
        //            };
        //            usuarioStatusRepository.Save(status);

        //            var existePosicao = posicaoRedeRepository.GetByExpression(p => p.UsuarioID == usuario.ID).Any();
        //            if (!existePosicao)
        //            {
        //                Posicao posicaoRede = new Posicao();
        //                posicaoRede.AcumuladoDireita = 0;
        //                posicaoRede.AcumuladoEsquerda = 0;
        //                posicaoRede.DataInicio = App.DateTimeZion.AddDays(-1);
        //                posicaoRede.DataFim = App.DateTimeZion.AddDays(-1);
        //                posicaoRede.DataCriacao = App.DateTimeZion;
        //                posicaoRede.Usuario = usuario;
        //                posicaoRede.UsuarioID = usuario.ID;
        //                posicaoRede.ValorPernaDireita = 0;
        //                posicaoRede.ValorPernaEsquerda = 0;
        //                posicaoRede.ReferenciaID = 0;
        //                posicaoRedeRepository.Save(posicaoRede);
        //            }

        //            var existeAssociacao = usuarioAssociacaoRepository.GetByExpression(u => u.UsuarioID == usuario.ID && u.NivelAssociacao == nivel).Count() > 0;
        //            if (!existeAssociacao)
        //            {
        //                var usuarioAssociacao = new Core.Entities.UsuarioAssociacao()
        //                {
        //                    Data = App.DateTimeZion,
        //                    NivelAssociacao = nivel,
        //                    UsuarioID = usuario.ID
        //                };
        //                usuarioAssociacaoRepository.Save(usuarioAssociacao);
        //            }

        //            #endregion

        //            //Ir para proximo passo
        //            carrinho.Limpar();
        //            return RedirectToAction("sucessoCartao", new { login = usuario.Login });

        //            #endregion
        //        }
        //        else
        //        {
        //            //Erro devolvido pela cielo para a requisição solicitada
        //            if (strRetCartao.Substring(0, 1) == "e")
        //            {
        //                int intRetCartao = 0;

        //                strRetCartao = strRetCartao.Substring(1);
        //                intRetCartao = Convert.ToInt32(strRetCartao);

        //                Session["ErroTitulo"] = traducaoHelper["CARTAO"];
        //                //Session["Erro"] = Cielo.Cielo.Erro(intRetCartao);

        //                return RedirectToAction("associacao", "cadastro", new { login = strLogin });

        //            }
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        #region erro
        //        Session["ErroTitulo"] = traducaoHelper["ERRO"];
        //        Session["Erro"] = ex.Message;
        //        return RedirectToAction("associacao", "cadastro", new { login = strLogin });
        //        #endregion
        //    }


        //    //Retorna a tela de pagamento com cartão
        //    return RedirectToAction("associacao", "cadastro", new { login = strLogin });

        //}

        public string CidadesToUF(string uf, string cidade)
        {
            string retorno = "";
            int estadoID = 0;
            string strCidade = Helpers.Local.removerAcentos(cidade).ToLower();

            var estados = estadoRepository.GetByExpression(x => x.Sigla == uf && x.PaisID == 2);
            foreach (Estado item in estados)
            {
                estadoID = item.ID;
            }

            var cidades = cidadeRepository.GetByEstado(estadoID).ToList();
            foreach (var i in cidades)
            {
                if (i.Nome.ToLower() == strCidade)
                    retorno += "<option value=" + i.ID + " selected >" + i.Nome + "</option>";
                else
                    retorno += "<option value=" + i.ID + " >" + i.Nome + "</option>";
            }

            return retorno;
        }

        #endregion

        #region JsonResult

        public JsonResult estadoID(string UF)
        {
            int retorno = 0;
            var estados = estadoRepository.GetByExpression(x => x.Sigla == UF && x.PaisID == 2);
            foreach (Estado item in estados)
            {
                retorno = item.ID;
            }
            return Json(retorno);

        }

        public JsonResult CidadeID(string uf, string cidade)
        {
            int retorno = 0;
            int estadoID = 0;
            cidade = Helpers.Local.removerAcentos(cidade);

            var estados = estadoRepository.GetByExpression(x => x.Sigla == uf && x.PaisID == 2);
            foreach (Estado item in estados)
            {
                estadoID = item.ID;
            }

            var cidades = cidadeRepository.GetByExpression(x => x.EstadoID == estadoID && x.Nome == cidade);
            foreach (Cidade item in cidades)
            {
                retorno = item.ID;
            }
            return Json(retorno);
        }

        public JsonResult Cidades(int estadoID)
        {
            var retorno = new List<lstCidade>();
            var cidades = cidadeRepository.GetByEstado(estadoID).ToList();
            cidades.ForEach(c => retorno.Add(new lstCidade(c)));
            return Json(retorno);
        }

        public JsonResult EstadoCache(string estado)
        {
            //Armazena estado escolhido para uso em cidadesAuto
            int EstadoID = estadoRepository.GetID(estado);
            Session["Estado"] = EstadoID;
            return Json("ok");
        }

        public JsonResult QuantidadeRepeticaoDocumento(string strDocumento)
        {
            int retorno = usuarioRepository.CountByDocumento(strDocumento);

            return Json(retorno);
        }

        #endregion

    }
}

