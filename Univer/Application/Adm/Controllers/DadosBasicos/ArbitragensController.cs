
#region Bibliotecas

//Identyty

//Entity
using Core.Entities;
using Helpers;
using Newtonsoft.Json;
//Models Local

//Lista
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Globalization;
//Excel
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador")]
    public class ArbitragensController : Controller
    {
        #region Variaveis

        private YLEVELEntities db = new YLEVELEntities();
        private Core.Helpers.TraducaoHelper traducaoHelper;

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
            var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo("en-US");  //var culture = new System.Globalization.CultureInfo(idioma.Sigla);
            traducaoHelper = new Core.Helpers.TraducaoHelper(idioma);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public class LAssoc
        {
            public string dtInicio { get; set; }
            public string dtfim { get; set; }
            public LArbitragem[] itens { get; set; }
        }
        public class LArbitragem
        {
            public string id { get; set; }
            public string arbitragemPeriodo { get; set; }
            public string nivelAssoc { get; set; }
            public string percSeg { get; set; }
            public string percTer { get; set; }
            public string percQua { get; set; }
            public string percQui { get; set; }
            public string percSex { get; set; }
        }

        #endregion

        #region Actions
        // GET: Blocos
        public ActionResult Index(string SortOrder,
                                  string CurrentProcuraData, string ProcuraData,
                                  string CurrentProcuraValor, string ProcuraValor,
                                  int? NumeroPaginas, int? Page)
        {
            Localizacao();

            //Verifica se a msg em popup para ser exibido na view
            obtemMensagem();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.Persistencia(ref SortOrder,
                                    ref CurrentProcuraData,
                                    ref ProcuraData,
                                    ref CurrentProcuraValor,
                                    ref ProcuraValor,
                                    ref NumeroPaginas,
                                    ref Page,
                                    "Arbitragem");
            objFuncoes = null;

            //List
            if (String.IsNullOrEmpty(SortOrder))
            {
                ViewBag.CurrentSort = "name_desc";
            }
            else
            {
                ViewBag.CurrentSort = SortOrder;
            }

            if (!(ProcuraData != null || ProcuraValor != null))
            {
                if (ProcuraData == null)
                {
                    ProcuraData = CurrentProcuraData;
                }
                if (ProcuraValor == null)
                {
                    ProcuraValor = CurrentProcuraValor;
                }
            }

            ViewBag.CurrentProcuraNome = ProcuraData;
            ViewBag.CurrentProcuraEndereco = ProcuraValor;

            IQueryable<ArbitragemPeriodo> lista = db.ArbitragemPeriodo;
            //var dic = db.Arbitragem.GroupBy(g => g.DataInicio, v => v).ToDictionary(k => k.Key, value => value.ToList());

            // var dic = db.Arbitragem.GroupBy(g => g.DataInicio, v => v).ToDictionary(k => k.Key, value => value.ToList());
            // dic[0].First().DataFim
            //var a = lista.Select(s => new {s.DataInicio}).Distinct().ToList<DateTime>();
            // ViewBag.Dist = lista.Select(s => new { s.DataInicio, s.DataFim }).Distinct();


            if (!String.IsNullOrEmpty(ProcuraData))
            {
                var data = Convert.ToDateTime(ProcuraData);
                lista = lista.Where(s => s.DataInicio <= data && s.DataFim >= data);
            }

            switch (SortOrder)
            {
                case "name_desc":
                    ViewBag.NameSortParm = "name";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderByDescending(s => s.DataInicio);
                    break;
                case "name":
                    ViewBag.NameSortParm = "name_desc";
                    ViewBag.DateSortParm = "date";

                    lista = lista.OrderBy(s => s.DataInicio);
                    break;
                default:  // Name Descending 
                    ViewBag.NameSortParm = "name";
                    ViewBag.DateSortParm = "date";
                    lista = lista.OrderByDescending(s => s.DataInicio);
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
            return View(lista.ToPagedList(PageNumber, PageSize));

        }

        // GET: Blocos/Details/5
        public ActionResult Details(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitragemPeriodo arbitragemPeriodo = db.ArbitragemPeriodo.Find(id);
            if (arbitragemPeriodo == null)
            {
                return HttpNotFound();
            }

            IQueryable<Arbitragem> lista = db.Arbitragem.Where(x => x.ArbitragemPeriodoID == id).OrderBy(x => x.NivelAssociacao);
            if (lista == null)
            {
                return HttpNotFound();
            }
            ViewBag.Arbitragens = lista;

            ViewBag.Associacoes = db.Associacao;                                

            return View(arbitragemPeriodo);
        }

        // GET: Bloco/Create
        public ActionResult Create()
        {
            Localizacao();

            var lista = db.Associacao.Where(x => x.Nivel > 0).OrderBy(x => x.Nivel).ToList<Associacao>();

            ViewBag.Associacoes = lista;
            ViewBag.QuantidadeLinhas = lista.Count;

            var arbitragemPeriodo = db.ArbitragemPeriodo.OrderByDescending(x => x.DataInicio).FirstOrDefault<ArbitragemPeriodo>();

            if (arbitragemPeriodo != null)
            {
                ViewBag.DataInicio = arbitragemPeriodo.DataInicio.AddDays(7).ToString("dd/MM/yyy");
                ViewBag.DataFim = arbitragemPeriodo.DataFim.AddDays(7).ToString("dd/MM/yyy");
            }
            else
            {
                int dia = (int)Core.Helpers.App.DateTimeZion.DayOfWeek;
                ViewBag.DataInicio = Core.Helpers.App.DateTimeZion.AddDays(7-dia).ToString("dd/MM/yyy");
                ViewBag.DataFim = Core.Helpers.App.DateTimeZion.AddDays(13-dia).ToString("dd/MM/yyy");
            }

            return View();
        }

        // POST: Bloco/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,Nivel,Nome,DuracaoDias")] Associacao Associacao)
        //{
        //    Localizacao();

        //    List<string> msg = new List<string>();
        //    msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

        //    if (Associacao.Nivel == 0)
        //    {
        //        msg.Add(traducaoHelper["NIVEL"]);
        //    }
        //    if (string.IsNullOrEmpty(Associacao.Nome))
        //    {
        //        msg.Add(traducaoHelper["NOME"]);
        //    }
        //    if (Associacao.DuracaoDias == 0)
        //    {
        //        msg.Add(traducaoHelper["DURACAO_EM_DIAS"]);
        //    }

        //    if (msg.Count > 1)
        //    {
        //        string[] erro = msg.ToArray();
        //        Mensagem(traducaoHelper["ASSOCIACAO"], erro, "err");
        //    }
        //    else
        //    {
        //        db.Associacao.Add(Associacao);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    obtemMensagem();

        //    return View(Associacao);
        //}

        // GET: Bloco/Edit/5
        public ActionResult Edit(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitragemPeriodo arbitragemPeriodo = db.ArbitragemPeriodo.Find(id);
            if (arbitragemPeriodo == null)
            {
                return HttpNotFound();
            }

            var lista = db.Arbitragem.Where(x => x.ArbitragemPeriodoID == id).OrderBy(x => x.NivelAssociacao).ToList<Arbitragem>();

            ViewBag.Arbitragens = lista;
            ViewBag.QuantidadeLinhas = lista.Count;

            ViewBag.Associacoes = db.Associacao;

            return View(arbitragemPeriodo);
        }

        // POST: Bloco/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ID,Nivel,Nome,DuracaoDias")] Associacao Associacao)
        //{
        //    Localizacao();

        //    List<string> msg = new List<string>();
        //    msg.Add(traducaoHelper["CAMPO_REQUERIDO"]);

        //    if (Associacao.Nivel == 0)
        //    {
        //        msg.Add(traducaoHelper["NIVEL"]);
        //    }
        //    if (string.IsNullOrEmpty(Associacao.Nome))
        //    {
        //        msg.Add(traducaoHelper["NOME"]);
        //    }
        //    if (Associacao.DuracaoDias == 0)
        //    {
        //        msg.Add(traducaoHelper["DURACAO_EM_DIAS"]);
        //    }

        //    if (msg.Count > 1)
        //    {
        //        string[] erro = msg.ToArray();
        //        Mensagem(traducaoHelper["ASSOCIACAO"], erro, "err");
        //    }
        //    else
        //    {
        //        db.Entry(Associacao).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    obtemMensagem();

        //    return View(Associacao);
        //}

        // GET: Bloco/Delete/5
        public ActionResult Delete(int? id)
        {
            Localizacao();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitragemPeriodo arbitragemPeriodo = db.ArbitragemPeriodo.Find(id);
            if (arbitragemPeriodo == null)
            {
                return HttpNotFound();
            }
            return View(arbitragemPeriodo);
        }

        // POST: Bloco/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localizacao();

            ArbitragemPeriodo arbitragemPeriodo = db.ArbitragemPeriodo.Find(id);

            IQueryable<Arbitragem> arbitragens = db.Arbitragem.Where(x => x.ArbitragemPeriodoID == id);
             
            foreach (var item in arbitragens)
            {
                db.Arbitragem.Remove(item);
            }
            db.ArbitragemPeriodo.Remove(arbitragemPeriodo);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Reload()
        {
            Localizacao();

            //Persistencia dos paramentros da tela
            Funcoes objFuncoes = new Funcoes(this.HttpContext);
            objFuncoes.LimparPersistencia();
            objFuncoes = null;
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            Localizacao();

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region JsonResult

        [HttpPost]
        public JsonResult Gavar()
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Request.InputStream);
                string strJSON = @reader.ReadToEnd();
                LAssoc lstArbitragem = JsonConvert.DeserializeObject<LAssoc>(@strJSON);

                if (lstArbitragem != null && lstArbitragem.itens.Length > 0)
                {

                    ArbitragemPeriodo arbitragemPeriodo = new ArbitragemPeriodo();

                    arbitragemPeriodo.DataCriacao = Core.Helpers.App.DateTimeZion;
                    arbitragemPeriodo.DataInicio = DateTime.ParseExact(lstArbitragem.dtInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    arbitragemPeriodo.DataFim = DateTime.ParseExact(lstArbitragem.dtfim, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    db.ArbitragemPeriodo.Add(arbitragemPeriodo);
                    db.SaveChanges();

                    foreach (LArbitragem item in lstArbitragem.itens)
                    {
                        Arbitragem arbitragem = new Arbitragem();

                        arbitragem.ArbitragemPeriodoID = arbitragemPeriodo.ID;
                        arbitragem.NivelAssociacao = int.Parse(item.nivelAssoc);

                        arbitragem.PercentualDomingo = 0;
                        arbitragem.PercentualSegunda = string.IsNullOrEmpty(item.percSeg) ? 0.0 : double.Parse(item.percSeg) / 100;
                        arbitragem.PercentualTerca = string.IsNullOrEmpty(item.percTer) ? 0.0 : double.Parse(item.percTer) / 100;
                        arbitragem.PercentualQuarta = string.IsNullOrEmpty(item.percQua) ? 0.0 : double.Parse(item.percQua) / 100;
                        arbitragem.PercentualQuinta = string.IsNullOrEmpty(item.percQui) ? 0.0 : double.Parse(item.percQui) / 100;
                        arbitragem.PercentualSexta = string.IsNullOrEmpty(item.percSex) ? 0.0 : double.Parse(item.percSex) / 100;
                        arbitragem.PercentualSabado = 0;

                        db.Arbitragem.Add(arbitragem);
                    }
                    db.SaveChanges();
                }

                return Json("OK");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult Editar()
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Request.InputStream);
                string strJSON = @reader.ReadToEnd();
                LAssoc lstArbitragem = JsonConvert.DeserializeObject<LAssoc>(@strJSON);

                if (lstArbitragem != null && lstArbitragem.itens.Length > 0)
                {

                    //ArbitragemPeriodo arbitragemPeriodo = new ArbitragemPeriodo();

                    //arbitragemPeriodo.DataCriacao = Core.Helpers.App.DateTimeZion;
                    //arbitragemPeriodo.DataInicio = DateTime.ParseExact(lstArbitragem.dtInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //arbitragemPeriodo.DataFim = DateTime.ParseExact(lstArbitragem.dtfim, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    //db.ArbitragemPeriodo.Add(arbitragemPeriodo);
                    //db.SaveChanges();

                    foreach (LArbitragem item in lstArbitragem.itens)
                    {
                        Arbitragem arbitragem = new Arbitragem();

                        arbitragem.ID = int.Parse(item.id);
                        arbitragem.ArbitragemPeriodoID = int.Parse(item.arbitragemPeriodo);
                        arbitragem.NivelAssociacao = int.Parse(item.nivelAssoc);

                        arbitragem.PercentualDomingo = 0;
                        arbitragem.PercentualSegunda = string.IsNullOrEmpty(item.percSeg) ? 0.0 : double.Parse(item.percSeg) / 100;
                        arbitragem.PercentualTerca = string.IsNullOrEmpty(item.percTer) ? 0.0 : double.Parse(item.percTer) / 100;
                        arbitragem.PercentualQuarta = string.IsNullOrEmpty(item.percQua) ? 0.0 : double.Parse(item.percQua) / 100;
                        arbitragem.PercentualQuinta = string.IsNullOrEmpty(item.percQui) ? 0.0 : double.Parse(item.percQui) / 100;
                        arbitragem.PercentualSexta = string.IsNullOrEmpty(item.percSex) ? 0.0 : double.Parse(item.percSex) / 100;
                        arbitragem.PercentualSabado = 0;

                        db.Entry(arbitragem).State = EntityState.Modified;
                        db.SaveChanges();
                    }                  
                }

                return Json("OK");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion

    }
}
