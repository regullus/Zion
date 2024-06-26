﻿[assembly: Microsoft.Owin.OwinStartup("ProductionConfiguration", typeof(Sistema.Startup))]
namespace Sistema
{
    using System.Web.Mvc;
    using NWebsec.Owin;
    using Owin;
    using System.Web.Helpers;
    using System.Security.Claims;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Strict-Transport-Security - Adds the Strict-Transport-Security HTTP header to responses.
            //      This HTTP header is only relevant if you are using TLS. It ensures that content is loaded over 
            //      HTTPS and refuses to connect in case of certificate errors and warnings. NWebSec currently does 
            //      not support an MVC filter that can be applied globally. Instead we can use Owin (Using the 
            //      added NWebSec.Owin NuGet package) to apply it.
            //      Note: Including subdomains and a minimum maxage of 18 weeks is required for preloading.
            //      Note: You can view preloaded HSTS domains in Chrome here: chrome://net-internals/#hsts
            //      https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security
            //      http://www.troyhunt.com/2015/06/understanding-http-strict-transport.html
            // app.UseHsts(options => options.MaxAge(days: 18 * 7).IncludeSubdomains().Preload());

            // Public-Key-Pins - Adds the Public-Key-Pins HTTP header to responses.
            //      This HTTP header is only relevant if you are using TLS. It stops man-in-the-middle attacks by 
            //      telling browsers exactly which TLS certificate you expect.
            //      Note: The current specification requires including a second pin for a backup key which isn't yet 
            //      used in production. This allows for changing the server's public key without breaking accessibility 
            //      for clients that have already noted the pins. This is important for example when the former key 
            //      gets compromised. 
            //      Note: You can use the ReportUri option to provide browsers a URL to post JSON violations of the 
            //      HPKP policy. Note that the report URI must not be this site as a violation would mean that the site
            //      is blocked. You must use a separate domain using HTTPS to report to. Consider using this service:
            //      https://report-uri.io/ for this purpose.
            //      Note: You can change UseHpkp to UseHpkpReportOnly to stop browsers blocking anything but continue
            //      reporting any violations.
            //      See https://developer.mozilla.org/en-US/docs/Web/Security/Public_Key_Pinning
            //      and https://scotthelme.co.uk/hpkp-http-public-key-pinning/
            // app.UseHpkp(options => options
            //     .Sha256Pins(
            //         "Base64 encoded SHA-256 hash of your first certificate e.g. cUPcTAZWKaASuYWhhneDttWpY3oBAkE3h2+soZS7sWs=",
            //         "Base64 encoded SHA-256 hash of your second backup certificate e.g. M8HztCzM3elUxkcjR2S5P4hhyBNf6lHkmjAHKhpGPWE=")
            //     .MaxAge(days: 18 * 7))
            //     .IncludeSubdomains();

            //ConfigureContainer(app);
            ConfigureAuth(app);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            //LoadBalance
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

        }

    }
}
