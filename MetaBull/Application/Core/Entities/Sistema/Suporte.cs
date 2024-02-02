using DomainExtension.Entities.Interfaces;

namespace Core.Entities
{
    public partial class Suporte : IPersistentEntity
    {

        public void ObtemImagens()
        {
            foreach (var item in this.SuporteMensagem)
            {
                item.ObtemImagens();
            }

        }

    }
}
