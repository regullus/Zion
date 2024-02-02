using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Core.Entities
{
    public class PedidoRelatorioData
    {
        public DateTime DataPedido { get; set; }
        public double Total { get; set; }
        public int QtdePedidos { get; set; }
    }

    public class PedidoRelatorioProduto
    {
        public string DataPedido { get; set; }
        public int idProduto { get; set; }
        public string Produto { get; set; }
        public int FormaPagamento { get; set; }
        public double Total { get; set; }
        public int QtdePedidos { get; set; }
    }

    public class PedidoRelatorioPedido
    {
        public int idPedido { get; set; }
        public string DataPedido { get; set; }
        public string DataPagamento { get; set; }
        public double StatusPagamento { get; set; }
        public double FormaPagamento { get; set; }
        public double TotalFrete { get; set; }
        public double Total { get; set; }
        public int FormaFrete { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public string EnderecoEntrega { get; set; }
        public string NumeroEntrega { get; set; }
        public string BairroEntrega { get; set; }
        public string CidadeEntrega { get; set; }
        public string EstadoEntrega { get; set; }
        public string CepEntrega { get; set; }
    }

    public class PedidoRelatorioFranqueado
    {
        public string Login { get; set; }
        public string Nome { get; set; }
        public double Total { get; set; }
        public int QtdePedidos { get; set; }
    }

    public class PedidoRelatorioFatFranqueado
    {
        public string Pais { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public double Total { get; set; }

    }

    public class PedidoRelatorioResidualAtivoMensal
    {
        public string A1_COD { get; set; }
        public string Login { get; set; }
        public string LoginPatrocinador { get; set; }
        public int Nivel { get; set; }
        public double TOTAL_ATIVO_MENSAL { get; set; }
        public double TOTAL_PONTOS_ATIVO_MENSAL { get; set; }
        public double PERCENTUAL_RESIDUAL { get; set; }
        public double TOTAL_RESIDUAL { get; set; }
    }





    public class PedidoRelatorioLancamento
    {
        public string Data { get; set; }
        public double ValorCreditado { get; set; }
        public double ValorDebitado { get; set; }
    }

    public class PedidoRelatorioLancamentoDia
    {
        public string Data { get; set; }
        public string Tipo { get; set; }
        public double Valor { get; set; }
        public int TipoLancamento { get; set; }
    }

    public class PedidoRelatorioLancamentoDiaFranqueado
    {
        public string idFranqueado { get; set; }
        public string loginFranqueado { get; set; }
        public double Valor { get; set; }
    }


}