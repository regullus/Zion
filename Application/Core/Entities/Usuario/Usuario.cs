using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Usuario : IPersistentEntity
    {

        public enum Tipos
        {
            Indefinido = 0
        }

        public enum Derramamentos
        {
            Indefinido = 0,
            Esquerda = 1,
            Direita = 2,
            [Description("Automático")]
            Automatico = 3,
            Manual = 4,
            //Linha = 9,
            Coluna0 = 10,
            Coluna1 = 11,
            Coluna2 = 12,
            Coluna3 = 13,
            Coluna4 = 14,
            Coluna5 = 15,
            Coluna6 = 16,
            Coluna7 = 17,
            Coluna8 = 18,
            Coluna9 = 19
        }

        public enum TodosStatus
        {
            Indefinido = 0,
            [Description("Não Associado")]
            NaoAssociado = 1,
            Associado = 2,
            Bloqueado = 3
        }

        public enum TodosStatusCelular
        {
            Indefinido = 0,
            [Description("Não Validado")]
            NaoValidado = 1,
            Validado = 2
        }

        public enum TodosStatusEmail
        {
            Indefinido = 0,
            [Description("Não Validado")]
            NaoValidado = 1,
            Validado = 2
        }

        public enum TodosTiposAtivacao
        {
            Dinheiro = 0,
            Pontos = 1
        }

        public Tipos Tipo
        {
            get { return (Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

        public Derramamentos? Entrada
        {
            get { return this.EntradaID.HasValue ? (Derramamentos?)this.EntradaID.Value : null; }
            set { this.EntradaID = value.HasValue ? (int?)value.Value : null; }
        }

        public Derramamentos Derramamento
        {
            get { return (Derramamentos)this.DerramamentoID; }
            set { this.DerramamentoID = (int)value; }
        }

        public TodosTiposAtivacao TipoDeAtivacao
        {
            get { return (TodosTiposAtivacao)this.TipoAtivacao; }
            set { this.TipoAtivacao = (int)value; }
        }


        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

        public TodosStatusCelular StatusCelular
        {
            get { return (TodosStatusCelular)this.StatusCelularID; }
            set { this.StatusCelularID = (int)value; }
        }

        public TodosStatusEmail StatusEmail
        {
            get { return (TodosStatusEmail)this.StatusEmailID; }
            set { this.StatusEmailID = (int)value; }
        }

        public Posicao UltimaPosicao
        {
            get { return this.Posicao.OrderByDescending(p => p.DataFim).FirstOrDefault(); }
        }

        public string Foto
        {
            //TODO: Capturar foto da pasta
            get { return ""; }
        }

        public Endereco EnderecoPrincipal
        {
            get
            {
                Endereco enderecoPrincipal = this.Enderecos.FirstOrDefault(e => e.Principal);
                if (enderecoPrincipal == null)
                {
                    //Caso não haja o endereço principal seta o endereço como vazio
                    enderecoPrincipal = new Endereco();
                    enderecoPrincipal.ID = 0;
                    enderecoPrincipal.CidadeID = 0;
                    enderecoPrincipal.EstadoID = 0;
                    enderecoPrincipal.Complemento = "";
                    enderecoPrincipal.Logradouro = "";
                    enderecoPrincipal.Nome = "";
                    enderecoPrincipal.Numero = "";

                }
                return enderecoPrincipal;
            }
        }

        public ContaDeposito ContaDepositoPrincipal
        {
            get { return this.ContaDeposito.FirstOrDefault(); }
        }

        public Endereco EnderecoAlternativo
        {
            get
            {
                Endereco enderecoAlternativo = this.Enderecos.FirstOrDefault(e => e.Principal == false);
                if (enderecoAlternativo == null)
                {
                    //Caso não haja o endereço alternativo seta o endereço como vazio
                    enderecoAlternativo = new Endereco();
                    enderecoAlternativo.ID = 0;
                    enderecoAlternativo.CidadeID = 0;
                    enderecoAlternativo.EstadoID = 0;
                    enderecoAlternativo.Complemento = "";
                    enderecoAlternativo.Logradouro = "";
                    enderecoAlternativo.Nome = "";
                    enderecoAlternativo.Numero = "";

                }
                return enderecoAlternativo;
            }
        }

        public bool DocumentoAprovado
        {
            get { return this.Documentos.Count(d => d.Validado) > 0; }
        }

        public DateTime Validade
        {
            get { return DataValidade.HasValue ? DataValidade.Value : DateTime.MinValue; }
            set { DataValidade = value; }
        }      
    }

    public class UsuarioDireto
    {
        public int UserID { get; set; }
        public int NivelAssociacao { get; set; }
        public int Status { get; set; }
        public int Nivel { get; set; }
    }
}
