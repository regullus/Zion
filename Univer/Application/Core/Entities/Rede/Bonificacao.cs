using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Bonificacao : IPersistentEntity
    {

        public enum TodosStatus
        {
            Pendente = 0,
            [Description("Em Processamento")]
            EmProcessamento = 1,
            Processado = 2
        }

        public TodosStatus Status
        {
            get { return (TodosStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

    }

    public class RelatorioBonificacaoGlobalAssociacao
    {
        public int NivelAssociacao { get; set; }
        public int Total { get; set; }
    }
}

