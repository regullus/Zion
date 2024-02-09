using Core.Proxies;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Entities;
using System.Net.Http;
using Core.Repositories.Financeiro;
using Core.Repositories.Usuario;
using Core.Repositories.Rede;
using static Core.Entities.Lancamento;
using static Core.Entities.Categoria;
using static Core.Entities.Conta;
using static Core.Entities.SplitCripto;

namespace Core.Services.Financeiro
{
    public class SplitCriptoService
    {

        private SplitCriptoRepository splitCriptoRepository;
        private PedidoRepository pedidoRepository;
        private PedidoPagamentoRepository pedidoPagamentoRepository;
        private PedidoPagamentoStatusRepository pedidoPagamentoStatusRepository;
        private PedidoItemStatusRepository pedidoItemStatusRepository;
        private LancamentoRepository lancamentoRepository;
        private UsuarioComplementoRepository complementoRepository;
        private BitCoinHelper bitCoinHelper;
        private PontosBinarioRepository pontosBinarioRepository;
        private CicloRepository cicloRepository;
        private ProdutoProxy produtoProxy;

        public SplitCriptoService(DbContext context)
        {
            splitCriptoRepository = new SplitCriptoRepository(context);
            pedidoRepository = new PedidoRepository(context);
            pedidoPagamentoRepository = new PedidoPagamentoRepository(context);
            pedidoPagamentoStatusRepository = new PedidoPagamentoStatusRepository(context);
            pedidoItemStatusRepository = new PedidoItemStatusRepository(context);
            bitCoinHelper = new BitCoinHelper(context);
            lancamentoRepository = new LancamentoRepository(context);
            complementoRepository = new UsuarioComplementoRepository(context);
            produtoProxy = new ProdutoProxy(context);
            pontosBinarioRepository = new PontosBinarioRepository(context);
            cicloRepository = new CicloRepository(context);
        }

        public List<Pedido> ObterSplitCriptos(int plataforma)
        {
            return this.pedidoRepository.GetByExpression(p => p.DataIntegracao == null && p.PedidoPagamento.OrderByDescending(pp => pp.ID).FirstOrDefault().PedidoPagamentoStatus.OrderByDescending(pps => pps.Data).FirstOrDefault().StatusID.Equals(3)).OrderBy(p => p.DataCriacao).ToList();
        }


        public List<SplitCripto> ObterSplitCriptoPorIdTransacaoCoinpayments(string id)
        {
            var splits = this.splitCriptoRepository.GetByExpression(x => x.IdGateway == id).ToList();

            return splits;
            
        }
    }
}
