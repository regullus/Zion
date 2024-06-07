#region Bibliotecas

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Sistema.ModelBinders;
using Core.Repositories.Usuario;
using Core.Entities;
using Core.Repositories.Rede;
using Core.Helpers;
using System.Text;
using System.Net;
using Fluentx;
using Core.Repositories.Financeiro;
using Core.Models;
using PagedList;
using Coinpayments.Api;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class MinhaRedeController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis

        private YLEVELEntities db = new YLEVELEntities();

        #endregion

        #region Core

        private UsuarioRepository usuarioRepository;
        private AssociacaoRepository associacaoRepository;
        private ClassificacaoRepository classificacaoRepository;
        private List<Usuario> filhosPosicao;
        private List<NoRedeBinder> _listaRetorno;
        private List<Associacao> _listaAssociacoes;
        private TabuleiroRepository tabuleiroRepository;
        private LancamentoRepository lancamentoRepository;

        private int idNo = 1;

        private const int NUMERO_USUARIOS_REDE_EXIBICAO = 1000;
        private Moeda moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
        private int diasPerdaPosicao;

        Random rndNumber = new Random();

        private int QuantidadeNivelExibicao = 3;

        public MinhaRedeController(DbContext context)
            : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            classificacaoRepository = new ClassificacaoRepository(context);
            tabuleiroRepository = new TabuleiroRepository(context);
            lancamentoRepository = new LancamentoRepository(context);

            moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();
            diasPerdaPosicao = Core.Helpers.ConfiguracaoHelper.GetInt("DIAS_PERDA_POSICAO_INATIVIDADE");
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

        public ActionResult Galaxy(int? idTab)
        {
            obtemMensagem();

            #region variaveis

            string log = "";

            ViewBag.Background = "background-image: url(" + @Url.Content("~/Arquivos/banners/" + Helpers.Local.Sistema + "/fundo.jpg") + "); background-repeat: no-repeat; background-color: #000000; background-size: cover;";
            ViewBag.idUsuario = usuario.ID;
            ViewBag.tabuleiroName = traducaoHelper["GALAXIA"];

            string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");
            tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);
            ViewBag.Token = tokenLocal;
            ViewBag.ShowReportPayment = false;
            ViewBag.RedeTabuleiro = true;
            ViewBag.idTabuleiro = 0;
            ViewBag.tabuleiro = null;
            ViewBag.tabuleirosUsuario = null;
            ViewBag.tabuleiroAtivo = null;
            ViewBag.Timer = null;
            ViewBag.NovoUsuario = false;
            ViewBag.ShowMsgFaltaPag = "";
            ViewBag.TabuleiroOpacity = false;
            ViewBag.InfoUsuario = "";
            ViewBag.HaConvite = "";
            ViewBag.TotalRecebimento = 0;
            ViewBag.TimerConvite = false;
            ViewBag.TimerPagamento = false;
            ViewBag.Pagar = false;
            ViewBag.PagarInforme = false;
            ViewBag.TimerConvite = false;
            ViewBag.ShowExitBoard = false;

            int idTabuleiro = idTab ?? 0;

            #endregion

            if (idTabuleiro < 0)
            {
                idTabuleiro = 0;
            }

            try
            {
                TabuleiroModel tabuleiro = null;
                TabuleiroUsuarioModel tabuleiroUsuario = null;

                log = "galaxy 01";
                IEnumerable<Core.Models.TabuleiroUsuarioModel> tabuleirosUsuario = tabuleiroRepository.ObtemTabuleirosUsuario(usuario.ID);
                ViewBag.tabuleirosUsuario = tabuleirosUsuario;

                log = "user 01";
                //Verifica se usuario já esta em nivel superior StatusID != 0 em boards superiores ao primeiro board
                bool blnTabuleiroAtivos = tabuleirosUsuario.Where(x => x.StatusID != 0 && x.BoardID > 1).Count() > 0;

                log = "user active";
                //Verifica se há um convite ativo para o usuario
                bool blnConviteAtivo = tabuleirosUsuario.Where(x => x.StatusID == 2).Count() > 0;

                log = "invitation 01";
                //Mensagem que o usuario tem um convite
                if (blnConviteAtivo)
                {
                    ViewBag.HaConvite = traducaoHelper["CONVITE_ENTRAR"];
                }
                else
                {
                    ViewBag.HaConvite = "";
                }

                if (idTabuleiro > 0)
                {
                    log = "galaxy 02.1";
                    tabuleiroUsuario = tabuleirosUsuario.Where(x => x.TabuleiroID == idTabuleiro).FirstOrDefault();
                }
                else
                {
                    log = "galaxy 02.2";
                    //Obtem 1º Tabuleiro ativo do usuario
                    tabuleiroUsuario = tabuleirosUsuario.Where(x => x.StatusID != 2).FirstOrDefault();
                }

                //Verifica se o tabuleiro esta fechado, para um masterID logado
                if (tabuleiroUsuario != null && usuario.ID == tabuleiroUsuario.MasterID)
                {
                    log = "galaxy closed 01";
                    //Verifica se o tabuleiro esta fechado
                    string tabuleiroFechado = tabuleiroRepository.TabuleiroFechado(tabuleiroUsuario.TabuleiroID ?? 0);

                    if (tabuleiroFechado != "sim")
                    {
                        log = "target 01";
                        //Master não tendo pendencias
                        //Tenta fechar tabuleiro
                        string tabuleiroCompleta = tabuleiroRepository.IncluiTabuleiro(usuario.ID, tabuleiroUsuario.MasterID, tabuleiroUsuario.BoardID, "Completa");
                        if (tabuleiroCompleta == "OK")
                        {
                            log = "include 01";
                            //Tabuleiro pode ter sido fechado ou não, ver log para confirmar

                            //Refaz carregamento de variaveis, considerando que o tabuleiro foi fechado
                            tabuleirosUsuario = tabuleiroRepository.ObtemTabuleirosUsuario(usuario.ID);
                            ViewBag.tabuleirosUsuario = tabuleirosUsuario;

                            log = "active 01";
                            //Verifica se usuario já esta em nivel superior StatusID != 0 em boards superiores ao primeiro board
                            blnTabuleiroAtivos = tabuleirosUsuario.Where(x => x.StatusID != 0 && x.BoardID > 1).Count() > 0;

                            log = "invitation 02";
                            //Verifica se há um convite ativo para o usuario
                            blnConviteAtivo = tabuleirosUsuario.Where(x => x.StatusID == 2).Count() > 0;

                            log = "invitation 03";
                            //Mensagem que o usuario tem um convite
                            if (blnConviteAtivo)
                            {
                                ViewBag.HaConvite = traducaoHelper["CONVITE_ENTRAR"];
                            }
                            else
                            {
                                ViewBag.HaConvite = "";
                            }

                            if (idTabuleiro > 0)
                            {
                                log = "galaxy 03.1";
                                tabuleiroUsuario = tabuleirosUsuario.Where(x => x.TabuleiroID == idTabuleiro).FirstOrDefault();
                            }
                            else
                            {
                                log = "galaxy 03.2";
                                //Obtem 1º Tabuleiro ativo do usuario
                                tabuleiroUsuario = tabuleirosUsuario.Where(x => x.StatusID != 2).FirstOrDefault();
                            }
                        }
                    }
                }
                //Caso seja um convite StatusID =2 - Obtem o 1º Ativo
                if (tabuleiroUsuario != null && tabuleiroUsuario.StatusID == 2 && !blnTabuleiroAtivos)
                {
                    log = "galaxy 04";
                    tabuleiroUsuario = tabuleirosUsuario.Where(x => x.StatusID == 0).OrderBy(x => x.BoardID).FirstOrDefault();
                }

                if (tabuleiroUsuario != null && tabuleiroUsuario.StatusID == 2 && blnTabuleiroAtivos)
                {
                    log = "galaxy 05";
                    tabuleiroUsuario = tabuleirosUsuario.Where(x => x.StatusID == 1).OrderBy(x => x.BoardID).FirstOrDefault();
                }

                if (tabuleiroUsuario != null)
                {
                    log = "galaxy 06";
                    idTabuleiro = tabuleiroUsuario.TabuleiroID ?? 0;
                    ViewBag.tabuleiroAtivo = tabuleiroUsuario;
                }
                else
                {
                    log = "galaxy 07";
                    //Obtem 1º tabuleiro ativo do usuario
                    tabuleiroUsuario = tabuleirosUsuario.Where(x => x.StatusID == 1).OrderBy(x => x.BoardID).FirstOrDefault();
                    if (tabuleiroUsuario != null)
                    {
                        log = "galaxy 08";
                        idTabuleiro = tabuleiroUsuario.TabuleiroID ?? 0;
                        ViewBag.tabuleiroAtivo = tabuleiroUsuario;
                    }
                    else
                    {
                        log = "galaxy 09";
                        idTabuleiro = 0;
                    }
                }

                if (idTabuleiro != 0)
                {
                    log = "galaxy 10";
                    ViewBag.idTabuleiro = idTabuleiro;

                    //Timer
                    if (tabuleiroUsuario != null)
                    {
                        log = "timer 01";
                        if (!tabuleiroUsuario.InformePag)
                        {
                            log = "timer 02";
                            int tempoMin = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_PAGAMENTO");
                            int tempoMax = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_MAX_PAGAMENTO");

                            if (tempoMin == 0)
                            {
                                tempoMin = 15;
                            }
                            if (tempoMax == 0)
                            {
                                tempoMax = 60;
                            }

                            log = "timer 03";
                            DateTime timePagamentoMin = tabuleiroUsuario.DataInicio.AddMinutes(tempoMin);

                            ViewBag.TimerPagamento = false;
                            if (timePagamentoMin > DateTime.Now)
                            {
                                log = "timer 04";
                                //Format: '03/30/2024 17:59:00'
                                ViewBag.Timer = timePagamentoMin.ToString("MM/dd/yyyy HH:mm:ss");
                                ViewBag.ShowReportPayment = true;
                                ViewBag.TimerPagamento = true;
                                ViewBag.ShowExitBoard = true;
                            }

                            log = "timer 05";
                            //Convidado tem até 1h para pagar
                            DateTime timePagamentoMax = tabuleiroUsuario.DataInicio.AddMinutes(tempoMax);
                            if (timePagamentoMax > DateTime.Now)
                            {
                                log = "timer 06";
                                ViewBag.ShowReportPayment = true;
                            }
                        }
                    }

                    log = "galaxy 11";
                    //Obtem o tabuleiro que será exibido quando a pag for carregada
                    tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro, usuario.ID);
                    ViewBag.tabuleiro = tabuleiro;
                    ViewBag.tabuleiroName = tabuleiroUsuario.BoardNome.Substring(0, 3).ToUpper() + "-" + tabuleiro.ID.ToString("000000");

                    log = "galaxy 12";
                    //Verifica se usuario tem que pagar ou não o sistema
                    if (usuario.ID == tabuleiro.Master && !tabuleiroUsuario.PagoSistema)
                    {
                        ViewBag.Pagar = true;
                    }
                    else
                    {
                        ViewBag.Pagar = false;
                    }

                    log = "galaxy 13";
                    if (usuario.ID == tabuleiro.Master && !tabuleiroUsuario.InformePagSistema)
                    {
                        ViewBag.PagarInforme = true;
                    }
                    else
                    {
                        ViewBag.PagarInforme = false;
                        if (usuario.ID == tabuleiro.Master && !tabuleiroUsuario.PagoSistema && tabuleiro.StatusID == 2)
                        {
                            ViewBag.InfoUsuario = traducaoHelper["NOOK_PAGTO_SISTEMA_AGUAR_ADMIN"];
                        }
                    }
                }
                else
                {
                    //Novo usuario, pega tabuleiro do seu pai
                    log = "new user 01";
                    int idPai = usuario.PatrocinadorDiretoID ?? 0;
                    bool tab10Disponiveis = false;
                    if (idPai > 0)
                    {
                        log = "new user 02";
                        tabuleirosUsuario = tabuleiroRepository.ObtemTabuleirosUsuario(idPai);
                        tabuleiroUsuario = tabuleirosUsuario.FirstOrDefault();

                        //Verifica se pai ainda esta no Mercurio
                        if (tabuleiroUsuario != null && tabuleiroUsuario.BoardID == 1 && tabuleiroUsuario.TabuleiroID != null)
                        {
                            log = "new user 03";
                            idTabuleiro = tabuleiroUsuario.TabuleiroID ?? 0;
                            //Ok carrega tabuleiro
                            if (idTabuleiro > 0)
                            {
                                log = "new user 04";
                                tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro, idPai);

                                log = "new user 07";
                                ViewBag.idTabuleiro = idTabuleiro;
                                ViewBag.tabuleiro = tabuleiro;
                                ViewBag.tabuleiroAtivo = tabuleiroUsuario;
                                ViewBag.NovoUsuario = true;
                                ViewBag.tabuleiroName = tabuleiroUsuario.BoardNome.Substring(0, 3).ToUpper() + "-" + tabuleiro.ID.ToString("000000");

                                //Verifica se há vagas no tabuleiro escolhido
                                if (
                                    tabuleiro.DonatorDirSup1 != null &&
                                    tabuleiro.DonatorDirInf1 != null &&
                                    tabuleiro.DonatorDirSup2 != null &&
                                    tabuleiro.DonatorDirInf2 != null &&
                                    tabuleiro.DonatorEsqSup1 != null &&
                                    tabuleiro.DonatorEsqInf1 != null &&
                                    tabuleiro.DonatorEsqSup2 != null &&
                                    tabuleiro.DonatorEsqInf2 != null
                                   )
                                {
                                    log = "new user 08";
                                    //Não havendo vagas obtem o mais antigo tabuleiro disponivel
                                    tab10Disponiveis = true;
                                }
                            }
                        }
                        else
                        {
                            log = "new user 09";
                            tab10Disponiveis = true;
                        }
                        if (tab10Disponiveis)
                        {
                            log = "new user 10";
                            //Obtem os 10 primeiros tabuleiros ativos
                            tabuleirosUsuario = tabuleiroRepository.ObtemTabuleirosUsuario(null);
                            tabuleiroUsuario = tabuleirosUsuario.FirstOrDefault();

                            log = "new user 11";
                            //Verifica se ainda esta no Mercurio
                            if (tabuleiroUsuario != null && tabuleiroUsuario.BoardID == 1)
                            {
                                log = "new user 12";
                                idTabuleiro = tabuleiroUsuario.TabuleiroID ?? 0;
                                //Ok carrega tabuleiro
                                if (idTabuleiro > 0)
                                {
                                    log = "new user 13";
                                    tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro, idPai);
                                    ViewBag.idTabuleiro = idTabuleiro;
                                    ViewBag.tabuleiro = tabuleiro;
                                    ViewBag.tabuleiroAtivo = tabuleiroUsuario;
                                    ViewBag.NovoUsuario = true;
                                    ViewBag.tabuleiroName = tabuleiroUsuario.BoardNome.Substring(0, 3).ToUpper() + "-" + tabuleiro.ID.ToString("000000");
                                }
                            }
                            else
                            {
                                log = "new user 14";
                                //Obtem tabuleiro com primeira posição disponivel
                                //Nao deve entrar aqui
                                ViewBag.idTabuleiro = null;
                                ViewBag.tabuleiro = null;
                                ViewBag.tabuleiroAtivo = null;
                            }
                        }
                    }

                    if (blnTabuleiroAtivos)
                    {
                        log = "Galaxy 14";
                        ViewBag.NovoUsuario = false;
                        ViewBag.TabuleiroOpacity = true;
                    }
                }

                //Verifica se usuario é o master do sistema
                if (tabuleiroUsuario.MasterID == usuario.ID)
                {
                    log = "Galaxy 15";
                    //Sendo o master verifica se ele esta ok com as regras
                    string ret = tabuleiroRepository.MasterRuleOK(usuario.ID, tabuleiroUsuario.BoardID);

                    switch (ret)
                    {
                        case "OK":
                            ViewBag.ShowMsgFaltaPag = "";
                            break;
                        case "NOOK_PAGTO_SISTEMA":
                            ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_PAGTO_SISTEMA"];
                            break;
                        case "NOOK_SEM_INDICACAO":
                            ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_SEM_INDICACAO"];
                            break;
                        case "NOOK_PAGTO_SISTEMA_SEM_INDICACAO":
                            ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_PAGTO_SISTEMA"] + " - " + traducaoHelper["NOOK_SEM_INDICACAO"];
                            break;
                        case "NOOK_PAGTO_SISTEMA_INFORME_OK":
                            ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_PAGTO_SISTEMA_AGUAR_ADMIN"];
                            break;
                        case "NOOK_BOARD_SUPERIOR":
                            ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_BOARD_SUPERIOR"];
                            break;
                        default:
                            ViewBag.ShowMsgFaltaPag = traducaoHelper[ret];
                            break;
                    }
                }
                log = "Galaxy 16";
                string check = tabuleiroRepository.UsuarioRuleOK(usuario.ID);

                switch (check)
                {
                    case "OK":
                        ViewBag.InfoUsuario = ViewBag.InfoUsuario;
                        break;
                    case "NOOK_PAGTO_SISTEMA":
                        ViewBag.InfoUsuario = ViewBag.InfoUsuario + " " + traducaoHelper["NOOK_PAGTO_SISTEMA"];
                        break;
                    case "NOOK_SEM_INDICACAO":
                        ViewBag.InfoUsuario = ViewBag.InfoUsuario + " " + traducaoHelper["NOOK_SEM_INDICACAO"];
                        break;
                    case "NOOK_PAGTO_SISTEMA_SEM_INDICACAO":
                        ViewBag.InfoUsuario = ViewBag.InfoUsuario + " " + traducaoHelper["NOOK_PAGTO_SISTEMA"] + " - " + traducaoHelper["NOOK_SEM_INDICACAO"];
                        break;
                    default:
                        ViewBag.InfoUsuario = ViewBag.InfoUsuario + " " + traducaoHelper[check];
                        break;
                }

                if (blnConviteAtivo)
                {
                    log = "invitation timer 01";
                    //Timer para entrar no convite de uma galaxia
                    int tempoMin = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_PAGAMENTO");
                    int tempoMax = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_MAX_PAGAMENTO");

                    if (tempoMin == 0)
                    {
                        tempoMin = 15;
                    }
                    if (tempoMax == 0)
                    {
                        tempoMax = 60;
                    }

                    log = "invitation timer 02";
                    ViewBag.TimerConvite = false;
                    DateTime dataCheck = DateTime.Now;

                    log = "invitation timer 03";
                    TabuleiroUsuarioModel tabuleiroConvite = tabuleirosUsuario.Where(x => x.StatusID == 2).FirstOrDefault();
                    if (tabuleiroConvite != null)
                    {
                        log = "invitation timer 04";
                        DateTime timePagamentoMin = tabuleiroConvite.DataInicio.AddMinutes(tempoMin);
                        ViewBag.TimerConvite = false;
                        if (timePagamentoMin > dataCheck)
                        {
                            log = "invitation timer 05";
                            //Format: '03/30/2024 17:59:00'
                            ViewBag.Timer = timePagamentoMin.ToString("MM/dd/yyyy HH:mm:ss");
                            ViewBag.TimerConvite = true;
                        }
                    }
                }

                log = "invitation timer 06";
                ViewBag.TotalRecebimento = tabuleiroUsuario.TotalRecebimento;
            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, 0, idTabuleiro, "Galaxy", "back", log + "|" + ex.Message);
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GAL_01", log, "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return RedirectToAction("Index", "Home", new { strPopupTitle = "Erro", strPopupMessage = ex.Message, Sair = "true" });
            }

            return View();
        }

        [HttpPost]
        public ActionResult GetData(string usuarioID, string targetID, string tabuleiroID, string nivel, string token)
        {
            string log = "parameter 01";
            if (usuarioID.IsNullOrEmpty() || targetID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || nivel.IsNullOrEmpty())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            log = "parameter parse 01";
            //Usuario Logado
            int idUsuario = 0;
            log = "parameter parse 02";
            //Usuario no qual se quer as informações
            int idTarget = 0;
            log = "parameter parse 03";
            //Tabuleiro que o usuario que se deseja informações esta
            int idTabuleiro = 0;
            int idBoard = 0;

            try
            {
                log = "parameter parse 01";
                //Usuario Logado
                idUsuario = int.Parse(usuarioID);
                log = "parameter parse 02";
                //Usuario no qual se quer as informações
                idTarget = int.Parse(targetID);
                log = "parameter parse 03";
                //Tabuleiro que o usuario que se deseja informações esta
                idTabuleiro = int.Parse(tabuleiroID);

                log = "parameter parse 04";
                if (idUsuario <= 0 || idTarget <= 0 || idTabuleiro <= 0)
                {
                    log = "parameter parse 05";
                    string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                log = "token 01";
                string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                log = "token 02";
                string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");
                log = "token 03";
                tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                if (token != tokenLocal)
                {
                    log = "token 04";
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                log = "user 01";
                if (idUsuario == usuario.ID)
                {
                    log = "user 02";
                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);
                    log = "user 03";
                    if (idBoard == 0)
                    {
                        log = "user 04";
                        JsonResult jsonResult0 = new JsonResult
                        {
                            Data = "NoValue",
                            RecursionLimit = 1000
                        };

                        log = "user 05";
                        return jsonResult0;
                    }

                    log = "galaxy 01";
                    //Obtem usuario target
                    TabuleiroInfoUsuarioModel obtemInfoUsuario = tabuleiroRepository.ObtemInfoUsuario(idTarget, idUsuario, idBoard);

                    log = "galaxy 02";
                    //Obtem os dados do tabuleiro do usuario que se quer informações
                    TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idTarget, idBoard);

                    log = "user info 01";
                    if (obtemInfoUsuario != null)
                    {
                        log = "user info 02";
                        //Se o target for o master
                        if (tabuleiroUsuario.MasterID == idTarget)
                        {
                            log = "user info 03";
                            //Verifica se Master Esta ok com as regras, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterRuleOK(idTarget, idBoard) != "OK")
                            {
                                log = "user info 04";
                                //Não estando ok, a conta do sistema é exibida para pagamento
                                obtemInfoUsuario = tabuleiroRepository.ObtemInfoSystem();
                            }
                            log = "user info 05";
                            //Verifica se Master Esta ok com as regras de indicados, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterIndicadosOK(idTarget, idBoard) != "OK")
                            {
                                log = "user info 06";
                                //Não estando ok, a conta do sistema é exibida para pagamento
                                obtemInfoUsuario = tabuleiroRepository.ObtemInfoSystem();
                            }
                        }
                        log = "user info 07";
                        
                        //Em produção teve essa mudança: não exibe pix para usuario comum, somente para o sistema
                        if(ConfiguracaoHelper.GetBoolean("TABULEIRO_EXIBE_PIX"))
                        {
                            obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                        }
                        else
                        {
                            if(obtemInfoUsuario.Nome == "System")
                            {
                                obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                            } else
                            {
                                obtemInfoUsuario.Pix = "";
                            }
                        }

                        obtemInfoUsuario.Carteira = CriptografiaHelper.Morpho(obtemInfoUsuario.Carteira, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    }
                    else
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02", log, traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                        Mensagem(traducaoHelper["ERRO"], strMensagemParam3, "err");
                        //Não há dados para ser exibido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02");
                    }

                    log = "galaxy 03";

                    //Se o UsuarioLogado é o Master,
                    //caso o master não tenha recebido (PagoMaster = false)
                    //Caso Convidado tenha informado que efetuou o pagamento (InformePag = true)
                    //O Master pode ter a opção de confirmação de recebimento
                    if (tabuleiroUsuario.MasterID == usuario.ID && !tabuleiroUsuario.PagoMaster && tabuleiroUsuario.InformePag)
                    {
                        log = "timer 01";
                        int tempoMin = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_PAGAMENTO");
                        int tempoMax = ConfiguracaoHelper.GetInt("TABULEIRO_TEMPO_MAX_PAGAMENTO");

                        if (tempoMin == 0)
                        {
                            tempoMin = 15;
                        }
                        if (tempoMax == 0)
                        {
                            tempoMax = 60;
                        }

                        log = "timer 02";
                        //Soma os dois para dar o tempo que o master pode confirmar o pagamento
                        DateTime timePagamento = tabuleiroUsuario.DataInicio.AddMinutes(tempoMin);
                        timePagamento = tabuleiroUsuario.DataInicio.AddMinutes(tempoMax);

                        log = "timer 03";
                        obtemInfoUsuario.ConfirmarRecebimento = true;
                    }

                    log = "user info 07";
                    obtemInfoUsuario.Observacao = "";

                    log = "user info 08";
                    //Verifica se usuario é o master do sistema
                    if (tabuleiroUsuario.MasterID == usuario.ID)
                    {
                        log = "target 01";
                        //Sendo o master verifica se ele esta ok com as regras
                        string check = tabuleiroRepository.MasterRuleOK(usuario.ID, tabuleiroUsuario.BoardID);

                        log = "target 02";
                        switch (check)
                        {
                            case "OK":
                                obtemInfoUsuario.Observacao = "";
                                break;
                            case "NOOK_PAGTO_SISTEMA":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_PAGTO_SISTEMA_2"];
                                break;
                            case "NOOK_SEM_INDICACAO":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_SEM_INDICACAO_2"];
                                break;
                            case "NOOK_PAGTO_SISTEMA_SEM_INDICACAO":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_PAGTO_SISTEMA_2"] + " - " + traducaoHelper["NOOK_SEM_INDICACAO_2"];
                                break;
                            case "NOOK_PAGTO_SISTEMA_INFORME_OK":
                                ViewBag.ShowMsgFaltaPag = traducaoHelper["NOOK_PAGTO_SISTEMA_AGUAR_ADMIN"];
                                break;
                            case "NOOK_PAGTO_SISTEMA_CONVITE":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_PAGTO_SISTEMA_2"] + " - " + traducaoHelper["NOOK_CONVITE"];
                                break;
                            case "NOOK_SEM_INDICACAO_CONVITE":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_SEM_INDICACAO_2"] + " - " + traducaoHelper["NOOK_CONVITE"];
                                break;
                            case "NOOK_PAGTO_SISTEMA_SEM_INDICACAO_CONVITE":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_PAGTO_SISTEMA_2"] + " - " + traducaoHelper["NOOK_SEM_INDICACAO_2"] + " - " + traducaoHelper["NOOK_CONVITE"];
                                break;
                            case "NOOK_PAGTO_SISTEMA_INFORME_OK_CONVITE":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_PAGTO_SISTEMA_AGUAR_ADMIN"] + " - " + traducaoHelper["NOOK_CONVITE"];
                                break;
                            case "NOOK_CONVITE":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_CONVITE"];
                                break;
                            case "NOOK_BOARD_SUPERIOR":
                                obtemInfoUsuario.Observacao = traducaoHelper["NOOK_BOARD_SUPERIOR"];
                                break;
                            default:
                                obtemInfoUsuario.Observacao = traducaoHelper[check];
                                break;
                        }
                    }

                    log = "target 03";
                    JsonResult jsonResult = new JsonResult
                    {
                        Data = obtemInfoUsuario,
                        RecursionLimit = 1000
                    };

                    log = "target 04";
                    return jsonResult;
                }

                string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "GetData", "back", log + "|" + ex.Message);
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_01", log, "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_01 " + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult GetDataSysPag(string usuarioID, string tabuleiroID, string token)
        {
            int idUsuario = 0;
            int idTabuleiro = 0;
            int idBoard = 0;

            try
            {
                if (usuarioID.IsNullOrEmpty() || usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                //Usuario Logado
                idUsuario = int.Parse(usuarioID);
                idTabuleiro = int.Parse(tabuleiroID);

                if (idUsuario <= 0 || idTabuleiro <= 0)
                {
                    string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                if (token != tokenLocal)
                {
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                //Obtem usuario do systema 
                TabuleiroInfoUsuarioModel obtemInfoUsuario = tabuleiroRepository.ObtemInfoSysPag();

                if (obtemInfoUsuario != null)
                {
                    obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    obtemInfoUsuario.Carteira = CriptografiaHelper.Morpho(obtemInfoUsuario.Carteira, CriptografiaHelper.TipoCriptografia.Descriptografa);

                }
                else
                {
                    string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                    Mensagem(traducaoHelper["ERRO"], strMensagemParam3, "err");
                    //Não há dados para ser exibido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02");
                }

                idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);

                //Obtem os dados do tabuleiro do usuario que se quer informações
                TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(usuario.ID, idBoard);

                string msgValor = "";
                if (tabuleiroUsuario.Ciclo > 1)
                {
                    switch (idBoard)
                    {
                        case 1:
                            msgValor = traducaoHelper["TAXA_VALOR_MERCURIO_2"];
                            break;
                        case 2:
                            msgValor = traducaoHelper["TAXA_VALOR_SATURNO_2"];
                            break;
                        case 3:
                            msgValor = traducaoHelper["TAXA_VALOR_MARTE_2"];
                            break;
                        case 4:
                            msgValor = traducaoHelper["TAXA_VALOR_JUPITER_2"];
                            break;
                        case 5:
                            msgValor = traducaoHelper["TAXA_VALOR_VENUS_2"];
                            break;
                        case 6:
                            msgValor = traducaoHelper["TAXA_VALOR_URANO_2"];
                            break;
                        case 7:
                            msgValor = traducaoHelper["TAXA_VALOR_TERRA_2"];
                            break;
                        case 8:
                            msgValor = traducaoHelper["TAXA_VALOR_SOL_2"];
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (idBoard)
                    {
                        case 1:
                            msgValor = traducaoHelper["TAXA_VALOR_MERCURIO_1"];
                            break;
                        case 2:
                            msgValor = traducaoHelper["TAXA_VALOR_SATURNO_1"];
                            break;
                        case 3:
                            msgValor = traducaoHelper["TAXA_VALOR_MARTE_1"];
                            break;
                        case 4:
                            msgValor = traducaoHelper["TAXA_VALOR_JUPITER_1"];
                            break;
                        case 5:
                            msgValor = traducaoHelper["TAXA_VALOR_VENUS_1"];
                            break;
                        case 6:
                            msgValor = traducaoHelper["TAXA_VALOR_URANO_1"];
                            break;
                        case 7:
                            msgValor = traducaoHelper["TAXA_VALOR_TERRA_1"];
                            break;
                        case 8:
                            msgValor = traducaoHelper["TAXA_VALOR_SOL_1"];
                            break;
                        default:
                            break;
                    }
                }
                obtemInfoUsuario.Valor = msgValor;

                JsonResult jsonResult = new JsonResult
                {
                    Data = obtemInfoUsuario,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, 0, idTabuleiro, "GetDataSysPag", "back", ex.Message);
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDS_01", "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDSP_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult GetDataSystem(string token)
        {
            try
            {
                string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                if (token != tokenLocal)
                {
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                //Obtem usuario do systema 
                Core.Models.TabuleiroInfoUsuarioModel obtemInfoUsuario = tabuleiroRepository.ObtemInfoSystem();

                if (obtemInfoUsuario != null)
                {
                    obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    obtemInfoUsuario.Carteira = CriptografiaHelper.Morpho(obtemInfoUsuario.Carteira, CriptografiaHelper.TipoCriptografia.Descriptografa);

                }
                else
                {
                    string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                    Mensagem(traducaoHelper["ERRO"], strMensagemParam3, "err");
                    //Não há dados para ser exibido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02");
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = obtemInfoUsuario,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, 0, 0, "GetDataSystem", "back", ex.Message);
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDS_01", "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDSP_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult GetTabuleiro(string usuarioID, string tabuleiroID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = 0;
            int idTabuleiro = 0;

            try
            {
                idUsuario = int.Parse(usuarioID);
                idTabuleiro = int.Parse(tabuleiroID);

                if (idUsuario <= 0 || idTabuleiro <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                if (token != tokenLocal)
                {
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                if (idUsuario != usuario.ID)
                {
                    string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                //Obtem usuario target
                Core.Models.TabuleiroModel tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro, usuario.ID);

                if (tabuleiro == null)
                {
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_02", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                    //Não há dados para ser exibido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_02");
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = tabuleiro,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, 0, idTabuleiro, "GetTabuleiro", "back", ex.Message);
                string[] strMensagemParam5 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01", "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam5, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult GetInvite(string usuarioID, string boardID, string token)
        {
            string log = "";

            if (usuarioID.IsNullOrEmpty() || boardID.IsNullOrEmpty())
            {
                log = "parameter 01";
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = 0;
            int idBoard = 0;
            string tabuleiroIncluir = "OK";
            int idTabuleiro = 0;


            try
            {
                idUsuario = int.Parse(usuarioID);
                idBoard = int.Parse(boardID);
                tabuleiroIncluir = "OK";
                idTabuleiro = 0;

                if (idUsuario <= 0)
                {
                    log = "Invalid parameter parse user 02";
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "GetInvite");

                if (getUltimoAcesso)
                {
                    //Seta para primeiro tabuleiro caso seja 0
                    if (idBoard == 0)
                    {
                        idBoard = 1;
                    }

                    if (idUsuario <= 0 || idBoard <= 0)
                    {
                        log = "parameter 02";
                        string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    log = "tocken 01";
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        log = "token 02";
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        log = "user 01";
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    log = "sponsor 01";
                    //Caso não exista o patrocionador usa o usuario 2580 que é o primeiro alvo Veja a tabela usuario.usuario
                    int patrocinadoID = usuario.PatrocinadorDiretoID ?? 2580; //2580 é o primeiro alvo

                    log = "user rule 01";

                    string userRule = tabuleiroRepository.UsuarioRuleOK(usuario.ID);

                    switch (userRule)
                    {
                        case "OK":
                            tabuleiroIncluir = "OK";
                            break;
                        case "NOOK_PAGTO_SISTEMA":
                            tabuleiroIncluir = traducaoHelper["PARA_ENTRAR_GALAXIA"] + " " + traducaoHelper["NOOK_PAGTO_SISTEMA"];
                            break;
                        case "NOOK_SEM_INDICACAO":
                            tabuleiroIncluir = traducaoHelper["PARA_ENTRAR_GALAXIA"] + " " + traducaoHelper["NOOK_SEM_INDICACAO"];
                            break;
                        case "NOOK_PAGTO_SISTEMA_SEM_INDICACAO":
                            tabuleiroIncluir = traducaoHelper["PARA_ENTRAR_GALAXIA"] + " " + traducaoHelper["NOOK_PAGTO_SISTEMA"] + " - " + traducaoHelper["NOOK_SEM_INDICACAO"];
                            break;
                        default:
                            tabuleiroIncluir = traducaoHelper[userRule];
                            break;
                    }

                    if (userRule == "OK")
                    {
                        log = "board 01: usuarioID:" + usuario.ID + " BoardID:" + idBoard + " Convite";
                        //Inclui usuario no novo tabuleiro
                        tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiro(usuario.ID, patrocinadoID, idBoard, "Convite");
                        log = "board 02";
                        TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuario, idBoard);
                        log = "board 03";
                        idTabuleiro = tabuleiroUsuario.TabuleiroID ?? 0;
                    }

                    log = "json 01";
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = idTabuleiro.ToString(),
                    RecursionLimit = 1000
                };

                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "GetInvite");

                log = "result";
                return jsonResult;
            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "getInvite", "back", ex.Message);

                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GI_01", "[" + ex.Message + "]", log, traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GI_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult GetInviteNew(string usuarioID, string token)
        {
            string log = "Start";

            if (usuarioID.IsNullOrEmpty())
            {
                log = "Invalid parameter user";
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = 0;

            try
            {
                log = "Invalid parameter parse user 01";
                idUsuario = int.Parse(usuarioID);
                string tabuleiroIncluir = "OK";

                if (idUsuario <= 0)
                {
                    log = "Invalid parameter parse user 02";
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "GetInviteNew");
                if (getUltimoAcesso)
                {
                    log = "Invalid token 01";
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    log = "Invalid token 02";
                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        log = "Invalid token 03";
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        log = "Invalid token 04";
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        log = "Invalid user 01";
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    log = "Invalid sponsor";
                    //Caso não exista o patrocionador usa o usuario 2580 que é o primeiro alvo Veja a tabela usuario.usuario
                    int patrocinadoID = usuario.PatrocinadorDiretoID ?? 2580; //2580 é o primeiro alvo

                    log = "Invalid include new";
                    //Inclui usuario no novo tabuleiro
                    tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiroNew(usuario.ID, patrocinadoID);
                }

                log = "Invalid result 01";

                JsonResult jsonResult = new JsonResult
                {
                    Data = traducaoHelper[tabuleiroIncluir],
                    RecursionLimit = 1000
                };

                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "GetInviteNew");

                log = "Invalid result 02";
                return jsonResult;
            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, 0, 0, "GetInviteNew", "back", ex.Message);
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GIN_01", log, "[" + ex.Message + "]", traducaoHelper["FAVOR_CONTATAR_SUPORTE"] };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GIN_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult ReportPayment(string usuarioID, string tabuleiroID, string usuarioIDPag, string token)
        {
            //Confirmar Pagamento de um usuario para o alvo
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || usuarioIDPag.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string retorno = "";

            int idUsuario = 0;
            int idTabuleiro = 0;
            int idUsuarioPag = 0;
            int idBoard = 0;
            try
            {
                idUsuario = int.Parse(usuarioID);
                idTabuleiro = int.Parse(tabuleiroID);
                idUsuarioPag = int.Parse(usuarioIDPag);

                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioPag <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "ReportPayment");

                getUltimoAcesso = true; //panda

                if (getUltimoAcesso)
                {
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);

                    //Verifica se é realmente o master do tabuleiro
                    TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuario, idBoard);
                    bool masterIDValido = false;
                    if (tabuleiroUsuario.MasterID == idUsuarioPag || idUsuarioPag == 2000)
                    {
                        masterIDValido = true;
                    }

                    if (masterIDValido)
                    {
                        //Verifica se master esta ok
                        if (idUsuarioPag != 2000)
                        {
                            //Verifica se Master Esta ok com as regras, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterRuleOK(idUsuarioPag, idBoard) != "OK")
                            {
                                //Não estando ok, a conta do sistema é usada para pagamento
                                idUsuarioPag = 2000;
                            }
                            //Verifica se Master Esta ok com as regras de indicados, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterIndicadosOK(idUsuarioPag, idBoard) != "OK")
                            {
                                //Não estando ok, a conta do sistema é usada para pagamento
                                idUsuarioPag = 2000;
                            }
                        }

                        //Informar Pagamento
                        retorno = tabuleiroRepository.InformarPagamento(usuario.ID, idBoard, idUsuarioPag);
                        switch (retorno)
                        {
                            case "OK":
                                string[] strMensagem = new string[] { traducaoHelper["PAGAMENTO_INFORMADO_COM_SUCESSO"], traducaoHelper["ALVO_1H_DAR_ACEITE"] };
                                Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                                break;
                            case "NOOK":
                                string[] strMensagemParam4 = new string[] { traducaoHelper["TEMPO_ESGOTADO"] };
                                Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TEMPO_ESGOTADO"]);
                            default:
                                string[] strMensagemParam5 = new string[] { traducaoHelper["TEMPO_ESGOTADO"] };
                                Mensagem(traducaoHelper["ALERTA"], strMensagemParam5, "ale");
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TEMPO_ESGOTADO"]);
                        }
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_03");
                    }
                }

                if (String.IsNullOrEmpty(retorno))
                {
                    retorno = traducaoHelper["SERVIDOR_OCUPADO"];
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ReportPayment");

                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "ReportPayment", "back", ex.Message);
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01 [" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult ReportReceipt(string usuarioID, string UsuarioConvidadoID, string tabuleiroID, string token)
        {
            string log = "";
            
            //Alvo confirma o recebimento
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || UsuarioConvidadoID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int idUsuario = 0;
            int idUsuarioConvidado = 0;
            int idTabuleiro = 0;
            int idBoard = 0;

            try
            {
                idUsuario = int.Parse(usuarioID);
                idUsuarioConvidado = int.Parse(UsuarioConvidadoID);
                idTabuleiro = int.Parse(tabuleiroID);

                string retorno = "";
                log = "Inicio";
                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioConvidado <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }
                log = "getUltimoAcesso";
                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "ReportReceipt");

                getUltimoAcesso = true;

                if (getUltimoAcesso)
                {
                    log = "getUltimoAcesso=true";
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);
                    log = "Token";
                    if (token != tokenLocal)
                    {
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }
                    log = "boardID";
                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);
                    int idUsuarioPag = tabuleiroRepository.ObtemUsuarioIDPag(idUsuarioConvidado, idBoard);
                    log = "tabuleiroUsuarioCheck";
                    //Verifica se é realmente o master do tabuleiro
                    TabuleiroUsuarioModel tabuleiroUsuarioCheck = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuario, idBoard);
                    log = "idUsuarioPag";
                    if (idUsuarioPag == 0)
                    {
                        idUsuarioPag = tabuleiroUsuarioCheck.MasterID;
                    }

                    bool masterIDValido = false;
                    if (tabuleiroUsuarioCheck.MasterID == idUsuarioPag || idUsuarioPag == 2000)
                    {
                        masterIDValido = true;
                    }
                    log = "masterIDValido";
                    //Informar Recebimento
                    if (masterIDValido)
                    {
                        log = "masterIDValido=true";
                        //Verifica se master esta ok
                        if (idUsuarioPag != 2000)
                        {
                            //Verifica se Master Esta ok com as regras, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterRuleOK(idUsuarioPag, idBoard) != "OK")
                            {
                                log = "idUsuarioPag=2000";
                                //Não estando ok, a conta do sistema é usada para pagamento
                                idUsuarioPag = 2000;
                            }
                            //Verifica se Master Esta ok com as regras de indicados, para que sua conta seja exibida
                            if (tabuleiroRepository.MasterIndicadosOK(idUsuarioPag, idBoard) != "OK")
                            {
                                log = "idUsuarioPag=2000";
                                //Não estando ok, a conta do sistema é usada para pagamento
                                idUsuarioPag = 2000;
                            }
                        }
                        log = "retMaster";
                        string retMaster = tabuleiroRepository.MasterRuleOK(usuario.ID, idBoard);
                        log = "retIndicados";
                        string retIndicados = tabuleiroRepository.MasterIndicadosOK(usuario.ID, idBoard);

                        if (retMaster == "OK" && retIndicados == "OK")
                        {
                            log = "retMaster retIndicados = OK";
                            retorno = tabuleiroRepository.InformarRecebimento(idUsuarioConvidado, idUsuarioPag, idBoard);
                            if (retorno == "OK")
                            {
                                log = "InformarRecebimento: " + retorno;
                                switch (retorno)
                                {
                                    case "OK":
                                        log = "lancamentos";
                                        TabuleiroBoardModel tabuleiroBoard = tabuleiroRepository.ObtemTabuleiroBoard(idTabuleiro);
                                        Usuario usuarioConvidado = usuarioRepository.Get(idUsuarioConvidado);
                                        TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuarioConvidado, idBoard);

                                        //Efetuar Credito no Master
                                        var lancamento = new Lancamento();
                                        lancamento.UsuarioID = idUsuarioPag;
                                        lancamento.Tipo = Lancamento.Tipos.Credito;
                                        lancamento.ReferenciaID = lancamento.UsuarioID;
                                        lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper[tabuleiroBoard.Nome].ToLower(), " - ", usuarioConvidado.Apelido.ToLower());
                                        lancamento.DataLancamento = App.DateTimeZion;
                                        lancamento.DataCriacao = App.DateTimeZion;
                                        lancamento.ContaID = 7; //Transferencia
                                        lancamento.CategoriaID = 7; //Transferencia
                                        lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                                        lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Transferencia);
                                        lancamentoRepository.Save(lancamento);

                                        //Efetuar Debito no Convidado
                                        lancamento = new Lancamento();
                                        lancamento.UsuarioID = idUsuarioConvidado;
                                        lancamento.Tipo = Lancamento.Tipos.Debito;
                                        lancamento.ReferenciaID = lancamento.UsuarioID;
                                        lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper[tabuleiroBoard.Nome].ToLower(), " - ", usuario.Apelido.ToLower());
                                        lancamento.DataLancamento = App.DateTimeZion;
                                        lancamento.DataCriacao = App.DateTimeZion;
                                        lancamento.ContaID = 7; //Transferencia
                                        lancamento.CategoriaID = 7; //Transferencia
                                        lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                                        lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Transferencia);
                                        lancamentoRepository.Save(lancamento);

                                        //Chama incluir no tabuleiro para ver se 
                                        //o tabuleiro esta completo
                                        log = "IncluiTabuleiro Completo";
                                        string tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiro(idUsuarioConvidado, idUsuario, tabuleiroBoard.ID, "Completa");

                                        log = "IncluiTabuleiro: " + tabuleiroIncluir;
                                        if (tabuleiroIncluir == "COMPLETO")
                                        {
                                            string[] strMensagem = new string[] { traducaoHelper["RECEBIMENTO_CONFIMADO_COM_SUCESSO"], traducaoHelper["MENSAGEM_TABULEIRO_COMPLETO_1"], traducaoHelper["MENSAGEM_TABULEIRO_COMPLETO_2"] };
                                            Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                                            retorno = "COMPLETO";
                                        }
                                        else
                                        {
                                            string[] strMensagem = new string[] { traducaoHelper["RECEBIMENTO_CONFIMADO_COM_SUCESSO"] };
                                            Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                                        }

                                        break;
                                    case "NOOK":
                                        string[] strMensagemParam4 = new string[] { traducaoHelper["RECEBIMENTO_NAO_CONFIMADO"] };
                                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["RECEBIMENTO_NAO_CONFIMADO"]);
                                    default:
                                        string[] strMensagemParam5 = new string[] { traducaoHelper["RECEBIMENTO_NAO_CONFIMADO"] };
                                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam5, "ale");
                                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["RECEBIMENTO_NAO_CONFIMADO"]);
                                }
                            }
                        }
                        else
                        {
                            log = "NAO_FOI_POSSIVEL_REMOVIDO_CONVIDADO";
                            retorno = traducaoHelper["NAO_FOI_POSSIVEL_REMOVIDO_CONVIDADO"];
                            if (retMaster != "OK")
                            {
                                retorno = retorno + " - " + traducaoHelper[retMaster];
                            }
                            if (retIndicados != "OK")
                            {
                                retorno = retorno + " - " + traducaoHelper["NOOK_SEM_INDICACAO"];
                            }
                        }

                        string tabuleiroFechado = tabuleiroRepository.TabuleiroFechado(idTabuleiro);

                        if (tabuleiroFechado == "ambos")
                        {
                            retorno = "COMPLETO";
                        }
                        
                        JsonResult jsonResult = new JsonResult
                        {
                            Data = retorno,
                            RecursionLimit = 1000
                        };
                        string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ReportReceipt");

                        return jsonResult;
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RR_01");
                    }
                }
                else
                {
                    log = log + "|getUltimoAcesso=false";
                    JsonResult jsonResult = new JsonResult
                    {
                        Data = traducaoHelper["SERVIDOR_OCUPADO"],
                        RecursionLimit = 1000
                    };
                    string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ReportReceipt");
                    return jsonResult;
                }
            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "ReportReceipt", "back", ex.Message);
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01 [" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RR_02" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(string usuarioID, string UsuarioConvidadoID, string tabuleiroID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || UsuarioConvidadoID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int idUsuario = 0;
            int idUsuarioConvidado = 0;
            int idTabuleiro = 0;
            int idBoard = 0;
            try
            {
                idUsuario = int.Parse(usuarioID);
                idUsuarioConvidado = int.Parse(UsuarioConvidadoID);
                idTabuleiro = int.Parse(tabuleiroID);

                string retorno = "";

                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioConvidado <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "ReportReceipt");
                if (getUltimoAcesso)
                {
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);

                    string retMaster = tabuleiroRepository.MasterRuleOK(usuario.ID, idBoard);
                    string retIndicados = tabuleiroRepository.MasterIndicadosOK(usuario.ID, idBoard);

                    if (retMaster == "OK" && retIndicados == "OK")
                    {
                        //Informar Recebimento
                        retorno = tabuleiroRepository.RemoverUsuario(idUsuarioConvidado, idUsuario, idBoard);
                        switch (retorno)
                        {
                            case "OK":
                                string[] strMensagem = new string[] { traducaoHelper["CONVIDADO_REMOVIDO_SUCESSO"] };
                                Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                                break;
                            default:
                                string[] strMensagemParam5 = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_REMOVIDO_CONVIDADO"] };
                                Mensagem(traducaoHelper["ALERTA"], strMensagemParam5, "ale");
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["NAO_FOI_POSSIVEL_REMOVIDO_CONVIDADO"]);
                        }
                    }
                    else
                    {
                        retorno = traducaoHelper["NAO_FOI_POSSIVEL_REMOVIDO_CONVIDADO"];

                        if (retMaster != "OK")
                        {
                            retorno = retorno + " - " + traducaoHelper[retMaster];
                        }
                        if (retIndicados != "OK")
                        {
                            retorno = retorno + " - " + traducaoHelper["NOOK_SEM_INDICACAO"];
                        }
                    }
                }
                if (String.IsNullOrEmpty(retorno))
                {
                    retorno = traducaoHelper["SERVIDOR_OCUPADO"];
                }
                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };
                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ReportReceipt");
                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "DeleteUser", "back", ex.Message);
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01 [" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult ReportPaymentSystem(string usuarioID, string tabuleiroID, string token)
        {
            //InformarPagtoSistema
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int idUsuario = 0;
            int idTabuleiro = 0;
            int idBoard = 0;

            try
            {
                idUsuario = int.Parse(usuarioID);
                idTabuleiro = int.Parse(tabuleiroID);
                string retorno = "";

                if (idUsuario <= 0 || idTabuleiro <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "ReportPaymentSystem");
                if (getUltimoAcesso)
                {
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);

                    //Obtem os dados do tabuleiro do usuario que se quer informações
                    Core.Models.TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuario, idBoard);

                    //Se o UsuarioLogado é o Master,
                    //Somente master para o sistema
                    if (tabuleiroUsuario.MasterID == usuario.ID)
                    {
                        //Informar Pagamento ao sistema
                        retorno = tabuleiroRepository.InformarPagtoSistema(usuario.ID, idBoard);
                        switch (retorno)
                        {
                            case "OK":
                                string[] strMensagem = new string[] { traducaoHelper["PAGAMENTO_SISTEMA_INFORMADO_COM_SUCESSO"] };
                                Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                                break;
                            case "NOOK":
                                string[] strMensagemParam4 = new string[] { traducaoHelper["PAGAMENTO_NAO_CONFIRMADO"] };
                                Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PAGAMENTO_NAO_CONFIRMADO"]);
                            default:
                                string[] strMensagemParam5 = new string[] { traducaoHelper["PAGAMENTO_NAO_CONFIRMADO"] };
                                Mensagem(traducaoHelper["ALERTA"], strMensagemParam5, "ale");
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PAGAMENTO_NAO_CONFIRMADO"]);
                        }
                    }
                    else
                    {
                        string[] strMensagemParam4 = new string[] { traducaoHelper["SOMENTE_ALVO_PODE_EFETUAR_PAGAMENTO_SISTEMA"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["SOMENTE_ALVO_PODE_EFETUAR_PAGAMENTO_SISTEMA"]);
                    }
                }

                if (String.IsNullOrEmpty(retorno))
                {
                    retorno = traducaoHelper["SERVIDOR_OCUPADO"];
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ReportPaymentSystem");
                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "ReportPaymentSystem", "back", ex.Message);
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01 [" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult ExitNivel(string usuarioID, string tabuleiroID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int idUsuario = 0;
            int idTabuleiro = 0;
            int idBoard = 0;
            try
            {
                idUsuario = int.Parse(usuarioID);
                idTabuleiro = int.Parse(tabuleiroID);
                String retorno = "";

                if (idUsuario <= 0 || idTabuleiro <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                bool getUltimoAcesso = tabuleiroRepository.GetUltimoAcesso(idUsuario, "ExitNivel");
                if (getUltimoAcesso)
                {
                    string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                    tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                    if (token != tokenLocal)
                    {
                        string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"], traducaoHelper["RELOAD"] + " " + traducaoHelper["PAGE"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                        //Devolve que tokem é invalido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                    }

                    if (idUsuario != usuario.ID)
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                        Mensagem(traducaoHelper["ALERTA"], strMensagemParam3, "ale");
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                    }

                    idBoard = tabuleiroRepository.ObtemBoardIDByTabuleiroID(idUsuario, idTabuleiro);
                    //Remove usuario do tabuleiro informado
                    retorno = tabuleiroRepository.TabuleiroSair(idUsuario, idBoard);

                    switch (retorno)
                    {
                        case "OK":
                            string[] strMensagem = new string[] { traducaoHelper["SAIDA_REALIZADA_COM_SUCESSO"] };
                            Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
                            break;
                        case "NOOK":
                            string[] strMensagemParam4 = new string[] { traducaoHelper["TEMPO_ESGOTADO"] };
                            Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"]);
                        default:
                            string[] strMensagemParam5 = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"] };
                            Mensagem(traducaoHelper["ALERTA"], strMensagemParam5, "ale");
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["NAO_FOI_POSSIVEL_COMPLETAR_REQUISICAO"]);
                    }
                }

                if (String.IsNullOrEmpty(retorno))
                {
                    retorno = traducaoHelper["SERVIDOR_OCUPADO"];
                }
                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                string setUltimoAcesso = tabuleiroRepository.SetUltimoAcesso(idUsuario, "ExitNivel");

                return jsonResult;

            }
            catch (Exception ex)
            {
                string ilog = tabuleiroRepository.SetLog(usuario.ID, idBoard, idTabuleiro, "ExitNivel", "back", ex.Message);
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01 [" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01" + "[" + ex.Message + "]");
            }
        }

        public ActionResult Indicados(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, string CurrentProcuraGalaxia, string ProcuraGalaxia, int? NumeroPaginas, int? Page)
        {
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref CurrentProcuraGalaxia,
                                    ref ProcuraGalaxia,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "Usuarios");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "login";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraLogin != null || ProcuraGalaxia != null))
            {
                if (ProcuraLogin == null)
                {
                    ProcuraLogin = CurrentProcuraLogin;
                }
                if (ProcuraGalaxia == null)
                {
                    ProcuraGalaxia = CurrentProcuraGalaxia;
                }
            }

            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcuraGalaxia = ProcuraGalaxia;

            IEnumerable<TabuleiroIndicados> lista = tabuleiroRepository.ObtemTabuleirosIndicados(usuario.ID);

            if (!String.IsNullOrEmpty(ProcuraLogin) && !String.IsNullOrEmpty(ProcuraGalaxia))
            {
                lista = lista.Where(x => x.Galaxia.ToLower().Contains(ProcuraGalaxia.ToLower()) && (x.Login.ToLower().Contains(ProcuraLogin.ToLower()) || x.Apelido.ToLower().Contains(ProcuraLogin.ToLower())));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraLogin))
                {
                    lista = lista.Where(x => x.Login.ToLower().Contains(ProcuraLogin.ToLower()) || x.Apelido.ToLower().Contains(ProcuraLogin.ToLower()));
                }
                if (!String.IsNullOrEmpty(ProcuraGalaxia))
                {
                    lista = lista.Where(x => x.Galaxia.ToLower().Contains(ProcuraGalaxia.ToLower()));
                }
            }

            switch (SortOrder)
            {
                case "login_desc":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador";
                    lista = lista.OrderByDescending(x => x.Login);
                    break;
                case "patrocinador":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador_desc";
                    lista = lista.OrderBy(x => x.Master);
                    break;
                case "patrocinador_desc":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador";
                    lista = lista.OrderByDescending(x => x.Master);
                    break;
                case "login":
                    ViewBag.FirstSortParm = "login_desc";
                    ViewBag.SecondSortParm = "date";
                    lista = lista.OrderBy(x => x.Login);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "login_desc";
                    ViewBag.SecondSortParm = "patrocinador";
                    lista = lista.OrderBy(x => x.Login);
                    break;
            }

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 5);

            //Caso seja selecionada toda a lista (-1), pega na verdade 1000
            if (PageSize == -1)
            {
                PageSize = 1000;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.CurrentNumeroPaginas = NumeroPaginas;

            //Pagina corrente
            int PageNumber = (Page ?? 1);

            //DropDown de paginação
            int intNumeroPaginas = (NumeroPaginas ?? 5);

            #region ViewBags
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            ViewBag.Lista = lista;

            #endregion

            return View(lista.ToPagedList(PageNumber, PageSize));
        }

        public ActionResult Pedido() {          
            obtemMensagem();




            return View();
        }   

        #endregion Actions

        #region Json

        #endregion
    }
}
