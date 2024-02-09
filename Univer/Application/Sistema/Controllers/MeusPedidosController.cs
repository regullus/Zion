#region Bibliotecas

using Base32;
using Core.Entities;
using Core.Helpers;
using Core.Repositories.Financeiro;
using Core.Services.Loja;
using OtpSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class MeusPedidosController : SecurityController<Core.Entities.Pedido>
    {

        #region Variaveis
        #endregion

        #region Core

        private ContaRepository contaRepository;
        private LancamentoRepository lancamentoRepository;

        private PedidoService pedidoService;

        public MeusPedidosController(DbContext context)
            : base(context)
        {
            contaRepository = new ContaRepository(context);
            lancamentoRepository = new LancamentoRepository(context);

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

        public ActionResult Index()
        {
            obtemMensagem();
            return View();
        }

        public ActionResult Detalhes(int id, string erroTitulo = null, string erroMensagem = null)
        {
            if (!(String.IsNullOrEmpty(erroTitulo) && String.IsNullOrEmpty(erroMensagem)))
            {
                string[] strMensagem = new string[] { erroMensagem };
                Mensagem(erroTitulo, strMensagem, "ale");
            }
            obtemMensagem();
            var pedido = this.repository.Get(id);
            ViewBag.UsuarioContainer = this.usuarioContainer;

            if (pedido != null)
            {
                return View(pedido);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Cancelar(int id)
        {
            var pedido = this.repository.Get(id);
            if (pedido.StatusAtual == Core.Entities.PedidoPagamentoStatus.TodosStatus.AguardandoPagamento)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                if (pagamento != null)
                {
                    pedidoService.Cancelar(pagamento.ID, null);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Pagar(int id, string token2FA = null)
        {
            #region Autenticação Google 
            if (token2FA != null)
            {
                byte[] secretKey = Base32Encoder.Decode(usuario.Autenticacao.GoogleAuthenticatorSecretKey);

                var otp = new Totp(secretKey);
                if (!otp.VerifyTotp(token2FA, out _, new VerificationWindow(10, 10)))
                {
                    string[] strMensagem = new string[] { traducaoHelper["TOKEN_INVALIDO"] };
                    Mensagem(traducaoHelper["INCONSISTENCIA"], strMensagem, "ale");

                    return RedirectToAction("Index");
                }
            }
            #endregion

            var pedido = this.repository.Get(id);

            if (pedido != null)
            {
                var pagamento = pedido.PedidoPagamento.FirstOrDefault();
                if (usuario.Lancamento.Where(l => l.ContaID == (int)Conta.Contas.Rentabilidade).Sum(l => l.Valor) >= pagamento.Valor)
                {
                    var lancamento = new Core.Entities.Lancamento()
                    {
                        CategoriaID = 6, //CatagoraiID = 6 é Pedido - Tabela Finaceiro.Categoria
                        ContaID = 1,
                        DataCriacao = App.DateTimeZion,
                        DataLancamento = App.DateTimeZion,
                        Descricao = "Pedido #" + pedido.Codigo,
                        ReferenciaID = pagamento.ID,
                        Tipo = Core.Entities.Lancamento.Tipos.Compra,
                        UsuarioID = usuario.ID,
                        Valor = -pagamento.Valor,
                        MoedaIDCripto = (int)Core.Entities.Moeda.Moedas.NEN, //Nenhum
                    };
                    lancamentoRepository.Save(lancamento);
                    bool ret = pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);
                }
            }
            return RedirectToAction("Index");
        }

        #endregion

    }
}
