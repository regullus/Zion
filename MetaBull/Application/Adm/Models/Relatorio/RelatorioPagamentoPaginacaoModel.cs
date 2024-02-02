using PagedList;
using System.Collections.Generic;
using Core.Models.Relatorios;

namespace Sistema.Models.Relatorio
{
    public class RelatorioPagamentoPaginacaoModel
    {
        public IPagedList<RelatorioPagamentoModel> Itens { get; set; }

        public RelatorioPagamentoPaginacaoModel(int pageSize)
        {
            Itens = new PagedList<RelatorioPagamentoModel>(new List<RelatorioPagamentoModel>(), 1, pageSize);
        }
    }
}