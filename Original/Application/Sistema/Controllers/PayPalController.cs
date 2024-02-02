#region Bibliotecas

using Core.Entities;
using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Sistema;
using Core.Repositories.Usuario;
using Core.Services.Globalizacao;
using Core.Services.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Fluentx.Mvc;

#endregion

namespace Sistema.Controllers
{
    public class PayPalController : Controller
   {

      #region Variaveis

      #endregion

      #region Core

      private PedidoPagamentoRepository pedidoPagamentoRepository;
      private BoletoRepository boletoRepository;
      private MoedaService moedaService;
      private PedidoRepository pedidoRepository;
      private PedidoItemRepository pedidoItemRepository;
      private UsuarioRepository usuarioRepository;
      public Usuario usuario;
      private PedidoService pedidoService;
      private Core.Helpers.TraducaoHelper traducaoHelper;

      public PayPalController(DbContext context)
      {
         pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
         boletoRepository = new BoletoRepository(context);
         moedaService = new MoedaService(context);
         pedidoRepository = new PedidoRepository(context);
         pedidoItemRepository = new PedidoItemRepository(context);
         usuarioRepository = new UsuarioRepository(context);
         pedidoService = new PedidoService(context);
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

      public ActionResult Pagar(int pedidoID, string chamada)
      {

         #region Variaveis

         var pedido = pedidoRepository.Get(pedidoID);

         Localizacao(pedido.Usuario.Pais);
         Fundos();

         bool blnContinua = true;
         List<string> strValidacao = new List<string>();

         string strURLPayPal = Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_URL");
         string strURLSucesso = Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_URL_SUCESSO");
         string strURLFalha = Core.Helpers.ConfiguracaoHelper.GetString("PAYPAL_URL_FALHA");

         //Paypal
         string strNome = "";
         string strPreco = "";
         string strSKU = ""; //Codigo do produto
         string strDescricao = "";
         string strInvoiceNumber = "";

         //Verifca quem esta chamando essa funcão
         switch (chamada)
         {
            case "loja":
               strInvoiceNumber = "2";
               break;
            default:
               //Cadastro
               strInvoiceNumber = "1";
               break;
         }

         #endregion

         #region popula valores para paypal
         var pedidoItem = pedidoItemRepository.GetByExpression(x => x.PedidoID == pedidoID);

         //Por enquanto aceita somente um item do pedido, futuramente tratar para receber mais itens

         //Mudo a configuração regional para o formato americano
         //pois, o Paypal recebe no formato americano
         Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
         Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

         //Codigo da compra id do usuario e pedido
         strInvoiceNumber += pedido.ID.ToString("0000000000");

         foreach (var item in pedidoItem)
         {
            strNome = item.Produto.Nome;
            strPreco = item.ValorUnitario.ToString();
            strSKU = item.Produto.SKU;
            strDescricao = item.Produto.Nome;
         }
         #endregion

         #region Gerando paypal

         //Inclui id do usuario no Invoice
         strInvoiceNumber = pedido.Usuario.ID.ToString("000000") + strInvoiceNumber;

         Dictionary<string, object> objData = new Dictionary<string, object>();

         //Chave para autenticacao no servico do paypal
         string strChave = App.DateTimeZion.ToString("yyyydd");

         strChave = strChave + strInvoiceNumber;
         strChave = CriptografiaHelper.Morpho(strChave, "Sirius2015", CriptografiaHelper.TipoCriptografia.Criptografa);

         //Parametors para pagamento
         objData.Add("nome", strNome);
         objData.Add("preco", strPreco);
         objData.Add("sku", strSKU);
         objData.Add("descricao", strDescricao);
         objData.Add("invoiceNumber", strInvoiceNumber);
         objData.Add("qtde", "1");
         objData.Add("shipping", "0");
         objData.Add("subTotal", strPreco);
         objData.Add("tax", "0");
         objData.Add("chave", strChave);
         objData.Add("total", strPreco); // Total deve ser igual a soma de shipping, tax e subtotal.
                                         //Caso branco pega do webConfig so servido paypal
         objData.Add("clientId", "");
         objData.Add("clientSecret", "");
         //Caso o pagamento tenha sido efetuado com sucesso, redireciona para a pagina abaixo
         objData.Add("urlSucesso", strURLSucesso);
         //Url de erro, caso não tenha sido possivel efetuar o pagamento o paypal informa o erro ocorrido e direciona para a pagina abaixo
         objData.Add("urlErro", strURLFalha);

         if (blnContinua)
         {
            if (!String.IsNullOrEmpty(strURLPayPal))
            {
               //Redireciona ao servico do paypal
               return this.RedirectAndPost(strURLPayPal, objData);
            }

         }

         return RedirectToAction("falha", "paypal");

         #endregion
      }

      private void Localizacao(Pais pais)
      {
         ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(pais.Idioma);
         var culture = new System.Globalization.CultureInfo(pais.Idioma.Sigla);
         Thread.CurrentThread.CurrentCulture = culture;
         Thread.CurrentThread.CurrentUICulture = culture;
      }

      private void Fundos()
      {
         ViewBag.Fundos = ArquivoRepository.BuscarArquivos(Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO"), Core.Helpers.ConfiguracaoHelper.GetString("PASTA_FUNDOS"), "*.jpg");
      }

      public ActionResult Sucesso()
      {
         string strkey = Request["key"];
         string strId = Request["id"];

         if (String.IsNullOrEmpty(strkey))
         {
            //Chamada dessa função sem ser vinda do paypal
            return RedirectToAction("Falha", "PayPal");
         }

         string strChave = CriptografiaHelper.Morpho(strkey, "Sirius2015", CriptografiaHelper.TipoCriptografia.Descriptografa);
         string strInvoiceNumber = CriptografiaHelper.Morpho(strId, "Sirius2015", CriptografiaHelper.TipoCriptografia.Descriptografa);
         int idUsuario = Convert.ToInt32(strInvoiceNumber.Substring(0, 6));
         int chamada = Convert.ToInt32(strInvoiceNumber.Substring(6, 1));
         int idPedido = Convert.ToInt32(strInvoiceNumber.Substring(7));

         //Valida chamada de uma origem valida
         string strChaveCompara = App.DateTimeZion.ToString("yyyydd");
         strChaveCompara = strChaveCompara + strInvoiceNumber;

         //Caso origem não seja valida
         if (strChave != strChaveCompara)
         {
            return RedirectToAction("Falha", "PayPal");
         }

         usuario = usuarioRepository.Get(idUsuario);
         Localizacao(usuario.Pais);
         Fundos();

         //pagamento concluido.
         var pedido = pedidoRepository.Get(idPedido);
         var pagamento = pedido.PedidoPagamento.FirstOrDefault();
         if (pagamento != null)
         {
            pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);
         }
         switch (chamada)
         {
            case 2:
               //loja
               string strLoja = "~/loja/pedido/finalizado?pedidoID=" + idPedido;
               return Redirect(strLoja);
            default:
               //Cadastro
               return View();
         }
      }

      public ActionResult Falha()
      {
         string strkey = Request["key"];
         string strId = Request["id"];
         string strMsg = Request["msg"];
         string strMensagem = "";

         //Usuario default, caso não haja algun, aqui somente serve para setar o idioma
         int idUsuario = 1;

         if (String.IsNullOrEmpty(strkey))
         {
            //Caso a chamada dessa função não seja provinda do paypal
            strMensagem = traducaoHelper["NAO_HA_CHAVE_DE_RETORNO"];
            strkey = "000";
            ViewBag.Motivo = traducaoHelper["PROBLEMA"] + ": " + strMensagem;
            ViewBag.Codigo = traducaoHelper["CODIGO"] + ": " + strkey;
            ViewBag.Title = traducaoHelper["ERRO_PAYPAL"];
         }
         else
         {
            string strChave = CriptografiaHelper.Morpho(strkey, "Sirius2015", CriptografiaHelper.TipoCriptografia.Descriptografa);
            string strInvoiceNumber = CriptografiaHelper.Morpho(strId, "Sirius2015", CriptografiaHelper.TipoCriptografia.Descriptografa);
            strMensagem = CriptografiaHelper.Morpho(strMsg, "Sirius2015", CriptografiaHelper.TipoCriptografia.Descriptografa);
            idUsuario = Convert.ToInt32(strInvoiceNumber.Substring(0, 6));
            int idPedido = Convert.ToInt32(strInvoiceNumber.Substring(6));

            ViewBag.Motivo = traducaoHelper["PROBLEMA"] + ": " + strMensagem;
            ViewBag.Codigo = traducaoHelper["CODIGO"] + ": " + strkey;
            ViewBag.Title = traducaoHelper["ERRO_PAYPAL"];
            ViewBag.Pedido = traducaoHelper["PEDIDO"] + ": " + idPedido;
         }

         usuario = usuarioRepository.Get(idUsuario);
         Localizacao(usuario.Pais);
         Fundos();

         //Armazena em banco a falha

         return View();
      }

      #endregion

   }
}
