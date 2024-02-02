using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class ProdutoValor : BaseEntity, IPersistentEntity
    {

        public IEnumerable<int> AssociacaoID
        {
            get { return this.StringToIntList(this.AssociacaoIDs); }
        }

        public IEnumerable<int> BlocoID
        {
            get { return this.StringToIntList(this.BlocoIDs); }
        }

        public IEnumerable<int> ClassificacaoID
        {
            get { return this.StringToIntList(this.ClassificacaoIDs); }
        }

        public IEnumerable<int> ContaID
        {
            get { return this.StringToIntList(this.ContaIDs); }
        }

        public IEnumerable<int> PaisID
        {
            get { return this.StringToIntList(this.PaisIDs); }
        }

    }
}
