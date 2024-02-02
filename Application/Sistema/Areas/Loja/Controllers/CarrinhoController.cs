using Core.Entities;
using Core.Models.Loja;
using Core.Repositories.Loja;
using Sistema.Controllers;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Core.Helpers;

namespace Sistema.Areas.Loja.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class CarrinhoController : SecurityController<Core.Entities.Produto>
    {

        private ProdutoRepository produtoRepository;
        private PedidoRepository pedidoRepository;

        public CarrinhoController(DbContext context)
            : base(context)
        {
            produtoRepository = new ProdutoRepository(context);
            pedidoRepository = new PedidoRepository(context);
        }

        public ActionResult Index(CarrinhoModel carrinho)
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
            if (carrinho.Itens.Any())
            {
                return View(carrinho);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult Adicionar(CarrinhoModel carrinho, int id)
        {
            var produto = produtoRepository.Get(id);

            var prosseguir = false;

            if (produto != null)
            {
                var ultimoPedido = pedidoRepository.GetPedidoUltimoStatus(usuario.ID, id);

                if (ultimoPedido == null || ultimoPedido.StatusId != (int)PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
                {
                    prosseguir = true;

                    //if (produto.Tipo == Core.Entities.Produto.Tipos.Upgrade)
                    //{
                    //    var upgrade = carrinho.Itens.FirstOrDefault(i => i.Produto.Tipo == Core.Entities.Produto.Tipos.Upgrade);
                    //    if (upgrade != null)
                    //    {
                    //        carrinho.Remover(upgrade.Produto.ID);
                    //    }
                    //}
                    //else if (produto.Tipo == Core.Entities.Produto.Tipos.ProdutoVirtual)
                    //{
                    //    var produtoVirtual = carrinho.Itens.FirstOrDefault(i => i.Produto.Tipo == Core.Entities.Produto.Tipos.ProdutoVirtual);
                    //    if (produtoVirtual != null)
                    //    {
                    //        carrinho.Remover(produtoVirtual.Produto.ID);
                    //    }
                    //}
                    //else
                    //{
                    //    var assoc = carrinho.Itens.FirstOrDefault(i => i.Produto.Tipo == Core.Entities.Produto.Tipos.Associacao);
                    //    if (assoc != null)
                    //    {
                    //        carrinho.Remover(assoc.Produto.ID);
                    //    }
                    //}

                    carrinho.Resetar();
                    carrinho.Adicionar(produto, produto.ValorMinimo(usuario));
                }
            }

            return Json(new { ok = prosseguir });
        }

        public JsonResult AdicionarValor(CarrinhoModel carrinho, int id, string valor)
        {
            var produto = produtoRepository.Get(id);
            string strMensagem = "";
            var prosseguir = false;
            valor = valor.Replace("_", "").Replace(",", ".");
            if (string.IsNullOrEmpty(valor))
            {
                return Json(new { valorInvalido = true, msg = traducaoHelper["VALOR_INVALIDO"] });
            }
            double valorDouble = 0.0;

            if (!double.TryParse(valor, out valorDouble))
            {
                return Json(new { valorInvalido = true, msg = traducaoHelper["VALOR_INVALIDO"] });
            }

            if (usuario.StatusID == 2) //Usuario ativo
            {
                if (valorDouble < ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO_USUARIO_ATIVO"))
                {
                    strMensagem = traducaoHelper["PRODUTO_VALOR_VARIAVEL_MINIMO"] + " " + ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO_USUARIO_ATIVO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
                    return Json(new { valorInvalido = true, msg = strMensagem });
                }
            }
            else
            {
                if (valorDouble < ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO"))
                {
                    strMensagem = traducaoHelper["PRODUTO_VALOR_VARIAVEL_MINIMO"] + " " + ConfiguracaoHelper.GetMoedaPadrao().Simbolo + " " + ConfiguracaoHelper.GetDouble("PRODUTO_VALOR_VARIAVEL_MINIMO").ToString(ConfiguracaoHelper.GetMoedaPadrao().MascaraOut);
                    return Json(new { valorInvalido = true, msg = strMensagem });
                }
            }

            if (produto != null)
            {
                var ultimoPedido = pedidoRepository.GetPedidoUltimoStatus(usuario.ID, id);

                if (ultimoPedido == null || ultimoPedido.StatusId != (int)PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
                {
                    prosseguir = true;
                    carrinho.Resetar();
                    produto.ProdutoValor.FirstOrDefault().Valor = valorDouble;
                    carrinho.Adicionar(produto, produto.ValorMinimo(usuario));
                }
                else
                {
                    return Json(new { pedidoExiste = true });
                }
            }

            return Json(new { ok = prosseguir });
        }

        public ActionResult Atualizar(CarrinhoModel carrinho, int id, int quantidade)
        {
            var produto = produtoRepository.Get(id);
            if (produto.Tipo != Core.Entities.Produto.Tipos.Upgrade || quantidade <= 1)
            {
                carrinho.Atualizar(id, quantidade);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remover(CarrinhoModel carrinho, int id)
        {
            carrinho.Remover(id);
            return RedirectToAction("Index");
        }

    }
}
