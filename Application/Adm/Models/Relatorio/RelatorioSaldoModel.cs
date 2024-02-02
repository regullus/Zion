using Core.Models.Relatorios;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Models.Relatorio
{
    public class RelatorioSaldoModel
    {
        public IPagedList<RelatorioSaldoItem> Items { get; set; }
        public RelatorioSaldoResumo Resumo { get; set; }

        public RelatorioSaldoModel(int pageSize)
        {
            Items = new PagedList<RelatorioSaldoItem>(new List<RelatorioSaldoItem>(), 1, pageSize);
            Resumo = new RelatorioSaldoResumo();
        }

    }
}

