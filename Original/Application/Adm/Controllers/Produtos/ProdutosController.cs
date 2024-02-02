#region Bibliotecas

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;

//Identyty
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

//Entity
using Core.Entities;
using Core.Repositories.Loja;
using Core.Repositories.Sistema;
using Core.Repositories.Rede;

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
using Core.Helpers;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ProdutosController : Controller
    {
        #region Variaveis

        private string cstrFalha = "";
        private Boolean cblnCadastroResunido;
        private ArquivoSecaoRepository arquivoSecaoRepository;
        private ProdutoTipoRepository produtoTipoRepository;

        private List<string> cstrValidacao = new List<string>();

        #endregion

        #region Core

        private AssociacaoRepository associacaoRepository;
        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

        public ProdutosController(DbContext context)
        {
            Localizacao();

            associacaoRepository = new AssociacaoRepository(context);
            arquivoSecaoRepository = new ArquivoSecaoRepository(context);
            produtoTipoRepository = new ProdutoTipoRepository(context);

            cblnCadastroResunido = Core.Helpers.ConfiguracaoHelper.GetBoolean("LOJA_PRODUTO_CADASTRO_RESUMIDO");
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

        private void Localizacao()
        {
            Core.Entities.Idioma idioma = Local.UsuarioIdioma;

            ViewBag.TraducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private bool GravaImagemFoto(HttpPostedFileBase imgFile, string caminhoFisico, string strSKU)
        {
            bool blnContinua = true;

            try
            {
                //string strPath = caminhoFisico + arquivoSecaoRepository.GetById(8).Caminho.Replace(@"\","/") + strSKU;
                //string strPath = caminhoFisico + Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS") + strSKU;

                string strPath = Path.Combine(caminhoFisico, Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS"), strSKU);

                Helpers.Files.CreateFolderIfNeeded(strPath);

                strPath = Path.Combine(strPath, strSKU + ".jpg");

                strPath = @strPath.Replace("\\", "/");
                if (imgFile != null)
                {
                    //Salva arquivo em file system
                    imgFile.SaveAs(@strPath);
                }
            }
            catch (Exception ex)
            {
                cstrFalha += "Não foi possivel salvar a imagem da FotoErro: " + ex.Message;
                blnContinua = false;
            }

            return blnContinua;
        }
        #endregion

        #region Actions
        // GET: Produtos
        public ActionResult Index(string SortOrder, string CurrentProcuraSKU, string ProcuraSKU, string CurrentProcuraNome, string ProcuraNome, string CurrentProcuraTipo, string ProcuraTipo, int? NumeroPaginas, int? Page)
        {
            // return View(usuarios.ToList());

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Helpers.Funcoes objFuncoes = new Helpers.Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder, ref CurrentProcuraSKU, ref ProcuraSKU, ref CurrentProcuraNome,
                                    ref ProcuraNome, ref CurrentProcuraTipo,
                                    ref ProcuraTipo, ref NumeroPaginas, ref Page, "Produtos");

            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "SKU";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraSKU != null || ProcuraNome != null || ProcuraTipo != null))
            {
                if (ProcuraSKU == null)
                {
                    ProcuraSKU = CurrentProcuraSKU;
                }
                if (ProcuraNome == null)
                {
                    ProcuraNome = CurrentProcuraNome;
                }
                if (ProcuraTipo == null)
                {
                    ProcuraTipo = CurrentProcuraTipo;
                }
            }

            ViewBag.CurrentProcuraSKU = ProcuraSKU;
            ViewBag.CurrentProcuraNome = ProcuraNome;
            ViewBag.CurrentProcuraTipo = ProcuraTipo;

            IQueryable<Produto> lista = null;

            lista = db.Produto.Include(p => p.ProdutoTipo);

            if (!String.IsNullOrEmpty(ProcuraSKU) && !String.IsNullOrEmpty(ProcuraNome) && !String.IsNullOrEmpty(ProcuraTipo))
            {
                lista = lista.Where(x => x.SKU.Contains(ProcuraSKU) && x.Nome.Contains(ProcuraNome) && x.ProdutoTipo.Nome.Contains(ProcuraTipo));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraSKU))
                {
                    lista = lista.Where(x => x.SKU.Contains(ProcuraSKU));
                }
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
                if (!String.IsNullOrEmpty(ProcuraTipo))
                {
                    lista = lista.Where(x => x.ProdutoTipo.Nome.Contains(ProcuraTipo));
                }
            }

            switch (SortOrder)
            {
                case "SKU_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "Nome":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome_desc";
                    lista = lista.OrderBy(x => x.Nome);
                    break;
                case "Nome_desc":
                    ViewBag.FirstSortParm = "SKU";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderByDescending(x => x.Nome);
                    break;
                case "SKU":
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";

                    lista = lista.OrderBy(x => x.SKU);
                    break;
                default:  // Name ascending 
                    ViewBag.FirstSortParm = "SKU_desc";
                    ViewBag.SecondSortParm = "Nome";
                    lista = lista.OrderBy(x => x.SKU);
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
            ViewBag.NumeroPaginas = new SelectList(db.Paginacao, "valor", "nome", intNumeroPaginas);
            ViewBag.ProcuraTipo = new SelectList(db.ProdutoTipo, "Nome", "Nome");
            return View(lista.ToPagedList(PageNumber, PageSize));
        }

        // GET: Produtos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produto Produto = db.Produto.Find(id);
            if (Produto == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProdutoItens = Produto.Itens();
            ViewBag.CadastroResunido = cblnCadastroResunido;

            return View(Produto);
        }

        // GET: Produtos/Create
        public ActionResult Create()
        {
            ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            ViewBag.CadastroResunido = cblnCadastroResunido;
            //ViewBag.ProdutoEdit = false;

            return View();
        }

        // POST: Produtos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,SKU,TipoID,NivelAssociacao,Nome,Chamada,Descricao,Peso,Destaque,LimitePorUsuario,LimitePorPedido,ControlaEstoque,Estoque,Publicado,DataCriacao,DataPublicacao,VendaDireta,Observacao,ProdutoCategoriaID,Dimensoes,AtivoMensal,Composto,CodigoExterno")] Produto Produto)
        {
            if (ModelState.IsValid)
            {
                Produto.DataCriacao = App.DateTimeZion;

                if (Produto.Publicado && Produto.DataPublicacao == DateTime.MinValue)
                {
                    Produto.DataPublicacao = App.DateTimeZion;
                }

                db.Produto.Add(Produto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TipoID = new SelectList(db.ProdutoTipo, "ID", "Nome", Produto.TipoID);
            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria, "ID", "Nome", Produto.ProdutoCategoriaID);

            return View(Produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalvarGeral(FormCollection form)
        {
            #region Variaveis
            //Produto
            string strID = form["ID"];
            string strSKU = form["SKU"];
            string strTipoID = form["ProdutoTipo"];
            string strNivelAssociacao = form["NivelAssociacao"];
            string strNome = form["Nome"];
            string strChamada = form["Chamada"];
            string strDescricao = form["Descricao"];
            string strPeso = form["Peso"];
            string strDestaque = form["Destaque"];
            string strLimitePorUsuario = form["LimitePorUsuario"];
            string strLimitePorPedido = form["LimitePorPedido"];
            string strControlaEstoque = form["ControlaEstoque"];
            string strEstoque = form["Estoque"];
            string strPublicado = form["Publicado"];
            string strVendaDireta = form["VendaDireta"];
            string strObservacao = form["Observacao"];
            string strProdutoCategoriaID = form["ProdutoCategoria"];
            string strDimensoes = form["Dimensoes"];
            string strAtivoMensal = form["AtivoMensal"];
            string strComposto = form["Composto"];
            string strDataPublicacao = form["DataPublicacao"];
            string strDataCriacao = form["DataCriacao"];
            string strCodigoExterno = form["CodigoExterno"];
            #endregion

            #region Salva Produto
            Produto produto = new Produto();

            if (strID != null)
                produto.ID = int.Parse(strID);

            produto.SKU = strSKU;
            produto.TipoID = int.Parse(strTipoID);
            produto.NivelAssociacao = int.Parse(strNivelAssociacao);
            produto.Nome = strNome;
            produto.Chamada = strChamada != null ? strChamada : "";
            produto.Descricao = strDescricao != null ? strDescricao : "";
            produto.Peso = strPeso != "" ? double.Parse(strPeso) : 0.0;
            produto.Destaque = strDestaque == "on" ? true : false;
            produto.LimitePorUsuario = strLimitePorUsuario != "" ? int.Parse(strLimitePorUsuario) : 9999;
            produto.LimitePorPedido = strLimitePorPedido != "" ? int.Parse(strLimitePorPedido) : 9999;
            produto.ControlaEstoque = strControlaEstoque == "on" ? true : false;
            produto.Estoque = strEstoque != "" ? int.Parse(strEstoque) : 9999;
            produto.Publicado = strPublicado == "on" ? true : false;
            produto.VendaDireta = strVendaDireta == "on" ? true : false;
            produto.Observacao = strObservacao != null ? strObservacao : "";
            produto.ProdutoCategoriaID = int.Parse(strProdutoCategoriaID);
            produto.Dimensoes = strDimensoes != null ? strDimensoes : "";
            produto.AtivoMensal = strAtivoMensal == "on" ? true : false;
            produto.Composto = strComposto == "on" ? true : false;
            produto.CodigoExterno = strCodigoExterno != null ? strCodigoExterno : "";


            //if (produto.Publicado && strDataPublicacao.Length == 0)
            produto.DataPublicacao = App.DateTimeZion;

            if (strDataCriacao.Length == 0)
                produto.DataCriacao = App.DateTimeZion;
            else
                produto.DataCriacao = DateTime.Parse(strDataCriacao);

            #endregion

            if (produto.ID > 0)
                db.Entry(produto).State = EntityState.Modified;
            else
                db.Produto.Add(produto);

            db.SaveChanges();

            var sigla = produtoTipoRepository.Get(produto.TipoID).Sigla;
            if (produto.SKU != sigla + produto.ID.ToString("D4"))
            {
                produto.SKU = sigla + produto.ID.ToString("D4");
                db.Entry(produto).State = EntityState.Modified;

                db.SaveChanges();
            }

            ViewBag.CadastroResunido = cblnCadastroResunido;
            ViewBag.ProdutoEdit = true;
            ViewBag.ProdutoID = produto.ID;

            return RedirectToAction("Index");
        }
        // GET: Produtos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produto Produto = db.Produto.Find(id);
            if (Produto == null)
            {
                return HttpNotFound();
            }

            ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            ViewBag.ProdutoItens = Produto.Itens();
            ViewBag.CadastroResunido = cblnCadastroResunido;

            if (Produto.SKU != null && Produto.SKU.Trim().Length > 0)
            {
                string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
                string caminhoVirtual = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO") + Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
                string diretorio = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS") + Produto.SKU + "/";
                string strVersao = "?id=" + Gerais.GerarChave();

                var retorno = ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, Produto.SKU + ".jpg");
                if (retorno.Any())
                {
                    ViewBag.Foto = retorno.Any();
                }
                else
                {
                    retorno = ArquivoRepository.BuscarArquivos(caminhoFisico, caminhoVirtual, diretorio, Produto.SKU + ".png");
                    ViewBag.Foto = retorno.Any() ? retorno.FirstOrDefault() + strVersao : null;
                }
            }
            else
            {
                ViewBag.Foto = "";
            }

            return View(Produto);
        }

        // POST: Produtos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,SKU,TipoID,NivelAssociacao,Nome,Chamada,Descricao,Peso,Destaque,LimitePorUsuario,LimitePorPedido,ControlaEstoque,Estoque,Publicado,DataCriacao,DataPublicacao,VendaDireta,Observacao,ProdutoCategoriaID,Dimensoes,AtivoMensal,Composto,CodigoExterno")] Produto Produto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Produto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TipoID = new SelectList(db.ProdutoTipo, "ID", "Nome");
            ViewBag.CadastroResunido = cblnCadastroResunido;

            return View(Produto);
        }

        // GET: Produtos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produto Produto = db.Produto.Find(id);
            if (Produto == null)
            {
                return HttpNotFound();
            }
            ViewBag.CadastroResunido = cblnCadastroResunido;

            return View(Produto);
        }

        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Produto Produto = db.Produto.Find(id);
            db.Produto.Remove(Produto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Produtos/Composicao
        public ActionResult Composicao(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produto Produto = db.Produto.Find(id);
            if (Produto == null)
            {
                return HttpNotFound();
            }

            //ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            //ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            //ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            ViewBag.ProdutoItens = Produto.Itens();
            //ViewBag.CadastroResunido = cblnCadastroResunido;

            return View(Produto);
        }

        public ActionResult Excel(string ProcuraSKU, string ProcuraNome, string ProcuraTipo)
        {
            //*Lista para montar excel          
            IQueryable<Produto> lista = null;

            lista = db.Produto.Include(p => p.ProdutoTipo);

            if (!String.IsNullOrEmpty(ProcuraSKU) && !String.IsNullOrEmpty(ProcuraNome) && !String.IsNullOrEmpty(ProcuraTipo))
            {
                lista = lista.Where(x => x.SKU.Contains(ProcuraSKU) && x.Nome.Contains(ProcuraNome) && x.ProdutoTipo.Nome.Contains(ProcuraTipo));
            }
            else
            {
                if (!String.IsNullOrEmpty(ProcuraSKU))
                {
                    lista = lista.Where(x => x.SKU.Contains(ProcuraSKU));
                }
                if (!String.IsNullOrEmpty(ProcuraNome))
                {
                    lista = lista.Where(x => x.Nome.Contains(ProcuraNome));
                }
                if (!String.IsNullOrEmpty(ProcuraTipo))
                {
                    lista = lista.Where(x => x.ProdutoTipo.Nome.Contains(ProcuraTipo));
                }
            }


            if (lista == null)
            {
                return HttpNotFound();
            }

            //*Titulo do relatorio
            string strReportHeader = traducaoHelper["PRODUTO"];

            //Total de linhas
            int intTotRows = lista.Count();

            //*Total de colunas - Verificar quantas colunas a planilha terá
            int intTotColumns = 8;

            XLWorkbook objWorkBook = new XLWorkbook();
            //XLWorkbook objWorkBook = new XLWorkbook(filepath);

            //*Nome da aba da planilha
            var objWorkSheet = objWorkBook.Worksheets.Add(traducaoHelper["PRODUTO"]);

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
            objWorkSheet.Cell(4, 1).Value = traducaoHelper["SKU"];
            objWorkSheet.Cell(4, 2).Value = traducaoHelper["NOME"];
            objWorkSheet.Cell(4, 3).Value = traducaoHelper["TIPO"];
            objWorkSheet.Cell(4, 4).Value = traducaoHelper["CHAMADAS"];
            objWorkSheet.Cell(4, 5).Value = traducaoHelper["DESCRICAO"];
            objWorkSheet.Cell(4, 6).Value = traducaoHelper["PUBLICADO"];
            objWorkSheet.Cell(4, 7).Value = traducaoHelper["DATA_CRIACAO"];
            objWorkSheet.Cell(4, 8).Value = traducaoHelper["NIVEL_ASSOCIACAO"];

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

            int intLinha = 4;
            //*Preenche planilha com os valores
            foreach (var objFor in lista)
            {
                intLinha++;

                var associacao = Associacoes.FirstOrDefault(a => a.Nivel == objFor.NivelAssociacao);

                objWorkSheet.Cell(intLinha, 1).Value = objFor.SKU;
                objWorkSheet.Cell(intLinha, 2).Value = objFor.Nome;
                objWorkSheet.Cell(intLinha, 3).Value = objFor.ProdutoTipo.Nome;
                objWorkSheet.Cell(intLinha, 4).Value = objFor.Chamada;
                objWorkSheet.Cell(intLinha, 5).Value = objFor.Descricao;
                objWorkSheet.Cell(intLinha, 6).Value = objFor.Publicado;
                objWorkSheet.Cell(intLinha, 7).Value = objFor.DataCriacao;
                objWorkSheet.Cell(intLinha, 8).Value = associacao.Nome;

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




        #region Crop

        public ActionResult CropImage()
        {
            string imagePath = Request["imagePath"];

            string strFile = imagePath.Substring(imagePath.LastIndexOf("/") + 1);
            string strPasta = strFile.Substring(0, strFile.LastIndexOf("."));
            string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
            //string strPath       = arquivoSecaoRepository.GetById(8).Caminho.Replace(@"\", "/");
            string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS");

            imagePath = caminhoFisico + strPath + strPasta + "/" + strFile;

            //System.Drawing.Image imgInfo = System.Drawing.Image.FromFile(Server.MapPath(imagePath));
            System.Drawing.Image imgInfo = System.Drawing.Image.FromFile(imagePath);

            int imgWidth = imgInfo.Width;
            int imgHeight = imgInfo.Height;
            imgInfo.Dispose();

            int intLado = 0;

            string strX1 = Request["x1"];
            string strY1 = Request["y1"];
            string strX2 = Request["x2"];
            string strY2 = Request["y2"];
            string strW = Request["coordsw"];
            string strH = Request["coordsh"];

            //Fixo no js, ex. 1580
            string strLarguraModelo = Request["largura"];
            //Fixo no js, ex. 800
            string strAlturaModelo = Request["altura"];

            //Fixo no js, ex. 385 (1580/4) é uma escala reduzida da imgem final
            string strLarguraPreview = Request["larguraPreview"];
            //Fixo no js, ex. 200 (800/4) é uma escala reduzida da imgem final
            string strAlturaPreview = Request["alturaPreview"];
            //Largura da imagem carregada na tela
            string strLarguraImagem = Request["larguraImagem"];
            //Altura da imagem carregada na tela (é fixo no style da img. ex 200)
            string strAlturaImagem = Request["alturaImagem"];

            //Acerta pontuação caso houver
            strX1 = strX1.Replace(".", ",");
            strY1 = strY1.Replace(".", ",");
            strX2 = strX2.Replace(".", ",");
            strY2 = strY2.Replace(".", ",");
            strLarguraModelo = strLarguraModelo.Replace(".", ",");
            strAlturaModelo = strAlturaModelo.Replace(".", ",");
            strLarguraPreview = strLarguraPreview.Replace(".", ",");
            strAlturaPreview = strAlturaPreview.Replace(".", ",");
            strAlturaImagem = strAlturaImagem.Replace(".", ",");

            double dblX1 = Convert.ToDouble(strX1);
            double dblY1 = Convert.ToDouble(strY1);
            double dblX2 = Convert.ToDouble(strX2);
            double dblY2 = Convert.ToDouble(strY2);

            double dblLarguraModelo = Convert.ToDouble(strLarguraModelo);
            double dblAlturaModelo = Convert.ToDouble(strAlturaModelo);

            double dblLarguraPreview = Convert.ToDouble(strLarguraPreview);
            double dblAlturaPreview = Convert.ToDouble(strAlturaPreview);

            double dblLarguraImagem = Convert.ToDouble(strLarguraImagem);
            double dblAlturaImagem = Convert.ToDouble(strAlturaImagem);

            double dblLado = 0;

            //Lado menor
            if (imgWidth > imgHeight)
            {
                intLado = imgHeight;
                dblLado = dblAlturaImagem;
            }
            else
            {
                intLado = imgWidth;
                dblLado = dblLarguraImagem;
            }

            double dblEscala = intLado / dblLado;

            dblX1 = dblX1 * dblEscala;
            dblY1 = dblY1 * dblEscala;
            dblX2 = dblX2 * dblEscala;
            dblY2 = dblY2 * dblEscala;

            int? X1 = Convert.ToInt32(Math.Round(dblX1));
            int? Y1 = Convert.ToInt32(Math.Round(dblY1));
            int? X2 = Convert.ToInt32(Math.Round(dblX2)) - X1;
            int? Y2 = Convert.ToInt32(Math.Round(dblY2)) - Y1;

            if (string.IsNullOrEmpty(imagePath)
                || !X1.HasValue
                || !Y1.HasValue
                || !X2.HasValue
                || !Y2.HasValue)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            //byte[] imageBytes = System.IO.File.ReadAllBytes(Server.MapPath(imagePath));
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            byte[] croppedImage = Imagens.CropImage(imageBytes, X1.Value, Y1.Value, X2.Value, Y2.Value);

            //string tempFolderName = ConfigurationManager.AppSettings["pathFoto"];
            //tempFolderName = Server.MapPath(tempFolderName);

            //string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            //string fileName = Path.GetFileName(imagePath).Replace(fileNameWithoutExtension, fileNameWithoutExtension);

            try
            {
                //Files.SaveFile(croppedImage, Path.Combine(tempFolderName, fileName));
                Files.SaveFile(croppedImage, imagePath);
            }
            catch (Exception ex)
            {
                //Log an error     
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            //PopulaViewBags();

            Produto Produto = db.Produto.Find(int.Parse(strPasta.Substring(3)));

            ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            ViewBag.ProdutoItens = Produto.Itens();
            ViewBag.CadastroResunido = cblnCadastroResunido;

            string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
            string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
            string strVersao = "?id=" + Gerais.GerarChave();
            ViewBag.Foto = strdominio + strCdn + strPath + strPasta + "//" + strFile + strVersao;

            obtemMensagem();

            return View("Edit", Produto);
        }

        public ActionResult GravarImagem(HttpPostedFileBase imgFoto, FormCollection form)
        {
            bool blnContinua = true;
            var caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO");
            //var diretorio     = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS");

            var strID = form["UsuarioID"];
            Produto Produto = db.Produto.Find(int.Parse(strID));
            if (Produto == null)
            {
                cstrFalha += "Não foi possivel salvar a imagem da Foto. Erro: ID invalido";
                blnContinua = false;
            }
            else
            {
                #region checa dimensoes das imagens
                try
                {
                    if (blnContinua)
                    {
                        blnContinua = GravaImagemFoto(imgFoto, caminhoFisico, Produto.SKU);
                    }
                }
                catch (Exception ex)
                {
                    blnContinua = false;
                    cstrFalha += " Não foi possivel salvar os dados. Erro: " + ex.Message;
                    cstrValidacao.Add(cstrFalha);
                    cstrFalha = "";
                }
                #endregion
            }

            #region Finalizando

            if (!blnContinua)
            {
                Mensagem("Inconsitência", cstrValidacao.ToArray(), "ale");
            }

            ViewBag.TipoID = new SelectList(db.ProdutoTipo.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.ProdutoCategoriaID = new SelectList(db.ProdutoCategoria.OrderBy(o => o.Nome), "ID", "Nome");
            ViewBag.NivelAssociacao = new SelectList(db.Associacao.OrderBy(o => o.Nome), "Nivel", "Nome");
            ViewBag.ProdutoItens = Produto.Itens();
            ViewBag.CadastroResunido = cblnCadastroResunido;

            string strdominio = Core.Helpers.ConfiguracaoHelper.GetString("DOMINIO");
            string strCdn = Core.Helpers.ConfiguracaoHelper.GetString("URL_CDN");
            //string strPath = arquivoSecaoRepository.GetById(8).Caminho.Replace(@"\", "//");
            string strPath = Core.Helpers.ConfiguracaoHelper.GetString("PASTA_PRODUTOS") + "/";
            string strVersao = "?id=" + Gerais.GerarChave();
            ViewBag.Foto = strdominio + strCdn + strPath + Produto.SKU + "//" + Produto.SKU + ".jpg" + strVersao;

            obtemMensagem();

            #endregion

            return View("Edit", Produto);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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

        #endregion
    }
}
