using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Lancamento : IPersistentEntity
    {

        public enum Tipos
        {
            [Description("Crédito")]
            Credito = 1,
            [Description("Débito")]
            Debito = 2,
            [Description("Ativação")]
            Ativacao = 3,
            [Description("Compra")]
            Compra = 4,
            [Description("Transferência")]
            Transferencia = 5,
            [Description("Bonificação")]
            Bonificacao = 6,
            [Description("Saque")]
            Saque = 7,
            [Description("Bonificação Ponto")]
            BonificacaoPonto = 8,
            [Description("Taxa")]
            Taxa = 9,
            [Description("Nenhum")]
            Nenhum = 10,
            [Description("Investimento")]
            Investimento = 11,
            [Description("Indefinido")]
            Indefinido = 12
        }

        public Tipos Tipo
        {
            get { return (Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

    }
}
