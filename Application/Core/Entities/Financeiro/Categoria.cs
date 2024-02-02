using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Categoria : IPersistentEntity
    {

        public enum Categorias
        {
            [Description("Debito Manual")]
            DebitoManual = 2,
            [Description("Descontos")]
            Descontos = 4,
            [Description("Frete")]
            Frete = 5,
            [Description("Pedido")]
            Pedido = 6,
            [Description("Transferência")]
            Transferencia = 7,
            [Description("Crédito Manual")]
            CreditoManual = 8,
            [Description("Saque")]
            Saque = 9,
            [Description("Débito")]
            Debito = 10,
            [Description("Bônus Rendimento Diário")]
            BonusRendimentoDiario = 11,
            [Description("Bônus de Equipe")]
            BonusDeEquipe = 12,
            [Description("Bônus de Resultado Mensal")]
            BonusDeResultadoMensal = 13,
            [Description("Taxa de Transferência")]
            TaxaDeTransferencia = 18,
            [Description("Taxa de Ativação")]
            TaxaDeAtivacao = 19,
            [Description("Investimento")]
            Investimento = 20
        }

        public Lancamento.Tipos Tipo
        {
            get { return (Lancamento.Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

    }
}
