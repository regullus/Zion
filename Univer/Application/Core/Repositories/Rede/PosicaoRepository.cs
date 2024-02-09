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
using System.Data.Entity.Core.Objects;

namespace Core.Repositories.Rede
{
    public class PosicaoRepository : PersistentRepository<Entities.Posicao>
    {

        private DbContext _context;

        public PosicaoRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public double AcumuladoEsquerda(int idUsuario)
        {
            double decReturn = 0;
            var posicao = this.GetByExpression(x => x.UsuarioID == idUsuario).OrderByDescending(x => x.ID).FirstOrDefault();
            if (posicao != null)
            {
                decReturn = (double)posicao.AcumuladoEsquerda;
            }
            else
            {
                decReturn = 0;
            }

            return decReturn;
        }

        public double AcumuladoDireita(int idUsuario)
        {
            double decReturn = 0;
            var posicao = this.GetByExpression(x => x.UsuarioID == idUsuario).OrderByDescending(x => x.ID).FirstOrDefault();
            if (posicao != null)
            {
                decReturn = (double)posicao.AcumuladoDireita;
            }
            else
            {
                decReturn = 0;
            }
            return decReturn;
        }

        public decimal PontosOld(int idUsuario)
        {
            Decimal decRetorno = 0m;
            IEnumerable<Models.StoredProcedures.spPontuacaoPosicao> objReg;

            try
            {
                var sql = string.Format("Exec spPontuacaoPosicao {0}", idUsuario);
                objReg = _context.Database.SqlQuery<Models.StoredProcedures.spPontuacaoPosicao>(sql).ToList();

                if (objReg.Count() > 0)
                {
                    foreach (var item in objReg)
                    {
                        decRetorno = item.Pontos ?? 0m;
                    }
                }
            }
            catch (Exception)
            {
                decRetorno = 0m;
            }

            return decRetorno;
        }

        public double Pontos(int idUsuario)
        {
            double pontos = 0;

            try
            {
                Entities.Posicao posicao = new Entities.Posicao();
                posicao = base.GetByExpression(p => p.UsuarioID == idUsuario).OrderByDescending(p => p.DataFim).FirstOrDefault();

                if (posicao.AcumuladoDireita < posicao.AcumuladoEsquerda)
                    pontos = (double)posicao.AcumuladoDireita;
                else
                    pontos = (double)posicao.AcumuladoEsquerda;
            }
            catch (Exception ex)
            {
                //sem dados
            }

            return pontos;
        }

        public double? ObtemPontuacao(int idUsuario)
        {
            string sql = "EXEC spOC_US_ObtemPontuacao " + idUsuario;

            var retorno = _context.Database.SqlQuery<double?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;

        }

        public double? ObtemPontuacaoCicloVt(int idUsuario)
        {
            string sql = "EXEC spOC_US_ObtemPontuacaoCiclo " + idUsuario + " , 'Vt' ";

            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public double? ObtemPontuacaoCiclo(int idUsuario, string tipoPonto, string tipoCiclo)
        {
            string sql = "EXEC spOC_US_ObtemPontuacaoCiclo " + idUsuario + " ," + tipoPonto + ", " + tipoCiclo;

            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public double? ObtemPontuacaoMaxima(int idUsuario, string tipoPonto)
        {
            string sql = "EXEC spOC_US_ObtemMaximaPontuacao " + idUsuario + " ," + tipoPonto;

            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public double? ObtemPontuacaTotal(int idUsuario)
        {
            string sql = "EXEC spOC_US_ObtemTotalPontuacao " + idUsuario;

            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public double? ObtemPontuacaoCicloVq(int idUsuario)
        {
            string sql = "EXEC spOC_US_ObtemPontuacaoCiclo " + idUsuario + " , 'Vq' ";
            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public int? ObtemTotalAfiliadosRede(int idUsuario)
        {
            string sql = "EXEC spC_ObtemTotalAfiliadosRede " + idUsuario;

            var retorno = _context.Database.SqlQuery<int?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }

        public double ObtemBonusTetoGanhoTotal(int idUsuario)
        {
            string sql = "spOC_US_ObtemBonusTetoGanhoTotal " + idUsuario;
            return _context.Database.SqlQuery<double>(sql).FirstOrDefault();
        }

        public double ObtemBonusTetoGanhoAlavancagem(int idUsuario)
        {
            string sql = "spOC_US_ObtemBonusTetoGanhoAlavancagem " + idUsuario;
            return _context.Database.SqlQuery<double>(sql).FirstOrDefault();
        }

        public double? ObtemBonusTetoGanhoBinario(int idUsuario)
        {
            string sql = "spOC_US_ObtemBonusTetoGanhoBinario " + idUsuario;
            var retorno = _context.Database.SqlQuery<double?>(sql).FirstOrDefault();
            return retorno != null ? retorno : 0;
        }
    }
}
