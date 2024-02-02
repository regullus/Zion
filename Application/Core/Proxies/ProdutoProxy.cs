using Core.Repositories.Loja;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Proxies
{
    public class ProdutoProxy : Services.Loja.Produtos.BaseService
    {

        private Services.Loja.Produtos.BaseService service;
        private DbContext _context;

        public ProdutoProxy(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public override void Liberar(Entities.PedidoItem pedidoItem)
        {
            base.Liberar(pedidoItem);

            InvestigacaoLog.LogNivelAssociacao(pedidoItem.Pedido.UsuarioID, 10);

            var produtoRepository = new ProdutoRepository(_context);
            var produto = produtoRepository.Get(pedidoItem.ProdutoID);

            InvestigacaoLog.LogNivelAssociacao(pedidoItem.Pedido.UsuarioID, 11, (produto != null ? "Achou o produto, tipo: " + produto.TipoID : "Não achou o produto")); ;

            if (produto != null)
            {
                switch (produto.Tipo)
                {
                    case Entities.Produto.Tipos.Associacao:
                        service = new Services.Loja.Produtos.AssociacaoService(_context);
                        break;
                    case Entities.Produto.Tipos.Upgrade:
                        service = new Services.Loja.Produtos.UpgradeService(_context);
                        break;
                    case Entities.Produto.Tipos.ProdutoFisico:
                        if (produto.AtivoMensal)
                            service = new Services.Loja.Produtos.AssociacaoService(_context);
                        else
                            service = new Services.Loja.Produtos.ProdutoFisicoService(_context);
                        break;
                    case Entities.Produto.Tipos.ProdutoVirtual:
                        service = new Services.Loja.Produtos.ProdutoVirtualService(_context);
                        break;
                    case Entities.Produto.Tipos.AtivoMensal:
                        service = new Services.Loja.Produtos.AtivoMensalService(_context);
                        break;
                    case Entities.Produto.Tipos.RenovacaoAssinatura:
                        service = new Services.Loja.Produtos.RenovacaoAssinaturaService(_context);
                        break;

                }
                service.Liberar(pedidoItem);
            }
        }

    }
}
