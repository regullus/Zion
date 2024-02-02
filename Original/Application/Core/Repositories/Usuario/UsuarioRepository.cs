using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Core.Repositories.Usuario
{
    public class UsuarioRepository : PersistentRepository<Entities.Usuario>
    {
        DbContext _context;

        public UsuarioRepository(DbContext context)
           : base(context)
        {
            _context = context;
        }

        public override IQueryable<Entities.Usuario> GetAll()
        {
            return base.GetByExpression(u => u.Oculto == false);
        }

        public IEnumerable<Entities.Usuario> Buscar(string termo, bool apenasAtivos = false, bool isAdmin = false)
        {
            if (isAdmin)
            {
                return base.GetByExpression(u => u.Oculto == false && (u.Login.Contains(termo) || u.Email.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
            }
            else
            {
                if (apenasAtivos)
                {
                    var statusAtivo = Entities.Usuario.TodosStatus.Associado.GetHashCode();
                    return base.GetByExpression(u => u.StatusID == statusAtivo && u.Oculto == false && u.Bloqueado == false && (u.Login.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
                }
                else
                {
                    return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && (u.Login.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
                }
            }
        }

        public IEnumerable<Entities.Usuario> BuscarAssinatura(string termo, string assinatura, bool apenasAtivos = false, bool isAdmin = false)
        {
            if (isAdmin)
            {
                return base.GetByExpression(u => u.Oculto == false && u.Assinatura.Contains(assinatura) && (u.Login.Contains(termo) || u.Email.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
            }
            else
            {
                if (apenasAtivos)
                {
                    var statusAtivo = Entities.Usuario.TodosStatus.Associado.GetHashCode();
                    return base.GetByExpression(u => u.StatusID == statusAtivo && u.Oculto == false && u.Bloqueado == false && u.Assinatura.Contains(assinatura) && (u.Login.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
                }
                else
                {
                    return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.Assinatura.Contains(assinatura) && (u.Login.Contains(termo) || u.Nome.Contains(termo) || u.NomeFantasia.Contains(termo) || u.Apelido.Contains(termo)));
                }
            }
        }

        public Entities.Usuario GetByLogin(string login)
        {
            login = login.ToLower();
            return base.GetByExpression(u => u.Oculto == false && u.Login.ToLower() == login).FirstOrDefault();
        }

        public Entities.Usuario GetByAssinatura(string assinatura)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Assinatura == assinatura).FirstOrDefault();
        }

        public Entities.Usuario GetByAutenticacao(int intIdAutenticacao)
        {
            return base.GetByExpression(u => u.Oculto == false && u.IdAutenticacao == intIdAutenticacao).FirstOrDefault();
        }

        public Entities.Usuario GetByEmail(string email)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.Email == email).FirstOrDefault();
        }

        public Entities.Usuario GetByLoginSenha(string login, string senha)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.Login == login && u.Senha == senha).FirstOrDefault();
        }

        public IEnumerable<Entities.UsuarioDireto> GetDiretos(int idUsuario)
        {
            string sql = "select UserID, NivelAssociacao, Status, Nivel from fn_DiretosPorUsuario(" + idUsuario + ", 7)";

            return _context.Database.SqlQuery<Entities.UsuarioDireto>(sql).ToList();
        }

        public bool VerificaDiretoAssociado(int idUsuario)
        {
            string sql = "exec spC_VerificaDiretoAssociado " + idUsuario + ", 7";

            return _context.Database.SqlQuery<bool>(sql).FirstOrDefault();
        }

        public Entities.Usuario GetByID(int idUsuario)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.ID == idUsuario).FirstOrDefault();
        }

        public List<Entities.UsuarioEntradaDia> GetEntradasPorData(int idUsuario, int Niveis, DateTime dataCorte)
        {
            string sql = "select Data, ";
            sql += " sum(CASE WHEN Nivel = 1 then Qtde Else 0 END) as Classificacao1, ";
            sql += " sum(CASE WHEN Nivel = 2 then Qtde Else 0 END) as Classificacao2, ";
            sql += " sum(CASE WHEN Nivel = 3 then Qtde Else 0 END) as Classificacao3, ";
            sql += " sum(CASE WHEN Nivel = 4 then Qtde Else 0 END) as Classificacao4, ";
            sql += " sum(CASE WHEN Nivel = 5 then Qtde Else 0 END) as Classificacao5 ";
            sql += " from  ";
            sql += " ( ";
            sql += " select   ";
            sql += " convert(date, F.DataCriacao) as Data,  ";
            sql += " F.NivelAssociacao as Nivel,  ";
            sql += " COUNT(*) as Qtde ";
            sql += " From fn_DiretosPorUsuario (" + idUsuario + ", " + Niveis + ") H ";
            sql += " inner join Usuario.Usuario F on H.UserID = F.ID ";
            sql += " where F.StatusID = 2 ";
            sql += " and F.DataCriacao >= '" + dataCorte.ToString("yyyy-MM-dd") + " 00:00:00' ";
            sql += " group by convert(date, F.DataCriacao), F.NivelAssociacao ";
            sql += " ) Dados ";
            sql += " group by Data order by 1 ";

            return _context.Database.SqlQuery<Entities.UsuarioEntradaDia>(sql).ToList();
        }

        public List<Entities.UsuarioRelatorio> GetRelatorioUsuarios()
        {
            var sql = "SELECT * FROM V_UsuariosGeral";
            return _context.Database.SqlQuery<Entities.UsuarioRelatorio>(sql).ToList();
        }

        public Entities.Usuario GetByDocumento(string documento)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.Documento == documento).FirstOrDefault();
        }

        public int CountByDocumento(string documento)
        {
            return base.GetByExpression(u => u.Oculto == false && u.Bloqueado == false && u.Documento == documento).Count();
        }

        public double Saldo(int idUsuario)
        {
            Entities.Usuario usuario = new Entities.Usuario();
            usuario = GetByID(idUsuario);

            var lancamentosSaldo = usuario.Lancamento.Where(l => l.ContaID == 1);
            if (lancamentosSaldo != null)
            {
                return (double)lancamentosSaldo.Sum(l => l.Valor);
            }
            return 0;
        }

        public double Pontos(int idUsuario)
        {
            double? decReturn = 0;
            Entities.Usuario usuario = new Entities.Usuario();
            usuario = GetByID(idUsuario);

            var contasPontos = usuario.Lancamento.Where(l => l.ContaID == 2).GroupBy(l => l.Conta);
            foreach (var conta in contasPontos)
            {
                decReturn = conta.Sum(l => l.Valor);
            }

            return (double)decReturn;
        }

        public IEnumerable<Models.StoredProcedures.spC_ObtemDiretos> GetMeusDiretos(int idUsuario, int exibeNivel, int maxNivel)
        {
            try
            {
                var sql = string.Format("Exec spC_ObtemDiretos {0}, {1}, {2}, {3}, {4} ", idUsuario, idUsuario, 1, maxNivel, exibeNivel);
                return _context.Database.SqlQuery<Models.StoredProcedures.spC_ObtemDiretos>(sql).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int GetPontos(int idUsuario)
        {
            int ret = 0;
            
            try
            {
                string sql = "EXEC spC_PontosAcumulados @UsuarioID=" + idUsuario;
                ret =  _context.Database.SqlQuery<int>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {
                String erro = ex.Message;
                ret = 0;
            }

            return ret;
        }

        public IEnumerable<Models.StoredProcedures.spC_AtivoMensal> GetAtivoMensal(int idUsuario)
        {
            try
            {
                var sql = string.Format("Exec spC_AtivoMensal {0}", idUsuario);
                return _context.Database.SqlQuery<Models.StoredProcedures.spC_AtivoMensal>(sql).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> GetUltimaAssinatura()
        {
            string sql =
               "select top (1) u.Assinatura " +
               "from Usuario.Usuario u " +
               "where u.NivelAssociacao > 0 " +
               "order by u.DataAtivacao desc ";

            return _context.Database.SqlQuery<string>(sql).ToList();
        }

        public List<string> GetUltimaAssinatura(Entities.Usuario patricinador)
        {
            string sql =
              "select top (1) u.Assinatura " +
              "from Usuario.Usuario u " +
              "where u.NivelAssociacao > 0 " +
              "  and u.Assinatura like '" + patricinador.Assinatura + "%' " +
              "order by u.DataAtivacao desc";

            return _context.Database.SqlQuery<string>(sql).ToList();
        }
        public List<string> GetUltimaAssinaturaPerna(int usuarioID, int perna, int redeBinaria)
        {
            try
            {
                var sql = string.Format("Exec spOC_US_ObtemUltimaAssinaturaPerna {0}, '{1}', {2}", usuarioID, perna, redeBinaria);
                return _context.Database.SqlQuery<string>(sql).ToList();
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        public List<string> GetListaAssinatura(Entities.Usuario patricinador)
        {
            string sql =
             "select u.Assinatura " +
             "from Usuario.Usuario u " +
             "where u.Assinatura like '" + patricinador.Assinatura + "%' " +
             "order by Assinatura";

            return _context.Database.SqlQuery<string>(sql).ToList();
        }

        public IEnumerable<Entities.UsuarioInativo> GetUsuariosInativos(int usuarioID, int qtdeNivel)
        {
            var sql = string.Format("Exec spOC_US_ObtemUsuariosInativos {0}, {1}", usuarioID, qtdeNivel);
            return _context.Database.SqlQuery<Entities.UsuarioInativo>(sql).ToList();
        }
        public List<SelectListItem> GetListaDerramamento(Entities.Usuario usuario, Helpers.TraducaoHelper traducaoHelper)
        {
            List<SelectListItem> ret = new List<SelectListItem>();

            if (Helpers.ConfiguracaoHelper.GetString("REDE_BINARIA") == "true")
            {
                ret.Add(new SelectListItem() { Text = traducaoHelper["ESQUERDA"], Value = "1", Selected = usuario.DerramamentoID == 1 });
                ret.Add(new SelectListItem() { Text = traducaoHelper["DIREITA"], Value = "2", Selected = usuario.DerramamentoID == 2 });
            }
            else
            {
                int totalCol = GetQuantidadeColunasOcupadas(usuario);
                int qtdeRamos = Helpers.ConfiguracaoHelper.GetInt("REDE_QTDE_RAMOS");

                if (totalCol >= qtdeRamos)
                {
                    totalCol = qtdeRamos - 1;
                }

                for (int i = 0; i <= totalCol; i++)
                {
                    ret.Add(new SelectListItem() { Text = traducaoHelper["COLUNA" + i], Value = "1" + i });
                }
            }

            return ret;
        }
        public int GetQuantidadeColunasOcupadas(Entities.Usuario usuario)
        {
            int ret = 0;

            string sql =
                "Select Count(id) " +
                "From Usuario.Usuario (nolock) " +
                "Where PatrocinadorPosicaoID = " + usuario.ID;

            ret = _context.Database.SqlQuery<int>(sql).FirstOrDefault();

            return ret;
        }

        public int GetPatrocinadorDiretoMigrado(int usuarioID)
        {
            var sql = string.Format("Exec spOC_US_ObtemPatrocinadorDiretoMigrado {0}", usuarioID);
            return _context.Database.SqlQuery<int>(sql).FirstOrDefault();
        }

        public void TwoFactorEnabled(int usuarioID, bool TwoFactorEnabled)
        {
            var sql = string.Format("Exec spOC_US_TwoFactor @id={0}, @valor='{1}'", usuarioID, TwoFactorEnabled.ToString().ToLower());
            _context.Database.ExecuteSqlCommand(sql);
        }

        public void AdmTwoFactorEnabled(int usuarioID, string chave, bool abilita)
        {
            var sql = string.Format("Exec spOC_US_AdmTwoFactor @id={0}, @valor='{1}', @abilita='{2}'", usuarioID, chave, abilita.ToString().ToLower());
            _context.Database.ExecuteSqlCommand(sql);
        }

        public IEnumerable<Entities.UsuarioMigrarRede> GetUsuarioMigrarRede(int usuarioID)
        {
            string sql = string.Format("Exec spOG_RE_GeraUsuariosMigracaoRede {0}", usuarioID);

            return _context.Database.SqlQuery<Entities.UsuarioMigrarRede>(sql).ToList();

        }

    }
}
