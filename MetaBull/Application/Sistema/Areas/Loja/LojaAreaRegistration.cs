using System.Web.Mvc;

namespace Site.Areas.Loja
{
    public class LojaAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Loja";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Loja_Default",
                "loja/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Sistema.Areas.Loja.Controllers" }
            ).RouteHandler = new Sistema.RouteConfig.HyphenatedRouteHandler();
        }

    }
}
