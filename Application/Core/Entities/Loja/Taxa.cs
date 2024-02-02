using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public partial class Taxa : BaseEntity, IPersistentEntity
    {

        public enum Tipos
        {
            Percentual = 1,
            Fixo = 2
        }

        public Tipos Tipo
        {
            get { return (Tipos)this.TipoID; }
            set { this.TipoID = (int)value; }
        }

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

        public IEnumerable<int> PaisID
        {
            get { return this.StringToIntList(this.PaisIDs); }
        }

        public IEnumerable<int> TipoProdutoID
        {
            get { return this.StringToIntList(this.TipoProdutoIDs); }
        }

    }
}
