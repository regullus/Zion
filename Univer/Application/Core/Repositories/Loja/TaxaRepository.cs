using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Loja
{
    public class TaxaRepository : CachedRepository<Entities.Taxa>
    {

        public TaxaRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Taxa> GetByUsuario(Entities.Usuario usuario)
        {
            var taxas = this.GetByExpression(t =>
                (!t.AssociacaoID.Any() || t.AssociacaoID.Contains(usuario.NivelAssociacao)) &&
                (!t.BlocoID.Any() || t.BlocoID.Contains(usuario.Pais.BlocoID)) &&
                (!t.ClassificacaoID.Any() || t.ClassificacaoID.Contains(usuario.NivelClassificacao)) &&
                (!t.PaisID.Any() || t.PaisID.Contains(usuario.PaisID))
            );
            return taxas;
        }

        public IEnumerable<Entities.Taxa> GetByCategoria(int idCategoria)
        {
            var taxas = this.GetByExpression(t => t.CategoriaID == idCategoria);
            return taxas;
        }


    }
}
