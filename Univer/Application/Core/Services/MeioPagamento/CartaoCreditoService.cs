using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entities;
using Core.Repositories.Financeiro;
using System.Data.Entity;

namespace Core.Services.MeioPagamento
{
    public class CartaoCreditoService
    {
        private CartaoCreditoRepository cartaoCreditoRepository;

        public CartaoCreditoService(DbContext context)
        {
            cartaoCreditoRepository = new CartaoCreditoRepository(context);
        }

        public CartaoCredito ObterPagamentoPorTransacaoID(string transacaoID)
        {
            return this.cartaoCreditoRepository.GetByExpression(p => p.TransacaoID == transacaoID).FirstOrDefault();
        }
    }
}
