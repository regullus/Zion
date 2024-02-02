using Core.Helpers;
using Core.Models.Relatorios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Relatorios
{

    public class RelatorioRepository
    {
        private DbContext _context;

        public RelatorioRepository(DbContext context)
        {
            _context = context;
        }

        public RelatorioSaldo RelatorioSaldo(string inicio, string fim, int status, string login, bool porAssinatura)
        {
            var retorno = new RelatorioSaldo();
            try
            {
                retorno.Itens = RelatorioSaldoItens(inicio, fim, status, login, porAssinatura);

                retorno.Resumo = RelatorioSaldoResumo(inicio, fim, login, porAssinatura);
            }
            catch (Exception)
            {
                retorno = null;
            }

            return retorno;
        }

        public List<RelatorioSaldoItem> RelatorioSaldoItens(string inicio, string fim, int status, string login, bool porAssinatura)
        {
            var relatorios = new List<RelatorioSaldoItem>();

            var sp = String.Format("EXEC spOC_FI_RelatorioSaldo '{0}', '{1}', {2}, '{3}', {4}", inicio, fim, status, login, (porAssinatura ? 1 : 0));

            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioSaldoItem>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }
            return relatorios;
        }

        public RelatorioSaldoResumo RelatorioSaldoResumo(string inicio, string fim, string login, bool porAssinatura)
        {
            var retorno = new RelatorioSaldoResumo();

            var sp = String.Format("EXEC spOC_FI_RelatorioSaldoResumo '{0}', '{1}', '{2}', {3}", inicio, fim, login, (porAssinatura ? 1 : 0));

            try
            {
                var resumo = _context.Database.SqlQuery<RelatorioSaldoResumo>(sp).ToList();
                if (resumo.Any())
                {
                    retorno = resumo.First();
                }
            }
            catch (Exception)
            {
                retorno = null;
            }
            
            return retorno;
        }

        public List<RelatorioPagamentoModel> RelatorioPagamento(DateTime? DataInicio, DateTime? DataFim)
        {
            return _context.Database.SqlQuery<RelatorioPagamentoModel>("EXEC [dbo].[spOC_FI_RelatorioPagamento] @DataIni, @DataFim",
                DataInicio.HasValue ?
                    new SqlParameter("@DataIni", SqlDbType.DateTime) { Value = DataInicio.Value }
                    : new SqlParameter("@DataIni", SqlDbType.DateTime) { Value = DBNull.Value },
                DataFim.HasValue ?
                    new SqlParameter("@DataFim", SqlDbType.DateTime) { Value = DataFim.Value }
                    : new SqlParameter("@DataFim", SqlDbType.DateTime) { Value = DBNull.Value }
                ).ToList();
        }

        public List<RelatorioPontosModel> RelatorioPontos(int usuarioId, int cicloId)
        {
            var relatorios = new List<RelatorioPontosModel>();

            var sp = String.Format("EXEC spOC_ObtemPontosUsuarioCiclo '{0}', '{1}' ", usuarioId, cicloId);

            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPontosModel>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }
            return relatorios;
        }

        public List<RelatorioPedidosModel> RelatorioPedidosAdesaoUpgrade(string dtIni, string dtFim, string login, int? statusId = null)
        {
            var relatorios = new List<RelatorioPedidosModel>();

            string sp = String.Format("Exec spOC_LO_ListaPedidosAdesaoUpgrade @DataIni='{0}', @DataFim='{1}', @Identificacao='{2}', @StatusId={3} ",
                                        Helpers.ProcedureHelper.ConverterDataInicio(dtIni), Helpers.ProcedureHelper.ConverterDataFim(dtFim), login, statusId);
                       
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPedidosModel>(sp).ToList();
            }
            catch (Exception)
            {

                relatorios = null;
            }
            return relatorios;
        }

        public List<RelatorioPedidosModel> RelatorioVendas(int? tipo,
                                                           string dtIni,
                                                           string dtFim,
                                                           string login,
                                                           string produto,
                                                           int? statusId = null,
                                                           int? categoriaId = null,
                                                           int? ProdutoTipo = null,
                                                           int? ProcuraMeioPagamento = null)
        {
            var relatorios = new List<RelatorioPedidosModel>();


            string sp = String.Format("Exec spOC_LO_ListaPedidosVenda @Tipo={0}, @DataIni='{1}', @DataFim='{2}', @Login='{3}', @StatusId={4}, @Categoria={5}, @Produto='{6}', @ProdutoTipo={7}, @MeioPagamento={8}",
                                      tipo, Helpers.ProcedureHelper.ConverterDataInicio(dtIni), Helpers.ProcedureHelper.ConverterDataFim(dtFim), login, statusId, categoriaId, produto, ProdutoTipo, ProcuraMeioPagamento);
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPedidosModel>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }

            return relatorios;
        }

        public List<RelatorioPedidosModel> RelatorioVendasComposicao(string dtIni,
                                                           string dtFim,
                                                           string login,
                                                           string produto,
                                                           int? statusId = null,
                                                           int? categoriaId = null,
                                                           int? ProdutoTipo = null,
                                                           int? ProcuraMeioPagamento = null)
        {
            var relatorios = new List<RelatorioPedidosModel>();


            string sp = String.Format("Exec spOC_LO_ListaPedidosVendaComposicao @DataIni='{0}', @DataFim='{1}', @Login='{2}', @StatusId={3}, @Categoria={4}, @Produto='{5}', @ProdutoTipo={6}, @MeioPagamento={7}",
                                      Helpers.ProcedureHelper.ConverterDataInicio(dtIni), Helpers.ProcedureHelper.ConverterDataFim(dtFim), login, statusId, categoriaId, produto, ProdutoTipo, ProcuraMeioPagamento);
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPedidosModel>(sp).ToList();
            }
            catch (Exception)
            {

                relatorios = null;
            }

            return relatorios;
        }

        

        public List<RelatorioBonusPagosModel> RelatorioBonusPagos(string dtIni, string dtFim, string login, int? categoriaId = null)
        {
            var relatorios = new List<RelatorioBonusPagosModel>();

            string sp = String.Format("Exec spOC_RE_ListaBonusPagos @DataIni='{0}', @DataFim='{1}', @Identificacao='{2}', @CategoriaId={3} ",
                                       Helpers.ProcedureHelper.ConverterDataInicio(dtIni), Helpers.ProcedureHelper.ConverterDataFim(dtFim), login, categoriaId);
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioBonusPagosModel>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }

            return relatorios;
        }

        public List<RelatorioPontosBinarioModel> RelatorioPontosBinario(int usuarioID, string dtIni, string dtFim)
        {
            var relatorios = new List<RelatorioPontosBinarioModel>();

            string sp = String.Format(  "Exec spOC_RE_ObtemPontosBinarioUsuario @UsuarioID={0}, @DataIni='{1}', @DataFim='{2}' ",
                                        usuarioID, 
                                        Helpers.ProcedureHelper.ConverterDataInicio(dtIni), 
                                        Helpers.ProcedureHelper.ConverterDataFim(dtFim)
                                     );
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPontosBinarioModel>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }

            return relatorios;
        }

        public List<RelatorioPontosBinarioModel> RelatorioAcumuladoBinario(int usuarioID)
        {
            var relatorios = new List<RelatorioPontosBinarioModel>();

            string sp = String.Format("Exec spOC_RE_ObtemAcumuladoBinarioUsuario @UsuarioID={0} ", usuarioID );
            try
            {
                relatorios = _context.Database.SqlQuery<RelatorioPontosBinarioModel>(sp).ToList();
            }
            catch (Exception)
            {
                relatorios = null;
            }
           
            return relatorios;
        }
    }
}
