using Core.Entities;
using Core.Models.Loja;
using Core.Repositories.Loja;
using Core.Repositories.Usuario;
using MvcExtension.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema.ModelBinders
{
    public class CarrinhoBinder : IModelBinder
    {
        private const string SESSION_KEY = "_carrinho";
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.Model != null)
                throw new InvalidOperationException("Cannot update instances");

            var carrinho = (CarrinhoModel)controllerContext.HttpContext.Session[SESSION_KEY];

            if (carrinho == null)
            {
                Usuario usuario = null;
                List<Taxa> taxas = new List<Taxa>();
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var context = new YLEVELEntities();
                    var usuarioRepository = new UsuarioRepository(context);
                    var taxaRepository = new TaxaRepository(context);
                    usuario = usuarioRepository.Get(Helpers.Local.idUsuario);
                    taxas = taxaRepository.GetByUsuario(usuario).ToList();
                }
                carrinho = new CarrinhoModel(usuario);
                taxas.ForEach(t => carrinho.Taxas.Add(new CarrinhoTaxaModel()
                {
                    Taxa = t,
                    Valor = 0
                }));
                controllerContext.HttpContext.Session[SESSION_KEY] = carrinho;
                controllerContext.HttpContext.Session.Timeout = 60;
            }

            return carrinho;
        }
    }
}