using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Sistema.ModelBinders
{
   public class UsuarioBinder
   {
      public string Foto
      {
         get
         {
            var caminhoVirtual = "arquivos/perfil/" + _usuario.ID.ToString("D6") + ".jpg";

            string caminhoFisico = Core.Helpers.ConfiguracaoHelper.GetString("CAMINHO_FISICO") + @"arquivos\perfil\" + _usuario.ID.ToString("D6") + ".jpg";

            if (File.Exists(caminhoFisico))
            {
               return "~/" + caminhoVirtual;
            }
            return null;
         }
      }

      private Core.Entities.Usuario _usuario;
      public Core.Entities.Usuario Usuario
      {
         get
         {
            return _usuario;
         }
         set
         {
            _usuario = value;
         }
      }
   }
}