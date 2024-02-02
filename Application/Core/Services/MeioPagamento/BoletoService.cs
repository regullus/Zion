using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Xml.Linq;

using Newtonsoft.Json;

using Core.Helpers;

namespace Core.Services.MeioPagamento
{

    public class RegistroEntrada
    {
        public string nuCPFCNPJ { get; set; }
        public string filialCPFCNPJ { get; set; }
        public string ctrlCPFCNPJ { get; set; }
        public string cdTipoAcesso { get; set; }
        public string clubBanco { get; set; }
        public string cdTipoContrato { get; set; }
        public string nuSequenciaContrato { get; set; }
        public string idProduto { get; set; }
        public string nuNegociacao { get; set; }
        public string cdBanco { get; set; }
        public string eNuSequenciaContrato { get; set; }
        public string tpRegistro { get; set; }
        public string cdProduto { get; set; }
        public string nuTitulo { get; set; }
        public string nuCliente { get; set; }
        public string dtEmissaoTitulo { get; set; }
        public string dtVencimentoTitulo { get; set; }
        public string tpVencimento { get; set; }
        public string vlNominalTitulo { get; set; }
        public string cdEspecieTitulo { get; set; }
        public string tpProtestoAutomaticoNegativacao { get; set; }
        public string prazoProtestoAutomaticoNegativacao { get; set; }
        public string controleParticipante { get; set; }
        public string cdPagamentoParcial { get; set; }
        public string qtdePagamentoParcial { get; set; }
        public string percentualJuros { get; set; }
        public string vlJuros { get; set; }
        public string qtdeDiasJuros { get; set; }
        public string percentualMulta { get; set; }
        public string vlMulta { get; set; }
        public string qtdeDiasMulta { get; set; }
        public string percentualDesconto1 { get; set; }
        public string vlDesconto1 { get; set; }
        public string dataLimiteDesconto1 { get; set; }
        public string percentualDesconto2 { get; set; }
        public string vlDesconto2 { get; set; }
        public string dataLimiteDesconto2 { get; set; }
        public string percentualDesconto3 { get; set; }
        public string vlDesconto3 { get; set; }
        public string dataLimiteDesconto3 { get; set; }
        public string prazoBonificacao { get; set; }
        public string percentualBonificacao { get; set; }
        public string vlBonificacao { get; set; }
        public string dtLimiteBonificacao { get; set; }
        public string vlAbatimento { get; set; }
        public string vlIOF { get; set; }
        public string nomePagador { get; set; }
        public string logradouroPagador { get; set; }
        public string nuLogradouroPagador { get; set; }
        public string complementoLogradouroPagador { get; set; }
        public string cepPagador { get; set; }
        public string complementoCepPagador { get; set; }
        public string bairroPagador { get; set; }
        public string municipioPagador { get; set; }
        public string ufPagador { get; set; }
        public string cdIndCpfcnpjPagador { get; set; }
        public string nuCpfcnpjPagador { get; set; }
        public string endEletronicoPagador { get; set; }
        public string nomeSacadorAvalista { get; set; }
        public string logradouroSacadorAvalista { get; set; }
        public string nuLogradouroSacadorAvalista { get; set; }
        public string complementoLogradouroSacadorAvalista { get; set; }
        public string cepSacadorAvalista { get; set; }
        public string complementoCepSacadorAvalista { get; set; }
        public string bairroSacadorAvalista { get; set; }
        public string municipioSacadorAvalista { get; set; }
        public string ufSacadorAvalista { get; set; }
        public string cdIndCpfcnpjSacadorAvalista { get; set; }
        public string nuCpfcnpjSacadorAvalista { get; set; }
        public string endEletronicoSacadorAvalista { get; set; }
    }

    public class RegistroRetorno
    {
        public string cdErro { get; set; }
        public string msgErro { get; set; }
        public string idProduto { get; set; }
        public string negociacao { get; set; }
        public string clubBanco { get; set; }
        public string tpContrato { get; set; }
        public string nuSequenciaContrato { get; set; }
        public string cdProduto { get; set; }
        public string nuTituloGerado { get; set; }
        public string agenciaCreditoBeneficiario { get; set; }
        public string contaCreditoBeneficiario { get; set; }
        public string digCreditoBeneficiario { get; set; }
        public string cdCipTitulo { get; set; }
        public string statusTitulo { get; set; }
        public string descStatusTitulo { get; set; }
        public string nomeBeneficiario { get; set; }
        public string logradouroBeneficiario { get; set; }
        public string nuLogradouroBeneficiario { get; set; }
        public string complementoLogradouroBeneficiario { get; set; }
        public string bairroBeneficiario { get; set; }
        public string cepBeneficiario { get; set; }
        public string cepComplementoBeneficiario { get; set; }
        public string municipioBeneficiario { get; set; }
        public string ufBeneficiario { get; set; }
        public string razaoContaBeneficiario { get; set; }
        public string nomePagador { get; set; }
        public string cpfcnpjPagador { get; set; }
        public string enderecoPagador { get; set; }
        public string bairroPagador { get; set; }
        public string municipioPagador { get; set; }
        public string ufPagador { get; set; }
        public string cepPagador { get; set; }
        public string cepComplementoPagador { get; set; }
        public string endEletronicoPagador { get; set; }
        public string nomeSacadorAvalista { get; set; }
        public string cpfcnpjSacadorAvalista { get; set; }
        public string enderecoSacadorAvalista { get; set; }
        public string municipioSacadorAvalista { get; set; }
        public string ufSacadorAvalista { get; set; }
        public string cepSacadorAvalista { get; set; }
        public string cepComplementoSacadorAvalista { get; set; }
        public string numeroTitulo { get; set; }
        public string dtRegistro { get; set; }
        public string especieDocumentoTitulo { get; set; }
        public string descEspecie { get; set; }
        public string vlIOF { get; set; }
        public string dtEmissao { get; set; }
        public string dtVencimento { get; set; }
        public string vlTitulo { get; set; }
        public string vlAbatimento { get; set; }
        public string dtInstrucaoProtestoNegativacao { get; set; }
        public string diasInstrucaoProtestoNegativacao { get; set; }
        public string dtMulta { get; set; }
        public string vlMulta { get; set; }
        public string qtdeCasasDecimaisMulta { get; set; }
        public string cdValorMulta { get; set; }
        public string descCdMulta { get; set; }
        public string dtJuros { get; set; }
        public string vlJurosAoDia { get; set; }
        public string dtDesconto1Bonificacao { get; set; }
        public string vlDesconto1Bonificacao { get; set; }
        public string qtdeCasasDecimaisDesconto1Bonificacao { get; set; }
        public string cdValorDesconto1Bonificacao { get; set; }
        public string descCdDesconto1Bonificacao { get; set; }
        public string dtDesconto2 { get; set; }
        public string vlDesconto2 { get; set; }
        public string qtdeCasasDecimaisDesconto2 { get; set; }
        public string cdValorDesconto2 { get; set; }
        public string descCdDesconto2 { get; set; }
        public string dtDesconto3 { get; set; }
        public string vlDesconto3 { get; set; }
        public string qtdeCasasDecimaisDesconto3 { get; set; }
        public string cdValorDesconto3 { get; set; }
        public string descCdDesconto3 { get; set; }
        public string diasDispensaMulta { get; set; }
        public string diasDispensaJuros { get; set; }
        public string cdBarras { get; set; }
        public string linhaDigitavel { get; set; }
        public string cdAcessorioEscrituralEmpresa { get; set; }
        public string tpVencimento { get; set; }
        public string indInstrucaoProtesto { get; set; }
        public string tipoAbatimentoTitulo { get; set; }
        public string cdValorJuros { get; set; }
        public string tpDesconto1 { get; set; }
        public string tpDesconto2 { get; set; }
        public string tpDesconto3 { get; set; }
        public string nuControleParticipante { get; set; }
        public string diasJuros { get; set; }
        public string cdJuros { get; set; }
        public string vlJuros { get; set; }
        public string cpfcnpjBeneficiario { get; set; }
        public string vlTituloEmitidoBoleto { get; set; }
        public string dtVencimentoBoleto { get; set; }
        public string indTituloPertenceBaseTitulos { get; set; }
        public string dtLimitePagamentoBoleto { get; set; }
        public string cdIdentificacaoTituloDDACIP { get; set; }
        public string indPagamentoParcial { get; set; }
        public string qtdePagamentoParciais { get; set; }
    }

    public class BoletoService
    {
        public static string Gerar(Entities.Boleto boleto, string sistema)
        {
            var moedaPadrao = Core.Helpers.ConfiguracaoHelper.GetMoedaPadrao();

            var agencia = ConfiguracaoHelper.GetString("BOLETO_AGENCIA");
            var digitoAgencia = ConfiguracaoHelper.GetString("BOLETO_AGENCIA_DIGITO");
            var conta = ConfiguracaoHelper.GetString("BOLETO_CONTA");
            var digitoConta = ConfiguracaoHelper.GetString("BOLETO_CONTA_DIGITO");

            BoletoNet.ContaBancaria contaBancaria;
            if (string.IsNullOrWhiteSpace(digitoConta))
            {
                contaBancaria = new BoletoNet.ContaBancaria(agencia, conta);
            }
            else
            {
                contaBancaria = new BoletoNet.ContaBancaria(agencia, digitoAgencia, conta, digitoConta);
            }

            var codigoBanco = ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO");

            BoletoNet.Cedente cedente = new BoletoNet.Cedente(contaBancaria);
            cedente.Nome = ConfiguracaoHelper.GetString("BOLETO_NOME_CEDENTE");
            cedente.CPFCNPJ = ConfiguracaoHelper.GetString("BOLETO_CPF_CNPJ_CEDENTE");
            cedente.Convenio = ConfiguracaoHelper.GetInt("BOLETO_CONVENIO");
            cedente.Codigo = ConfiguracaoHelper.GetString("BOLETO_CONVENIO");

            var especie = ConfiguracaoHelper.GetString("BOLETO_ESPECIE");

            BoletoNet.EspecieDocumento especieDocumento = new BoletoNet.EspecieDocumento(ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO"), especie);
            BoletoNet.Banco banco = new BoletoNet.Banco(codigoBanco);

            var instrucoes = "";

            if (!string.IsNullOrWhiteSpace(ConfiguracaoHelper.GetString("BOLETO_INSTRUCOES")))
            {
                instrucoes = ConfiguracaoHelper.GetString("BOLETO_INSTRUCOES");
            }
            float taxaGeracao = 0;

            instrucoes += ";login:" + boleto.PedidoPagamento.Pedido.Usuario.Login;
            instrucoes += ";Pedido:" + boleto.PedidoPagamento.Pedido.Codigo;
            if (ConfiguracaoHelper.TemChave("BOLETO_TAXA") && ConfiguracaoHelper.GetInt("BOLETO_TAXA") > 0)
            {
                taxaGeracao = ConfiguracaoHelper.GetInt("BOLETO_TAXA") / 100;
                instrucoes += ";Taxa de geração do boleto: " + moedaPadrao.Simbolo + " " + taxaGeracao.ToString("0.00");

            }

            BoletoNet.Boleto boletoNet = new BoletoNet.Boleto(boleto.DataVencimento, (decimal)boleto.ValorTotal + (decimal)taxaGeracao, ConfiguracaoHelper.GetString("BOLETO_CARTEIRA"), boleto.NossoNumero, cedente, especieDocumento);
            if (!string.IsNullOrWhiteSpace(instrucoes))
            {
                foreach (var i in instrucoes.Split(';'))
                {
                    if (ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO") == 341)
                    {
                        BoletoNet.Instrucao_Itau itemInstrucao = new BoletoNet.Instrucao_Itau();
                        itemInstrucao.Descricao = i;
                        boletoNet.Instrucoes.Add(itemInstrucao);
                    }
                    else if (ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO") == 237)
                    {
                        BoletoNet.Instrucao_Bradesco itemInstrucao = new BoletoNet.Instrucao_Bradesco();
                        itemInstrucao.Descricao = i;
                        boletoNet.Instrucoes.Add(itemInstrucao);
                    }
                    else if (ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO") == 33)
                    {
                        BoletoNet.Instrucao_Santander itemInstrucao = new BoletoNet.Instrucao_Santander();
                        itemInstrucao.Descricao = i;
                        boletoNet.Instrucoes.Add(itemInstrucao);
                    }
                }
            }

            boletoNet.Banco = banco;
            boletoNet.NumeroDocumento = boleto.NumeroDocumento.ToString();

            boletoNet.Sacado = new BoletoNet.Sacado(boleto.PedidoPagamento.Usuario.Nome);
            boletoNet.Sacado.Endereco.End = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.EnderecoCompleto;

            boletoNet.Sacado.Endereco.Logradouro = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Logradouro;
            boletoNet.Sacado.Endereco.Numero = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Numero;
            boletoNet.Sacado.Endereco.Complemento = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Complemento;

            boletoNet.Sacado.Endereco.Bairro = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Distrito;
            boletoNet.Sacado.Endereco.Cidade = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Cidade.Nome;
            boletoNet.Sacado.Endereco.CEP = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.CodigoPostal;
            boletoNet.Sacado.Endereco.UF = boleto.PedidoPagamento.Usuario.EnderecoPrincipal.Estado.Sigla;
            boletoNet.Sacado.CPFCNPJ = boleto.PedidoPagamento.Usuario.Documento;

            BoletoNet.BoletoBancario boletoBancario = new BoletoNet.BoletoBancario();
            boletoBancario.CodigoBanco = (short)banco.Codigo;
            boletoBancario.Boleto = boletoNet;
            boletoBancario.MostrarCodigoCarteira = false;
            boletoBancario.MostrarComprovanteEntrega = false;
            boletoBancario.FormatoCarne = false;

            boletoBancario.Boleto.Valida();

            string ret = Registrar(boletoBancario.Boleto, sistema).ToString();
            if (ret == "ok")
            {
                var strUrl = ConfiguracaoHelper.GetString("PASTA_BOLETOS") + "/";
                var BoletoHtml = boletoBancario.MontaHtml(strUrl, boleto.ID.ToString(), true);
                var strSubPasta = ConfiguracaoHelper.GetString("SUBPASTA_OFFICE");
                if (strSubPasta != null && strSubPasta.Trim().Length > 0)
                {
                    BoletoHtml = BoletoHtml.Replace(strUrl, ConfiguracaoHelper.GetString("SUBPASTA_OFFICE") + strUrl);
                }

                return BoletoHtml;
            }
            else
            {
                return ret;
            }
        }

        static string Registrar(BoletoNet.Boleto boletoNet, string sistema)
        {
            if (!ConfiguracaoHelper.GetBoolean("BOLETO_REGISTRO_ATIVO"))
                return "ok";

            string strUrl = ConfiguracaoHelper.GetString("BOLETO_URL_REGISTRO");
            if (string.IsNullOrEmpty(strUrl))
                return "URL para registro do boleto não parametrizada.";

            #region Cria Registro de Entrada 

            RegistroEntrada entrada = new RegistroEntrada();

            if (boletoNet.Cedente.CPFCNPJ.Length == 11) // === CPF
            {
                string documento = Int64.Parse(boletoNet.Cedente.CPFCNPJ).ToString("D11");
                entrada.nuCPFCNPJ = documento.Substring(0, 9);
                entrada.filialCPFCNPJ = "0";
                entrada.ctrlCPFCNPJ = documento.Substring(9, 2);
            }
            else
            {
                string documento = Int64.Parse(boletoNet.Cedente.CPFCNPJ).ToString("D14");
                entrada.nuCPFCNPJ = documento.Substring(0, 8);
                entrada.filialCPFCNPJ = documento.Substring(8, 4);
                entrada.ctrlCPFCNPJ = documento.Substring(12, 2);
            }
            entrada.cdTipoAcesso = "2";
            entrada.clubBanco = "2269651";
            entrada.cdTipoContrato = "48";
            entrada.nuSequenciaContrato = "0";
            entrada.idProduto = ConfiguracaoHelper.GetInt("BOLETO_CARTEIRA").ToString("D2");
            entrada.nuNegociacao = ConfiguracaoHelper.GetInt("BOLETO_AGENCIA").ToString("D4") + ConfiguracaoHelper.GetInt("BOLETO_CONTA").ToString("D14");
            entrada.cdBanco = ConfiguracaoHelper.GetInt("BOLETO_CODIGO_BANCO").ToString("D3");
            entrada.eNuSequenciaContrato = "0";
            entrada.tpRegistro = "1";
            entrada.cdProduto = "0";
            entrada.nuTitulo = boletoNet.NossoNumero.Substring(3, 11);
            entrada.nuCliente = ConfiguracaoHelper.GetString("BOLETO_CONVENIO");
            entrada.dtEmissaoTitulo = boletoNet.DataDocumento.ToString("dd.MM.yyyy");
            entrada.dtVencimentoTitulo = boletoNet.DataVencimento.ToString("dd.MM.yyyy");
            entrada.tpVencimento = "0";
            entrada.vlNominalTitulo = (boletoNet.ValorBoleto * 100).ToString().Replace(".", "");
            entrada.cdEspecieTitulo = ConfiguracaoHelper.GetInt("BOLETO_ESPECIE_CODIGO").ToString("D2");
            entrada.tpProtestoAutomaticoNegativacao = "0";
            entrada.prazoProtestoAutomaticoNegativacao = "0";
            entrada.controleParticipante = "";
            entrada.cdPagamentoParcial = "N";
            entrada.qtdePagamentoParcial = "0";
            entrada.percentualJuros = "0";
            entrada.vlJuros = "0";
            entrada.qtdeDiasJuros = "0";
            entrada.percentualMulta = "0";
            entrada.vlMulta = "0";
            entrada.qtdeDiasMulta = "0";
            entrada.percentualDesconto1 = "0";
            entrada.vlDesconto1 = "0";
            entrada.dataLimiteDesconto1 = "";
            entrada.percentualDesconto2 = "0";
            entrada.vlDesconto2 = "0";
            entrada.dataLimiteDesconto2 = "";
            entrada.percentualDesconto3 = "0";
            entrada.vlDesconto3 = "0";
            entrada.dataLimiteDesconto3 = "";
            entrada.prazoBonificacao = "0";
            entrada.percentualBonificacao = "0";
            entrada.vlBonificacao = "0";
            entrada.dtLimiteBonificacao = "";
            entrada.vlAbatimento = "0";
            entrada.vlIOF = "0";

            entrada.nomePagador = boletoNet.Sacado.Nome;
            entrada.logradouroPagador = boletoNet.Sacado.Endereco.Logradouro;
            entrada.nuLogradouroPagador = boletoNet.Sacado.Endereco.Numero;
            entrada.complementoLogradouroPagador = boletoNet.Sacado.Endereco.Complemento;
            if (boletoNet.Sacado.Endereco.CEP.Length >= 5)
            {
                entrada.cepPagador = boletoNet.Sacado.Endereco.CEP.Substring(0, 5);
            }
            else
            {
                entrada.cepPagador = "00000";
            }

            if (boletoNet.Sacado.Endereco.CEP.Length >= 8)
            {
                entrada.complementoCepPagador = boletoNet.Sacado.Endereco.CEP.Substring(5, 3);
            }
            else
            {
                entrada.complementoCepPagador = "000";
            }

            entrada.bairroPagador = boletoNet.Sacado.Endereco.Bairro;
            entrada.municipioPagador = boletoNet.Sacado.Endereco.Cidade;
            entrada.ufPagador = boletoNet.Sacado.Endereco.UF;
            entrada.cdIndCpfcnpjPagador = boletoNet.Sacado.CPFCNPJ.Length == 11 ? "1" : "2";
            entrada.nuCpfcnpjPagador = boletoNet.Sacado.CPFCNPJ;
            entrada.endEletronicoPagador = "";

            entrada.nomeSacadorAvalista = "";
            entrada.logradouroSacadorAvalista = "";
            entrada.nuLogradouroSacadorAvalista = "0";
            entrada.complementoLogradouroSacadorAvalista = "";
            entrada.cepSacadorAvalista = "0";
            entrada.complementoCepSacadorAvalista = "0";
            entrada.bairroSacadorAvalista = "";
            entrada.municipioSacadorAvalista = "";
            entrada.ufSacadorAvalista = "";
            entrada.cdIndCpfcnpjSacadorAvalista = "0";
            entrada.nuCpfcnpjSacadorAvalista = "0";
            entrada.endEletronicoSacadorAvalista = "";

            #endregion

            try
            {
                string strEntrada = JsonConvert.SerializeObject(entrada);

                // == Assinatura Digital  
                var entradaSign = SignDataPKCS_7CMS(strEntrada, sistema);

                strEntrada = Convert.ToBase64String(entradaSign);

                var req = new HttpRequestMessage(HttpMethod.Post, strUrl);
                req.Content = new StringContent(strEntrada, Encoding.UTF8, "application/json");

                HttpClient c = new HttpClient();
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var resp = c.SendAsync(req).Result;

                if (resp == null || resp.StatusCode != System.Net.HttpStatusCode.OK && resp.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    return resp.StatusCode.ToString();
                }

                var xmlRetorno = XDocument.Parse(resp.Content.ReadAsStringAsync().Result);
                RegistroRetorno registroRetorno = JsonConvert.DeserializeObject<RegistroRetorno>(xmlRetorno.Root.Value);
                if (registroRetorno.cdErro == "0" || registroRetorno.cdErro == "69") //  0=Ok 69=Titulo ja Cadastrado
                {
                    return "ok";
                }
                else
                {
                    return registroRetorno.msgErro;
                }
            }
            catch (Exception e)
            {
                return "Error ocurred in HttpWebRequest: " + e.Message;
            }
        }

        private static byte[] SignDataPKCS_7CMS(string data, string certFriendlyName)
        {
            X509Certificate2 cert = GetCertificateByFriendlyName(certFriendlyName);
            ContentInfo content = new ContentInfo(new UTF8Encoding().GetBytes(data));
            SignedCms scms = new SignedCms(content, false);
            CmsSigner signer = new CmsSigner(cert);
            signer.IncludeOption = X509IncludeOption.EndCertOnly;
            scms.ComputeSignature(signer);

            return scms.Encode();
        }

        private static X509Certificate2 GetCertificateByFriendlyName(string certFriendlyName)
        {
            // Access Personal (MY) certificate store of current user
            X509Store my = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            my.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in my.Certificates)
            {
                if (cert.FriendlyName.Contains(certFriendlyName))
                {
                    return cert;
                }
            }
            my.Close();

            throw new Exception("No valid cert was found");
        }

    }

}


