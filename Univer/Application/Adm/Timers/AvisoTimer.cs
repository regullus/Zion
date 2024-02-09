using Core.Entities;
using Core.Helpers;
using Core.Services.Sistema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Sistema.Timers
{
    public class AvisoTimer
    {

        private static Timer _timer;

        public static void Start()
        {
            _timer = new Timer(3600000);
            _timer.AutoReset = true;
            _timer.Elapsed += EnviarAvisos;
            _timer.Start();
        }

        private static void EnviarAvisos(object sender, ElapsedEventArgs e)
        {
            //ToDo mudou a tabella de aviso

            //TraducaoHelper traducaoHelper;

            //var context = new YLEVELEntities();
            //var emailService = new EmailService();
            //var idiomas = context.Idiomas.AsEnumerable();

            //var avisos = context.Database.SqlQuery<Aviso>("SELECT UsuarioID, IdiomaID, Login, Nome, Email, TipoID FROM Usuario.Aviso GROUP BY UsuarioID, IdiomaID, Login, Nome, Email, TipoID");

            //if (avisos != null && avisos.Any())
            //{
            //    var de = ConfiguracaoHelper.GetString("EMAIL_DE");

            //    var administrador = new StringBuilder();
            //    administrador.AppendLine("Usuários com Contas Negativas");

            //    var tipos = avisos.GroupBy(a => a.TipoID);
            //    foreach (var tipo in tipos)
            //    {
            //        var chaveAssunto = "";
            //        var chaveCorpo = "";
            //        var enviar = false;

            //        switch (tipo.Key)
            //        {
            //            //Hoje
            //            case 1:
            //                chaveAssunto = "EMAIL_NEGATIVO_1_DIA_ASSUNTO";
            //                chaveCorpo = "EMAIL_NEGATIVO_1_DIA_CORPO";
            //                administrador.AppendLine("\nHOJE");
            //                enviar = true;
            //                break;

            //            //Há 5 Dias
            //            case 2:
            //                chaveAssunto = "EMAIL_NEGATIVO_5_DIAS_ASSUNTO";
            //                chaveCorpo = "EMAIL_NEGATIVO_5_DIAS_CORPO";
            //                administrador.AppendLine("\nHÁ 5 DIAS");
            //                enviar = true;
            //                break;
            //        }

            //        if (enviar)
            //        {
            //            foreach (var usuario in tipo)
            //            {
            //                var idioma = idiomas.FirstOrDefault(i => i.ID == usuario.IdiomaID);
            //                traducaoHelper = new TraducaoHelper(idioma);

            //                var assunto = traducaoHelper[chaveAssunto];
            //                var corpo = traducaoHelper[chaveCorpo];

            //                emailService.Send(de, usuario.Email, assunto, corpo, true);

            //                administrador.AppendFormat("{0} ({1})\n", usuario.Nome, usuario.Login);
            //            }
            //        }
            //    }

            //    emailService.Send(de, "suporte@XXXXXX1234ddddd.com", "Contas Negativas", administrador.ToString(), false);
            //}

            //context.Database.ExecuteSqlCommand("DELETE FROM Usuario.Aviso");
        }

        private struct Aviso
        {
            public int UsuarioID { get; set; }
            public int IdiomaID { get; set; }
            public string Login { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public int TipoID { get; set; }
        }
    }
}
