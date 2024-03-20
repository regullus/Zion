#region Bibliotecas

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

        public ActionResult Tabuleiro()
        {
            obtemMensagem();

            ViewBag.Background = "background-image: url(" + @Url.Content("~/Arquivos/banners/" + Helpers.Local.Sistema + "/fundo.jpg") + "); background-repeat: no-repeat; background-color: #000000; background-size: cover;";
            ViewBag.idUsuario = usuario.ID;
            try
            {
                ViewBag.RedeTabuleiro = true;
                
                int idTabuleiro = 0;
                
                IEnumerable<Core.Models.TabuleiroNivelModel> tabuleirosNivelConvite = tabuleiroRepository.ObtemNivelTabuleiro(usuario.ID, 1); //1 - Convite
                IEnumerable<Core.Models.TabuleiroNivelModel> tabuleirosNivelAtivos = tabuleiroRepository.ObtemNivelTabuleiro(usuario.ID, 2); //2 - em andamento
                
                ViewBag.TabuleirosNivelConvite = tabuleirosNivelConvite;
                ViewBag.TabuleirosNivelAtivos = tabuleirosNivelAtivos;

                Core.Models.TabuleiroModel tabuleiro = null;

                if (tabuleirosNivelAtivos.Count() > 0)
                {
                    Core.Models.TabuleiroNivelModel tabuleiroAtivo = tabuleirosNivelAtivos.FirstOrDefault();
                    //Obtem o tabuleiro que será exibido quando a pag for carregada
                    idTabuleiro = tabuleiroAtivo.TabuleiroID;
                    ViewBag.idTabuleiro = idTabuleiro;
                    if (idTabuleiro > 0)
                    {
                        tabuleiro = tabuleiroRepository.ObtemTabuleiro(idTabuleiro);
                        ViewBag.tabuleiro = tabuleiro;
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("login", "Account", new { strPopupTitle = "Erro", strPopupMessage = ex.Message, Sair = "true" });
            }

            return View();
        }

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
                RecursionLimit = 1000 //_listaRetorno.Count
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

        #endregion

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
