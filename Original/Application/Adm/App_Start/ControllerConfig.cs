using Sistema.Factories;
using System.Web;
using System.Web.Mvc;

namespace Sistema
{
    public class ControllerConfig
    {
        public static void RegisterBuilders(ControllerBuilder controllerBuilder)
        {
            controllerBuilder.SetControllerFactory(new NinjectControllerFactory());
        }
    }
}