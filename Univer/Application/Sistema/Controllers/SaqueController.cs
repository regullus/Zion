#region Bibliotecas

using Base32;
using Core.Entities;
using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using Core.Repositories.Usuario;
using Core.Services.MeioPagamento;
using cpUtilities;
using Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using OtpSharp;
using Sistema.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Sistema.Controllers
{
    public enum SolicitacaoSaqueStatus
    {
        Pode,
        DiaNaoPermitido,
        NaoPermitido,
        JaSacou
    }

    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class SaqueController : SecurityController<Core.Entities.Saque>
    {
        #region Variaveis
        YLEVELEntities db = new YLEVELEntities();
        #endregion

        #region Core

        private ContaRepository contaRepository;
        private LancamentoRepository lancamentoRepository;
        private SaqueRepository saqueRepository;
        private SaqueStatusRepository saqueStatusRepository;
        private MoedaRepository moedaRepository;
        private BancoRepository bancoRepository;
        private BlockchainService blockchainService;
        private ContaDepositoRepository contaDepositoRepository;
        private UsuarioRepository usuarioRepository;
        private UsuarioGanhoRepository usuarioGanhoRepository;
        private MoedaCotacaoRepository moedaCotacaoRepository;
        private ProdutoRepository produtoRepository;

        private Moeda moedaPadrao = new Moeda();
        public SaqueController(DbContext context)
            : base(context)
        {
            contaRepository = new ContaRepository(context);
            lancamentoRepository = new LancamentoRepository(context);
            saqueRepository = new SaqueRepository(context);
            saqueStatusRepository = new SaqueStatusRepository(context);
            moedaRepository = new MoedaRepository(context);
            bancoRepository = new BancoRepository(context);
            blockchainService = new BlockchainService(context);
            contaDepositoRepository = new ContaDepositoRepository(context);
            usuarioRepository = new UsuarioRepository(context);
            usuarioGanhoRepository = new UsuarioGanhoRepository(context);
            moedaCotacaoRepository = new MoedaCotacaoRepository(context);
            produtoRepository = new ProdutoRepository(context);

            moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
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

        public SolicitacaoSaqueStatus ValidaDataSolicitaSaque()
        {
            var status = SolicitacaoSaqueStatus.DiaNaoPermitido;

            var data = App.DateTimeZion.Date;
            //Somente dias da semana
            if (data.DayOfWeek == DayOfWeek.Monday ||
                data.DayOfWeek == DayOfWeek.Tuesday ||
                data.DayOfWeek == DayOfWeek.Wednesday ||
                data.DayOfWeek == DayOfWeek.Thursday ||
                data.DayOfWeek == DayOfWeek.Friday)
            {
                status = SolicitacaoSaqueStatus.Pode;
            }
            return status;
        }

        public void SolicitaSaque()
        {
            ViewBag.SolicitarSaqueRentabilidade = ValidaDataSolicitaSaque();
            ViewBag.SolicitarSaqueInvest = SolicitacaoSaqueStatus.NaoPermitido;
            if (usuario.DataRenovacao < DateTime.Now)
            {
                ViewBag.SolicitarSaqueInvest = ValidaDataSolicitaSaque();
            }
            
            ViewBag.SolicitarSaqueBonus = ValidaDataSolicitaSaque();

            //Verifica se usuario pode efetuar saque -- Cadastro de usuario no admim abilita ou não o saque
            //É abilitado por default na primeira compra
            if (!usuario.ExibeSaque.HasValue || usuario.ExibeSaque == 0)
            {
                ViewBag.SolicitarSaqueRentabilidade = SolicitacaoSaqueStatus.NaoPermitido;
                ViewBag.SolicitarSaqueInvest = SolicitacaoSaqueStatus.NaoPermitido;
                ViewBag.SolicitarSaqueBonus = SolicitacaoSaqueStatus.NaoPermitido;
            }
            if (ViewBag.SolicitarSaqueRentabilidade == SolicitacaoSaqueStatus.Pode)
            {
                //Verifica se usuario já fez soliciatação de saque hoje
                var saquesRentabilidade = saqueRepository.GetForToday(usuario.ID, 1);
                if (saquesRentabilidade.Count > 0)
                {
                    ViewBag.SolicitarSaqueRentabilidade = SolicitacaoSaqueStatus.JaSacou;
                }
                var saquesBonus = saqueRepository.GetForToday(usuario.ID, 2);
                if (saquesBonus.Count > 0)
                {
                    ViewBag.SolicitarSaqueBonus = SolicitacaoSaqueStatus.JaSacou;
                }
                var saquesInvest = saqueRepository.GetForToday(usuario.ID, 8);
                if (saquesInvest.Count > 0)
                {
                    ViewBag.SolicitarSaqueInvest = SolicitacaoSaqueStatus.JaSacou;
                }
            }

            ViewBag.UsuarioLideranca = false;

            if (usuario.Complemento != null)
            {
                ViewBag.UsuarioLideranca = usuario.Complemento.IsLideranca;
            }
        }

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

        private bool ConsistenteVariacaoBTC(double cotacao)
        {
            var mediaCotacoes = usuario.PedidoPagamento.Average(x => x.CotacaoCripto);

            return (cotacao * 100 / mediaCotacoes - 100) <= 30 && (cotacao * 100 / mediaCotacoes - 100) >= -30;
        }

        private string ValorMinimoSaque(int nivelAssociacao)
        {
            switch (nivelAssociacao)
            {
                case 1:  //    100
                case 2:  //    300
                case 3:  //    500
                    return "60";
                case 4:  //  1.000
                case 5:  //  3.000
                    return "80";
                case 6:  //  5.000
                    return "180";
                case 7:  // 10.000
                case 8:  // 20.000
                case 9:  // 30.000
                case 10: // 50.000
                    return "360";
            }

            return "60";
        }

        #endregion

        #region Actions

        public ActionResult Index()
        {
            obtemMensagem();

            SolicitaSaque();

            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            ViewBag.MascaraSaida = moedaPadrao.MascaraOut;
            ViewBag.MascaraEntrada = moedaPadrao.MascaraIn;
            ViewBag.TaxaSaqueRentabilidade = ConfiguracaoHelper.GetString("SAQUE_TAXA_EXIBIDO");
            ViewBag.TaxaSaqueBonus = ConfiguracaoHelper.GetString("SAQUE_TAXA_EXIBIDO");
            ViewBag.TaxaSaqueInvest = ConfiguracaoHelper.GetString("SAQUE_TAXA_EXIBIDO");
            ViewBag.DiaSaqueRentabilidade = ConfiguracaoHelper.GetString("SAQUE_DIAS");
            List<Saque> saqueContaRentabilidade = saqueRepository.BuscaSaquePorConta(usuario.ID, 1).ToList();
            List<Saque> saqueContaBonus = saqueRepository.BuscaSaquePorConta(usuario.ID, 2).ToList();
            List<Saque> saqueContaInvest = saqueRepository.BuscaSaquePorConta(usuario.ID, 8).ToList();
            ContaDeposito conta = usuario.ContaDeposito.FirstOrDefault();

            ViewBag.SaldoRentabilidade = @moedaPadrao.Simbolo + " " + usuarioContainer.SaldoRentabilidade.ToString(moedaPadrao.MascaraOut);
            ViewBag.SaldoBonus = @moedaPadrao.Simbolo + " " + usuarioContainer.SaldoBonus.ToString(moedaPadrao.MascaraOut);
            ViewBag.SaldoInvestimento = @moedaPadrao.Simbolo + " " + usuarioContainer.SaldoInvestimento.ToString(moedaPadrao.MascaraOut);

            ViewBag.ContaDeposito = "";
            ViewBag.ExibeBotaoRenovacao = false;
            ViewBag.ExibeBotaoRenovacaoAutomatica = false;

            if (usuario.RenovacaoAutomatica.HasValue)
            {
                if (!usuario.RenovacaoAutomatica.Value)
                { 
                    ViewBag.ExibeBotaoRenovacaoAutomatica = true; 
                }
            }
            else
            {
                ViewBag.ExibeBotaoRenovacaoAutomatica = true;
            }

            if (usuario.DataRenovacao.HasValue && ViewBag.ExibeBotaoRenovacaoAutomatica)
            {
                if (usuario.DataRenovacao.Value <= DateTime.Now)
                {
                    ViewBag.ExibeBotaoRenovacao = true;
                }
            }

            if (conta == null)
            {
                string[] strMensagem = new string[] { traducaoHelper["CONTA_DEPOSITO_NAO_EXISTE"] };
                Mensagem("Sucesso", strMensagem, "msg");
                obtemMensagem();
            }
            else
            {
                if (conta.MoedaIDCripto == (int)Moeda.Moedas.BTC)
                {
                    ViewBag.ContaDeposito = "BTC";
                    string contaBTC = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Bitcoin);
                    contaBTC = cpUtilities.Gerais.Morpho(contaBTC, TipoCriptografia.Descriptografa);
                    ViewBag.ContaBTC = contaBTC;
                }
                if (conta.MoedaIDCripto == (int)Moeda.Moedas.LTC)
                {
                    string contaLTC = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Litecoin);
                    contaLTC = cpUtilities.Gerais.Morpho(contaLTC, TipoCriptografia.Descriptografa);
                    ViewBag.ContaDeposito = "LTC";
                    ViewBag.ContaLTC = contaLTC;
                }
                if (conta.MoedaIDCripto == (int)Moeda.Moedas.USDT)
                {
                    string contaUSDT = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Tether);
                    contaUSDT = cpUtilities.Gerais.Morpho(contaUSDT, TipoCriptografia.Descriptografa);
                    ViewBag.ContaDeposito = "USDT";
                    ViewBag.ContaUSDT = contaUSDT;
                }
            }

            foreach (Saque item in saqueContaRentabilidade)
            {
                if (!String.IsNullOrEmpty(item.Carteira))
                {
                    string contaCripto = Core.Helpers.CriptografiaHelper.Descriptografar(item.Carteira);
                    contaCripto = cpUtilities.Gerais.Morpho(contaCripto, TipoCriptografia.Descriptografa);
                    item.Carteira = contaCripto;
                }
            }

            foreach (Saque item in saqueContaBonus)
            {
                if (!String.IsNullOrEmpty(item.Carteira))
                {
                    string contaCripto = Core.Helpers.CriptografiaHelper.Descriptografar(item.Carteira);
                    contaCripto = cpUtilities.Gerais.Morpho(contaCripto, TipoCriptografia.Descriptografa);
                    item.Carteira = contaCripto;
                }
            }

            foreach (Saque item in saqueContaInvest)
            {
                if (!String.IsNullOrEmpty(item.Carteira))
                {
                    string contaCripto = Core.Helpers.CriptografiaHelper.Descriptografar(item.Carteira);
                    contaCripto = cpUtilities.Gerais.Morpho(contaCripto, TipoCriptografia.Descriptografa);
                    item.Carteira = contaCripto;
                }
            }

            ViewBag.SaquesContaRentabilidade = saqueContaRentabilidade;
            ViewBag.SaquesContaBonus = saqueContaBonus;
            ViewBag.SaquesContaInvest = saqueContaInvest;

            #region Saque Bônus

            var valorMinimo = Convert.ToDouble(ConfiguracaoHelper.GetString("SAQUE_VALOR_MINIMO"), CultureInfo.GetCultureInfo("en-US"));
            ViewBag.ValorMinimoSaqueRentabilidade = valorMinimo.ToString(ViewBag.MascaraSaida);

            var valorMinimoBonus = Convert.ToDouble(ValorMinimoSaque(usuario.NivelAssociacao), CultureInfo.GetCultureInfo("en-US"));
            ViewBag.ValorMinimoSaqueBonus = valorMinimoBonus.ToString(ViewBag.MascaraSaida);

            var valorMinimoInvest = Convert.ToDouble(ConfiguracaoHelper.GetString("SAQUE_VALOR_MINIMO"), CultureInfo.GetCultureInfo("en-US"));
            ViewBag.ValorMinimoSaqueInvest = valorMinimo.ToString(ViewBag.MascaraSaida);

            #endregion

            return View();
        }

        public ActionResult Cancelar(int id)
        {
            SolicitaSaque();
            var saque = this.repository.Get(id);

            if (saque != null)
            {
                if (saque.StatusAtual.Status != Core.Entities.SaqueStatus.TodosStatus.Indefinido && saque.StatusAtual.Status != Core.Entities.SaqueStatus.TodosStatus.Solicitado)
                {
                    return RedirectToAction("Index", new { mensagem = traducaoHelper["SAQUE_ANDAMENTO"] });
                }

                var limiteCancelamento = saque.StatusInicial.Data;
                while (limiteCancelamento.DayOfWeek != DayOfWeek.Friday)
                {
                    limiteCancelamento = limiteCancelamento.AddDays(1);
                }
                limiteCancelamento = new DateTime(limiteCancelamento.Year, limiteCancelamento.Month, limiteCancelamento.Day, 23, 59, 59, 999);

                if (App.DateTimeZion > limiteCancelamento)
                {
                    return RedirectToAction("Index", new { mensagem = traducaoHelper["SAQUE_CANCELAMENTO_NEGADO_DIA_SEMANA"] });
                }

                SaqueStatus saqueStatus = new SaqueStatus();
                saqueStatus.Data = App.DateTimeZion;
                saqueStatus.SaqueID = saque.ID;
                saqueStatus.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Cancelado;
                saqueStatus.Ultimo = true;

                saqueStatusRepository.GravaSaqueStatus(saqueStatus);

                var tipoLctoSaque = Core.Entities.Lancamento.Tipos.Saque.GetHashCode();

                Lancamento lcto = lancamentoRepository.GetByExpression(l => l.ReferenciaID == saque.ID).FirstOrDefault();

                if (lcto != null)
                {
                    var cancelarSaque = new Lancamento()
                    {
                        UsuarioID = usuario.ID,
                        ContaID = lcto.ContaID, //Receber
                        CategoriaID = lcto.CategoriaID, //CategoriaID = 9 é Saque - Tabela Finaceiro.Categoria
                        Tipo = lcto.Tipo,
                        ReferenciaID = lcto.ReferenciaID,
                        Valor = -lcto.Valor,
                        Descricao = lcto.Descricao += " (" + traducaoHelper["CANCELADO"] + ")",
                        DataCriacao = App.DateTimeZion,
                        DataLancamento = App.DateTimeZion,
                        MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                    };

                    try
                    {
                        lancamentoRepository.Save(cancelarSaque);
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
                        return Json(traducaoHelper["ERRO: " + strRetErro]);
                    }
                    catch (Exception ex)
                    {
                        return Json(traducaoHelper["ERRO: " + ex.Message]);
                    }
                }

                string[] strMensagem = new string[] { traducaoHelper["SOLICITACAO_SAQUE_CANCELADO"] };
                Mensagem("Sucesso", strMensagem, "msg");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TransferirRentabilidade(string login, int valor, string token = null)
        {
            try
            {
                if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
                {
                    if (!VerificaAutenticacao2FA(token))
                        return Json(traducaoHelper["TOKEN_INVALIDO"]);
                }

                login = login.Trim();
                if (!String.IsNullOrEmpty(cpUtilities.Gerais.VerificarSQLInjection(login)))
                {
                    Local.Log("Tentativa de SQLInjection, Usuario: " + usuario.ID + " em TransferirRentabilidade", "Seguranca");
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
                var loginExistente = usuarioRepository.GetByLogin(login);
                if (loginExistente != null && loginExistente.ID > 0)
                {
                    var franqueadoTransferir = this.repository.Get(loginExistente.ID);
                    this.Transferir(usuario, loginExistente, valor, (int)Conta.Contas.Rentabilidade);
                }
                else
                {
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("OK");
        }

        [HttpPost]
        public ActionResult TransferirBonus(string login, int valor, string token = null)
        {
            try
            {
                if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
                {
                    if (!VerificaAutenticacao2FA(token))
                        return Json(traducaoHelper["TOKEN_INVALIDO"]);
                }
                login = login.Trim();
                if (!String.IsNullOrEmpty(cpUtilities.Gerais.VerificarSQLInjection(login)))
                {
                    Local.Log("Tentativa de SQLInjection, Usuario: " + usuario.ID + " em TransferirBonus", "Seguranca");
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
                var loginExistente = usuarioRepository.GetByLogin(login);
                if (loginExistente != null && loginExistente.ID > 0)
                {
                    var franqueadoTransferir = this.repository.Get(loginExistente.ID);
                    this.Transferir(usuario, loginExistente, valor, (int)Conta.Contas.Bonus);
                }
                else
                {
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("OK");
        }

        [HttpPost]
        public ActionResult TransferirInvestimento(string login, int valor, string token = null)
        {
            try
            {
                if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
                {
                    if (!VerificaAutenticacao2FA(token))
                        return Json(traducaoHelper["TOKEN_INVALIDO"]);
                }
                login = login.Trim();
                if (!String.IsNullOrEmpty(cpUtilities.Gerais.VerificarSQLInjection(login)))
                {
                    Local.Log("Tentativa de SQLInjection, Usuario: " + usuario.ID + " em TransferirInvestimento", "Seguranca");
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
                var loginExistente = usuarioRepository.GetByLogin(login);
                if (loginExistente != null && loginExistente.ID > 0)
                {
                    var franqueadoTransferir = this.repository.Get(loginExistente.ID);
                    this.Transferir(usuario, loginExistente, valor, (int)Conta.Contas.Investimento);
                }
                else
                {
                    return Json(traducaoHelper["NENHUM_USUARIO_LOCALIZADO"]);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("OK");
        }

        public void Transferir(Usuario de, Usuario para, double valor, int contaId)
        {
            valor = valor / 100;

            //Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");

            double valorMinimo = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_MINIMO");
            double valorMinimoBeneficiario = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_MINIMO_BENEFICIARIO");
            double valorTaxa = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_TAXA");

            //Em % do valor solicitado
            valorTaxa = valor * valorTaxa / 100;

            if (de.ID == para.ID)
            {
                throw new Exception(traducaoHelper["VOCE_NAO_PODE_TRANSFERIR_PARA_VOCE"]);
            }
            //Seguranca De não pode ser outro q o usuario logado
            if (de.ID != usuario.ID)
            {
                Local.Log("Tentativa de tranferência com outro usuario que não o logado, Usuario: " + usuario.ID + " de: " + de.ID + " para: " + para.ID, "Seguranca");
                throw new Exception(traducaoHelper["ACESSO_INDEVIDO_REPORTADO"]);
            }
            if (valor < valorMinimo)
            {
                throw new Exception(traducaoHelper["VALOR_MINIMO_TRANSFERENCIA"] + valorMinimo.ToString());
            }
            double saldo = 0.0;
            switch (contaId)
            {
                case (int)Conta.Contas.Rentabilidade:
                    saldo = usuarioContainer.SaldoRentabilidade;
                    break;
                case (int)Conta.Contas.Bonus:
                    saldo = usuarioContainer.SaldoBonus;
                    break;
                case (int)Conta.Contas.Investimento:
                    saldo = usuarioContainer.SaldoInvestimento;
                    break;
                case (int)Conta.Contas.Transferencias:
                    saldo = usuarioContainer.SaldoTransferencias;
                    break;
                default:
                    saldo = usuarioContainer.Saldo;
                    break;
            }

            if ((valor + valorTaxa) > saldo)
            {
                throw new Exception(traducaoHelper["SALDO_INSUFICIENTE"] + " " + traducaoHelper["SALDO_NECESSARIO"] +": " + moedaPadrao.Simbolo.ToUpper() + " " + (valor + valorTaxa).ToString(moedaPadrao.MascaraOut)) ;
            }

            var saldoBeneficiario = usuarioRepository.Saldo(para.ID);

            if (saldoBeneficiario < valorMinimoBeneficiario)
            {
                throw new Exception(traducaoHelper["SALDO_INSUFICIENTE_BENEFICIARIO"]);
            }

            var debito = new Lancamento()
            {
                CategoriaID = (int)Categoria.Categorias.Transferencia, //CategoriaID = 7 é Transferencia - Tabela Finaceiro.Categoria
                ContaID = contaId,
                DataCriacao = App.DateTimeZion,
                DataLancamento = App.DateTimeZion,
                UsuarioID = de.ID,
                Descricao = traducaoHelper["TRANSFERIR_PARA"] + para.Login,
                ReferenciaID = para.ID,
                Tipo = Lancamento.Tipos.Transferencia,
                Valor = -valor,
                MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
            };
            lancamentoRepository.Save(debito);

            if (valorTaxa > 0)
            {
                var debitoTaxa = new Lancamento()
                {
                    CategoriaID = (int)Categoria.Categorias.TaxaDeTransferencia, //CategoriaID = 18 é Taxa de Transferencia - Tabela Finaceiro.Categoria
                    ContaID = contaId, //Taxa cobrada do usuario DE
                    DataCriacao = App.DateTimeZion,
                    DataLancamento = App.DateTimeZion,
                    UsuarioID = de.ID,
                    Descricao = traducaoHelper["TAXA_TRASFERENCIA_PARA"] + para.Login,
                    ReferenciaID = debito.ID,
                    Tipo = Lancamento.Tipos.Taxa,
                    Valor = -valorTaxa,
                    MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                };
                lancamentoRepository.Save(debitoTaxa);
            }

            var credito = new Lancamento()
            {
                CategoriaID = (int)Categoria.Categorias.Transferencia, //CategoriaID = 7 é Transferencia - Tabela Finaceiro.Categoria
                ContaID = (int)Conta.Contas.Transferencias, //Transferencia credito para usuario PARA
                DataCriacao = App.DateTimeZion,
                DataLancamento = App.DateTimeZion,
                UsuarioID = para.ID,
                Descricao = traducaoHelper["TRANSFERENCIA_DE"] + de.Login,
                ReferenciaID = de.ID,
                Tipo = Lancamento.Tipos.Transferencia,
                Valor = valor,
                MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
            };
            lancamentoRepository.Save(credito);
        }

        #endregion

        #region JsonResult

        [HttpPost]
        public async Task<JsonResult> Solicitar(string valor, string senha, string token = null, string tipo = null)
        {
            string strSenha = CriptografiaHelper.Descriptografar(usuario.Senha);

            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                if (!VerificaAutenticacao2FA(token))
                    return Json(traducaoHelper["TOKEN_INVALIDO"]);
            }

            SolicitaSaque();
            double saqueFinal = 0;
            double feeSaque = ConfiguracaoHelper.GetDouble("SAQUE_FEE");
            int contaIdParaSaque = 0;
            //== Obtem a Taxa de saque parametrizada
            double taxa = ConfiguracaoHelper.GetDouble("SAQUE_TAXA");

            //Verificar esse saldo
            double saldo = 0.0;

            //1 - Bonus de Rendimento; 2 - Bonus de Equipe; 7 - Transferencias; 8 - Valor Investido
            //9 - BitCoin; 10 - LiteCoin e 14 - Saldo
            switch (tipo)
            {
                case "1": //Rentabilidade
                    if (ViewBag.SolicitarSaqueRentabilidade == SolicitacaoSaqueStatus.DiaNaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_DIA_NAO_PERMITIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueRentabilidade == SolicitacaoSaqueStatus.NaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_NAO_PERMIIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueRentabilidade == SolicitacaoSaqueStatus.JaSacou)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_JA_SACOU"]);
                    }
                    //1 - Bonus de Rendimento; 2 - Bonus de Equipe
                    //saldo = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade && l.TipoID != (int)Lancamento.Tipos.Compra && l.Valor.HasValue).Sum(l => l.Valor.Value);
                    saldo = usuarioContainer.SaldoRentabilidade;
                    contaIdParaSaque = (int)Conta.Contas.Rentabilidade;
                    taxa = ConfiguracaoHelper.GetDouble("SAQUE_TAXA_RENTABILIDADE");
                    feeSaque = ConfiguracaoHelper.GetDouble("SAQUE_FEE_RENTABILIDADE");
                    break;
                case "2": //Bonus (de equipe)
                    if (ViewBag.SolicitarSaqueBonus == SolicitacaoSaqueStatus.DiaNaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_DIA_NAO_PERMITIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueBonus == SolicitacaoSaqueStatus.NaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_NAO_PERMIIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueBonus == SolicitacaoSaqueStatus.JaSacou)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_JA_SACOU"]);
                    }
                    //ContaID == 2 é Bonus de equipe
                    //saldo = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Bonus && l.TipoID != (int)Lancamento.Tipos.Compra && l.Valor.HasValue).Sum(l => l.Valor.Value);
                    saldo = saldo = usuarioContainer.SaldoBonus;
                    contaIdParaSaque = (int)Conta.Contas.Bonus;
                    taxa = ConfiguracaoHelper.GetDouble("SAQUE_TAXA_BONUS");
                    feeSaque = ConfiguracaoHelper.GetDouble("SAQUE_FEE_BONUS");
                    break;
                case "3": //Investimento
                    if (ViewBag.SolicitarSaqueInvest == SolicitacaoSaqueStatus.DiaNaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_DIA_NAO_PERMITIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueInvest == SolicitacaoSaqueStatus.NaoPermitido)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_NAO_PERMIIDO"]);
                    }
                    else if (ViewBag.SolicitarSaqueInvest == SolicitacaoSaqueStatus.JaSacou)
                    {
                        return Json(traducaoHelper["SAQUE_SOLICITACAO_JA_SACOU"]);
                    }
                    //ContaID == 8 é investimento
                    //saldo = usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Investimento && l.TipoID != (int)Lancamento.Tipos.Compra && l.Valor.HasValue).Sum(l => l.Valor.Value);
                    saldo = saldo = usuarioContainer.SaldoInvestimento;
                    contaIdParaSaque = (int)Conta.Contas.Investimento;
                    taxa = ConfiguracaoHelper.GetDouble("SAQUE_TAXA_INVESTIMENTO");
                    feeSaque = ConfiguracaoHelper.GetDouble("SAQUE_FEE_INVESTIMENTO");
                    break;
                default:
                    return Json(traducaoHelper["PARAMETRO_INVALIDO"]);
            }

            var valorLimpo = valor.Replace("_", "").Replace(",", ".");
            if (string.IsNullOrEmpty(valor))
            {
                return Json(new { valorInvalido = true });
            }
            saqueFinal = Convert.ToDouble(valorLimpo, new CultureInfo("en"));

            //Verifica se usuário possui saldo para o saque solicitado 
            if (saldo < saqueFinal)
            {
                return Json(String.Format(traducaoHelper["SOLICITACAO_SAQUE_SALDO_INSUFICIENTE"], moedaPadrao.Simbolo.ToUpper(), saldo.ToString(moedaPadrao.MascaraOut)));
            }

            var conta = usuario.ContaDeposito.FirstOrDefault();
            if (conta == null)
            {
                //Usuario deve cadastrar sua conta antes
                return Json(traducaoHelper["CADASTRAR_DADOS_ALERTA"]);
            }

            var valorMinimoUSD = Convert.ToDouble(ConfiguracaoHelper.GetString("SAQUE_VALOR_MINIMO"), CultureInfo.GetCultureInfo("en-US"));

            Moeda moeda = usuario.Pais.Moeda;
            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
            {
                moeda = db.Moedas.Find(3); //Default Dollar 
            }

            var liquido = saqueFinal - taxa - (saqueFinal * feeSaque / 100);

            var saque = new Core.Entities.Saque();
            saque.MoedaID = (int)Moeda.Moedas.NEN;
            saque.MoedaIDCripto = (int)Moeda.Moedas.NEN;

            if (usuario.Bancos.Count == 0)
            {
                var usuariobanco = new Banco
                {
                    Dados = "",
                    UsuarioID = usuario.ID,
                    Principal = true
                };

                bancoRepository.Save(usuariobanco);
                saque.BancoID = usuariobanco.ID;
            }
            else
            {
                saque.BancoID = usuario.Bancos.FirstOrDefault().ID;
            }

            if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
            {
                var usuarioGanho = usuarioGanhoRepository.GetAtual(usuario.ID);
                if (usuarioGanho.DataAtingiuLimite.HasValue)
                {
                    return Json(String.Format(traducaoHelper["LIMITE_POR_PLANO"]));
                }
            }
            //switch (conta.MoedaIDCripto)
            //{
            //    case (int)Moeda.Moedas.LTC:
            //        double cotacaoLTC = await moedaCotacaoRepository.GetLTCDolar1Async();

            //        if ((conta == null) || string.IsNullOrEmpty(conta.Litecoin))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_LTC_INVALIDA"]);
            //        }
            //        string contaLTC = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Litecoin);
            //        contaLTC = cpUtilities.Gerais.Morpho(contaLTC, TipoCriptografia.Descriptografa);
            //        if (!BlockchainService.ValidarCarteiraLitecoin(contaLTC))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_LTC_INVALIDA"]);
            //        }
            //        if (saqueFinal < valorMinimoUSD)
            //        {
            //            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
            //            var valorMinimo = (valorMinimoUSD / cotacaoLTC).ToString(moedaPadrao.MascaraOut);
            //            return Json(string.Format(traducaoHelper["SOLICITACAO_SAQUE_LIMITE_MINIMO"], moedaPadrao.Simbolo.ToUpper(), valorMinimo));
            //        }
            //        saque.MoedaID = (int)Moeda.Moedas.LTC;
            //        saque.MoedaIDCripto = (int)Moeda.Moedas.LTC;
            //        saque.Carteira = conta.Litecoin;
            //        saque.TotalCripto = saqueFinal / cotacaoLTC;
            //        saque.Fee = feeSaque;
            //        break;
            //    case (int)Moeda.Moedas.BTC:
            //        double CotacaoCripto = await moedaCotacaoRepository.GetBTCDolar1Async();

            //        if ((conta == null) || string.IsNullOrEmpty(conta.Bitcoin))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_BTC_INVALIDA"]);
            //        }

            //        string contaBTC = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Bitcoin);
            //        contaBTC = cpUtilities.Gerais.Morpho(contaBTC, TipoCriptografia.Descriptografa);
            //        if (!BlockchainService.ValidarCarteiraBitcoin(contaBTC))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_BTC_INVALIDA"]);
            //        }
            //        if (saqueFinal < valorMinimoUSD)
            //        {
            //            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
            //            var valorMinimo = (valorMinimoUSD / CotacaoCripto).ToString(moedaPadrao.MascaraOut);
            //            return Json(string.Format(traducaoHelper["SOLICITACAO_SAQUE_LIMITE_MINIMO"], moedaPadrao.Simbolo.ToUpper(), valorMinimo));
            //        }
            //        saque.MoedaID = (int)Moeda.Moedas.BTC;
            //        saque.MoedaIDCripto = (int)Moeda.Moedas.BTC;
            //        saque.Carteira = conta.Bitcoin;
            //        saque.TotalCripto = saqueFinal / CotacaoCripto;
            //        saque.Fee = feeSaque;
            //        break;
            //    default: //USDT
            //        double cotacaoTether = await moedaCotacaoRepository.GetTetherDolar1Async();

            //        if ((conta == null) || string.IsNullOrEmpty(conta.Tether))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_TETHER_INVALIDA"]);
            //        }
            //        string contaTether = Core.Helpers.CriptografiaHelper.Descriptografar(conta.Tether);
            //        contaTether = cpUtilities.Gerais.Morpho(contaTether, TipoCriptografia.Descriptografa);

            //        if (!BlockchainService.ValidarCarteiraTether(contaTether))
            //        {
            //            return Json(traducaoHelper["CARTEIRA_TETHER_INVALIDA"]);
            //        }
            //        if (saqueFinal < valorMinimoUSD)
            //        {
            //            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
            //            var valorMinimo = (valorMinimoUSD / cotacaoTether).ToString(moedaPadrao.MascaraOut);
            //            return Json(string.Format(traducaoHelper["SOLICITACAO_SAQUE_LIMITE_MINIMO"], moedaPadrao.Simbolo.ToUpper(), valorMinimo));
            //        }
            //        saque.MoedaID = (int)Moeda.Moedas.USDT;
            //        saque.MoedaIDCripto = (int)Moeda.Moedas.USDT;
            //        saque.Carteira = conta.Tether;
            //        saque.TotalCripto = saqueFinal / cotacaoTether;
            //        saque.Fee = feeSaque;
            //        break;
            //}

            saque.MoedaID = moeda.ID;
            saque.UsuarioID = usuario.ID;
            saque.Taxas = taxa;
            saque.Total = saqueFinal;
            saque.Liquido = liquido;
            saque.Data = App.DateTimeZion;

            try
            {
                saqueRepository.Save(saque);
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
                return Json(traducaoHelper["ERRO: " + strRetErro]);
            }
            catch (Exception ex)
            {
                return Json(traducaoHelper["ERRO: " + ex.Message]);
            }

            //var saqueStatus = new Core.Entities.SaqueStatus { Data = App.DateTimeZion, SaqueID = saque.ID, Status = Core.Entities.SaqueStatus.TodosStatus.Solicitado, Saque = saque };
            var saqueStatus = new Core.Entities.SaqueStatus()
            {
                Data = App.DateTimeZion,
                SaqueID = saque.ID,
                Status = Core.Entities.SaqueStatus.TodosStatus.Solicitado,
                Saque = saque,
                Ultimo = true
            };
            try
            {
                saqueStatusRepository.Save(saqueStatus);
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
                return Json(traducaoHelper["ERRO: " + strRetErro]);
            }
            catch (Exception ex)
            {
                return Json(traducaoHelper["ERRO: " + ex.Message]);
            }

            var debitoSaque = new Lancamento()
            {
                UsuarioID = usuario.ID,
                ContaID = contaIdParaSaque, //Receber
                CategoriaID = (int)Categoria.Categorias.Saque, //CategoriaID = 9 é Saque - Tabela Finaceiro.Categoria
                Tipo = Lancamento.Tipos.Saque,
                ReferenciaID = saque.ID,
                Valor = -saque.Total,
                Descricao = traducaoHelper["SOLICITACAO_SAQUE"],
                DataCriacao = App.DateTimeZion,
                DataLancamento = App.DateTimeZion,
                MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
            };
            try
            {
                lancamentoRepository.Save(debitoSaque);
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
                return Json(traducaoHelper["ERRO: " + strRetErro]);
            }
            catch (Exception ex)
            {
                return Json(traducaoHelper["ERRO: " + ex.Message]);
            }

            return Json("OK");
        }

        [HttpPost]
        public JsonResult solicitarRenovacao(string valor = null, string token = null)
        {
            //Não faz nada com o valor, por hora

            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                if (!VerificaAutenticacao2FA(token))
                    return Json(traducaoHelper["TOKEN_INVALIDO"]);
            }
            var callSpRenovacao = contaRepository.CallSpRenovacao(usuario.ID);

            return Json("OK");
        }

        [HttpPost]
        public JsonResult solicitarRenovacaoAutomatica(string valor = null, string token = null)
        {
            //Não faz nada com o valor, por hora
            
            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                if (!VerificaAutenticacao2FA(token))
                    return Json(traducaoHelper["TOKEN_INVALIDO"]);
            }
            var callSpRenovacaoAutomatica = contaRepository.CallSpRenovacaoAutomatica(usuario.ID, true);

            return Json("OK");
        }

        [HttpPost]
        public JsonResult solicitarCancelarRenovacaoAutomatica(string valor = null, string token = null)
        {
            //Não faz nada com o valor, por hora
          
            if (ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                if (!VerificaAutenticacao2FA(token))
                    return Json(traducaoHelper["TOKEN_INVALIDO"]);
            }
            var callSpCancelarRenovacaoAutomatica = contaRepository.CallSpRenovacaoAutomatica(usuario.ID, false);

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
