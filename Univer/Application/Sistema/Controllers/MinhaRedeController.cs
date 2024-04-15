﻿#region Bibliotecas

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sistema.ModelBinders;
using Core.Repositories.Usuario;
using Core.Entities;
using Core.Repositories.Rede;
using Core.Helpers;
using Sistema.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Fluentx;
using System.Globalization;
using Core.Repositories.Financeiro;
using static Core.Entities.Classificacao;
using static Core.Entities.Conta;
using Core.Models;


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

        #region Helpers

        private void GetJsonRede()
        {

            var noRetorno = new NoRedeBinder();

            if (filhosPosicao != null && filhosPosicao.Count() > 0)
            {
                var primeiroNo = filhosPosicao.First();

                Boolean blnArvoreBinaria = false;
                if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
                    blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

                if (blnArvoreBinaria)
                    MontaExibicaoRedeBinaria(primeiroNo, null);
                else
                    MontaExibicaoRede(primeiroNo, null);
            }
            else
            {
                var noVazio = new NoRedeBinder();
                noVazio.id = idNo;
                idNo++;
                noVazio.label = usuario.Login;
                noVazio.title = usuario.Login;
                noVazio.parent = null;
                noVazio.itemTitleColor = "#FFFFFF";
                noVazio.groupTitleColor = "#FFFFFF";
                /*
                noVazio.data = new DataNoRedeBinder()
                {
                    classificacao = usuario.NivelAssociacao.ToString().ToLower(),
                    email = usuario.Email,
                    telefone = usuario.Celular,
                    nome = usuario.Nome,
                    sexo = usuario.Sexo.ToUpper(),
                    observacoes = (!usuario.GeraBonus ? " STANDBY " : "") + (!usuario.RecebeBonus ? " BLOQUEADO " : ""),
                    statusAtivacao = usuario.Status.GetHashCode(),
                    ladoPatrocinador = usuario.EntradaID.ToString()

                };
                 */
                _listaRetorno.Add(noVazio);
            }

        }


        private void MontaExibicaoRedeBinaria(Usuario usuarioNo, int? idParent)
        {
            if (usuarioNo == null) return;

            //var franqueadoNo = franqueadoRepository.GetByCodigoProtheus(noAssinatura.idFranqueadoProtheus, true);

            var filhoEsquerdo = filhosPosicao.SingleOrDefault(n => n.Assinatura.Equals(usuarioNo.Assinatura + "0"));
            var filhoDireito = filhosPosicao.SingleOrDefault(n => n.Assinatura.Equals(usuarioNo.Assinatura + "1"));
            var associacao = _listaAssociacoes.FirstOrDefault(a => a.Nivel == usuarioNo.NivelAssociacao);

            NoRedeBinder noRetorno = new NoRedeBinder();
            noRetorno.id = usuarioNo.ID;
            noRetorno.label = usuarioNo.Login;
            noRetorno.title = usuarioNo.Login;
            noRetorno.parent = idParent;
            noRetorno.description = "<br />Nome: " + usuarioNo.NomeFantasia + "<br />kit: " + associacao.Nome + (usuarioNo.PatrocinadorDireto != null ? "<br />Patrocinador: " + usuarioNo.PatrocinadorDireto.Login : "");
            if (idParent == null)
            {
                noRetorno.phone = "Pontuação E:" + (usuarioNo.UltimaPosicao != null ? usuarioNo.UltimaPosicao.AcumuladoEsquerda.Value.ToString(moedaPadrao.MascaraOut) : "0") +
                                          "  D:" + (usuarioNo.UltimaPosicao != null ? usuarioNo.UltimaPosicao.AcumuladoDireita.Value.ToString(moedaPadrao.MascaraOut) : "0");
            }
            //noRetorno.groupTitleColor = "blue";

            var imagePath = Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_" + usuarioNo.NivelAssociacao);
            if (String.IsNullOrEmpty(imagePath))
                imagePath = "~/Content/img/" + Helpers.Local.Cliente + "/classificacao/1.PNG";

            var color = Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_COLOR_" + usuarioNo.NivelAssociacao);
            if (string.IsNullOrEmpty(color))
                color = "#ff5000";

            noRetorno.image = Url.Content(imagePath);
            noRetorno.itemTitleColor = color;

            //var color = "";
            //switch (usuarioNo.NivelAssociacao)
            //{
            //   case 0:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_0")); 
            //      color = "#fff"; //Branco - Não utilizado
            //      break;
            //   case 1:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_1")); 
            //      color = "#008080"; //Verde - Promocional
            //      break;
            //   case 2:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_2")); 
            //      color = "#ff5000"; //Amarelo - Adesão 1
            //      break;
            //   case 3:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_3")); 
            //      color = "#d00030"; //Vermelho - Adesão 1
            //      break;
            //   case 4:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_4")); 
            //     color = "#003546"; //Azul - Adesão 3
            //      break;
            //   case 5:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_5")); 
            //      color = "#000"; //preto - Não utilizado
            //      break;
            //   case 6:
            //      break;
            //}

            //noRetorno.itemTitleColor = color;

            /*
            noRetorno.data = new DataNoRedeBinder()
            {
                classificacao = usuarioNo.NivelAssociacao.ToString().ToLower(),
                email = usuarioNo.Email,
                telefone = usuarioNo.Celular,
                nome = usuarioNo.Nome,
                sexo = usuarioNo.Sexo.ToUpper(),
                observacoes = (!usuarioNo.GeraBonus ? " STANDBY " : "") + (!usuarioNo.RecebeBonus ? " BLOQUEADO " : ""),
                ladoPatrocinador = usuarioNo.EntradaID.ToString()
            };
            */
            _listaRetorno.Add(noRetorno);

            MontaExibicaoRedeBinaria(filhoEsquerdo, usuarioNo.ID);
            MontaExibicaoRedeBinaria(filhoDireito, usuarioNo.ID);
        }

        private void MontaExibicaoRede(Usuario usuarioNo, int? idParent, int? intQtdeRamos = null)
        {
            if (usuarioNo == null) return;

            //var franqueadoNo = franqueadoRepository.GetByCodigoProtheus(noAssinatura.idFranqueadoProtheus, true);

            if (intQtdeRamos == null)
            {
                if (ConfiguracaoHelper.TemChave("REDE_QTDE_RAMOS"))
                    intQtdeRamos = ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS") - 1;
                else
                    intQtdeRamos = 3;
            }

            ArrayList filhos = new ArrayList();
            for (int i = 0; i <= intQtdeRamos; i++)
            {
                if (filhosPosicao.Any(n => n.Assinatura.Equals(usuarioNo.Assinatura + i)))
                {
                    filhos.Add(filhosPosicao.Where(n => n.Assinatura.Equals(usuarioNo.Assinatura + i)).First());
                }
            }


            NoRedeBinder noRetorno = new NoRedeBinder();
            noRetorno.id = usuarioNo.ID;
            noRetorno.label = usuarioNo.Login;
            noRetorno.title = usuarioNo.Login;
            noRetorno.parent = idParent;

            string status = usuarioNo.DataValidade >= App.DateTimeZion ? "Ativo" : "Inativo";
            if (diasPerdaPosicao > 0)
            {
                if (usuarioNo.DataValidade < Core.Helpers.App.DateTimeZion.AddDays(-diasPerdaPosicao))
                    status = "Cancelado";
            }

            //noRetorno.description =
            //   "<br />Nome: " + (usuarioNo.NomeFantasia.Length > 18 ? usuarioNo.NomeFantasia.Substring(0,18) : usuarioNo.NomeFantasia) +
            //   "<br />kit: " + associacao.Nome +
            //   (usuarioNo.PatrocinadorDiretoID.HasValue ? ("<br />Patrocinador: " + usuarioNo.PatrocinadorDireto.Login) : "") +
            //   "<br />Status: " + status;

            var infoHtmp = new StringBuilder();

            infoHtmp.AppendFormat("<div class='item-rede' data-login='{0}'>", usuarioNo.Login);

            if (usuarioNo.PatrocinadorDiretoID.HasValue)
            {
                infoHtmp.AppendFormat("<span>{0}: {1}</span>", traducaoHelper["PATROCINADOR"], usuarioNo.PatrocinadorDireto.Login);
            }

            if (usuarioNo.DataAtivacao.HasValue && usuarioNo.DataAtivacao.Value != new DateTime(1900, 1, 1))
            {
                infoHtmp.AppendFormat("<span>{0}: {1}</span>", traducaoHelper["ATIVACAO"], usuarioNo.DataAtivacao.Value.ToShortDateString());
            }

            if (usuarioNo.NivelAssociacao != 0)
            {
                var associacao = _listaAssociacoes.FirstOrDefault(a => a.Nivel == usuarioNo.NivelAssociacao);
                infoHtmp.AppendFormat("<span>{0}: {1}</span>", traducaoHelper["ASSOCIACAO"], associacao.Nome);
            }

            infoHtmp.AppendFormat("<span>Ranking: {0}</span>", usuarioNo.NivelClassificacao);

            infoHtmp.Append("</div>");

            noRetorno.description = infoHtmp.ToString();


            if (idParent == null)
            {
                //noRetorno.phone = "Pontuação E:" + usuarioNo.UltimaPosicao.AcumuladoEsquerda.ToString("#,###,##0.##") + "  D:" + usuarioNo.UltimaPosicao.AcumuladoDireita.ToString("#,###,##0.##");
            }
            //noRetorno.groupTitleColor = "blue";


            var color = Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_COLOR_" + usuarioNo.NivelAssociacao);
            if (color == null)
                color = "#ff5000";

            if (status == "Inativo")
                color = "#c7c7c7";
            else
               if (status == "Cancelado")
                color = "#DC143C";

            noRetorno.itemTitleColor = "#CCC";
            noRetorno.groupTitleColor = "#CCC";

            var imagePath = Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_" + usuarioNo.NivelAssociacao);
            if (imagePath == null || imagePath == "")
                imagePath = "~/Content/img/" + Helpers.Local.Cliente + "/classificacao/1.PNG";
            noRetorno.image = Url.Content(imagePath);

            //var color = "";
            //switch (usuarioNo.NivelAssociacao)
            //{
            //   case 0:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_0"));
            //      color = "#fff"; //Branco - Não utilizado
            //      break;
            //   case 1:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_1"));
            //      color = "#008080"; //Verde - Promocional
            //      break;
            //   case 2:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_2"));
            //      color = "#ff5000"; //Amarelo - Adesão 1
            //      break;
            //   case 3:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_3"));
            //      color = "#d00030"; //Vermelho - Adesão 1
            //      break;
            //   case 4:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_4"));
            //      color = "#003546"; //Azul - Adesão 3
            //      break;
            //   case 5:
            //      noRetorno.image = Url.Content(Core.Helpers.ConfiguracaoHelper.GetString("IMAGEM_NIVEL_ASSOCIACAO_5"));
            //      color = "#000"; //preto - Não utilizado
            //      break;
            //   case 6:
            //      break;
            //}

            //noRetorno.itemTitleColor = color;

            /*
            noRetorno.data = new DataNoRedeBinder()
            {
                classificacao = usuarioNo.NivelAssociacao.ToString().ToLower(),
                email = usuarioNo.Email,
                telefone = usuarioNo.Celular,
                nome = usuarioNo.Nome,
                sexo = usuarioNo.Sexo.ToUpper(),
                observacoes = (!usuarioNo.GeraBonus ? " STANDBY " : "") + (!usuarioNo.RecebeBonus ? " BLOQUEADO " : ""),
                ladoPatrocinador = usuarioNo.EntradaID.ToString()
            };
            */
            _listaRetorno.Add(noRetorno);

            foreach (var filho in filhos)
            {
                MontaExibicaoRede(filho as Usuario, usuarioNo.ID, intQtdeRamos);
            }

        }


        public class UsuarioLinha
        {
            public string id { get; set; }
            public string usuarioId { get; set; }
            public string linha { get; set; }
            public string ordem { get; set; }
        }

        #endregion

        #region Actions

        #region Tabuleiro

        public ActionResult Tabuleiro(int? idTab)
        {
            obtemMensagem();

            ViewBag.Background = "background-image: url(" + @Url.Content("~/Arquivos/banners/" + Helpers.Local.Sistema + "/fundo.jpg") + "); background-repeat: no-repeat; background-color: #000000; background-size: cover;";
            ViewBag.idUsuario = usuario.ID;

            ViewBag.tabuleiroName = traducaoHelper["GALAXIA"];

            string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");
            tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);
            ViewBag.Token = tokenLocal;

            int idTabuleiro = idTab ?? 0;

            if (idTabuleiro < 0)
            {
                idTabuleiro = 0;
            }

            try
            {
                ViewBag.ShowReportPayment = false;
                ViewBag.RedeTabuleiro = true;

                IEnumerable<Core.Models.TabuleiroNivelModel> tabuleirosNivelConvite = tabuleiroRepository.ObtemNivelTabuleiro(usuario.ID, 1); //1 - Convite
                IEnumerable<Core.Models.TabuleiroNivelModel> tabuleirosNivelAtivos = tabuleiroRepository.ObtemNivelTabuleiro(usuario.ID, 2); //2 - em andamento

                ViewBag.TabuleirosNivelConvite = tabuleirosNivelConvite;
                ViewBag.TabuleirosNivelAtivos = tabuleirosNivelAtivos;
                ViewBag.idTabuleiro = 0;

                Core.Models.TabuleiroModel tabuleiro = null;

                if (tabuleirosNivelAtivos != null && idTabuleiro == 0)
                {
                    //Obtem 1º Tabuleiro ativo do usuario
                    Core.Models.TabuleiroNivelModel tabuleirosNivelAtivo = tabuleirosNivelAtivos.FirstOrDefault();
                    if (tabuleirosNivelAtivo != null)
                    {
                        idTabuleiro = tabuleirosNivelAtivo.TabuleiroID;
                    }
                }

                if (idTabuleiro < 1)
                {
                    idTabuleiro = 1;
                }

                Core.Models.TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(usuario.ID, idTabuleiro);

                if (tabuleiroUsuario == null)
                {
                    //Pega primeiro disponivel
                    tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(usuario.ID, 0);

                    if (tabuleiroUsuario == null)
                    {
                        //Usuariop não possui tabuleiro
                        idTabuleiro = 0;
                    }
                }
                else
                {
                    idTabuleiro = tabuleiroUsuario.TabuleiroID;
                }

                if (idTabuleiro != 0)
                {
                    ViewBag.Timer = null;
                    if (tabuleiroUsuario != null)
                    {
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

                        if (!tabuleiroUsuario.InformePag)
                        {
                            DateTime timePagamentoMin = tabuleiroUsuario.DataInicio.AddMinutes(tempoMin);

                            if (timePagamentoMin > DateTime.Now)
                            {
                                //Format: '03/30/2024 17:59:00'
                                ViewBag.Timer = tabuleiroUsuario.DataInicio.AddMinutes(tempoMin).ToString("MM/dd/yyyy HH:mm:ss");
                                ViewBag.ShowReportPayment = true;
                            }

                            //Convidado tem até 1h para pagar
                            DateTime timePagamentoMax = tabuleiroUsuario.DataInicio.AddMinutes(tempoMax);
                            if (timePagamentoMax > DateTime.Now)
                            {
                                ViewBag.ShowReportPayment = true;
                            }
                        }
                    }

                    ViewBag.idTabuleiro = idTabuleiro;
                    ViewBag.tabuleiroAtivo = null;

                    if (tabuleirosNivelAtivos.Count() > 0)
                    {
                        Core.Models.TabuleiroNivelModel tabuleiroAtivo = tabuleirosNivelAtivos.Where(x => x.TabuleiroID == idTabuleiro).FirstOrDefault();
                        if (tabuleiroAtivo == null)
                        {
                            tabuleiroAtivo = tabuleirosNivelAtivos.FirstOrDefault();
                        }

                        //Obtem o tabuleiro que será exibido quando a pag for carregada
                        int idTabuleiroAtivo = tabuleiroAtivo.TabuleiroID;
                        ViewBag.tabuleiroAtivo = tabuleiroAtivo;
                        if (idTabuleiroAtivo > 0)
                        {
                            tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro, usuario.ID);
                            ViewBag.tabuleiro = tabuleiro;
                            if (!String.IsNullOrEmpty(tabuleiro.ApelidoMaster) && tabuleiro.ApelidoMaster.Length > 3)
                            {
                                ViewBag.tabuleiroName = tabuleiro.ApelidoMaster.Substring(0, 3).ToUpper() + "-" + tabuleiro.ID.ToString("00000");
                            }
                            if (usuario.ID == tabuleiro.Master && !tabuleiroUsuario.PagoSistema)
                            {
                                ViewBag.Pagar = true;
                            }
                            else
                            {
                                ViewBag.Pagar = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { strPopupTitle = "Erro", strPopupMessage = ex.Message, Sair = "true" });
            }

            return View();
        }

        [HttpPost]
        public ActionResult GetData(string usuarioID, string targetID, string tabuleiroID, string nivel, string token)
        {
            if (usuarioID.IsNullOrEmpty() || targetID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || nivel.IsNullOrEmpty())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                //Usuario Logado
                int idUsuario = int.Parse(usuarioID);
                //Usuario no qual se quer as informações
                int idTarget = int.Parse(targetID);
                //Tabuleiro que o usuario que se deseja informações esta
                int idTabuleiro = int.Parse(tabuleiroID);

                if (idUsuario <= 0 || idTarget <= 0 || idTabuleiro <= 0)
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                if (idUsuario == usuario.ID)
                {
                    //Obtem usuario target
                    Core.Models.TabuleiroInfoUsuarioModel obtemInfoUsuario = tabuleiroRepository.ObtemInfoUsuario(idTarget, idUsuario, idTabuleiro);

                    if (obtemInfoUsuario != null)
                    {
                        //Verifica se Master Esta ok com as regras, para que sua conta seja exibida
                        if (!tabuleiroRepository.MasterRuleOK(idUsuario, idTabuleiro))
                        {
                            //Não estando ok, a conta do sistema é exibida para pagamento
                            obtemInfoUsuario = tabuleiroRepository.ObtemInfoSystem();
                        }
                        obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                        obtemInfoUsuario.Carteira = CriptografiaHelper.Morpho(obtemInfoUsuario.Carteira, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    }
                    else
                    {
                        string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02" };
                        Mensagem(traducaoHelper["ERRO"], strMensagemParam3, "err");
                        //Não há dados para ser exibido
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02");
                    }

                    //Obtem os dados do tabuleiro do usuario que se quer informações
                    Core.Models.TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idTarget, idTabuleiro);

                    //Se o UsuarioLogado é o Master,
                    //caso o master não tenha recebido (PagoMaster = false)
                    //Caso Convidado tenha informado que efetuou o pagamento (InformePag = true)
                    //O Master pode ter a opção de confirmação de recebimento
                    if (tabuleiroUsuario.MasterID == usuario.ID && !tabuleiroUsuario.PagoMaster && tabuleiroUsuario.InformePag)
                    {
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

                        //Soma os dois para dar o tempo que o master pode confirmar o pagamento
                        DateTime timePagamento = tabuleiroUsuario.DataInicio.AddMinutes(tempoMin);
                        timePagamento = tabuleiroUsuario.DataInicio.AddMinutes(tempoMax);

                        //Se estiver no prazo, master pode confirmar recebimento
                        //Removido, pois job vai retirar o usuario do tabuleiro
                        //if (timePagamento > DateTime.Now)
                        //{
                        //    obtemInfoUsuario.ConfirmarRecebimento = true;
                        //}
                        obtemInfoUsuario.ConfirmarRecebimento = true;
                    }

                    JsonResult jsonResult = new JsonResult
                    {
                        Data = obtemInfoUsuario,
                        RecursionLimit = 1000
                    };

                    return jsonResult;
                }

                string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);

            }
            catch (Exception)
            {
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_01");
            }
        }

        [HttpPost]
        public ActionResult GetDataSysPag(string token)
        {
            try
            {
                string tokenDescript = CriptografiaHelper.Morpho(token, CriptografiaHelper.TipoCriptografia.Descriptografa);
                string tokenLocal = usuario.ID.ToString() + "|" + usuario.Nome + "|" + DateTime.Now.ToString("yyyyMMdd");

                tokenLocal = CriptografiaHelper.Morpho(tokenLocal, CriptografiaHelper.TipoCriptografia.Criptografa);

                if (token != tokenLocal)
                {
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemToken, "ale");
                    //Devolve que tokem é invalido
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["TOKEN_INVALIDO"]);
                }

                //Obtem usuario do systema 
                Core.Models.TabuleiroInfoUsuarioModel obtemInfoUsuario = tabuleiroRepository.ObtemInfoSysPag();

                if (obtemInfoUsuario != null)
                {
                    obtemInfoUsuario.Pix = CriptografiaHelper.Morpho(obtemInfoUsuario.Pix, CriptografiaHelper.TipoCriptografia.Descriptografa);
                    obtemInfoUsuario.Carteira = CriptografiaHelper.Morpho(obtemInfoUsuario.Carteira, CriptografiaHelper.TipoCriptografia.Descriptografa);

                }
                else
                {
                    string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02" };
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
            catch (Exception)
            {
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDS_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDSP_01");
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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
                    string[] strMensagemParam3 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GD_02" };
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
            catch (Exception)
            {
                string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDS_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam4, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GDSP_01");
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

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idTabuleiro = int.Parse(tabuleiroID);

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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_02" };
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
            catch (Exception)
            {
                string[] strMensagemParam5 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam5, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01");
            }
        }

        [HttpPost]
        public ActionResult GetInvite(string usuarioID, string boardID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || boardID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idBoard = int.Parse(boardID);

                //Seta para primeiro tabuleiro caso seja 0
                if (idBoard == 0)
                {
                    idBoard = 1;
                }

                if (idUsuario <= 0 || idBoard <= 0)
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Caso não exista o patrocionador usa o usuario 2580 que é o primeiro alvo Veja a tabela usuario.usuario
                int patrocinadoID = usuario.PatrocinadorDiretoID ?? 2580; //2580 é o primeiro alvo

                //Inclui usuario no novo tabuleiro
                string tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiro(usuario.ID, patrocinadoID, idBoard, "Convite");

                JsonResult jsonResult = new JsonResult
                {
                    Data = traducaoHelper[tabuleiroIncluir],
                    RecursionLimit = 1000
                };
                return jsonResult;
            }
            catch (Exception)
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GI_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GI_01");
            }
        }

        [HttpPost]
        public ActionResult ReportPayment(string usuarioID, string tabuleiroID, string usuarioIDPag, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || usuarioIDPag.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idTabuleiro = int.Parse(tabuleiroID);
                int idUsuarioPag = int.Parse(usuarioIDPag);

                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioPag <= 0)
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Informar Pagamento
                string retorno = tabuleiroRepository.InformarPagamento(usuario.ID, idTabuleiro, idUsuarioPag);
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

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01");
            }
        }

        [HttpPost]
        public ActionResult ReportReceipt(string usuarioID, string UsuarioConvidadoID, string tabuleiroID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty() || UsuarioConvidadoID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idUsuarioConvidado = int.Parse(UsuarioConvidadoID);
                int idTabuleiro = int.Parse(tabuleiroID);

                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioConvidado <= 0)
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Informar Recebimento
                string retorno = tabuleiroRepository.InformarRecebimento(idUsuarioConvidado, idUsuario, idTabuleiro);
                switch (retorno)
                {
                    case "OK":

                        TabuleiroBoardModel tabuleiroBoard = tabuleiroRepository.ObtemTabuleiroBoard(idTabuleiro);

                        Usuario usuarioConvidado = usuarioRepository.Get(idUsuarioConvidado);

                        //Efetuar Credito no Master
                        var lancamento = new Lancamento();
                        lancamento.UsuarioID = idUsuario;
                        lancamento.Tipo = Lancamento.Tipos.Credito;
                        lancamento.ReferenciaID = lancamento.UsuarioID;
                        lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper[tabuleiroBoard.Nome], " - ", usuarioConvidado.Nome);
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
                        lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper[tabuleiroBoard.Nome], " - ", usuario.Nome);
                        lancamento.DataLancamento = App.DateTimeZion;
                        lancamento.DataCriacao = App.DateTimeZion;
                        lancamento.ContaID = 7; //Transferencia
                        lancamento.CategoriaID = 7; //Transferencia
                        lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                        lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Transferencia);
                        lancamentoRepository.Save(lancamento);

                        //Chama incluir no tabuleiro para ver se 
                        //o tabuleiro esta completo
                        string tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiro(idUsuarioConvidado, idUsuario, tabuleiroBoard.ID, "Completa");
                        if (tabuleiroIncluir == "COMPLETO")
                        {
                            string[] strMensagem = new string[] { traducaoHelper["RECEBIMENTO_CONFIMADO_COM_SUCESSO"], traducaoHelper["MENSAGEM_TABULEIRO_COMPLETO_1"], traducaoHelper["MENSAGEM_TABULEIRO_COMPLETO_2"] };
                            Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
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

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01");
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

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idUsuarioConvidado = int.Parse(UsuarioConvidadoID);
                int idTabuleiro = int.Parse(tabuleiroID);

                if (idUsuario <= 0 || idTabuleiro <= 0 || idUsuarioConvidado <= 0)
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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Informar Recebimento
                string retorno = tabuleiroRepository.RemoverUsuario(idUsuarioConvidado, idUsuario, idTabuleiro);
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

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RP_01");
            }
        }

        [HttpPost]
        public ActionResult ReportPaymentSystem(string usuarioID, string tabuleiroID, string token)
        {
            if (usuarioID.IsNullOrEmpty() || tabuleiroID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idTabuleiro = int.Parse(tabuleiroID);

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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Obtem os dados do tabuleiro do usuario que se quer informações
                Core.Models.TabuleiroUsuarioModel tabuleiroUsuario = tabuleiroRepository.ObtemTabuleiroUsuario(idUsuario, idTabuleiro);

                string retorno = "";
                //Se o UsuarioLogado é o Master,
                //Somente master para o sistema
                if (tabuleiroUsuario.MasterID == usuario.ID)
                {
                    //Informar Pagamento ao sistema
                    retorno = tabuleiroRepository.InformarPagtoSistema(usuario.ID, idTabuleiro);
                    switch (retorno)
                    {
                        case "OK":
                            string[] strMensagem = new string[] { traducaoHelper["PAGAMENTO_SISTEMA_INFORMADO_COM_SUCESSO"] };
                            Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");

                            TabuleiroBoardModel tabuleiroBoard = tabuleiroRepository.ObtemTabuleiroBoard(1);

                            //Efetuar Credito no Master
                            var lancamento = new Lancamento();
                            lancamento.UsuarioID = idUsuario;
                            lancamento.Tipo = Lancamento.Tipos.Debito;
                            lancamento.ReferenciaID = lancamento.UsuarioID;
                            lancamento.Descricao = String.Format("{0}{1}{2}", tabuleiroBoard.Nome, " - ", traducaoHelper["SISTEMA"]);
                            lancamento.DataLancamento = App.DateTimeZion;
                            lancamento.DataCriacao = App.DateTimeZion;
                            lancamento.ContaID = 7; //Transferencia
                            lancamento.CategoriaID = 7; //Transferencia
                            lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                            lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Licenca);
                            lancamentoRepository.Save(lancamento);

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

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01");
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

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idTabuleiro = int.Parse(tabuleiroID);

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
                    string[] strMensagemToken = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
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

                //Remove usuario do tabuleiro informado
                String retorno = tabuleiroRepository.TabuleiroSair(idUsuario, idTabuleiro);

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

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string erro = ex.Message;
                string[] strMensagemParam1 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam1, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_RPS_01");
            }
        }

        #endregion Tabuleiro

        #region Outros Sistemas

        public ActionResult MinhaArvore()
        {

            return View(usuario);
        }

        [HttpPost]
        public ActionResult MinhaArvore(FormCollection form)
        {
            var loginFranqueado = string.IsNullOrWhiteSpace(form["l"]) ? Request["l"] : form["l"];
            ViewBag.LoginFranqueado = loginFranqueado;

            return View(usuario);
        }

        public ActionResult MeusDiretos()
        {
            int intQtdeNiveis = 3;
            if (ConfiguracaoHelper.TemChave("REDE_QTDE_NIVEIS_DIRETOS"))
                intQtdeNiveis = ConfiguracaoHelper.GetInt("REDE_QTDE_NIVEIS_DIRETOS");

            ArrayList meusDiretos = new ArrayList();
            var lista = usuarioRepository.GetMeusDiretos(usuario.ID, 0, intQtdeNiveis);

            for (int nivel = 1; nivel <= intQtdeNiveis; nivel++)
            {
                var teste = lista.Where(x => x.Nivel == nivel).ToList();
                meusDiretos.Add(teste);
            }

            ViewBag.QuantidadeNiveis = intQtdeNiveis;
            ViewBag.MeusDiretos = meusDiretos;

            //ViewBag.MeusDiretos1 = usuarioRepository.GetMeusDiretos(usuario.ID, 1);
            //ViewBag.MeusDiretos2 = usuarioRepository.GetMeusDiretos(usuario.ID, 2);
            //ViewBag.MeusDiretos3 = usuarioRepository.GetMeusDiretos(usuario.ID, 3);
            //ViewBag.MeusDiretos4 = usuarioRepository.GetMeusDiretos(usuario.ID, 4);
            //ViewBag.MeusDiretos5 = usuarioRepository.GetMeusDiretos(usuario.ID, 5);
            //ViewBag.MeusDiretos6 = usuarioRepository.GetMeusDiretos(usuario.ID, 6);

            return View(usuario);
        }

        public ActionResult MeusInativos()
        {
            int intQtdeNiveis = 7;
            //   if (ConfiguracaoHelper.TemChave("REDE_QTDE_NIVEIS_DIRETOS"))
            //      intQtdeNiveis = ConfiguracaoHelper.GetInt("REDE_QTDE_NIVEIS_DIRETOS");

            ViewBag.QuantidadeNiveis = intQtdeNiveis;

            var inativos = usuarioRepository.GetUsuariosInativos(usuario.ID, intQtdeNiveis);


            ArrayList meusInativos = new ArrayList();
            List<int> niveis = new List<int>();
            foreach (var n in inativos.GroupBy(y => y.Nivel))
            {
                meusInativos.Add(inativos.Where(x => x.Nivel == n.Key).OrderBy(x => x.Login));
                niveis.Add(n.Key);
            }

            ViewBag.Niveis = niveis;
            ViewBag.MeusInativos = meusInativos;

            return View(usuario);
        }

        public ActionResult CadastrosPendentes()
        {
            //Exibe lado de derramamento seguindo a regra de que se o usuario possui filhos ativos, exibe se não, não exibe
            bool blnTrataLadoDerramamento = (Core.Helpers.ConfiguracaoHelper.GetString("EXIBE_OU_NAO_LADO_DERRAMAMENTO") == "true");
            ViewBag.ExibeLadoDerramamento = blnTrataLadoDerramamento;
            if (blnTrataLadoDerramamento)
            {
                var usuarioDireto = usuarioRepository.GetDiretos(usuario.ID);
                bool blnExibeLadoDerramamento = false;
                foreach (var item in usuarioDireto)
                {
                    if (item.Status == (int)Core.Entities.Usuario.TodosStatus.Associado)
                    {
                        blnExibeLadoDerramamento = true;
                        break;
                    }
                }
                ViewBag.ExibeLadoDerramamento = blnExibeLadoDerramamento;

                ViewBag.ListaDerramamento = usuarioRepository.GetListaDerramamento(usuario, traducaoHelper);
            }

            return View(usuario);
        }

        [HttpPost]
        public ContentResult Lado(int usuarioID, int entradaID)
        {
            var patrocinado = usuarioRepository.Get(usuarioID);
            if (patrocinado != null)
            {
                if (patrocinado.PatrocinadorDiretoID == usuario.ID)
                {
                    patrocinado.EntradaID = entradaID;
                    usuarioRepository.Save(patrocinado);
                }
            }
            return Content("OK");
        }

        [HttpPost]
        public JsonResult CarregaDiretos(int codFranqueadoReferencia)
        {

            List<NoRedeBinderOld> ret = new List<NoRedeBinderOld>();

            int codFranqueado = 0;
            if (codFranqueadoReferencia == 0)
            {
                codFranqueado = usuario.ID;
            }
            else
            {
                codFranqueado = codFranqueadoReferencia;
            }

            var filhos = usuarioRepository.GetByExpression(u => u.PatrocinadorDiretoID == codFranqueado).ToList();
            foreach (var filho in filhos)
            {
                NoRedeBinderOld noRet = new NoRedeBinderOld();

                noRet.data = new DataNoRedeBinder()
                {
                    classificacao = associacaoRepository.GetByNivel(filho.NivelAssociacao).Nome,
                    email = filho.Login,
                    telefone = filho.Celular,
                    nome = filho.Nome,
                    statusAtivacao = filho.Status.GetHashCode(),
                    sexo = filho.Sexo.ToUpper(),
                    observacoes = (!filho.GeraBonus ? " STANDBY " : "") + (!filho.RecebeBonus ? " BLOQUEADO " : ""),
                    ladoPatrocinador = filho.EntradaID.ToString()
                };

                ret.Add(noRet);
            }

            return Json(ret);
        }

        private List<Usuario> GetUser(string assinaturaBusca)
        {
            return usuarioRepository.GetByExpression(r => r.Assinatura.StartsWith(assinaturaBusca) && r.Assinatura.Length <= (assinaturaBusca.Length + 3)).OrderBy(r => r.Assinatura.Length).ToList();

        }

        [HttpPost]
        public JsonResult CarregaArvore(FormCollection form)
        {

            NoRedeBinder noRet;
            _listaRetorno = new List<NoRedeBinder>();
            _listaAssociacoes = associacaoRepository.GetAll().ToList();

            var loginFranqueado = string.IsNullOrWhiteSpace(form["l"]) ? Request["l"] : form["l"];

            var patrocinadorPosicaoLogin = usuario.PatrocinadorPosicao != null ? usuario.PatrocinadorPosicao.Login : null;

            var assinaturaBusca = usuario.Assinatura;
            if (!string.IsNullOrWhiteSpace(loginFranqueado))
            {
                var usuarioBusca = usuarioRepository.GetByLogin(loginFranqueado);
                if (usuarioBusca != null && !string.IsNullOrWhiteSpace(usuarioBusca.Assinatura))//&& usuarioBusca.Status == Usuario.TodosStatus.Associado && usuarioBusca.Assinatura.Contains(assinaturaBusca))
                {
                    assinaturaBusca = usuarioBusca.Assinatura;
                    patrocinadorPosicaoLogin = usuarioBusca.PatrocinadorPosicao != null ? usuarioBusca.PatrocinadorPosicao.Login : null;
                }
                else
                {
                    return Json("");
                }
            }
            //apenas permite carregar resultados, se o login de busca fizer parte da rede de quem buscou

            if (!assinaturaBusca.Contains(usuario.Assinatura))
            {
                return Json("");
            }

            filhosPosicao = usuarioRepository.GetByExpression(r => r.Assinatura.StartsWith(assinaturaBusca) && r.Assinatura.Length <= (assinaturaBusca.Length + QuantidadeNivelExibicao)).OrderBy(r => r.Assinatura.Length).Take(NUMERO_USUARIOS_REDE_EXIBICAO).ToList();

            //filhosPosicao = GetUser(assinaturaBusca);
            GetJsonRede();

            //rede fake
            //var lista = new List<NoRedeBinder>();
            //var description = "<div class='item-rede'> " +
            //"<span>Patrocinador: XPTO0912</span>" +
            //"<span>Activation: 01/01/1987 </span>" +
            //"<span>Product: XXXXXX</span> " +
            //"<span>Ranking: 1</span>" +
            //"</div>";
            //var imagePath = Url.Content("~/Content/img/" + Helpers.Local.Cliente + "/classificacao/1.PNG");
            //lista.Add(new NoRedeBinder { id = 0, title = "Usuário 0", label = "Usuário 0", description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 1, title = "Usuário 00", label = "Usuário 00", parent = 0, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 2, title = "Usuário 01", label = "Usuário 01", parent = 0, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 3, title = "Usuário 02", label = "Usuário 02", parent = 0, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 000", label = "Usuário 000", parent = 1, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 001", label = "Usuário 001", parent = 1, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 002", label = "Usuário 002", parent = 1, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 010", label = "Usuário 010", parent = 2, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 011", label = "Usuário 011", parent = 2, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 012", label = "Usuário 012", parent = 2, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 020", label = "Usuário 020", parent = 3, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 021", label = "Usuário 021", parent = 3, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });
            //lista.Add(new NoRedeBinder { id = 4, title = "Usuário 022", label = "Usuário 022", parent = 3, description = description, itemTitleColor = "#CCC", groupTitleColor = "#CCC", image = imagePath });

            var data = new
            {
                Data = _listaRetorno,
                patrocinadorPosicaoLogin = patrocinadorPosicaoLogin,
                exibirControleSubirNivel = loginFranqueado != usuario.Login && !string.IsNullOrEmpty(loginFranqueado)
            };

            JsonResult jsonResult = new JsonResult
            {
                Data = data,
                RecursionLimit = 1000
            };

            return jsonResult;
        }

        public ActionResult Unilevel(int topo = 0)
        {
            if (topo == 0 || topo == usuario.ID || topo == usuario.PatrocinadorDiretoID)
            {
                topo = usuario.ID;
                //ViewBag.NivelAtual = usuarioRepository.GetMeusDiretos(usuario.ID, 1, 1);
                //ViewBag.SeqNivelAtual = 1;

                ViewBag.Nivel0 = 0;
                ViewBag.Nivel1 = usuario;
                ViewBag.Nivel2 = ViewBag.NivelAtual;
            }
            else
            {
                // Validar topo
                Usuario usuarioTopo = usuarioRepository.Get(topo);
                //if (usuarioTopo == null)
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}
                //if (usuarioTopo.Assinatura.Length < usuario.Assinatura.Length)
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}
                //if (!usuarioTopo.Assinatura.Contains(usuario.Assinatura.Trim()))
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}

                // Montar lista de upline
                //List<Usuario> upline = new List<Usuario>();
                //int idUsuarioAtual = usuarioTopo.ID;
                //int iNivel = 1;

                //Usuario usuarioAtual = new Usuario();
                //usuarioAtual = usuarioTopo;

                //while (idUsuarioAtual != usuario.ID)
                //{
                //    if (usuarioAtual != null)
                //    {
                //        upline.Add(usuarioAtual);
                //        idUsuarioAtual = usuarioAtual.PatrocinadorDiretoID.Value;
                //        usuarioAtual = usuarioRepository.Get(idUsuarioAtual);
                //        iNivel++;
                //    }
                //    else
                //    {
                //        idUsuarioAtual = usuario.ID;
                //    }
                //}

                //upline.Add(usuario); // Carregar nível 0

                // Carregar nível selecionado
                //ViewBag.Upline = upline;
                //ViewBag.NivelAtual = usuarioRepository.GetMeusDiretos_V2(usuarioTopo.ID, 1, 1);
                //ViewBag.NivelAtual = usuarioRepository.GetMeusDiretos(usuarioTopo.ID, 1, 1);
                //ViewBag.SeqNivelAtual = iNivel;


                ViewBag.Nivel0 = usuarioTopo.PatrocinadorDiretoID != null ? usuarioTopo.PatrocinadorDiretoID : 0;
                ViewBag.Nivel1 = usuarioTopo;
                ViewBag.Nivel2 = ViewBag.NivelAtual;
            }

            ViewBag.Topo = topo;

            ViewBag.MeusDiretos1 = usuarioRepository.GetMeusDiretos(topo, 1, 4).ToList();
            ViewBag.MeusDiretos2 = usuarioRepository.GetMeusDiretos(topo, 2, 4).ToList();
            ViewBag.MeusDiretos3 = usuarioRepository.GetMeusDiretos(topo, 3, 4).ToList();

            return View();
        }

        public ActionResult BuscarUnilevel(int Topo = 0, string Login = "", string Nome = "")
        {
            var IdTopo = (Topo == 0 ? usuario.ID : Topo);
            var usuarioTopo = usuarioRepository.Get(IdTopo);

            // Buscar por Login/Nome
            #region nivel por Login/Nome


            var listaBusca = new List<UsuarioRede>();
            if (!string.IsNullOrEmpty(Login.Trim()) || !string.IsNullOrEmpty(Nome.Trim()))
            {
                UsuarioRede usuarioRedeNivel;

                var listaUsuarioNivel = usuarioRepository.GetByExpression(
                    usr => (
                            (!string.IsNullOrEmpty(Login.Trim()) && usr.Login.Contains(Login.Trim()))
                            || (!string.IsNullOrEmpty(Nome.Trim()) && usr.Nome.Contains(Nome.Trim()))
                        )
                    ).Take(10).ToList();

                if (listaUsuarioNivel != null)
                {
                    foreach (var item in listaUsuarioNivel)
                    {
                        // Validar assinatura depois por conta de truncar o LINQ por causa do tamanho
                        if (item.Assinatura.StartsWith(usuarioTopo.Assinatura))
                        {
                            usuarioRedeNivel = new UsuarioRede
                            {
                                ID = item.ID,
                                Nome = item.Nome,
                                Login = item.Login,
                                Classificacao = classificacaoRepository.GetByExpression(cl => cl.Nivel.Equals(item.NivelClassificacao)).FirstOrDefault().Nome,
                                AtivacaoMensal = (item.DataAtivacao.HasValue ? (item.DataAtivacao.Value <= App.DateTimeZion.AddMonths(1).AddDays(-App.DateTimeZion.Day) ? "Sim" : "Não") : "Não")
                            };

                            listaBusca.Add(usuarioRedeNivel);
                        }
                    }
                }
                else
                {
                    listaBusca.Add(null);
                }
            }

            #endregion
            ViewBag.Busca = listaBusca;

            return PartialView("UnilevelResultadoBusca");
        }

        public ActionResult UnilevelD3(int topo = 0)
        {
            Usuario usuarioTopo;

            if (topo == 0 || topo == usuario.ID)
            {
                usuarioTopo = usuario;
                topo = usuario.ID;
                //ViewBag.NivelAtual = usuarioRepository.GetMeusDiretos(usuario.ID, 1, 1);
                //ViewBag.SeqNivelAtual = 1;

                ViewBag.Nivel0 = 0;
                ViewBag.Nivel1 = usuario;
                ViewBag.Nivel2 = ViewBag.NivelAtual;
            }
            else
            {
                // Validar topo
                usuarioTopo = usuarioRepository.Get(topo);
                //if (usuarioTopo == null)
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}
                //if (usuarioTopo.Assinatura.Length < usuario.Assinatura.Length)
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}
                //if (!usuarioTopo.Assinatura.Contains(usuario.Assinatura.Trim()))
                //{
                //    return RedirectToAction("SemAcesso", "Home");
                //}

                ViewBag.Nivel0 = usuarioTopo.PatrocinadorDiretoID != null ? usuarioTopo.PatrocinadorDiretoID : 0;
                ViewBag.Nivel1 = usuarioTopo;
                ViewBag.Nivel2 = ViewBag.NivelAtual;
            }

            ViewBag.Topo = topo;

            List<Core.Models.StoredProcedures.spC_ObtemDiretos> listaN1 = usuarioRepository.GetMeusDiretos(topo, 1, 3).ToList();
            List<Core.Models.StoredProcedures.spC_ObtemDiretos> listaN2 = usuarioRepository.GetMeusDiretos(topo, 2, 3).ToList();

            int qtdeN1 = listaN1.Count() - listaN2.GroupBy(x => x.PatrocinadorDiretoID).Count();
            int qtdeN2 = listaN2.Count();

            // === 
            var usuBinder = new Sistema.ModelBinders.UsuarioBinder();
            usuBinder.usuarioId = usuarioTopo.ID;
            var foto0 = usuBinder.FotoUsuarioId;

            if (String.IsNullOrEmpty(foto0))
            {
                foto0 = Url.Content("~/Content/img/" + Helpers.Local.Sistema + "/icoNivel2ativa.png");
            }
            else
            {
                foto0 = Url.Content(foto0);
            }

            if (listaN1 != null && listaN1.Count > 0)
            {
                foreach (var usuario1 in listaN1)
                {
                    var usuarioBinder1 = new Sistema.ModelBinders.UsuarioBinder();
                    usuarioBinder1.usuarioId = usuario1.IDAfiliado;
                    var foto1 = usuarioBinder1.FotoUsuarioId;
                    if (String.IsNullOrEmpty(foto1))
                    {
                        foto1 = Url.Content("~/Content/img/" + Helpers.Local.Sistema + "/icoNivel2ativa.png");
                    }
                    else
                    {
                        foto1 = Url.Content(foto1);
                    }

                    if (listaN2 != null && listaN2.Count > 0)
                    {
                        var filtro2 = listaN2.Where(x => x.PatrocinadorDiretoID == usuario1.IDAfiliado).ToList();
                        if (filtro2 != null && filtro2.Count > 0)
                        {
                            foreach (var usuario2 in filtro2)
                            {
                                var usuarioBinder2 = new Sistema.ModelBinders.UsuarioBinder();
                                usuarioBinder2.usuarioId = usuario2.IDAfiliado;
                                var foto2 = usuarioBinder2.FotoUsuarioId;

                                if (String.IsNullOrEmpty(foto2))
                                {
                                    foto2 = Url.Content("~/Content/img/" + Helpers.Local.Sistema + "/icoNivel2ativa.png");
                                }
                                else
                                {
                                    foto2 = Url.Content(foto2);
                                }
                            }
                        }
                    }
                }
            }

            // ===

            ViewBag.qq = (qtdeN1 - qtdeN2) * 320;
            //ViewBag.MeusDiretos1 = listaN1;
            //ViewBag.MeusDiretos2 = listaN2;


            return View();
        }

        public ActionResult Rede(int topo = 0)
        {
            obtemMensagem();

            var usuarioId = topo == 0 ? usuario.ID : topo;

            var usuarioTopo = usuario;
            ViewBag.Nivel0 = 0;
            ViewBag.Nivel1 = usuario;

            if (!topo.Equals(0) && topo != usuario.PatrocinadorDiretoID)
            {
                usuarioTopo = usuarioRepository.GetByExpression(g => g.ID.Equals(usuarioId) && g.Assinatura.StartsWith(usuario.Assinatura)).FirstOrDefault();

                if (usuarioTopo == null)
                {
                    Mensagem(traducaoHelper["INCONSISTENCIA"], new string[] { traducaoHelper["USUARIO_NAO_FAZ_PARTE_REDE"] }, "ale");
                    return RedirectToAction("Rede", new { topo = usuario.ID });
                }

                ViewBag.Nivel0 = usuarioTopo.PatrocinadorPosicaoID != null ? usuarioTopo.PatrocinadorPosicaoID : 0;
                ViewBag.Nivel1 = usuarioTopo;
            }

            var usuarios = usuarioRepository.GetByExpression(r =>
                r.Assinatura.StartsWith(usuarioTopo.Assinatura)
                && r.Assinatura.Length <= (usuarioTopo.Assinatura.Length + QuantidadeNivelExibicao))
                .OrderBy(r => r.Assinatura.Length)
                .ToList();

            if (usuarios.Any())
            {
                var listaAssociacoes = associacaoRepository.GetAll();

                foreach (var item in usuarios)
                {
                    item.Apelido = listaAssociacoes.Where(x => x.Nivel == item.NivelAssociacao).First().Nome;
                }

                ViewBag.MeusDiretos1 = usuarios;
                ViewBag.MeusDiretos2 = usuarios;
                ViewBag.MeusDiretos3 = usuarios;

                ViewBag.UltimoEsquerda = usuarios.LastOrDefault(x => x.Assinatura.EndsWith("0")) != null ? usuarios.LastOrDefault(x => x.Assinatura.EndsWith("0")).ID != topo ? usuarios.LastOrDefault(x => x.Assinatura.EndsWith("0")).ID : 0 : 0;
                ViewBag.UltimoDireita = usuarios.LastOrDefault(x => x.Assinatura.EndsWith("1")) != null ? usuarios.LastOrDefault(x => x.Assinatura.EndsWith("1")).ID != topo ? usuarios.LastOrDefault(x => x.Assinatura.EndsWith("1")).ID : 0 : 0;
            }

            ViewBag.Topo = topo;

            return View();
        }

        [HttpPost]
        public ActionResult BuscaRede(FormCollection form)
        {
            var id = 0;

            int.TryParse(form["ProcuraLogin"], out id);

            return RedirectToAction("Rede", new { topo = id });
        }

        public ActionResult MigrarRede()
        {
            var meusDiretos = usuarioRepository.GetUsuarioMigrarRede(usuario.ID);

            ViewBag.Assinatura = usuario.Assinatura;

            ViewBag.MeusDiretos = meusDiretos;
            ViewBag.QuantidadeLinhas = meusDiretos.Count();

            List<Object> linhas = new List<object>();
            linhas.Add(new { nome = traducaoHelper["LINHA"] + " 1", id = "0" });
            linhas.Add(new { nome = traducaoHelper["LINHA"] + " 2", id = "1" });
            //linhas.Add(new { nome = traducaoHelper["LINHA"] + " 3", id = "2" });
            ViewBag.Linhas = new SelectList(linhas, "id", "nome"); ;

            return View(usuario);
        }

        #endregion Outros Sistemas

        #endregion Actions

        #region Json

        public JsonResult GetUsuarios(string search)
        {
            IQueryable<Usuario> usuarios = usuarioRepository.GetByExpression(x => x.Login.Contains(search) && x.Assinatura.StartsWith(usuario.Assinatura));
            return Json(usuarios.Select(s => new { id = s.ID, text = s.Login }).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MigerRedeGravar()
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Request.InputStream);
                string strJSON = @reader.ReadToEnd();

                IList<UsuarioLinha> usuarioLinhas = JsonConvert.DeserializeObject<IList<UsuarioLinha>>(@strJSON);

                if (usuarioLinhas != null && usuarioLinhas.Any(a => a.linha.Equals("0")))
                {
                    foreach (UsuarioLinha item in usuarioLinhas)
                    {
                        RedeMigracao redeMigracao = new RedeMigracao();
                        redeMigracao.UsuarioID = int.Parse(item.usuarioId);
                        redeMigracao.PatrocinadorID = usuario.ID;
                        redeMigracao.Linha = int.Parse(item.linha);
                        redeMigracao.Ordem = int.Parse(item.ordem);

                        if (item.id == "0")
                        {
                            db.RedeMigracao.Add(redeMigracao);
                        }
                        else
                        {
                            redeMigracao.ID = int.Parse(item.id);
                            db.Entry(redeMigracao).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();

                    return Json("OK");
                }
                else
                {
                    return Json(traducaoHelper["MIGRACAO_AVISO_LINHA_1"]);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        #endregion
    }

}
