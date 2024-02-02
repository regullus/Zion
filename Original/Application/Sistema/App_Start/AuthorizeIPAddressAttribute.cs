using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Sistema
{
    public class AuthorizeIPAddressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                string ipAddress = HttpContext.Current.Request.UserHostAddress;
                if (!IsIpAddressAllowed(ipAddress.Trim()))
                {
                    context.Result = new HttpStatusCodeResult(403);
                }
                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                context.Result = new HttpStatusCodeResult(200, ex.Message);
            }

        }
        private bool IsIpAddressAllowed(string IpAddress)
        {
            if (!string.IsNullOrWhiteSpace(IpAddress))
            {
                string[] addresses = Convert.ToString(WebConfigurationManager.AppSettings["AllowedIPAddresses"]).Split(',');
                return addresses.Where(a => a.Trim().Equals(IpAddress, StringComparison.InvariantCultureIgnoreCase)).Any();
            }
            return false;
        }
    }
}