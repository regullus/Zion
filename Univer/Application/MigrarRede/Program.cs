using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;
using Core.Entities;
using Core.Repositories.Usuario;
using Core.Repositories.Rede;
using System.IO;

namespace MigrarRede
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zion - Migração de Rede");

            var context = new YLEVELEntities();
            var redeMigracaoRepository = new RedeMigracaoRepository(context);
            var usuarioRepository = new UsuarioRepository(context);

            int patrocinadorPaiID = 0;
            Usuario patrocinadorPai;
            var redeMigracao = redeMigracaoRepository.GetListaPatrocinadores();

            foreach (int patrocinadorID in redeMigracao)
            {
                Console.WriteLine("M P:" + patrocinadorID.ToString("00000"));
                Log("#################################################################################");
                Log("Usuário id: " + patrocinadorID.ToString("00000"));

                // Verifica se oPatrocinador já tem posição na rede 
                var patrocinador = usuarioRepository.Get(patrocinadorID);
                Log("Login: " + patrocinador.Login);

                //if (patrocinadorID == 3558)
                //    Console.WriteLine("M P:" + patrocinadorID.ToString("00000") + "A:" + patrocinador.Assinatura);

                if (string.IsNullOrEmpty(patrocinador.Assinatura))
                {
                    Log("Assinatura: Não tem");

                    patrocinadorPaiID = usuarioRepository.GetPatrocinadorDiretoMigrado(patrocinadorID);

                    Log("Patrocinador pai (id): " + patrocinadorPaiID);

                    if (patrocinadorPaiID == 0)
                    {
                        patrocinadorPai = usuarioRepository.GetByAssinatura("#0");
                    }
                    else
                    {
                        patrocinadorPai = usuarioRepository.Get(patrocinadorPaiID);
                    }

                    Log("Patrocinador pai (login): " + patrocinadorPai.Login);

                    AssociarRedeHierarquiaComDerramamento(patrocinador, patrocinadorPai, 0, context);
                }
                else
                {
                    Log("Assinatura: " + patrocinador.Assinatura);
                }

                var rede = redeMigracaoRepository.GetByPatricinador(patrocinadorID).OrderBy(x => x.Ordem);
                foreach (RedeMigracao item in rede)
                {
                    Log("---------------------------------------------------------------------------------");
                    Log("Usuário da rede");
                    Log(string.Format("- id: {0}", item.UsuarioID));

                    Usuario usuario = usuarioRepository.GetByID(item.UsuarioID);
                    if (usuario != null)
                    {
                        Log(string.Format("- login: {0}", usuario.Login));

                        AssociarRedeHierarquiaComDerramamento(usuario, patrocinador, item.Linha, context);
                    }
                    else
                    {
                        Log("- não encontrado");
                    }
                }
            }
        }

        static void AssociarRedeHierarquiaComDerramamento(Usuario usuarioCorrente, Usuario patricinador, int intColuna, YLEVELEntities context)
        {
            Log("Associar");
            Log(string.Format("- usuário     : {0}", usuarioCorrente.ID));
            Log(string.Format("- assinatura U: {0}", usuarioCorrente.Assinatura));
            Log(string.Format("- patrocinador: {0}", patricinador.ID));
            Log(string.Format("- assinatura P: {0}", patricinador.Assinatura));
            Log(string.Format("- coluna: {0}", intColuna));

            var usuarioRepository = new UsuarioRepository(context);

            string newAssinatura = string.Empty;

            if (intColuna >= 0)
            {
                var lstAssinatura = usuarioRepository.GetUltimaAssinaturaPerna(patricinador.ID, intColuna, 0);

                Log(string.Format("Assinaturas encontradas (count) {0}", lstAssinatura.Count()));

                if (lstAssinatura.Count > 0)
                {
                    newAssinatura = lstAssinatura[0];

                    Log(string.Format("Nova Assinatura {0}", newAssinatura));

                    if (newAssinatura == patricinador.Assinatura)
                        newAssinatura = lstAssinatura[0] + intColuna.ToString();
                    else
                        newAssinatura = lstAssinatura[0] + "0";

                    Log(string.Format("Nova Assinatura processada {0}", newAssinatura));

                }
                else
                {
                    newAssinatura = patricinador.Assinatura + intColuna.ToString();

                    Log(string.Format("Assinatura processada {0}", newAssinatura));
                }


                var i = 0;
                while (true)
                {
                    Log(string.Format("Testar assinatura (i={0}) {1}", i, newAssinatura));

                    var usuarioValidaAssinatura = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura);

                    foreach (var item in usuarioValidaAssinatura)
                    {
                        Log(string.Format("- encontrada {0}", item.Assinatura));
                    }

                    if (usuarioValidaAssinatura.Count() == 0)
                    {
                        Log(string.Format("- válida {0}", newAssinatura));

                        usuarioCorrente.Assinatura = newAssinatura;
                        usuarioRepository.Save(usuarioCorrente);

                        Log("- salvou assinatura");

                        break;
                    }

                    newAssinatura = newAssinatura + "0";
                    i++;
                    if (i > 10000000) break;
                }
            }

            Log(string.Format("Buscar patrocinador posição assinatura: {0}", newAssinatura));
            Log(string.Format("- substring {0}", newAssinatura.Substring(0, newAssinatura.Length - 1)));

            var patrocinadorPosicao = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura.Substring(0, newAssinatura.Length - 1)).FirstOrDefault();
            if (patrocinadorPosicao != null)
            {
                Log(string.Format("- encontrou {0}", patrocinadorPosicao.ID));

                usuarioCorrente.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                usuarioCorrente.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
            }
            else
            {
                Log("- não encontrou patrocinador");
            }

            usuarioCorrente.PatrocinadorDiretoID = patricinador.ID;
            usuarioCorrente.DataMigracao = Core.Helpers.App.DateTimeZion;

            usuarioCorrente.DataAtivacao = usuarioCorrente.DataMigracao;

            usuarioRepository.Save(usuarioCorrente);

            Log("- salvou paterocinadores");

            Console.WriteLine("  U:" + usuarioCorrente.ID.ToString("00000") + " L: " + intColuna + " A: " + usuarioCorrente.Assinatura);
        }


        static void Log(string texto)
        {
            string path = @"d:\logs\"+ DateTime.Now.ToString("yyyyMMdd") + "_LogProgram.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(texto);
            }
        }

    }
}
