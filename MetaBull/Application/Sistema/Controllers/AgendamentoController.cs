using Core.Entities;
using Core.Helpers;
using Core.Repositories.Globalizacao;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema.Controllers
{
   public class AgendamentoController : SecurityController<Core.Entities.Agendamento>
   {
      #region Core

      private AgendamentoRepository agendamentoRepository;
      private AgendamentoItemRepository agendamentoItemRepository;
      private PedidoRepository pedidoRepository;
      private FilialRepository filialReporitory;

      public AgendamentoController(DbContext context) : base(context)
      {
         this.agendamentoRepository = new AgendamentoRepository(context);
         this.agendamentoItemRepository = new AgendamentoItemRepository(context);
         this.pedidoRepository = new PedidoRepository(context);
         this.filialReporitory = new FilialRepository(context);
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

      public class HorarioDisponivel
      {
         public int ID { get; set; }
         public string Horario { get; set; }
      }

      #endregion

      #region Actions

      // GET: Agendamento
      public ActionResult Agendar(int? id)
      {

         if (id == null)
         {
            return RedirectToAction("index", "home");
         }

         int idLocal = id ?? 0;

         var produto = this.pedidoRepository.Get(idLocal).PedidoItem.FirstOrDefault().ProdutoID;

         ViewBag.Filiais = this.filialReporitory.GetAll().ToList();
         ViewBag.Itens = this.agendamentoItemRepository.GetByExpression(i => i.ProdutoID == produto).ToList();
         ViewBag.ID = idLocal;
         ViewBag.ProdutoID = produto;
         return View();
      }

      [HttpPost]
      public ActionResult Salvar(FormCollection form)
      {
         var culture = new CultureInfo("pt-BR");

         var item = this.agendamentoItemRepository.Get(int.Parse(form["horario"]));

         var agendamento = new Agendamento()
         {
            AgendamentoItem = item.ID,
            Data = DateTime.Parse(form["data"], culture),
            Status = 0,
            DataCriacao = App.DateTimeZion,
            TipoID = 1,
            UsuarioID = usuario.ID,
            PedidoID = int.Parse(form["id"])
         };

         this.agendamentoRepository.Save(agendamento);

         var pedido = this.pedidoRepository.Get(agendamento.PedidoID);

         if (pedido.QuantidadeAgendamento.HasValue)
         {
            pedido.QuantidadeAgendamento += 1;
         }
         else
         {
            pedido.QuantidadeAgendamento = 0;
            pedido.QuantidadeAgendamento += 1;
         }

         this.pedidoRepository.Save(pedido);

         return RedirectToAction("Index", "meuspedidos");
      }

      [HttpPost]
      public JsonResult RangeDatas(int filial, string carro)
      {
         var agendamentoItens = this.agendamentoItemRepository.GetByExpression(i => i.FilialID == filial && i.Nome == carro).ToList();

         var range = new List<string>();

         foreach (var item in agendamentoItens)
         {
            DateTime inicio = item.Inicio;
            DateTime fim = item.Fim;

            while (inicio < fim)
            {
               range.Add(inicio.ToString("dd/MM/yyyy"));
               inicio = inicio.AddDays(1);
            }

            range.Add(item.Fim.ToString("dd/MM/yyyy"));
         }

         range = range.Distinct().ToList();

         return Json(range, JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public JsonResult RangeHorario(int filial, string produto, string data)
      {
         var culture = new CultureInfo("pt-BR");
         var dt = DateTime.Parse(data, culture);

         var agendamentoItens = this.agendamentoItemRepository.GetByExpression(i => i.FilialID == filial
                                                                                 && i.Nome == produto
                                                                                 && (dt >= i.Inicio && dt <= i.Fim)).ToList();

         var range = new List<HorarioDisponivel>();
         if (agendamentoItens.Count() == 0)
         {
            range.Add(new HorarioDisponivel() { ID = 0, Horario = traducaoHelper["AGENDAMENTO_SEM_HORARIO"] });
         }
         else
         {
            foreach (var item in agendamentoItens)
            {
               range.Add(new HorarioDisponivel() { ID = item.ID, Horario = string.Format("{0}-{1}", item.De, item.Ate) });
            }
         }

         return Json(range, JsonRequestBehavior.AllowGet);

      }

      [HttpPost]
      public JsonResult CarroPorFilial(int id, int produto)
      {
         var carros = this.agendamentoItemRepository.GetByExpression(i => i.FilialID == id && i.ProdutoID == produto).Select(i => i.Nome).Distinct().ToList();

         return Json(carros, JsonRequestBehavior.AllowGet);
      }

      public ActionResult AgendarPedido()
      {
         return View();
      }

   }

   #endregion

}