namespace Core.Models.Relatorios
{
    public class RelatorioPagamentoModel
    {
        public string Login { get; set; }
        public string Nome { get; set; }
        public string Celular { get; set; }
        public string SKU { get; set; }
        public string Produto { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string CodigoPostal { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string LogradouroAlt { get; set; }
        public string NumeroAlt { get; set; }
        public string ComplementoAlt { get; set; }
        public string CodigoPostalAlt { get; set; }
        public string CidadeAlt { get; set; }
        public string EstadoAlt { get; set; }
        public string DataPagamento { get; set; }
        public string Codigo { get; set; }
        public double Total { get; set; }
        public double TotalBTC { get; set; }
        public string Email { get; set; }

        public string EnderecoPrincipal { get; set; }
    }
}