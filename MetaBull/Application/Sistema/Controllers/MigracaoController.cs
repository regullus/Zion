using Core.Entities;
using Core.Helpers;
using Core.Repositories.Usuario;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Sistema.Controllers
{
    [Authorize(Roles = "Master, perfilAdministrador, perfilUsuario")]
    public class MigracaoController : SecurityController<Core.Entities.Usuario>
    {

        #region Core

        private UsuarioRepository usuarioRepository;

        #endregion

        public MigracaoController(DbContext context)
         : base(context)
        {
            usuarioRepository = new UsuarioRepository(context);
        }

        // GET: Migracao
        public ActionResult Rede()
        {
            if (usuario.DataMigracao.HasValue)
            {
                return RedirectToAction("index", "home");
            }

            ViewBag.PatrocinadorID = 0;
            ViewBag.PatrocinadorLogin = "";
            ViewBag.PatrocinadorMigrou = false;

            int patrocinadorDiretoID = usuarioRepository.GetPatrocinadorDiretoMigrado(usuario.ID);
            if (patrocinadorDiretoID > 0)
            {
                Usuario patrocinadorDireto = usuarioRepository.Get(patrocinadorDiretoID);
                if (patrocinadorDireto != null)
                {
                    ViewBag.PatrocinadorID = patrocinadorDireto.ID;
                    ViewBag.PatrocinadorLogin = patrocinadorDireto.Login;
                    ViewBag.PatrocinadorMigrou = true;
                }
            }

            return View();
        }

        [HttpPost]
        public JsonResult MigrarComPatrocinador(FormCollection form)
        {
            try
            {    
                var patrocinadorId = 0;

                int.TryParse(form["patrocinadorId"], out patrocinadorId);

                AssociarRedeHierarquiaComDerramamento(patrocinadorId);

                return Json(new { ok = true });

            }
            catch (Exception ex)
            {
                return Json(new { mensagem = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult Migrar()
        {
            try
            {    
                AssociarRedeHierarquiaComDerramamento();

                return Json(new { ok = true });

            }
            catch (Exception ex)
            {
                return Json(new { mensagem = ex.Message });
            }
        }

        private void AssociarRedeHierarquiaComDerramamento(int patrocinadorId = 0)
        {
            if (!patrocinadorId.Equals(0))
            {
                usuario.PatrocinadorDiretoID = patrocinadorId;
                usuarioRepository.Save(usuario);
            }

            var usuarioCorrente = usuarioRepository.Get(usuario.ID);

            Core.Entities.Usuario.Derramamentos derramamento = usuarioCorrente.PatrocinadorDireto.Derramamento;

            int intColuna = 0;
            switch (derramamento)
            {
                case Core.Entities.Usuario.Derramamentos.Indefinido:
                case Core.Entities.Usuario.Derramamentos.Coluna0:
                    intColuna = 0;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna1:
                    intColuna = 1;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna2:
                    intColuna = 2;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna3:
                    intColuna = 3;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna4:
                    intColuna = 4;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna5:
                    intColuna = 5;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna6:
                    intColuna = 6;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna7:
                    intColuna = 7;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna8:
                    intColuna = 8;
                    break;
                case Core.Entities.Usuario.Derramamentos.Coluna9:
                    intColuna = 9;
                    break;
                    //case Entities.Usuario.Derramamentos.Linha:
                    //    intColuna = -1;
                    //    break;
            }

            string newAssinatura = string.Empty;

            // == Derramamento por Coluna
            if (intColuna >= 0)
            {
                var lstAssinatura = usuarioRepository.GetUltimaAssinaturaPerna(usuarioCorrente.PatrocinadorDireto.ID, intColuna, 0);
                if (lstAssinatura.Count > 0)
                {
                    newAssinatura = lstAssinatura[0];
                    if (newAssinatura == usuarioCorrente.PatrocinadorDireto.Assinatura)
                        newAssinatura = lstAssinatura[0] + intColuna.ToString();
                    else
                        newAssinatura = lstAssinatura[0] + "0";
                }
                else
                    newAssinatura = usuarioCorrente.PatrocinadorDireto.Assinatura + intColuna.ToString();

                var i = 0;
                while (true)
                {
                    if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                    {
                        usuarioCorrente.Assinatura = newAssinatura;
                        usuarioRepository.Save(usuarioCorrente);
                        break;
                    }

                    newAssinatura = newAssinatura + "0";

                    i++;
                    if (i > 10000000) break;
                }
            }

            var patrocinadorPosicao = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura.Substring(0, newAssinatura.Length - 1)).FirstOrDefault();
            if (patrocinadorPosicao != null)
            {
                usuarioCorrente.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                usuarioCorrente.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
            }

            usuarioCorrente.DataMigracao = App.DateTimeZion;
            usuarioCorrente.RecebeBonus  = true;

            usuarioRepository.Save(usuarioCorrente);
        }

        public JsonResult GetUsuarios(string search)
        {
            IQueryable<Usuario> usuarios = usuarioRepository.GetByExpression(x => x.Login.Contains(search) && x.DataMigracao.HasValue);
            return Json(usuarios.Select(s => new { id = s.ID, text = s.Login }).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}