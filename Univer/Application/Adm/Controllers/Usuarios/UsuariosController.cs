#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;

using Core.Entities;
using Core.Factories;
using Core.Helpers;
using Core.Models.Loja;
using Core.Repositories.Financeiro;
using Core.Repositories.Loja;
using Core.Repositories.Rede;
using Core.Repositories.Usuario;
using Core.Repositories.Globalizacao;
using Core.Services.Loja;
using Core.Services.Usuario;
using Core.Services.Sistema;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Models Local
using Sistema.Models;

//Lista
using PagedList;
using Helpers;

//Excel
using ClosedXML.Excel;
using cpUtilities;
using System.Threading;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using Base32;
using OtpSharp;
using DocumentFormat.OpenXml.Spreadsheet;
using Fluentx;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Core.Models;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class UsuariosController : Controller
    {

        #region Core

        private UsuarioRepository usuarioRepository;
        private AssociacaoRepository associacaoRepository;
        private ClassificacaoRepository classificacaoRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;
        private PedidoFactory pedidofactory;
        private ProdutoRepository produtoRepository;
        private MeioPagamentoRepository meioPagamentoRepository;
        private PedidoService pedidoService;
        private PedidoPagamentoStatusRepository pedidoPagamentoStatusRepository;
        private UsuarioService usuarioService;
        private BoletoRepository boletoRepository;
        private PedidoRepository pedidoRepository;
        private EstadoRepository estadoRepository;
        private CidadeRepository cidadeRepository;
        private PaisRepository paisRepository;
        private EnderecoRepository enderecoRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private DocumentoRepository documentoRepository;
        private FilialRepository filialRepository;
        private UsuarioLoginExternoRepository usuarioLoginExternoRepository;
        private TabuleiroRepository tabuleiroRepository;
        private LancamentoRepository lancamentoRepository;

        private Core.Helpers.TraducaoHelper traducaoHelper;

        public UsuariosController(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            classificacaoRepository = new ClassificacaoRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            pedidofactory = new PedidoFactory(context);
            produtoRepository = new ProdutoRepository(context);
            meioPagamentoRepository = new MeioPagamentoRepository(context);
            pedidoService = new PedidoService(context);
            boletoRepository = new BoletoRepository(context);
            pedidoRepository = new PedidoRepository(context);
            pedidoPagamentoStatusRepository = new PedidoPagamentoStatusRepository(context);
            estadoRepository = new EstadoRepository(context);
            paisRepository = new PaisRepository(context);
            cidadeRepository = new CidadeRepository(context);
            enderecoRepository = new EnderecoRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            documentoRepository = new DocumentoRepository(context);
            usuarioService = new UsuarioService(context);
            filialRepository = new FilialRepository(context);
            usuarioLoginExternoRepository = new UsuarioLoginExternoRepository(context);
            tabuleiroRepository = new TabuleiroRepository(context);
            lancamentoRepository = new LancamentoRepository(context);

            Localizacao();
        }

        private YLEVELEntities db = new YLEVELEntities();

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

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion

        #region Validacao

        public JsonResult DataValidar(string DataNascimento)
        {
            DateTime _data;
            string idioma = "pt-BR";

            Pais pais = (Pais)Session["pais"];

            if (pais != null)
            {
                idioma = pais.Idioma.Sigla;
            }

            bool blnChecar = DateTime.TryParse(DataNascimento, new CultureInfo(idioma), DateTimeStyles.None, out _data) ? true : false;

            return Json(blnChecar, JsonRequestBehavior.AllowGet);

        }

        public JsonResult DocumentoValidar(string Documento)
        {
            string idioma = "pt-BR";
            bool retorno = true;

            Pais pais = (Pais)Session["pais"];

            if (pais != null)
            {
                idioma = pais.Idioma.Sigla;
            }

            if (idioma == "pt-BR")
            {
                //se brasil valida cpf
                retorno = cpUtilities.Validacoes.ValidaCPF(Documento);

                if (!retorno)
                {
                    return Json("CPF inválido", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_DOCUMENTO_EM_USO") &&
                        ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_DOCUMENTO_EM_USO"))
                    {
                        retorno = usuarioRepository.GetByDocumento(Documento) == null ? true : false;
                        if (!retorno)
                        {
                            return Json("CPF já cadastrado", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json(retorno, JsonRequestBehavior.AllowGet);

        }

        public JsonResult EmailValidar(string Email)
        {
            bool retorno = true;
            if (ConfiguracaoHelper.TemChave("CADASTRO_VALIDA_EMAIL_EM_USO") &&
                ConfiguracaoHelper.GetBoolean("CADASTRO_VALIDA_EMAIL_EM_USO"))
            {
                retorno = usuarioRepository.GetByEmail(Email) == null ? true : false;
            }

            return Json(retorno, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Function

        public struct Cidade
        {
            public int ID;
            public string Nome;

            public Cidade(Core.Entities.Cidade cidade)
            {
                this.ID = cidade.ID;
                this.Nome = cidade.Nome;
            }
        }

        #endregion

        #region Autorizacao

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // Add the Group Manager (NOTE: only access through the public
        // Property, not by the instance variable!)
        private AutenticacaoGrupoManager _groupManager;

        public AutenticacaoGrupoManager GroupManager
        {
            get
            {
                return _groupManager ?? new AutenticacaoGrupoManager();
            }
            private set
            {
                _groupManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        #endregion

        #region Actions

        // GET: Usuarios
        public ActionResult Index(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, string CurrentProcuraPatrocinador, string ProcuraPatrocinador, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref CurrentProcuraPatrocinador,
                                    ref ProcuraPatrocinador,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "Usuarios");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "login";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraLogin != null || ProcuraPatrocinador != null))
            {
                if (ProcuraLogin == null)
                {
                    ProcuraLogin = CurrentProcuraLogin;
                }
                if (ProcuraPatrocinador == null)
                {
                    ProcuraPatrocinador = CurrentProcuraPatrocinador;
                }
            }

            ViewBag.CurrentProcuraLogin = ProcuraLogin;
            ViewBag.CurrentProcuraPatrocinador = ProcuraPatrocinador;

            IQueryable<Usuario> lista = null;

            lista = db.Usuarios.Include(u => u.Pais).Include(u => u.PatrocinadorDireto).Include(u => u.PatrocinadorPosicao).Include(u => u.UltimoDireita).Include(u => u.UltimoEsquerda).Include(u => u.Filial).Include(u => u.Autenticacao);
            lista = lista.Where(x => x.ID > 2500); //Anteriores são admins ou logins do Sistema que não podem ser alterados

            //Obtem lista de usuarioID que não efetuaram pagamento
            IEnumerable<Core.Models.TabuleiroUsuarioModel> tabuleirosUsuario = tabuleiroRepository.ObtemTabuleirosNaoPagaramSistema();

            //Remove da lista os usuarios que não informaram um pagamento
            foreach (var item in lista)
            {
                //Verifica se o usuario informou um pagamento ao sistema
                var usuario = tabuleirosUsuario.Where(x => x.UsuarioID == item.ID).FirstOrDefault();
                if (usuario != null)
                {
                    item.Documento = usuario.Posicao; //Usando o campo Documento para exibir a posição do usuario   
                    item.SenhaLegado = usuario.BoardNome; //Usando o campo SenhaLegado para exibir o nome da galaxia que o usuario está
                    item.TipoID = usuario.BoardID; //Usado o campo TipoID como BoardID
                    item.Oculto = true; //Usado para exibir icone de confirmação de pagamento
                }
                else
                {
                    item.Oculto = false; //Usado para exibir icone de confirmação de pagamento
                }
            }

            if (!String.IsNullOrEmpty(ProcuraLogin) && !String.IsNullOrEmpty(ProcuraPatrocinador))
            {
                lista = lista.Where(x => x.PatrocinadorDireto.Login.Contains(ProcuraPatrocinador) &&
                                       (x.Login.Contains(ProcuraLogin) ||
                                         x.Nome.Contains(ProcuraLogin) ||
                                         x.Email.Contains(ProcuraLogin)
                                       )
                                    );
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraLogin))
                {
                    lista = lista.Where(x => x.Login.Contains(ProcuraLogin) ||
                                             x.Nome.Contains(ProcuraLogin) ||
                                             x.Email.Contains(ProcuraLogin)
                                       );
                }
                if (!String.IsNullOrEmpty(ProcuraPatrocinador))
                {
                    lista = lista.Where(x => x.PatrocinadorDireto.Nome.Contains(ProcuraPatrocinador));
                }
            }

            switch (SortOrder)
            {
                case "login_desc":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador";
                    lista = lista.OrderByDescending(x => x.Login);
                    break;
                case "patrocinador":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador_desc";
                    lista = lista.OrderBy(x => x.PatrocinadorDireto.Login);
                    break;
                case "patrocinador_desc":
                    ViewBag.FirstSortParm = "login";
                    ViewBag.SecondSortParm = "patrocinador";

                    lista = lista.OrderByDescending(x => x.PatrocinadorDireto.Nome);
                    break;
                case "login":
                    ViewBag.FirstSortParm = "login_desc";
                    ViewBag.SecondSortParm = "date";

                    lista = lista.OrderBy(x => x.Login);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "login_desc";
                    ViewBag.SecondSortParm = "patrocinador";
                    lista = lista.OrderBy(x => x.Login);
                    break;
            }

            //Numero de linhas por Pagina
            int PageSize = (NumeroPaginas ?? 5);

            //Caso seja selecionada toda a lista (-1), pega na verdade 1000
            if (PageSize == -1)
            {
                PageSize = 1000;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.CurrentNumeroPaginas = NumeroPaginas;

            //Pagina corrente
            int PageNumber = (Page ?? 1);

            //DropDown de paginação
            int intNumeroPaginas = (NumeroPaginas ?? 5);

            #region ViewBags

            ViewBag.UsuarioEntraOutraAba = ConfiguracaoHelper.GetBoolean("USUARIO_ENTRA_OUTRA_ABA");

            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            ViewBag.Associacoes = associacaoRepository.GetAll().ToList();
            ViewBag.Classificacoes = classificacaoRepository.GetAll().ToList();
            ViewBag.Produtos = produtoRepository.GetAtivacao();
            ViewBag.MeioPagamento = meioPagamentoRepository.GetAtivos();
            ViewBag.Lista = lista;

            #endregion

            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Associacoes = associacaoRepository.GetAll().ToList();

            if (ConfiguracaoHelper.TemChave("ADESAO_PRODUTOS_FIXOS") && ConfiguracaoHelper.GetBoolean("ADESAO_PRODUTOS_FIXOS"))
            {
                ViewBag.Produtos = produtoRepository.GetByTipo(Produto.Tipos.ProdutoFisico);
            }
            else
            {
                ViewBag.Produtos = produtoRepository.GetByTipo(Produto.Tipos.Associacao);
            }

            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.PaisID = new SelectList(db.Paises, "ID", "Sigla");
            //ViewBag.PatrocinadorDiretoID = new SelectList(db.Usuarios, "ID", "Assinatura");
            //ViewBag.PatrocinadorPosicaoID = new SelectList(db.Usuarios, "ID", "Assinatura");
            //ViewBag.UltimoDireitaID = new SelectList(db.Usuarios, "ID", "Assinatura");
            //ViewBag.UltimoEsquerdaID = new SelectList(db.Usuarios, "ID", "Assinatura");
            ViewBag.FilialID = new SelectList(db.Filial, "ID", "Nome");
            //ViewBag.IdAutenticacao = new SelectList(db.Autenticacao, "Id", "Email");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Assinatura,PaisID,PatrocinadorDiretoID,PatrocinadorPosicaoID,UltimoDireitaID,UltimoEsquerdaID,NivelAssociacao,NivelClassificacao,ProfundidadeRede,DerramamentoID,TipoID,StatusID,StatusCelularID,StatusEmailID,Nome,NomeFantasia,Apelido,Celular,Telefone,Sexo,Email,Login,Senha,Documento,DataNascimento,UltimoLogin,DataCriacao,GeraBonus,RecebeBonus,EntradaID,Bloqueado,Oculto,SenhaLegado,DataAtivacao,DataValidade,DataMigracao,TipoAtivacao,FilialID,IdAutenticacao,ExibeSaque,QtdeDiretos,DataRenovacao")] Usuario usuario)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (string.IsNullOrEmpty(usuario.Login))
            {
                msg.Add(traducaoHelper["LOGIN"]);
            }
            //if (string.IsNullOrEmpty(usuario.Apelido))
            //{
            //    msg.Add(traducaoHelper["APELIDO"]);
            //}
            if (string.IsNullOrEmpty(usuario.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(usuario.NomeFantasia))
            {
                msg.Add(traducaoHelper["NOME_FANTASIA"]);
            }
            if (string.IsNullOrEmpty(usuario.Documento))
            {
                msg.Add(traducaoHelper["DOCUMENTO"]);
            }
            if (string.IsNullOrEmpty(usuario.Email))
            {
                msg.Add(traducaoHelper["EMAIL"]);
            }
            if (string.IsNullOrEmpty(usuario.Telefone))
            {
                msg.Add(traducaoHelper["TELEFONE"]);
            }
            if (string.IsNullOrEmpty(usuario.Celular))
            {
                msg.Add(traducaoHelper["CELULAR"]);
            }
            if (usuario.ExibeSaque == 0)
            {
                msg.Add(traducaoHelper["BONUS"]);
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem("Classificacao", erro, "err");
            }
            else
            {
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            obtemMensagem();

            ViewBag.PaisID = new SelectList(db.Paises, "ID", "Sigla", usuario.PaisID);
            //ViewBag.PatrocinadorDiretoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorDiretoID);
            //ViewBag.PatrocinadorPosicaoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorPosicaoID);
            //ViewBag.UltimoDireitaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoDireitaID);
            //ViewBag.UltimoEsquerdaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoEsquerdaID);
            ViewBag.FilialID = new SelectList(db.Filial, "ID", "Nome", usuario.FilialID);
            //ViewBag.IdAutenticacao = new SelectList(db.Autenticacao, "Id", "Email", usuario.IdAutenticacao);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaisID = new SelectList(db.Paises, "ID", "Sigla", usuario.PaisID);
            //ViewBag.PatrocinadorDiretoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorDiretoID);
            //ViewBag.PatrocinadorPosicaoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorPosicaoID);
            //ViewBag.UltimoDireitaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoDireitaID);
            //ViewBag.UltimoEsquerdaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoEsquerdaID);
            ViewBag.FilialID = new SelectList(db.Filial, "ID", "Nome", usuario.FilialID);
            //ViewBag.IdAutenticacao = new SelectList(db.Autenticacao, "Id", "Email", usuario.IdAutenticacao);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Assinatura,PaisID,PatrocinadorDiretoID,TermoAceite,PatrocinadorPosicaoID,UltimoDireitaID,UltimoEsquerdaID,NivelAssociacao,NivelClassificacao,ProfundidadeRede,DerramamentoID,TipoID,StatusID,StatusCelularID,StatusEmailID,Nome,NomeFantasia,Apelido,Celular,Telefone,Sexo,Email,Login,Senha,Documento,DataNascimento,UltimoLogin,DataCriacao,GeraBonus,RecebeBonus,EntradaID,Bloqueado,Oculto,SenhaLegado,DataAtivacao,DataValidade,DataMigracao,TipoAtivacao,FilialID,IdAutenticacao,ExibeSaque,QtdeDiretos,DataRenovacao")] Usuario usuario)
        {
            Localizacao();

            List<string> msg = new List<string>();
            msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

            if (usuario.Assinatura == null)
                usuario.Assinatura = "";

            if (usuario.Apelido == null)
                usuario.Apelido = "";

            if (string.IsNullOrEmpty(usuario.Login))
            {
                msg.Add(traducaoHelper["LOGIN"]);
            }
            //if (string.IsNullOrEmpty(usuario.Apelido))
            //{
            //    msg.Add(traducaoHelper["APELIDO"]);
            //}
            if (string.IsNullOrEmpty(usuario.Nome))
            {
                msg.Add(traducaoHelper["NOME"]);
            }
            if (string.IsNullOrEmpty(usuario.NomeFantasia))
            {
                msg.Add(traducaoHelper["NOME_FANTASIA"]);
            }
            if (string.IsNullOrEmpty(usuario.Documento))
            {
                usuario.Documento = "";
                //msg.Add(traducaoHelper["DOCUMENTO"]);
            }
            if (string.IsNullOrEmpty(usuario.Email))
            {
                msg.Add(traducaoHelper["EMAIL"]);
            }
            if (string.IsNullOrEmpty(usuario.Telefone))
            {
                msg.Add(traducaoHelper["TELEFONE"]);
            }
            if (string.IsNullOrEmpty(usuario.Celular))
            {
                msg.Add(traducaoHelper["CELULAR"]);
            }
            if (usuario.ExibeSaque == 0)
            {
                msg.Add(traducaoHelper["BONUS"]);
            }

            if (!string.IsNullOrEmpty(usuario.Telefone))
            {
                usuario.Telefone = "0";
            }

            if (!string.IsNullOrEmpty(usuario.Celular))
            {
                usuario.Celular = "0";
            }

            if (msg.Count > 1)
            {
                string[] erro = msg.ToArray();
                Mensagem("Classificacao", erro, "err");
            }
            else
            {
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                Autenticacao autenticacao = db.Autenticacao.Find(usuario.IdAutenticacao);
                if (autenticacao != null)
                {
                    autenticacao.UserName = usuario.Login;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }



            obtemMensagem();

            ViewBag.PaisID = new SelectList(db.Paises, "ID", "Sigla", usuario.PaisID);
            //ViewBag.PatrocinadorDiretoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorDiretoID);
            //ViewBag.PatrocinadorPosicaoID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.PatrocinadorPosicaoID);
            //ViewBag.UltimoDireitaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoDireitaID);
            //ViewBag.UltimoEsquerdaID = new SelectList(db.Usuarios, "ID", "Assinatura", usuario.UltimoEsquerdaID);
            ViewBag.FilialID = new SelectList(db.Filial, "ID", "Nome", usuario.FilialID);
            //ViewBag.IdAutenticacao = new SelectList(db.Autenticacao, "Id", "Email", usuario.IdAutenticacao);

            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Entrar(int id)
        {
            //Idioma
            Localizacao();

            if (User.IsInRole("perfilMaster") || User.IsInRole("perfilAdministrador"))
            {
                string strPath = string.Empty;
                if (ConfiguracaoHelper.TemChave("LOGIN_URL_ENTRAR_COM_USUARIO"))
                    strPath = ConfiguracaoHelper.GetString("LOGIN_URL_ENTRAR_COM_USUARIO");
                else
                    return RedirectToAction("Index");

                if (Local.Ambiente == "dev")
                {
                    strPath = "http://localhost:14996/account/loginexterno";
                }

                Usuario usuario = db.Usuarios.Find(id);

                //string strSenha = Gerais.Cripto(usuario.Senha, "Ava2014", TipoCriptografia.Descriptografa);
                string strSenha = Core.Helpers.CriptografiaHelper.Descriptografar(usuario.Senha);

                var guid = Guid.NewGuid();

                var loginExterno = new LoginExterno
                {
                    Login = cpUtilities.Gerais.Morpho(usuario.Login, TipoCriptografia.Criptografa),
                    Senha = cpUtilities.Gerais.Morpho(strSenha, TipoCriptografia.Criptografa),
                    DataHora = App.DateTimeZion,
                    Guid = guid,
                    UsuarioID = id,
                    AdministradorID = Local.idUsuario
                };

                usuarioLoginExternoRepository.Salvar(loginExterno);

                string strURL = Url.Content(strPath);
                strURL += "?id=" + guid.ToString();

                return Redirect(strURL);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public string Logar(int id)
        {
            //Idioma
            Localizacao();

            if (User.IsInRole("perfilMaster") || User.IsInRole("perfilAdministrador"))
            {
                string strPath = string.Empty;
                if (ConfiguracaoHelper.TemChave("LOGIN_URL_ENTRAR_COM_USUARIO"))
                    strPath = ConfiguracaoHelper.GetString("LOGIN_URL_ENTRAR_COM_USUARIO");
                else
                    return ("Index");

                if (Local.Ambiente == "dev")
                {
                    strPath = "http://localhost/zion/sistema/account/loginexterno";
                }

                Usuario usuario = db.Usuarios.Find(id);

                string strSenha = Core.Helpers.CriptografiaHelper.Descriptografar(usuario.Senha);

                strSenha = cpUtilities.Gerais.Morpho(strSenha, TipoCriptografia.Criptografa);
                string strLogin = cpUtilities.Gerais.Morpho(usuario.Login, TipoCriptografia.Criptografa);

                string strURL = Url.Content(strPath);
                strURL += "?login=" + strLogin + "&" + "senha=" + strSenha;

                return (strURL);
            }
            else
            {
                return ("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //*************INICIO Tabuleiro***************

        public ActionResult InformePagamento(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, string CurrentProcuraPatrocinador, string ProcuraPatrocinador, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref CurrentProcuraPatrocinador,
                                    ref ProcuraPatrocinador,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "Usuarios");

            try
            {

                objFuncoes = null;

                //List
                if (String.IsNullOrEmpty(SortOrder))
                {
                    ViewBag.CurrentSort = "login";
                }
                else
                {
                    ViewBag.CurrentSort = SortOrder;
                }

                if (!(ProcuraLogin != null || ProcuraPatrocinador != null))
                {
                    if (ProcuraLogin == null)
                    {
                        ProcuraLogin = CurrentProcuraLogin;
                    }
                    if (ProcuraPatrocinador == null)
                    {
                        ProcuraPatrocinador = CurrentProcuraPatrocinador;
                    }
                }

                ViewBag.CurrentProcuraLogin = ProcuraLogin;
                ViewBag.CurrentProcuraPatrocinador = ProcuraPatrocinador;

                //Obtem lista de usuarioID que informaram o pagamento
                IEnumerable<Core.Models.TabuleiroInformaramPagtoModel> lista = tabuleiroRepository.ObtemTabuleirosInformaramPgto();

                if (!String.IsNullOrEmpty(ProcuraLogin) && !String.IsNullOrEmpty(ProcuraPatrocinador))
                {
                    lista = lista.Where(x => x.Patrocinador.Contains(ProcuraPatrocinador) &&
                                           (x.Login.Contains(ProcuraLogin) ||
                                             x.Nome.Contains(ProcuraLogin) ||
                                             x.Email.Contains(ProcuraLogin)
                                           )
                                        );
                }
                else
                {
                    if (!String.IsNullOrEmpty(ProcuraLogin))
                    {
                        lista = lista.Where(x => x.Login.Contains(ProcuraLogin) ||
                                                 x.Nome.Contains(ProcuraLogin) ||
                                                 x.Email.Contains(ProcuraLogin)
                                           );
                    }
                    if (!String.IsNullOrEmpty(ProcuraPatrocinador))
                    {
                        lista = lista.Where(x => x.Patrocinador.Contains(ProcuraPatrocinador));
                    }
                }

                switch (SortOrder)
                {
                    case "login_desc":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador";
                        lista = lista.OrderByDescending(x => x.Login);
                        break;
                    case "patrocinador":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador_desc";
                        lista = lista.OrderBy(x => x.Patrocinador);
                        break;
                    case "patrocinador_desc":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador";

                        lista = lista.OrderByDescending(x => x.Patrocinador);
                        break;
                    case "login":
                        ViewBag.FirstSortParm = "login_desc";
                        ViewBag.SecondSortParm = "date";

                        lista = lista.OrderBy(x => x.Login);
                        break;
                    default:  // Name ascending 
                        ViewBag.FirstSortParm = "login_desc";
                        ViewBag.SecondSortParm = "patrocinador";
                        lista = lista.OrderBy(x => x.Login);
                        break;
                }

                //Numero de linhas por Pagina
                int PageSize = (NumeroPaginas ?? 5);

                //Caso seja selecionada toda a lista (-1), pega na verdade 1000
                if (PageSize == -1)
                {
                    PageSize = 1000;
                }
                ViewBag.PageSize = PageSize;
                ViewBag.CurrentNumeroPaginas = NumeroPaginas;

                //Pagina corrente
                int PageNumber = (Page ?? 1);

                //DropDown de paginação
                int intNumeroPaginas = (NumeroPaginas ?? 5);

                #region ViewBags

                ViewBag.UsuarioEntraOutraAba = ConfiguracaoHelper.GetBoolean("USUARIO_ENTRA_OUTRA_ABA");

                ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
                ViewBag.Lista = lista;

                #endregion

                return View(lista.ToPagedList(PageNumber, PageSize));

            }
            catch (Exception ex)
            {
                String erro = ex.Message;
                return View();
            }

        }


        public ActionResult PagamentoOK(string SortOrder, string CurrentProcuraLogin, string ProcuraLogin, string CurrentProcuraPatrocinador, string ProcuraPatrocinador, int? NumeroPaginas, int? Page)
        {
            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraLogin,
                                    ref ProcuraLogin,
                                    ref CurrentProcuraPatrocinador,
                                    ref ProcuraPatrocinador,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "Usuarios");

            try
            {

                objFuncoes = null;

                //List
                if (String.IsNullOrEmpty(SortOrder))
                {
                    ViewBag.CurrentSort = "login";
                }
                else
                {
                    ViewBag.CurrentSort = SortOrder;
                }

                if (!(ProcuraLogin != null || ProcuraPatrocinador != null))
                {
                    if (ProcuraLogin == null)
                    {
                        ProcuraLogin = CurrentProcuraLogin;
                    }
                    if (ProcuraPatrocinador == null)
                    {
                        ProcuraPatrocinador = CurrentProcuraPatrocinador;
                    }
                }

                ViewBag.CurrentProcuraLogin = ProcuraLogin;
                ViewBag.CurrentProcuraPatrocinador = ProcuraPatrocinador;

                //Obtem lista de usuarioID que informaram o pagamento
                IEnumerable<Core.Models.TabuleiroInformaramPagtoModel> lista = tabuleiroRepository.ObtemTabuleirosPagos();

                if (!String.IsNullOrEmpty(ProcuraLogin) && !String.IsNullOrEmpty(ProcuraPatrocinador))
                {
                    lista = lista.Where(x => x.Patrocinador.Contains(ProcuraPatrocinador) &&
                                           (x.Login.Contains(ProcuraLogin) ||
                                             x.Nome.Contains(ProcuraLogin) ||
                                             x.Email.Contains(ProcuraLogin)
                                           )
                                        );
                }
                else
                {
                    if (!String.IsNullOrEmpty(ProcuraLogin))
                    {
                        lista = lista.Where(x => x.Login.Contains(ProcuraLogin) ||
                                                 x.Nome.Contains(ProcuraLogin) ||
                                                 x.Email.Contains(ProcuraLogin)
                                           );
                    }
                    if (!String.IsNullOrEmpty(ProcuraPatrocinador))
                    {
                        lista = lista.Where(x => x.Patrocinador.Contains(ProcuraPatrocinador));
                    }
                }

                switch (SortOrder)
                {
                    case "login_desc":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador";
                        lista = lista.OrderByDescending(x => x.Login);
                        break;
                    case "patrocinador":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador_desc";
                        lista = lista.OrderBy(x => x.Patrocinador);
                        break;
                    case "patrocinador_desc":
                        ViewBag.FirstSortParm = "login";
                        ViewBag.SecondSortParm = "patrocinador";

                        lista = lista.OrderByDescending(x => x.Patrocinador);
                        break;
                    case "login":
                        ViewBag.FirstSortParm = "login_desc";
                        ViewBag.SecondSortParm = "date";

                        lista = lista.OrderBy(x => x.Login);
                        break;
                    default:  // Name ascending 
                        ViewBag.FirstSortParm = "login_desc";
                        ViewBag.SecondSortParm = "patrocinador";
                        lista = lista.OrderBy(x => x.Login);
                        break;
                }

                //Numero de linhas por Pagina
                int PageSize = (NumeroPaginas ?? 5);

                //Caso seja selecionada toda a lista (-1), pega na verdade 1000
                if (PageSize == -1)
                {
                    PageSize = 1000;
                }
                ViewBag.PageSize = PageSize;
                ViewBag.CurrentNumeroPaginas = NumeroPaginas;

                //Pagina corrente
                int PageNumber = (Page ?? 1);

                //DropDown de paginação
                int intNumeroPaginas = (NumeroPaginas ?? 5);

                #region ViewBags

                ViewBag.UsuarioEntraOutraAba = ConfiguracaoHelper.GetBoolean("USUARIO_ENTRA_OUTRA_ABA");

                ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
                ViewBag.Lista = lista;

                #endregion

                return View(lista.ToPagedList(PageNumber, PageSize));

            }
            catch (Exception ex)
            {
                String erro = ex.Message;
                return View();
            }

        }


        [HttpPost]
        public ActionResult FoiPago(string usuarioID, string BoardID)
        {
            if (usuarioID.IsNullOrEmpty() || BoardID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idBoard = int.Parse(BoardID);

                if (idUsuario <= 0 || idBoard <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                string retorno = tabuleiroRepository.ConfirmarPagtoSistema(idUsuario, idBoard, true);

                if (retorno == null)
                {
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_FP_01" };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_FP_01");
                }

                if (retorno != "OK")
                {
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_FP_02" };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_FP_02");
                }

                string tabuleiroIncluir = tabuleiroRepository.IncluiTabuleiro(idUsuario, idUsuario, idBoard, "Completa");
                
                Usuario usuario = db.Usuarios.Find(idUsuario);

                TabuleiroBoardModel tabuleiroBoard = tabuleiroRepository.ObtemTabuleiroBoardID(idBoard);

                //Efetuar Credito no syspag
                var lancamento = new Lancamento();
                lancamento.UsuarioID = 2001; //Syspag
                lancamento.Tipo = Lancamento.Tipos.Credito;
                lancamento.ReferenciaID = lancamento.UsuarioID;
                lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper["Board"+BoardID], " - ", usuario.Nome);
                lancamento.DataLancamento = App.DateTimeZion;
                lancamento.DataCriacao = App.DateTimeZion;
                lancamento.ContaID = 7; //Transferencia
                lancamento.CategoriaID = 7; //Transferencia
                lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Transferencia);
                lancamentoRepository.Save(lancamento);

                //Efetuar Debito no Convidado
                lancamento = new Lancamento();
                lancamento.UsuarioID = idUsuario;
                lancamento.Tipo = Lancamento.Tipos.Debito;
                lancamento.ReferenciaID = lancamento.UsuarioID;
                lancamento.Descricao = String.Format("{0}{1}{2}", traducaoHelper["PAGAMENTO_SISTEMA"], " - ", traducaoHelper["Board" + BoardID]);
                lancamento.DataLancamento = App.DateTimeZion;
                lancamento.DataCriacao = App.DateTimeZion;
                lancamento.ContaID = 7; //Transferencia
                lancamento.CategoriaID = 7; //Transferencia
                lancamento.MoedaIDCripto = (int)Moeda.Moedas.USD; //Nenhum
                lancamento.Valor = decimal.ToDouble(tabuleiroBoard.Transferencia);
                lancamentoRepository.Save(lancamento);

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;
            }
            catch (Exception ex)
            {
                string[] strMensagemParam5 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01", "[" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam5, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01" + "[" + ex.Message + "]");
            }
        }

        [HttpPost]
        public ActionResult NaoFoiPago(string usuarioID, string BoardID)
        {
            if (usuarioID.IsNullOrEmpty() || BoardID.IsNullOrEmpty())
            {
                string[] strMensagemParam1 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                Mensagem(traducaoHelper["ALERTA"], strMensagemParam1, "ale");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int idUsuario = int.Parse(usuarioID);
                int idBoard = int.Parse(BoardID);

                if (idUsuario <= 0 || idBoard <= 0)
                {
                    string[] strMensagemParam2 = new string[] { traducaoHelper["PARAMETRO_INVALIDO"] };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam2, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["PARAMETRO_INVALIDO"]);
                }

                string retorno = tabuleiroRepository.ConfirmarPagtoSistema(idUsuario, idBoard, false);

                if (retorno == null)
                {
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_NFP_01" };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_NFP_01");
                }

                if (retorno != "OK")
                {
                    string[] strMensagemParam4 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_NFP_02" };
                    Mensagem(traducaoHelper["ALERTA"], strMensagemParam4, "ale");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_NFP_02");
                }

                JsonResult jsonResult = new JsonResult
                {
                    Data = retorno,
                    RecursionLimit = 1000
                };

                return jsonResult;

            }
            catch (Exception ex)
            {
                string[] strMensagemParam5 = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01", "[" + ex.Message + "]" };
                Mensagem(traducaoHelper["ERRO"], strMensagemParam5, "err");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, traducaoHelper["MENSAGEM_ERRO"] + " COD MRC_GT_01" + "[" + ex.Message + "]");
            }
        }

        public ActionResult ReloadInformePgto()
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("InformePagamento");
        }

        //*************FIM Tabuleiro***************

        public ActionResult Documentos(string login, bool? validado = null, DateTime? de = null, DateTime? ate = null)
        {
            //List<Documento> documentos = null;
            var documentos = documentoRepository.GetAll();

            if (validado.HasValue)
            {
                documentos = documentos.Where(d => (validado.Value ? d.DataValidacao.HasValue : !d.DataValidacao.HasValue));
            }
            if (!String.IsNullOrEmpty(login))
            {
                documentos = documentos.Where(d => d.Usuario.Login.Contains(login) || d.Usuario.Nome.Contains(login));
            }
            if (de.HasValue)
            {
                ViewBag.De = de.Value.ToShortDateString();
                documentos = documentos.Where(d => d.DataEnvio >= de.Value);
            }
            if (ate.HasValue)
            {
                ViewBag.Ate = ate.Value.ToShortDateString();
                var _ate = ate.Value.AddDays(1);
                documentos = documentos.Where(d => d.DataEnvio < _ate);
            }

            ViewBag.Login = login;
            ViewBag.Validado = validado;

            return View(documentos.OrderByDescending(u => u.DataEnvio).Take(50).ToList());
        }

        public ActionResult Aceitar(int id)
        {
            var documento = documentoRepository.Get(id);
            if (documento != null)
            {
                documento.Validado = true;
                documento.DataValidacao = App.DateTimeZion;
                //documento.AdministradorID = administradorID; // merda
                documentoRepository.Save(documento);
            }
            return RedirectToAction("documentos");
        }

        public ActionResult Rejeitar(int id)
        {
            var documento = documentoRepository.Get(id);
            if (documento != null)
            {
                documento.Validado = false;
                documento.DataValidacao = App.DateTimeZion;
                //documento.AdministradorID = administradorID; // merda
                documentoRepository.Save(documento);
            }
            return RedirectToAction("documentos");
        }

        public JsonResult DadosBancarios(int id)
        {
            var contaDeposito = usuarioRepository.Get(id).ContaDeposito.FirstOrDefault();
            var meioPagamento = contaDeposito.MeioPagamento;

            if (contaDeposito.Bitcoin != null && contaDeposito.Bitcoin != "")
            {
                contaDeposito.Bitcoin = Core.Helpers.CriptografiaHelper.Descriptografar(contaDeposito.Bitcoin);
                contaDeposito.Bitcoin = cpUtilities.Gerais.Morpho(contaDeposito.Bitcoin, TipoCriptografia.Descriptografa);
            }
            if (contaDeposito.Litecoin != null && contaDeposito.Litecoin != "")
            {
                contaDeposito.Litecoin = Core.Helpers.CriptografiaHelper.Descriptografar(contaDeposito.Litecoin);
                contaDeposito.Litecoin = cpUtilities.Gerais.Morpho(contaDeposito.Litecoin, TipoCriptografia.Descriptografa);
            }
            if (contaDeposito.Tether != null && contaDeposito.Tether != "")
            {
                contaDeposito.Tether = Core.Helpers.CriptografiaHelper.Descriptografar(contaDeposito.Tether);
                contaDeposito.Tether = cpUtilities.Gerais.Morpho(contaDeposito.Tether, TipoCriptografia.Descriptografa);
            }
            //contaDeposito.Usuario.Nome
            if (String.IsNullOrEmpty(contaDeposito.IdentificacaoProprietario))
            {
                contaDeposito.IdentificacaoProprietario = contaDeposito.Usuario.Nome;
            }
            return Json(new { contaDeposito.Agencia, contaDeposito.Conta, contaDeposito.DigitoConta, contaDeposito.IdentificacaoProprietario, contaDeposito.Bitcoin, contaDeposito.Litecoin, contaDeposito.Tether }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Editar(int id)
        {
            //
            var usuario = usuarioRepository.Get(id);
            var pais = usuario.Pais;
            ViewBag.Estados = estadoRepository.GetByPais(pais.ID).ToList();
            ViewBag.Filiais = filialRepository.GetAll().ToList();

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(pais.Idioma);
            var culture = new System.Globalization.CultureInfo(pais.Idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Session["estado"] = usuario.EnderecoPrincipal.EstadoID;

            return View(usuario);
        }

        [HttpPost]
        public ActionResult Editar(FormCollection form)
        {

            var codFranqueado = form["Codigo"];
            var usuario = usuarioRepository.Get(int.Parse(codFranqueado));
            var pais = usuario.Pais;
            string strLogin = usuario.Login;

            ViewBag.Estados = estadoRepository.GetByPais(pais.ID).ToList();
            ViewBag.Filiais = filialRepository.GetAll().ToList();

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(pais.Idioma);
            var culture = new System.Globalization.CultureInfo(pais.Idioma.Sigla);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            usuario.Email = form["Email"];
            usuario.Login = form["Login"];

            usuario.Apelido = form["Apelido"];
            usuario.Celular = form["Celular"];

            //Erro quando usuario é en-US, dai a cultura
            System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("pt-BR");
            usuario.DataNascimento = DateTime.Parse(form["DataNascimento"], cultureinfo);
            usuario.Documento = form["Documento"];
            usuario.Nome = form["Nome"];
            usuario.NomeFantasia = form["NomeFantasia"];
            usuario.FilialID = int.Parse(form["Filial"]);

            usuario.ExibeSaque = form["ExibeSaque"] == "on" ? 1 : 0;

            var usuarioExistente = usuarioRepository.GetByEmail(form["Email"]);
            if (usuarioExistente != null && usuarioExistente.ID != usuario.ID)
            {
                ViewBag.MensagemAlerta = "E-mail já existente";
                return View(usuario);
            }

            var loginExistente = usuarioRepository.GetByLogin(form["Login"]);
            if (loginExistente != null && loginExistente.ID != usuario.ID)
            {
                ViewBag.MensagemAlerta = "Login já existente!";
                return View(usuario);
            }

            if (!string.IsNullOrWhiteSpace(form["Senha"]))
            {
                usuario.Senha = Core.Helpers.CriptografiaHelper.Criptografar(form["Senha"]);
            }
            usuarioRepository.Save(usuario);

            if (strLogin != usuario.Login)
            {
                //Caso haja alteração de login acerta o identity
                Autenticacao autenticacao = db.Autenticacao.Find(usuario.IdAutenticacao);
                if (autenticacao != null)
                {
                    autenticacao.UserName = usuario.Login;
                    db.SaveChanges();
                }
            }

            var endereco = enderecoRepository.Get(usuario.EnderecoPrincipal.ID);

            endereco.CodigoPostal = form["CEP"];
            endereco.Complemento = form["Complemento"];
            endereco.Distrito = form["Distrito"];
            endereco.EstadoID = int.Parse(form["Estado"]);
            endereco.Logradouro = form["Logradouro"];
            endereco.Numero = form["Numero"];
            endereco.CidadeNome = form["CidadeNome"];
            //endereco.CidadeID = int.Parse(form["Cidade"]);
            //endereco.CidadeID = usuarioService.GetCidadeIDPorNome(endereco.EstadoID, endereco.CidadeNome);
            endereco.CidadeID = 1;
            enderecoRepository.Save(endereco);

            return RedirectToAction("Detalhes", new { id = usuario.ID });
        }

        public ActionResult Email(string cf)
        {
            var usuario = usuarioRepository.Get(int.Parse(cf));

            if (usuario != null && (usuario.Status == Usuario.TodosStatus.Indefinido || usuario.Status == Usuario.TodosStatus.NaoAssociado))
            {
                usuarioService.EnviarValidacaoEmail(usuario, Helpers.Local.Sistema);
            }
            return Json("OK");
        }

        public ActionResult Excluir(string cf)
        {
            var usuario = usuarioRepository.Get(int.Parse(cf));

            if (usuario != null && (usuario.Status == Usuario.TodosStatus.Indefinido || usuario.Status == Usuario.TodosStatus.NaoAssociado))
            {
                usuario.Oculto = true;
                usuario.Bloqueado = true;
                usuarioRepository.Save(usuario);
            }
            return Json("OK");
        }

        public ActionResult Bloquear(string cf)
        {
            var usuario = usuarioRepository.Get(int.Parse(cf));

            if (usuario != null)
            {
                usuario.Bloqueado = !usuario.Bloqueado;
                usuarioRepository.Save(usuario);
            }
            return Json("OK");
        }

        public ActionResult Ativar(string UsuAtivarID, string skuProdutoAdesao, int meioPagamento, int paraAvaliacao)
        {
            //perfil = SKU do produto
            string msgRetorno = "NOK";

            try
            {

                #region Carrinho

                var usuario = usuarioRepository.Get(int.Parse(UsuAtivarID));

                if (paraAvaliacao == 1)
                {
                    usuario.GeraBonus = false;
                    usuarioRepository.Save(usuario);
                }

                CarrinhoModel carrinho = new CarrinhoModel(usuario);

                PedidoPagamento.MeiosPagamento meiopgto = (PedidoPagamento.MeiosPagamento)meioPagamento;

                #endregion

                #region Pagamento

                var produto = produtoRepository.GetBySKU(skuProdutoAdesao);
                var valor = produto.ValorMinimo(usuario);

                carrinho.EnderecoEntrega = usuario.EnderecoPrincipal;
                carrinho.EnderecoFaturamento = usuario.EnderecoPrincipal;
                carrinho.Adicionar(produto, valor);
                carrinho.Adicionar(meiopgto, PedidoPagamento.FormasPagamento.Padrao);

                var pedido = pedidofactory.Criar(carrinho);

                var pagamento = pedido.PedidoPagamento.FirstOrDefault();

                #endregion

                #region Boleto

                if (meiopgto == PedidoPagamento.MeiosPagamento.Boleto)
                {
                    DateTime dataVencimento = App.DateTimeZion.AddDays(5);
                    if (paraAvaliacao == 1)
                    {
                        dataVencimento = App.DateTimeZion.AddDays(30);
                    }

                    var boleto = pagamento.Boleto.FirstOrDefault(b => b.StatusID == (int)Boleto.TodosStatus.AguardandoPagamento);
                    if (boleto == null)
                    {
                        var numeroDocumento = boletoRepository.GetMaxID() + 1;
                        boleto = new Boleto()
                        {
                            DataCriacao = App.DateTimeZion,
                            DataVencimento = dataVencimento,
                            NossoNumero = numeroDocumento.ToString("00000000"),
                            NumeroDocumento = numeroDocumento,
                            PedidoPagamentoID = pagamento.ID,
                            Status = Boleto.TodosStatus.AguardandoPagamento,
                            ValorPago = 0,
                            ValorTotal = pagamento.Valor
                        };
                        boletoRepository.Save(boleto);
                    }
                }

                #endregion

                #region Avaliacao

                if (paraAvaliacao == 1) //pedidos em avaliação, não podem ser aprovados. Ficam em aberto.
                {
                    #region apenas ativa usuário
                    usuarioService.Associar(usuario.ID, produto.NivelAssociacao);
                    usuario.GeraBonus = false;
                    usuario.RecebeBonus = true;
                    usuarioRepository.Save(usuario);
                    #endregion
                }
                else
                {
                    bool ret = pedidoService.ProcessarPagamento(pagamento.ID, Core.Entities.PedidoPagamentoStatus.TodosStatus.Pago);
                }

                #endregion

                #region Cancelar demais kits

                //por fim, cancela os outros pedidos (de kits de associação) que eventualmente estejam em aberto

                var pedidosUsuario = pedidoRepository.GetByExpression(p => p.UsuarioID == usuario.ID).ToList();
                var pedidosEmAberto = pedidosUsuario.Where(p => p.StatusAtual == PedidoPagamentoStatus.TodosStatus.Indefinido || p.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoConfirmacao || p.StatusAtual == PedidoPagamentoStatus.TodosStatus.AguardandoPagamento).ToList();

                var pedidosCancelar = new List<Pedido>();
                foreach (var p in pedidosEmAberto)
                {
                    foreach (var i in p.PedidoItem)
                    {
                        if (i.Produto.Tipo == Produto.Tipos.Associacao || i.Produto.Tipo == Produto.Tipos.Upgrade)
                        {
                            pedidosCancelar.Add(p);
                            break;
                        }
                    }
                }

                foreach (var pCancelar in pedidosCancelar)
                {
                    var pgtoCancelar = pCancelar.PedidoPagamento.FirstOrDefault();
                    if (pgtoCancelar != null)
                    {
                        var pps = new PedidoPagamentoStatus()
                        {
                            PedidoPagamentoID = pgtoCancelar.ID,
                            Status = PedidoPagamentoStatus.TodosStatus.Cancelado,
                            Data = App.DateTimeZion,
                        };
                        pedidoPagamentoStatusRepository.Save(pps);
                    }
                }

                #endregion

                msgRetorno = "OK";
            }
            catch (Exception ex)
            {
                msgRetorno = ex.Message;
            }

            return Json(msgRetorno);
        }

        public ContentResult AlterarPlano(string cf, int perfil)
        {
            //perfil = SKU do produto
            string msgRetorno = "NOK";

            try
            {
                var usuarioAlteracao = usuarioRepository.Get(int.Parse(cf));

                LoggerHelper.WriteFile(string.Format("{0} {1} {2} {3} {4} {5} de {6} para {7}", App.DateTimeZion, this.Request.UserHostAddress, Request.Browser.Platform, Request.Browser.Browser, usuarioAlteracao.ID, usuarioAlteracao.Login, usuarioAlteracao.NivelAssociacao, perfil), "AdminUsuariosController");

                usuarioAlteracao.NivelAssociacao = perfil;
                usuarioRepository.Save(usuarioAlteracao);

                UsuarioAssociacao ua = new UsuarioAssociacao() { UsuarioID = usuarioAlteracao.ID, NivelAssociacao = perfil, Manual = true, Data = App.DateTimeZion };
                usuarioAssociacaoRepository.Save(ua);

                msgRetorno = "OK";
            }
            catch (Exception ex)
            {
                msgRetorno = "NOK";
            }

            return Content(msgRetorno);
        }

        public ActionResult ContaAdminstrativa(string cf)
        {
            var usuario = usuarioRepository.Get(int.Parse(cf));
            if (usuario != null)
            {
                usuario.DataValidade = DateTime.Parse("2100-01-01 00:00:00");
                usuario.DataRenovacao = DateTime.Parse("2100-01-01 00:00:00");
                usuarioRepository.Save(usuario);
            }
            return Json("OK");
        }

        public ActionResult CidadeNome(string term)
        {
            int estadoId = 0;

            if (Session["estado"] != null)
            {
                estadoId = (int)Session["estado"];
            }

            int intEstadoId = estadoId; // Convert.ToInt32(estadoId);
            var cidades = cidadeRepository.GetByEstado(estadoId).OrderBy(x => x.Nome);
            List<string> tags = new List<string>();
            foreach (var item in cidades)
            {
                tags.Add(item.Nome);
            }

            var filteredItems = tags.Where(
                item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0
                );

            return Json(filteredItems, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Excel(string CurrentProcuraLogin, string CurrentProcuraPatrocinador)
        {
            //*Lista para montar excel
            IQueryable<Usuario> lista = null;

            lista = db.Usuarios.Include(u => u.Pais).Include(u => u.PatrocinadorDireto).Include(u => u.PatrocinadorPosicao).Include(u => u.UltimoDireita).Include(u => u.UltimoEsquerda).Include(u => u.Filial).Include(u => u.Autenticacao);

            if (!String.IsNullOrEmpty(CurrentProcuraLogin) && !String.IsNullOrEmpty(CurrentProcuraPatrocinador))
            {
                lista = lista.Where(x => x.PatrocinadorDireto.Login.Contains(CurrentProcuraPatrocinador) &&
                                       (x.Login.Contains(CurrentProcuraLogin) ||
                                         x.Nome.Contains(CurrentProcuraLogin) ||
                                         x.Email.Contains(CurrentProcuraLogin)
                                       )
                                    );
            }
            else
            {
                if (!String.IsNullOrEmpty(CurrentProcuraLogin))
                {
                    lista = lista.Where(x => x.Login.Contains(CurrentProcuraLogin) ||
                                             x.Nome.Contains(CurrentProcuraLogin) ||
                                             x.Email.Contains(CurrentProcuraLogin)
                                       );
                }
                if (!String.IsNullOrEmpty(CurrentProcuraPatrocinador))
                {
                    lista = lista.Where(x => x.PatrocinadorDireto.Nome.Contains(CurrentProcuraPatrocinador));
                }
            }


            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["USUARIOS"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 8;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["USUARIOS"]);

            //Range do Cabeçalho da planilha
            var objHeadLine = objWorkSheet.Range(objWorkSheet.Cell(2, 1).Address, objWorkSheet.Cell(2, intTotColumns).Address);

            //Formata cabeçalho do relatorio
            objHeadLine.Style.Font.Bold = true;
            objHeadLine.Style.Font.FontSize = 14;
            objHeadLine.Style.Font.FontColor = XLColor.White;
            objHeadLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            objHeadLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            objHeadLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
            objHeadLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            objHeadLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;
            objHeadLine.Merge();
            objHeadLine.Value = strReportHeader;

            //Nome das Colunas 
            objWorkSheet.Cell(4, 1).Value = traducaoHelper["LOGIN"];
            objWorkSheet.Cell(4, 2).Value = traducaoHelper["NOME"];
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["EMAIL"];
            objWorkSheet.Cell(4, 4).Value = traducaoHelper["CELULAR"];
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["PATROCINADOR"];
            objWorkSheet.Cell(4, 6).Value = traducaoHelper["ASSOCIACAO"];
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["CLASSIFICACAO"];
            objWorkSheet.Cell(4, 8).Value = traducaoHelper["STATUS"];

            //formata na linha dos nomes dos campos
            var columnRange = objWorkSheet.Range(objWorkSheet.Cell(4, 1).Address, objWorkSheet.Cell(4, intTotColumns).Address);
            columnRange.Style.Font.Bold = true;
            columnRange.Style.Font.FontSize = 10;
            columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);
            columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            var Associacoes = associacaoRepository.GetAll().ToList();
            var Classificacoes = classificacaoRepository.GetAll().ToList();

            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                var associacao = Associacoes.FirstOrDefault(a => a.Nivel == objFor.NivelAssociacao);
                var classificacao = Classificacoes.FirstOrDefault(c => c.Nivel == objFor.NivelClassificacao);

                objWorkSheet.Cell(intLinha, 1).Value = objFor.Login;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.Nome;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.Email;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.Celular;
                objWorkSheet.Cell(intLinha, 5).Value = objFor.PatrocinadorDireto != null ? objFor.PatrocinadorDireto.Nome : "";
                objWorkSheet.Cell(intLinha, 6).Value = associacao.Nome;
                objWorkSheet.Cell(intLinha, 7).Value = classificacao.Nome;
                objWorkSheet.Cell(intLinha, 8).Value = (objFor.Bloqueado ? traducaoHelper["BLOQUEADO"] : objFor.Status.ToString());

                if (intLinha % 2 == 0)
                {
                    //coloca cor nas linhas impares
                    var dataRowRangeImp = objWorkSheet.Range(objWorkSheet.Cell(intLinha, 1).Address, objWorkSheet.Cell(intLinha, intTotColumns).Address);
                    dataRowRangeImp.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                }
            }

            //Formata range dos valores preenchidos
            var dataRowRange = objWorkSheet.Range(objWorkSheet.Cell(5, 1).Address, objWorkSheet.Cell(intTotRows + 4, intTotColumns).Address);
            dataRowRange.Style.Font.Bold = false;
            dataRowRange.Style.Font.FontSize = 10;
            dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            objWorkSheet.Columns().AdjustToContents();

            // Preparação para o response
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"" + strReportHeader + ".xlsx\"");

            // Planilha vai para memoria
            using (MemoryStream memoryStream = new MemoryStream())
            {
                objWorkBook.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                memoryStream.Close();
            }
            //Planilha vai para download
            Response.End();

            //Retorna a lista
            return RedirectToAction("Index");

        }

        public ActionResult Reload()
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Senha(int? id)
        {
            //Idioma
            Localizacao();

            try
            {
                #region Variaveis

                var emailService = new Core.Services.Sistema.EmailService();
                string tipoEnvio = ConfiguracaoHelper.GetString("EMAIL_TIPO_ENVIO");

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                bool blnContinua = true;

                Usuario usuario = db.Usuarios.Find(id); //usuarioRepository.Get(id.Value);

                if (usuario == null)
                {
                    return HttpNotFound();
                }

                string strSenha = Gerais.GerarSenha;

                #endregion

                #region Autenticacao

                var user = await UserManager.FindByIdAsync(usuario.IdAutenticacao);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var code = await UserManager.GeneratePasswordResetTokenAsync(usuario.IdAutenticacao);
                var result = await UserManager.ResetPasswordAsync(usuario.IdAutenticacao, code, strSenha);

                if (result.Succeeded)
                {
                    blnContinua = true;
                }
                else
                {
                    string strErro = "";
                    //Msg não foi possivel alterar senha
                    foreach (var error in result.Errors)
                    {
                        strErro += error + ";";
                    }
                    string[] strMensagem2 = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], traducaoHelper["MENSAGEM_ERRO"] + " " + strErro };
                    Mensagem(traducaoHelper["SENHA"], strMensagem2, "msg");
                }

                #endregion

                #region Usuario

                if (blnContinua)
                {
                    string strMensagem = String.Format(traducaoHelper["MENSAGEM_TROCA_SENHA"] + ConfiguracaoHelper.GetString("NOME_SITE") + "<br />", usuario.Nome, usuario.Login, strSenha);

                    //Senha do Usuario deve ser criptografada
                    strSenha = CriptografiaHelper.Criptografar(strSenha);
                    usuario.Senha = strSenha;
                    usuario.Bloqueado = false;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    string strRet = string.Empty;

                    if (tipoEnvio.ToUpper() == "SMTP")
                    {
                        emailService.Send(ConfiguracaoHelper.GetString("EMAIL_DE"), usuario.Email, traducaoHelper["EMAIL_SENHA_ASSUNTO"], strMensagem);
                    }
                    else
                    {
                        if (tipoEnvio.ToUpper() == "SENDGRID")
                        {
                            var response = await SendGridService.SendGridEnviaSync(ConfiguracaoHelper.GetString("SENDGRID_API_KEY"), traducaoHelper["EMAIL_SENHA_ASSUNTO"], strMensagem, ConfiguracaoHelper.GetString("EMAIL_DE"), usuario.Email);

                            if (response.StatusCode != HttpStatusCode.Accepted)
                            {
                                #region Erro
                                string[] strErros = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], " " + response.StatusCode.ToString() };
                                Mensagem(traducaoHelper["ERRO"], strErros, "err");
                                #endregion
                            }
                        }
                    }

                    string[] strMensagem2 = new string[] { traducaoHelper["TROCAR_ENVIAR_SENHA_SUCESSO"], "", " " + traducaoHelper["SENHA_ENVIADA_EMAIL"] + ":", "", usuario.Email };
                    Mensagem(traducaoHelper["SUCESSO"], strMensagem2, "msg");

                    #endregion

                }

            }
            catch (DbEntityValidationException dbEx)
            {
                #region Erro

                string strErro = "";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        strErro = "Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage;
                    }

                    string[] strErros = new string[] { traducaoHelper["MENSAGEM_ERRO"], strErro };
                    Mensagem(traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"], strErros, "ale");
                }

                #endregion
            }
            catch (Exception ex)
            {
                #region Erro
                string[] strErros = new string[] { traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"] };
                Mensagem(traducaoHelper["ERRO"], strErros, "err");
                #endregion
            }

            return RedirectToAction("Index");
        }

        public bool VerificaAutenticacao2FA(string token)
        {
            if (Core.Helpers.ConfiguracaoHelper.GetBoolean("AUTENTICACAO_DOIS_FATORES"))
            {
                return UserManager.VerifyTwoFactorToken(Local.idUsuario, "GoogleAuthenticator", token);
            }
            else
            {
                return true;
            }
        }

        public async Task<ActionResult> AlterarSenha(int id, string senha, string token2F)
        {
            //Idioma
            Localizacao();

            bool blnContinua = true;
            string strMensagem = "";

            try
            {
                Usuario usuario = db.Usuarios.Find(id); //usuarioRepository.Get(id.Value);

                if (usuario == null)
                {
                    return HttpNotFound();
                }

                if (VerificaAutenticacao2FA(token2F))
                {
                    var user = await UserManager.FindByIdAsync(usuario.IdAutenticacao);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    var code = await UserManager.GeneratePasswordResetTokenAsync(usuario.IdAutenticacao);
                    var result = await UserManager.ResetPasswordAsync(usuario.IdAutenticacao, code, senha);

                    blnContinua = result.Succeeded;


                    if (blnContinua)
                    {
                        //Senha do Usuario deve ser criptografada
                        senha = CriptografiaHelper.Criptografar(senha);
                        usuario.Senha = senha;
                        usuario.Bloqueado = false;
                        db.Entry(usuario).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        strMensagem = string.Join("<br />", result.Errors);
                    }
                }
                else
                {
                    blnContinua = false;
                    strMensagem = traducaoHelper["TOKEN_INVALIDO"];
                }
            }
            catch (Exception ex)
            {
                blnContinua = false;
                strMensagem = traducaoHelper["NAO_FOI_POSSIVEL_ALTERAR_SENHA"];
            }

            return Json(new { OK = blnContinua, msg = (blnContinua ? traducaoHelper["SENHA_ALTERADA_SUCESSO"] : strMensagem) });
        }

        public ActionResult DesabilitaAutenticacao(int id)
        {
            try
            {
                usuarioRepository.TwoFactorEnabled(id, false);

                string[] strMensagem = new string[] { traducaoHelper["SOLICITACAO_EFETUADA_SUCESSO"] };
                Mensagem(traducaoHelper["SUCESSO"], strMensagem, "msg");
            }
            catch (Exception ex)
            {
                string[] strMensagem = new string[] { traducaoHelper["MENSAGEM_ERRO"] + " " + ex.Message };
                Mensagem(traducaoHelper["FALHA"], strMensagem, "msg");
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Json

        public JsonResult Cidades(int estadoID)
        {
            Session["estado"] = estadoID;
            var retorno = new List<Cidade>();
            var cidades = cidadeRepository.GetByEstado(estadoID).ToList();
            cidades.ForEach(c => retorno.Add(new Cidade(c)));
            return Json(retorno);
        }
        #endregion

    }
}
