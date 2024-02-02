using Elmah;
using System.Web.Mvc;

namespace Helpers
{
    public class ElmahExceptionLogger : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                ErrorSignal.FromCurrentContext().Raise(context.Exception);
            }
        }
    }
}