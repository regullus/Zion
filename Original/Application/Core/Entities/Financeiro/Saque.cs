using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Saque : IPersistentEntity
    {

        public SaqueStatus StatusInicial
        {
            get
            {
                var status = this.SaqueStatus.OrderBy(s => s.Data).FirstOrDefault();
                return status != null ? status : new SaqueStatus();
            }
        }

        public SaqueStatus StatusAtual
        {
            get
            {
                var status = this.SaqueStatus.OrderByDescending(s => s.Data).FirstOrDefault();
                return status != null ? status : new SaqueStatus();
            }
        }

    }
}
