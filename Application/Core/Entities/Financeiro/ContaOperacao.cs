using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class ContaOperacao : IPersistentEntity
    {

        public enum Operacoes
        {
            Indefinido = 0,
            Compra = 1,
            Saque = 2,
            [Description("Transferência")]
            Transferencia = 3
        }

        public Operacoes Operacao
        {
            get { return (Operacoes)this.OperacaoID; }
            set { this.OperacaoID = (int)value; }
        }

    }
}
