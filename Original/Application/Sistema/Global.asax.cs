﻿namespace Sistema
{
   using System.Web.Helpers;
   using System.Web.Mvc;
   using System.Web.Optimization;
   using System.Web.Routing;
   using Boilerplate.Web.Mvc;
   using Sistema.Services;
   using NWebsec.Csp;
   using System;
    using System.Web.Http;

    public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         ConfigureViewEngines();
         ConfigureAntiForgeryTokens();

         AreaRegistration.RegisterAllAreas();
         GlobalConfiguration.Configure(WebApiConfig.Register);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         ControllerConfig.RegisterBuilders(ControllerBuilder.Current);
         System.Web.Mvc.ModelBinders.Binders.Add(typeof(Core.Models.Loja.CarrinhoModel), new ModelBinders.CarrinhoBinder());
         Timers.AvisoTimer.Start();
         Application["ContadorAcessos"] = 0;
      }

      protected void Session_Start(object sender, EventArgs e)
      {
         Application["ContadorAcessos"] = (int)(Application["ContadorAcessos"]) + 1;
      }
      protected void Session_End(Object sender, EventArgs e)
      {
         Application["ContadorAcessos"] = (int)(Application["ContadorAcessos"]) - 1;
      }


      /// <summary>
      /// Handles the Content Security Policy (CSP) violation errors. For more information see FilterConfig.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="CspViolationReportEventArgs"/> instance containing the event data.</param>
      protected void NWebsecHttpHeaderSecurityModule_CspViolationReported(object sender, CspViolationReportEventArgs e)
      {
         // Log the Content Security Policy (CSP) violation.
         CspViolationReport violationReport = e.ViolationReport;
         CspReportDetails reportDetails = violationReport.Details;
         string violationReportString = string.Format(
             "UserAgent:<{0}>\r\nBlockedUri:<{1}>\r\nColumnNumber:<{2}>\r\nDocumentUri:<{3}>\r\nEffectiveDirective:<{4}>\r\nLineNumber:<{5}>\r\nOriginalPolicy:<{6}>\r\nReferrer:<{7}>\r\nScriptSample:<{8}>\r\nSourceFile:<{9}>\r\nStatusCode:<{10}>\r\nViolatedDirective:<{11}>",
             violationReport.UserAgent,
             reportDetails.BlockedUri,
             reportDetails.ColumnNumber,
             reportDetails.DocumentUri,
             reportDetails.EffectiveDirective,
             reportDetails.LineNumber,
             reportDetails.OriginalPolicy,
             reportDetails.Referrer,
             reportDetails.ScriptSample,
             reportDetails.SourceFile,
             reportDetails.StatusCode,
             reportDetails.ViolatedDirective);
         CspViolationException exception = new CspViolationException(violationReportString);
         DependencyResolver.Current.GetService<ILoggingService>().Log(exception);
      }

      /// <summary>
      /// Configures the view engines. By default, Asp.Net MVC includes the Web Forms (WebFormsViewEngine) and 
      /// Razor (RazorViewEngine) view engines that supports both C# (.cshtml) and VB (.vbhtml). You can remove view 
      /// engines you are not using here for better performance and include a custom Razor view engine that only 
      /// supports C#.
      /// </summary>
      private static void ConfigureViewEngines()
      {
         ViewEngines.Engines.Clear();
         ViewEngines.Engines.Add(new CSharpRazorViewEngine());
      }

      /// <summary>
      /// Configures the anti-forgery tokens. See 
      /// http://www.asp.net/mvc/overview/security/xsrfcsrf-prevention-in-aspnet-mvc-and-web-pages
      /// </summary>
      private static void ConfigureAntiForgeryTokens()
      {
         // Rename the Anti-Forgery cookie from "__RequestVerificationToken" to "f". This adds a little security 
         // through obscurity and also saves sending a few characters over the wire. Sadly there is no way to change 
         // the form input name which is hard coded in the @Html.AntiForgeryToken helper and the 
         // ValidationAntiforgeryTokenAttribute to  __RequestVerificationToken.
         // <input name="__RequestVerificationToken" type="hidden" value="..." />
         AntiForgeryConfig.CookieName = "f";

         // If you have enabled SSL. Uncomment this line to ensure that the Anti-Forgery 
         // cookie requires SSL to be sent across the wire. 
         // AntiForgeryConfig.RequireSsl = true;
      }
   }
}
