#region Bibliotecas

using Core.Entities;
using Core.Factories;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Rede;
using Core.Repositories.Usuario;
using Core.Services.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Repositories.Sistema;
using Core.Helpers;
using System.IO;
using System.Configuration;
using Helpers;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class UsuarioController : SecurityController<Core.Entities.Usuario>
    {

        #region Variaveis

        #endregion

        #region Core

        UsuarioRepository usuarioRepository;
        AssociacaoRepository associacaoRepository;
        ClassificacaoRepository classificacaoRepository;
        LancamentoRepository lancamentoRepository;
        ProdutoRepository produtoRepository;
        PedidoFactory pedidoFactory;
        PedidoService pedidoService;
        ConfiguracaoRepository configuracaoRepository;

        private Moeda moedaPadrao = new Moeda();

        public UsuarioController(DbContext context)
         : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            classificacaoRepository = new ClassificacaoRepository(context);
            lancamentoRepository = new LancamentoRepository(context);
            produtoRepository = new ProdutoRepository(context);
            pedidoFactory = new PedidoFactory(context);
            pedidoService = new PedidoService(context);
            configuracaoRepository = new ConfiguracaoRepository(context);

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
        #endregion

        #region Actions

        public ActionResult Idioma(int id)
        {
            var idioma = idiomas.FirstOrDefault(i => i.ID == id);
            Session["Idioma"] = idioma;
            return Content("OK");
        }

        public ActionResult Buscar(string q)
        {
            if (String.IsNullOrEmpty(q))
            {
                ViewBag.Resultado = true;
                return View();
            }
            ViewBag.Termo = q;
            ViewBag.Associacoes = associacaoRepository.GetAll();

            IEnumerable<Core.Entities.Usuario> franqueados;

            if (Core.Helpers.ConfiguracaoHelper.GetString("BUSCAR_POR_ASSINATURA") == "true")
            {
                franqueados = usuarioRepository.BuscarAssinatura(q, usuario.Assinatura, false, false);
            }
            else
            {
                franqueados = usuarioRepository.Buscar(q, false, false);
            }

            List<Sistema.Containers.UsuarioContainer> usuarios = new List<Containers.UsuarioContainer>();

            bool blnResultado = false;
            foreach (var f in franqueados)
            {
                blnResultado = true;
                usuarios.Add(new Containers.UsuarioContainer(f));
            }

            ViewBag.Resultado = blnResultado;

            return View(usuarios);
        }

        public ActionResult Detalhes(int? id)
        {

            if (id == null)
            {
                //Caso não seja passado paramento do usuario
                return RedirectToAction("Index", "Home");
            }
            int idLocal = id ?? 0;
            try
            {
                var franqueadoAlvo = this.repository.Get(idLocal);
                ViewBag.Saldo = moedaPadrao.Simbolo + this.usuarioContainer.Saldo.ToString(moedaPadrao.MascaraOut);

                ViewBag.Produtos = produtoRepository.GetDisponiveis();

                //if (usuario.Status == Core.Entities.Usuario.TodosStatus.NaoAssociado)
                //{
                //   //ToDo - Alterar para trazer somente somente os produtos acima do que o usuario tem atualmente
                //   ViewBag.Produtos = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao);
                //}
                //else
                //{
                //   ViewBag.Produtos = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Upgrade);
                //}
                ViewBag.Associacao = associacaoRepository.GetByNivel(franqueadoAlvo.NivelAssociacao).Nome;
                ViewBag.Classificacao = classificacaoRepository.GetByNivel(franqueadoAlvo.NivelClassificacao).Nome;
                var franqueadoAlvoContainer = new Containers.UsuarioContainer(franqueadoAlvo);

                #region SaldoAtivo

                ViewBag.AtivarSaldo = true;

                if (usuario.ExibeSaque == 0 || Core.Helpers.ConfiguracaoHelper.GetString("MEIO_PGTO_SALDO_ATIVO") != "true" || franqueadoAlvo.Status == Core.Entities.Usuario.TodosStatus.Associado)
                {
                    ViewBag.AtivarSaldo = false;
                }

                #endregion

                return View(franqueadoAlvoContainer);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public JsonResult Comprar(int franqueadoID, int produtoID)
        {
            try
            {
                int valorTaxa = 0;
                if (!String.IsNullOrEmpty(Core.Helpers.ConfiguracaoHelper.GetString("TRANSFERENCIA_VALOR_TAXA")))
                {
                    valorTaxa = int.Parse(Core.Helpers.ConfiguracaoHelper.GetString("TRANSFERENCIA_VALOR_TAXA"));
                }

                var franqueado = this.repository.Get(franqueadoID);

                var carrinho = new Core.Models.Loja.CarrinhoModel(franqueado);
                var produto = produtoRepository.Get(produtoID);
                var valor = produto.ValorMinimo(franqueado);

                if (!produto.Publicado || valor == null)
                {
                    return Json(traducaoHelper["PRODUTO_INDISPONIVEL"]);
                }

                carrinho.EnderecoEntrega = franqueado.EnderecoPrincipal;
                carrinho.EnderecoFaturamento = franqueado.EnderecoPrincipal;
                carrinho.Adicionar(produto, valor);
                carrinho.Adicionar(PedidoPagamento.MeiosPagamento.Saldo, PedidoPagamento.FormasPagamento.Padrao);

                if (usuarioContainer.Saldo < (double)(valor.Valor + valorTaxa))
                {
                    return Json(traducaoHelper["SALDO_INSUFICIENTE"]);
                }

                var debito = new Lancamento()
                {
                    CategoriaID = 8, //CategoriaID = 8 é Ativacao - Tabela Finaceiro.Categoria
                    ContaID = 1,
                    DataCriacao = App.DateTimeZion,
                    DataLancamento = App.DateTimeZion,
                    UsuarioID = usuario.ID,
                    Descricao = traducaoHelper["ATIVACAO_COM_SALDO_PARA"] + franqueado.Login,
                    ReferenciaID = franqueado.ID,
                    Tipo = Lancamento.Tipos.Ativacao,
                    Valor = (double)-valor.Valor,
                    MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                };
                lancamentoRepository.Save(debito);

                if (valorTaxa > 0)
                {
                    var taxa = new Lancamento()
                    {
                        CategoriaID = 19, //CategoriaID = 19 é Taxa de Ativação - Tabela Finaceiro.Categoria
                        ContaID = 1,
                        DataCriacao = App.DateTimeZion,
                        DataLancamento = App.DateTimeZion,
                        UsuarioID = usuario.ID,
                        Descricao = traducaoHelper["TAXA_ATIVACAO"] + franqueado.Login + ")",
                        ReferenciaID = debito.ID,
                        Tipo = Lancamento.Tipos.Taxa,
                        Valor = -valorTaxa,
                        MoedaIDCripto = (int)Moeda.Moedas.NEN, //Nenhum
                    };
                    lancamentoRepository.Save(taxa);
                }

                var pedido = pedidoFactory.Criar(carrinho);
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                bool ret = pedidoService.ProcessarPagamento(pagamento.ID, PedidoPagamentoStatus.TodosStatus.Pago);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
            return Json("OK");
        }

        [HttpPost]
        public ActionResult Transferir(int id, int valor)
        {
            try
            {
                var franqueadoTransferir = this.repository.Get(id);
                this.Transferir(usuario, franqueadoTransferir, valor);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("OK");
        }

        public void Transferir(Usuario de, Usuario para, double valor)
        {
            valor = valor / 100;

            //Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");

            double valorMinimo = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_MINIMO");
            double valorMinimoBeneficiario = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_MINIMO_BENEFICIARIO");
            double valorTaxa = Core.Helpers.ConfiguracaoHelper.GetDouble("TRANSFERENCIA_VALOR_TAXA");

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

            var saldo = usuarioContainer.Saldo;

            if ((valor + valorTaxa) > saldo)
            {
                throw new Exception(traducaoHelper["SALDO_INSUFICIENTE"]);
            }

            var saldoBeneficiario = usuarioRepository.Saldo(para.ID);

            if (saldoBeneficiario < valorMinimoBeneficiario)
            {
                throw new Exception(traducaoHelper["SALDO_INSUFICIENTE_BENEFICIARIO"]);
            }

            var debito = new Lancamento()
            {
                CategoriaID = (int)Categoria.Categorias.Transferencia, //CategoriaID = 7 é Transferencia - Tabela Finaceiro.Categoria
                ContaID = (int)Conta.Contas.Rentabilidade, //Transferencia - Debito do usuario DE
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
                    ContaID = (int)Conta.Contas.Rentabilidade, //Taxa cobrada do usuario DE
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
                CategoriaID = 7, //CategoriaID = 7 é Transferencia - Tabela Finaceiro.Categoria
                ContaID = 7, //Transferencia credito para usuario PARA
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
    }
}
