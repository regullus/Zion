using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Containers
{
   public struct UsuarioContainer
   {
      private Core.Entities.Usuario _usuario;

      public UsuarioContainer(Core.Entities.Usuario u)
      {
         this._usuario = u;
      }

      public Core.Entities.Usuario Usuario
      {
         get
         {
            return _usuario;
         }
      }

      public double Saldo
      {
         get
         {
            var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == 1);
            if (lancamentosSaldo != null)
            {
               return (double)lancamentosSaldo.Sum(l => l.Valor);
            }
            return 0;
         }
      }

      public double Pontos
      {
         get
         {
            var lancamentosSaldo = this._usuario.Lancamento.Where(l => l.ContaID == 2);
            if (lancamentosSaldo != null)
            {
               return (double)lancamentosSaldo.Sum(l => l.Valor);
            }
            return 0;
         }
      }

      public decimal PontoPosicao
      {
         get
         {
            
            return 0;
         }
      }

   }
}