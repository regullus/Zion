using Core.Modules;
using MvcExtension.Infrastructure;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema.Factories
{
    public class NinjectControllerFactory : DefaultNinjectControllerFactory
    {
        public NinjectControllerFactory()
            : base(new StandardKernel(new NinjectModule()))
        {
        }

        public override void OnLoad()
        {
            //kernel.Bind<HomeController>().ToSelf();
        }
    }
}