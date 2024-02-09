using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Moeda : IPersistentEntity
    {
        public enum Moedas
        {
            [Description("Real")]
            BRL = 1,
            [Description("Pontos")]
            PTS = 2,
            [Description("Dolar")]
            USD = 3,
            [Description("Nenhuma")]
            NEN = 4,
            [Description("LiteCoin")]
            LTC = 5,
            [Description("Bitcoin")]
            BTC = 6,
            [Description("USDT (TRC20)")]
            USDT = 7
        }
    }
}
