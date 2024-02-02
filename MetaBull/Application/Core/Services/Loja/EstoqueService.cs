using Core.Proxies;
using Core.Repositories.Loja;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using Core.Entities;
using Core.Helpers;

namespace Core.Services.Loja
{
    public class EstoqueService
    {

        private PedidoRepository pedidoRepository;
        private PedidoItemRepository pedidoItemRepository;
        private PedidoItemStatusEntregaRepository pedidoItemStatusEntregaRepository;
        private EstoqueSaldoRepository estoqueSaldoRepository;
        private EstoqueMovimentoRepository estoqueMovimentoRepository;

        private ProdutoProxy produtoProxy;

        public EstoqueService(DbContext context)
        {
            pedidoRepository = new PedidoRepository(context);
            pedidoItemRepository = new PedidoItemRepository(context);
            pedidoItemStatusEntregaRepository = new PedidoItemStatusEntregaRepository(context);
            estoqueMovimentoRepository = new EstoqueMovimentoRepository(context);
            estoqueSaldoRepository = new EstoqueSaldoRepository(context);

        }

        public void Alterar(int armazemID, int pedidoID, int produtoID, int quantidade, string produtoDescricao = "")
        {
            using (TransactionScope transacao = new TransactionScope(TransactionScopeOption.Required))
            {
                estoqueMovimentoRepository.Save(new EstoqueMovimento()
                {
                    ArmazemID = armazemID,
                    Data = App.DateTimeZion,
                    PedidoID = pedidoID,
                    ProdutoID = produtoID,
                    ProdutoDescricao = produtoDescricao,
                    Quantidade = quantidade,
                    TipoID = (quantidade > 0 ? EstoqueMovimento.TipoEntrada : EstoqueMovimento.TipoSaida)
                });

                var estoqueSaldo = estoqueSaldoRepository.GetByExpression(e => e.ArmazemID == armazemID && e.ProdutoID == produtoID).FirstOrDefault();
                if (estoqueSaldo == null) estoqueSaldo = new EstoqueSaldo();
                estoqueSaldo.ProdutoID = produtoID;
                estoqueSaldo.ArmazemID = armazemID;
                estoqueSaldo.Data = App.DateTimeZion;
                estoqueSaldo.Quantidade = estoqueSaldo.Quantidade + quantidade;

                estoqueSaldoRepository.Save(estoqueSaldo);

                transacao.Complete();
            }
        }

    }
}
