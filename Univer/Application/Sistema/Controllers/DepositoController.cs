#region Bibliotecas

using Core.Entities;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Sistema;
using Core.Services.Globalizacao;
using Core.Services.MeioPagamento;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Sistema.Controllers
{
   public class DepositoController : Controller
   {

      #region Variaveis

      #endregion

      #region Core

      private PedidoPagamentoRepository pedidoPagamentoRepository;
      private ContaDepositoRepository contaDepositoRepository;
      private MoedaService moedaService;

      public DepositoController(DbContext context)
      {
         pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
         contaDepositoRepository = new ContaDepositoRepository(context);
         moedaService = new MoedaService(context);
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

      #endregion

      #region Actions

      public ActionResult Pagar(int pagamentoID)
      {
         var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
         if (pagamento != null)
         {
            if (pagamento.UltimoStatus.Status == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
               Localizacao(pagamento.Usuario.Pais);
               Fundos();
               ContaDeposito contaDeposito = null;
               if (pagamento.MeioPagamento == PedidoPagamento.MeiosPagamento.Deposito)
               {
                  contaDeposito = contaDepositoRepository.Get(pagamento.ReferenciaID);
               }
               else
               {
                  contaDeposito = contaDepositoRepository.GetAtual();
                  pagamento.MeioPagamento = PedidoPagamento.MeiosPagamento.Deposito;
                  pagamento.ReferenciaID = contaDeposito.ID;
                  pedidoPagamentoRepository.Save(pagamento);
               }

               var valorDeposito = moedaService.Converter("USD", "BRL", MoedaCotacao.Tipos.Entrada, (double)pagamento.Valor);
               var cotacao = moedaService.Converter("USD", "BRL", MoedaCotacao.Tipos.Entrada, 1);
               ViewBag.ValorDeposito = valorDeposito;
               ViewBag.Cotacao = cotacao;

               ViewBag.ContaDeposito = contaDeposito;
               return View(pagamento);
            }
         }
         return RedirectToAction("Index", "Home");
      }

      public ActionResult Sucesso(int pagamentoID)
      {
         var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
         if (pagamento != null)
         {
            Localizacao(pagamento.Usuario.Pais);
            Fundos();
            return View(pagamento);
         }
         return View();
      }

      #endregion

   }
}
