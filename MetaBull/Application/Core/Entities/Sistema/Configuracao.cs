using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
   public partial class Configuracao : IPersistentEntity
   {

      public enum Categorias
      {
         URLs = 1,
         Imagem = 2,
         Site = 3,
         Admin = 4,
         File = 5,
         Desing = 6,
         Codigo = 7,
         Email = 8,
         Texto = 9
      }

      public enum Tipos
      {
         String = 1,
         Boolean = 2,
         Int = 3
      }

      public Categorias Categoria
      {
         get { return (Categorias)this.CategoriaID; }
         set { this.CategoriaID = (int)value; }
      }

      public Tipos Tipo
      {
         get { return (Tipos)this.TipoID; }
         set { this.TipoID = (int)value; }
      }

   }
}
