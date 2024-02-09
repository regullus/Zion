#region Bibliotecas

using Core.Entities;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Sistema;
using Core.Services.MeioPagamento;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Core.Helpers;
using Core.Services.Globalizacao;

#endregion

namespace Sistema.Controllers
{
   public class BoletoController : Controller
   {

      #region Variaveis

      #endregion

      #region Core

      private PedidoPagamentoRepository pedidoPagamentoRepository;
      private BoletoRepository boletoRepository;
      private MoedaService moedaService;

      public BoletoController(DbContext context)
      {
         pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
         boletoRepository = new BoletoRepository(context);
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
               return View(pagamento);
            }
         }
         return RedirectToAction("Index", "Home");
      }

      public ActionResult Gerar(int pagamentoID)
      {
         var pagamento = pedidoPagamentoRepository.Get(pagamentoID);
         if (pagamento != null)
         {
            if (pagamento.UltimoStatus.Status == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
               DateTime dataVencimento = App.DateTimeZion.AddDays(5);
               string strMascaraNossoNumero = ConfiguracaoHelper.GetString("BOLETO_DIGITOS_NOSSONUMERO");

               var boleto = pagamento.Boleto.FirstOrDefault(b => b.StatusID == (int)Boleto.TodosStatus.AguardandoPagamento);
               if (boleto == null)
               {
                  var numeroDocumento = boletoRepository.GetMaxID() + 1;
                  boleto = new Boleto()
                  {
                     DataCriacao = App.DateTimeZion,
                     DataVencimento = dataVencimento,
                     NossoNumero = numeroDocumento.ToString(strMascaraNossoNumero),
                     NumeroDocumento = numeroDocumento,
                     PedidoPagamentoID = pagamento.ID,
                     Status = Boleto.TodosStatus.AguardandoPagamento,
                     ValorPago = 0,
                     ValorTotal = moedaService.Converter("USD", "BRL", MoedaCotacao.Tipos.Entrada, (double)pagamento.Valor)
                  };
                  boletoRepository.Save(boleto);
               }
               else if (boleto.Status == Boleto.TodosStatus.Vencido)
               {
                  boleto.DataVencimento = dataVencimento;
                  boletoRepository.Save(boleto);
               }
               ViewBag.Boleto = BoletoService.Gerar(boleto, cpUtilities.Gerais.Sistema.ToUpper());
               return View();
            }
         }
         return RedirectToAction("Index", "Home");
      }

      #endregion

   }
}
