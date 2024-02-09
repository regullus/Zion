using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Classificacao : IPersistentEntity
    {
        public enum Tipo
        {
            [Description("Nao Ativo")]
            NaoAtivo = 0,
            [Description("Broker")]
            Broker = 50,
            [Description("Diretor Bronze")]
            DiretorBronze = 50000,
            [Description("Diretor Silver")]
            DiretorSilver = 200000,
            [Description("Diretor Gold")]
            DiretorGold = 800000,
            [Description("Diretor Platinum")]
            DiretorPlatinum = 3200000,
            [Description("Diretor Diamante")]
            DiretorDiamante = 12800000,
            [Description("Presidente")]
            Presidente = 50000000
        }
    }
}
