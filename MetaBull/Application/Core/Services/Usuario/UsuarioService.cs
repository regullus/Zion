using Core.Helpers;
using Core.Repositories.Usuario;
using Core.Repositories.Rede;
using Core.Repositories.Globalizacao;
using Core.Services.Sistema;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Threading;
using Core.Entities;
using Core.Repositories.Loja;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Canducci.Zip;

namespace Core.Services.Usuario
{
    public class UsuarioService
    {
        #region Core

        private UsuarioRepository usuarioRepository;
        private PosicaoRepository posicaoRedeRepository;
        private AssociacaoRepository associacaoRepository;
        private UsuarioAssociacaoRepository usuarioAssociacaoRepository;
        private UsuarioStatusRepository usuarioStatusRepository;
        private ProdutoRepository produtoRepository;
        private PaisRepository paisRepository;
        private CidadeRepository cidadeRepository;
        private EnderecoRepository enderecoRepository;
        private ClassificacaoRepository classificacaoRepository;
        private PosicaoRepository posicaoRepository;
        private AvisoRepository avisoRepository;

        //private UsuarioExternoRepository usuarioExternoRepository;
        //private UsuarioDerramamentoRepository usuarioDerramamentoRepository;

        public UsuarioService(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            associacaoRepository = new AssociacaoRepository(context);
            usuarioAssociacaoRepository = new UsuarioAssociacaoRepository(context);
            usuarioStatusRepository = new UsuarioStatusRepository(context);
            posicaoRedeRepository = new PosicaoRepository(context);
            produtoRepository = new ProdutoRepository(context);
            paisRepository = new PaisRepository(context);
            cidadeRepository = new CidadeRepository(context);
            enderecoRepository = new EnderecoRepository(context);
            classificacaoRepository = new ClassificacaoRepository(context);
            posicaoRepository = new PosicaoRepository(context);
            avisoRepository = new AvisoRepository(context);
            //usuarioExternoRepository = new UsuarioExternoRepository(context);
            //usuarioDerramamentoRepository = new UsuarioDerramamentoRepository(context);
        }

        #endregion

        #region Services

        public Entities.Usuario Autenticar(string login, string senha)
        {
            var usuario = usuarioRepository.GetByLogin(login);
            if (usuario != null)
            {
                string strSenhaBanco = Helpers.CriptografiaHelper.Descriptografar(usuario.Senha);
                if (senha == strSenhaBanco)
                {
                    if (!usuario.Bloqueado)
                    {
                        usuario.UltimoLogin = App.DateTimeZion;
                        usuarioRepository.Save(usuario);
                    }

                    return usuario;
                }
                else if (Helpers.CriptografiaHelper.CriptografarMD5(senha) == usuario.SenhaLegado)
                {
                    if (!usuario.Bloqueado)
                    {
                        //acertou o hash md5.atualiza a senha.
                        usuario.UltimoLogin = App.DateTimeZion;
                        usuario.Senha = Helpers.CriptografiaHelper.Criptografar(senha);
                        usuarioRepository.Save(usuario);
                    }
                    return usuario;
                }
            }
            return null;
        }

        public void Associar(int usuarioID, int nivel, int? administradorID = null)
        {
            var usuario = usuarioRepository.Get(usuarioID);
            if (usuario != null)
            {
                if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado || usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
                {
                    var atualizaDadosRede = false;

                    /*TODO: configuracao para kit de associacao ue nao gere posicao de rede*/
                    var nivelRedeBinaria = 1;
                    if (ConfiguracaoHelper.TemChave("NIVEL_REDE_BINARIA"))
                    {
                        nivelRedeBinaria = ConfiguracaoHelper.GetInt("NIVEL_REDE_BINARIA");
                    }
                    if (nivel < nivelRedeBinaria)
                    {
                        usuario.Status = Entities.Usuario.TodosStatus.Associado;
                        if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
                            usuario.DataAtivacao = App.DateTimeZion;
                        usuario.NivelAssociacao = nivel;
                        usuario.Status = Entities.Usuario.TodosStatus.Associado;

                        usuarioRepository.Save(usuario);

                        var statusUser = new Entities.UsuarioStatus()
                        {
                            AdministradorID = administradorID,
                            Data = App.DateTimeZion,
                            Status = Entities.Usuario.TodosStatus.Associado,
                            UsuarioID = usuario.ID
                        };
                        usuarioStatusRepository.Save(statusUser);
                        //return;
                    }

                    if (ConfiguracaoHelper.TemChave("REDE_DIRETOS") && ConfiguracaoHelper.GetBoolean("REDE_DIRETOS"))
                    {
                        //apenas rede de diretos
                        atualizaDadosRede = true;
                    }
                    else
                    {
                        bool direita = true;
                        Entities.Usuario patrocinadorDireto = usuario.PatrocinadorDireto;
                        Entities.Usuario patrocinadorPosicao = null;
                        Entities.Usuario.Derramamentos derramamento = usuario.Entrada.HasValue ? usuario.Entrada.Value : patrocinadorDireto.Derramamento;
                        switch (derramamento)
                        {
                            case Entities.Usuario.Derramamentos.Direita:
                            case Entities.Usuario.Derramamentos.Indefinido:
                                direita = true;
                                patrocinadorPosicao = patrocinadorDireto.UltimoDireita != null ? patrocinadorDireto.UltimoDireita : patrocinadorDireto;
                                break;
                            case Entities.Usuario.Derramamentos.Esquerda:
                                direita = false;
                                patrocinadorPosicao = patrocinadorDireto.UltimoEsquerda != null ? patrocinadorDireto.UltimoEsquerda : patrocinadorDireto;
                                break;
                            case Entities.Usuario.Derramamentos.Automatico:
                                if (patrocinadorDireto.UltimoDireita == null || patrocinadorDireto.UltimoEsquerda == null)
                                {
                                    patrocinadorPosicao = patrocinadorDireto;
                                    if (patrocinadorDireto.UltimoEsquerda == null)
                                    {
                                        direita = false;
                                    }
                                }
                                else
                                {
                                    if (patrocinadorDireto.UltimoEsquerda.ProfundidadeRede < patrocinadorDireto.UltimoDireita.ProfundidadeRede)
                                    {
                                        patrocinadorPosicao = patrocinadorDireto.UltimoEsquerda;
                                        direita = false;
                                    }
                                    else
                                    {
                                        patrocinadorPosicao = patrocinadorDireto.UltimoDireita;
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        if (direita)
                        {
                            usuario.Assinatura = String.Format("{0}1", patrocinadorPosicao.Assinatura);
                            /*se assinatura gerada for repetida, alterna patrocinador de posicao até achar posição vaga*/
                            if (usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura).Count() > 0)
                            {
                                //assinatura gerada já existe
                                var assinaturaGerada = usuario.Assinatura;
                                var i = 0;
                                while (true)
                                {
                                    var usuarioPosicaoAnterior = usuarioRepository.GetByExpression(u => u.Assinatura == assinaturaGerada).FirstOrDefault();
                                    assinaturaGerada = assinaturaGerada + "1";

                                    if (usuarioRepository.GetByExpression(u => u.Assinatura == assinaturaGerada).Count() == 0)
                                    {
                                        /*posição livre*/
                                        patrocinadorPosicao = usuarioPosicaoAnterior;
                                        usuario.Assinatura = assinaturaGerada;
                                        break;
                                    }
                                    i++;
                                    if (i > 10000000) break;
                                }
                            }



                            /*entre o patrocinador e a posição do usuario*/
                            if (patrocinadorDireto.UltimoDireitaID.HasValue)
                            {
                                var diretos = usuarioRepository.GetByExpression(u => u.UltimoDireitaID == patrocinadorDireto.UltimoDireitaID).ToList();
                                foreach (var direto in diretos)
                                {
                                    direto.UltimoDireitaID = usuario.ID;
                                    usuarioRepository.Save(direto);
                                }
                            }
                            else
                            {
                                /*patrocinador é o ultimo nó. precisa atualizar todos acima*/
                                var diretos = usuarioRepository.GetByExpression(u => u.UltimoDireitaID == patrocinadorDireto.ID).ToList();
                                foreach (var direto in diretos)
                                {
                                    direto.UltimoDireitaID = usuario.ID;
                                    usuarioRepository.Save(direto);
                                }
                            }
                            /*o proprio patrocinador*/
                            patrocinadorDireto.UltimoDireitaID = usuario.ID;
                            usuarioRepository.Save(patrocinadorDireto);

                            patrocinadorPosicao.UltimoDireitaID = usuario.ID;
                            usuarioRepository.Save(patrocinadorPosicao);

                            atualizaDadosRede = true;
                        }
                        else
                        {
                            usuario.Assinatura = String.Format("{0}0", patrocinadorPosicao.Assinatura);
                            /*se assinatura gerada for repetida, alterna patrocinador de posicao até achar posição vaga*/
                            if (usuarioRepository.GetByExpression(u => u.Assinatura == usuario.Assinatura).Count() > 0)
                            {
                                //assinatura gerada já existe
                                var assinaturaGerada = usuario.Assinatura;
                                var i = 0;
                                while (true)
                                {
                                    var usuarioPosicaoAnterior = usuarioRepository.GetByExpression(u => u.Assinatura == assinaturaGerada).FirstOrDefault();
                                    assinaturaGerada = assinaturaGerada + "0";

                                    if (usuarioRepository.GetByExpression(u => u.Assinatura == assinaturaGerada).Count() == 0)
                                    {
                                        /*posição livre*/
                                        patrocinadorPosicao = usuarioPosicaoAnterior;
                                        usuario.Assinatura = assinaturaGerada;
                                        break;
                                    }
                                    i++;
                                    if (i > 10000000) break;
                                }
                            }


                            if (patrocinadorDireto.UltimoEsquerdaID.HasValue)
                            {
                                var diretos = usuarioRepository.GetByExpression(u => u.UltimoEsquerdaID == patrocinadorDireto.UltimoEsquerdaID).ToList();
                                foreach (var direto in diretos)
                                {
                                    direto.UltimoEsquerdaID = usuario.ID;
                                    usuarioRepository.Save(direto);
                                }
                            }
                            else
                            {
                                /*patrocinador é o ultimo nó. precisa atualizar todos acima*/
                                var diretos = usuarioRepository.GetByExpression(u => u.UltimoEsquerdaID == patrocinadorDireto.ID).ToList();
                                foreach (var direto in diretos)
                                {
                                    direto.UltimoEsquerdaID = usuario.ID;
                                    usuarioRepository.Save(direto);
                                }
                            }
                            /*o proprio patrocinador*/
                            patrocinadorDireto.UltimoEsquerdaID = usuario.ID;
                            usuarioRepository.Save(patrocinadorDireto);

                            patrocinadorPosicao.UltimoEsquerdaID = usuario.ID;
                            usuarioRepository.Save(patrocinadorPosicao);

                            atualizaDadosRede = true;
                        }

                        usuario.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                        usuario.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;

                    }

                    if (atualizaDadosRede)
                    {

                        usuario.NivelAssociacao = nivel;
                        usuario.Status = Entities.Usuario.TodosStatus.Associado;
                        usuario.DataAtivacao = App.DateTimeZion;

                        usuarioRepository.Save(usuario);

                        var status = new Entities.UsuarioStatus()
                        {
                            AdministradorID = administradorID,
                            Data = App.DateTimeZion,
                            Status = Entities.Usuario.TodosStatus.Associado,
                            UsuarioID = usuario.ID
                        };
                        usuarioStatusRepository.Save(status);

                        var existePosicao = posicaoRedeRepository.GetByExpression(p => p.UsuarioID == usuario.ID).Any();
                        if (!existePosicao)
                        {
                            Posicao posicaoRede = new Posicao();
                            posicaoRede.AcumuladoDireita = 0;
                            posicaoRede.AcumuladoEsquerda = 0;
                            posicaoRede.DataInicio = App.DateTimeZion.AddDays(-1);
                            posicaoRede.DataFim = App.DateTimeZion.AddDays(-1);
                            posicaoRede.DataCriacao = App.DateTimeZion;
                            posicaoRede.Usuario = usuario;
                            posicaoRede.UsuarioID = usuario.ID;
                            posicaoRede.ValorPernaDireita = 0;
                            posicaoRede.ValorPernaEsquerda = 0;
                            posicaoRede.ReferenciaID = 0;
                            posicaoRedeRepository.Save(posicaoRede);
                        }

                        //AssociarSiteParceiro(usuario);
                    }
                }
                else
                {
                    usuario.NivelAssociacao = nivel;
                    usuarioRepository.Save(usuario);
                }

            }
        }

        public void AssociarRedeSequencia(int pUsuarioID, int pNivel, int? pAdministradorID = null)
        {
            var usuario = usuarioRepository.Get(pUsuarioID);
            if (usuario == null)
                return;

            if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado ||
                usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
            {
                int intQtdeRamos = 3;
                if (ConfiguracaoHelper.TemChave("REDE_QTDE_RAMOS"))
                {
                    intQtdeRamos = ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS") - 1;
                }

                var lstAssinatura = usuarioRepository.GetUltimaAssinatura();
                string newAssinatura = string.Empty;
                if (lstAssinatura.Count > 0)
                    newAssinatura = lstAssinatura[0];

                var i = 0;
                while (true)
                {
                    if (newAssinatura.Length == 2)
                        newAssinatura = newAssinatura + "0";
                    else
                    {
                        int intRamo = Convert.ToInt16(newAssinatura.Substring(newAssinatura.Length - 1, 1));
                        if (intRamo == intQtdeRamos)
                            newAssinatura = newAssinatura.Substring(0, newAssinatura.Length - 1) + "00";
                        else
                            newAssinatura = newAssinatura.Substring(0, newAssinatura.Length - 1) + (intRamo + 1).ToString();
                    }

                    if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                    {
                        usuario.Assinatura = newAssinatura;
                        break;
                    }

                    i++;
                    if (i > 10000000) break;
                }

                usuario.Assinatura = newAssinatura;
                usuarioRepository.Save(usuario);

                var patrocinadorPosicao = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura.Substring(0, newAssinatura.Length - 1)).FirstOrDefault();
                if (patrocinadorPosicao != null)
                {
                    usuario.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                    usuario.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
                }

                usuario.NivelAssociacao = pNivel;
                usuario.Status = Entities.Usuario.TodosStatus.Associado;
                if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
                    usuario.DataAtivacao = App.DateTimeZion;

                usuarioRepository.Save(usuario);

                var status = new Entities.UsuarioStatus()
                {
                    AdministradorID = pAdministradorID,
                    Data = App.DateTimeZion,
                    Status = Entities.Usuario.TodosStatus.Associado,
                    UsuarioID = usuario.ID
                };
                usuarioStatusRepository.Save(status);
            }
            else
            {
                usuario.NivelAssociacao = pNivel;
                usuarioRepository.Save(usuario);
            }
        }

        public void AssociarUsuario(int pUsuarioID, int pNivel, int? pAdministradorID = null, bool isUpgrade = false)
        {
            var usuario = usuarioRepository.Get(pUsuarioID);

            if (usuario == null)
                return;

            InvestigacaoLog.LogNivelAssociacao(pUsuarioID, 13, "Novo nivel associacao: " + pNivel + ", nivel associacao atual: " + usuario.NivelAssociacao + ", Status atual: " + usuario.Status);

            var nivelAtual = usuario.NivelAssociacao;

            if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado ||
                usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
            {
                usuario.NivelAssociacao = pNivel;
                usuario.Status = Entities.Usuario.TodosStatus.Associado;
                usuario.DataAtivacao = App.DateTimeZion;

                usuarioRepository.Save(usuario);

                var status = new Entities.UsuarioStatus()
                {
                    AdministradorID = pAdministradorID,
                    Data = App.DateTimeZion,
                    Status = Entities.Usuario.TodosStatus.Associado,
                    UsuarioID = usuario.ID
                };
                usuarioStatusRepository.Save(status);
            }
            else
            {
                usuario.NivelAssociacao = pNivel;
                usuarioRepository.Save(usuario);
            }

            InvestigacaoLog.LogNivelAssociacao(pUsuarioID, 14);
        }

        public void GeraUsuarioAssociacao(Entities.Usuario usuario, int pNivelAssociacao, bool isUpgrade)
        {
            var dataValidadeBase = DateTime.MinValue;

            if (usuario.Validade > App.DateTimeZion)
            {
                dataValidadeBase = usuario.Validade;
            }
            else
            {
                dataValidadeBase = App.DateTimeZion;
            }

            //Sp esta tratando renovacao
            //if (ConfiguracaoHelper.GetBoolean("REDE_RENOVACAO_ASSOCIACAO_POSSUI"))
            //    usuario.DataRenovacao = dataValidadeBase.AddDays(associacaoRepository.GetByNivel(pNivelAssociacao).DuracaoDias);
            //else
            //    usuario.DataRenovacao = dataValidadeBase.AddDays(36500);

            bool existeAssociacao = usuarioAssociacaoRepository.GetByExpression(u => u.UsuarioID == usuario.ID && u.NivelAssociacao == pNivelAssociacao).Count() > 0;

            if (!existeAssociacao)
            {
                var usuarioAssociacao = new Entities.UsuarioAssociacao()
                {
                    Data = App.DateTimeZion,
                    NivelAssociacao = pNivelAssociacao,
                    UsuarioID = usuario.ID,
                    DataValidade = usuario.DataRenovacao,
                    Upgrade = isUpgrade
                };
                usuarioAssociacaoRepository.Save(usuarioAssociacao);
            }
        }

        public void AssociarRedeHierarquiaNew(int pUsuarioID, int pNivel, int? pAdministradorID = null)
        {
            var usuario = usuarioRepository.Get(pUsuarioID);
            if (usuario == null)
                return;

            if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado ||
                usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
            {
                int intQtdeRamos = 3;
                if (ConfiguracaoHelper.TemChave("REDE_QTDE_RAMOS"))
                {
                    intQtdeRamos = ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS") - 1;
                }

                var lstAssinatura = usuarioRepository.GetUltimaAssinatura(usuario.PatrocinadorDireto);
                string newAssinatura = string.Empty;
                if (lstAssinatura.Count > 0)
                    newAssinatura = lstAssinatura[0];

                var i = 0;
                while (true)
                {
                    if (usuario.PatrocinadorDireto.Assinatura == newAssinatura)
                        newAssinatura = newAssinatura + "0";
                    else
                    {
                        int intRamo = Convert.ToInt16(newAssinatura.Substring(newAssinatura.Length - 1, 1));
                        if (intRamo == intQtdeRamos)
                            newAssinatura = newAssinatura.Substring(0, newAssinatura.Length - 1) + "00";
                        else
                            newAssinatura = newAssinatura.Substring(0, newAssinatura.Length - 1) + (intRamo + 1).ToString();
                    }

                    if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                    {
                        usuario.Assinatura = newAssinatura;
                        break;
                    }

                    i++;
                    if (i > 10000000) break;
                }

                usuarioRepository.Save(usuario);

                var patrocinadorPosicao = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura.Substring(0, newAssinatura.Length - 1)).FirstOrDefault();
                if (patrocinadorPosicao != null)
                {
                    usuario.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                    usuario.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
                }

                usuario.NivelAssociacao = pNivel;
                usuario.Status = Entities.Usuario.TodosStatus.Associado;
                if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
                    usuario.DataAtivacao = App.DateTimeZion;

                usuarioRepository.Save(usuario);

                var status = new Entities.UsuarioStatus()
                {
                    AdministradorID = pAdministradorID,
                    Data = App.DateTimeZion,
                    Status = Entities.Usuario.TodosStatus.Associado,
                    UsuarioID = usuario.ID
                };
                usuarioStatusRepository.Save(status);
            }
            else
            {
                usuario.NivelAssociacao = pNivel;
                usuarioRepository.Save(usuario);
            }

        }

        public void AssociarRedeHierarquia(int pUsuarioID, int pNivel, int? pAdministradorID = null)
        {
            var usuario = usuarioRepository.Get(pUsuarioID);
            if (usuario == null)
                return;

            if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado ||
                usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
            {
                int intQtdeRamos = 3;
                if (ConfiguracaoHelper.TemChave("REDE_QTDE_RAMOS"))
                {
                    intQtdeRamos = ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS") - 1;
                }

                var lstAssinatura = usuarioRepository.GetListaAssinatura(usuario.PatrocinadorDireto);
                string newAssinatura = string.Empty;
                if (lstAssinatura.Count > 0)
                    newAssinatura = lstAssinatura[0];

                if (newAssinatura.Length == 2 || usuario.PatrocinadorDireto.Assinatura == newAssinatura)
                    newAssinatura = newAssinatura + "0";

                var i = 0;
                var iPernaAnterior = 0;
                var assinaturaAnterior = usuario.PatrocinadorDireto.Assinatura.Trim();
                var tamanhoInicial = usuario.PatrocinadorDireto.Assinatura.Trim().Length;
                while (true)
                {
                    if (!lstAssinatura.Contains(newAssinatura))
                    {
                        if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                        {
                            usuario.Assinatura = newAssinatura;
                            break;
                        }
                    }

                    int intPerna = Convert.ToInt16(newAssinatura.Substring(newAssinatura.Length - 1, 1));
                    if (intPerna == intQtdeRamos)
                    {
                        if (iPernaAnterior < intQtdeRamos)
                        {
                            iPernaAnterior++;
                            newAssinatura = assinaturaAnterior + iPernaAnterior.ToString() + "0";
                        }
                        else
                        {
                            iPernaAnterior = 0;
                            assinaturaAnterior = assinaturaAnterior + iPernaAnterior.ToString();
                            newAssinatura = assinaturaAnterior + "0";
                        }
                    }
                    else
                        newAssinatura = newAssinatura.Substring(0, newAssinatura.Length - 1) + (intPerna + 1).ToString();

                    i++;
                    if (i > 10000000) break;
                }

                usuarioRepository.Save(usuario);

                var patrocinadorPosicao = usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura.Substring(0, newAssinatura.Length - 1)).FirstOrDefault();
                if (patrocinadorPosicao != null)
                {
                    usuario.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                    usuario.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
                }

                usuario.NivelAssociacao = pNivel;
                usuario.Status = Entities.Usuario.TodosStatus.Associado;
                if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
                    usuario.DataAtivacao = App.DateTimeZion;

                usuarioRepository.Save(usuario);

                var status = new Entities.UsuarioStatus()
                {
                    AdministradorID = pAdministradorID,
                    Data = App.DateTimeZion,
                    Status = Entities.Usuario.TodosStatus.Associado,
                    UsuarioID = usuario.ID
                };
                usuarioStatusRepository.Save(status);
            }
            else
            {
                usuario.NivelAssociacao = pNivel;
                usuarioRepository.Save(usuario);
            }

        }

        public void AssociarRedeHierarquiaComDerramamento(int pUsuarioID, int pNivel, int? pAdministradorID = null)
        {
            var usuario = usuarioRepository.Get(pUsuarioID);
            if (usuario == null)
                return;

            if (usuario.Status == Entities.Usuario.TodosStatus.NaoAssociado ||
                usuario.Status == Entities.Usuario.TodosStatus.Indefinido)
            {
                int intQtdeRamos = 3;
                if (ConfiguracaoHelper.TemChave("REDE_QTDE_RAMOS"))
                {
                    intQtdeRamos = ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS") - 1;
                }

                Entities.Usuario.Derramamentos derramamento = usuario.PatrocinadorDireto.Derramamento;

                int intColuna = 0;
                switch (derramamento)
                {
                    case Entities.Usuario.Derramamentos.Indefinido:
                    case Entities.Usuario.Derramamentos.Coluna0:
                        intColuna = 0;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna1:
                        intColuna = 1;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna2:
                        intColuna = 2;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna3:
                        intColuna = 3;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna4:
                        intColuna = 4;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna5:
                        intColuna = 5;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna6:
                        intColuna = 6;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna7:
                        intColuna = 7;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna8:
                        intColuna = 8;
                        break;
                    case Entities.Usuario.Derramamentos.Coluna9:
                        intColuna = 9;
                        break;
                        //case Entities.Usuario.Derramamentos.Linha:
                        //    intColuna = -1;
                        //    break;
                }

                string newAssinatura = string.Empty;

                // == Derramamento por Linha
                //if (intColuna < 0)
                //{
                //    intColuna = 0;
                //    int totalColunasOcupadas = usuarioRepository.GetQuantidadeColunasOcupadas(usuario.PatrocinadorDireto);

                //    newAssinatura = usuario.PatrocinadorDireto.Assinatura;
                //    for (int i = totalColunasOcupadas; i <= intQtdeRamos; i++)
                //    {
                //        newAssinatura = newAssinatura + i;
                //        if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                //        {
                //            usuario.Assinatura = newAssinatura;
                //            usuarioRepository.Save(usuario);
                //            intColuna = -1;
                //            break;
                //        }
                //    }
                //}

                // == Derramamento por Coluna
                if (intColuna >= 0)
                {
                    var lstAssinatura = usuarioRepository.GetUltimaAssinaturaPerna(usuario.PatrocinadorDireto.ID, intColuna, 0);
                    if (lstAssinatura.Count > 0)
                    {
                        newAssinatura = lstAssinatura[0];
                        if (newAssinatura == usuario.PatrocinadorDireto.Assinatura)
                            newAssinatura = lstAssinatura[0] + intColuna.ToString();
                        else
                            newAssinatura = lstAssinatura[0] + "0";
                    }
                    else
                        newAssinatura = usuario.PatrocinadorDireto.Assinatura + intColuna.ToString();

                    var i = 0;
                    while (true)
                    {
                        if (usuarioRepository.GetByExpression(u => u.Assinatura == newAssinatura).Count() == 0)
                        {
                            usuario.Assinatura = newAssinatura;
                            usuarioRepository.Save(usuario);
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
                    usuario.PatrocinadorPosicaoID = patrocinadorPosicao.ID;
                    usuario.ProfundidadeRede = patrocinadorPosicao.ProfundidadeRede + 1;
                }

                usuario.NivelAssociacao = pNivel;
                usuario.Status = Entities.Usuario.TodosStatus.Associado;
                if (!ConfiguracaoHelper.GetBoolean("REDE_POSICIONA_SEM_PAGAMENTO"))
                    usuario.DataAtivacao = App.DateTimeZion;

                if (ConfiguracaoHelper.GetBoolean("EXIGI_MIGRACAO"))
                    usuario.DataMigracao = App.DateTimeZion;

                usuarioRepository.Save(usuario);

                var status = new Entities.UsuarioStatus()
                {
                    AdministradorID = pAdministradorID,
                    Data = App.DateTimeZion,
                    Status = Entities.Usuario.TodosStatus.Associado,
                    UsuarioID = usuario.ID
                };
                usuarioStatusRepository.Save(status);
            }
            else
            {
                usuario.NivelAssociacao = pNivel;
                usuarioRepository.Save(usuario);
            }

        }

        //public void AtualizarDerramamento(int DerramamentoID, ref Entities.Usuario Usuario)
        //{
        //    Usuario.DerramamentoID = DerramamentoID;
        //    usuarioRepository.Save(Usuario);

        //    usuarioDerramamentoRepository.Save(new UsuarioDerramamento
        //    {
        //        UsuarioID = Usuario.ID,
        //        DerramamentoID = Usuario.DerramamentoID,
        //        Data = App.DateTimeZion
        //    });
        //}

        public bool Upgrade(int usuarioID, int nivel)
        {
            var usuario = usuarioRepository.Get(usuarioID);
            if (usuario != null)
            {
                if (usuario.Status == Entities.Usuario.TodosStatus.Associado)
                {
                    //if (UpgradeAutomatico(usuario, nivel))
                    //    usuario.NivelAssociacao = nivel;

                    usuario.NivelAssociacao = nivel;
                    usuario.RecebeBonus = true;
                    usuario.GeraBonus = true;
                    usuarioRepository.Save(usuario);

                    return true;
                }
            }
            return false;
        }

        public bool UpgradeAutomatico(Entities.Usuario usuario, int nivel)
        {
            if (usuario.NivelAssociacao < nivel)
            {
                var proximoProdutoUpgrade = produtoRepository.GetUpgrade(usuario.NivelAssociacao).OrderBy(x => x.NivelAssociacao).First();

                if (proximoProdutoUpgrade.NivelAssociacao > 8)
                {
                    var somaPagamentos = usuario.Pedido.SelectMany(x => x.PedidoPagamento).Where
                        (x => x.PedidoPagamentoStatus.Any
                        (p => p.Status == PedidoPagamentoStatus.TodosStatus.Pago)).Sum
                        (s => s.Valor) * ConfiguracaoHelper.GetInt("FATOR_MULTIPLICADOR_TETO");

                    if (somaPagamentos >= proximoProdutoUpgrade.ProdutoValor.First().Valor)
                    {
                        //Gera registro de Associacao
                        GeraUsuarioAssociacao(usuario, proximoProdutoUpgrade.NivelAssociacao, true);

                        #region Envia Aviso para Usuario informando o UpgradeAutomatico
                        //Cria um aviso informando o usuário do upgrade
                        Aviso aviso = new Aviso()
                        {
                            UsuarioIDs = usuario.ID.ToString(),
                            Atualizacao = App.DateTimeZion,
                            TipoID = 1,
                            Urgente = true,
                            Bloqueado = false,
                            IdiomaID = usuario.Pais.IdiomaID,
                        };

                        switch (usuario.Pais.IdiomaID)
                        {
                            case 1:
                                aviso.Titulo = "Upgrade de Plano Automático";
                                aviso.Texto = "Sua compra de plano mais recente, possibilitou um upgrade automático para o plano " + proximoProdutoUpgrade.Nome.ToUpper();
                                break;
                            case 2:
                                aviso.Titulo = "Actualización Automática del Plan";
                                aviso.Texto = "Su compra más reciente del plan permitió una actualización automática del plan." + proximoProdutoUpgrade.Nome.ToUpper();
                                break;
                            case 3:
                                aviso.Titulo = "Automatic Plan Upgrade";
                                aviso.Texto = "Your most recent plan purchase enabled an automatic upgrade to the plan" + proximoProdutoUpgrade.Nome.ToUpper();
                                break;
                            case 4:
                                aviso.Titulo = "Автоматическое обновление плана";
                                aviso.Texto = "Ваша самая последняя покупка плана включила автоматическое обновление плана" + proximoProdutoUpgrade.Nome.ToUpper();
                                break;
                            default:
                                break;
                        }

                        avisoRepository.Save(aviso);
                        #endregion

                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public void EnviarValidacaoEmail(Entities.Usuario usuario, string sistema)
        {
            int cont = 0;
            TraducaoHelper traducaoHelper;
            string tipoEnvio = ConfiguracaoHelper.GetString("EMAIL_TIPO_ENVIO");
            try
            {

                if (usuario.Pais != null)
                {
                    traducaoHelper = new TraducaoHelper(usuario.Pais.Idioma);
                }
                else
                {
                    Pais pais = paisRepository.Get(usuario.PaisID);
                    traducaoHelper = new TraducaoHelper(pais.Idioma);
                }

                cont++;
                var emailService = new EmailService();

                var de = Helpers.ConfiguracaoHelper.GetString("EMAIL_DE");
                cont++;
                var para = usuario.Email;
                cont++;
                var assunto = traducaoHelper["EMAIL_VALIDACAO_ASSUNTO"];
                cont++;
                var corpo = "<!DOCTYPE html><html xmlns='http://www.w3.org/1999/xhtml' lang='pt-br'><head><meta http-equiv='content-type' content='text/html; charset=utf-8' /><meta name='viewport' content='width=device-width, initial-scale=1.0;' /><style>@import url('https://fonts.googleapis.com/css2?family=Roboto:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap');:root {--bs-font-sans-serif: Roboto;} body {margin: 0;padding: 0;min-width: 100%;width: 100% !important;height: 100% !important;font-family: 'Roboto';} body, table, td, div, p, a {-webkit-font-smoothing: antialiased;text-size-adjust: 100%;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;line-height: 100%;} table, td {border-collapse: collapse !important;border-spacing: 0;} img {border: 0;line-height: 100%;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;} #outlook a {padding: 0;} .ReadMsgBody {width: 100%;} .ExternalClass {width: 100%;} .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {line-height: 100%;} @media all and (min-width: 540px) {.container {border-radius: 8px;-webkit-border-radius: 8px;-moz-border-radius: 8px;-khtml-border-radius: 8px;} } a, a:hover {color: #127DB3;} .footer a, .footer a:hover {color: #999999;} </style><title>[SISTEMA]</title><link rel='stylesheet' type='text/css' href='https://fonts.googleapis.com/css?family=Roboto' /></head><body style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; width: 100%; height: 100%; -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; background-color: #F0F0F0; color: #000000;'><table width='100%' align='center' border='0' cellpadding='0' cellspacing='0' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; width: 100%;' class='background'><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;' bgcolor='#F0F0F0'><table border='0' cellpadding='0' cellspacing='0' align='center' width='540px' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 540px; background-color: #FFFFFF' class='wrapper'><tr><td align='center' valign='middle' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 540px; height: 0px; background-color: #333333;'></td></tr><tr><td align='center' valign='middle' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding-bottom: 10px; padding-left: 6.25%; padding-right: 6.25%; width: 540px; height: 60px; background-color: #333333; border-bottom-left-radius: 30px; -webkit-border-bottom-left-radius: 30px; -moz-border-bottom-left-radius: 30px; -khtml-border-bottom-left-radius: 30px; border-bottom-right-radius: 30px; -webkit-border-bottom-right-radius: 30px; -moz-border-bottom-right-radius: 30px; -khtml-border-bottom-right-radius: 30px;'><a target='_blank' style='width: 120px; height: 52px;' href='#'><img border='0' vspace='0' hspace='0' src='[IMG_LOGO]' width='120' height='52' alt='Logo' title='Logo' style='font-size: 10px; margin: 0; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; border: none; display: block;' /></a></td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding-top: 40px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 18px; font-weight: 300; line-height: 150%; color: #000000; font-family: Roboto;' class='subheader'>[MSG_HEADER]</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding-top: 60px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 24px; font-weight: bold; line-height: 130%; color: #000000; font-family: Roboto;' class='header'>[USUARIO_NOME]</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 60px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 17px; font-weight: 400; line-height: 160%; color: #000000; font-family: Roboto;' class='paragraph'><p>[TEXT_1]</p><p>[TEXT_2]</p></td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 80px; padding-bottom: 5px;' class='button'><a href='[LINK]' target='_blank'> <table border='0' cellpadding='0' cellspacing='0' align='center' style='min-width: 240px; border-collapse: collapse; border-spacing: 0; padding: 0;'><tr><td align='center' valign='middle' style='padding: 12px 24px; margin: 0; border-collapse: collapse; border-spacing: 0; border-radius: 30px; -webkit-border-radius: 30px; -moz-border-radius: 30px; -khtml-border-radius: 30px; color: #FFFFFF; font-size: 18px; font-weight: 700; font-family: Roboto;' bgcolor='#E23000'><span style='display: inline-block; color: #ffffff; cursor: pointer; text-decoration: none;'>[TEXT_LINK]</span></td></tr></table></a></td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 125px;' class='line'><hr color='#E0E0E0' align='center' width='100%' size='1' noshade=noshade style='margin: 0; padding: 0;' /></td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 10px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 12px; line-height: 160%; color: #000000; font-family: Roboto;' class='paragraph'>[EM_CASO_DE_DUVIDA] [EMAIL_SUPORTE].</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 30px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 12px; line-height: 160%; color: #000000; font-family: Roboto;' class='paragraph'>[EMAIL_ENVIADO_PARA] [EMAIL_USUARIO]</td></tr></table></td></tr></table></body></html>";
                cont++;

                var senhaAnterior = usuario.Senha;
                var senhaDescripto = "";
                cont++;
                try
                {
                    senhaDescripto = Helpers.CriptografiaHelper.Descriptografar(senhaAnterior);
                }
                catch (Exception ex)
                {
                    senhaDescripto = senhaAnterior;
                }

                string strLink = ConfiguracaoHelper.GetString("DOMINIO") + "cadastro/ativar?codigo=" + CriptografiaHelper.Morpho(usuario.Login, CriptografiaHelper.TipoCriptografia.Criptografa);
                string strLogo = ConfiguracaoHelper.GetString("DOMINIO") + "/content/img/" + sistema + "/logo-sistema.png";


                var campos = new List<KeyValuePair<string, string>>();
                cont++;
                campos.Add(new KeyValuePair<string, string>("IMG_LOGO", strLogo));
                cont++;
                //campos.Add(new KeyValuePair<string, string>("LINK", HttpContext.Current.Server.UrlEncode(strLink)));
                campos.Add(new KeyValuePair<string, string>("LINK", strLink));
                cont++;
                campos.Add(new KeyValuePair<string, string>("USUARIO_NOME", usuario.Nome));
                cont++;
                campos.Add(new KeyValuePair<string, string>("TEXT_1", traducaoHelper["LOGIN"] + " :" + usuario.Login));
                cont++;
                campos.Add(new KeyValuePair<string, string>("TEXT_2", traducaoHelper["SENHA"] + " :" + senhaDescripto));
                cont++;
                campos.Add(new KeyValuePair<string, string>("SISTEMA", sistema));
                cont++;
                campos.Add(new KeyValuePair<string, string>("MSG_HEADER", traducaoHelper["CONFIRMAR_EMAIL_HEADER"]));
                cont++;
                campos.Add(new KeyValuePair<string, string>("TEXT_LINK", traducaoHelper["CONFIRMAR_EMAIL_LINK"]));
                cont++;
                campos.Add(new KeyValuePair<string, string>("EMAIL_SUPORTE", Helpers.ConfiguracaoHelper.GetString("EMAIL_DE")));
                cont++;
                campos.Add(new KeyValuePair<string, string>("EMAIL_USUARIO", para));
                cont++;
                campos.Add(new KeyValuePair<string, string>("LOGO", Helpers.ConfiguracaoHelper.GetString("EMAIL_DE")));
                cont++;
                campos.Add(new KeyValuePair<string, string>("EM_CASO_DE_DUVIDA", traducaoHelper["EM_CASO_DE_DUVIDA"]));
                cont++;
                campos.Add(new KeyValuePair<string, string>("EMAIL_ENVIADO_PARA", traducaoHelper["EMAIL_ENVIADO_PARA"]));
                cont++;

                if (tipoEnvio.ToUpper() == "SMTP")
                {
                    emailService.Send(de, para, assunto, corpo, true, campos);
                }
                else
                {
                    if (tipoEnvio.ToUpper() == "SENDGRID")
                    {
                        SendGridService.SendGridEnviaAsync(ConfiguracaoHelper.GetString("SENDGRID_API_KEY"), assunto, corpo, de, para, campos);
                    }
                }

                cont++;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                cpUtilities.LoggerHelper.WriteFile("erro envio de email " + tipoEnvio + " (" + cont + ") " + usuario.Email + " : " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), "CadastroEnvioEmail");
            }
        }

        public bool EnviarSenha(string email)
        {
            string tipoEnvio = ConfiguracaoHelper.GetString("EMAIL_TIPO_ENVIO");
            try
            {
                var usuario = usuarioRepository.GetByEmail(email);
                if (usuario != null)
                {
                    var traducaoHelper = new TraducaoHelper(usuario.Pais.Idioma);
                    var emailService = new EmailService();

                    var de = Helpers.ConfiguracaoHelper.GetString("EMAIL_DE");
                    var para = usuario.Email;
                    var assunto = traducaoHelper["EMAIL_SENHA_ASSUNTO"];
                    var corpo = traducaoHelper["EMAIL_SENHA_CORPO"];
                    var team = traducaoHelper["TEAM"];

                    usuario.Senha = Helpers.CriptografiaHelper.Descriptografar(usuario.Senha);

                    ReflectionHelper.Replace(ref corpo, usuario);
                    traducaoHelper.Traduzir(ref corpo);

                    ReflectionHelper.Replace(ref assunto, usuario);
                    traducaoHelper.Traduzir(ref assunto);

                    corpo = corpo.Replace("[TEAM]", team);

                    if (tipoEnvio.ToUpper() == "SMTP")
                    {
                        emailService.Send(de, para, assunto, corpo, true);
                    }
                    else
                    {
                        if (tipoEnvio.ToUpper() == "SENDGRID")
                        {
                            SendGridService.SendGridEnviaAsync(ConfiguracaoHelper.GetString("SENDGRID_API_KEY"), assunto, corpo, de, para);
                        }
                    }

                    return true;
                }
            }
            catch { }
            return false;
        }

        public void EnviarEmailCompraLoja(Entities.Usuario usuario, string codigo, string produtos, int meioPagamentoID, double valorFrete)
        {
            int cont = 0;
            TraducaoHelper traducaoHelper;
            string tipoEnvio = ConfiguracaoHelper.GetString("EMAIL_TIPO_ENVIO");
            try
            {

                if (usuario.Pais != null)
                {
                    traducaoHelper = new TraducaoHelper(usuario.Pais.Idioma);
                }
                else
                {
                    Pais pais = paisRepository.Get(usuario.PaisID);
                    traducaoHelper = new TraducaoHelper(pais.Idioma);
                }

                cont++;
                var emailService = new EmailService();

                var de = Helpers.ConfiguracaoHelper.GetString("EMAIL_DE");
                cont++;
                var para = usuario.Email;
                cont++;
                var assunto = traducaoHelper["PAGAMENTO_SUCESSO_EMAIL_TITULO"] + codigo;
                cont++;
                var corpo = traducaoHelper["PAGAMENTO_SUCESSO_EMAIL_MENSAGEM"];

                cont++;
                string aguadPgto = string.Empty;
                switch (meioPagamentoID)
                {
                    case (int)Core.Entities.PedidoPagamento.MeiosPagamento.Indefinido:
                    case (int)Core.Entities.PedidoPagamento.MeiosPagamento.Boleto:
                    case (int)Core.Entities.PedidoPagamento.MeiosPagamento.Deposito:
                        aguadPgto = traducaoHelper["AGUARDANDO_PAGAMENTO"];
                        break;
                    default:
                        aguadPgto = " ";
                        break;
                }

                cont++;
                var campos = new List<KeyValuePair<string, string>>();
                cont++;
                campos.Add(new KeyValuePair<string, string>("DOMINIO", ConfiguracaoHelper.GetString("DOMINIO")));
                cont++;
                campos.Add(new KeyValuePair<string, string>("CLIENTE", ConfiguracaoHelper.GetString("NOME_SITE")));
                cont++;
                campos.Add(new KeyValuePair<string, string>("AGUARDANDO_PAGAMENTO", aguadPgto));
                cont++;
                campos.Add(new KeyValuePair<string, string>("CODIGO", codigo));
                cont++;
                campos.Add(new KeyValuePair<string, string>("NOME", usuario.Nome));
                cont++;
                campos.Add(new KeyValuePair<string, string>("PRODUTOS", produtos));
                cont++;

                if (tipoEnvio.ToUpper() == "SMTP")
                {
                    emailService.Send(de, para, assunto, corpo, true, campos);
                }
                else
                {
                    if (tipoEnvio.ToUpper() == "SENDGRID")
                    {
                        SendGridService.SendGridEnviaAsync(ConfiguracaoHelper.GetString("SENDGRID_API_KEY"), assunto, corpo, de, para, campos);
                    }
                }
                cont++;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                cpUtilities.LoggerHelper.WriteFile("erro envio de email " + tipoEnvio + " (" + cont + ") " + usuario.Email + " : " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""), "CadastroEnvioEmail");
            }
        }

        public Entities.Usuario ValidarEmail(string codigo)
        {
            var login = CriptografiaHelper.Morpho(codigo,CriptografiaHelper.TipoCriptografia.Descriptografa);
            var usuario = usuarioRepository.GetByLogin(login);
            if (usuario != null)
            {
                usuario.StatusEmail = Entities.Usuario.TodosStatusEmail.Validado;
                usuarioRepository.Save(usuario);
            }
            return usuario;
        }

        /// <summary>
        /// Verifica de deve verificar se CPF já existe na base
        /// e vê ele ele existe
        /// </summary>
        /// <param name="documento"></param>
        /// <returns>Retorna False, caso não tenha que validar o CPF, Retorna false, tambem caso o CPF não exista</returns>
        public bool ExisteCPF(string documento)
        {
            bool blnValidarCPF = false;
            bool blnRetorno = false;

            try
            {
                var validarCPF = Core.Helpers.ConfiguracaoHelper.GetString("EXISTE_CPF");
                if (!String.IsNullOrEmpty(validarCPF))
                {
                    blnValidarCPF = (validarCPF == "true");
                }
                if (blnValidarCPF)
                {
                    var usuario = usuarioRepository.GetByDocumento(documento);
                    if (usuario != null)
                    {
                        blnRetorno = true;
                    }
                    else
                    {
                        blnRetorno = false;
                    }
                }
                else
                {
                    blnRetorno = false;
                }
            }
            catch
            {
                blnRetorno = false;
            }
            //Importante: O Retono so é somente so será true caso a na tabela de configuração 
            //a Chave EXISTE_CPF seja true, e realmente o CPF esteja já cadastrado,
            //qualquer coisa diferente disso retorna false, deixando que o cadastro de um usuario, por exemplo seja efetuado.
            return blnRetorno;
        }

        /*
        public bool ValidarCEP(string strCEP)
        {
          bool blnRetorno = false;
          try
          {
             ZipCodeInfo zipCodeInfo = ZipCodeLoad.Find(strCEP); // BUSCA DE CEP
             //ZipCodeInfo[] zipCodeInfos = ZipCodeLoad.Address(ZipCodeUF.SP, "São", "Mar"); //BUSCA POR ENDEREÇO

             if (zipCodeInfo.Erro == false)
             {
                blnRetorno = true;
             }
             else
             {
                blnRetorno = false;
             }

             //if (zipCodeInfos != null)
             //{
             //}
             //else
             //{
             //   //Error Find
             //}

          }
          catch (ZipCodeException ex)
          {
             blnRetorno = false;
          }
          return blnRetorno;
        }
        */

        private string RemoverCaracteres(string texto)
        {
            string resultado = texto;

            resultado = resultado.Replace("\n", "");
            resultado = resultado.Replace("\r", "");
            resultado = resultado.Replace("\t", "");
            resultado = resultado.Trim();

            return resultado;
        }

        /// <summary>
        /// Obtem o Id da cidade passada o nome da cidade como paramentro
        /// </summary>
        /// <param name="estadoId">Estado para busca da cidades</param>
        /// <param name="cidadeNome">Nome da cidade procurada</param>
        /// <returns></returns>
        public int GetCidadeIDPorNome(int estadoId, string cidadeNome)
        {
            cidadeNome = cidadeNome.Trim();

            int cidadeID = 0;

            var cidades = cidadeRepository.GetByEstado(estadoId);
            var cidadesFiltro = cidades.Where(x => x.Nome.ToLower() == cidadeNome.ToLower());
            foreach (var item in cidadesFiltro)
            {
                cidadeID = item.ID;
            }
            if (cidadeID == 0)
            {
                var cidadesNenhum = cidades.Where(x => x.Nome == "Nenhum");
                foreach (var item in cidadesNenhum)
                {
                    cidadeID = item.ID;
                }
            }

            return cidadeID;
        }

        /// <summary>
        /// Obtem o id do plano de carreira atual do usuario passado como paramentro
        /// </summary>
        /// <param name="idUsuario">id do usuário</param>
        /// <returns>nome do plano de carreira</returns>
        public int PlanoCarreiraNivel(int idUsuario)
        {
            double decPontos = posicaoRepository.Pontos(idUsuario);
            int intID = 1;

            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos <= decPontos).OrderByDescending(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                intID = classificacao.Nivel;
            }

            return intID;
        }

        public int PlanoCarreiraNivel(int idUsuario, double decPontos)
        {
            int intID = 1;

            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos <= decPontos).OrderByDescending(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                intID = classificacao.Nivel;
            }

            return intID;
        }

        /// <summary>
        /// Obtem o nome do plano de carreira atual do usuario passado como paramentro
        /// </summary>
        /// <param name="idUsuario">id do usuário</param>
        /// <returns>nome do plano de carreira</returns>
        public string PlanoCarreira(int idUsuario)
        {
            double decPontos = posicaoRepository.Pontos(idUsuario);
            string strNome = "";

            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos <= decPontos).OrderByDescending(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                strNome = classificacao.Nome;
            }

            return strNome;
        }

        public string PlanoCarreira(int idUsuario, double decPontos)
        {
            string strNome = "";

            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos <= decPontos).OrderByDescending(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                strNome = classificacao.Nome;
            }

            return strNome;
        }

        public decimal PlanoCarreiraPercentagem(int idUsuario)
        {
            double decPontos = posicaoRepository.Pontos(idUsuario);
            double decMin = -1;
            double decMax = -1;
            double decPercentagem = 0;


            //Obtem Ponto maximo da classificacao atual do usuario
            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos >= decPontos).OrderBy(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                decMax = (double)classificacao.Pontos;
            }

            //Obtem Ponto da classificacao anterior do usuario
            if (decMax > 0)
            {
                classificacao = classificacaoRepository.GetByExpression(x => x.Pontos < decMax).OrderByDescending(x => x.Pontos).FirstOrDefault();
                if (classificacao != null)
                {
                    decMin = (double)classificacao.Pontos;
                }
            }

            //Existindo valores obtem o percentual
            if (decMin >= 0)
            {
                //Maximo deve ser maior que minimo
                if (decMax > decMin)
                {
                    decPercentagem = (decPontos - decMin) / (decMax - decMin);
                }
            }

            return (decimal)decPercentagem;
        }

        public decimal PlanoCarreiraPercentagem(int idUsuario, double decPontos)
        {
            double decMin = -1;
            double decMax = -1;
            double decPercentagem = 0;


            //Obtem Ponto maximo da classificacao atual do usuario
            var classificacao = classificacaoRepository.GetByExpression(x => x.Pontos >= decPontos).OrderBy(x => x.Pontos).FirstOrDefault();
            if (classificacao != null)
            {
                decMax = (double)classificacao.Pontos;
            }

            //Obtem Ponto da classificacao anterior do usuario
            if (decMax > 0)
            {
                classificacao = classificacaoRepository.GetByExpression(x => x.Pontos < decMax).OrderByDescending(x => x.Pontos).FirstOrDefault();
                if (classificacao != null)
                {
                    decMin = (double)classificacao.Pontos;
                }
            }

            //Existindo valores obtem o percentual
            if (decMin >= 0)
            {
                //Maximo deve ser maior que minimo
                if (decMax > decMin)
                {
                    decPercentagem = (decPontos - decMin) / (decMax - decMin);
                }
            }

            return (decimal)decPercentagem;
        }

        public void TwoFactorEnabled(int usuarioID, bool TwoFactorEnabled)
        {
            usuarioRepository.TwoFactorEnabled(usuarioID, TwoFactorEnabled);
        }

        public void AdmTwoFactorEnabled(int usuarioID, string chave, bool abilita)
        {
            usuarioRepository.AdmTwoFactorEnabled(usuarioID, chave, abilita);
        }

        //public void AssociarSistemaExterno(int UsuarioID, int ExternoID)
        //{
        //    Externo usuarioExterno = new Externo();
        //    usuarioExterno.UsuarioID = UsuarioID;
        //    usuarioExterno.ExternoID = ExternoID;
        //    usuarioExternoRepository.Save(usuarioExterno);
        //}

        /*
        public void AssociarSiteParceiro(Entities.Usuario u)
        {
            try
            {
                if (Helpers.ConfiguracaoHelper.TemChave("SITE_PARCEIRO_URL_CADASTRO"))
                {

                    cpUtilities.LoggerHelper.WriteFile("chamando AssociarSiteParceiro " + u.Email, "usuarioService");

                    string url = Helpers.ConfiguracaoHelper.GetString("SITE_PARCEIRO_URL_CADASTRO");
                    Hashtable post_values = new Hashtable();

                    post_values.Add("username", u.Login);
                    post_values.Add("password", Helpers.CriptografiaHelper.Descriptografar(u.Senha));
                    post_values.Add("passwordc", Helpers.CriptografiaHelper.Descriptografar(u.Senha));
                    post_values.Add("email", u.Email);
                    post_values.Add("promocode", "");
                    post_values.Add("bDay", u.DataNascimento.Day);
                    post_values.Add("bMonth", u.DataNascimento.Month);
                    post_values.Add("bYear", u.DataNascimento.Year);

                    var resposta = curlService.curl(url, post_values);

                    var sincronizacao = sincronizacaoRepository.GetByExpression(s => s.UsuarioID == u.ID).FirstOrDefault();
                    if (sincronizacao == null)
                    {
                        sincronizacao = new Entities.Sincronizacao();
                    }

                    sincronizacao.UsuarioID = u.ID;
                    sincronizacao.Usuario = u;
                    sincronizacao.Data = App.DateTimeZion;
                    sincronizacao.Observacao = resposta.Length > 500 ? resposta.Substring(0, 499) : resposta;
                    sincronizacao.Status = 0;

                    if (resposta.ToLower().IndexOf("<status>0</status>") > -1)
                    {
                        sincronizacao.Status = 1; //OK
                    }

                    sincronizacaoRepository.Save(sincronizacao);

                }
            }
            catch (Exception ex)
            {
                cpUtilities.LoggerHelper.WriteFile("erro AssociarSiteParceiro " + u.Email + " : " + ex.Message + " - " + (ex.InnerException == null ? "" : ex.InnerException.Message), "UsuarioService");
            }
        }
        */

        #endregion

    }
}
