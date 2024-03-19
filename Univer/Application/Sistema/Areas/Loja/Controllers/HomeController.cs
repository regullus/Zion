using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Usuario;
using Core.Repositories.Rede;
using Sistema.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema.Areas.Loja.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class HomeController : SecurityController<Core.Entities.Produto>
    {

        private ProdutoRepository produtoRepository;
        private UsuarioGanhoRepository usuarioGanhoRepository;
        private AssociacaoLimiteGanhoRepository associacaoLimiteGanhoRepository;
        private LancamentoRepository lancamentoRepository;

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

        public HomeController(DbContext context)
            : base(context)
        {
            produtoRepository = new ProdutoRepository(context);
            usuarioGanhoRepository = new UsuarioGanhoRepository(context);
            associacaoLimiteGanhoRepository = new AssociacaoLimiteGanhoRepository(context);
            lancamentoRepository = new LancamentoRepository(context);
        }

        public ActionResult Index(string tipo, bool novoAssociado = false)
        {
            obtemMensagem();
            if (ConfiguracaoHelper.GetBoolean("PRODUTO_VALOR_VARIAVEL"))
            {
                double total = 0.0;

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

                if (usuario.Pedido.Any())
                {
                    foreach (var pedido in usuario.Pedido.OrderByDescending(p => p.DataCriacao))
                    {
                        if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago)
                        {
                            total += pedido.PedidoPagamento.Where(w => w.Valor.HasValue).Sum(s => s.Valor).Value;
                        }
                    }
                }
                ViewBag.ValorAtual = total;

                if (usuario.NivelAssociacao == 0)
                {
                    ViewBag.BoasVindas = true;
                    ViewBag.BemVindo = traducaoHelper["BEM_VINDO"];
                    ViewBag.Obrigado = traducaoHelper["OBRIGADO_INSCRICAO"];
                    ViewBag.CorpoPopUp = traducaoHelper["ADQUIRIR_PRODUTO_ASSOCIACAO"];
                }
                else
                {
                    ViewBag.BoasVindas = false;
                }
                ViewBag.Produtos = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao).OrderBy(Prd => Prd.Peso);
            }
            else
            {
                if (novoAssociado)
                {
                    ViewBag.BoasVindas = true;
                    ViewBag.BemVindo = traducaoHelper["BEM_VINDO"];
                    ViewBag.Obrigado = traducaoHelper["OBRIGADO_INSCRICAO"];
                    ViewBag.CorpoPopUp = traducaoHelper["ADQUIRIR_PRODUTO_ASSOCIACAO"];
                }
                else
                {
                    ViewBag.BoasVindas = false;
                }
                switch (tipo)
                {
                    case "ativo":
                        if (usuario.NivelAssociacao > 0)
                        {
                            ViewBag.Upgrades = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Upgrade).OrderBy(Prd => Prd.Peso).Select(prd => prd.NivelAssociacao > usuario.NivelAssociacao);
                            if (usuario.NivelAssociacao > 1)
                            {
                                ViewBag.Produtos = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.AtivoMensal).OrderBy(Prd => Prd.SKU).Select(prd => prd.NivelAssociacao <= usuario.NivelAssociacao);
                            }
                        }
                        else
                        {
                            ViewBag.Ativacao = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao).OrderBy(Prd => Prd.Peso);
                        }
                        break;
                    default:
                        if (usuario.Status == Core.Entities.Usuario.TodosStatus.NaoAssociado || usuario.NivelAssociacao == 0)
                        {
                            ViewBag.Ativacao = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao).OrderBy(Prd => Prd.Peso);
                            break;
                        }
                        else
                        {
                            if (usuario.NivelAssociacao > 0)
                            {
                                if (ConfiguracaoHelper.GetBoolean("REDE_CONTROLA_GANHO_ASSOCIACAO"))
                                {
                                    if (ConfiguracaoHelper.GetBoolean("REDE_RENOVACAO_ASSOCIACAO_POSSUI"))
                                    {
                                        string strDiasRenovacao = ConfiguracaoHelper.GetString("REDE_RENOVACAO_ASSOCIACAO_DIAS_ALERTA");
                                        if (usuario.DataRenovacao <= App.DateTimeZion.AddDays(int.Parse(strDiasRenovacao == null ? strDiasRenovacao : "0")))
                                        {
                                            ViewBag.Renovacao = produtoRepository.GetRenovacao(usuario.NivelAssociacao).OrderBy(Prd => Prd.Peso);
                                            break;
                                        }
                                        else
                                        {
                                            var usuarioGanho = usuario.UsuarioGanho.OrderByDescending(g => g.ID).FirstOrDefault();
                                            var LimiteGanho = associacaoLimiteGanhoRepository.GetByTipo(Core.Entities.AssociacaoLimiteGanho.Tipos.Associacao, usuario.NivelAssociacao);
                                            if (usuarioGanho != null && LimiteGanho != null && usuarioGanho.AcumuladoGanho >= LimiteGanho.Valor)
                                            {
                                                ViewBag.Renovacao = produtoRepository.GetRenovacao(usuario.NivelAssociacao).OrderBy(Prd => Prd.Peso);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var usuarioGanho = usuarioGanhoRepository.GetAtual(usuario.ID);
                                        var somaLancamentos = lancamentoRepository.GetByExpression(x => x.UsuarioID == usuario.ID &&
                                                                                                        x.Categoria.TipoID == 4 &&
                                                                                                        x.DataLancamento >= usuarioGanho.DataInicio &&
                                                                                                        x.DataLancamento <= usuarioGanho.DataFim).Sum(s => s.ValorCripto);
                                        somaLancamentos = somaLancamentos != null ? somaLancamentos : 0;
                                        double valorMigracao = 0;
                                        if (usuario.Complemento != null && usuarioGanho.Indicador == 0) // Tem Complemento e não teve renovação (Indicador == 0) 
                                        {
                                            valorMigracao = usuario.Complemento.MaximoGanhos.HasValue ? double.Parse(usuario.Complemento.MaximoGanhos.Value.ToString()) : 0;
                                        }
                                        var LimiteGanho = somaLancamentos * ConfiguracaoHelper.GetInt("FATOR_MULTIPLICADOR_TETO") + valorMigracao;

                                        if (usuarioGanho != null && LimiteGanho != null && usuarioGanho.DataAtingiuLimite.HasValue)
                                        //if (usuarioGanho != null && LimiteGanho != null && usuarioGanho.AcumuladoGanho >= LimiteGanho)
                                        {
                                            ViewBag.Renovacao = produtoRepository.GetRenovacao(usuario.NivelAssociacao).OrderBy(Prd => Prd.Peso);
                                            break;
                                        }
                                    }
                                }
                            }

                            if (ConfiguracaoHelper.GetBoolean("REDE_ATIVO_MENSAL_POSSUI"))
                            {
                                string strDiasAtivoMensal = ConfiguracaoHelper.GetString("REDE_ATIVO_MENSAL_DIAS_ALERTA");
                                if (usuario.DataValidade <= App.DateTimeZion.AddDays(int.Parse(strDiasAtivoMensal != null ? strDiasAtivoMensal : "0")))
                                {
                                    ViewBag.Ativacao = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.AtivoMensal).OrderBy(Prd => Prd.Peso);
                                    if (ConfiguracaoHelper.GetBoolean("REDE_ATIVO_MENSAL_EXCLUSIVO"))
                                    {
                                        break;
                                    }
                                }
                            }

                            if (usuario.NivelAssociacao > 0)
                            {
                                //ViewBag.Upgrades = produtoRepository.GetUpgrade(usuario.NivelAssociacao).OrderBy(Prd => Prd.Peso);
                                ViewBag.Ativacao = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao).OrderBy(Prd => Prd.Peso);
                            }
                        }

                        if (usuario.NivelAssociacao > 0)
                        {
                            var pacotesComplementaresAdquiridos = usuario.Pedido.SelectMany(x => x.PedidoItem).Where(x => x.Produto.Tipo == Core.Entities.Produto.Tipos.ProdutoVirtual);
                            var somaComplementares = pacotesComplementaresAdquiridos.SelectMany(x => x.Produto.ProdutoValor);
                            ViewBag.Produtos = produtoRepository.GetPacotesComplmentares(usuario.NivelAssociacao).OrderBy(Prd => Prd.Peso);
                        }
                        break;
                }

            }
            return View();
        }

        public ActionResult Index2()
        {
            if (usuario.Status == Core.Entities.Usuario.TodosStatus.NaoAssociado)
            {
                ViewBag.Upgrades = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.Associacao);
            }
            else
            {
                ViewBag.Upgrades = produtoRepository.GetByTipo(Core.Entities.Produto.Tipos.ProdutoFisico);
            }

            ViewBag.Produtos = produtoRepository.GetProdutos();
            return View();
        }

    }
}
