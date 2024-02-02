using Core.Helpers;
using Core.Repositories.Globalizacao;
using Core.Repositories.Rede;
using Core.Repositories.Usuario;
using Core.Services.Usuario;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Builders
{
    internal class UsuarioBuilder
    {

        private UsuarioRepository usuarioRepository;
        private UsuarioStatusRepository usuarioStatusRepository;
        private EnderecoRepository enderecoRepository;
        private PaisRepository paisRepository;
        private PosicaoRepository posicaoRepository;
        private UsuarioService usuarioService;

        private Entities.Usuario _usuario;
        private Entities.Usuario _patrocinador;
        private Entities.Pais _pais;

        private Helpers.TraducaoHelper _traducaoHelper;

        public UsuarioBuilder(DbContext context)
        {
            usuarioRepository = new UsuarioRepository(context);
            usuarioStatusRepository = new UsuarioStatusRepository(context);
            enderecoRepository = new EnderecoRepository(context);
            paisRepository = new PaisRepository(context);
            posicaoRepository = new PosicaoRepository(context);
            usuarioService = new UsuarioService(context);

        }

        public void AdicionarUsuario(Entities.Usuario usuario)
        {
            //TODO: Revalidar
            _usuario = usuario;

            _usuario.Senha = Helpers.CriptografiaHelper.Criptografar(_usuario.Senha);

            _pais = paisRepository.Get(_usuario.PaisID);
            _patrocinador = usuarioRepository.Get(_usuario.PatrocinadorDiretoID.Value);
            _traducaoHelper = new Helpers.TraducaoHelper(_pais.Idioma);

            _usuario.Assinatura = "";
            _usuario.DataCriacao = App.DateTimeZion;
            _usuario.Derramamento = Entities.Usuario.Derramamentos.Indefinido;  // _patrocinador.Derramamento;
            _usuario.NivelAssociacao = 0;
            _usuario.NivelClassificacao = 0;
            _usuario.ProfundidadeRede = 0;
            _usuario.Tipo = Entities.Usuario.Tipos.Indefinido;
            _usuario.Status = Entities.Usuario.TodosStatus.NaoAssociado;
            _usuario.StatusCelular = Entities.Usuario.TodosStatusCelular.NaoValidado;
            _usuario.StatusEmail = Entities.Usuario.TodosStatusEmail.NaoValidado;
            _usuario.GeraBonus = true;
            _usuario.RecebeBonus = true;

            _usuario.Complemento = new Entities.Complemento()
            {
                IsLideranca = false,
                Login = _usuario.Login
            };

            usuarioRepository.Save(_usuario);
        }

        public void AdicionarEndereco(Entities.Endereco endereco)
        {
            if (endereco.Principal)
            {
                endereco.Nome = _traducaoHelper["PRINCIPAL"];
                endereco.Observacoes = "";
                endereco.Principal = true;
                endereco.UsuarioID = _usuario.ID;
                enderecoRepository.Save(endereco);
            }
            else
            {
                endereco.Nome = _traducaoHelper["ALTERNATIVO"];
                endereco.Principal = false;
                endereco.UsuarioID = _usuario.ID;
                enderecoRepository.Save(endereco);
            }
        }

        public void AdicionarStatus()
        {
            var status = new Entities.UsuarioStatus()
            {
                Data = App.DateTimeZion,
                Status = Entities.Usuario.TodosStatus.NaoAssociado,
                UsuarioID = _usuario.ID
            };
            usuarioStatusRepository.Save(status);
        }

        public void AdicionarPosicao()
        {
            var posicao = new Entities.Posicao()
            {
                AcumuladoDireita = 0,
                AcumuladoEsquerda = 0,
                DataCriacao = App.DateTimeZion,
                DataFim = new DateTime(2014, 1, 1),
                DataInicio = new DateTime(2014, 1, 1),
                ReferenciaID = 0,
                UsuarioID = _usuario.ID,
                ValorPernaDireita = 0,
                ValorPernaEsquerda = 0
            };
            posicaoRepository.Save(posicao);
        }

        public Entities.Usuario GetUsuario()
        {
            return usuarioRepository.Get(_usuario.ID);
        }

    }
}
