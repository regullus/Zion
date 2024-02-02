using Core.Builders;
using Core.Services.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Factories
{
    public class UsuarioFactory
    {

        private UsuarioBuilder usuarioBuilder;
        private UsuarioService usuarioService;

        public UsuarioFactory(DbContext context)
        {
            usuarioBuilder = new UsuarioBuilder(context);
            usuarioService = new UsuarioService(context);
        }

        public Entities.Usuario Criar(Entities.Usuario usuario, List<Entities.Endereco> lstEnderecos)
        {
            usuarioBuilder.AdicionarUsuario(usuario);

            if (lstEnderecos != null)
            {
                foreach (var endereco in lstEnderecos)
                    usuarioBuilder.AdicionarEndereco(endereco);
            }

            usuarioBuilder.AdicionarStatus();
            usuarioBuilder.AdicionarPosicao();

            var _usuario = usuarioBuilder.GetUsuario();

            return _usuario;
        }
    }
}
