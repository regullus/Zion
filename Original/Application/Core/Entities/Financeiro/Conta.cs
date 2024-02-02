using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Conta : IPersistentEntity
    {
        public enum Contas
        {
            [Description("Rentabilidade")]
            Rentabilidade = 1,
            [Description("Bonus")]
            Bonus = 2,
            [Description("Transferencias")]
            Transferencias = 7,
            [Description("Investimento")]
            Investimento = 8,
            [Description("BitCoin")]
            BitCoin = 9,
            [Description("LiteCoin")]
            LiteCoin = 10,
            [Description("Saldo")]
            Saldo = 14
        }
     
    }
}
