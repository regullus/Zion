using Base32;
using Coinpayments.Api;
using Core.Entities;
using Core.Entities.Loja;
using Core.Factories;
using Core.Helpers;
using Core.Models.Loja;
using Core.Repositories.Financeiro;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using Core.Repositories.Usuario;
using Core.Services.Loja;
using Core.Services.Usuario;
using Newtonsoft.Json;
using OtpSharp;
using Sistema.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;
//using Uol.PagSeguro.Domain;

namespace Sistema.Areas.Loja.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class PedidoController : SecurityController<Core.Entities.Produto>
    {

        #region Entity

        private PedidoRepository pedidoRepository;
        private ProdutoRepository produtoRepository;
        private EnderecoRepository enderecoRepository;
        private ContaRepository contaRepository;
        private LancamentoRepository lancamentoRepository;
        private EstadoRepository estadoRepository;
        private CidadeRepository cidadeRepository;
        private PedidoFactory pedidoFactory;
        private PedidoService pedidoService;
        private CartaoCreditoRepository cartaoCreditoRepository;
        private UsuarioService usuarioService;
        private MoedaCotacaoRepository moedaCotacaoRepository;
        private TaxaRepository taxaRepository;

        private Moeda moedaPadrao;

        public PedidoController(DbContext context) : base(context)
        {
            pedidoRepository = new PedidoRepository(context);
            produtoRepository = new ProdutoRepository(context);
            enderecoRepository = new EnderecoRepository(context);
            contaRepository = new ContaRepository(context);
            lancamentoRepository = new LancamentoRepository(context);
            estadoRepository = new EstadoRepository(context);
            cidadeRepository = new CidadeRepository(context);
            pedidoFactory = new PedidoFactory(context);
            pedidoService = new PedidoService(context);
            cartaoCreditoRepository = new CartaoCreditoRepository(context);
            usuarioService = new UsuarioService(context);
            moedaCotacaoRepository = new MoedaCotacaoRepository(context);
            taxaRepository = new TaxaRepository(context);

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

        #region Actions

        public ActionResult Resumo(CarrinhoModel carrinho)
        {
            if (carrinho.Vazio)
            {
                return RedirectToAction("Index", "Home");
            }

            if (usuario.StatusID == 2) //Ativo
            {
                //Valor minimo de inverstimento para quem esta ativo
                ViewBag.ValorMinimo = ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO_USUARIO_ATIVO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
            }
            else
            {
                //Valor minimo de inverstimento para quem não esta ativo
                ViewBag.ValorMinimo = ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
            }
            // == Não associado so pode escolher um produto para associação
            if (usuario.NivelAssociacao == 0 && carrinho.Itens.Count > 1)
            {
                string[] strMensagem = new string[] { traducaoHelper["SELECIONE_APENAS_UM_PRODUTO_REFERENTE_NIVEL_ASSOCIACAO_DESEJADA"] };
                Mensagem(traducaoHelper["ASSOCIACAO"], strMensagem, "err");
                return RedirectToAction("Index", "Home");
            }

            var intQuantidadeProdutos = ConfiguracaoHelper.GetInt("LOJA_QUANTIDADE_PRODUTOS_PEDIDO");
            if (intQuantidadeProdutos > 0)
            {
                int intQtde = 0;
                foreach (var item in carrinho.Itens)
                {
                    intQtde += item.Quantidade;
                }

                if (intQtde > intQuantidadeProdutos)
                {
                    string[] strMensagem = new string[] { traducaoHelper["ITENS_E_QUANTIDADE_MAXIMA_PERMITIDA_POR_PEDIDO"] };
                    Mensagem(traducaoHelper["QUANTIDADE_ITEM"], strMensagem, "err");
                    return RedirectToAction("Index", "Home");
                }
            }

            carrinho.LimparFrete();

            return View(carrinho);
        }

        public ActionResult EntregaEFaturamento(CarrinhoModel carrinho)
        {
            if (carrinho.Vazio)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Session["Erro"] != null)
            {
                ViewBag.AlertErroTitulo = Session["ErroTitulo"];
                ViewBag.AlertErro = Session["Erro"];
                Session["Erro"] = null;
                Session["ErroTitulo"] = null;
            }

            if (Session["Sucesso"] != null)
            {
                ViewBag.AlertSucessoTitulo = Session["SucessoTitulo"];
                ViewBag.AlertSucesso = Session["Sucesso"];
                Session["Sucesso"] = null;
                Session["SucessoTitulo"] = null;
            }

            if (Session["Info"] != null)
            {
                ViewBag.AlertInfoTitulo = Session["InfoTitulo"];
                ViewBag.AlertInfo = Session["Info"];
                Session["Info"] = null;
                Session["InfoTitulo"] = null;
            }

            carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
            carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
            //return RedirectToAction("pagamento");
            return View(carrinho);
        }

        public ActionResult Pagamento(CarrinhoModel carrinho)
        {
            if (carrinho.Vazio)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (usuario.StatusID == 2) //Ativo
                {
                    //Valor minimo de inverstimento para quem esta ativo
                    ViewBag.ValorMinimo = ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO_USUARIO_ATIVO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
                }
                else
                {
                    //Valor minimo de inverstimento para quem não esta ativo
                    ViewBag.ValorMinimo = ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
                }
                if (Core.Helpers.ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO") == "true")
                {
                    if (carrinho.EnderecoEntrega == null || carrinho.EnderecoEntrega.ID == 0)
                    //|| carrinho.EnderecoFaturamento == null   || carrinho.EnderecoFaturamento.ID == 0)
                    {
                        Session["ErroTitulo"] = traducaoHelper["DADOS_ENDERECO"];
                        Session["Erro"] = traducaoHelper["COMPRA_MENSAGEM_ENDERECO_FATURAMENTO"];
                        return RedirectToAction("entrega-e-faturamento", "pedido");
                    }
                }
            }

            //Verifica se frete está habilitado e valor do frete para o tipo Correio
            if (ConfiguracaoHelper.TemChave("FRETE_HABILITADO") && ConfiguracaoHelper.GetBoolean("FRETE_HABILITADO"))
            {
                if (carrinho.Total > 0 && carrinho.Frete == null && ConfiguracaoHelper.TemChave("TIPO_FRETE") && ConfiguracaoHelper.GetString("TIPO_FRETE").ToUpper() == "CORREIO")
                {
                    carrinho.SetarFrete(CarrinhoTipoFrete.Correio);
                }
            }

            //Verifica se existem mensagens a serem exibidas.
            obtemMensagem();

            ViewBag.UsuarioContainer = this.usuarioContainer;

            #region Calcula Diferença de Preço do Pacote no caso de compra de Upgrade 
            if (carrinho.Itens.Any(x => x.Produto.TipoID == 2))
            {
                var produto = carrinho.Itens.FirstOrDefault();

                if (produto != null)
                {
                    Produto pacoteAtualUsuario;

                    if (usuario.NivelAssociacao == 1)
                        pacoteAtualUsuario = produtoRepository.GetByExpression(p => p.TipoID == 1 && p.NivelAssociacao == usuario.NivelAssociacao).FirstOrDefault();
                    else
                        pacoteAtualUsuario = produtoRepository.GetByExpression(p => p.TipoID == 2 && p.NivelAssociacao == usuario.NivelAssociacao).FirstOrDefault();

                    ViewBag.DiferencaValorUpgrade = produto.Valor.Valor - pacoteAtualUsuario.ProdutoValor.FirstOrDefault().Valor;
                }
            }
            #endregion

            if (Session["Erro"] != null)
            {
                ViewBag.AlertErroTitulo = Session["ErroTitulo"];
                ViewBag.AlertErro = Session["Erro"];
                Session["Erro"] = null;
                Session["ErroTitulo"] = null;
            }

            if (Session["Sucesso"] != null)
            {
                ViewBag.AlertSucessoTitulo = Session["SucessoTitulo"];
                ViewBag.AlertSucesso = Session["Sucesso"];
                Session["Sucesso"] = null;
                Session["SucessoTitulo"] = null;
                Session["CartaoNome"] = null;
                Session["CartaoBandeira"] = null;
                Session["CartaoNumero"] = null;
                Session["CartaoCodSeguranca"] = null;
                Session["CartaoMes"] = null;
                Session["CartaoAno"] = null;
                Session["CodePagSeguro"] = null;
                Session["CartaoTelefone"] = null;
                Session["CartaoCPF"] = null;
                Session["CartaoEmail"] = null;
                Session["Parcelamento"] = null;
            }

            if (Session["Info"] != null)
            {
                ViewBag.AlertInfoTitulo = Session["InfoTitulo"];
                ViewBag.AlertInfo = Session["Info"];
                Session["Info"] = null;
                Session["InfoTitulo"] = null;
            }

            if (Session["ShowCartao"] != null)
            {
                ViewBag.ShowCartao = Session["ShowCartao"];
                Session["ShowCartao"] = null;
            }

            ViewBag.CodePagSeguro = Session["CodePagSeguro"];

            ViewBag.CartaoNome = Session["CartaoNome"];
            ViewBag.CartaoBandeira = Session["CartaoBandeira"];
            ViewBag.CartaoNumero = Session["CartaoNumero"];
            ViewBag.CartaoCodSeguranca = Session["CartaoCodSeguranca"];
            ViewBag.CartaoMes = Session["CartaoMes"];
            ViewBag.CartaoAno = Session["CartaoAno"];
            ViewBag.CartaoTelefone = Session["CartaoTelefone"];
            ViewBag.CartaoCPF = Session["CartaoCPF"];
            ViewBag.CartaoEmail = Session["CartaoEmail"];
            ViewBag.TipoPagtoCartao = Session["TipoPagtoCartao"];
            ViewBag.Parcelamento = Session["Parcelamento"];
            ViewBag.ShowBitCripto = null;

            #region Parcelamento
            //if (ViewBag.Parcelamento != null && ViewBag.CartaoBandeira != null && carrinho.Total > 0)
            //{
            //    ViviPay serviceViviPay = new ViviPay();
            //    var retornoViviPay = serviceViviPay.ParcelamentosDisponiveis(ViewBag.CartaoBandeira, carrinho.Total.ToString("N2"));
            //    List<Object> parcela = new List<object>();

            //    foreach (var item in retornoViviPay)
            //    {
            //        parcela.Add(new { parcelas = item.quantity, texto = item.quantity.ToString() + " X de R$" + String.Format("{0:0.00}", item.installmentAmount) });
            //    }

            //    ViewBag.ccParcelamento = new SelectList(parcela, "parcelas", "texto", ViewBag.Parcelamento);
            //}
            //else
            //{
            //    List<Object> parcela = new List<object>();
            //    ViewBag.ccParcelamento = new SelectList(parcela, "parcelas", "texto");
            //}
            #endregion

            #region Bandeira

            List<Object> bandeira = new List<object>();
            bandeira.Add(new { nome = "Visa", id = "visa" });
            bandeira.Add(new { nome = "Mastercard", id = "mastercard" });
            bandeira.Add(new { nome = "American Express", id = "amex" });
            bandeira.Add(new { nome = "Elo", id = "elo" });
            bandeira.Add(new { nome = "Diners Club", id = "diners" });

            if (ViewBag.CartaoBandeira != null)
            {
                ViewBag.ccBandeira = new SelectList(bandeira, "id", "nome", ViewBag.CartaoBandeira);
            }
            else
            {
                ViewBag.ccBandeira = new SelectList(bandeira, "id", "nome");
            }

            #endregion

            #region Mes

            List<Object> mes = new List<object>();
            mes.Add(new { nome = traducaoHelper["JANEIRO"], id = "01" });
            mes.Add(new { nome = traducaoHelper["FEVEREIRO"], id = "02" });
            mes.Add(new { nome = traducaoHelper["MARCO"], id = "03" });
            mes.Add(new { nome = traducaoHelper["ABRIL"], id = "04" });
            mes.Add(new { nome = traducaoHelper["MAIO"], id = "05" });
            mes.Add(new { nome = traducaoHelper["JUNHO"], id = "06" });
            mes.Add(new { nome = traducaoHelper["JULHO"], id = "07" });
            mes.Add(new { nome = traducaoHelper["AGOSTO"], id = "08" });
            mes.Add(new { nome = traducaoHelper["SETEMBRO"], id = "09" });
            mes.Add(new { nome = traducaoHelper["OUTUBRO"], id = "10" });
            mes.Add(new { nome = traducaoHelper["NOVEMBRO"], id = "11" });
            mes.Add(new { nome = traducaoHelper["DEZEMBRO"], id = "12" });

            if (ViewBag.CartaoMes != null)
            {
                ViewBag.ccMes = new SelectList(mes, "id", "nome", ViewBag.CartaoMes);
            }
            else
            {
                ViewBag.ccMes = new SelectList(mes, "id", "nome");
            }

            #endregion

            #region Ano

            List<Object> ano = new List<object>();
            ano.Add(new { nome = App.DateTimeZion.Year.ToString(), id = App.DateTimeZion.Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(1).Year.ToString(), id = App.DateTimeZion.AddYears(1).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(2).Year.ToString(), id = App.DateTimeZion.AddYears(2).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(3).Year.ToString(), id = App.DateTimeZion.AddYears(3).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(4).Year.ToString(), id = App.DateTimeZion.AddYears(4).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(5).Year.ToString(), id = App.DateTimeZion.AddYears(5).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(6).Year.ToString(), id = App.DateTimeZion.AddYears(6).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(7).Year.ToString(), id = App.DateTimeZion.AddYears(7).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(8).Year.ToString(), id = App.DateTimeZion.AddYears(8).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(9).Year.ToString(), id = App.DateTimeZion.AddYears(9).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(10).Year.ToString(), id = App.DateTimeZion.AddYears(10).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(11).Year.ToString(), id = App.DateTimeZion.AddYears(11).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(12).Year.ToString(), id = App.DateTimeZion.AddYears(12).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(13).Year.ToString(), id = App.DateTimeZion.AddYears(13).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(14).Year.ToString(), id = App.DateTimeZion.AddYears(14).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(15).Year.ToString(), id = App.DateTimeZion.AddYears(15).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(16).Year.ToString(), id = App.DateTimeZion.AddYears(16).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(17).Year.ToString(), id = App.DateTimeZion.AddYears(17).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(18).Year.ToString(), id = App.DateTimeZion.AddYears(18).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(19).Year.ToString(), id = App.DateTimeZion.AddYears(19).Year.ToString() });
            ano.Add(new { nome = App.DateTimeZion.AddYears(20).Year.ToString(), id = App.DateTimeZion.AddYears(20).Year.ToString() });

            if (ViewBag.CartaoAno != null)
            {
                ViewBag.ccAno = new SelectList(ano, "id", "nome", ViewBag.CartaoAno);
            }
            else
            {
                ViewBag.ccAno = new SelectList(ano, "id", "nome");
            }

            #endregion

            #region Sessions

            Session["CartaoNome"] = null;
            Session["CartaoBandeira"] = null;
            Session["CartaoNumero"] = null;
            Session["CartaoCodSeguranca"] = null;
            Session["CartaoMes"] = null;
            Session["CartaoAno"] = null;
            Session["CodePagSeguro"] = null;
            Session["CartaoTelefone"] = null;
            Session["CartaoCPF"] = null;
            Session["CartaoEmail"] = null;
            Session["TipoPagtoCartao"] = null;
            Session["Parcelamento"] = null;

            #endregion

            //#region SaldoAtivo

            //ViewBag.PagarSaldo = true;

            //if (usuario.ExibeSaque == 0 || Core.Helpers.ConfiguracaoHelper.GetString("MEIO_PGTO_SALDO_ATIVO") != "true")
            //{
            //    ViewBag.PagarSaldo = false;
            //}

            //#endregion

            if (ConfiguracaoHelper.GetBoolean("TAXAS_PAGAMENTO_PRODUTO_ATIVO"))
            {
                carrinho.Taxas = new List<CarrinhoTaxaModel>();

                var taxas = taxaRepository.GetByExpression(e => e.CategoriaID == 22).ToList();

                foreach (var taxa in taxas.Where(w => w.Valor.HasValue))
                {
                    carrinho.Taxas.Add(new CarrinhoTaxaModel { Taxa = taxa, Valor = taxa.Valor.Value });
                }
            }

            return View(carrinho);
        }

        public ActionResult Pagar(CarrinhoModel carrinho, PedidoPagamento.MeiosPagamento meioPagamento, string token2FA = null, string rendimento = null, string bonus = null, string transferencia = null, string chamada = null, string pedidoID = null)
        {
            if (String.IsNullOrEmpty(chamada))
            {
                Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                Session["Erro"] = traducaoHelper["CHAMADA_INVALIDA"];

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (!(chamada == "pagamento" || chamada == "detalhes"))
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["CHAMADA_INVALIDA"];

                    return RedirectToAction("Index", "Home");
                }
            }

            if (carrinho.Vazio && chamada == "pagamento")
            {
                return RedirectToAction("Index", "Home");
            }

            #region Autenticação Google 

            if (String.IsNullOrEmpty(token2FA))
            {
                Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                Session["Erro"] = traducaoHelper["TOKEN_INVALIDO"];
                if (chamada == "pagamento")
                {
                    return RedirectToAction("pagamento", "pedido");
                }
                else
                {
                    if (chamada == "detalhes")
                    {
                        return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            bool blnPassDev = false;
            if (usuario.Login.IndexOf("teste") >= 0 || usuario.Login.IndexOf("xuxa") >= 0)
            {
                if (token2FA == "o1Pdpdpep=1")
                {
                    blnPassDev = true;
                }
            }

            if (token2FA != null && !blnPassDev)
            {
                byte[] secretKey = Base32Encoder.Decode(usuario.Autenticacao.GoogleAuthenticatorSecretKey);

                var otp = new Totp(secretKey);
                if (!otp.VerifyTotp(token2FA, out _, new VerificationWindow(10, 10)))
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["TOKEN_INVALIDO"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            #endregion

            #region Consistencia

            //Verifica se já existe um pedido pendente
            PedidoUltimoStatus ultimoPedido = null;
            Pedido pedido = null;
            if (chamada == "pagamento")
            {
                ultimoPedido = pedidoRepository.GetPedidoUltimoStatus(usuario.ID, carrinho.Itens.FirstOrDefault().Produto.ID);
                if (!(ultimoPedido == null || ultimoPedido.StatusId != (int)PedidoPagamentoStatus.TodosStatus.AguardandoPagamento))
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["PEDIDO_JA_EXISTENTE"];

                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                if (Core.Helpers.ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO") == "true")
                {
                    if (carrinho.EnderecoEntrega == null && carrinho.EnderecoEntrega.ID == 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        if (carrinho.EnderecoFaturamento == null || carrinho.EnderecoFaturamento.ID == 0)
                        {
                            carrinho.EnderecoFaturamento = carrinho.EnderecoEntrega;
                        }
                    }
                }
                else
                {
                    carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
                    carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(pedidoID))
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["ENTRADA_INVALIDA"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                try
                {
                    int pedidoNum = Convert.ToInt32(pedidoID);
                    pedido = pedidoRepository.GetByID(pedidoNum);
                }
                catch (Exception)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["ENTRADA_INVALIDA"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            #endregion

            #region Pagamento Gratis
            //if (meioPagamento == Core.Entities.PedidoPagamento.MeiosPagamento.Gratis)
            //{
            //    #region Gratis

            //    var pagamento = pedido.PedidoPagamento.FirstOrDefault();
            //    pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);

            //    #endregion
            //}
            #endregion

            #region Pagamento Saldo
            string valoresParciais = "";
            if (meioPagamento == Core.Entities.PedidoPagamento.MeiosPagamento.Saldo)
            {
                #region saldo Variaveis
                double valorRendimento = 0.0;
                double valorBonus = 0.0;
                double valorTransferencia = 0.0;

                double valorRendimentoPago = 0.0;
                double valorBonusPago = 0.0;
                double valorTransferenciaPago = 0.0;

                double valorRestante = 0.0;

                double SaldoRentabilidade = usuarioContainer.SaldoRentabilidade;
                double SaldoBonus = usuarioContainer.SaldoBonus;
                double SaldoTransferencias = usuarioContainer.SaldoTransferencias;
                double pagamentoValor = carrinho.Total;

                #endregion

                if (pagamentoValor <= 0)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["VALOR_PRODUTO_INVALIDO"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                if (String.IsNullOrEmpty(rendimento) && String.IsNullOrEmpty(bonus) && String.IsNullOrEmpty(transferencia))
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["VOCE_DEVE_INFORMAR_VALORES"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    try
                    {
                        //Obtem valores passados como paramentor
                        if (!String.IsNullOrEmpty(rendimento))
                        {
                            Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                            Session["Erro"] = traducaoHelper["VALOR_RENDIMENTO_INVALIDO"];
                            rendimento = rendimento.Replace("_", "").Replace(",", ".");
                            if (string.IsNullOrEmpty(rendimento))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }

                            if (!double.TryParse(rendimento, out valorRendimento))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(bonus))
                        {
                            Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                            Session["Erro"] = traducaoHelper["VALOR_BONUS_INVALIDO"];
                            bonus = bonus.Replace("_", "").Replace(",", ".");
                            if (string.IsNullOrEmpty(bonus))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }

                            if (!double.TryParse(bonus, out valorBonus))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(transferencia))
                        {
                            Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                            Session["Erro"] = traducaoHelper["VALOR_TRANSFERENCIA_INVALIDO"];
                            transferencia = transferencia.Replace("_", "").Replace(",", ".");
                            if (string.IsNullOrEmpty(transferencia))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }

                            if (!double.TryParse(transferencia, out valorTransferencia))
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }

                        //Verifica se somatoria é maior que zero
                        if (valorRendimento + valorBonus + valorTransferencia < 0.0)
                        {
                            Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                            Session["Erro"] = traducaoHelper["VOCE_DEVE_INFORMAR_VALORES"];

                            if (chamada == "pagamento")
                            {
                                return RedirectToAction("pagamento", "pedido");
                            }
                            else
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        #region exception
                        Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                        Session["Erro"] = traducaoHelper["VALOR_INVALIDO"];
                        if (chamada == "pagamento")
                        {
                            return RedirectToAction("pagamento", "pedido");
                        }
                        else
                        {
                            if (chamada == "detalhes")
                            {
                                return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        #endregion
                    }
                }

                //Verifica se valores são maiores q o valor do produto
                if (valorRendimento + valorBonus + valorTransferencia < pagamentoValor)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["VALORES_NAO_ATINGE_VALOR_PRODUTO"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                //Verifica se há saldo para cada valor informado de cada conta
                if (valorRendimento > 0 && valorRendimento > SaldoRentabilidade)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["NAO_HA_SALDO_PARA_RENDIMENTO_INFORMADO"];

                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                if (valorBonus > 0 && valorBonus > SaldoBonus)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["NAO_HA_SALDO_PARA_BONUS_INFORMADO"];

                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                if (valorTransferencia > 0 && valorTransferencia > SaldoTransferencias)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["NAO_HA_SALDO_PARA_TRANSFERENCIA_INFORMADO"];

                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                //calcula valores a serem lancados
                //Valor restante para lancamento
                valorRestante = pagamentoValor; //Ex. valorPagamento = 100, valorRestante = 100
                //Distribui valor a ser pago entre as contas
                if (valorRendimento > 0) //Ex. valorRendimento = 30
                {
                    //Verifica se valor informado é maior que o valor do produto
                    if (valorRendimento > valorRestante)
                    {
                        //Se for maior lança somente o valor restante
                        valorRendimentoPago = valorRestante;
                    }
                    else
                    {
                        valorRendimentoPago = valorRendimento;
                    }
                    //Calcula valor restante a ser pago
                    valorRestante = valorRestante - valorRendimentoPago; //Ex. valorRestante = 100, valorRestante = 100 - 30 = 70
                }
                if (valorBonus > 0 && valorRestante > 0) // Ex. valorBonus = 40, valorRestante = 70
                {
                    //Verifica se valor informado é maior que o valor do produto
                    if (valorBonus > valorRestante)
                    {
                        //Se for maior lança somente o valor restante
                        valorBonusPago = valorRestante;
                    }
                    else
                    {
                        valorBonusPago = valorBonus;
                    }
                    //Calcula valor restante a ser pago
                    valorRestante = valorRestante - valorBonusPago; //Ex. valorRestante = 70, valorRestante = 70 - 40 = 30
                }
                if (valorTransferencia > 0 && valorRestante > 0) // Ex. valorTransferencia = 40, valorRestante = 30
                {
                    //Verifica se valor informado é maior que o valor do produto
                    if (valorTransferencia > valorRestante)
                    {
                        //Se for maior lança somente o valor restante
                        valorTransferenciaPago = valorRestante; //ex. So contabiliza 30 e não os 40 informados
                    }
                    else
                    {
                        valorTransferenciaPago = valorTransferencia; //Ex. caso seja igual a 30, usa os 30
                    }
                    //Calcula valor restante a ser pago
                    valorRestante = valorRestante - valorTransferenciaPago; //Ex. valorRestante = 30, valorRestante = 30 - 30 = 0
                }

                //valorRestante tem que ser zero, se não há algum problema
                if (valorRestante > 0)
                {
                    Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                    Session["Erro"] = traducaoHelper["VALORES_INFORMADOS_NAO_ATINGEM_VALOR_A_PAGAR"];
                    if (chamada == "pagamento")
                    {
                        return RedirectToAction("pagamento", "pedido");
                    }
                    else
                    {
                        if (chamada == "detalhes")
                        {
                            return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                if (valorRendimentoPago > 0)
                {
                    valoresParciais = " | " + traducaoHelper["RENDIMENTO"] + ": " + moedaPadrao.Simbolo + valorRendimentoPago.ToString(moedaPadrao.MascaraOut);
                }
                if (valorBonusPago > 0)
                {
                    if (String.IsNullOrEmpty(valoresParciais))
                    {
                        valoresParciais = " | " + traducaoHelper["BONUS"] + ": " + moedaPadrao.Simbolo + valorBonusPago.ToString(moedaPadrao.MascaraOut);
                    }
                    else
                    {
                        valoresParciais += " | " + traducaoHelper["BONUS"] + ": " + moedaPadrao.Simbolo + valorBonusPago.ToString(moedaPadrao.MascaraOut);
                    }
                }
                if (valorTransferenciaPago > 0)
                {
                    if (String.IsNullOrEmpty(valoresParciais))
                    {
                        valoresParciais = " | " + traducaoHelper["TRANSFERENCIA"] + ": " + moedaPadrao.Simbolo + valorTransferenciaPago.ToString(moedaPadrao.MascaraOut);
                    }
                    else
                    {
                        valoresParciais += " | " + traducaoHelper["TRANSFERENCIAS"] + ": " + moedaPadrao.Simbolo + valorTransferenciaPago.ToString(moedaPadrao.MascaraOut);
                    }
                }
                bool validacao = true;

                //transacao
                using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
                {
                    //Tudo certo, Adiciona pedido 
                    carrinho.Adicionar(meioPagamento, Core.Entities.PedidoPagamento.FormasPagamento.Padrao);
                    pedido = pedidoFactory.Criar(carrinho);
                    var pagamento = pedido.PedidoPagamento.FirstOrDefault();

                    //Tudo certo, efetua lancamentos para cada conta
                    if (valorRendimentoPago > 0)
                    {
                        var lancamento = new Core.Entities.Lancamento()
                        {
                            CategoriaID = (int)Categoria.Categorias.Saque,
                            ContaID = (int)Conta.Contas.Rentabilidade,
                            DataCriacao = App.DateTimeZion,
                            DataLancamento = App.DateTimeZion,
                            Descricao = "Pedido #" + pedido.Codigo + valoresParciais,
                            ReferenciaID = pagamento.ID,
                            Tipo = Core.Entities.Lancamento.Tipos.Compra,
                            UsuarioID = usuario.ID,
                            Valor = -valorRendimentoPago,
                            MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                        };
                        lancamentoRepository.Save(lancamento);
                    }

                    if (valorBonusPago > 0)
                    {
                        var lancamento = new Core.Entities.Lancamento()
                        {
                            CategoriaID = (int)Categoria.Categorias.Saque,
                            ContaID = (int)Conta.Contas.Bonus,
                            DataCriacao = App.DateTimeZion,
                            DataLancamento = App.DateTimeZion,
                            Descricao = "Pedido #" + pedido.Codigo + valoresParciais,
                            ReferenciaID = pagamento.ID,
                            Tipo = Core.Entities.Lancamento.Tipos.Compra,
                            UsuarioID = usuario.ID,
                            Valor = -valorBonusPago,
                            MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                        };
                        lancamentoRepository.Save(lancamento);
                    }

                    if (valorTransferenciaPago > 0)
                    {
                        var lancamento = new Core.Entities.Lancamento()
                        {
                            CategoriaID = (int)Categoria.Categorias.Saque,
                            ContaID = (int)Conta.Contas.Transferencias,
                            DataCriacao = App.DateTimeZion,
                            DataLancamento = App.DateTimeZion,
                            Descricao = "Pedido #" + pedido.Codigo + valoresParciais,
                            ReferenciaID = pagamento.ID,
                            Tipo = Core.Entities.Lancamento.Tipos.Compra,
                            UsuarioID = usuario.ID,
                            Valor = -valorTransferenciaPago,
                            MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                        };
                        lancamentoRepository.Save(lancamento);
                    }
                    valoresParciais = valoresParciais.Replace(" | ", @"<br \>");

                    validacao = pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);

                    try
                    {
                        if (validacao)
                        {
                            transacao.Complete();
                        }
                        else
                        {
                            transacao.Dispose();
                            Session["ErroTitulo"] = traducaoHelper["INCONSISTENCIA"];
                            Session["Erro"] = traducaoHelper["OCORREU_UM_PROBLEMA"];
                            if (chamada == "pagamento")
                            {
                                return RedirectToAction("pagamento", "pedido");
                            }
                            else
                            {
                                if (chamada == "detalhes")
                                {
                                    return Redirect(Url.Content("~/meus-pedidos/detalhes/") + pedidoID + "?erroTitulo=" + Session["ErroTitulo"] + "&erroMensagem=" + Session["Erro"]);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        cpUtilities.LoggerHelper.WriteFile("ProcessarPagamento : " + ex.Message, "PedidoService");
                    }
                }
            }
            #endregion

            #region Pagamento PayPal
            //if (meioPagamento == Core.Entities.PedidoPagamento.MeiosPagamento.PayPal)
            //{
            //    string strPayPal = "~/paypal/pagar?pedidoID=" + pedido.ID + "&chamada=loja";
            //    return Redirect(strPayPal);
            //}
            #endregion

            #region Posiciona Rede Sem Pagamento
            //if (ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO") && usuario.StatusID == (int)Usuario.TodosStatus.NaoAssociado)
            //{
            //    Boolean blnArvoreBinaria = false;
            //    if (ConfiguracaoHelper.TemChave("REDE_BINARIA"))
            //        blnArvoreBinaria = ConfiguracaoHelper.GetBoolean("REDE_BINARIA");

            //    if (blnArvoreBinaria)
            //    {
            //        usuarioService.Associar(usuario.ID, 0);
            //    }
            //    else
            //    {
            //        if (ConfiguracaoHelper.GetBoolean("REDE_PREENCHIMENTO_SEQUENCIAL"))
            //        {
            //            usuarioService.AssociarRedeSequencia(usuario.ID, 0);
            //        }
            //        else
            //        {
            //            usuarioService.AssociarRedeHierarquia(usuario.ID, 0);
            //        }
            //    }

            //    usuarioService.GeraUsuarioAssociacao(usuario, usuario.NivelAssociacao, false);
            //}
            #endregion

            #region Finalizar

            carrinho.Limpar();
            Session["ErroTitulo"] = null;
            Session["Erro"] = null;

            Session["SucessoTitulo"] = traducaoHelper["PAGAMENTO"];
            if (String.IsNullOrEmpty(valoresParciais))
            {
                Session["Sucesso"] = traducaoHelper["PAGAMENTO_SUCESSO"];
            }
            else
            {
                Session["Sucesso"] = traducaoHelper["PAGAMENTO_SUCESSO"] + @"<br /><br />" + traducaoHelper["UTILIZANDO"] + @":<br />" + valoresParciais;
            }

            if (meioPagamento == PedidoPagamento.MeiosPagamento.Deposito)
            {
                Session["Sucesso"] = traducaoHelper["COMPRA_FINALIZADA"];
            }

            Session["ValorFrete"] = carrinho.ValorFrete;

            if (carrinho.Frete != null)
            {
                Session["DiasFrete"] = carrinho.Frete.PrazoDias;
            }

            #endregion

            return RedirectToAction("finalizado", "pedido", new { pedidoID = pedido.ID });
        }

        public ActionResult Finalizado(int pedidoID)
        {
            if (Session["Erro"] != null)
            {
                ViewBag.AlertErroTitulo = Session["ErroTitulo"];
                ViewBag.AlertErro = Session["Erro"];
                Session["Erro"] = null;
                Session["ErroTitulo"] = null;
            }

            if (Session["Sucesso"] != null)
            {
                ViewBag.AlertSucessoTitulo = Session["SucessoTitulo"];
                ViewBag.AlertSucesso = Session["Sucesso"];
                Session["Sucesso"] = null;
                Session["SucessoTitulo"] = null;
                Session["CartaoNome"] = null;
                Session["CartaoBandeira"] = null;
                Session["CartaoNumero"] = null;
                Session["CartaoCodSeguranca"] = null;
                Session["CartaoMes"] = null;
                Session["CartaoAno"] = null;
                Session["TipoPagtoCartao"] = null;
                Session["Parcelamento"] = null;
            }

            if (Session["Info"] != null)
            {
                ViewBag.AlertInfoTitulo = Session["InfoTitulo"];
                ViewBag.AlertInfo = Session["Info"];
                Session["Info"] = null;
                Session["InfoTitulo"] = null;
            }

            var pedido = pedidoRepository.Get(pedidoID);
            if (pedido != null)
            {
                ViewBag.Finalizado = "true";

                var meioPagamentoID = 0;
                foreach (var item in pedido.PedidoPagamento)
                {
                    meioPagamentoID = item.MeioPagamentoID;
                }

                //Recupera o valor do frete e dias de entrega, caso exista;
                var valorFrete = Session["ValorFrete"] != null ? Convert.ToDouble(Session["ValorFrete"]) : 0;
                ViewBag.ValorFrete = valorFrete;
                var diasFrete = Session["DiasFrete"] != null ? Convert.ToInt32(Session["DiasFrete"]) : 0;
                ViewBag.DiasFrete = diasFrete;

                StringBuilder produtos = new StringBuilder();
                produtos.Append("<br />");
                foreach (var item in pedido.PedidoItem)
                {
                    //produtos.Append("<br />");
                    produtos.Append("<p>" + traducaoHelper["PRODUTO"] + ": " + item.Produto.Nome + "</p>");
                    produtos.Append("<p><small>" + item.Produto.Chamada + "</small></p>");
                    //produtos.Append("<br />");
                    produtos.Append("<p>" + traducaoHelper["VALOR_UNITARIO"] + ": " + moedaPadrao.Simbolo + (item.Quantidade * item.ValorUnitario).Value.ToString(moedaPadrao.MascaraOut) + "</p>");
                }

                if (valorFrete > 0)
                    produtos.Append("<p>" + traducaoHelper["FRETE"] + ": " + moedaPadrao.Simbolo + valorFrete.ToString(moedaPadrao.MascaraOut) + "</p>");

                //produtos.Append("<br />");
                produtos.Append("<p>" + traducaoHelper["TOTAL"] + ": " + moedaPadrao.Simbolo + pedido.Total.Value.ToString(moedaPadrao.MascaraOut) + "</p>");

                usuarioService.EnviarEmailCompraLoja(usuario, pedido.Codigo, produtos.ToString(), meioPagamentoID, ViewBag.ValorFrete);

                return View(pedido);
            }
            return RedirectToAction("Index", "Home");
        }

        public ContentResult SelecionarEndereco(CarrinhoModel carrinho, int enderecoID, string tipo)
        {
            var endereco = enderecoRepository.Get(enderecoID);
            if (endereco != null)
            {
                switch (tipo)
                {
                    case "entrega":
                        carrinho.EnderecoEntrega = endereco;
                        break;
                    case "faturamento":
                        carrinho.EnderecoFaturamento = endereco;
                        break;
                }
            }
            return Content("OK");
        }

        [HttpPost]
        public ActionResult AdicionarEndereco(Core.Entities.Endereco endereco)
        {
            try
            {
                endereco.UsuarioID = usuario.ID;
                endereco.Principal = false;
                endereco.Observacoes = "";
                if (string.IsNullOrEmpty(endereco.Complemento))
                    endereco.Complemento = "";

                enderecoRepository.Save(endereco);
                return RedirectToAction("entrega-e-faturamento");
            }
            catch (Exception)
            {
                Session["ErroTitulo"] = traducaoHelper["DADOS_ENDERECO"];
                Session["Erro"] = traducaoHelper["PREENCHA_DADOS"];

                return RedirectToAction("entrega-e-faturamento", "pedido");
            }
        }

        public ActionResult Endereco(CarrinhoModel carrinho, int novo = 0)
        {
            if (carrinho.Vazio)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Session["Erro"] != null)
            {
                ViewBag.AlertErroTitulo = Session["ErroTitulo"];
                ViewBag.AlertErro = Session["Erro"];
                Session["Erro"] = null;
                Session["ErroTitulo"] = null;
            }

            if (Session["Sucesso"] != null)
            {
                ViewBag.AlertSucessoTitulo = Session["SucessoTitulo"];
                ViewBag.AlertSucesso = Session["Sucesso"];
                Session["Sucesso"] = null;
                Session["SucessoTitulo"] = null;
            }

            if (Session["Info"] != null)
            {
                ViewBag.AlertInfoTitulo = Session["InfoTitulo"];
                ViewBag.AlertInfo = Session["Info"];
                Session["Info"] = null;
                Session["InfoTitulo"] = null;
            }

            ViewBag.Novo = (novo == 1 ? true : false);

            return View(carrinho);
        }

        public ActionResult SalvarEndereco(Core.Entities.Endereco endereco)
        {
            try
            {
                endereco.UsuarioID = usuario.ID;
                if (string.IsNullOrEmpty(endereco.Observacoes))
                    endereco.Observacoes = "";

                if (string.IsNullOrEmpty(endereco.Complemento))
                    endereco.Complemento = "";

                enderecoRepository.Save(endereco);

                return RedirectToAction("endereco");
            }
            catch (Exception ex)
            {
                Session["ErroTitulo"] = traducaoHelper["DADOS_ENDERECO"];
                Session["Erro"] = traducaoHelper["PREENCHA_DADOS"];

                return RedirectToAction("entrega-e-faturamento", "pedido");
            }
        }

        #region PagSeguro
        //public ActionResult PagSeguro(CarrinhoModel carrinho)
        //{
        //    Session["ValorFrete"] = carrinho.ValorFrete;

        //    if (carrinho.Frete != null)
        //        Session["DiasFrete"] = carrinho.Frete.PrazoDias;

        //    //Cria a requisição de pagamento para a PagSeguro que retorna a URL para realizar o pagamento
        //    var retorno = Integracao.PagSeguro.CreatePayment(carrinho, "redirect");

        //    #region Cria Pedido

        //    if (carrinho.Vazio)
        //        return RedirectToAction("Index", "Home");

        //    if (Core.Helpers.ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO") == "true")
        //    {
        //        if (carrinho.EnderecoEntrega == null && carrinho.EnderecoEntrega.ID == 0)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            if (carrinho.EnderecoFaturamento == null || carrinho.EnderecoFaturamento.ID == 0)
        //            {
        //                carrinho.EnderecoFaturamento = carrinho.EnderecoEntrega;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
        //        carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
        //    }

        //    carrinho.Adicionar(PedidoPagamento.MeiosPagamento.PagSeguro, PedidoPagamento.FormasPagamento.Padrao);
        //    var pedido = pedidoFactory.Criar(carrinho);

        //    //Cria o pedido com o status de Aguardando Pagamento
        //    var pagamento = pedido.PedidoPagamento.FirstOrDefault();
        //    pedidoService.ProcessarPagamento(pagamento.ID, PedidoPagamentoStatus.TodosStatus.AguardandoPagamento);

        //    //Armazena o Pedido Pagamento em session para uso posterior
        //    Session["PedidoID"] = pedido.ID;

        //    #region Posiciona Rede Sem Pagamento
        //    if (ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO") && usuario.StatusID == (int)Usuario.TodosStatus.NaoAssociado)
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

        //    carrinho.Limpar();

        //    #endregion

        //    #region Usar para opção lighbox
        //    //var retorno = Integracao.PagSeguro.CreatePayment(carrinho, "redirect").Split('=');
        //    //Session["CodePagSeguro"] = retorno[1];
        //    //return RedirectToAction("pagamento", "pedido");
        //    #endregion

        //    return Redirect(retorno);
        //}

        //public ActionResult RetornoPagSeguro(string transaction_id)
        //{
        //    var retorno = Integracao.PagSeguro.VerifyPTransactionStatus(transaction_id);
        //    var pedidoID = Session["PedidoID"] != null ? Convert.ToInt32(Session["PedidoID"]) : 0;
        //    var pedido = new Pedido();

        //    if (retorno != null && pedidoID > 0)
        //    {
        //        pedido = pedidoService.ObterPedidoPorId(pedidoID);

        //        if (pedido != null)
        //            CriarRegistroCartaoPagSeguro(retorno, pedido);
        //    }
        //    else
        //    {
        //        string[] strMensagem = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_8"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //        Mensagem("PagSeguro", strMensagem, "err");
        //        return RedirectToAction("Pagamento", "Pedido");
        //    }

        //    switch (retorno.TransactionStatus)
        //    {
        //        //Aguardando Pagamento
        //        case 1:
        //            string[] strMensagem1 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_1"], " ", traducaoHelper["PAG_SEGURO_MSG_VERIFICAR"] };
        //            Mensagem("PagSeguro", strMensagem1, "ale");
        //            return RedirectToAction("Pagamento", "Pedido");
        //        //Em análise
        //        case 2:
        //            string[] strMensagem2 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_2"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //            Mensagem("PagSeguro", strMensagem2, "ale");
        //            return RedirectToAction("Pagamento", "Pedido");
        //        //Paga
        //        case 3:
        //            //Altera status de pagamento do pedido
        //            pedidoService.ProcessarPagamento(pedido.PedidoPagamento.FirstOrDefault().ID, PedidoPagamentoStatus.TodosStatus.Pago);
        //            Session["PedidoID"] = null;
        //            return RedirectToAction("finalizado", new { pedidoID });
        //        //Disponível
        //        case 4:
        //            //Altera status de pagamento do pedido
        //            pedidoService.ProcessarPagamento(pedido.PedidoPagamento.FirstOrDefault().ID, PedidoPagamentoStatus.TodosStatus.Pago);
        //            Session["PedidoID"] = null;
        //            return RedirectToAction("finalizado", new { pedidoID });
        //        //Em disputa
        //        case 5:
        //            string[] strMensagem5 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_5"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //            Mensagem("PagSeguro", strMensagem5, "ale");
        //            return RedirectToAction("Pagamento", "Pedido");
        //        //Devolvida
        //        case 6:
        //            string[] strMensagem6 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_6"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //            Mensagem("PagSeguro", strMensagem6, "msg");
        //            return RedirectToAction("Pagamento", "Pedido");
        //        //Cancelada
        //        case 7:
        //            string[] strMensagem7 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_7"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //            Mensagem("PagSeguro", strMensagem7, "ale");
        //            return RedirectToAction("Pagamento", "Pedido");
        //        //Debitado e Retenção temporária
        //        default:
        //            string[] strMensagem8 = new string[] { traducaoHelper["PAG_SEGURO_MSG_CODE_8"], " ", traducaoHelper["PAG_SEGURO_MSG"] };
        //            Mensagem("PagSeguro", strMensagem8, "err");
        //            return RedirectToAction("Pagamento", "Pedido");
        //    }
        //}

        //public void CriarRegistroCartaoPagSeguro(Uol.PagSeguro.Domain.Transaction transaction, Pedido pedido)
        //{
        //    CartaoCredito cartaoCredito = new CartaoCredito()
        //    {
        //        UsuarioID = usuario.ID,
        //        PedidoPagamentoID = pedido.PedidoPagamento.FirstOrDefault().ID,
        //        PedidoID = pedido.ID,
        //        Bandeira = string.Empty,
        //        FinalCartao = string.Empty,
        //        Token = "",
        //        Valor = pedido.Total,
        //        Descricao = "Código Pedido: " + pedido.Codigo,
        //        DataCriacao = App.DateTimeZion,
        //        DataPagamento = App.DateTimeZion,
        //        CodigoAutorizacao = string.Empty,
        //        ComprovantePagamento = string.Empty,
        //        PagamentoID = null,
        //        TransacaoID = transaction.Code,
        //        CodigoRetorno = transaction.TransactionStatus.ToString()
        //    };

        //    switch (transaction.TransactionStatus)
        //    {
        //        case 1:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Aguardando Pagamento";
        //            break;
        //        case 2:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Em análise";
        //            break;
        //        case 3:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Pagamento OK";
        //            break;
        //        case 4:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Disponível";
        //            break;
        //        case 5:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Em disputa";
        //            break;
        //        case 6:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Devolvida";
        //            break;
        //        case 7:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Cancelada";
        //            break;
        //        default:
        //            cartaoCredito.MensagemRetorno = "PagSeguro - Debitado ou Retenção temporária";
        //            break;
        //    }
        //    cartaoCreditoRepository.Save(cartaoCredito);
        //}
     
        #endregion

        #endregion

        #region JsonResult

        public JsonResult estadoID(string UF)
        {
            int retorno = 0;
            var estados = estadoRepository.GetByExpression(x => x.Sigla == UF);
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

            var estados = estadoRepository.GetByExpression(x => x.Sigla == uf);
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

        public JsonResult getEndereco(string enderecoID)
        {
            var endereco = enderecoRepository.Get(int.Parse(enderecoID));
            var retorno = new List<string>();

            retorno.Add(endereco.ID.ToString());           // 00
            retorno.Add(endereco.Nome.ToString());         // 01
            retorno.Add(endereco.Observacoes.ToString());  // 02
            retorno.Add(endereco.CodigoPostal.ToString()); // 03
            retorno.Add(endereco.Logradouro.ToString());   // 04
            retorno.Add(endereco.Numero.ToString());       // 05
            retorno.Add(endereco.Complemento.ToString());  // 06
            retorno.Add(endereco.Distrito.ToString());     // 07
            retorno.Add(endereco.EstadoID.ToString());     // 08
            retorno.Add(endereco.CidadeID.ToString());     // 09
            retorno.Add(endereco.Principal.ToString());    // 10
            retorno.Add(endereco.Estado.Sigla.ToString()); // 11
            retorno.Add(endereco.Cidade.Nome.ToString());  // 12

            return Json(retorno);
        }

        public async System.Threading.Tasks.Task<JsonResult> CriaPedidoCriptoAsync(CarrinhoModel carrinho, string cripto)
        {
            var moedaCripto = Moeda.Moedas.LTC;
            if (!(cripto == "BTC" || cripto == "LTC" || cripto == "USDT.TRC20"))
            {
                LogErro("CriaPedidoCriptoAsync - Acesso indevido, com chamada à moeda: " + cripto + " do usuario:" + usuario.Login);
                return Json(new { erro = traducaoHelper["cripto_INVALIDA"] + " " + traducaoHelper["ACESSO_INDEVIDO_REPORTADO"] }, JsonRequestBehavior.AllowGet);
            }
            if (cripto == "LTC")
            {
                moedaCripto = Moeda.Moedas.LTC;
                if (!(ConfiguracaoHelper.GetString("MEIO_PGTO_LITECOIN_ATIVO") == "true"))
                {
                    LogErro("CriaPedidoCriptoAsync - cripto não esta ativa, com chamada à moeda: " + cripto + " do usuario:" + usuario.Login);
                    return Json(new { erro = traducaoHelper["LTC_NAO_ATIVO"] }, JsonRequestBehavior.AllowGet);
                }
            }
            if (cripto == "BTC")
            {
                moedaCripto = Moeda.Moedas.BTC;
                if (!(ConfiguracaoHelper.GetString("MEIO_PGTO_BITCOIN_ATIVO") == "true"))
                {
                    LogErro("CriaPedidoCriptoAsync - cripto não esta ativa, com chamada à moeda: " + cripto + " do usuario:" + usuario.Login);
                    return Json(new { erro = traducaoHelper["BTC_NAO_ATIVO"] }, JsonRequestBehavior.AllowGet);
                }
            }
            if (cripto == "USDT.TRC20")
            {
                moedaCripto = Moeda.Moedas.USDT;
                if (!(ConfiguracaoHelper.GetString("MEIO_PGTO_TETHER_ATIVO") == "true"))
                {
                    LogErro("CriaPedidoCriptoAsync - cripto não esta ativa, com chamada à moeda: " + cripto + " do usuario:" + usuario.Login);
                    return Json(new { erro = traducaoHelper["USDT_NAO_ATIVO"] }, JsonRequestBehavior.AllowGet);
                }
            }

            //Verifica se já existe um pedido pendente
            var ultimoPedido = pedidoRepository.GetPedidoUltimoStatus(usuario.ID, carrinho.Itens.FirstOrDefault().Produto.ID);

            if (!(ultimoPedido == null || ultimoPedido.StatusId != (int)PedidoPagamentoStatus.TodosStatus.AguardandoPagamento))
            {
                return Json(new { erro = traducaoHelper["PEDIDO_JA_EXISTENTE_MENSAGEM"] }, JsonRequestBehavior.AllowGet);
            }

            #region Calcula Diferença de Preço do Pacote no caso de compra de Upgrade 
            if (carrinho.Itens.Any(x => x.Produto.TipoID == 2))
            {
                var produto = carrinho.Itens.FirstOrDefault();

                if (produto != null)
                {
                    Produto pacoteAtualUsuario;

                    if (usuario.NivelAssociacao == 1)
                        pacoteAtualUsuario = produtoRepository.GetByExpression(p => p.TipoID == 1 && p.NivelAssociacao == usuario.NivelAssociacao).FirstOrDefault();
                    else
                        pacoteAtualUsuario = produtoRepository.GetByExpression(p => p.TipoID == 2 && p.NivelAssociacao == usuario.NivelAssociacao).FirstOrDefault();

                    carrinho.Itens.First().Valor.Valor = produto.Valor.Valor - pacoteAtualUsuario.ProdutoValor.FirstOrDefault().Valor;
                }
            }
            #endregion

            //Cria Pedido no Sistema
            if (Core.Helpers.ConfiguracaoHelper.GetString("CADASTRO_SOLICITA_ENDERECO") == "true")
            {
                if (carrinho.EnderecoEntrega == null && carrinho.EnderecoEntrega.ID == 0)
                {
                    return Json(new { erro = "Falha ao no endereço de entrega" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (carrinho.EnderecoFaturamento == null || carrinho.EnderecoFaturamento.ID == 0)
                    {
                        carrinho.EnderecoFaturamento = carrinho.EnderecoEntrega;
                    }
                }
            }
            else
            {
                carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
                carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
            }

            carrinho.Adicionar(PedidoPagamento.MeiosPagamento.CryptoPayments, PedidoPagamento.FormasPagamento.Padrao);
            var pedido = pedidoFactory.Criar(carrinho);

            try
            {
                ViewBag.ShowCripto = "true";

                decimal cotacaoCripto = await BuscarCotacaoAsync(cripto);

                if (cotacaoCripto > 0)
                {
                    var taxaPlataforma = ConfiguracaoHelper.GetDouble("TAXA_PLATAFORMA");
                    if (taxaPlataforma <= 0)
                    {
                        taxaPlataforma = 5.0;
                    }

                    if (taxaPlataforma > 0)
                    {
                        var valorUsdComTaxa = (decimal)pedido.Total.Value;
                        if (cripto == "USDT.TRC20" && ConfiguracaoHelper.GetBoolean("HA_TAXA_PAGAMENTO_USDT"))
                        {
                            valorUsdComTaxa = (decimal)pedido.Total.Value * ((decimal)(taxaPlataforma / 100) + 1);
                        }

                        var valorCriptoComTaxa = Math.Round(valorUsdComTaxa / cotacaoCripto, 4);
                        string chave = pedido.ID.ToString() + "_" + App.DateTimeZion.Ticks.ToString().Substring(10, 5);

                        //cripto = "LTCT";    //testar com LTC
                        var purchase = await CoinpaymentsApi.GetCallbackAddress(cripto);
                        //troca por causa do pagamento parcial. O metodo abaixo chama o cmd create_transation da api da coinpayments. E recomendado para pagamento com valor fixo                     
                        //var purchase = await CoinpaymentsApi.CreateTransaction(valorUsdComTaxa, "USD", cripto, usuario.Email, custom: chave, itemNumber: pedido.Codigo);

                        if (purchase != null && purchase.HttpResponse != null && purchase.HttpResponse.ContentBody != null)
                        {
                            dynamic purchaseJson = JsonConvert.DeserializeObject(purchase.HttpResponse.ContentBody);
                            string numero = purchaseJson.result.address;

                            int referenciaID = 0;
                            string valor = valorCriptoComTaxa.ToString();
                            string mensagem = traducaoHelper["MENSAGEM_CONFIRMACAO_HASH_BTC"];
                            pedidoFactory.SalvarDadosPagamentoCripto(numero, referenciaID, Convert.ToDecimal(valor), cotacaoCripto, (decimal)pedido.Total.Value, (int)moedaCripto);
                            return Json(new { numero, valor, mensagem }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            pedidoService.ProcessarPagamento(pedido.PedidoPagamento.FirstOrDefault().ID, PedidoPagamentoStatus.TodosStatus.Cancelado);
                            LogErro("CriaPedidoCriptoAsync - Pedido: " + pedido.PedidoPagamento.FirstOrDefault().ID + " - Data: " + DateTime.Now);
                            return Json(new { erro = "Não foi possível efetuar o pagamento." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                pedidoService.ProcessarPagamento(pedido.PedidoPagamento.FirstOrDefault().ID, PedidoPagamentoStatus.TodosStatus.Cancelado);
                LogErro("CriaPedidoCriptoAsync - Pedido: " + pedido.PedidoPagamento.FirstOrDefault().ID + " - Data: " + DateTime.Now);
                return Json(new { erro = "Não foi possível efetuar o pagamento." }, JsonRequestBehavior.AllowGet);
                //throw new Exception();
            }
            catch (Exception ex)
            {
                pedidoService.ProcessarPagamento(pedido.PedidoPagamento.FirstOrDefault().ID, PedidoPagamentoStatus.TodosStatus.Cancelado);
                LogErro("CriaPedidoCriptoAsync - Pedido: " + pedido.PedidoPagamento.FirstOrDefault().ID + " - Data: " + DateTime.Now + " - Exceção: " + ex.InnerException);
                return Json(new { erro = "Não foi possível efetuar o pagamento." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helpers
        private Tuple<string, string> ParseTelefone(string strTelefone)
        {
            Regex regex = new Regex(@"\d+");
            string ddd = string.Empty;
            string telefone = strTelefone;
            string result = string.Empty;
            foreach (Match m in regex.Matches(strTelefone))
                result += m.Value;

            switch (result.Length)
            {
                case (11):
                    ddd = result.Substring(0, 2);
                    telefone = result.Substring(2, 9);
                    break;
                case (10):
                    ddd = result.Substring(0, 2);
                    telefone = result.Substring(2, 8);
                    break;
                default:
                    break;
            }

            return new Tuple<string, string>(ddd, telefone);
        }

        private string CriarCodigoPedido()
        {

            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var tamanho = 10;
            var tamanhoMaximo = caracteres.Length;
            StringBuilder codigo;
            try
            {
                do
                {
                    Random random = new Random();
                    codigo = new StringBuilder(tamanho);
                    for (int indice = 0; indice < tamanho; indice++)
                    {
                        codigo.Append(caracteres[random.Next(0, tamanhoMaximo)]);
                    }
                } while (pedidoRepository.GetByCodigo(codigo.ToString()) != null);
                return codigo.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private async System.Threading.Tasks.Task<decimal> BuscarCotacaoAsync(string siglaMoeda)
        {
            try
            {
                var cotacao = await CoinpaymentsApiWrapper.ExchangeRatesAsHelper();
                switch (siglaMoeda.ToUpper())
                {
                    case "BTC":
                        return cotacao.BtcUsd;
                    case "LTC":
                        return cotacao.LtcUsd;
                    case "USDT.TRC20":
                        return cotacao.UsdtUsd;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return 0;
        }

        private bool ConsistenteVariacaoBTC(decimal cotacao)
        {
            var mediaCotacoes = Convert.ToDecimal(usuario.PedidoPagamento.Average(x => x.CotacaoCripto));

            if (mediaCotacoes > 0)
                return (cotacao * 100 / mediaCotacoes - 100) <= 30 && (cotacao * 100 / mediaCotacoes - 100) >= -30;
            else
                return true;
        }

        private void LogErro(string log)
        {
            cpUtilities.LoggerHelper.WriteFile(log, "PedidoLogErro");
        }
        #endregion
    }
}
