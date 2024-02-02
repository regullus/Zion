using DomainExtension.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
   public partial class AssociacaoLimiteGanho : IPersistentEntity
   {
      public enum Tipos
      {
         [Description("Associação")]
         Associacao = 1,
         [Description("Anual")]
         Anual = 2,
         [Description("Diario")]
         Diario = 3,
         [Description("Semanal")]
         Semanal = 4,
         [Description("Quinzenal")]
         Quinzenal = 5,
         [Description("Mensal")]
         Mensal = 6,
         [Description("Bimestral")]
         Bimestral = 7,
         [Description("Trimestral")]
         Trimestral = 8,
         [Description("Quadrimestral")]
         Quadrimestral = 9,
         [Description("Quimestral")]
         Quimestral = 10,
         [Description("Semestral")]
         Semestral = 11
      }

      public Tipos Tipo
      {
         get { return (Tipos)this.TipoID; }
         set { this.TipoID = (int)value; }
      }
   }
}

