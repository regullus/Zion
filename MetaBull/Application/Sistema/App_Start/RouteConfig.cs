namespace Sistema
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.AppendTrailingSlash = true;
            routes.LowercaseUrls = true;
            routes.MapMvcAttributeRoutes();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Patrocinador",
                url: "p/{login}",
                defaults: new { controller = "Cadastro", action = "BuscarPatrocinador" },
                namespaces: new string[] { "Sistema.Controllers" }
            ).RouteHandler = new HyphenatedRouteHandler();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Sistema.Controllers" }
            ).RouteHandler = new HyphenatedRouteHandler();
        }

        public class HyphenatedRouteHandler : MvcRouteHandler
        {
            protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                requestContext.RouteData.Values["controller"] = requestContext.RouteData.Values["controller"].ToString().Replace("-", "");
                requestContext.RouteData.Values["action"] = requestContext.RouteData.Values["action"].ToString().Replace("-", "");
                return base.GetHttpHandler(requestContext);
            }
        }
    }
}
