#region Bibliotecas

using Core.Entities;
using Core.Repositories.Globalizacao;
using Core.Services.Globalizacao;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Helpers;
using Sistema.Models.Envelope;
using Base32;
using OtpSharp;
using Core.Services.Sistema;
using System.Globalization;
using System.Web.UI;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class MeusDadosController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis

        public enum Tabs
        {
            InformacoesPessoais = 1,
            EnderecoContato = 2,
            DadosDeAcesso = 3,
            EnvioDeDocumentos = 4,
            DadosBancarios = 5
        }

        private string cstrFalha = "";

        private List<string> cstrValidacao = new List<string>();

        #endregion

        #region Core

        private ArquivoSecaoRepository arquivoSecaoRepository;
        private EstadoRepository estadoRepository;
        private CidadeRepository cidadeRepository;
        private EnderecoRepository enderecoRepository;
        private DocumentoRepository documentoRepository;
        private ContaDepositoRepository contaDepositoRepository;
        private PersistentRepository<Instituicao> instituicaoRepository;
        private ValidacaoCadastroRepository validacaoCadastroRepository;
        private PaisRepository paisRepository;
        private GeolocalizacaoService geolocalizacaoService;
        private FilialRepository filialRepository;

        public MeusDadosController(DbContext context)
            : base(context)
        {
            arquivoSecaoRepository = new ArquivoSecaoRepository(context);
            estadoRepository = new EstadoRepository(context);
            cidadeRepository = new CidadeRepository(context);
            enderecoRepository = new EnderecoRepository(context);
            documentoRepository = new DocumentoRepository(context);
            contaDepositoRepository = new ContaDepositoRepository(context);
            instituicaoRepository = new PersistentRepository<Instituicao>(context);
            filialRepository = new FilialRepository(context);
            validacaoCadastroRepository = new ValidacaoCadastroRepository(context);
            paisRepository = new PaisRepository(context);
            geolocalizacaoService = new GeolocalizacaoService(context);
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
        private bool ChecarTamanhoImagem(HttpPostedFileBase imgFile, int intX, int intY, float floTamanho, string strTexto)
        {
            bool blnContinua = true;
            int intXPadrao = intX;
            int intYPadrao = intY;
            float floTamanhoPadrao = floTamanho;


            if (!Local.ChecarImagem(imgFile, ref intX, ref intY, ref floTamanho, true))
            {
                blnContinua = false;
                cstrValidacao.Add("A imagem " + strTexto + " não esta no padrao!");
                cstrValidacao.Add("   Imagem: " + intX + "x" + intY + "px e " + floTamanho.ToString("0.00") + "KB de tamanho.");
                cstrValidacao.Add("   Padrao: " + intXPadrao + "x" + intYPadrao + "px com no máximo " + floTamanhoPadrao + "500KB de tamanho.");
            }

            return blnContinua;
        }

        private bool GravaImagemFoto(HttpPostedFileBase imgFile)
        {
            bool blnContinua = true;


            try
            {
                // Caminho Virtual
                string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
                string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
                string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL");

                //string strPath = ConfigurationManager.AppSettings["pathFoto"];
                string diretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL");

                //Caminho virtual
                string caminhoVirtual = strdominio + strCdn.Replace("//", "/") + strPath.Replace("\\", "/") + usuario.ID.ToString("D6") + ".jpg";
                // Caminho Fisico
                string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO") + "\\office\\cdn\\" + strPath + usuario.ID.ToString("D6") + ".jpg";

                if (imgFile != null)
                {
                    //Salva arquivo em file system
                    imgFile.SaveAs(@caminhoFisico);
                }
            }
            catch (Exception ex)
            {
                cstrFalha += "Não foi possivel salvar a imagem da Foto";
                blnContinua = false;
            }

            return blnContinua;
        }

        private void PopulaViewBags()
        {
            //ViewBag.TipoPessoa = Regex.Replace(usuario.Documento, @"[^\d]", "").Length == 11 ? "F" : "J";

            ViewBag.TipoPessoa = "F";
            ViewBag.CriptoAtual = "Bitcoin";

            ViewBag.Estados = estadoRepository.GetByPais(usuario.PaisID);
            if (usuario.EnderecoPrincipal != null)
            {
                ViewBag.Cidades = cidadeRepository.GetByEstado(usuario.EnderecoPrincipal.EstadoID);
            }
            else
            {
                //Caso não haja idEstado seta id do estado de sâo paulo
                ViewBag.Cidades = cidadeRepository.GetByEstado(4); //São paulo         
            }

            //Fixo idPais = 1 para pegar criptos
            ViewBag.Instituicoes = instituicaoRepository.GetByExpression(i => (i.IDPais == 1 || i.IDPais == usuario.PaisID) && i.IDAtivo == 1); 

            ContaDeposito conta = usuario.ContaDeposito.FirstOrDefault(c => c.IDTipoConta == 2);
            if (conta != null)
            {
                if (conta.Bitcoin != null)
                {
                    conta.Bitcoin = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Bitcoin);
                    conta.Bitcoin = cpUtilities.Gerais.Morpho(conta.Bitcoin, TipoCriptografia.Descriptografa);
                }
                if (conta.Litecoin != null)
                {
                    conta.Litecoin = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Litecoin);
                    conta.Litecoin = cpUtilities.Gerais.Morpho(conta.Litecoin, TipoCriptografia.Descriptografa);
                }
                if (conta.Tether != null)
                {
                    conta.Tether = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Tether);
                    conta.Tether = cpUtilities.Gerais.Morpho(conta.Tether, TipoCriptografia.Descriptografa);
                }
                ViewBag.Bitcoin = conta.Bitcoin;
                ViewBag.Litecoin = conta.Litecoin;
                ViewBag.Tether = conta.Tether;
            }

            ViewBag.Conta = conta;

            ViewBag.Filiais = filialRepository.GetAll();

            string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
            string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
            string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL");
            string strVersao = "?id=" + Gerais.GerarChave();
            string caminhoVirtual = strdominio + strCdn.Replace("//", "/") + strPath.Replace("\\", "/") + usuario.ID.ToString("D6") + ".jpg";

            string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO") + "cdn\\" + strPath + usuario.ID.ToString("D6") + ".jpg";

            ViewBag.Foto = caminhoVirtual;

            #region Page Tour                      

            var textos = new Dictionary<string, string>();
            textos.Add("NomeFantasia", "Teste1");
            textos.Add("Apelido", "Teste2");

            ViewBag.JavaScriptPtMarcador = Local.GerarJsPageTourMarcador("Teste do PJ", textos);

            //ViewBag.Token2FA = Session["Token2FA"] ?? string.Empty;

            #endregion
        }

        public bool Valida2FA(string token)
        {
            if (!Core.Helpers.ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES")) { 
                return true; 
            }

            var user = UserManager.FindById(usuario.IdAutenticacao);
            byte[] secretKey = Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey);

            var otp = new Totp(secretKey);
            if (otp.VerifyTotp(token, out _, new VerificationWindow(10, 10)))
                return true;
            else
                return false;
            //response.Mensagem = traducaoHelper["TOKEN_INVALIDO"];
        }

        private void Localizacao(string sigla = null)
        {
            Pais pais = null;

            if (usuario.Pais != null)
                pais = paisRepository.GetBySigla(usuario.Pais.Sigla);

            if (pais == null && Request.UserLanguages.Any())
            {
                pais = paisRepository.GetBySigla(Request.UserLanguages[0]);
            }

            if (pais == null)
            {
                pais = geolocalizacaoService.GetByIP();
            }

            if (pais == null)
            {
                pais = paisRepository.GetPadrao();
            }

            ViewBag.Paises = paisRepository.GetDisponiveis();
            ViewBag.Pais = pais;
        }
        #endregion

        #region Actions

        public ActionResult Index()
        {
            Localizacao();

            var _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var twoFactorEnabled = _userManager.GetTwoFactorEnabled(usuario.IdAutenticacao);

            bool validado = usuario.Status == Core.Entities.Usuario.TodosStatus.Associado || usuario.Status == Core.Entities.Usuario.TodosStatus.NaoAssociado;

            if (validado)
            {
                return LoadView(Tabs.InformacoesPessoais);
            }
            else
            {
                ViewBag.TwoFactorEnabled = twoFactorEnabled;
                ViewBag.Validado = validado;
                return RedirectToAction("detail");
            }
        }

        public ActionResult Detail()
        {
            ViewBag.TipoPessoa = Regex.Replace(usuario.Documento, @"[^\d]", "").Length == 11 ? "F" : "J";

            ContaDeposito conta = usuario.ContaDeposito.FirstOrDefault(c => c.IDTipoConta == 2);
            ViewBag.Conta = conta;

            string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
            string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
            string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL") + "/";
            string strVersao = "?id=" + Gerais.GerarChave();
            ViewBag.Foto = strdominio + strCdn + strPath + usuario.ID.ToString("D6") + ".jpg" + strVersao;


            var validado = validacaoCadastroRepository.ObtemStatus(usuario.ID);
            ViewBag.Validado = validado;

            return View(usuario);
        }

        public ActionResult InformacoesPessoais(string tipoPessoa)
        {
            Localizacao();
            ViewBag.TipoPessoa = tipoPessoa;

            return LoadView(Tabs.InformacoesPessoais);
        }

        public ActionResult EnderecoEContato()
        {
            Localizacao();
            return LoadView(Tabs.EnderecoContato);
        }

        public ActionResult DadosDeAcesso()
        {
            Localizacao();
            return LoadView(Tabs.DadosDeAcesso);
        }

        public ActionResult EnvioDeDocumentos()
        {
            Localizacao();
            return LoadView(Tabs.EnvioDeDocumentos);
        }

        public ActionResult DadosBancarios()
        {
            Localizacao();
            return LoadView(Tabs.DadosBancarios);
        }

        [HttpPost]
        public ActionResult ValidacaoDados(FormCollection form)
        {
            var ok = form["ok"];

            var valido = ok == "true";

            validacaoCadastroRepository.SalvarStatus(usuario.ID, valido);

            return RedirectToAction("detail");
        }

        [HttpPost]
        public async Task<ActionResult> Salvar(FormCollection form)
        {
            string[] strMensagem = new string[] { };
            var tab = (Tabs)Enum.Parse(typeof(Tabs), form["Tab"]);

            #region Valida Token Google Authenticator

            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                string token2FA = form["form" + tab + "Token"] != null ? form["form" + tab + "Token"] : form["fileuploadToken"];

                if (!Valida2FA(token2FA))
                {
                    strMensagem = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
                    Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                    ViewBag.Tab = tab;

                    PopulaViewBags();
                    obtemMensagem();

                    return View("Index", usuario);
                }
            }
            #endregion

            try
            {
                switch (tab)
                {
                    case Tabs.InformacoesPessoais:
                        #region InformacoesPessoais

                        #region Validacoes

                        //string strDocPF = form["DocPF"];
                        //string strDocPJ = form["DocPJ"];
                        string strTpPessoa = form["TipoPessoa"];
                        string strPais = form["pais"];

                        //bool retorno = true;

                        //Pais pais = (Pais)Session["pais"];
                        //string idioma = usuario.Pais.Idioma.Sigla;

                        string strDocumento = string.Empty;
                        //if (strTpPessoa == "F")
                        //    strDocumento = strDocPF;
                        //else
                        //    strDocumento = strDocPJ;

                        //if (idioma == "pt-BR")
                        //{
                        //    //se brasil valida CPF/ CNPJ
                        //    if (strTpPessoa == "F") 
                        //        retorno = cpUtilities.Validacoes.ValidaCPF(strDocumento);
                        //    else
                        //        retorno = cpUtilities.Validacoes.ValidaCNPJ(strDocumento);

                        //    //se brasil valida cpf          
                        //    if (!retorno)
                        //    {
                        //        strMensagem = new string[] { traducaoHelper["DOCUMENTO_IDENTIFICACAO"] + ": " + "inválido" };
                        //        Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                        //        return RedirectToAction("informacoes-pessoais", new { tipoPessoa = strTpPessoa });
                        //    }
                        //}

                        #endregion

                        string strDataNascimento = form["DataNascimento"];

                        strDataNascimento = cpUtilities.Gerais.ConverterDataBanco(strDataNascimento, usuario.Pais.Idioma.Sigla);
                        DateTime dataAniversario = DateTime.Parse(strDataNascimento);

                        //usuario.NomeFantasia = form["NomeFantasia"];
                        //usuario.Apelido = form["Apelido"];
                        usuario.Nome = form["Nome"];
                        usuario.Sexo = form["Sexo"];
                        usuario.Documento = strDocumento;
                        //usuario.FilialID = int.Parse(form["FilialID"]);
                        usuario.DataNascimento = dataAniversario;
                        usuario.PaisID = Convert.ToInt32(strPais);

                        //Truncar em 15 caracteres
                        //string truncate = usuario.Apelido;
                        //usuario.Apelido = truncate.Length <= 12 ? truncate : truncate.Substring(0, 12);

                        repository.Save(usuario);

                        await SendEmailEdicaoCadastro("INFORMACOES_PESSOAIS");

                        return RedirectToAction("informacoes-pessoais", new { tipoPessoa = strTpPessoa });
                    #endregion
                    case Tabs.EnderecoContato:
                        #region EnderecoContato

                        Endereco enderecoPrincipal;
                        if (usuario.EnderecoPrincipal.ID != 0)
                        {
                            enderecoPrincipal = enderecoRepository.Get(usuario.EnderecoPrincipal.ID);
                        }
                        else
                        {
                            enderecoPrincipal = new Endereco();
                            enderecoPrincipal.ID = 0;
                            enderecoPrincipal.Principal = true;
                            enderecoPrincipal.UsuarioID = usuario.ID;
                            enderecoPrincipal.Observacoes = "";
                            enderecoPrincipal.Nome = "Home";
                        }
                        string estado = form["EstadoID"];
                        if(String.IsNullOrEmpty(estado))
                        {
                            estado = "1";
                        }
                        string cidade = form["CidadeID"];
                        if (String.IsNullOrEmpty(cidade))
                        {
                            cidade = "1";
                        }
                        enderecoPrincipal.EstadoID = int.Parse(estado);
                        enderecoPrincipal.CidadeID = int.Parse(cidade);
                        enderecoPrincipal.CodigoPostal = form["CodigoPostal"];
                        enderecoPrincipal.Logradouro = form["Logradouro"];
                        enderecoPrincipal.Numero = form["Numero"];
                        enderecoPrincipal.Distrito = form["Distrito"];
                        enderecoPrincipal.Complemento = form["Complemento"];
                        enderecoPrincipal.CidadeNome = "";

                        enderecoRepository.Save(enderecoPrincipal);

                        if (ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO_ALTERNATIVO") == "true")
                        {
                            Endereco enderecoAlternativo;
                            if (usuario.EnderecoAlternativo.ID != 0)
                            {
                                enderecoAlternativo = enderecoRepository.Get(usuario.EnderecoAlternativo.ID);
                            }
                            else
                            {
                                enderecoAlternativo = new Endereco();
                                enderecoAlternativo.ID = 0;
                                enderecoAlternativo.Principal = false;
                                enderecoAlternativo.UsuarioID = usuario.ID;
                                enderecoAlternativo.Observacoes = "";
                                enderecoAlternativo.Nome = "Alternativo";
                            }

                            enderecoAlternativo.EstadoID = int.Parse(form["EstadoID2"]);
                            enderecoAlternativo.CidadeID = int.Parse(form["CidadeID2"]);
                            enderecoAlternativo.CodigoPostal = form["CodigoPostal2"];
                            enderecoAlternativo.Logradouro = form["Logradouro2"];
                            enderecoAlternativo.Numero = form["Numero2"];
                            enderecoAlternativo.Distrito = form["Distrito2"];
                            enderecoAlternativo.Complemento = form["Complemento2"];
                            enderecoAlternativo.Observacoes = form["Observacoes2"];
                            enderecoAlternativo.CidadeNome = "";

                            enderecoRepository.Save(enderecoAlternativo);
                        }


                        usuario.Telefone = ""; //Regex.Replace(form["Telefone"], @"[^\d]", ""); // == so numeros
                        usuario.Celular = form["Celular"]; //Regex.Replace(form["Celular"], @"[^\d]", "");  // == so numeros
                        repository.Save(usuario);

                        await SendEmailEdicaoCadastro("ENDERECO_CONTATO");

                        strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");

                        return RedirectToAction("endereco-e-contato");
                    #endregion
                    case Tabs.DadosDeAcesso:
                        #region DadosDeAcesso

                        bool blnValid = true;
                        string[] strMensagem2;

                        if (form["NovaSenha"] != form["ConfirmacaoNovaSenha"])
                        {
                            strMensagem2 = new string[] { traducaoHelper["DIGITE_MESMA_SENHA"] };
                            Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                            blnValid = false;
                        }


                        if (!String.IsNullOrEmpty(form["NovaSenha"]) && blnValid)
                        {
                            string strSenha = Core.Helpers.CriptografiaHelper.Descriptografar(usuario.Senha);
                            string strNovaSenha = form["NovaSenha"];

                            if (strNovaSenha.Length < 6)
                            {
                                strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["MINIMO_6_CARACTERES"] };
                                Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                blnValid = false;
                            }
                            else
                            {
                                if (!(strNovaSenha.Any(x => char.IsDigit(x))))
                                {
                                    strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"] };
                                    Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                    blnValid = false;
                                }
                                else
                                {
                                    if (!(strNovaSenha.Any(x => char.IsLetter(x))))
                                    {
                                        strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"] };
                                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                        blnValid = false;
                                    }
                                    else
                                    {
                                        if (!(strNovaSenha.Any(x => char.IsLower(x))))
                                        {
                                            strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"] };
                                            Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                            blnValid = false;
                                        }
                                        else
                                        {
                                            if (!(strNovaSenha.Any(x => char.IsUpper(x))))
                                            {
                                                strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"] };
                                                Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                                blnValid = false;
                                            }
                                            else
                                            {
                                                char[] SpecialChars = @"!@#$%&*()-+=|\/:.,_;".ToCharArray();
                                                int indexOf = strNovaSenha.IndexOfAny(SpecialChars);
                                                if (indexOf == -1)
                                                {
                                                    strMensagem2 = new string[] { traducaoHelper["SENHA"] + ": " + traducaoHelper["SENHA_DEVE_CONTER_LETRA_NUMERO"] };
                                                    Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                                    blnValid = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnValid)
                            {
                                usuario.Senha = Core.Helpers.CriptografiaHelper.Criptografar(strNovaSenha);
                                repository.Save(usuario);

                                var userSave = await UserManager.FindByIdAsync(usuario.IdAutenticacao);
                                if (userSave == null)
                                {
                                    return HttpNotFound();
                                }

                                var result = await UserManager.ChangePasswordAsync(usuario.IdAutenticacao, strSenha, strNovaSenha);

                                if (result.Succeeded)
                                {
                                    await SendEmailEdicaoCadastro("DADOS_ACESSO");

                                    blnValid = true;
                                }
                                else
                                {
                                    string strErro = "";
                                    //Msg não foi possivel alterar senha
                                    foreach (var error in result.Errors)
                                    {
                                        strErro += error + ";";
                                    }
                                    strMensagem2 = new string[] { traducaoHelper["MENSAGEM"], "Err:" };
                                    Mensagem(traducaoHelper["MENSAGEM"], strMensagem2, "msg");
                                    blnValid = false;
                                }
                            }
                        }

                        if (blnValid)
                        {
                            strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                            Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");
                        }

                        return RedirectToAction("dados-de-acesso");
                    #endregion
                    case Tabs.EnvioDeDocumentos:
                        #region EvioDeDocumentos

                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];

                            if (Request.Files[0].ContentLength > 0)
                            {
                                var documento = new Documento()
                                {
                                    DataEnvio = App.DateTimeZion,
                                    TipoID = int.Parse(form["TipoID"]),
                                    UsuarioID = usuario.ID,
                                    Validado = false
                                };
                                var info = new FileInfo(Request.Files[0].FileName);
                                string strExtension = info.Extension;

                                if (strExtension.ToLower() == ".jpg" || strExtension.ToLower() == ".png" || strExtension.ToLower() == ".doc" || strExtension.ToLower() == ".pdf")
                                {

                                    documentoRepository.Save(documento);
                                    var caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                                    if (caminhoFisico == "~")
                                    {
                                        caminhoFisico = Server.MapPath("~");
                                    }
                                    var diretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_DOCUMENTOS_USUARIOS");
                                    var caminho = caminhoFisico + diretorio + "/" + documento.ID + info.Extension.ToLower();
                                    Request.Files[0].SaveAs(caminho);

                                    await SendEmailEdicaoCadastro("ENVIO_DE_DOCUMENTOS");

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

                        ViewBag.Tab = "envio-de-documentos";
                        PopulaViewBags();

                        return RedirectToAction("envio-de-documentos");
                    #endregion
                    case Tabs.DadosBancarios:
                        #region DadosBancarios

                        ContaDeposito conta = usuario.ContaDeposito.FirstOrDefault(c => c.IDTipoConta == 2);
                        if (conta == null)
                        {
                            conta = new ContaDeposito()
                            {
                                DataCriacao = App.DateTimeZion,
                                IDTipoConta = 2,
                                IDUsuario = usuario.ID
                            };
                        }

                        conta.IDInstituicao = int.Parse(form["InstituicaoID"]);
                        conta.IDMeioPagamento = (int)PedidoPagamento.MeiosPagamento.Indefinido;
                        
                        //panda Está fixo isso aqui!!!!
                        conta.MoedaIDCripto = (int)Moeda.Moedas.BTC;

                        if (Core.Helpers.ConfiguracaoHelper.GetString("MEUS_DADOS_CONTA_CRIPTOMOEDA") == "true")
                        {
                            switch (conta.IDInstituicao)
                            {
                                case 107: //PIX -- Usando o antigo livecoin
                                    //if (!BlockchainService.ValidarCarteiraLitecoin(form["Bitcoin"]))
                                    //{
                                    //    strMensagem = new string[] { traducaoHelper["CARTEIRA_LTC_INVALIDA"] };
                                    //    Mensagem(traducaoHelper["ERRO"], strMensagem, "err");

                                    //    return RedirectToAction("dados-bancarios");
                                    //}
                                    conta.IDMeioPagamento = (int)PedidoPagamento.MeiosPagamento.CryptoPayments;
                                    conta.Litecoin = Core.Helpers.CriptografiaHelper.Morpho(form["Bitcoin"], Core.Helpers.CriptografiaHelper.TipoCriptografia.Criptografa);
                                    conta.MoedaIDCripto = (int)Moeda.Moedas.PIX;
                                    ViewBag.CriptoAtual = "Litecoin";

                                    break;
                                case 108: //Bitcoin
                                    if (!BlockchainService.ValidarCarteiraBitcoin(form["Bitcoin"]))
                                    {
                                        strMensagem = new string[] { traducaoHelper["CARTEIRA_BTC_INVALIDA"] };
                                        Mensagem(traducaoHelper["ERRO"], strMensagem, "err");

                                        return RedirectToAction("dados-bancarios");
                                    }
                                    conta.IDMeioPagamento = (int)PedidoPagamento.MeiosPagamento.CryptoPayments;
                                    conta.Bitcoin = Core.Helpers.CriptografiaHelper.Morpho(form["Bitcoin"], Core.Helpers.CriptografiaHelper.TipoCriptografia.Criptografa);
                                    conta.MoedaIDCripto = (int)Moeda.Moedas.BTC;
                                    ViewBag.CriptoAtual = "Bitcoin";

                                    break;
                                

                                default: //Tether
                                    if (!BlockchainService.ValidarCarteiraTether(form["Bitcoin"]))
                                    {
                                        strMensagem = new string[] { traducaoHelper["CARTEIRA_TETHER_INVALIDA"] };
                                        Mensagem(traducaoHelper["ERRO"], strMensagem, "err");

                                        return RedirectToAction("dados-bancarios");
                                    }
                                    conta.IDMeioPagamento = (int)PedidoPagamento.MeiosPagamento.CryptoPayments;
                                    conta.Tether = Core.Helpers.CriptografiaHelper.Morpho(form["Bitcoin"], Core.Helpers.CriptografiaHelper.TipoCriptografia.Criptografa);
                                    conta.MoedaIDCripto = (int)Moeda.Moedas.USDT;
                                    ViewBag.CriptoAtual = "Tether";
                                    break;
                            }

                            conta.ProprietarioConta = "";
                            conta.IdentificacaoProprietario = "";
                            conta.Agencia = "";
                            conta.Conta = "";
                            conta.DigitoConta = "";
                            conta.CPF = "";
                            conta.CNPJ = "";
                        }
                        else
                        {
                            conta.ProprietarioConta = form["Proprietario"];
                            conta.IdentificacaoProprietario = form["IdentificacaoProprietario"];
                            conta.Agencia = form["Agencia"];
                            conta.Conta = String.Format("{0}|{1}", form["TipoConta"], form["Conta"]);
                            conta.DigitoConta = form["DigitoConta"];
                            conta.CPF = form["CPF"].ToString().Replace(".", "").Replace("-", "");
                            conta.CNPJ = form["CNPJ"].ToString().Replace(".", "").Replace("-", "").Replace("/", "");
                            conta.Bitcoin = "";
                        }

                        try
                        {
                            contaDepositoRepository.Save(conta);
                        }
                        catch (Exception ex)
                        { 
                            String erro = ex.Message;
                        }
                        
                        await SendEmailEdicaoCadastro("DADOS_BANCARIOS");
                        
                        strMensagem = new string[] { traducaoHelper["DADOS_SALVOS_SUCESSO"] };
                        Mensagem(traducaoHelper["MENSAGEM"], strMensagem, "msg");

                        return RedirectToAction("dados-bancarios");
                        #endregion
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                string strRetErro = "";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        strRetErro += validationError.PropertyName + ": " + validationError.ErrorMessage + " ";
                    }
                }
                string[] strRetorno = new string[] { strRetErro };
                Mensagem(" " + traducaoHelper["ERRO"] + " ", strRetorno, "err");
            }
            catch (Exception ex)
            {
                strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"], ex.Message };
                Mensagem(traducaoHelper["ERRO"], strMensagem, "err");
            }

            return RedirectToAction("Index");
        }

        private async Task SendEmailEdicaoCadastro(string key)
        {
            try
            {
                var emailService = new Core.Services.Sistema.EmailService();

                var body = string.Format(traducaoHelper["MENSAGEM_CADASTRO_EDITADO"], usuario.Nome, traducaoHelper[key], App.DateTimeZion.Date.ToShortDateString(), App.DateTimeZion.ToString("HH:mm"));

                string tipoEnvio = ConfiguracaoHelper.GetString("EMAIL_TIPO_ENVIO");

                if (tipoEnvio.ToUpper() == "SMTP")
                {
                    emailService.Send(ConfiguracaoHelper.GetString("EMAIL_DE"), usuario.Email, traducaoHelper["MENSAGEM_CADASTRO_EDITADO_ASSUNTO"], body);
                }
                else if (tipoEnvio.ToUpper() == "SENDGRID")
                {
                    await SendGridService.SendGridEnviaSync(ConfiguracaoHelper.GetString("SENDGRID_API_KEY"), traducaoHelper["MENSAGEM_CADASTRO_EDITADO_ASSUNTO"], body, ConfiguracaoHelper.GetString("EMAIL_DE"), usuario.Email);
                }
            }
            catch (Exception ex)
            {
                // TODO 
                cpUtilities.LoggerHelper.WriteFile("SendEmailEdicaoCadastro: " + ex.Message, "MeusDadosController");
            }
        }

        private string EncodeFile(object fullPath)
        {
            throw new NotImplementedException();
        }

        private ActionResult LoadView(Tabs tab)
        {
            ViewBag.Tab = tab;

            PopulaViewBags();
            obtemMensagem();

            return View("Index", usuario);
        }

        #region Crop

        public ActionResult CropImage()
        {
            string imagePath = Request["imagePath"];

            string strFile = imagePath.Substring(imagePath.LastIndexOf("/") + 1);
            string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
            string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PERFIL"); // arquivoSecaoRepository.GetById(8).Caminho;

            imagePath = caminhoFisico + strPath + strFile;

            //System.Drawing.Image imgInfo = System.Drawing.Image.FromFile(Server.MapPath(imagePath));
            System.Drawing.Image imgInfo = System.Drawing.Image.FromFile(imagePath);
            int imgWidth = imgInfo.Width;
            int imgHeight = imgInfo.Height;
            imgInfo.Dispose();

            int intLado = 0;

            string strX1 = Request["x1"];
            string strY1 = Request["y1"];
            string strX2 = Request["x2"];
            string strY2 = Request["y2"];
            string strW = Request["coordsw"];
            string strH = Request["coordsh"];

            //Fixo no js, ex. 1580
            string strLarguraModelo = Request["largura"];
            //Fixo no js, ex. 800
            string strAlturaModelo = Request["altura"];

            //Fixo no js, ex. 385 (1580/4) é uma escala reduzida da imgem final
            string strLarguraPreview = Request["larguraPreview"];
            //Fixo no js, ex. 200 (800/4) é uma escala reduzida da imgem final
            string strAlturaPreview = Request["alturaPreview"];
            //Largura da imagem carregada na tela
            string strLarguraImagem = Request["larguraImagem"];
            //Altura da imagem carregada na tela (é fixo no style da img. ex 200)
            string strAlturaImagem = Request["alturaImagem"];

            //Acerta pontuação caso houver
            strX1 = strX1.Replace(".", ",");
            strY1 = strY1.Replace(".", ",");
            strX2 = strX2.Replace(".", ",");
            strY2 = strY2.Replace(".", ",");
            strLarguraModelo = strLarguraModelo.Replace(".", ",");
            strAlturaModelo = strAlturaModelo.Replace(".", ",");
            strLarguraPreview = strLarguraPreview.Replace(".", ",");
            strAlturaPreview = strAlturaPreview.Replace(".", ",");
            strAlturaImagem = strAlturaImagem.Replace(".", ",");

            double dblX1 = Convert.ToDouble(strX1);
            double dblY1 = Convert.ToDouble(strY1);
            double dblX2 = Convert.ToDouble(strX2);
            double dblY2 = Convert.ToDouble(strY2);

            double dblLarguraModelo = Convert.ToDouble(strLarguraModelo);
            double dblAlturaModelo = Convert.ToDouble(strAlturaModelo);

            double dblLarguraPreview = Convert.ToDouble(strLarguraPreview);
            double dblAlturaPreview = Convert.ToDouble(strAlturaPreview);

            double dblLarguraImagem = Convert.ToDouble(strLarguraImagem);
            double dblAlturaImagem = Convert.ToDouble(strAlturaImagem);

            double dblLado = 0;

            //Lado menor
            if (imgWidth > imgHeight)
            {
                intLado = imgHeight;
                dblLado = dblAlturaImagem;
            }
            else
            {
                intLado = imgWidth;
                dblLado = dblLarguraImagem;
            }

            double dblEscala = intLado / dblLado;

            dblX1 = dblX1 * dblEscala;
            dblY1 = dblY1 * dblEscala;
            dblX2 = dblX2 * dblEscala;
            dblY2 = dblY2 * dblEscala;

            int? X1 = Convert.ToInt32(Math.Round(dblX1));
            int? Y1 = Convert.ToInt32(Math.Round(dblY1));
            int? X2 = Convert.ToInt32(Math.Round(dblX2)) - X1;
            int? Y2 = Convert.ToInt32(Math.Round(dblY2)) - Y1;

            if (string.IsNullOrEmpty(imagePath)
                || !X1.HasValue
                || !Y1.HasValue
                || !X2.HasValue
                || !Y2.HasValue)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            //byte[] imageBytes = System.IO.File.ReadAllBytes(Server.MapPath(imagePath));
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            byte[] croppedImage = Imagens.CropImage(imageBytes, X1.Value, Y1.Value, X2.Value, Y2.Value);

            //string tempFolderName = ConfigurationManager.AppSettings["pathFoto"];
            //tempFolderName = Server.MapPath(tempFolderName);

            //string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            //string fileName = Path.GetFileName(imagePath).Replace(fileNameWithoutExtension, fileNameWithoutExtension);

            try
            {
                //Files.SaveFile(croppedImage, Path.Combine(tempFolderName, fileName));
                Files.SaveFile(croppedImage, imagePath);
            }
            catch (Exception ex)
            {
                //Log an error     
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            PopulaViewBags();
            obtemMensagem();
            return LoadView(Tabs.InformacoesPessoais);
        }

        public ActionResult Gravar(FormCollection form, HttpPostedFileBase imgFoto)
        {
            #region Valida Token Google Authenticator
            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                string[] strMensagem;
                string token2FA = string.Empty;

                if (form.Count > 0)
                    token2FA = form[0];

                if (!Valida2FA(token2FA))
                {
                    strMensagem = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
                    Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "msg");
                    ViewBag.Tab = Tabs.InformacoesPessoais;

                    PopulaViewBags();
                    obtemMensagem();

                    return View("Index", usuario);
                }
            }
            #endregion

            bool blnContinua = true;
            #region checa dimensoes das imagens
            try
            {
                // blnContinua = ChecarTamanhoImagem(imgFoto, 1580, 800, 2.0f, "");
                if (blnContinua)
                {
                    blnContinua = GravaImagemFoto(imgFoto);
                }
            }
            catch (Exception ex)
            {
                blnContinua = false;
                cstrFalha += " Não foi possivel salvar os dados. Erro: " + ex.Message;
                cstrValidacao.Add(cstrFalha);
                cstrFalha = "";
            }

            #endregion

            #region Finalizando

            if (!blnContinua)
            {
                cstrFalha += " Não foi possivel salvar os dados.";
                cstrValidacao.Add(cstrFalha);
                cstrFalha = "";
                Mensagem("Inconsitência", cstrValidacao.ToArray(), "ale");
            }

            //PopulaViewBags();
            //obtemMensagem();

            #endregion

            return LoadView(Tabs.InformacoesPessoais);
        }

        #endregion

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
    }

}
